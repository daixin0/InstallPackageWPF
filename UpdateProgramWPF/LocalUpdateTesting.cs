using Microsoft.Win32;
using System;
using ProgramConfiguration;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UpdateProgramWPF
{
    public class LocalUpdateTesting
    {
        public static UpdaterOptions Configuration { get; } =
            ConfigurationJson.LoadEmbedded<UpdaterOptions>(typeof(LocalUpdateTesting).Assembly);
        private static string _sFingerPrint;

        /// <summary>
        /// 计算机唯一标识
        /// </summary>
        public static string sFingerPrint
        {
            get
            {
                if (string.IsNullOrEmpty(_sFingerPrint))
                {
                    _sFingerPrint = Configuration.GetRegistryKeyName(UUID(), GetBIOSSerialNumber());
                }
                return _sFingerPrint;
            }
        }
        /// <summary>
        /// 获取UUID
        /// </summary>
        /// <returns></returns>
        public static string UUID()
        {
            string code = null;
            SelectQuery query = new SelectQuery("select * from Win32_ComputerSystemProduct");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (var item in searcher.Get())
                {
                    using (item)
                        code = item["UUID"].ToString();
                }
            }
            return code;
        }
        public static string GetBIOSSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_BIOS");
                string sBIOSSerialNumber = "";
                foreach (ManagementObject mo in searcher.Get())
                {
                    sBIOSSerialNumber = mo["SerialNumber"].ToString().Trim();
                }
                return sBIOSSerialNumber;
            }
            catch
            {
                return "";
            }
        }
        public static bool Is64Bit()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string Version { get; set; }
        public static string UpdateDate { get; set; }
        public static string InstallPath { get; set; }
        public static string InstallGuid { get; set; }
        public static void SelectRegedit()
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (!Is64Bit())
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                RegistryKey registryKey = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                RegistryKey uninstallKey = registryKey.OpenSubKey(sFingerPrint, true);
                if (uninstallKey == null)
                {
                    key.Close();
                    throw new InvalidOperationException("找不到此产品的安装信息，请先安装程序。");
                }
                Version = uninstallKey.GetValue("DisplayVersion").ToString();
                InstallPath = uninstallKey.GetValue("InstallLocation").ToString();
                if (string.IsNullOrWhiteSpace(InstallPath))
                {
                    throw new InvalidOperationException("产品安装目录为空。");
                }
                if (!File.Exists(Path.Combine(InstallPath, Configuration.StartExe)))
                    throw new FileNotFoundException("安装目录中找不到已配置的主程序。", Configuration.StartExe);
                InstallGuid = uninstallKey.GetValue("InstallGuid").ToString();
                UpdateDate = uninstallKey.GetValue("InstallDateTime").ToString();
                uninstallKey.Close();
                registryKey.Close();
                key.Close();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("读取安装信息失败：" + ex.Message, ex);
            }
        }
        public static void UpdateVersion()
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (!Is64Bit())
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                RegistryKey registryKey = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                RegistryKey uninstallKey = registryKey.OpenSubKey(sFingerPrint, true);
                if (uninstallKey == null)
                {
                    key.Close();
                    return;
                }
                uninstallKey.SetValue("DisplayVersion", Version);
                uninstallKey.SetValue("InstallDateTime", UpdateDate);
                uninstallKey.Close();
                registryKey.Close();
                key.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("写入注册表失败：" + ex.Message);
            }
        }
    }
}
