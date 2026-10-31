using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using ProgramConfiguration;

namespace ContinuousDeliveryMainProgram;

/// <summary>按产品 JSON 生成独立的更新器和卸载器；配置内嵌，不修改模板源码。</summary>
public static class MaintenanceProgramBuilder
{
    private sealed record ProgramPlan(string Name, string ProjectPath, ProductProgramOptions Configuration);

    public static void Validate(InstallerBuildOptions options) => ReadPlans(options);

    private static List<ProgramPlan> ReadPlans(InstallerBuildOptions options)
    {
        var plans = new List<ProgramPlan>();
        if (options.UninstallerConfigPath is null && options.UpdaterConfigPath is null) return plans;
        using var product = JsonDocument.Parse(File.ReadAllText(options.ConfigPath), new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true
        });
        if (options.UninstallerConfigPath is not null)
        {
            var config = ConfigurationJson.Load<UninstallerOptions>(options.UninstallerConfigPath);
            Match(config, ["DisplayName", "MainProgram", "RegistryTag", "StartMenuGroupName", "UninstallShortcutName"]);
            if (ProductValue("UninstallExe") != "UninstallProgramWPF.exe")
                throw new InvalidDataException("生成卸载器时，InstallerProductInfo.UninstallExe 必须为 UninstallProgramWPF.exe。");
            if (string.IsNullOrWhiteSpace(ProductValue("ControlPanelDisplayName")))
                throw new InvalidDataException("卸载器需要安装注册信息，ControlPanelDisplayName 不能为空。");
            Add("UninstallProgramWPF", config);
        }
        if (options.UpdaterConfigPath is not null)
        {
            var config = ConfigurationJson.Load<UpdaterOptions>(options.UpdaterConfigPath);
            Match(config, ["DisplayName", "MainProgram", "RegistryTag", "StartExe"]);
            if (string.IsNullOrWhiteSpace(ProductValue("ControlPanelDisplayName")))
                throw new InvalidDataException("更新器需要安装注册信息，ControlPanelDisplayName 不能为空。");
            Add("UpdateProgramWPF", config);
        }
        return plans;

        string ProductValue(string name) => product.RootElement.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String ? value.GetString()! : "";

        void Match(ProductProgramOptions configuration, string[] names)
        {
            var json = JsonSerializer.SerializeToElement(configuration, configuration.GetType(), ConfigurationJson.Options);
            foreach (var name in names)
                if (json.GetProperty(name).GetString() != ProductValue(name))
                    throw new InvalidDataException($"{configuration.GetType().Name}.{name} 与 InstallerProductInfo.json 不一致。");
        }

        void Add(string name, ProductProgramOptions configuration)
        {
            var project = Path.GetFullPath($"../{name}/{name}.csproj", options.ConfigurationDirectory);
            if (!File.Exists(project)) throw new FileNotFoundException("找不到维护程序模板项目。", project);
            var projectDirectory = Path.GetDirectoryName(project)!;
            var payload = Path.GetFullPath(options.PayloadDirectory);
            var output = options.GetOutputDirectory();
            if (Contains(projectDirectory, payload) || Contains(payload, projectDirectory) ||
                Contains(payload, output) || Contains(output, payload))
                throw new ArgumentException("维护程序的待安装目录、模板源码目录和发布目录必须彼此独立。");
            plans.Add(new(name, project, configuration));
        }
    }

    public static async Task BuildAsync(InstallerBuildOptions options, CancellationToken cancellationToken = default)
    {
        InstallerBuildConfiguration.Validate(options);
        var plans = ReadPlans(options);
        if (plans.Count == 0) return;
        if (!Directory.Exists(options.PayloadDirectory)) throw new DirectoryNotFoundException(options.PayloadDirectory);
        if (!File.Exists(options.IconPath)) throw new FileNotFoundException("找不到程序图标。", options.IconPath);
        // 两个程序全部成功后才加入待安装目录，避免更新到一半就继续打包。
        var completed = new List<(string Name, string PublishDirectory)>();
        var stage = Path.Combine(options.GetOutputDirectory(), "MaintenancePrograms", Guid.NewGuid().ToString("N"));
        foreach (var plan in plans)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = Path.Combine(stage, plan.Name);
            Directory.CreateDirectory(directory);
            var configPath = Path.Combine(directory, ConfigurationJson.ResourceName);
            File.WriteAllText(configPath, JsonSerializer.Serialize(plan.Configuration,
                plan.Configuration.GetType(), ConfigurationJson.Options));
            var publishDirectory = Path.Combine(directory, "publish");
            var start = new ProcessStartInfo("dotnet")
            {
                UseShellExecute = false, CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(plan.ProjectPath)!
            };
            // 独立自包含 EXE，避免不同产品或原应用与工具的依赖 DLL 相互覆盖。
            string[] arguments = ["publish", plan.ProjectPath, "-c", InstallerProjectRules.Configuration,
                "-r", InstallerProjectRules.RuntimeIdentifier, "--self-contained", "true",
                "-o", publishDirectory, "-p:PublishSingleFile=true", "-p:IncludeNativeLibrariesForSelfExtract=true",
                "-p:DebugType=None", "-p:DebugSymbols=false",
                "-p:ProgramConfigurationPath=" + Escape(configPath),
                "-p:ApplicationIcon=" + Escape(options.IconPath),
                "-p:OutputPath=" + Escape(Path.Combine(directory, "build")) + Path.DirectorySeparatorChar,
                "--verbosity", "normal", "--tl:off"];
            foreach (var argument in arguments) start.ArgumentList.Add(argument);
            Console.WriteLine($"正在生成 {plan.Name}，MSBuild 日志如下：");
            using var process = Process.Start(start) ?? throw new InvalidOperationException("无法启动 dotnet publish。");
            try { await process.WaitForExitAsync(cancellationToken); }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                throw;
            }
            if (process.ExitCode != 0) throw new InvalidOperationException($"{plan.Name} 生成失败：{process.ExitCode}");
            if (!File.Exists(Path.Combine(publishDirectory, plan.Name + ".exe")))
                throw new FileNotFoundException($"未生成 {plan.Name}.exe。");
            completed.Add((plan.Name, publishDirectory));
        }
        foreach (var program in completed)
        {
            var fileName = program.Name + ".exe";
            File.Copy(Path.Combine(program.PublishDirectory, fileName), Path.Combine(options.PayloadDirectory, fileName), true);
            Console.WriteLine($"已生成 {fileName}：{options.PayloadDirectory}");
        }
    }

    private static bool Contains(string parent, string child) =>
        Path.TrimEndingDirectorySeparator(parent).Equals(Path.TrimEndingDirectorySeparator(child), StringComparison.OrdinalIgnoreCase) ||
        child.StartsWith(Path.TrimEndingDirectorySeparator(parent) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Escape(string value) => Regex.Replace(value, @"[%$@();'*?]", match => $"%{(int)match.Value[0]:X2}");
}
