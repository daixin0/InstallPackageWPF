using UpdateProgramWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UpdateProgramWPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                _ = LocalUpdateTesting.Configuration;
                MainWindow window = new MainWindow();
                window.ShowDialog();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.GetBaseException().Message, "更新程序配置错误");
                Shutdown(1);
            }
        }
    }
}
