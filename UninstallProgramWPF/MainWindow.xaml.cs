using UninstallProgramWPF.WindowsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UninstallProgramWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChildWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Title = "卸载 " + LocalUnInstallTesting.DisplayName;
        }
        public string ProductDisplayName => LocalUnInstallTesting.DisplayName;
        BackgroundWorker worker = new BackgroundWorker();
        private int _installProgress;

        /// <summary>
        /// Get or set InstallProgress value
        /// </summary>
        public int InstallProgress
        {
            get { return _installProgress; }
            set { Set(ref _installProgress, value); }
        }
        private bool _isVisiableProgress;

        /// <summary>
        /// Get or set IsVisiableProgress value
        /// </summary>
        public bool IsVisiableProgress
        {
            get { return _isVisiableProgress; }
            set { Set(ref _isVisiableProgress, value); }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            InstallProgress = e.ProgressPercentage;
        }
        private void btnUnInstall_Click(object sender, RoutedEventArgs e)
        {
            IsVisiableProgress = true;
            worker.RunWorkerAsync();
        }

        private void ChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork ;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += (s, args) =>
            {
                if (args.Error != null)
                {
                    IsVisiableProgress = false;
                    MessageBox.Show(args.Error.Message, "卸载失败");
                }
            };
        }
        private bool _isDelResource;

        /// <summary>
        /// Get or set IsDelResource value
        /// </summary>
        public bool IsDelResource
        {
            get { return _isDelResource; }
            set { Set(ref _isDelResource, value); }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(15);
            LocalUnInstallTesting.DelRegedit();
            LocalUnInstallTesting.DeleteFolderFile(LocalUnInstallTesting.SetupPath, IsDelResource);
            LocalUnInstallTesting.DelDesktopLnk();
            LocalUnInstallTesting.DelMenuLnk();
            // .NET 10 的宿主 EXE、托管 DLL 和已加载依赖需等进程退出后再清理。
            string cleanupData = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    ProcessId = Environment.ProcessId,
                    Root = LocalUnInstallTesting.SetupPath,
                    Files = LocalUnInstallTesting.GetUninstallerFiles(),
                    RemoveDirectory = IsDelResource
                })));
            string script = "$cleanup = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('" + cleanupData + "')) | ConvertFrom-Json\n" + """
                Wait-Process -Id $cleanup.ProcessId -ErrorAction SilentlyContinue
                $root = [IO.Path]::GetFullPath($cleanup.Root).TrimEnd('\')
                foreach ($file in $cleanup.Files) {
                    $full = [IO.Path]::GetFullPath($file)
                    if ($full.StartsWith($root + '\', [StringComparison]::OrdinalIgnoreCase)) {
                        Remove-Item -LiteralPath $full -Force -ErrorAction SilentlyContinue
                    }
                }
                if ($cleanup.RemoveDirectory -and (Test-Path -LiteralPath $root) -and
                    @(Get-ChildItem -LiteralPath $root -Force).Count -eq 0) {
                    [IO.Directory]::Delete($root)
                }
                """;
            var info = new ProcessStartInfo
            {
                FileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                    @"WindowsPowerShell\v1.0\powershell.exe"),
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = System.IO.Path.GetTempPath()
            };
            info.ArgumentList.Add("-NoProfile");
            info.ArgumentList.Add("-NonInteractive");
            info.ArgumentList.Add("-EncodedCommand");
            info.ArgumentList.Add(Convert.ToBase64String(Encoding.Unicode.GetBytes(script)));
            worker.ReportProgress(100);
            Process.Start(info);
            Environment.Exit(0);
        }
    }
}
