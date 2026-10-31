using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace UninstallProgramWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try { _ = LocalUnInstallTesting.Configuration; }
            catch (Exception exception)
            {
                MessageBox.Show(exception.GetBaseException().Message, "卸载程序配置错误");
                Shutdown(1);
                return;
            }
            bool createdNew;
            string mutexName = "UninstallProgramWPF-" + LocalUnInstallTesting.RegistryTag;
            Mutex singleInstanceWatcher = new Mutex(false, mutexName, out createdNew);
            if (!createdNew)
            {
                Environment.Exit(-1);
            }

            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(LocalUnInstallTesting.MainProgram);
            if (p != null && p.Count() > 0)
            {
                MessageBox.Show("请退出正在运行的“" + LocalUnInstallTesting.DisplayName + "”程序后再卸载");
                Environment.Exit(0);
            }
        }
    }
}
