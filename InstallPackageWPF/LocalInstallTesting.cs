using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InstallPackageWPF
{
    public class LocalInstallTesting
    {
        #region 本地环境检测

        public static bool IsInstallVC1()
        {
            const string subkey = @"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\";
            try
            {
                if (Is64Bit())
                {
                    RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(subkey + @"X64\");

                    if (ndpKey != null && ndpKey.GetValue("Version") != null)
                    {
                        string version = ndpKey.GetValue("Version").ToString();
                        if (string.IsNullOrWhiteSpace(version))
                            return false;
                        int versionNo = Convert.ToInt32(version.Substring(1, version.IndexOf('.') - 1));
                        if (versionNo >= 14)
                            return true;
                    }
                    ndpKey.Close();
                }
                else
                {
                    RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey + @"X86\");

                    if (ndpKey != null && ndpKey.GetValue("Version") != null)
                    {
                        string version = ndpKey.GetValue("Version").ToString();
                        if (string.IsNullOrWhiteSpace(version))
                            return false;
                        int versionNo = Convert.ToInt32(version.Substring(1, version.IndexOf('.') - 1));
                        if (versionNo >= 14)
                            return true;
                    }
                    ndpKey.Close();
                }
                return false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return false;
            }

        }
        public static bool IsInstallVC2()
        {
            const string subkey = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\";
            try
            {
                if (Is64Bit())
                {
                    RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(subkey + @"X64\");

                    if (ndpKey != null && ndpKey.GetValue("Version") != null)
                    {
                        string version = ndpKey.GetValue("Version").ToString();
                        if (string.IsNullOrWhiteSpace(version))
                            return false;
                        int versionNo = Convert.ToInt32(version.Substring(1, version.IndexOf('.') - 1));
                        if (versionNo >= 14)
                            return true;
                    }
                    ndpKey.Close();
                }
                else
                {
                    RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey + @"X86\");

                    if (ndpKey != null && ndpKey.GetValue("Version") != null)
                    {
                        string version = ndpKey.GetValue("Version").ToString();
                        if (string.IsNullOrWhiteSpace(version))
                            return false;
                        int versionNo = Convert.ToInt32(version.Substring(1, version.IndexOf('.') - 1));
                        if (versionNo >= 14)
                            return true;
                    }
                    ndpKey.Close();
                }
                return false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return false;
            }

        }
        public enum INSTALLSTATE
        {
            INSTALLSTATE_NOTUSED = -7,  // component disabled
            INSTALLSTATE_BADCONFIG = -6,  // configuration data corrupt
            INSTALLSTATE_INCOMPLETE = -5,  // installation suspended or in progress
            INSTALLSTATE_SOURCEABSENT = -4,  // run from source, source is unavailable
            INSTALLSTATE_MOREDATA = -3,  // return buffer overflow
            INSTALLSTATE_INVALIDARG = -2,  // invalid function argument
            INSTALLSTATE_UNKNOWN = -1,  // unrecognized product or feature
            INSTALLSTATE_BROKEN = 0,  // broken
            INSTALLSTATE_ADVERTISED = 1,  // advertised feature
            INSTALLSTATE_REMOVED = 1,  // component being removed (action state, not settable)
            INSTALLSTATE_ABSENT = 2,  // uninstalled (or action state absent but clients remain)
            INSTALLSTATE_LOCAL = 3,  // installed on local drive
            INSTALLSTATE_SOURCE = 4,  // run from source, CD or net
            INSTALLSTATE_DEFAULT = 5,  // use default, local or source
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
        public static bool GetDotNetRelease(int release = 379893)
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return (int)ndpKey.GetValue("Release") >= release ? true : false;
                }
                return false;
            }
        }
        #endregion


        #region 获取本机硬件信息
        public static string RegeditGuid { get; set; }
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
                    InstallGuid = Guid.NewGuid().ToString();
                    InstallDateTime = DateTime.Now;
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
        public static long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace;
                }
            }
            return freeSpace;

        }
        public static DateTime InstallDateTime { get; set; }
        public static string InstallGuid { get; set; }

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

        /// <summary>  
        /// CPU名称信息  
        /// </summary>  
        public static string GetCPUName()
        {
            string st = "";
            ManagementObjectSearcher driveID = new ManagementObjectSearcher("Select * from Win32_Processor");
            foreach (ManagementObject mo in driveID.Get())
            {
                st = mo["Name"].ToString();
            }
            return st;
        }
        /// <summary>  
        /// 物理内存  
        /// </summary>  
        public static string GetPhysicalMemory()
        {
            string st = "";
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                st = mo["TotalPhysicalMemory"].ToString();
            }
            return st;
        }
        /// <summary>  
        /// 获取IP地址  
        /// </summary>  
        public static string GetIPAddress()
        {
            string st = "";
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    //st=mo["IpAddress"].ToString();   
                    System.Array ar;
                    ar = (System.Array)(mo.Properties["IpAddress"].Value);
                    st = ar.GetValue(0).ToString();
                    break;
                }
            }
            return st;
        }
        #endregion

        /// <summary>
        /// 程序图标
        /// </summary>
        public const string AppIco = "logo.ico";
        /// <summary>
        /// 控制面板显示名称
        /// </summary>
        public const string ControlPanelDisplayName = "音乐教学客户端";
        /// <summary>
        /// 程序版本
        /// </summary>
        public static string Version { get; set; }
        /// <summary>
        /// 程序发行公司
        /// </summary>
        public const string Publisher = "某某有限公司";
        /// <summary>
        /// 卸载程序名称（必须包含在压缩包内）
        /// </summary>
        public const string UninstallExe = "UninstallPackageWPF.exe";
        /// <summary>
        /// 程序名称
        /// </summary>
        public const string StartExe = "MusicTeachingWindow.exe";
        /// <summary>
        /// 显示名称
        /// </summary>
        public const string DisplayName = "音乐教学客户端";
        /// <summary>
        /// 注册应用信息
        /// </summary>
        /// <param name="setupPath">安装路径</param>
        public static void AddRegedit(string setupPath)
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (!Is64Bit())
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                RegistryKey software = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true).CreateSubKey(sFingerPrint);
                software.SetValue("DisplayIcon", setupPath + "\\" + AppIco);
                software.SetValue("DisplayName", ControlPanelDisplayName);
                software.SetValue("DisplayVersion", Version);
                software.SetValue("Publisher", Publisher);
                software.SetValue("InstallLocation", setupPath);
                software.SetValue("InstallSource", setupPath);
                software.SetValue("UninstallString", setupPath + "\\" + UninstallExe);
                software.SetValue("InstallGuid", InstallGuid);
                software.SetValue("InstallDateTime", InstallDateTime.ToString());
                software.Flush();
                software.Close();
                key.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 将文件属性设置为“嵌入的资源”的文件拷贝到安装目录
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="sourceFile"></param>
        /// <param name="assemblyName"></param>
        public static void CopyEmbedFile(string targetPath, string sourceFile, string assemblyName)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            Stream stream = assm.GetManifestResourceStream(assemblyName);

            if (!Directory.Exists(targetPath + "/" + Path.GetDirectoryName(sourceFile)))
            {
                Directory.CreateDirectory(targetPath + "/" + Path.GetDirectoryName(sourceFile));
            }

            try
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                FileStream fs = new FileStream(targetPath + "/" + sourceFile, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(bytes);
                bw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        /// <summary>
        /// 将文件属性设置为“Resource”的文件拷贝到安装目录
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="sourceFile"></param>
        /// <param name="assemblyName"></param>
        public static void CopyResourceFile(string targetPath, string sourceFile)
        {
            Stream stream = System.Windows.Application.GetResourceStream(new Uri(@"/InstallPackageWPF;component/Resources/" + sourceFile, UriKind.Relative)).Stream;
            if (!Directory.Exists(targetPath + "/" + Path.GetDirectoryName(sourceFile)))
            {
                Directory.CreateDirectory(targetPath + "/" + Path.GetDirectoryName(sourceFile));
            }
            try
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                FileStream fs = new FileStream(targetPath + "/" + sourceFile, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(bytes);
                bw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private static bool IsFile(string targetPath, string[] str,ref string fileName, int index)
        {
            string path = "";
            if (index >= str.Length)
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                path = targetPath + "/" + fileName;
            }
            try
            {
                Path.GetFullPath(path);
                if (string.IsNullOrWhiteSpace(Path.GetExtension(path)))
                    throw new Exception();
                if (index >= 0)
                    throw new Exception();
                return true;
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = str[index];
                }
                else
                {
                    if (!Path.GetExtension(fileName).Contains("config") && (index <= str.Length - 3) && str.Length >= 3)
                        fileName = str[index] + "/" + fileName;
                    else if (Path.GetExtension(fileName).Contains("config") && (index <= str.Length - 4) && str.Length >= 4)
                        fileName = str[index] + "/" + fileName;
                    else
                        fileName = str[index] + "." + fileName;
                }

                return IsFile(targetPath, str, ref fileName, index - 1);

            }


        }

        public static void CopyAllFile(string targetPath, Action<int> progress)
        {
            string dirs = "InstallPackageWPF.Resources";
            string[] resourceStr = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            int resourceCount = resourceStr.Length - 2;
            for (int i = 0; i < resourceStr.Length; i++)
            {
                string item = resourceStr[i];
                if (item.Contains(dirs))
                {
                    string relativePath = item.Replace(dirs + ".", "");
                    string[] str = relativePath.Split('.');
                    string fileName = "";
                    if (IsFile(targetPath, str, ref fileName, str.Length - 1))
                    {
                        CopyEmbedFile(targetPath, fileName, item);
                    }
                    else 
                    {
                        continue;
                    }

                    progress((i + 1) / resourceCount * 60);
                }
            }
            RegeditGuid = Guid.NewGuid().ToString();
            Configuration config = ConfigurationManager.OpenExeConfiguration(targetPath + "\\" + StartExe);
            config.AppSettings.Settings["StartPath"].Value = targetPath;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            progress(90);
        }

        public static void CreateShortcut(string targetFile)
        {
            string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            var shortcut = shell.CreateShortcut(desktop + "\\" + DisplayName + ".lnk");
            shortcut.TargetPath = targetFile + "\\" + StartExe;
            shortcut.WorkingDirectory = targetFile;
            shortcut.Save();


        }

        /// <summary>
        /// 创建程序菜单快捷方式
        /// </summary>
        /// <param name="targetPath">可执行文件路径</param>
        /// <param name="menuName">程序菜单中子菜单名称，为空则不创建子菜单</param>
        /// <returns></returns>
        public static void CreateProgramsShortcut(string targetPath, string menuName)
        {
            string startMenu = System.Environment.GetFolderPath(System.Environment.SpecialFolder.StartMenu);
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            if (!string.IsNullOrEmpty(menuName))
            {
                startMenu += "\\" + menuName;
                if (!System.IO.Directory.Exists(startMenu))
                {
                    System.IO.Directory.CreateDirectory(startMenu);

                }
            }
            var shortcut = shell.CreateShortcut(startMenu + "\\" + DisplayName + ".lnk");
            shortcut.TargetPath = targetPath + "\\" + StartExe;
            shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            shortcut.Save();
        }
        public static void CreateProgramsUninstallShortcut(string targetPath, string menuName)
        {
            string startMenu = System.Environment.GetFolderPath(System.Environment.SpecialFolder.StartMenu);
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            if (!string.IsNullOrEmpty(menuName))
            {
                startMenu += "\\" + menuName;
                if (!System.IO.Directory.Exists(startMenu))
                {
                    System.IO.Directory.CreateDirectory(startMenu);

                }
            }
            var shortcut = shell.CreateShortcut(startMenu + "\\卸载音乐教学客户端.lnk");
            shortcut.TargetPath = targetPath + "\\" + UninstallExe;
            shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            shortcut.Save();
        }
    }
}
