using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallProgramWPF.Models
{
    public class RegeditInfoModel
    {
        /// <summary>
        /// 程序图标（启动程序图标）
        /// </summary>
        public string AppIco { get; set; }
        /// <summary>
        /// 控制面板中程序显示名称
        /// </summary>
        public string ControlPanelDisplayName { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 发行组织
        /// </summary>
        public string Publisher { get; set; }
        /// <summary>
        /// 默认安装路径
        /// </summary>
        public string DefaultInstallPath { get; set; }
        /// <summary>
        /// 启动菜单文件夹名称
        /// </summary>
        public string StartMenuGroupName { get; set; }
        /// <summary>
        /// 卸载程序EXE名称（不含路径）
        /// </summary>
        public string UninstallExe { get; set; }
        /// <summary>
        /// 卸载快捷方式名称
        /// </summary>
        public string UninstallShortcutName { get; set; }
        /// <summary>
        /// 启动程序EXE名称（不含路径）
        /// </summary>
        public string StartExe { get; set; }
        /// <summary>
        /// 启动程序快捷方式名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 安装唯一编码
        /// </summary>
        public string InstallGuid { get; set; }
        /// <summary>
        /// 写入注册表的子项（此项下存有用户名和Token等信息）
        /// </summary>
        public string RegistryKey { get; set; }

        /// <summary>
        /// 主程序名称
        /// </summary>
        public string MainProgram { get; set; }
        /// <summary>
        /// 主程序目录的环境变量
        /// </summary>
        public string ProgramEnvironmentVariable { get; set; }
        /// <summary>
        /// 注册表标签项
        /// </summary>
        public string RegistryTag { get; set; }

    }
}
