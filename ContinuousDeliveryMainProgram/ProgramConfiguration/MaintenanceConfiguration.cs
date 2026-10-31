using System.Reflection;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgramConfiguration;

public abstract class ProductProgramOptions
{
    public required string DisplayName { get; set; }
    public required string MainProgram { get; set; }
    public required string RegistryTag { get; set; }

    public virtual void Validate()
    {
        ConfigurationJson.ValidateName(DisplayName);
        ConfigurationJson.ValidateName(MainProgram);
        if (MainProgram.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("MainProgram 填写进程名，不包含 .exe。");
        ArgumentNullException.ThrowIfNull(RegistryTag);
        if (RegistryTag.IndexOfAny(['\\', '/', '{', '}']) >= 0)
            throw new ArgumentException("RegistryTag 不能包含路径分隔符或花括号。");
    }

    public string GetRegistryKeyName(string uuid, string bios) => string.IsNullOrWhiteSpace(RegistryTag)
        ? "{" + uuid + "+" + bios + "}"
        : "{" + uuid + "+" + RegistryTag + "-" + bios + "}";
}

public sealed class UninstallerOptions : ProductProgramOptions
{
    public required string StartMenuGroupName { get; set; }
    public required string UninstallShortcutName { get; set; }
    public string[] PreservedDirectories { get; set; } = ["Resources"];

    public override void Validate()
    {
        base.Validate();
        ConfigurationJson.ValidateName(StartMenuGroupName, allowEmpty: true);
        ConfigurationJson.ValidateName(UninstallShortcutName, allowEmpty: true);
        ArgumentNullException.ThrowIfNull(PreservedDirectories);
        foreach (var directory in PreservedDirectories)
            ConfigurationJson.ValidateName(directory);
    }
}

public sealed class UpdaterOptions : ProductProgramOptions
{
    public required string StartExe { get; set; }
    public string VersionManifestUrl { get; set; } = "";
    public int RequestTimeoutSeconds { get; set; } = 30;

    public override void Validate()
    {
        base.Validate();
        ConfigurationJson.ValidateName(StartExe);
        if (!StartExe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("StartExe 必须是 EXE 文件名。");
        ArgumentNullException.ThrowIfNull(VersionManifestUrl);
        if (VersionManifestUrl.Length > 0)
            ConfigurationJson.RequireHttps(VersionManifestUrl);
        if (RequestTimeoutSeconds is < 1 or > 600)
            throw new ArgumentException("RequestTimeoutSeconds 必须在 1–600 秒之间。");
    }
}

public static class ConfigurationJson
{
    public const string ResourceName = "ProgramConfiguration.json";
    public static JsonSerializerOptions Options { get; } = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true
    };

    public static T Load<T>(string path) where T : ProductProgramOptions => Parse<T>(File.ReadAllText(path));

    public static T LoadEmbedded<T>(Assembly assembly) where T : ProductProgramOptions
    {
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException("程序缺少内嵌产品配置，请重新生成。");
        using var reader = new StreamReader(stream);
        return Parse<T>(reader.ReadToEnd());
    }

    public static T Parse<T>(string json) where T : ProductProgramOptions
    {
        var options = JsonSerializer.Deserialize<T>(json, Options)
            ?? throw new InvalidDataException("程序配置不能为空。");
        options.Validate();
        return options;
    }

    public static void ValidateName(string name, bool allowEmpty = false)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (allowEmpty && name.Length == 0) return;
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." ||
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.EndsWith('.') || name.EndsWith(' '))
            throw new ArgumentException($"必须提供单个有效名称，不能包含路径：{name}");
    }

    public static Uri RequireHttps(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException("更新地址必须是无用户名密码的完整 HTTPS 地址。");
        return uri;
    }
}
