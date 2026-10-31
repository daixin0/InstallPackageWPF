using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace InstallProgramWPF.Helpers
{
    public class HardwareEnvironmentalTesting
    {
        
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
        public static double GetHardDiskFreeSpace(string str_HardDiskName)
        {
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (!drive.IsReady || drive.DriveType != System.IO.DriveType.Fixed)
                    continue;
                if (drive.Name == str_HardDiskName)
                {
                    double gbSize = Convert.ToDouble(Convert.ToDouble(drive.AvailableFreeSpace) / 1024 / 1024 / 1024);
                    return gbSize;
                }
            }
            return 0;
        }
        public static string GetHardDisk(double requiredGb)
        {
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (!drive.IsReady || drive.DriveType != System.IO.DriveType.Fixed)
                    continue;
                double gbSize = Convert.ToDouble(Convert.ToDouble(drive.AvailableFreeSpace) / 1024 / 1024 / 1024);
                if (gbSize >= requiredGb)
                {
                    return drive.Name;
                }
            }
            return "";

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

    }
}
