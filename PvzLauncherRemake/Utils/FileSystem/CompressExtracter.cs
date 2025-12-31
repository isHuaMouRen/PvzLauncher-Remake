using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO;

namespace PvzLauncherRemake.Utils.FileSystem
{
    public class CompressExtracter
    {
        public static void ExtractWithProgress(
            string archivePath,
            string extractionPath,
            Action<double>? progressCallback = null)
        {
            if (string.IsNullOrEmpty(archivePath)) throw new ArgumentNullException(nameof(archivePath));
            if (string.IsNullOrEmpty(extractionPath)) throw new ArgumentNullException(nameof(extractionPath));

            Directory.CreateDirectory(extractionPath);

            using var archive = ArchiveFactory.Open(archivePath);

            // 第一步：预计算总未压缩大小
            long totalUncompressedSize = 0;
            foreach (var entry in archive.Entries)
                if (!entry.IsDirectory)
                    totalUncompressedSize += entry.Size;

            if (totalUncompressedSize == 0)
            {
                // 极少数损坏或特殊格式档案 Size 为 0，直接用原方法
                ArchiveFactory.WriteToDirectory(archivePath, extractionPath, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                progressCallback?.Invoke(100.0);
                return;
            }

            long alreadyExtracted = 0; // 已解压的字节数（跨文件累计）

            using var reader = archive.ExtractAllEntries();

            while (reader.MoveToNextEntry())
            {
                if (reader.Entry.IsDirectory) continue;

                string entryFullPath = Path.Combine(extractionPath, reader.Entry.Key!.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(entryFullPath)!);

                // 关键：手动读取流并写入文件，这样可以实时监控写入字节
                using var entryStream = reader.OpenEntryStream();
                using var fileStream = File.Create(entryFullPath);

                byte[] buffer = new byte[1024 * 512]; // 512 KB 缓冲区，足够大又不会占用过多内存
                int bytesRead;

                while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);

                    if (progressCallback != null)
                    {
                        Interlocked.Add(ref alreadyExtracted, bytesRead);
                        double progress = alreadyExtracted * 100.0 / totalUncompressedSize;
                        progressCallback(Math.Min(100.0, progress));
                    }
                }
            }

            progressCallback?.Invoke(100.0);
        }
    }
}