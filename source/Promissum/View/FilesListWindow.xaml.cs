using Lekco.Promissum.Control;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// FilesListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FilesListWindow : AnimatedWindow
    {
        public IEnumerable<FileSystemInfo> Files { get; set; }

        public FilesListWindow(IEnumerable<FileSystemInfo> files)
        {
            InitializeComponent();

            Files = files;
            DataContext = this;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void ShowFilesList(IEnumerable<FileSystemInfo> files)
        {
            new FilesListWindow(files).Show();
        }
    }
}
