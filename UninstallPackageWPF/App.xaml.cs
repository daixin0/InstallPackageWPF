using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace UninstallPackageWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool createdNew;
            string mutexName = "UninstallPackageWPF";
            Mutex singleInstanceWatcher = new Mutex(false, mutexName, out createdNew);
            if (!createdNew)
            {
                Environment.Exit(-1);
            }

            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName("MusicTeachingWindow");
            if (p != null && p.Count() > 0)
            {
                MessageBox.Show("请退出正在运行的“音乐教学客户端”程序后再卸载");
                Environment.Exit(0);
            }
        }
    }
}
