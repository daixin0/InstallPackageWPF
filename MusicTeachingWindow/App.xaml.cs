using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MusicTeachingWindow
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
                bool createdNew;
                string mutexName = "MusicTeachingWindow";
                Mutex singleInstanceWatcher = new Mutex(false, mutexName, out createdNew);
                if (!createdNew)
                {
                    Environment.Exit(-1);
                }
                string mutexName2 = "UpdateWindowProgram";
                Mutex singleInstanceWatcher2 = new Mutex(false, mutexName2, out createdNew);
                if (!createdNew)
                {
                    Environment.Exit(-1);
                }

            }
            catch (Exception ex)
            {
                
            }
            try
            {
                string basePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["StartPath"]) ? Directory.GetCurrentDirectory() : ConfigurationManager.AppSettings["StartPath"];
                bool isInspectUpdate = Convert.ToBoolean(ConfigurationManager.AppSettings["IsAutomaticInspectUpdate"]);
                string serverUrl = "";
                if (isInspectUpdate)
                {
                    if (e.Args.Length > 0)
                    {
                        string targetPath = e.Args[0];
                        if (!string.IsNullOrWhiteSpace(targetPath))
                        {
                            string arg = "";
                            if (targetPath.Contains(" "))
                            {
                                arg = targetPath.Substring(targetPath.LastIndexOf(' '));
                            }
                            else
                            {
                                arg = targetPath;
                            }
                            string[] args = arg.Split(',');
                            if (args.Length <= 1)
                            {
                                serverUrl = arg;
                                Process.Start(basePath + "\\UpdateWindowProgram.exe", arg);
                                Environment.Exit(0);
                            }
                            else
                            {
                                serverUrl = args[0];
                            }

                        }
                        else
                        {
                            Process.Start(basePath + "\\UpdateWindowProgram.exe", serverUrl);
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        Process.Start(basePath + "\\UpdateWindowProgram.exe", serverUrl);
                        Environment.Exit(0);

                    }
                }

                MainWindow window = new MainWindow();
                window.ShowDialog();
            }
            catch (Exception ex)
            {
               
            }
        }
    }
}
