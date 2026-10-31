using InstallProgramWPF.Helpers;
using InstallProgramWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallProgramWPF.Config
{
    public class SystemConfig
    {
        /// <summary>
        /// 写入Windows控制面板信息
        /// </summary>
        public static RegeditInfoModel RegeditInfo { get; set; }

        /// <summary>
        /// 计算机唯一标识(写入系统注册表的文件夹名称)
        /// </summary>
        public static string UniqueIdentification { get; set; }

    }
}
