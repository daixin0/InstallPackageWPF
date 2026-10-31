using System.Net.Http;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using ProgramConfiguration;

namespace UpdateProgramWPF.Helpers;

public sealed class UpdateManifest
{
    public string releaseVersion { get; set; } = "";
    public string installerFileUrl { get; set; } = "";
    public string installerFileSha256 { get; set; } = "";

    public Version Validate()
    {
        if (!Version.TryParse(releaseVersion, out var version))
            throw new InvalidDataException("更新清单缺少有效的 releaseVersion。");
        ConfigurationJson.RequireHttps(installerFileUrl);
        if (string.IsNullOrEmpty(installerFileSha256) || installerFileSha256.Length != 64 || !installerFileSha256.All(Uri.IsHexDigit))
            throw new InvalidDataException("更新清单必须提供 64 位十六进制 installerFileSha256。");
        return version;
    }
}

public sealed class UpdateManifestClient(HttpClient client)
{
    public async Task<UpdateManifest> ReadAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(ConfigurationJson.RequireHttps(url), cancellationToken);
        response.EnsureSuccessStatusCode();
        ConfigurationJson.RequireHttps(response.RequestMessage?.RequestUri?.AbsoluteUri ?? url);
        var manifest = JsonSerializer.Deserialize<UpdateManifest>(await response.Content.ReadAsStringAsync(cancellationToken))
            ?? throw new InvalidDataException("更新清单为空。");
        manifest.Validate();
        return manifest;
    }

    public async Task DownloadAsync(UpdateManifest manifest, string destination, CancellationToken cancellationToken)
    {
        manifest.Validate();
        using var response = await client.GetAsync(ConfigurationJson.RequireHttps(manifest.installerFileUrl),
            HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        ConfigurationJson.RequireHttps(response.RequestMessage?.RequestUri?.AbsoluteUri ?? manifest.installerFileUrl);
        bool created = false;
        try
        {
            await using (var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                created = true;
                await response.Content.CopyToAsync(stream, cancellationToken);
                stream.Position = 0;
                var hash = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken));
                if (!string.Equals(hash, manifest.installerFileSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("更新安装包 SHA256 校验失败，已取消更新。");
            }
        }
        catch
        {
            if (created && File.Exists(destination)) File.Delete(destination);
            throw;
        }
    }
}
