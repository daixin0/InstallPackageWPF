using InstallProgramWPF.Config;
using InstallProgramWPF.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace InstallProgramWPF.Helpers
{
    public class CrateInstallProgramInfo
    {

        public bool StartInstallProgram(string setupPath,Action<int> installProgress)
        {
            try
            {
                string unzipPath = setupPath.Replace('\\', '/');
                ResourceFileHelper resourceFileHelper = new ResourceFileHelper();
                if (resourceFileHelper.CopyAllFile(unzipPath, installProgress))
                {
                    if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.StartMenuGroupName))
                    {
                        CreateShortcut(setupPath);
                        installProgress.Invoke(83);
                        CreateProgramsShortcut(setupPath);
                        installProgress.Invoke(86);
                        if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.UninstallExe))
                        {
                            CreateProgramsUninstallShortcut(setupPath);
                        }
                        installProgress.Invoke(90);
                    }
                    if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.ControlPanelDisplayName))
                    {
                        AddRegedit(setupPath);
                    }
                    if (!string.IsNullOrWhiteSpace(SystemConfig.RegeditInfo.RegistryKey))
                    {
                        AddOldRegedit(setupPath, SystemConfig.RegeditInfo.RegistryKey);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "安装过程异常");
                return false;
            }
            
        }
        /// <summary>
        /// 保留原安装包注册表
        /// </summary>
        /// <param name="setupPath"></param>
        public void AddOldRegedit(string setupPath,string regostryPath)
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                if (!SoftwareEnvironmentalTesting.Is64Bit())
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                }
                string[] regeditStr = regostryPath.Split('/');
                RegistryKey software = key.OpenSubKey("SOFTWARE", true).CreateSubKey(SystemConfig.RegeditInfo.Publisher).CreateSubKey(regeditStr[0]).CreateSubKey(regeditStr[1]);
                software.SetValue("Version", SystemConfig.RegeditInfo.Version);
                software.SetValue("Location", setupPath);
                software.Flush();
                software.Close();
                key.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("安装位置注册信息：" + ex.Message);
            }
        }

        public static string Serializer<T>(T serialObject) where T : class
        {
            try
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                var xmlSerializer = new XmlSerializer(typeof(T));
                var stringBuilder = new StringBuilder();
                using (var xmlWriter = XmlWriter.Create(new StringWriter(stringBuilder), new XmlWriterSettings
                {
                    Indent = true
                }))
                {
                    xmlSerializer.Serialize(xmlWriter, serialObject, ns);
                }
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "生成AddIn文件失败");
                return null;
            }
        }

        /// <summary>
        /// 注册应用信息
        /// </summary>
        /// <param name="setupPath">安装路径</param>
        public void AddRegedit(string setupPath)
        {
            try
            {
                RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (!SoftwareEnvironmentalTesting.Is64Bit())
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                }
                RegistryKey software = key.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true).CreateSubKey(SystemConfig.UniqueIdentification);
                software.SetValue("DisplayIcon", setupPath + "\\" + SystemConfig.RegeditInfo.AppIco);
                software.SetValue("DisplayName", SystemConfig.RegeditInfo.ControlPanelDisplayName);
                software.SetValue("DisplayVersion", SystemConfig.RegeditInfo.Version);
                software.SetValue("Publisher", SystemConfig.RegeditInfo.Publisher);
                software.SetValue("InstallLocation", setupPath);
                software.SetValue("InstallSource", setupPath);
                software.SetValue("UninstallString", setupPath + "\\" + SystemConfig.RegeditInfo.UninstallExe);
                software.SetValue("InstallGuid", SystemConfig.RegeditInfo.InstallGuid);
                software.SetValue("InstallDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture));
                software.Flush();
                software.Close();
                key.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("写入控制面板注册信息：" + ex.Message);
            }
        }
        
        public void CreateShortcut(string targetFile)
        {
            try
            {
                string desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                var shortcut = shell.CreateShortcut(desktop + "\\" + SystemConfig.RegeditInfo.DisplayName + ".lnk");
                shortcut.TargetPath = targetFile + "\\" + SystemConfig.RegeditInfo.StartExe;
                shortcut.WorkingDirectory = targetFile;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "桌面快捷方式创建异常");
                return;
            }

        }

        /// <summary>
        /// 创建程序菜单快捷方式
        /// </summary>
        /// <param name="targetPath">可执行文件路径</param>
        /// <param name="menuName">程序菜单中子菜单名称，为空则不创建子菜单</param>
        /// <returns></returns>
        public void CreateProgramsShortcut(string targetPath)
        {
            try
            {
                string startMenu = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Programs);
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                if (!string.IsNullOrEmpty(SystemConfig.RegeditInfo.StartMenuGroupName))
                {
                    startMenu += "\\" + SystemConfig.RegeditInfo.StartMenuGroupName;
                    if (!System.IO.Directory.Exists(startMenu))
                    {
                        System.IO.Directory.CreateDirectory(startMenu);

                    }
                }
                var shortcut = shell.CreateShortcut(startMenu + "\\" + SystemConfig.RegeditInfo.DisplayName + ".lnk");
                shortcut.TargetPath = targetPath + "\\" + SystemConfig.RegeditInfo.StartExe;
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "启动菜单快捷方式创建异常");
                return;
            }
        }
        public void CreateProgramsUninstallShortcut(string targetPath)
        {
            try
            {
                string startMenu = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Programs);
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                if (!string.IsNullOrEmpty(SystemConfig.RegeditInfo.StartMenuGroupName))
                {
                    startMenu += "\\" + SystemConfig.RegeditInfo.StartMenuGroupName;
                    if (!System.IO.Directory.Exists(startMenu))
                    {
                        System.IO.Directory.CreateDirectory(startMenu);

                    }
                }
                var shortcut = shell.CreateShortcut(startMenu + "\\" + SystemConfig.RegeditInfo.UninstallShortcutName + ".lnk");
                shortcut.TargetPath = targetPath + "\\" + SystemConfig.RegeditInfo.UninstallExe;
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "启动菜单快捷方式创建异常");
                return;
            }
        }
    }
}
