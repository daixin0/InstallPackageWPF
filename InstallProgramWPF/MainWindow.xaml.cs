using InstallProgramWPF.Config;
using InstallProgramWPF.Helpers;
using InstallProgramWPF.WindowsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace InstallProgramWPF
{
    public enum SetupState
    {
        Default,
        CustomPath,
        Agreement,
        SetupProgress,
        SetupComplete
    }
    public partial class MainWindow : ChildWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private SetupState _currentSetupState;

        /// <summary>
        /// Get or set CurrentSetupState value
        /// </summary>
        public SetupState CurrentSetupState
        {
            get { return _currentSetupState; }
            set
            {
                Set(ref _currentSetupState, value);
            }
        }

        private string _installProgressDesc;

        /// <summary>
        /// Get or set InstallProgressDesc value
        /// </summary>
        public string InstallProgressDesc
        {
            get { return _installProgressDesc; }
            set { Set(ref _installProgressDesc, value); }
        }


        #region 配置初始化

        private string _installPath;

        /// <summary>
        /// Get or set InstallPath value
        /// </summary>
        public string InstallPath
        {
            get { return _installPath; }
            set { Set(ref _installPath, value); }
        }

        private string _installTitle;

        /// <summary>
        /// Get or set InstallTitle value
        /// </summary>
        public string InstallTitle
        {
            get { return _installTitle; }
            set { Set(ref _installTitle, value); }
        }


        #endregion


        private bool _isAgree = true;

        /// <summary>
        /// Get or set IsAgree value
        /// </summary>
        public bool IsAgree
        {
            get { return _isAgree; }
            set { Set(ref _isAgree, value); }
        }
        private string _diskSize;
        private long _requiredDiskBytes;
        private double RequiredDiskGb => _requiredDiskBytes / (1024d * 1024 * 1024);
        public string RequiredDiskSize => _requiredDiskBytes < 1024L * 1024 * 1024
            ? $"{Math.Ceiling(_requiredDiskBytes / (1024d * 1024)):0} MB"
            : $"{Math.Ceiling(RequiredDiskGb * 100) / 100:0.00} GB";

        /// <summary>
        /// Get or set DiskSize value
        /// </summary>
        public string DiskSize
        {
            get { return _diskSize; }
            set { Set(ref _diskSize, value); }
        }
        private SolidColorBrush _colorDisk;

        /// <summary>
        /// Get or set ColorDisk value
        /// </summary>
        public SolidColorBrush ColorDisk
        {
            get { return _colorDisk; }
            set { Set(ref _colorDisk, value); }
        }

        private void btnCustomSetup_Click(object sender, RoutedEventArgs e)
        {
            CurrentSetupState = SetupState.CustomPath;
        }

        private void btnUserAgreement_Click(object sender, RoutedEventArgs e)
        {
            //CurrentSetupState = SetupState.Agreement;

        }

        private void ChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _requiredDiskBytes = ResourceFileHelper.GetRequiredDiskBytes();
                RaisePropertyChanged(nameof(RequiredDiskSize));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "安装包错误");
                Close();
                return;
            }
            InstallTitle = SystemConfig.RegeditInfo.DisplayName;
            SystemConfig.RegeditInfo.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            CurrentSetupState = SetupState.Default;

            string install = "";
            if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.ControlPanelDisplayName))
            {
                install = uninstallOldProgram.GetBeforeRegeditInfo();
            }
            double gbSize = 0;
            if (string.IsNullOrWhiteSpace(install))
            {
                string hard = HardwareEnvironmentalTesting.GetHardDisk(RequiredDiskGb);
                if (string.IsNullOrWhiteSpace(hard))
                {
                    MessageBox.Show("无满足安装条件的盘符。");
                    return;
                }
                install = hard + SystemConfig.RegeditInfo.DefaultInstallPath;
            }
            gbSize = HardwareEnvironmentalTesting.GetHardDiskFreeSpace(install.Substring(0, 1));
            if (gbSize < RequiredDiskGb)
            {
                string hard = HardwareEnvironmentalTesting.GetHardDisk(RequiredDiskGb);
                if (string.IsNullOrWhiteSpace(hard))
                {
                    MessageBox.Show("无满足安装条件的盘符。");
                    return;
                }
                gbSize = HardwareEnvironmentalTesting.GetHardDiskFreeSpace(hard.Substring(0, 1));
                install = hard + SystemConfig.RegeditInfo.DefaultInstallPath;
            }
            ColorDisk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F6F6F"));
            InstallPath = install;
            DiskSize = gbSize.ToString("f2") + "G";
        }
        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            Regex r = new Regex(@"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$");
            return r.IsMatch(path);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsValidPath(InstallPath))
            {
                return;
            }
            double gbSize = HardwareEnvironmentalTesting.GetHardDiskFreeSpace(InstallPath.Substring(0, 1));
            DiskSize = gbSize.ToString("f2") + "G";
            if (gbSize >= RequiredDiskGb)
            {
                ColorDisk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F6F6F"));
            }
            else
            {
                ColorDisk = new SolidColorBrush(Colors.Red);
            }
        }
        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "选择安装文件夹",
                InitialDirectory = InstallPath,
                Multiselect = false
            };
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            string selectedPath = System.IO.Path.TrimEndingDirectorySeparator(dialog.FolderName);
            string folderName = System.IO.Path.GetFileName(
                System.IO.Path.TrimEndingDirectorySeparator(SystemConfig.RegeditInfo.DefaultInstallPath));
            string installPath = string.Equals(System.IO.Path.GetFileName(selectedPath), folderName,
                StringComparison.OrdinalIgnoreCase)
                ? selectedPath
                : System.IO.Path.Combine(selectedPath, folderName);

            double gbSize = HardwareEnvironmentalTesting.GetHardDiskFreeSpace(installPath.Substring(0, 1));
            DiskSize = gbSize.ToString("f2") + "G";
            if (gbSize >= RequiredDiskGb)
            {
                ColorDisk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F6F6F"));
            }
            else
            {
                ColorDisk = new SolidColorBrush(Colors.Red);
            }
            InstallPath = installPath;
        }

        private void btnCancl_Click(object sender, RoutedEventArgs e)
        {
            CurrentSetupState = SetupState.Default;
        }

        private void btnAgree_Click(object sender, RoutedEventArgs e)
        {
            IsAgree = true;
        }
        BackgroundWorker worker = new BackgroundWorker();
        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (IsAgree)
            {
                if (!IsValidPath(InstallPath))
                {
                    MessageBox.Show("安装路径无效。");
                    return;
                }
                double gbSize = HardwareEnvironmentalTesting.GetHardDiskFreeSpace(InstallPath.Substring(0, 1));
                DiskSize = gbSize.ToString("f2") + "G";
                if (gbSize < RequiredDiskGb)
                {
                    MessageBox.Show($"磁盘空间不足，需要 {RequiredDiskSize}。");
                    return;
                }

                try
                {
                    var executable = string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.StartExe)
                        ? SystemConfig.RegeditInfo.MainProgram + ".exe" : SystemConfig.RegeditInfo.StartExe;
                    var warning = PayloadRuntimeCheck.GetWarning(executable);
                    if (warning is not null)
                    {
                        MessageBox.Show(warning, "运行环境提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("无法检测运行环境：" + exception.Message, "运行环境提示");
                    return;
                }

                CurrentSetupState = SetupState.SetupProgress;
                worker.WorkerReportsProgress = true;
                worker.DoWork += Worker_DoWork;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerAsync();
            }
        }
        private int _installProgress;

        /// <summary>
        /// Get or set InstallProgress value
        /// </summary>
        public int InstallProgress
        {
            get { return _installProgress; }
            set { Set(ref _installProgress, value); }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            InstallProgress = e.ProgressPercentage;
        }
        UninstallOldProgram uninstallOldProgram = new UninstallOldProgram();
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(2);

            worker.ReportProgress(10);
            InstallProgressDesc = "正在检测"+ SystemConfig.RegeditInfo.MainProgram + "相关程序是否开启...";
            if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.StartExe))
            {
                Process[] p1 = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(SystemConfig.RegeditInfo.StartExe));
                for (int i = 0; i < p1.Length; i++)
                {
                    p1[i].Kill();
                }
            }
            Process[] p2 = Process.GetProcessesByName(SystemConfig.RegeditInfo.MainProgram);
            for (int i = 0; i < p2.Length; i++)
            {
                p2[i].Kill();
            }
            worker.ReportProgress(15);
            if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.ControlPanelDisplayName))
            {
                InstallProgressDesc = "正在检测和卸载旧版本安装程序...";
                uninstallOldProgram.StartUninstallOldProgram(new Action<int>((progress) =>
                {
                    worker.ReportProgress(progress);
                }));
                InstallProgressDesc = "等待旧版本程序卸载完成...";
                int count = 0;
                while (Directory.Exists(InstallPath) && Directory.GetFileSystemEntries(InstallPath).Length > 0)
                {
                    if (count > 5)
                        break;
                    var task = Task.Delay(1000);
                    task.Wait();
                    count++;
                }
                if (Directory.Exists(InstallPath) && Directory.GetFileSystemEntries(InstallPath).Length > 0)
                {
                    MessageBox.Show("请选择一个空文件夹。");
                    Environment.Exit(0);
                }
            }
            
            worker.ReportProgress(40);
            InstallProgressDesc = "正在安装...";
            CrateInstallProgramInfo crateInstallProgramInfo = new CrateInstallProgramInfo();
            bool result = crateInstallProgramInfo.StartInstallProgram(InstallPath, new Action<int>((progress) =>
              {
                  worker.ReportProgress(progress);
              }));

            if (result)
            {
                worker.ReportProgress(100);
                CurrentSetupState = SetupState.SetupComplete;
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.StartExe))
            {
                Process.Start(InstallPath + "/" + SystemConfig.RegeditInfo.StartExe);
            }
            else
            {
                string sPath = Environment.GetEnvironmentVariable(SystemConfig.RegeditInfo.ProgramEnvironmentVariable);
                Process.Start(sPath + "\\" + SystemConfig.RegeditInfo.MainProgram + ".exe");
            }
            
        }


    }
}
