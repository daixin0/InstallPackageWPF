using UninstallPackageWPF.WindowsBase;
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

namespace UninstallPackageWPF
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
        }
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
            //LocalUnInstallTesting.DeleteCurrentExe(LocalUnInstallTesting.UnInstallExe, LocalUnInstallTesting.SetupPath);
            string fileName = System.IO.Path.GetTempPath() + "remove.bat";
            StreamWriter bat = new StreamWriter(fileName, false, Encoding.Default);
            string exePath = LocalUnInstallTesting.UnInstallExe;
            bat.WriteLine("cd..");
            bat.WriteLine("ping -n 1 -w 3000 192.186.221.125");
            bat.WriteLine(string.Format("del \"{0}\" /q", exePath));
            if (IsDelResource)
            {
                bat.WriteLine(string.Format("rd \"{0}\" /q", exePath.Substring(0, exePath.LastIndexOf('\\'))));
                bat.WriteLine(string.Format("del \"{0}\" /q", fileName));
            }
            bat.Close();
            worker.ReportProgress(100);
            ProcessStartInfo info = new ProcessStartInfo(fileName);
            info.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = new Process();
            process.StartInfo = info;
            process.Start();
            Environment.Exit(0);
        }
    }
}
