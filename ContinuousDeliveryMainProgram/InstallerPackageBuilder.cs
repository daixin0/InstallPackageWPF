using System.Diagnostics;
using ContinuousDeliveryMainProgram.MSBuild;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ContinuousDeliveryMainProgram;

/// <summary>准备安装资源，并调用 dotnet publish 发布安装程序。</summary>
public static class InstallerPackageBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task BuildAsync(InstallerBuildOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        InstallerBuildConfiguration.Validate(options);
        // 在生成维护程序之前验证安装输入；任一步发布失败均中止后续打包。
        RequireFile(options.GetProjectPath());
        RequireFile(options.ConfigPath);
        RequireFile(options.IconPath);
        RequireFile(options.LogoPath);
        MaintenanceProgramBuilder.Validate(options);
        await MaintenanceProgramBuilder.BuildAsync(options, cancellationToken);
        PrepareResources(options);
        await PublishProjectAsync(options, cancellationToken);
    }

    private static async Task PublishProjectAsync(InstallerBuildOptions options, CancellationToken cancellationToken)
    {
        var projectPath = options.GetProjectPath();
        var startInfo = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(projectPath))!
        };
        // ArgumentList 负责处理空格等字符，不拼接命令行。
        string[] arguments =
        [
            "publish", Path.GetFullPath(projectPath),
            "--configuration", InstallerProjectRules.Configuration,
            "--runtime", InstallerProjectRules.RuntimeIdentifier,
            "--self-contained", InstallerProjectRules.SelfContained ? "true" : "false",
            "-p:SolutionDir=" + EscapeMsBuild(options.GetSolutionDirectory() + Path.DirectorySeparatorChar),
            "--output", options.GetOutputDirectory(),
            "--verbosity", "normal", "--tl:off"
        ];
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        Console.WriteLine($"正在生成安装器 {Path.GetFileName(projectPath)}，MSBuild 日志如下：");
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("无法启动 dotnet publish。");
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            throw;
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"项目发布失败：{projectPath}，退出码：{process.ExitCode}。");
    }

    /// <summary>写入资源和清单，通过实体序列化更新整个项目；重复运行不会累加资源节点。</summary>
    public static void PrepareResources(InstallerBuildOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        InstallerBuildConfiguration.Validate(options);
        var resourceOptions = options.Resources;
        var projectPath = RequireFile(options.GetProjectPath());
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var payloadDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.PayloadDirectory));
        if (!Directory.Exists(payloadDirectory))
            throw new DirectoryNotFoundException(payloadDirectory);
        RequireFile(options.ConfigPath);
        RequireFile(options.IconPath);
        RequireFile(options.LogoPath);
        // 源产品信息允许注释；嵌入标准 JSON，以兼容安装端的 DataContractJsonSerializer。
        using var productInfo = JsonDocument.Parse(File.ReadAllText(options.ConfigPath), new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });
        var productInfoJson = JsonSerializer.Serialize(productInfo.RootElement, JsonOptions);
        // 防止扫描项目生成目录或上一次发布结果，导致资源递归膨胀。
        if (IsInside(projectDirectory, payloadDirectory) ||
            IsInside(payloadDirectory, projectDirectory) ||
            IsInside(options.GetOutputDirectory(), payloadDirectory))
            throw new ArgumentException("待安装文件目录必须独立于项目目录，且不能包含发布目录。");

        var projectFile = MsBuildProjectFile.Load(projectPath);

        var files = Directory.EnumerateFiles(payloadDirectory, "*", SearchOption.AllDirectories)
            .Where(path => !IsEnvironmentInstaller(Path.GetRelativePath(payloadDirectory, path)))
            .Where(path => !resourceOptions.ExcludedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path =>
            {
                var relativePath = Path.GetRelativePath(payloadDirectory, path);
                return new FileData
                {
                    FileName = Path.GetFileName(path),
                    FilePath = relativePath,
                    ResourceName = GetResourceName(relativePath)
                };
            }).ToList();

        if (files.Select(file => file.ResourceName).Distinct(StringComparer.Ordinal).Count() != files.Count)
            throw new InvalidDataException("待安装文件存在重复的清单资源名称，请调整文件或目录名。");

        var resources = new MsBuildItemGroup { Label = InstallerProjectRules.ResourceGroupLabel };
        foreach (var file in files)
        {
            var include = Path.Combine(InstallerProjectRules.ResourceDirectoryName, file.FilePath);
            CopyFile(Path.Combine(payloadDirectory, file.FilePath), Path.Combine(projectDirectory, include));
            resources.Items.Add(new MsBuildEmbeddedResource
            {
                Include = EscapeMsBuild(include),
                Metadata = [new MsBuildLogicalName { Value = "$(RootNamespace)." + InstallerProjectRules.ResourceDirectoryName + "." + EscapeMsBuild(file.ResourceName) }]
            });
        }

        File.WriteAllText(Path.Combine(projectDirectory, InstallerProjectRules.ManifestFileName),
            JsonSerializer.Serialize(files, JsonOptions));
        File.WriteAllText(Path.Combine(projectDirectory, resourceOptions.ConfigFileName), productInfoJson);
        CopyFile(options.IconPath, Path.Combine(projectDirectory, resourceOptions.IconFileName));
        CopyFile(options.LogoPath, Path.Combine(projectDirectory, resourceOptions.LogoFileName));

        foreach (var name in new[] { InstallerProjectRules.ManifestFileName, resourceOptions.ConfigFileName })
            resources.Items.Add(new MsBuildEmbeddedResource
            {
                Include = EscapeMsBuild(name), Metadata = [new MsBuildLogicalName { Value = "$(RootNamespace)." + EscapeMsBuild(name) }]
            });
        foreach (var name in new[] { resourceOptions.IconFileName, resourceOptions.LogoFileName })
            resources.Items.Add(InstallerProjectRules.CreateImageResource(options.UIFramework, EscapeMsBuild(name)));

        var project = projectFile.Project;
        // 迁移上一版本生成的导入；其他 Import 原样保留。
        project.Elements.RemoveAll(element => element is MsBuildImport import &&
            string.Equals(import.Project, InstallerProjectRules.LegacyResourcesImport, StringComparison.OrdinalIgnoreCase));
        project.SetGeneratedGroup(CreateProjectProperties(options));
        project.SetGeneratedGroup(resources);
        projectFile.Save(projectPath);
    }

    private static MsBuildPropertyGroup CreateProjectProperties(InstallerBuildOptions options)
    {
        var settings = options.Project;
        var group = new MsBuildPropertyGroup
        {
            Label = InstallerProjectRules.PropertyGroupLabel,
            Properties =
            [
                new MsBuildOutputType { Value = InstallerProjectRules.OutputType },
                new MsBuildTargetFramework { Value = settings.TargetFramework },
                new MsBuildNullable { Value = InstallerProjectRules.Nullable },
                new MsBuildImplicitUsings { Value = InstallerProjectRules.ImplicitUsings },
                new MsBuildUseWPF { Value = XmlBoolean(InstallerProjectRules.UsesWpf(options.UIFramework)) },
                new MsBuildOutputPath { Value = InstallerProjectRules.OutputPath },
                new MsBuildBaseOutputPath { Value = InstallerProjectRules.BaseOutputPath },
                new MsBuildAppendTargetFrameworkToOutputPath { Value = XmlBoolean(InstallerProjectRules.AppendTargetFrameworkToOutputPath) },
                new MsBuildAppendRuntimeIdentifierToOutputPath { Value = XmlBoolean(InstallerProjectRules.AppendRuntimeIdentifierToOutputPath) },
                new MsBuildValue { Value = EscapeMsBuild(options.Resources.IconFileName) }
            ]
        };
        if (settings.AssemblyName is not null)
            group.Properties.Add(new MsBuildAssemblyName { Value = EscapeMsBuild(settings.AssemblyName) });
        if (settings.RootNamespace is not null)
            group.Properties.Add(new MsBuildRootNamespace { Value = EscapeMsBuild(settings.RootNamespace) });
        return group;
    }

    private static string XmlBoolean(bool value) => value ? "true" : "false";

    // 排除离线环境安装包；应用自身的 runtimes 目录和依赖 DLL 仍正常打包。
    internal static bool IsEnvironmentInstaller(string relativePath)
    {
        var parts = relativePath.Split(['/', '\\']);
        if (parts.Take(parts.Length - 1).Any(part => part.Equals("Environmental", StringComparison.OrdinalIgnoreCase)))
            return true;
        var name = parts[^1];
        return Regex.IsMatch(name, @"^(?:NDP.*|vc_redist[.].*|vcredist[_].*|dotnet-runtime-.*|windowsdesktop-runtime-.*|aspnetcore-runtime-.*|dotnet-sdk-.*)[.]exe$", RegexOptions.IgnoreCase);
    }

    private static string RequireFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("找不到构建输入文件。", fullPath);
        return fullPath;
    }

    private static bool IsInside(string path, string directory) =>
        path.Equals(directory, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(Path.TrimEndingDirectorySeparator(directory) + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);

    private static void CopyFile(string source, string destination)
    {
        if (Path.GetFullPath(source).Equals(Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            return;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, overwrite: true);
    }

    private static string GetResourceName(string path) =>
        string.Join(".", path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Select(part => Regex.IsMatch(part, @"^\d+$") ? "_" + part : part));

    private static string EscapeMsBuild(string value) =>
        Regex.Replace(value, @"[%$@();'*?]", match => $"%{(int)match.Value[0]:X2}");
}








