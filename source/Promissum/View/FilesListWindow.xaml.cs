using Lekco.Promissum.Control;
using System.Collections.Generic;
using System.IO;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// FilesListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FilesListWindow : CustomWindow
    {
        public IEnumerable<FileSystemInfo> Files { get; set; }

        public FilesListWindow(IEnumerable<FileSystemInfo> files)
        {
            InitializeComponent();

            Files = files;
            DataContext = this;
        }

        public static void ShowFilesList(IEnumerable<FileSystemInfo> files)
        {
            new FilesListWindow(files).Show();
        }
    }
}
