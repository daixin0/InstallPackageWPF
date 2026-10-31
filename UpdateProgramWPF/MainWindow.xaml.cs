using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using UpdateProgramWPF.Helpers;
using UpdateProgramWPF.WindowsBase;

namespace UpdateProgramWPF;

public partial class MainWindow : ChildWindow
{
    private readonly CancellationTokenSource cancellation = new();
    private bool started;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Title = "更新 " + ProductDisplayName;
        Closed += (_, _) => cancellation.Cancel();
    }

    public string ProductDisplayName => LocalUpdateTesting.Configuration.DisplayName;
    private int installProgress;
    public int InstallProgress { get => installProgress; set => Set(ref installProgress, value); }
    private bool isVisiableProgress;
    public bool IsVisiableProgress { get => isVisiableProgress; set => Set(ref isVisiableProgress, value); }
    private string currentVersion = "";
    public string CurrentVersion { get => currentVersion; set => Set(ref currentVersion, value); }
    private string updateDateTime = "";
    public string UpdateDateTime { get => updateDateTime; set => Set(ref updateDateTime, value); }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (started) return;
        started = true;
        var config = LocalUpdateTesting.Configuration;
        if (string.IsNullOrWhiteSpace(config.VersionManifestUrl))
        {
            MessageBox.Show(this, "尚未配置更新服务，请在 UpdaterConfig.json 中填写 VersionManifestUrl 后重新生成更新程序。", "更新提示");
            return;
        }
        string? downloadedFile = null;
        bool installerStarted = false;
        try
        {
            LocalUpdateTesting.SelectRegedit();
            CurrentVersion = LocalUpdateTesting.Version;
            UpdateDateTime = LocalUpdateTesting.UpdateDate;
            if (!Version.TryParse(CurrentVersion, out var installedVersion))
                throw new InvalidDataException("已安装程序的版本号无效。");
            IsVisiableProgress = true;
            InstallProgress = 10;
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token);
            timeout.CancelAfter(TimeSpan.FromSeconds(config.RequestTimeoutSeconds));
            // 禁止自动重定向，避免 HTTPS 配置被重定向到非 HTTPS 下载。
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            var updates = new UpdateManifestClient(client);
            var manifest = await updates.ReadAsync(config.VersionManifestUrl, timeout.Token);
            timeout.CancelAfter(Timeout.InfiniteTimeSpan);
            if (manifest.Validate() <= installedVersion)
            {
                InstallProgress = 100;
                MessageBox.Show(this, "当前已是最新版本。", "更新提示");
                return;
            }
            if (MessageBox.Show(this, $"发现 {ProductDisplayName} 新版本 {manifest.releaseVersion}，是否下载安装包？",
                    "更新确认", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            var running = Process.GetProcessesByName(config.MainProgram);
            try
            {
                if (running.Length > 0)
                    throw new InvalidOperationException($"请关闭 {ProductDisplayName} 后重新运行更新程序。");
            }
            finally { foreach (var process in running) process.Dispose(); }
            var directory = Path.Combine(Path.GetTempPath(), "ProgramUpdates", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            downloadedFile = Path.Combine(directory, "setup.exe");
            InstallProgress = 30;
            timeout.CancelAfter(TimeSpan.FromSeconds(config.RequestTimeoutSeconds));
            await updates.DownloadAsync(manifest, downloadedFile, timeout.Token);
            InstallProgress = 100;
            cancellation.Token.ThrowIfCancellationRequested();
            // 版本及安装登记交由完整安装包更新，下载成功不等于安装成功。
            _ = Process.Start(new ProcessStartInfo(downloadedFile) { UseShellExecute = true })
                ?? throw new InvalidOperationException("无法启动更新安装包。");
            installerStarted = true;
            Close();
        }
        catch (OperationCanceledException)
        {
            if (!cancellation.IsCancellationRequested) MessageBox.Show(this, "更新请求超时，请稍后重试。", "更新提示");
        }
        catch (Exception exception)
        {
            if (!cancellation.IsCancellationRequested) MessageBox.Show(this, exception.Message, "更新失败");
        }
        finally
        {
            IsVisiableProgress = false;
            if (!installerStarted && downloadedFile is not null && File.Exists(downloadedFile)) File.Delete(downloadedFile);
        }
    }
}