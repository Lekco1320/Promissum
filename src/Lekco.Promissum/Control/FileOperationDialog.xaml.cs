using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// FileOperationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FileOperationDialog : CustomWindow, IInteractive, INotifyPropertyChanged
    {
        public DriveBase SourceDrive { get; }

        public FileBase SourceFile { get; }

        public string SourceDriveName => SourceDrive.Name;

        public string SourceFileName => SourceFile.Name;

        public PathBase? TargetPath { get; set; }

        [DependsOn(nameof(TargetPath))]
        public string TargetDriveName => TargetPath != null ? TargetPath.Drive.Name : "无";

        [DependsOn(nameof(TargetPath))]
        public string TargetDirectoryName => TargetPath?.RelativePath switch
        {
            null => "无",
            "" => "{根目录}",
            _ => TargetPath.RelativePath
        };

        public bool IsCopy { get; set; }

        public bool IsMove { get; set; }

        public bool IsDelete { get; set; }

        [DependsOn(nameof(TargetPath))]
        public bool CanOK => TargetPath != null;

        public bool IsOK { get; protected set; }

        public RelayCommand SelectTargetPathCommand => new RelayCommand(SelectTargetPath);

        public ICommand OKCommand => new RelayCommand(OK);

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected FileOperationDialog(DriveBase drive, FileBase file, OperationType operation)
        {
            SourceDrive = drive;
            SourceFile = file;

            var _ = operation switch
            {
                OperationType.Copy => IsCopy = true,
                OperationType.Move => IsMove = true,
                OperationType.Delete => IsDelete = true,
                _ => throw new ArgumentException("Invalid operation value.", nameof(operation))
            };

            InitializeComponent();
            DataContext = this;
        }

        private void SelectTargetPath()
        {
            if (PathSelectorDialog.Select(out var path))
            {
                TargetPath = path;
            }
        }

        private void OK()
        {
            if ((IsCopy || IsMove) && TargetPath == null)
            {
                DialogHelper.ShowError("请选择目标路径。");
                return;
            }
            IsOK = true;
            Close();
        }

        private void Cancel()
        {
            Close();
        }

        public static void OperateFile(DriveBase drive, FileBase file, OperationType operation)
        {
            var window = new FileOperationDialog(drive, file, operation);
            window.ShowDialog();
            if (!window.IsOK)
            {
                return;
            }

            var path = window.TargetPath!;
            OperationType type = OperationType.Copy;
            ExceptionRecord? exRecord = null;
            if (window.IsCopy)
            {
                type = OperationType.Copy;
                var newFile = path.GetFile(file.Name);
                file.TryCopyTo(newFile, out exRecord);
            }
            else if (window.IsMove)
            {
                type = OperationType.Move;
                var newFile = path.GetFile(file.Name);
                file.TryMoveTo(newFile, true, out exRecord);
            }
            else if (window.IsDelete)
            {
                type = OperationType.Delete;
                file.TryDelete(out exRecord);
            }
            if (exRecord != null)
            {
                DialogHelper.ShowError($"文件{type.GetDiscription()}失败：{exRecord.ExceptionType.GetDiscription()}。");
            }
            else
            {
                DialogHelper.ShowSuccess($"文件{type.GetDiscription()}成功。");
            }
        }
    }
}
