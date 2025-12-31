using ModernWpf.Controls;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils.UI
{
    public static class DialogManager
    {
        private static readonly SemaphoreSlim DialogSemaphore = new(1, 1);

        public static async Task<ContentDialogResult> ShowDialogAsync(
            ContentDialog dialog,
            Action? primaryCallback = null,
            Action? secondaryCallback = null,
            Action? closeCallback = null)
        {
            logger.Info($"[对话框管理器] 请求显示对话框 → {dialog.Title}");

            await DialogSemaphore.WaitAsync();
            try
            {
                var result = await dialog.ShowAsync();

                logger.Info($"[对话框管理器] 对话框关闭 ← {dialog.Title}，用户选择：{result}");

                // 执行回调
                switch (result)
                {
                    case ContentDialogResult.Primary: primaryCallback?.Invoke(); break;
                    case ContentDialogResult.Secondary: secondaryCallback?.Invoke(); break;
                    default: closeCallback?.Invoke(); break;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.Error($"[对话框管理器] 显示对话框异常: {ex}");
                return ContentDialogResult.None;
            }
            finally
            {
                DialogSemaphore.Release();
            }
        }
    }
}