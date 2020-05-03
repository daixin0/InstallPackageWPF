using InstallPackageWPF.WindowsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InstallPackageWPF
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
                if (value == SetupState.Agreement)
                {
                    Agreement = false;
                    SetupProgress = false;
                    SetupComplete = false;
                }
                else
                {
                    Default = false;
                    CustomPath = false;
                    Agreement = false;
                    SetupProgress = false;
                    SetupComplete = false;
                }

                switch (value)
                {
                    case SetupState.Default:
                        Default = true;
                        break;
                    case SetupState.CustomPath:
                        CustomPath = true;
                        break;
                    case SetupState.Agreement:
                        Agreement = true;
                        break;
                    case SetupState.SetupProgress:
                        SetupProgress = true;
                        break;
                    case SetupState.SetupComplete:
                        SetupComplete = true;
                        break;
                }
                Set(ref _currentSetupState, value);
            }
        }
        private bool _default;

        /// <summary>
        /// Get or set Default value
        /// </summary>
        public bool Default
        {
            get { return _default; }
            set { Set(ref _default, value); }
        }
        private bool _customPath;

        /// <summary>
        /// Get or set CustomPath value
        /// </summary>
        public bool CustomPath
        {
            get { return _customPath; }
            set { Set(ref _customPath, value); }
        }
        private bool _agreement;

        /// <summary>
        /// Get or set Agreement value
        /// </summary>
        public bool Agreement
        {
            get { return _agreement; }
            set { Set(ref _agreement, value); }
        }
        private bool _setupProgress;

        /// <summary>
        /// Get or set SetupProgress value
        /// </summary>
        public bool SetupProgress
        {
            get { return _setupProgress; }
            set { Set(ref _setupProgress, value); }
        }
        private bool _setupComplete;

        /// <summary>
        /// Get or set SetupComplete value
        /// </summary>
        public bool SetupComplete
        {
            get { return _setupComplete; }
            set { Set(ref _setupComplete, value); }
        }
        private string _installPath = @"C:\Program Files (x86)\MusicTeachingWindow";

        /// <summary>
        /// Get or set InstallPath value
        /// </summary>
        public string InstallPath
        {
            get { return _installPath; }
            set { Set(ref _installPath, value); }
        }
        private bool _isAgree;

        /// <summary>
        /// Get or set IsAgree value
        /// </summary>
        public bool IsAgree
        {
            get { return _isAgree; }
            set { Set(ref _isAgree, value); }
        }
        private string _diskSize;

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
            CurrentSetupState = SetupState.Agreement;

        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = InstallPath;
            var result = dialog.ShowDialog();
            string selectedPath = "";
            if (dialog.SelectedPath.EndsWith("\\"))
            {
                selectedPath = dialog.SelectedPath.Substring(dialog.SelectedPath.Length - 3, 2);
            }
            else
            {
                selectedPath = dialog.SelectedPath;
            }
            if (result== System.Windows.Forms.DialogResult.OK)
            {
                if (!selectedPath.Contains("MusicTeachingWindow"))
                    InstallPath = selectedPath + @"\MusicTeachingWindow";
            }
            else
            {
                InstallPath = selectedPath;
            }
            
            long size = LocalInstallTesting.GetHardDiskFreeSpace(InstallPath.Substring(0, 1));
            double gbSize = Convert.ToDouble(Convert.ToDouble(size) / 1024 / 1024 / 1024);
            if (gbSize >= 10)
            {
                ColorDisk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF888888"));
                DiskSize = InstallPath.Substring(0, 1).ToUpper() + "盘可用空间：" + gbSize.ToString("f2") + "G";
            }
            else
            {
                ColorDisk = new SolidColorBrush(Colors.Red);
                DiskSize = InstallPath.Substring(0, 1).ToUpper() + "盘可用空间：" + gbSize.ToString("f2") + "G(尽量选择剩余空间10G以上的盘符)";
            }


        }

        private void ChildWindow_Loaded(object sender, RoutedEventArgs e)
        {

            CurrentSetupState = SetupState.Default;
            LocalInstallTesting.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            long size = LocalInstallTesting.GetHardDiskFreeSpace(InstallPath.Substring(0, 1));
            double gbSize = Convert.ToDouble(Convert.ToDouble(size) / 1024 / 1024 / 1024);
            if (gbSize >= 10)
            {
                ColorDisk = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF888888"));
                DiskSize = InstallPath.Substring(0, 1).ToUpper() + "盘可用空间：" + gbSize.ToString("f2") + "G";
            }
            else
            {
                ColorDisk = new SolidColorBrush(Colors.Red);
                DiskSize = InstallPath.Substring(0, 1).ToUpper() + "盘可用空间：" + gbSize.ToString("f2") + "G(尽量选择剩余空间10G以上的盘符)";
            }
        }

        private void btnCancl_Click(object sender, RoutedEventArgs e)
        {
            CurrentSetupState = SetupState.Default;
        }

        private void btnAgree_Click(object sender, RoutedEventArgs e)
        {
            Agreement = false;
            IsAgree = true;
        }
        BackgroundWorker worker = new BackgroundWorker();
        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            if (IsAgree)
            {
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

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(2);
            string unzipPath = InstallPath.Replace('\\', '/');
            try
            {
                if (!LocalInstallTesting.GetDotNetRelease())
                {
                    //LocalInstallTesting.CopyFile(unzipPath, @"Environmental/NDP452-KB2901907-x86-x64-AllOS-ENU.exe");
                    //Process.Start(unzipPath + @"/Environmental/NDP452-KB2901907-x86-x64-AllOS-ENU.exe").WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("检测并安装.Net版本错误：" + ex.Message);
            }

            worker.ReportProgress(10);
            try
            {
                if (!LocalInstallTesting.IsInstallVC1() && !LocalInstallTesting.IsInstallVC2())
                {
                    if (LocalInstallTesting.Is64Bit())
                    {
                        //LocalInstallTesting.CopyFile(unzipPath, @"Environmental/vc_redist.x64.exe");
                        //Process.Start(unzipPath + @"/Environmental/vc_redist.x64.exe").WaitForExit();
                    }
                    else
                    {
                        //LocalInstallTesting.CopyFile(unzipPath, @"Environmental/vc_redist.x86.exe");
                        //Process.Start(unzipPath + @"/Environmental/vc_redist.x86.exe").WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("检测并安装VC运行环境错误：" + ex.Message);
            }

            worker.ReportProgress(15);
            Process[] ExeList = Process.GetProcessesByName("MusicTeachingWindow");
            for (int i = 0; i < ExeList.Length; i++)
            {
                ExeList[i].Kill();
            }
            worker.ReportProgress(20);

            LocalInstallTesting.CopyAllFile(unzipPath, new Action<int>((p) =>
            {
                worker.ReportProgress(p);
            }));
            try
            {
                LocalInstallTesting.CreateShortcut(InstallPath);
                LocalInstallTesting.CreateProgramsShortcut(InstallPath, "音乐教学云平台");
                LocalInstallTesting.CreateProgramsUninstallShortcut(InstallPath, "音乐教学云平台");
            }
            catch (Exception ex)
            {
                MessageBox.Show("生成快捷方式错误：" + ex.Message);
            }
            worker.ReportProgress(95);
            try
            {
                LocalInstallTesting.AddRegedit(InstallPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            worker.ReportProgress(100);
            CurrentSetupState = SetupState.SetupComplete;
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Process.Start(InstallPath + @"/MusicTeachingWindow.exe");
        }
    }
}
