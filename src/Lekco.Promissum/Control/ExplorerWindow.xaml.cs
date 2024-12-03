using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Promissum.Utility;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// ExplorerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExplorerWindow : CustomWindow, INotifyPropertyChanged
    {
        public DriveBase Drive { get; set; }

        public DirectoryBase Directory { get; set; }

        public string Path { get; set; }

        public int EntityCount { get; set; }

        public ObservableCollection<FileSystemBaseVM> ViewModels { get; }

        public RelayCommand ChangeDriveCommand => new RelayCommand(ChangeDrive);

        public RelayCommand GoBackCommand => new RelayCommand(() => GoTo(Directory.Parent));

        public RelayCommand RefreshCommand => new RelayCommand(Refresh);

        public RelayCommand<FileSystemBaseVM> OpenCommand => new RelayCommand<FileSystemBaseVM>(vm => DoubleClick(vm.FileSystemBase));

        public RelayCommand<FileSystemBaseVM> CopyToCommand => new RelayCommand<FileSystemBaseVM>(vm => CopyTo(vm.FileSystemBase));

        public RelayCommand<FileSystemBaseVM> MoveToCommand => new RelayCommand<FileSystemBaseVM>(vm => MoveTo(vm.FileSystemBase));

        public RelayCommand<FileSystemBaseVM> DeleteCommand => new RelayCommand<FileSystemBaseVM>(vm => Delete(vm.FileSystemBase));

        public RelayCommand CreateDirectoryCommand => new RelayCommand(CreateDirectory);

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExplorerWindow(DriveBase drive, DirectoryBase directory)
        {
            Drive = drive;
            Directory = directory;
            Path = Drive.GetRelativePath(Directory);
            ViewModels = new ObservableCollection<FileSystemBaseVM>();
            GetEntities();

            InitializeComponent();
            Title = Directory.Name.Length > 0 ? $"{Directory.Name} ({Drive.Name})" : Drive.Name;
            DataContext = this;
        }

        private void GetEntities()
        {
            ViewModels.Clear();
            try
            {
                foreach (var dir in Directory.EnumerateDirectories())
                {
                    ViewModels.Add(new FileSystemBaseVM(dir, this));
                }
                foreach (var file in Directory.EnumerateFiles())
                {
                    ViewModels.Add(new FileSystemBaseVM(file, this));
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                DialogHelper.ShowError($"文件夹访问失败：找不到文件夹。{Environment.NewLine}{ex.Message}");
            }
            catch (IOException ex)
            {
                DialogHelper.ShowError($"文件夹访问失败：路径无效。{Environment.NewLine}{ex.Message}");
            }
            catch (SecurityException ex)
            {
                DialogHelper.ShowError($"文件夹访问失败：访问未许可。{Environment.NewLine}{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                DialogHelper.ShowError($"文件夹访问失败：访问未授权。{Environment.NewLine}{ex.Message}");
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"文件夹访问失败：未知错误。{Environment.NewLine}{ex.Message}");
            }
            EntityCount = ViewModels.Count;
        }

        private void ChangeDrive()
        {
            if (DriveSelectorDialog.ShowDialog(DriveCategory.All, out var targetDrive))
            {
                GoTo(targetDrive);
            }
        }

        private void DoubleClick(FileSystemBase selectedEntity)
        {
            if (selectedEntity is DirectoryBase directory)
            {
                GoTo(directory);
            }
            else if (selectedEntity is FileBase file)
            {
                Drive.OpenFile(file);
            }
        }

        private void GoTo(DriveBase drive)
        {
            if (!drive.IsReady)
            {
                DialogHelper.ShowError($"设备\"{drive.Name}\"尚未就绪。");
                return;
            }

            Drive = drive;
            Directory = Drive.RootDirectory;
            Path = Drive.GetRelativePath(Directory);
            Title = Directory.Name.Length > 0 ? $"{Directory.Name} ({Drive.Name})" : Drive.Name;
            GetEntities();
        }

        private void GoTo(DirectoryBase directory)
        {
            if (!directory.Exists)
            {
                DialogHelper.ShowError("设备连接断开或转到目录不存在。");
                return;
            }

            Directory = directory;
            Path = Drive.GetRelativePath(Directory);
            Title = Directory.Name.Length > 0 ? $"{Directory.Name} ({Drive.Name})" : Drive.Name;
            GetEntities();
        }

        private void CopyTo(FileSystemBase entity)
        {
            if (entity is FileBase file)
            {
                FileOperationDialog.OperateFile(Drive, file, OperationType.Copy);
                Refresh();
            }
        }

        private void MoveTo(FileSystemBase entity)
        {
            if (entity is FileBase file)
            {
                FileOperationDialog.OperateFile(Drive, file, OperationType.Move);
                Refresh();
            }
        }

        private void Delete(FileSystemBase entity)
        {
            if (entity is FileBase file && DialogHelper.ShowWarning("确定是否要删除该文件？此操作将无法撤销。"))
            {
                FileOperationDialog.OperateFile(Drive, file, OperationType.Delete);
                Refresh();
            }
        }

        private void CreateDirectory()
        {
            if (InputDialog.ShowDialog(
                description: "文件夹名：",
                result: out string result,
                title: "新建文件夹",
                validator: str => !System.IO.Path.GetInvalidPathChars().Any(c => str.Contains(c)),
                messager: str => $"文件夹名不得含有字符 {string.Join(' ', System.IO.Path.GetInvalidPathChars())}"
            ))
            {
                var directory = Drive.GetDirectory(Path + '\\' + result);
                if (directory.TryCreate(out var exRecord))
                {
                    DialogHelper.ShowSuccess($"新建文件夹成功。");
                    Refresh();
                }
                else
                {
                    DialogHelper.ShowError($"新建文件夹失败：{exRecord.ExceptionType.GetDiscription()}。");
                }
            }
        }

        private void Refresh()
        {
            GoTo(Drive.GetDirectory(Path));
        }

        private void DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is FileSystemBaseVM vm)
            {
                DoubleClick(vm.FileSystemBase);
            }
        }

        public class FileSystemBaseVM
        {
            public FileSystemBase FileSystemBase { get; }

            public ExplorerWindow ParentWindow { get; }

            public bool IsFile { get; }

            public string Name => FileSystemBase.Name;

            public DateTime LastWriteTime => FileSystemBase.LastWriteTime;

            public long Size => IsFile ? ((FileBase)FileSystemBase).Size : 0L;

            public ImageSource Icon => IsFile ? SHFileInfoHelper.GetFileIconImage(((FileBase)FileSystemBase).Extension, false) : SHFileInfoHelper.SmallFolderIconImage;

            public string TypeInfo => IsFile ? SHFileInfoHelper.GetTypeInfo(((FileBase)FileSystemBase).Extension) : SHFileInfoHelper.FolderInfo;

            public FileSystemBaseVM(FileSystemBase fileSystemBase, ExplorerWindow parentWindow)
            {
                FileSystemBase = fileSystemBase;
                ParentWindow = parentWindow;
                IsFile = fileSystemBase is FileBase;
            }
        }
    }
}
