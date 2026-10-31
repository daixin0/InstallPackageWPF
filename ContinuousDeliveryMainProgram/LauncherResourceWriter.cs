using ContinuousDeliveryMainProgram.MSBuild;

namespace ContinuousDeliveryMainProgram;

/// <summary>通过项目实体序列化写入启动程序资源。</summary>
public static class LauncherResourceWriter
{
    public static void StartProgramResourceWrite(string csprjPath, string addinFile,
        string configFile, string startProgramIco, string loginLogo,
        InstallerUIFramework uiFramework = InstallerUIFramework.Wpf)
    {
        var projectPath = Path.GetFullPath(csprjPath);
        var directory = Path.GetDirectoryName(projectPath)!;
        var projectFile = MsBuildProjectFile.Load(projectPath);
        var resources = new MsBuildItemGroup { Label = "LauncherResources" };

        AddContent(configFile, Path.Combine("sys", "ProgramConfig.json"));
        CopyResource(loginLogo, Path.Combine("Images", "loginlogo.png"));
        resources.Items.Add(InstallerProjectRules.CreateImageResource(uiFramework, Path.Combine("Images", "loginlogo.png")));
        if (!string.IsNullOrWhiteSpace(addinFile))
            AddContent(addinFile, "LFBimDesign.addin");
        var iconName = Path.GetFileName(startProgramIco);
        AddContent(startProgramIco, iconName);

        projectFile.Project.SetGeneratedGroup(resources);
        projectFile.Project.SetGeneratedGroup(new MsBuildPropertyGroup
        {
            Label = "LauncherProperties",
            Properties = [new MsBuildValue { Value = iconName }]
        });
        projectFile.Save(projectPath);

        void AddContent(string source, string relativePath)
        {
            CopyResource(source, relativePath);
            resources.Items.Add(new MsBuildContent
            {
                Include = relativePath,
                Metadata = [new MsBuildCopyToOutputDirectory { Value = "Always" }]
            });
        }

        void CopyResource(string source, string relativePath)
        {
            var destination = Path.Combine(directory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            if (!Path.GetFullPath(source).Equals(destination, StringComparison.OrdinalIgnoreCase))
                File.Copy(source, destination, overwrite: true);
        }
    }
}

