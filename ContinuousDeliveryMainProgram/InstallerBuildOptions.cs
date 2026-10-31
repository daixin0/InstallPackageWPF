namespace ContinuousDeliveryMainProgram;

/// <summary>用户构建配置；路径相对于配置文件，MSBuild 表达式保持原样。</summary>
public sealed class InstallerBuildOptions
{
    public InstallerUIFramework UIFramework { get; set; } = InstallerUIFramework.Wpf;
    internal string ConfigurationDirectory { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>按 UI 框架选择安装项目；直接调用时相对于当前目录。</summary>
    public string GetProjectPath() => Path.GetFullPath(
        InstallerProjectRules.GetProjectRelativePath(UIFramework), ConfigurationDirectory);
    public required string PayloadDirectory { get; set; }
    public required string ConfigPath { get; set; }
    public required string IconPath { get; set; }
    public required string LogoPath { get; set; }
    /// <summary>沿用安装项目的固定输出规则，不接受 JSON 配置。</summary>
    internal string GetOutputDirectory() => Path.GetFullPath(
        InstallerProjectRules.OutputPath.Replace("$(SolutionDir)", GetSolutionDirectory()));

    internal string GetSolutionDirectory() => Path.GetFullPath("..", Path.GetDirectoryName(GetProjectPath())!);
    public string? UninstallerConfigPath { get; set; }
    public string? UpdaterConfigPath { get; set; }
    public InstallerProjectOptions Project { get; set; } = new();
    public InstallerResourceOptions Resources { get; set; } = new();
}

public sealed class InstallerProjectOptions
{
    /// <summary>不带扩展名的输出程序集名称；null 表示沿用原项目。</summary>
    public string? AssemblyName { get; set; }
    public string? RootNamespace { get; set; }
    public string TargetFramework { get; set; } = "net10.0-windows";
}

public sealed class InstallerResourceOptions
{
    public string ConfigFileName { get; set; } = "InstallerProductInfo.json";
    public string IconFileName { get; set; } = "install.ico";
    public string LogoFileName { get; set; } = "installLogo.png";
    public string[] ExcludedExtensions { get; set; } = [".pdb"];
}




