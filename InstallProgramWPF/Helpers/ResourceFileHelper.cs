using InstallProgramWPF.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InstallProgramWPF.Helpers
{
    public class ResourceFileHelper
    {
        public static long GetRequiredDiskBytes()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var manifest = assembly.GetManifestResourceStream("InstallProgramWPF.ResourceFileList.json")
                ?? throw new InvalidDataException("缺少安装文件清单，请重新生成安装包。");
            using var reader = new StreamReader(manifest);
            var files = JsonHelper.Deserialize<List<FileData>>(reader.ReadToEnd());
            long total = 0;
            foreach (var file in files)
            {
                using var stream = assembly.GetManifestResourceStream("InstallProgramWPF.Resources." + file.ResourceName)
                    ?? throw new InvalidDataException("缺少安装文件：" + file.FilePath);
                total = checked(total + stream.Length);
            }
            return total;
        }

        public static Stream? OpenPayloadFile(string relativePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var manifest = assembly.GetManifestResourceStream("InstallProgramWPF.ResourceFileList.json")
                ?? throw new InvalidDataException("缺少安装文件清单。");
            using var reader = new StreamReader(manifest);
            var file = JsonHelper.Deserialize<List<FileData>>(reader.ReadToEnd()).FirstOrDefault(item =>
                item.FilePath.Replace('\\', '/').Equals(relativePath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));
            return file is null ? null : assembly.GetManifestResourceStream("InstallProgramWPF.Resources." + file.ResourceName);
        }

        /// <summary>
        /// 将文件属性设置为“嵌入的资源”的文件拷贝到安装目录
        /// 这个方法有限制。你的文件名称和文件夹名称内不能带有符号“.”，因为是通过程序集名称来识别分隔的，程序集的分隔是用“.”，所以加了“.”就无法识别是文件夹名称还是文件名称。
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="sourceFile"></param>
        /// <param name="assemblyName"></param>
        public static void CopyEmbedFile(string targetPath, string sourceFile, string assemblyName)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            Stream stream = assm.GetManifestResourceStream(assemblyName);

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
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
            Stream stream = System.Windows.Application.GetResourceStream(new Uri(@"/InstallProgramWPF;component/Resources/" + sourceFile, UriKind.Relative)).Stream;
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
        /// 将程序集名称转路径和文件名
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="str"></param>
        /// <param name="fileName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static bool AssemblyNameConvetPath(string targetPath, string[] str, ref string fileName, int index)
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

                return AssemblyNameConvetPath(targetPath, str, ref fileName, index - 1);

            }


        }

        public bool CopyAllFile(string targetPath, Action<int> progress)
        {
            string resourcePrefix = "InstallProgramWPF.Resources";
            string[] resourceStr = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Assembly assm = Assembly.GetExecutingAssembly();
            Stream istr = assm.GetManifestResourceStream("InstallProgramWPF.ResourceFileList.json");
            if (istr==null)
            {
                MessageBox.Show("缺少文件信息列表。");
                return false;
            }
            System.IO.StreamReader sr = new System.IO.StreamReader(istr);
            string fileJson = sr.ReadToEnd();
            List<FileData> fileDatas = JsonHelper.Deserialize<List<FileData>>(fileJson);
            int resourceCount = resourceStr.Length - 2;
            for (int i = 0; i < resourceStr.Length; i++)
            {
                string filePath = resourceStr[i].Replace(resourcePrefix + ".", "");
                if (resourceStr[i].Contains(resourcePrefix))
                {
                    foreach (var file in fileDatas)
                    {
                        if (file.ResourceName == filePath)
                        {
                            string pathResource = Path.GetDirectoryName(file.FilePath);
                            if (pathResource.Contains(Path.GetDirectoryName(file.FilePath)))
                            {
                                CopyEmbedFile(targetPath + "/" + pathResource, file.FileName, resourceStr[i]);
                                progress(40 + Convert.ToInt32(Convert.ToDouble(Convert.ToDouble((i + 1)) / Convert.ToDouble(resourceCount)) * 40));
                                break;
                            }
                        }
                    }
                   
                }
            }
            return true;
        }

    }
}
