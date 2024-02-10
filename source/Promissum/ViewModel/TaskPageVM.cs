using Lekco.Promissum.Apps;
using Lekco.Promissum.Sync;
using Lekco.Promissum.Utility;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Lekco.Promissum.ViewModel
{
    public class TaskPageVM : BindableBase
    {
        private SyncProject _parentProject;
        private SyncTask _originTask;
        public SyncTask Task { get; set; }
        public SyncTask OriginTask { get => _originTask; }

        public int SyncPeriodIndex
        {
            get => _syncPeriodIndex;
            set
            {
                _syncPeriodIndex = value;
                RaisePropertyChanged(nameof(SyncPeriodIndex));
            }
        }
        private int _syncPeriodIndex;

        public bool IsByTimeChecked
        {
            get => _isByTimeChecked;
            set
            {
                _isByTimeChecked = value;
                RaisePropertyChanged(nameof(IsByTimeChecked));
            }
        }
        private bool _isByTimeChecked;

        public bool IsBySizeChecked
        {
            get => _isBySizeChecked;
            set
            {
                _isBySizeChecked = value;
                RaisePropertyChanged(nameof(IsBySizeChecked));
            }
        }
        private bool _isBySizeChecked;

        public bool IsByMD5Checked
        {
            get => _isByMD5Checked;
            set
            {
                _isByMD5Checked = value;
                RaisePropertyChanged(nameof(IsByMD5Checked));
            }
        }
        private bool _isByMD5Checked;

        public int DayInterval
        {
            get => _dayInterval;
            set
            {
                _dayInterval = value;
                RaisePropertyChanged(nameof(DayInterval));
            }
        }
        private int _dayInterval;

        public int HourInterval
        {
            get => _hourInterval;
            set
            {
                _hourInterval = value;
                RaisePropertyChanged(nameof(HourInterval));
            }
        }
        private int _hourInterval;

        public int MinuteInterval
        {
            get => _minuteInterval;
            set
            {
                _minuteInterval = value;
                RaisePropertyChanged(nameof(MinuteInterval));
            }
        }
        private int _minuteInterval;

        public int DayTerm
        {
            get => _dayTerm;
            set
            {
                _dayTerm = value;
                RaisePropertyChanged(nameof(DayTerm));
            }
        }
        private int _dayTerm;

        public int HourTerm
        {
            get => _hourTerm;
            set
            {
                _hourTerm = value;
                RaisePropertyChanged(nameof(HourTerm));
            }
        }
        private int _hourTerm;

        public int MinuteTerm
        {
            get => _minuteTerm;
            set
            {
                _minuteTerm = value;
                RaisePropertyChanged(nameof(MinuteTerm));
            }
        }
        private int _minuteTerm;

        public string OriginPath
        {
            get => _originPath;
            set
            {
                _originPath = value;
                RaisePropertyChanged(nameof(OriginPath));
            }
        }
        private string _originPath;

        public string DestinationPath
        {
            get => _destinationPath;
            set
            {
                _destinationPath = value;
                RaisePropertyChanged(nameof(DestinationPath));
            }
        }
        private string _destinationPath;

        public string DeletionPath
        {
            get => _deletionPath;
            set
            {
                _deletionPath = value;
                RaisePropertyChanged(nameof(DeletionPath));
            }
        }
        private string _deletionPath;

        public DelegateCommand ModifyOriginPathCommand { get; set; }
        public DelegateCommand ModifyDestinationPathCommand { get; set; }
        public DelegateCommand NewSyncExclusionCommand { get; set; }
        public DelegateCommand<SyncExclusion> ModifySyncExclusionCommand { get; set; }
        public DelegateCommand<SyncExclusion> DeleteSyncExclusionCommand { get; set; }
        public DelegateCommand ShowSyncFileRecordsCommand { get; set; }
        public DelegateCommand ShowSyncRecordsCommand { get; set; }
        public DelegateCommand ShowDeletionRecordsCommand { get; set; }
        public DelegateCommand OpenDeletionPathCommand { get; set; }
        public DelegateCommand ModifyDeletionPathCommand { get; set; }
        public DelegateCommand SaveTaskCommand { get; set; }
        public DelegateCommand RestoreTaskCommand { get; set; }
        public DelegateCommand SaveAndExecuteCommand { get; set; }

        public TaskPageVM(SyncProject syncProject, SyncTask syncTask)
        {
            _parentProject = syncProject;
            _originTask = syncTask;
            Task = Functions.DeepCopy(syncTask);

            SyncPeriodIndex = (int)Task.SyncPlan.SyncPeriod;
            IsByTimeChecked = Task.CompareMode == CompareMode.ByTime;
            IsBySizeChecked = Task.CompareMode == CompareMode.BySize;
            IsByMD5Checked = Task.CompareMode == CompareMode.ByMD5;
            DayInterval = Task.SyncPlan.IntervalSpan.Days;
            HourInterval = Task.SyncPlan.IntervalSpan.Hours;
            MinuteInterval = Task.SyncPlan.IntervalSpan.Minutes;
            DayTerm = Task.DeletionBehavior.ReserveTerm.Days;
            HourTerm = Task.DeletionBehavior.ReserveTerm.Hours;
            MinuteTerm = Task.DeletionBehavior.ReserveTerm.Minutes;
            _originPath = Task.OriginPath.FullPath;
            _destinationPath = Task.DestinationPath.FullPath;
            _deletionPath = Task.DeletionBehavior.DeletionPath.FullPath;

            ModifyOriginPathCommand = new DelegateCommand(ModifyOriginPath);
            ModifyDestinationPathCommand = new DelegateCommand(ModifyDestinationPath);
            NewSyncExclusionCommand = new DelegateCommand(NewExclusion);
            ModifySyncExclusionCommand = new DelegateCommand<SyncExclusion>(exclusion => ModifyExclusion(exclusion));
            DeleteSyncExclusionCommand = new DelegateCommand<SyncExclusion>(exclusion => DeleteExclusion(exclusion));
            ShowSyncFileRecordsCommand = new DelegateCommand(ShowSyncFileRecords);
            ShowSyncRecordsCommand = new DelegateCommand(ShowSyncRecords);
            ShowDeletionRecordsCommand = new DelegateCommand(ShowDeletionFileRecords);
            OpenDeletionPathCommand = new DelegateCommand(OpenDeletionPath);
            ModifyDeletionPathCommand = new DelegateCommand(ModifyDeletionPath);
            SaveTaskCommand = new DelegateCommand(() => SaveTask());
            RestoreTaskCommand = new DelegateCommand(RestoreTask);
            SaveAndExecuteCommand = new DelegateCommand(SaveAndExecute);
        }

        private static void ModifyExclusion(SyncExclusion syncExclusion)
        {
            SyncExclusionWindow.NewOrModifySyncExclusion(syncExclusion);
        }

        private void NewExclusion()
        {
            var exclusion = SyncExclusionWindow.NewOrModifySyncExclusion(null);
            if (exclusion != null)
            {
                Task.ExclusionBehavior.Exclusions.Add(exclusion);
            }
        }

        private void DeleteExclusion(SyncExclusion syncExclusion)
        {
            Task.ExclusionBehavior.Exclusions.Remove(syncExclusion);
        }

        private void ModifyOriginPath()
        {
            var browserDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "选择备份任务的源文件夹"
            };
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                OriginPath = browserDialog.SelectedPath;
            }
        }

        private void ModifyDestinationPath()
        {
            var browserDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "选择备份任务的目标文件夹"
            };
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                DestinationPath = browserDialog.SelectedPath;
            }
        }

        private void ModifyDeletionPath()
        {
            var browserDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "选择保留文件的指定目录"
            };
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                DeletionPath = browserDialog.SelectedPath;
            }
        }

        private bool SaveTask()
        {
            if (SyncPath.IsSubPath(new SyncPath(OriginPath), new SyncPath(DestinationPath)))
            {
                MessageWindow.ShowDialog(
                    message: "目标目录不得为源目录或其子目录。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
                return false;
            }

            if (Task.SyncPlan.IntervalSync && DayInterval == 0 && HourInterval == 0 && MinuteInterval == 0)
            {
                MessageWindow.ShowDialog(
                    message: "计划任务中隔期执行的间隔时间不得少于1分钟。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
                return false;
            }

            if (Task.DeletionBehavior.UseReserveTerm && DayTerm == 0 && HourTerm == 0 && MinuteTerm == 0)
            {
                MessageWindow.ShowDialog(
                    message: "指定目录中文件保留期限不得少于1分钟。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
                return false;
            }

            if (DestinationPath != Task.DestinationPath.FullPath)
            {
                var info = new DirectoryInfo(DestinationPath);
                if ((info.GetFiles().Length > 0 || info.GetDirectories().Length > 0) &&
                    !MessageWindow.ShowDialog(
                        message: "目标目录中含有文件或文件夹，备份时可能会删除这些文件，建议您选择空目录作为目标目录。是否继续？",
                        icon: MessageWindowIcon.Warning,
                        buttonStyle: MessageWindowButtonStyle.OKCancel
                    ))
                {
                    return false;
                }
            }

            if ((OriginPath != Task.OriginPath.FullPath || DestinationPath != Task.DestinationPath.FullPath) &&
                (Task.HasSyncFileRecord || Task.HasDeletionFileRecord) &&
                !MessageWindow.ShowDialog(
                    message: "变更备份目录将删除已有的“备份记录”和“删除/移动记录”，是否继续？",
                    icon: MessageWindowIcon.Warning,
                    buttonStyle: MessageWindowButtonStyle.OKCancel
                ))
            {
                return false;
            }

            if (DeletionPath != Task.DeletionBehavior.DeletionPath.FullPath && Task.HasDeletionFileRecord &&
                !MessageWindow.ShowDialog(
                    message: "变更保留文件的指定目录将删除已有的“删除/移动记录”，是否继续？",
                    icon: MessageWindowIcon.Warning,
                    buttonStyle: MessageWindowButtonStyle.OKCancel
                ))
            {
                return false;
            }

            if (IsByTimeChecked)
            {
                Task.CompareMode = CompareMode.ByTime;
            }
            else if (IsBySizeChecked)
            {
                Task.CompareMode = CompareMode.BySize;
            }
            else
            {
                Task.CompareMode = CompareMode.ByMD5;
            }

            Task.SyncPlan.SyncPeriod = (SyncPeriod)SyncPeriodIndex;
            Task.SyncPlan.IntervalSpan = TimeSpan.FromMinutes(DayInterval * 1440 + HourInterval * 60 + MinuteInterval);
            Task.DeletionBehavior.ReserveTerm = TimeSpan.FromMinutes(DayTerm * 1440 + HourTerm * 60 + MinuteTerm);

            _originTask.BusyAction(task =>
            {
                task.Name = Task.Name;
                task.SetOriginPath(_parentProject, new SyncPath(OriginPath));
                task.SetDestinationPath(_parentProject, new SyncPath(DestinationPath));
                task.CompareMode = Task.CompareMode;
                task.SyncPlan = Task.SyncPlan;
                task.DeletionBehavior = Task.DeletionBehavior;
                task.SetDeletionPath(_parentProject, new SyncPath(DeletionPath));
                task.ExclusionBehavior = Task.ExclusionBehavior;
            });

            try
            {
                _parentProject.Save();
            }
            catch (IOException)
            {
                MessageWindow.ShowDialog(
                    message: $"项目“{_parentProject.Name}”保存失败：文件正被占用。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
            catch (SubTaskIsRunningException ex)
            {
                MessageWindow.ShowDialog(
                    message: $"项目“{ex.ParentProject}”的任务“{ex.SubTask.Name}”正在执行，请稍后保存。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }

            Task = Functions.DeepCopy(_originTask);
            RaisePropertyChanged(nameof(Task));
            SyncEngine.LoadAutoRunProject(_parentProject);

            return true;
        }

        private void RestoreTask()
        {
            SyncPeriodIndex = (int)Task.SyncPlan.SyncPeriod;
            IsByTimeChecked = Task.CompareMode == CompareMode.ByTime;
            IsBySizeChecked = Task.CompareMode == CompareMode.BySize;
            IsByMD5Checked = Task.CompareMode == CompareMode.ByMD5;
            DayInterval = Task.SyncPlan.IntervalSpan.Days;
            HourInterval = Task.SyncPlan.IntervalSpan.Hours;
            MinuteInterval = Task.SyncPlan.IntervalSpan.Minutes;
            DayTerm = Task.DeletionBehavior.ReserveTerm.Days;
            HourTerm = Task.DeletionBehavior.ReserveTerm.Hours;
            MinuteTerm = Task.DeletionBehavior.ReserveTerm.Minutes;
            OriginPath = Task.OriginPath.FullPath;
            DestinationPath = Task.DestinationPath.FullPath;
            DeletionPath = Task.DeletionBehavior.DeletionPath.FullPath;

            Task = Functions.DeepCopy(_originTask);
            RaisePropertyChanged(nameof(Task));
        }

        private void SaveAndExecute()
        {
            if (SaveTask())
            {
                if (!SyncEngine.TryExecuteTask(_parentProject, _originTask, ExecutionTrigger.Manual))
                {
                    MessageWindow.ShowDialog(
                        message: $"任务“{_originTask.Name}”已在执行队列中。",
                        icon: MessageWindowIcon.Information,
                        buttonStyle: MessageWindowButtonStyle.OK
                    );
                }
            }
        }

        private void OpenDeletionPath()
        {
            var directoryInfo = new DirectoryInfo(Task.DeletionBehavior.DeletionPath.FullPath);
            if (directoryInfo.Exists)
            {
                Process.Start("explorer.exe", directoryInfo.FullName);
            }
        }

        private void ShowSyncFileRecords()
        {
            new SyncFileRecordWindow(new SyncFileRecordWindowVM(_parentProject, _originTask)).Show();
        }

        private void ShowSyncRecords()
        {
            new SyncRecordWindow(new SyncRecordWindowVM(_parentProject, _originTask)).Show();
        }

        private void ShowDeletionFileRecords()
        {
            new DeletionRecordWindow(new DeletionRecordWindowVM(_parentProject, _originTask)).Show();
        }
    }
}
