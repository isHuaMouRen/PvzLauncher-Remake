using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PvzLauncherRemake.Controls
{
    public class UserCard:ContentControl
    {
        static UserCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserCard), new FrameworkPropertyMetadata(typeof(UserCard)));
        }
    }
}
