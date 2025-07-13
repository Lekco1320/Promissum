using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Execution;
using Lekco.Promissum.View.Sync;
using Lekco.Wpf.MVVM;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using Lekco.Wpf.Utility.Navigation;
using System;
using System.Windows.Input;

namespace Lekco.Promissum.ViewModel.Sync
{
    public class SyncTaskBriefPageVM : ViewModelBase
    {
        public SyncTask SyncTask { get; }

        public INavigationService NavigationService { get; }

        public ICommand SuspendCommand => new RelayCommand(SuspendTask);

        public ICommand RestoreCommand => new RelayCommand(RestoreTask);

        public ICommand ExecuteCommand => new RelayCommand(() => SyncEngine.TryExecuteTask(SyncTask, ExecutionTrigger.Manual));

        public ICommand ModifyCommand => new RelayCommand(ModifyTask);

        public bool TaskIsBusy => SyncTask.IsBusy;

        public bool TaskCanExecute => SyncTask.IsReady && !SyncTask.IsSuspended && !SyncTask.IsBusy;

        public bool TaskIsSuspended => SyncTask.IsSuspended;

        public bool TaskCanSuspend => !SyncTask.IsSuspended && !TaskIsBusy;

        public bool TaskCanModify => !SyncTask.IsBusy;

        public string TaskName => SyncTask.Name;

        public string SourceDriveName => SyncTask.Source.Drive.Name;

        public DriveType SourceDriveType => SyncTask.Source.Drive.DriveType;

        public bool SourceIsReady => SyncTask.Source.IsReady;

        public string SourcePath => SyncTask.Source.RelativePath;

        public double SourceUsage => 100d * SyncTask.Source.Drive.UsedSpace / SyncTask.Source.Drive.TotalSpace;

        public string SourceSpace => $"{new FileSize(SyncTask.Source.Drive.AvailableSpace)} / " +
                                     $"{new FileSize(SyncTask.Source.Drive.TotalSpace)}";

        public ICommand OpenSourcePathCommand => new RelayCommand(() => OpenDirectory(SyncTask.Source.Drive, SyncTask.Source.Directory));

        public string DestinationDriveName => SyncTask.Destination.Drive.Name;

        public DriveType DestinationDriveType => SyncTask.Destination.Drive.DriveType;

        public bool DestinationIsReady => SyncTask.Destination.IsReady;

        public string DestinationPath => SyncTask.Destination.RelativePath;

        public double DestinationUsage => 100d * SyncTask.Destination.Drive.UsedSpace / SyncTask.Destination.Drive.TotalSpace;

        public string DestinationSpace => $"{new FileSize(SyncTask.Destination.Drive.AvailableSpace)} / " +
                                          $"{new FileSize(SyncTask.Destination.Drive.TotalSpace)}";

        public ICommand OpenDestinationPathCommand => new RelayCommand(() => OpenDirectory(SyncTask.Destination.Drive, SyncTask.Destination.Directory));

        public string TaskCreationTime => SyncTask.CreationTime.ToString();

        public string LastExecuteTime => SyncTask.LastExecuteTime != DateTime.MinValue ? SyncTask.LastExecuteTime.ToString() : "从未执行";

        public FileSyncMode FileSyncMode => SyncTask.FileSyncMode;

        public bool EnableSchedule => SyncTask.EnableSchedule;

        public bool EnableCleanUp => SyncTask.EnableCleanUpBehavior;

        public ICommand ViewRecordsCommand => new RelayCommand(ViewFilesRecords);

        public SyncTaskBriefPageVM(SyncTask task)
        {
            SyncTask = task;
            NavigationService = NavigationServiceManager.GetService(task.ParentProject);

            RegisterNotifier(task, nameof(task.IsBusy), [nameof(TaskIsBusy), nameof(TaskCanSuspend), nameof(TaskCanModify), nameof(TaskCanExecute)]);
            RegisterNotifier(task, nameof(task.IsReady), nameof(TaskCanExecute));
            RegisterNotifier(task, nameof(task.Name), nameof(TaskName));
            RegisterNotifier(task, nameof(task.IsSuspended), [nameof(TaskIsSuspended), nameof(TaskCanSuspend), nameof(TaskCanExecute)]);
            RegisterNotifier(task.Source, nameof(PathBase.IsReady), nameof(SourceIsReady));
            RegisterNotifier(task.Source.Drive, nameof(DriveBase.AvailableSpace), [nameof(SourceUsage), nameof(SourceUsage), nameof(SourceSpace)]);
            RegisterNotifier(task.Destination, nameof(PathBase.IsReady), nameof(DestinationIsReady));
            RegisterNotifier(task.Destination.Drive, nameof(DriveBase.AvailableSpace), [nameof(DestinationUsage), nameof(DestinationSpace)]);
            RegisterNotifier(task, nameof(task.LastExecuteTime), nameof(LastExecuteTime));
            RegisterNotifier(task, nameof(task.FileSyncMode), nameof(FileSyncMode));
            RegisterNotifier(task, nameof(task.EnableSchedule), nameof(EnableSchedule));
            RegisterNotifier(task, nameof(task.EnableCleanUpBehavior), nameof(EnableCleanUp));
        }

        protected void SuspendTask()
        {
            bool suspended = SyncTask.IsSuspended;
            SyncTask.BusyAction(() => SyncTask.IsSuspended = true);
            if (!SyncTask.ParentProject.SaveWhole())
            {
                SyncTask.BusyAction(() => SyncTask.IsSuspended = suspended);
            }
        }

        protected void RestoreTask()
        {
            bool suspended = SyncTask.IsSuspended;
            SyncTask.BusyAction(() => SyncTask.IsSuspended = false);
            if (!SyncTask.ParentProject.SaveWhole())
            {
                SyncTask.BusyAction(() => SyncTask.IsSuspended = suspended);
            }
        }

        protected void ModifyTask()
        {
            SyncTask.BusyAction(() => SyncTask.IsSuspended = true);
            var vm = new SyncTaskEditPageVM(SyncTask);
            var view = new SyncTaskEditPage(vm);
            var oldView = NavigationService.ChangeView(SyncTask, view);
            vm.OldView = oldView;
        }

        protected static void OpenDirectory(DriveBase drive, DirectoryBase directory)
        {
            if (drive.CheckIsReady())
            {
                try
                {
                    drive.OpenInExplorer(directory);
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowError(ex);
                }
            }
        }

        protected void ViewFilesRecords()
        {
            var context = SyncTask.GetDbContextReadonly();
            var vm = new SyncRecordsWindowVM(SyncTask, context);
            new SyncRecordsWindow(vm).Show();
        }
    }
}
