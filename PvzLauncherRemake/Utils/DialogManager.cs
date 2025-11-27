using ModernWpf.Controls;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class DialogManager
    {
        private static bool isDialogShow = false;

        public static async Task ShowDialogAsync(ContentDialog dialog, Action? primaryCallback = null, Action? secondaryCallback = null, Action? closeCallback = null)
        {
            logger.Info($"[对话框管理器] 显示对话框: Title=\"{dialog.Title}\" Content=\"{dialog.Content}\" PrimaryButton=\"{dialog.PrimaryButtonText}\" SecondaryButton=\"{dialog.SecondaryButtonText}\" CloseButton=\"{dialog.CloseButtonText}\"");
            logger.Info($"[对话框管理器] 等待对话框空闲");
            // 等待直到没有对话框显示
            await WaitForDialogToCloseAsync();
            logger.Info($"[对话框管理器] 显示对话框: {dialog.Title}");
            isDialogShow = true;

            var result = await dialog.ShowAsync();
            logger.Info($"[对话框管理器] 对话框关闭, 用户选择: {(
                result == ContentDialogResult.Primary ? "Primary" :
                result == ContentDialogResult.Secondary ? "Secondary" : "Close"
            )}: {(
                result == ContentDialogResult.Primary ? dialog.PrimaryButtonText :
                result == ContentDialogResult.Secondary ? dialog.SecondaryButtonText : dialog.CloseButtonText
            )}");

            isDialogShow = false;
            HandleDialogResult(result, primaryCallback, secondaryCallback, closeCallback);
        }

        private static async Task WaitForDialogToCloseAsync()
        {
            while (isDialogShow)
            {
                await Task.Delay(100);
            }
        }

        private static void HandleDialogResult(ContentDialogResult result, Action? primaryCallback, Action? secondaryCallback = null, Action? closeCallback = null)
        {
            if (result == ContentDialogResult.Primary)
                primaryCallback?.Invoke();
            else if (result == ContentDialogResult.Secondary)
                secondaryCallback?.Invoke();
            else
                closeCallback?.Invoke();
        }
    }
}
