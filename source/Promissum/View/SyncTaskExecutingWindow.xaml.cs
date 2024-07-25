using Lekco.Promissum.Apps;
using Lekco.Promissum.Control;
using System.Windows;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncTaskExecutingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncTaskExecutingWindow : CustomWindow
    {
        public SyncController _vm;
        public SyncTaskExecutingWindow(SyncController syncController)
        {
            InitializeComponent();

            DataContext = syncController;
            _vm = syncController;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }
    }
}
