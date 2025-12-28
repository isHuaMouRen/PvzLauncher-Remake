using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace UpdateService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string executePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                string updateFilePath = null!;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-updatefile" && args.Length >= i + 1)
                        updateFilePath = args[i + 1];
                }

                if (string.IsNullOrEmpty(updateFilePath))
                {
                    throw new Exception($"未传入的参数 -updatefile");
                }

                Console.WriteLine($"更新文件路径: {updateFilePath}\n程序执行路径: {executePath}");
                Console.WriteLine("等待主程序完全退出...");

                if (!WaitForProcessExit("PvzLauncherRemake", 30))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("警告: 主程序在 30 秒内仍未退出, 文件替换可能失败！");
                    Console.ResetColor();
                }

                Console.WriteLine("开始解压文件...");
                ZipFile.ExtractToDirectory(updateFilePath, executePath, true);
                Console.WriteLine("解压完毕, 将在3秒后启动启动主程序...");

                for (int i = 0; i < 3; i++)
                {
                    Console.WriteLine($"{3 - i}");
                    Thread.Sleep(1000);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(executePath, "PvzLauncherRemake.exe"),
                    Arguments = $"-shell -update",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });

                return;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"发生错误: {ex.Message}\n  详细信息: {ex}");
                Console.ResetColor();
                Console.Write("程序异常终止，更新失败，按任意键退出...");
                Console.ReadKey();
            }
        }

        static bool WaitForProcessExit(string processName, int timeoutSeconds = 30)
        {
            var processes = Process.GetProcessesByName(processName);
            var stopwatch = Stopwatch.StartNew();

            while (processes.Length > 0 && stopwatch.Elapsed.TotalSeconds < timeoutSeconds)
            {
                Thread.Sleep(500);
                processes = Process.GetProcessesByName(processName);
            }

            return processes.Length == 0;
        }
    }
}
