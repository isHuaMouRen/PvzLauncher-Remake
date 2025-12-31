using System.Windows;
using System.Windows.Media;

namespace PvzLauncherRemake.Utils.UI
{
    public static class VisualTreeTools
    {
        /// <summary>
        /// 获取指定 DependencyObject 的所有子视觉元素
        /// </summary>
        /// <param name="parent">起始的 DependencyObject（如 Page）</param>
        /// <returns>所有子视觉元素的枚举</returns>
        public static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent)
        {
            if (parent == null) yield break;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                yield return child;

                // 递归遍历子元素
                foreach (DependencyObject grandChild in GetVisualChildren(child))
                {
                    yield return grandChild;
                }
            }
        }

        /// <summary>
        /// 获取指定类型的所有控件（包括嵌套子控件）
        /// </summary>
        /// <typeparam name="T">目标控件类型</typeparam>
        /// <param name="parent">起始元素</param>
        /// <returns>匹配类型的控件集合</returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            foreach (DependencyObject child in GetVisualChildren(parent))
            {
                if (child is T target)
                {
                    yield return target;
                }
            }
        }
    }
}
