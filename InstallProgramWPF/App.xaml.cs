using InstallProgramWPF.Config;
using InstallProgramWPF.Helpers;
using InstallProgramWPF.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InstallProgramWPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// 该函数设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWlndow函数的说明部分</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        /// <summary>
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int SW_SHOWNOMAL = 1;
        private static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, SW_SHOWNOMAL);//显示
            SetForegroundWindow(instance.MainWindowHandle);//当到最前端
        }
        private static Process RuningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] Processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in Processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == currentProcess.MainModule.FileName)
                    {
                        return process;
                    }
                }
            }
            return null;
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Process process = RuningInstance();
            if (process != null)
            {
                MessageBox.Show("安装程序已启动", "安装程序");
                Environment.Exit(-1);
            }
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            bool createdNew;
            string mutexName = "InstallProgramWPF";
            Mutex singleInstanceWatcher = new Mutex(false, mutexName, out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("安装程序已启动");
                Environment.Exit(-1);
            }
            Assembly assm = Assembly.GetExecutingAssembly();
            Stream istr = assm.GetManifestResourceStream("InstallProgramWPF.InstallerProductInfo.json");
            System.IO.StreamReader sr = new System.IO.StreamReader(istr);
            string json = sr.ReadToEnd();
            SystemConfig.RegeditInfo = JsonHelper.Deserialize<RegeditInfoModel>(json);
            
            if (string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.RegistryTag))
            {
                SystemConfig.UniqueIdentification = "{" + HardwareEnvironmentalTesting.UUID() + "+" + HardwareEnvironmentalTesting.GetBIOSSerialNumber() + "}";
            }
            else
            {
                SystemConfig.UniqueIdentification = "{" + HardwareEnvironmentalTesting.UUID() + "+" + SystemConfig.RegeditInfo.RegistryTag + "-" + HardwareEnvironmentalTesting.GetBIOSSerialNumber() + "}";
            }
            if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.StartExe))
            {
                System.Diagnostics.Process[] p1 = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(SystemConfig.RegeditInfo.StartExe));
                if (p1 != null && p1.Count() > 0)
                {
                    MessageBox.Show("请退出正在运行的“" + SystemConfig.RegeditInfo.DisplayName + "”程序");
                    Environment.Exit(0);
                }
            }
            
            System.Diagnostics.Process[] p2 = System.Diagnostics.Process.GetProcessesByName(SystemConfig.RegeditInfo.MainProgram);
            if (p2 != null && p2.Count() > 0)
            {
                MessageBox.Show("请退出正在运行的"+ SystemConfig.RegeditInfo.MainProgram + "程序");
                Environment.Exit(0);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    System.Windows.MessageBox.Show(exception.Message + ":" + exception.StackTrace, "CurrentDomain_UnhandledException");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.StackTrace, "CurrentDomain_UnhandledException");
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception + ":" + e.Exception.StackTrace, "Application_DispatcherUnhandledException");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                var exception = e.Exception as Exception;
                if (exception != null)
                {
                    System.Windows.MessageBox.Show(exception.StackTrace, exception.Message);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.StackTrace, ex.Message);
            }
            finally
            {
                e.SetObserved();
            }

        }
    }
}
