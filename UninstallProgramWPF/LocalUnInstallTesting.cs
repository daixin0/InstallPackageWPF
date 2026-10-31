using Microsoft.Win32;
using System;
using ProgramConfiguration;
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

namespace UninstallProgramWPF
{
    public class LocalUnInstallTesting
    {
        public static UninstallerOptions Configuration { get; } =
            ConfigurationJson.LoadEmbedded<UninstallerOptions>(typeof(LocalUnInstallTesting).Assembly);

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
                    _sFingerPrint = Configuration.GetRegistryKeyName(UUID(), GetBIOSSerialNumber());
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
        public static string DisplayName => Configuration.DisplayName;
        public static string MainProgram => Configuration.MainProgram;
        public static string RegistryTag => Configuration.RegistryTag;
        public static string StartMenuGroupName => Configuration.StartMenuGroupName;
        public static string UninstallShortcutName => Configuration.UninstallShortcutName;
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
                    throw new InvalidOperationException("未找到此产品的安装信息，请从已安装的目录运行卸载程序。");
                }
                if (unistallKey.ValueCount <= 0)
                {
                    key.Close();
                    throw new InvalidOperationException("产品安装信息为空，无法卸载。");
                }
                SetupPath = unistallKey.GetValue("InstallLocation").ToString();
                UnInstallExe = unistallKey.GetValue("UninstallString").ToString();
                string currentExe = Environment.ProcessPath
                    ?? throw new InvalidOperationException("无法确定当前卸载程序路径。");
                if (!string.Equals(Path.GetFullPath(SetupPath).TrimEnd('\\'),
                        Path.GetDirectoryName(currentExe).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(Path.GetFullPath(UnInstallExe.Trim('"')), currentExe, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("注册的安装目录与当前卸载程序不匹配，已停止卸载。");
                UnInstallExe = currentExe;
                registryKey.DeleteSubKey(sFingerPrint);
                unistallKey.Close();
                registryKey.Close();
                key.Close();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("读取或删除卸载注册信息失败：" + ex.Message, ex);
            }
        }
        //public static void DeleteCurrentExe(string currentFile,string currentPath)
        //{
        //    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 1000 > Nul & Del " + currentFile + " & rd "+ currentPath);
        //    psi.WindowStyle = ProcessWindowStyle.Hidden;
        //    psi.CreateNoWindow = true;
        //    Process.Start(psi);
        //}
        public static string[] GetUninstallerFiles()
        {
            string root = Path.GetFullPath(SetupPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string assemblyPath = Path.ChangeExtension(UnInstallExe, ".dll");
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                .Select(assembly => assembly.Location)
                .Concat(new[] { UnInstallExe, assemblyPath,
                    Path.ChangeExtension(UnInstallExe, ".deps.json"),
                    Path.ChangeExtension(UnInstallExe, ".runtimeconfig.json") })
                .Select(Path.GetFullPath)
                .Where(path => path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static bool IsUninstallerFile(string path) =>
            GetUninstallerFiles().Contains(Path.GetFullPath(path), StringComparer.OrdinalIgnoreCase);

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
                            if (IsUninstallerFile(f))
                            {
                                continue;
                            }
                            File.Delete(f);
                        }
                        else
                        {
                            if (!isDelResource)
                            {
                                if (string.Equals(Path.GetFullPath(file).TrimEnd('\\'),
                                        Path.GetFullPath(SetupPath).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) &&
                                    Configuration.PreservedDirectories.Contains(Path.GetFileName(f), StringComparer.OrdinalIgnoreCase))
                                    continue;
                            }
                            
                            DeleteFolderFile(f, isDelResource);
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
            if (string.IsNullOrEmpty(StartMenuGroupName)) return;
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

            if (System.IO.File.Exists(desktop + "\\" + DisplayName + ".lnk"))
            {
                System.IO.File.Delete(desktop + "\\" + DisplayName + ".lnk");
            }
        }
        public static void DelMenuLnk()
        {
            if (string.IsNullOrEmpty(StartMenuGroupName)) return;
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Programs);

            if (System.IO.File.Exists(desktop + "\\" + StartMenuGroupName + "\\" + DisplayName + ".lnk"))
            {
                System.IO.File.Delete(desktop + "\\" + StartMenuGroupName + "\\" + DisplayName + ".lnk");
            }
            if (System.IO.File.Exists(desktop + "\\" + StartMenuGroupName + "\\" + UninstallShortcutName + ".lnk"))
            {
                System.IO.File.Delete(desktop + "\\" + StartMenuGroupName + "\\" + UninstallShortcutName + ".lnk");
            }
            if (Directory.Exists(desktop + "\\" + StartMenuGroupName) &&
                !Directory.EnumerateFileSystemEntries(desktop + "\\" + StartMenuGroupName).Any())
            {
                Directory.Delete(desktop + "\\" + StartMenuGroupName);
            }
        }
    }
}
