using Lekco.Promissum.Control;
using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using PropertyChanged;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Lekco.Promissum.View.Sync
{
    /// <summary>
    /// NewTaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewSyncTaskDialog : CustomWindow, INotifyPropertyChanged
    {
        public SyncProject SyncProject { get; }

        public string TaskName { get; set; }

        public int FileSyncModeIndex { get; set; }

        public RelayCommand ChangeSourcePathCommand => new RelayCommand(ChangeSourcePath);

        public RelayCommand ChangeDestinationPathCommand => new RelayCommand(ChangeDestinationPath);

        public PathBase? SourcePath { get; set; }

        [DependsOn(nameof(SourcePath))]
        public string SourceDriveType => SourcePath?.Drive.DriveType.GetDiscription() ?? "无";

        [DependsOn(nameof(SourcePath))]
        public string SourceDriveModel => SourcePath?.Drive.Model ?? "无";

        [DependsOn(nameof(SourcePath))]
        public string SourceDriveID => SourcePath?.Drive.ID ?? "无";

        [DependsOn(nameof(SourcePath))]
        public string SourceRelativePath => SourcePath?.RelativePath ?? "无";

        public PathBase? DestinationPath { get; set; }

        [DependsOn(nameof(DestinationPath))]
        public string DestinationDriveType => DestinationPath?.Drive.DriveType.GetDiscription() ?? "无";

        [DependsOn(nameof(DestinationPath))]
        public string DestinationDriveModel => DestinationPath?.Drive.Model ?? "无";

        [DependsOn(nameof(DestinationPath))]
        public string DestinationDriveID => DestinationPath?.Drive.ID ?? "无";

        [DependsOn(nameof(DestinationPath))]
        public string DestinationRelativePath => DestinationPath?.RelativePath ?? "无";

        public bool IsOK { get; protected set; }

        public RelayCommand OKCommand => new RelayCommand(OK);

        public RelayCommand CancelCommand => new RelayCommand(Cancel);

        public event PropertyChangedEventHandler? PropertyChanged;

        private NewSyncTaskDialog(SyncProject syncProject)
        {
            SyncProject = syncProject;
            TaskName = "";
            FileSyncModeIndex = 1;

            InitializeComponent();
            DataContext = this;
        }

        private void ChangeSourcePath()
        {
            if (PathSelectorDialog.Select(out var path))
            {
                SourcePath = path;
            }
        }

        private void ChangeDestinationPath()
        {
            if (PathSelectorDialog.Select(out var path))
            {
                DestinationPath = path;
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(TaskName))
            {
                DialogHelper.ShowError("任务名不得为空。");
                return false;
            }
            if (SyncProject.Tasks.Any(task => task.Name == TaskName))
            {
                DialogHelper.ShowError($"项目\"{SyncProject.Name}\"已有一个名为\"{TaskName}\"的任务。");
                return false;
            }
            if (SourcePath == null || DestinationPath == null)
            {
                DialogHelper.ShowError($"同步源目录和同步至目录不得为空。");
                return false;
            }
            if (SourcePath.IsSubPath(DestinationPath) || DestinationPath.IsSubPath(SourcePath))
            {
                DialogHelper.ShowError($"同步源目录和同步至目录不得相同或互为子目录。");
                return false;
            }
            return true;
        }

        private void OK()
        {
            if (Validate())
            {
                IsOK = true;
                Close();
            }
        }

        private void Cancel()
            => Close();

        public static bool NewTask(SyncProject syncProject, [MaybeNullWhen(false)] out SyncTask newTask)
        {
            var dialog = new NewSyncTaskDialog(syncProject);
            dialog.ShowDialog();
            newTask = dialog.IsOK ? new SyncTask(dialog.TaskName, dialog.SourcePath!, dialog.DestinationPath!, syncProject)
            { FileSyncMode = (FileSyncMode)dialog.FileSyncModeIndex } : null;
            return dialog.IsOK;
        }
    }
}
