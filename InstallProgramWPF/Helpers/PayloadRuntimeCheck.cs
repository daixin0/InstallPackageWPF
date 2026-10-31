using Microsoft.Win32;
using System.IO;
using System.Xml.Linq;

namespace InstallProgramWPF.Helpers;

/// <summary>读取待安装主程序声明的 .NET Framework 需求；不下载或安装环境。</summary>
internal static class PayloadRuntimeCheck
{
    internal static int RequiredRelease(Version version) => version.ToString() switch
    {
        "4.5" => 378389, "4.5.1" => 378675, "4.5.2" => 379893,
        "4.6" => 393295, "4.6.1" => 394254, "4.6.2" => 394802,
        "4.7" => 460798, "4.7.1" => 461308, "4.7.2" => 461808,
        "4.8" => 528040, "4.8.1" => 533320,
        _ => -1
    };

    public static string? GetWarning(string executable)
    {
        using var config = ResourceFileHelper.OpenPayloadFile(executable + ".config");
        if (config is null) return null;
        var runtime = XDocument.Load(config).Root?.Element("startup")?.Elements("supportedRuntime")
            .FirstOrDefault(element => ((string?)element.Attribute("sku"))?.StartsWith(".NETFramework,Version=v", StringComparison.OrdinalIgnoreCase) == true);
        var sku = (string?)runtime?.Attribute("sku");
        if (sku is null) return null;
        var versionText = sku.Split("Version=v", StringSplitOptions.None)[1].Split(',')[0];
        if (!Version.TryParse(versionText, out var version))
            return "无法识别主程序的运行环境要求，请检查主程序配置。";
        int required = RequiredRelease(version);
        if (required < 0)
            return $"请确认本机已安装主程序要求的 .NET Framework {version}，当前安装器无法自动检测该版本。";
        // Microsoft 的 Release 最小值规则允许兼容的较新版本。
        using var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        using var key = root.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full");
        return key?.GetValue("Release") is int installed && installed >= required ? null
            : $"本机缺少 .NET Framework {version} 或兼容的更高版本。请先安装该运行环境，再运行本安装程序。本安装包不包含环境安装包。";
    }
}
