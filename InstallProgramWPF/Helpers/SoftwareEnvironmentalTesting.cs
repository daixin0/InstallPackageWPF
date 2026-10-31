using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InstallProgramWPF.Helpers
{
    public class SoftwareEnvironmentalTesting
    {
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
        public static string GetNetDateTime()
        {
            WebRequest request = null;
            WebResponse response = null;
            WebHeaderCollection headerCollection = null;
            string datetime = string.Empty;
            try
            {
                request = WebRequest.Create("https://www.baidu.com");
                request.Timeout = 3000;
                request.Credentials = CredentialCache.DefaultCredentials;
                response = (WebResponse)request.GetResponse();
                headerCollection = response.Headers;
                foreach (var h in headerCollection.AllKeys)
                { if (h == "Date") { datetime = headerCollection[h]; } }
                return datetime;
            }
            catch (Exception) { return datetime; }
            finally
            {
                if (request != null)
                { request.Abort(); }
                if (response != null)
                { response.Close(); }
                if (headerCollection != null)
                { headerCollection.Clear(); }
            }
        }
    }
}
