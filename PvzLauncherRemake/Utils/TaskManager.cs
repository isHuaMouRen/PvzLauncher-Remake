using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public class TaskManager
    {
        public static event Action<DownloadTaskInfo>? TaskAdded;
        public static event Action<DownloadTaskInfo>? TaskRemoved;


        //主任务列表
        public static List<DownloadTaskInfo> DownloadTaskList = new List<DownloadTaskInfo>();

        /// <summary>
        /// 向任务列表添加任务
        /// </summary>
        /// <param name="taskInfo"></param>
        /// <returns></returns>
        public static void AddTask(DownloadTaskInfo taskInfo)
        {
            foreach (var task in DownloadTaskList)
            {
                if(task.TaskName == taskInfo.TaskName)
                {
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "无法创建任务",
                        Message = $"任务列表内已有与 \"{taskInfo.TaskName}\" 同名任务，请等待已有任务完成",
                        Type = NotificationType.Error
                    });
                    return;
                }

            }

            var originDownloader = taskInfo.Downloader;
            taskInfo.Downloader = new Downloader
            {
                Completed = (async (s, e) =>
                {
                    if (!s)
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "下载失败",
                            Message = $"无法下载 {taskInfo.TaskName}\n\n错误信息: {e}",
                            Type = NotificationType.Error
                        });
                        DownloadTaskList.Remove(taskInfo);
                        TaskRemoved?.Invoke(taskInfo);
                        return;
                    }

                    logger.Info($"[任务中心] 任务 {taskInfo.TaskName} 完成下载");

                    await Task.Delay(1000);

                    if (!Directory.Exists(taskInfo.SavePath))
                        Directory.CreateDirectory(taskInfo.SavePath);

                    //解压
                    await Task.Run(() =>
                    {
                        CompressExtracter.ExtractWithProgress(originDownloader!.SavePath, taskInfo.SavePath, ((p) =>
                        {
                            taskInfo.ExtractProgress = p;
                            logger.Info($"{p}");
                        }));
                    });


                    string configName = Path.GetFileName(taskInfo.SavePath);
                    if (taskInfo.GameInfo != null) 
                    {
                        var cfg = new JsonGameInfo.Index
                        {
                            GameInfo = new JsonGameInfo.GameInfo
                            {
                                ExecuteName = taskInfo.GameInfo.ExecuteName,
                                Version = taskInfo.GameInfo.Version,
                                VersionType = taskInfo.GameInfo.VersionType,
                                Name = configName,
                                Icon = taskInfo.GameInfo.Icon
                            },
                            Record = new JsonGameInfo.Record
                            {
                                FirstPlay = DateTimeOffset.Now.ToUnixTimeSeconds(),
                                PlayCount = 0,
                                PlayTime = 0
                            }
                        };
                        Json.WriteJson(Path.Combine(taskInfo.SavePath, ".pvzl.json"), cfg);
                        AppInfo.Config.CurrentGame = configName;
                    }
                    else
                    {
                        var cfg = new JsonTrainerInfo.Index
                        {
                            ExecuteName = taskInfo.TrainerInfo!.ExecuteName,
                            Version = taskInfo.TrainerInfo.Version,
                            Name = configName,
                            Icon = taskInfo.TrainerInfo.Icon
                        };
                        Json.WriteJson(Path.Combine(taskInfo.SavePath, ".pvzl.json"), cfg);
                        AppInfo.Config.CurrentTrainer = configName;
                    }

                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "下载完成",
                        Message = $"成功完成任务 \"{taskInfo.TaskName}\"",
                        Type = NotificationType.Success
                    });
                    DownloadTaskList.Remove(taskInfo);
                    TaskRemoved?.Invoke(taskInfo);
                }),
                Progress = ((p, s) =>
                {
                    taskInfo.Progress = p;
                    taskInfo.Speed = s / 1024;
                }),
                Url = originDownloader!.Url,
                SavePath = originDownloader.SavePath
            };

            DownloadTaskList.Add(taskInfo);
            TaskAdded?.Invoke(taskInfo);
            StartTask(taskInfo);


            new NotificationManager().Show(new NotificationContent
            {
                Title = "下载已开始",
                Message = "您的下载任务已被添加进任务列表",
                Type = NotificationType.Information
            });
        }

        /// <summary>
        /// 开始所有任务
        /// </summary>
        public static void StartAllTask()
        {
            foreach (var task in DownloadTaskList)
            {
                task.Downloader?.StartDownload();
            }
        }

        /// <summary>
        /// 根据信息开始任务
        /// </summary>
        /// <param name="taskInfo"></param>
        public static void StartTask(DownloadTaskInfo taskInfo)
        {
            var task = DownloadTaskList.FirstOrDefault(t => t == taskInfo);
            if (task != null)
            {
                task.Downloader?.StartDownload();
                TaskRemoved?.Invoke(task);
            }
        }

        /// <summary>
        /// 结束所有任务
        /// </summary>
        public static void StopAllTask()
        {
            foreach (var task in DownloadTaskList)
            {
                task.Downloader?.StopDownload();
            }
            DownloadTaskList.Clear();
        }


        /// <summary>
        /// 根据下载信息结束任务
        /// </summary>
        /// <param name="taskInfo"></param>
        public static void StopTask(DownloadTaskInfo taskInfo)
        {
            var task = DownloadTaskList.FirstOrDefault(t => t == taskInfo);
            if (task != null)
            {
                task.Downloader?.StopDownload();
                DownloadTaskList.Remove(task);
            }
        }
    }



    public class DownloadTaskInfo
    {
        public Downloader? Downloader { get; set; } = null;//下载器
        public JsonDownloadIndex.GameInfo? GameInfo { get; set; }//游戏信息
        public JsonDownloadIndex.TrainerInfo? TrainerInfo { get; set; }//修改器信息
        public string? TaskName { get; set; } = "未命名下载任务";//任务名
        public GameIcons TaskIcon { get; set; } = GameIcons.Unknown;//任务图标
        public string SavePath { get; set; }//保存路径
        public TaskType TaskType { get; set; }
        public bool? IsComplete { get; set; } = null;//是否完成  true=下载成功  false=下载失败
        public string? ErrorMessage { get; set; } = null;//下载失败时的错误反馈

        public double Progress { get; set; } = 0.0;//下载进度%
        public double Speed { get; set; } = 0.0;//下载速度Mb/s
        public double ExtractProgress { get; set; } = 0.0;//解压进度%
    }

    public enum TaskType
    {
        Game,
        Trainer
    }
}