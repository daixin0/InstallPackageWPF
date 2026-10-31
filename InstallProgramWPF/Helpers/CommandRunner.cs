using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallProgramWPF.Helpers
{
    public class CommandRunner
    {
        public string Run(string workingDirectory, string executeablePath, string arguments)
        {
            var info = new ProcessStartInfo(executeablePath, arguments)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };
            var process = new Process
            {
                StartInfo = info,
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            return result;
        }
        public void StartUninstallCmd(string cmd)
        {
            string outputResult = Run("", "cmd", "/C "+ cmd);
        }
    }
}
