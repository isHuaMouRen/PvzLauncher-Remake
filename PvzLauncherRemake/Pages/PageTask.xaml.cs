using PvzLauncherRemake.Class;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageTask.xaml 的交互逻辑
    /// </summary>
    public partial class PageTask : ModernWpf.Controls.Page
    {
        private DispatcherTimer _timer;

        private void ShowNoneTip()
        {
            if (AppDownloader.DownloadTaskList.Count > 0)
                textBlock_None.Visibility = Visibility.Hidden;
            else
                textBlock_None.Visibility = Visibility.Visible;
        }

        #region Init
        public void Initialize()
        {
            try
            {
                listBox_Task.Items.Clear();

                foreach (var task in AppDownloader.DownloadTaskList)
                {
                    var card = new UserTaskCard
                    {
                        Title = task.TaskName!,
                        Tag = task
                    };
                    card.button_Cancel.Click += (s, e) =>
                    {
                        if (card.Tag != null)
                        {
                            AppDownloader.StopTask((DownloadTaskInfo)card.Tag);
                        }
                    };

                    card.UpdateControl();
                    listBox_Task.Items.Add(card);
                }

                ShowNoneTip();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "初始化 PageTask 发生错误", ex);
            }
        }
        #endregion

        public PageTask()
        {
            InitializeComponent();
            Loaded += ((s, e) => Initialize());

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            AppDownloader.TaskAdded += OnTaskAdded;
            AppDownloader.TaskRemoved += OnTaskRemoved;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                foreach (UserTaskCard card in listBox_Task.Items)
                {
                    if (card.Tag != null)
                    {
                        card.Progress = ((DownloadTaskInfo)card.Tag).Progress;
                        card.Speed = ((DownloadTaskInfo)card.Tag).Speed;
                        card.UpdateControl();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
            
        }

        private void OnTaskAdded(DownloadTaskInfo task)
        {
            Dispatcher.Invoke(() =>
            {

                ShowNoneTip();
                var card = new UserTaskCard
                {
                    Title = task.TaskName!,
                    Tag = task
                };

                card.button_Cancel.Click += (s, e) => AppDownloader.StopTask(task);

                card.UpdateControl();
                listBox_Task.Items.Add(card);
            });
        }

        private void OnTaskRemoved(DownloadTaskInfo task)
        {
            Dispatcher.Invoke(() =>
            {
                ShowNoneTip();

                for (int i = listBox_Task.Items.Count - 1; i >= 0; i--)
                {
                    if (listBox_Task.Items[i] is UserTaskCard card &&
                        ReferenceEquals(card.Tag, task))  // 使用引用比较
                    {
                        listBox_Task.Items.RemoveAt(i);
                        break;
                    }
                }
            });
        }
    }
}
