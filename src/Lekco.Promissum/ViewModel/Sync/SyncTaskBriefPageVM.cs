using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.View.Sync;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using Lekco.Wpf.Utility.Navigation;
using Lekco.Wpf.Utility.Progress;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using DriveType = Lekco.Promissum.Model.Sync.Base.DriveType;

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

        public bool TaskCanExecute => SyncTask.IsReady && !SyncTask.IsSuspended;

        public bool TaskIsSuspended => SyncTask.IsSuspended;

        public bool TaskCanSuspend => !SyncTask.IsSuspended && !TaskIsBusy;

        public string TaskName => SyncTask.Name;

        public string SourceDriveName => SyncTask.Source.Drive.Name;

        public DriveType SourceDriveType => SyncTask.Source.Drive.DriveType;

        public bool SourceIsReady => SyncTask.Source.IsReady;

        public string SourcePath => SyncTask.Source.RelativePath;

        public double SourceUsage => 100d * SyncTask.Source.Drive.UsedSpace / SyncTask.Source.Drive.TotalSpace;

        public string SourceSpace => $"{new FileSize(SyncTask.Source.Drive.UsedSpace)} / " +
                                     $"{new FileSize(SyncTask.Source.Drive.TotalSpace)}";

        public ICommand OpenSourcePathCommand => new RelayCommand(() => OpenDirectory(SyncTask.Source.Drive, SyncTask.Source.Directory));

        public string DestinationDriveName => SyncTask.Destination.Drive.Name;

        public DriveType DestinationDriveType => SyncTask.Destination.Drive.DriveType;

        public bool DestinationIsReady => SyncTask.Destination.IsReady;

        public string DestinationPath => SyncTask.Destination.RelativePath;

        public double DestinationUsage => 100d * SyncTask.Destination.Drive.UsedSpace / SyncTask.Destination.Drive.TotalSpace;

        public string DestinationSpace => $"{new FileSize(SyncTask.Destination.Drive.UsedSpace)} / " +
                                          $"{new FileSize(SyncTask.Destination.Drive.TotalSpace)}";

        public ICommand OpenDestinationPathCommand => new RelayCommand(() => OpenDirectory(SyncTask.Destination.Drive, SyncTask.Destination.Directory));

        public string TaskCreationTime => SyncTask.CreationTime.ToString();

        public string LastExecuteTime => SyncTask.LastExecuteTime != DateTime.MinValue ? SyncTask.LastExecuteTime.ToString() : "从未执行";

        public FileSyncMode FileSyncMode => SyncTask.FileSyncMode;

        public bool EnableSchedule => SyncTask.EnableSchedule;

        public bool EnableCleanUp => SyncTask.EnableCleanUpBehavior;

        public ICommand ViewRecordsCommand => new RelayCommand(async () => await ViewFilesRecords());

        public SyncTaskBriefPageVM(SyncTask task)
        {
            SyncTask = task;
            NavigationService = NavigationServiceManager.GetService(task.ParentProject);

            RegisterNotifier(task, nameof(task.IsBusy), [nameof(TaskIsBusy), nameof(TaskCanSuspend)]);
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
            SyncTask.BusyAction(() => SyncTask.IsSuspended = true);
            // SyncTask.ParentProject.Save();
        }

        protected void RestoreTask()
        {
            SyncTask.BusyAction(() => SyncTask.IsSuspended = false);
        }

        protected void ModifyTask()
        {
            SuspendTask();
            var vm = new SyncTaskModifyPageVM(SyncTask);
            var view = new SyncTaskModifyPage(vm);
            var oldView = NavigationService.ChangeView(SyncTask, view);
            vm.OldView = oldView;
        }

        protected void OpenDirectory(DriveBase drive, DirectoryBase directory)
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

        protected async Task ViewFilesRecords()
        {
            var reporter = new LoadRecordsProgressReporter();
            try
            {
                reporter.Begin();
                using var context = SyncTask.GetDbContextReadonly();
                reporter.PushProgress();
                var fileRecords = await context.FileRecords.ToListAsync();
                reporter.PushProgress();
                var cleanUpRecords = await context.CleanUpRecords.ToListAsync();
                reporter.PushProgress();
                var executionRecords = await context.ExecutionRecords.Include(record => record.ExceptionRecords).ToListAsync();
                reporter.End();
                var vm = new SyncRecordsWindowVM(SyncTask, fileRecords, cleanUpRecords, executionRecords);
                new SyncRecordsWindow(vm).Show();
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError($"数据查询失败：{ex.Message}");
            }
            finally
            {
                reporter.End();

                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        protected class LoadRecordsProgressReporter : ISingleProgressReporter
        {
            public event EventHandler<double>? OnProgressValueChanged;

            public event EventHandler<string>? OnProgressTextChanged;

            public event EventHandler? OnProgressBegin;

            public event EventHandler? OnProgressEnd;

            protected int ProgressState;

            private readonly STAThreadHolder<SingleProgressDialog> _STAThreadHolder;

            public LoadRecordsProgressReporter()
            {
                _STAThreadHolder = new STAThreadHolder<SingleProgressDialog>(() => new SingleProgressDialog(this));
            }

            public void Begin()
            {
                OnProgressTextChanged?.Invoke(this, "正在连接数据库……");
                OnProgressValueChanged?.Invoke(this, 20d);
                OnProgressBegin?.Invoke(this, EventArgs.Empty);
            }

            public void End()
            {
                OnProgressTextChanged?.Invoke(this, "正在完成……");
                OnProgressValueChanged?.Invoke(this, 100d);
                OnProgressEnd?.Invoke(this, EventArgs.Empty);
                _STAThreadHolder.Dispose();
            }

            public void PushProgress()
            {
                var (text, value) = ++ProgressState switch
                {
                    1 => ("读取同步记录……", 40d),
                    2 => ("读取清理记录……", 60d),
                    3 => ("读取执行记录……", 80d),
                    _ => ("", double.NaN),
                };
                OnProgressTextChanged?.Invoke(this, text);
                OnProgressValueChanged?.Invoke(this, value);
            }
        }
    }
}
