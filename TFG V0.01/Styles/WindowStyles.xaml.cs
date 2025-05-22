using System.Windows;
using System.Windows.Controls;

namespace TFG_V0._01.Styles
{
    public partial class WindowStyles : ResourceDictionary
    {
        public void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Parent is FrameworkElement parent)
            {
                var window = Window.GetWindow(parent);
                if (window != null)
                {
                    window.WindowState = WindowState.Minimized;
                }
            }
        }

        public void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Parent is FrameworkElement parent)
            {
                var window = Window.GetWindow(parent);
                if (window != null)
                {
                    window.WindowState = window.WindowState == WindowState.Maximized 
                        ? WindowState.Normal 
                        : WindowState.Maximized;
                }
            }
        }

        public void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Parent is FrameworkElement parent)
            {
                var window = Window.GetWindow(parent);
                if (window != null)
                {
                    window.Close();
                }
            }
        }
    }
} 