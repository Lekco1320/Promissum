using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.ViewModel;
using Lekco.Wpf.Control;
using System.Collections.Generic;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// FilesListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FileListWindow : CustomWindow
    {
        protected FileListWindow(FileListWindowVM vm)
        {
            InitializeComponent();

            DataContext = vm;
        }

        public static void ShowFilesList(IEnumerable<FileBase> files)
        {
            var vm = new FileListWindowVM(files);
            new FileListWindow(vm).Show();
        }

        public static void ShowFilesList(IEnumerable<RelativedFile> files)
        {
            var vm = new FileListWindowVM(files);
            new FileListWindow(vm).Show();
        }
    }
}
