using Notifications.Wpf;
using PvzLauncherRemake.Class;
using System.Reflection;
using System.Windows.Controls;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDeveloper.xaml 的交互逻辑
    /// </summary>
    public partial class PageDeveloper : ModernWpf.Controls.Page
    {
        private bool isInitialize = false;

        public async void MainCycle()
        {
            logger.Info($"[开发者控制面板:周期循环] 开始进入周期循环");
            while (true)
            {
                await Task.Delay(1000);

                // =====

                string text = "";
                Type type = typeof(AppInfo);

                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (FieldInfo field in fields)
                {
                    string fieldName = field.Name;
                    object? value = field.GetValue(null);
                    text = $"{text}{fieldName} = {value}\n";
                }

                textBlock_varInfo.Text = text;
            }
        }


        public PageDeveloper()
        {
            InitializeComponent();
            MainCycle();
            Loaded += (async (s, e) =>
            {
                logger.Info($"[开发者控制面板] 初始化...");
                logger.Info($"[开发者控制面板] 初始化WebView2...");

                await webView2.EnsureCoreWebView2Async(null);
                logger.Info($"[开发者控制面板] 初始化WebView2成功");

                isInitialize = true;
                logger.Info($"[开发者控制面板] 完成初始化!");
                new NotificationManager().Show(new NotificationContent
                {
                    Title = "WebView2",
                    Message = "完成初始化",
                    Type = NotificationType.Success
                });
            });
        }

        private void textBox_markdown_TextChanged(object sender, TextChangedEventArgs e)
        {
            markdownViewer.Markdown = textBox_markdown.Text;
        }

        private void textBox_WebView_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitialize)
            {
                webView2.CoreWebView2.NavigateToString(textBox_WebView.Text);
            }
        }
    }
}
