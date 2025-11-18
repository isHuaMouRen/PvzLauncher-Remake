using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PvzLauncherRemake.Utils
{
    public static class DirectoryManager
    {
        public static async Task CopyDirectoryAsync(string sourceDir, string destDir)
        {
            var sourceDirInfo = new DirectoryInfo(sourceDir);

            // 创建目标文件夹（包括多级）
            Directory.CreateDirectory(destDir);

            // 复制所有文件
            foreach (FileInfo file in sourceDirInfo.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                await Task.Run(() => file.CopyTo(targetFilePath));
            }

            // 递归复制所有子文件夹
            foreach (DirectoryInfo subDir in sourceDirInfo.GetDirectories())
            {
                string targetSubDir = Path.Combine(destDir, subDir.Name);
                await CopyDirectoryAsync(subDir.FullName, targetSubDir);
            }
        }
    }
}