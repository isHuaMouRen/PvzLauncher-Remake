using ModernWpf.Controls;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageAbout.xaml 的交互逻辑
    /// </summary>
    public partial class PageAbout : ModernWpf.Controls.Page
    {
        private long EggCount = 0;

        public PageAbout()
        {
            InitializeComponent();

            textBlock_Version.Text = AppInfo.Version;
        }

        public void GoToUrl(object sender,RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = button.Tag.ToString(),
                    UseShellExecute = true
                });
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EggCount++;

            switch (EggCount)
            {
                case 10:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "你真的很无聊...",
                        Type = NotificationType.Information
                    }); break;
                case 20:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "不是我说，你无聊的话可以去干其他事，能不能不要点我了",
                        Type = NotificationType.Information
                    }); break;
                case 40:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "不 要 再 点 我 了 ! ! !",
                        Type = NotificationType.Warning
                    }); break;
                case 45:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "......",
                        Type = NotificationType.Information
                    }); break;
                case 60:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "我知道你很无聊...",
                        Type = NotificationType.Information
                    }); break;
                case 70:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "你可以去干一些有意义的事情，而不是在这里点一堆矢量路径！！！",
                        Type = NotificationType.Error
                    }); break;
                case 90:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "你还在点...",
                        Type = NotificationType.Information
                    }); break;
                case 100:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "好了，到此为止。作者只做了100次点击的判断，后面没有了",
                        Type = NotificationType.Success
                    }); break;
                case 130:
                    ErrorReportDialog.Show("发生错误", "某处代码访问了非法内存！", new Exception("我是异常，我被抛出了"));
                    break;
                case 135:
                    Thread.Sleep(5000);
                    break;
                case 150:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "看来骗不到你",
                        Type = NotificationType.Information
                    }); break;
                case 160:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "何必呢，我也只是一个被设定好的程序，按照特定的逻辑执行。你到底无聊到什么程度，来这里点我？",
                        Type = NotificationType.Information,
                    }); break;
                case 170:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "...",
                        Type = NotificationType.Information,
                    }); break;
                case 200:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "恭喜！200次点击",
                        Type = NotificationType.Information,
                    }); break;
                case 250:
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "香蒲",
                        Message = "好了，这次是真的没了，最大值就是250了。快走吧。",
                        Type = NotificationType.Information,
                    }); break;
            }
        }
    }
}
