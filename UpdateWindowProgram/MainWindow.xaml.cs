using UpdateWindowProgram.Helpers;
using UpdateWindowProgram.Models;
using UpdateWindowProgram.WindowsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace UpdateWindowProgram
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : ChildWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
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
        private bool _isVisiableProgress;

        /// <summary>
        /// Get or set IsVisiableProgress value
        /// </summary>
        public bool IsVisiableProgress
        {
            get { return _isVisiableProgress; }
            set { Set(ref _isVisiableProgress, value); }
        }

        private string _currentVersion;

        /// <summary>
        /// Get or set CurrentVersion value
        /// </summary>
        public string CurrentVersion
        {
            get { return _currentVersion; }
            set { Set(ref _currentVersion, value); }
        }
        private string _updateDateTime;

        /// <summary>
        /// Get or set UpdateDateTime value
        /// </summary>
        public string UpdateDateTime
        {
            get { return _updateDateTime; }
            set { Set(ref _updateDateTime, value); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process[] ExeList = Process.GetProcessesByName("MusicTeachingWindow");
            for (int i = 0; i < ExeList.Length; i++)
            {
                ExeList[i].Kill();
            }
            try
            {
                InstallProgress = 2;
                LocalUpdateTesting.SelectRegedit();
                CurrentVersion = LocalUpdateTesting.Version;
                UpdateDateTime = LocalUpdateTesting.UpdateDate;
                InstallProgress = 10;

                if (!HttpHelper.Instance.IsInternet)
                {
                    Process.Start(LocalUpdateTesting.InstallPath + "\\MusicTeachingWindow.exe", ApplicationPath.JavaApiHttpAddress + ",0");
                    Environment.Exit(0);
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    InstallProgress = 20;
                }));

                IsVisiableProgress = true;
                string savePath = LocalUpdateTesting.InstallPath + "\\UpdateProgram";
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                bool isDownload = false;

                if (isDownload)
                {
                    InstallProgress = 100;
                    //LocalUpdateTesting.Version = latestEdition.releaseVersion;
                    LocalUpdateTesting.UpdateDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    LocalUpdateTesting.UpdateVersion();
                    Process.Start(LocalUpdateTesting.InstallPath + "\\MusicTeachingWindow.exe", ApplicationPath.JavaApiHttpAddress + ",0");
                    this.Close();
                }
                else
                {
                    Process.Start(LocalUpdateTesting.InstallPath + "\\MusicTeachingWindow.exe", ApplicationPath.JavaApiHttpAddress + ",-1");
                    Environment.Exit(-1);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }

    }
}
