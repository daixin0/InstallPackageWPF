using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContinuousDeliveryMainProgram;

public static class InstallerBuildConfiguration
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter<InstallerUIFramework>(allowIntegerValues: false) }
    };

    /// <summary>支持 // 和 /* */ 注释；输入路径以配置文件为基准。</summary>
    public static InstallerBuildOptions Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var options = JsonSerializer.Deserialize<InstallerBuildOptions>(File.ReadAllText(fullPath), JsonOptions)
            ?? throw new InvalidDataException("构建配置不能为空。");
        var directory = Path.GetDirectoryName(fullPath)!;
        options.ConfigurationDirectory = directory;
        options.PayloadDirectory = Resolve(options.PayloadDirectory);
        options.ConfigPath = Resolve(options.ConfigPath);
        options.IconPath = Resolve(options.IconPath);
        options.LogoPath = Resolve(options.LogoPath);
        if (options.UninstallerConfigPath is not null)
            options.UninstallerConfigPath = Resolve(options.UninstallerConfigPath);
        if (options.UpdaterConfigPath is not null)
            options.UpdaterConfigPath = Resolve(options.UpdaterConfigPath);
        Validate(options);
        return options;

        string Resolve(string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            return Path.GetFullPath(value, directory);
        }
    }

    public static void Validate(InstallerBuildOptions options)
    {
        ArgumentNullException.ThrowIfNull(options.Project);
        ArgumentNullException.ThrowIfNull(options.Resources);
        var project = options.Project;
        var projectPath = options.GetProjectPath();
        ArgumentException.ThrowIfNullOrWhiteSpace(project.TargetFramework);
        if (project.AssemblyName is not null)
            ValidateName(project.AssemblyName);
        if (project.RootNamespace is not null)
            ArgumentException.ThrowIfNullOrWhiteSpace(project.RootNamespace);

        var payloadDirectory = Path.GetFullPath(options.PayloadDirectory);
        var installerDirectory = Path.GetDirectoryName(projectPath)!;
        if (ContainsDirectory(payloadDirectory, installerDirectory) || ContainsDirectory(installerDirectory, payloadDirectory) ||
            ContainsDirectory(payloadDirectory, options.GetOutputDirectory()))
            throw new ArgumentException("PayloadDirectory 必须独立于安装项目，且不能包含安装程序发布目录。");

        var resources = options.Resources;
        string[] names = [InstallerProjectRules.ResourceDirectoryName, InstallerProjectRules.ManifestFileName, resources.ConfigFileName,
            resources.IconFileName, resources.LogoFileName];
        foreach (var name in names)
            ValidateName(name);
        if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Length)
            throw new ArgumentException("资源目录和生成文件的名称不能重复。");
        string[] reserved = ["bin", "obj", Path.GetFileName(projectPath)];
        if (names.Any(name => reserved.Contains(name, StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException("资源名称不能占用项目文件、bin 或 obj。");
        ArgumentNullException.ThrowIfNull(resources.ExcludedExtensions);
        if (resources.ExcludedExtensions.Any(extension => string.IsNullOrWhiteSpace(extension) ||
                !extension.StartsWith('.') || extension.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            throw new ArgumentException("ExcludedExtensions 中每项必须是以点开头的扩展名，例如 .pdb。");
    }

    private static bool ContainsDirectory(string parent, string child) =>
        Path.TrimEndingDirectorySeparator(parent).Equals(Path.TrimEndingDirectorySeparator(child), StringComparison.OrdinalIgnoreCase) ||
        child.StartsWith(Path.TrimEndingDirectorySeparator(parent) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static void ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            name.EndsWith('.') || name.EndsWith(' '))
            throw new ArgumentException($"必须提供有效的单个文件或目录名称，不能包含路径：{name}");
    }
}




