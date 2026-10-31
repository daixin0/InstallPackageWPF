using ContinuousDeliveryMainProgram.MSBuild;

namespace ContinuousDeliveryMainProgram;

/// <summary>用户通过 JSON 选择 UI 框架；资源类型遵循对应框架的构建协议。</summary>
public enum InstallerUIFramework
{
    Wpf,
    Avalonia
}

/// <summary>固定构建协议，不作为用户配置；改动 Label 会影响重复生成时的分组识别。</summary>
public static class InstallerProjectRules
{
    // WPF 与 Avalonia 安装项目共用的固定属性，不通过 JSON 配置。
    public const string OutputType = "WinExe";
    public const string Nullable = "enable";
    public const string ImplicitUsings = "enable";
    public const string OutputPath = @"$(SolutionDir)\Output";
    public const string BaseOutputPath = @"$(SolutionDir)\Output";
    public const bool AppendTargetFrameworkToOutputPath = false;
    public const bool AppendRuntimeIdentifierToOutputPath = false;

    // 发布参数由代码固定，用户仅配置项目、资源和图标。
    public const string Configuration = "Release";
    public const string RuntimeIdentifier = "win-x64";
    public const bool SelfContained = false;

    // 待安装文件的目录与逻辑资源名前缀共用固定名称。
    public const string ResourceDirectoryName = "Resources";
    public const string ManifestFileName = "ResourceFileList.json";
    public const string ResourceGroupLabel = "InstallerPackageResources";
    public const string PropertyGroupLabel = "InstallerPackageProperties";
    public const string LegacyResourcesImport = "InstallerResources.g.targets";

    public static string GetProjectRelativePath(InstallerUIFramework framework) => framework switch
    {
        InstallerUIFramework.Wpf => "../InstallProgramWPF/InstallProgramWPF.csproj",
        InstallerUIFramework.Avalonia => "../InstallProgramAvalonia/InstallProgramAvalonia.csproj",
        _ => throw new ArgumentOutOfRangeException(nameof(framework))
    };

    public static bool UsesWpf(InstallerUIFramework framework) => framework switch
    {
        InstallerUIFramework.Wpf => true,
        InstallerUIFramework.Avalonia => false,
        _ => throw new ArgumentOutOfRangeException(nameof(framework))
    };

    public static MsBuildItem CreateImageResource(InstallerUIFramework framework, string include) => framework switch
    {
        InstallerUIFramework.Wpf => new MsBuildResource { Include = include },
        InstallerUIFramework.Avalonia => new MsBuildAvaloniaResource { Include = include },
        _ => throw new ArgumentOutOfRangeException(nameof(framework))
    };
}



