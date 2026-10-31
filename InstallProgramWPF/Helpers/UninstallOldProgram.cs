using InstallProgramWPF.Config;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InstallProgramWPF.Helpers
{
    public class UninstallOldProgram
    {
        public string GetBeforeRegeditInfo()
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (!SoftwareEnvironmentalTesting.Is64Bit())
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                RegistryKey registryKey = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                RegistryKey uninstallKey = registryKey.OpenSubKey(SystemConfig.UniqueIdentification, true);
                if (uninstallKey == null)
                {
                    key.Close();
                    return "";
                }
                string installPath = uninstallKey.GetValue("InstallLocation")?.ToString();
                uninstallKey.Close();
                registryKey.Close();
                key.Close();
                return installPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取之前的版本注册表失败：" + ex.Message);
                return "";
            }
        }


        public void StartUninstallOldProgram(Action<int> progress)
        {
            CommandRunner runner = new CommandRunner();
            string installPath = "";
            progress(20);
            installPath = GetBeforeRegeditInfo();
            progress(25);
            if (!string.IsNullOrWhiteSpace(installPath))
            {
                MessageBox.Show("检测到存在旧版本的插件，需要先卸载，请在弹出的窗体中点击“卸载程序”。");
                runner.Run(installPath, "cmd", "/C " + SystemConfig.RegeditInfo.UninstallExe);
                progress(30);
                if (!string.IsNullOrWhiteSpace(GetBeforeRegeditInfo()))
                {
                    MessageBox.Show("未卸载旧版本插件，程序退出。");
                    Environment.Exit(0);
                }
                progress(35);
            }

        }
    }
}
