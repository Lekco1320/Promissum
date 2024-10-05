using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Disk;
using Lekco.Promissum.Model.Sync.MTP;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using PropertyChanged;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// PathSelectorDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PathSelectorDialog : CustomWindow, IInteractive, INotifyPropertyChanged
    {
        [OnChangedMethod(nameof(ClearDriveAndPath))]
        public bool IsMTPDrive { get; set; }

        public RelayCommand SelectDriveCommand => new RelayCommand(SelectMTPDrive);

        public RelayCommand SelectPathCommand => new RelayCommand(SelectPath);

        public DriveBase? SelectedDrive { get; set; }

        [DependsOn(nameof(SelectedDrive))]
        public string DriveType => SelectedDrive?.DriveType.GetDiscription() ?? "无";

        [DependsOn(nameof(SelectedDrive))]
        public string DriveModel => SelectedDrive?.Model ?? "无";

        [DependsOn(nameof(SelectedDrive))]
        public string DriveID => SelectedDrive?.ID ?? "无";

        [DependsOn(nameof(SelectedDrive))]
        public string DriveUsage
        {
            get
            {
                if (SelectedDrive == null)
                {
                    return "无";
                }
                return $"{new FileSize(SelectedDrive.AvailableSpace)} / {new FileSize(SelectedDrive.TotalSpace)}";
            }
        }

        public string? RelativePath
        {
            get
            {
                return _relativePath switch
                {
                    null => "无",
                    "" => "{根目录}",
                    _ => _relativePath
                };
            }
            set => _relativePath = value;
        }
        private string? _relativePath;

        public bool IsOK { get; protected set; }

        public ICommand OKCommand => new RelayCommand(OK);

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected PathSelectorDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void ClearDriveAndPath()
        {
            SelectedDrive = null;
            RelativePath = null;
        }

        private void SelectDiskPath()
        {
            var dialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "选择目标目录"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = dialog.SelectedPath;
                var pathRoot = Path.GetPathRoot(path)!;
                SelectedDrive = new DiskDrive(new DriveInfo(pathRoot));
                RelativePath = dialog.SelectedPath[pathRoot.Length..];
            }
        }

        private void SelectMTPPath()
        {
            if (SelectedDrive == null)
            {
                DialogHelper.ShowError("请先选择目标设备。");
                return;
            }

            if (DirectorySelectorDialog.ShowDialog(SelectedDrive.RootDirectory, out var target))
            {
                RelativePath = target.FullName[1..];
            }
        }

        private void SelectPath()
        {
            if (IsMTPDrive)
            {
                SelectMTPPath();
            }
            else
            {
                SelectDiskPath();
            }
        }

        private void SelectMTPDrive()
        {
            if (DriveSelectorDialog.ShowDialog(DriveCategory.MTPDrive, out var drive))
            {
                SelectedDrive = drive;
                RelativePath = null;
            }
        }

        private void OK()
        {
            if (SelectedDrive == null || _relativePath == null)
            {
                DialogHelper.ShowError("所选的设备或目录非法。");
                return;
            }

            IsOK = true;
            Close();
        }

        private void Cancel()
            => Close();

        public static bool Select([MaybeNullWhen(false)] out PathBase path)
        {
            path = null;
            var selector = new PathSelectorDialog();
            selector.ShowDialog();
            if (selector.IsOK)
            {
                path = selector.IsMTPDrive ? new MTPPath((MTPDrive)selector.SelectedDrive!, selector._relativePath!)
                                           : new DiskPath((DiskDrive)selector.SelectedDrive!, selector._relativePath!);
                return true;
            }
            return false;
        }
    }
}
