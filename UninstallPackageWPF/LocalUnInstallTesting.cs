using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UninstallPackageWPF
{
    public class LocalUnInstallTesting
    {

        #region 获取本机硬件信息
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
                    _sFingerPrint = "{" + UUID() + "+" + GetBIOSSerialNumber() + "}";
                }
                return _sFingerPrint;
            }
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

        #endregion


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

        public static string UnInstallExe { get; set; }
        public static string SetupPath { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public const string DisplayName = "音乐教学客户端";
        /// <summary>
        /// 删除注册表
        /// </summary>
        public static void DelRegedit()
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (!Is64Bit())
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                RegistryKey registryKey = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                RegistryKey unistallKey = registryKey.OpenSubKey(sFingerPrint, true);
                if (unistallKey == null)
                {
                    key.Close();
                    return;
                }
                if (unistallKey.ValueCount <= 0)
                {
                    key.DeleteSubKey(sFingerPrint);
                    key.Close();
                    return;
                }
                SetupPath = unistallKey.GetValue("InstallLocation").ToString();
                UnInstallExe = unistallKey.GetValue("UninstallString").ToString();
                registryKey.DeleteSubKey(sFingerPrint);
                unistallKey.Close();
                registryKey.Close();
                key.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除注册表失败：" + ex.Message);
            }
        }
        //public static void DeleteCurrentExe(string currentFile,string currentPath)
        //{
        //    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 1000 > Nul & Del " + currentFile + " & rd "+ currentPath);
        //    psi.WindowStyle = ProcessWindowStyle.Hidden;
        //    psi.CreateNoWindow = true;
        //    Process.Start(psi);
        //}
        public static void DeleteFolderFile(string file,bool isDelResource=false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(file))
                    return;
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            if (f.Contains("UninstallPackageWPF"))
                            {
                                continue;
                            }
                            File.Delete(f);
                        }
                        else
                        {
                            if (!isDelResource)
                            {
                                if (f.Substring(f.LastIndexOf("\\") + 1) == "Resources")
                                    continue;
                            }
                            
                            DeleteFolderFile(f);
                        }
                    }
                    if (SetupPath != file)
                        Directory.Delete(file);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("删除文件失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 删除桌面快捷
        /// </summary>
        public static void DelDesktopLnk()
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

            if (System.IO.File.Exists(desktop + "\\" + DisplayName + ".lnk"))
            {
                System.IO.File.Delete(desktop + "\\" + DisplayName + ".lnk");
            }
        }
        public static void DelMenuLnk()
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.StartMenu);

            if (System.IO.File.Exists(desktop + "\\音乐教学云平台\\" + DisplayName + ".lnk"))
            {
                System.IO.File.Delete(desktop + "\\音乐教学云平台\\" + DisplayName + ".lnk");
            }
            if (System.IO.File.Exists(desktop + "\\音乐教学云平台\\卸载音乐教学客户端.lnk"))
            {
                System.IO.File.Delete(desktop + "\\音乐教学云平台\\卸载音乐教学客户端.lnk");
            }
            if (Directory.Exists(desktop + "\\音乐教学云平台"))
            {
                Directory.Delete(desktop + "\\音乐教学云平台");
            }
        }
    }
}
