using Lekco.Promissum.Control;
using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Wpf.MVVM;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using Lekco.Wpf.Utility.Navigation;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lekco.Promissum.ViewModel.Sync
{
    public class SyncTaskEditPageVM : ViewModelBase
    {
        public SyncTask SyncTask { get; }

        public object? OldView { get; set; }

        public INavigationService NavigationService { get; set; }

        public string TaskName { get; set; }

        public RelayCommand CancelCommand => new RelayCommand(Cancel);

        public RelayCommand SaveCommand => new RelayCommand(Save);

        public string SourceDriveName => $"{SyncTask.Source.Drive.Name} ({SyncTask.Source.Drive.DriveType.GetDiscription()})";

        public string SourcePath => SyncTask.Source.RelativePath;

        public string DestinationDriveName => $"{SyncTask.Destination.Drive.Name} ({SyncTask.Destination.Drive.DriveType.GetDiscription()})";

        public string DestinationPath => SyncTask.Destination.RelativePath;

        public int FileSyncModeIndex { get; set; }

        public bool EnableCleanUp { get; set; }

        public bool EnableReserve { get; set; }

        public RelayCommand SelectReserveCommand => new RelayCommand(SelectReservePath);

        public PathBase? ReservePath { get; set; }

        [DependsOn(nameof(ReservePath))]
        public string ReserveDriveName => ReservePath?.Drive.Name ?? "无";

        [DependsOn(nameof(ReservePath))]
        public string ReserveRelativePath => ReservePath?.RelativePath ?? "无";

        public bool EnableRetentionPeriod { get; set; }

        public int RetentionPeriodDay { get; set; }

        public int RetentionPeriodHour { get; set; }

        public int RetentionPeriodMinute { get; set; }

        public bool EnableVersioning { get; set; }

        public bool EnableMaxVersionRetention { get; set; }

        public int MaxVersion { get; set; }

        public bool EnableExclusionRules { get; set; }

        public RelayCommand AddRuleCommand => new RelayCommand(AddRule);

        public RelayCommand<ExclusionRule> ModifyRuleCommand => new RelayCommand<ExclusionRule>(ModifyRule);

        public RelayCommand<ExclusionRule> RemoveRuleCommand => new RelayCommand<ExclusionRule>(RemoveRule);

        public ObservableCollection<ExclusionRule> ExclusionRules { get; set; }

        public bool EnableSchedule { get; set; }

        public bool EnableOnDriveReady { get; set; }

        public bool EnableScheduledDays { get; set; }

        public ObservableCollection<int> ScheduledDays { get; set; }

        public bool EnableOnInterval { get; set; }

        public int IntervalDay { get; set; }

        public int IntervalHour { get; set; }

        public int IntervalMinute { get; set; }

        public SyncTaskEditPageVM(SyncTask syncTask)
        {
            SyncTask = syncTask;
            NavigationService = NavigationServiceManager.GetService(SyncTask.ParentProject);

            TaskName = SyncTask.Name;
            FileSyncModeIndex = (int)SyncTask.FileSyncMode;
            EnableCleanUp = SyncTask.EnableCleanUpBehavior;
            EnableReserve = SyncTask.CleanUpBehavior.EnableReserve;
            ReservePath = SyncTask.CleanUpBehavior.ReservedPath;
            EnableRetentionPeriod = SyncTask.CleanUpBehavior.EnableRetentionPeriod;
            RetentionPeriodDay = SyncTask.CleanUpBehavior.RetentionPeriod.Days;
            RetentionPeriodHour = SyncTask.CleanUpBehavior.RetentionPeriod.Hours;
            RetentionPeriodMinute = SyncTask.CleanUpBehavior.RetentionPeriod.Minutes;
            EnableVersioning = SyncTask.CleanUpBehavior.EnableVersioning;
            EnableMaxVersionRetention = SyncTask.CleanUpBehavior.EnableMaxVersionRetention;
            MaxVersion = SyncTask.CleanUpBehavior.MaxVersion;
            EnableExclusionRules = SyncTask.EnableExclusionRules;
            ExclusionRules = new ObservableCollection<ExclusionRule>(SyncTask.ExclusionRules);
            EnableSchedule = SyncTask.EnableSchedule;
            EnableOnDriveReady = SyncTask.Schedule.OnDriveReady;
            EnableScheduledDays = SyncTask.Schedule.OnScheduledDays != Day.None;
            ScheduledDays = new ObservableCollection<int>(SyncTask.Schedule.OnScheduledDays.SplitFlags().Select(flag => flag.GetIndex()));
            EnableOnInterval = SyncTask.Schedule.OnInterval != TimeSpan.Zero;
            IntervalDay = SyncTask.Schedule.OnInterval.Days;
            IntervalHour = SyncTask.Schedule.OnInterval.Hours;
            IntervalMinute = SyncTask.Schedule.OnInterval.Minutes;
        }

        protected void GoBack()
        {
            SyncTask.BusyAction(() => SyncTask.IsSuspended = false);
            _ = NavigationService.ChangeView(SyncTask, OldView!);
        }

        protected void Cancel()
        {
            if (DialogHelper.ShowWarning("尚有未保存的更改，是否仍要返回？"))
            {
                GoBack();
            }
        }

        protected void Save()
        {
            if (ValidateTaskName() && ValidateReservePath())
            {
                SyncTask.BusyAction(DumpToTask);
                SyncTask.ParentProject.SyncProjectFile.Save();
                GoBack();
            }
        }

        private bool ValidateTaskName()
        {
            if (TaskName == "")
            {
                DialogHelper.ShowError("任务名不得为空。");
                return false;
            }
            if (TaskName != SyncTask.Name && SyncTask.ParentProject.Tasks.Any(task => task.Name == TaskName))
            {
                DialogHelper.ShowError($"项目\"{SyncTask.ParentProject.Name}\"已有一个名为\"{TaskName}\"的任务。");
                return false;
            }
            return true;
        }

        private bool ValidateReservePath()
        {
            if (!EnableCleanUp || !EnableReserve)
            {
                return true;
            }

            if (ReservePath == null)
            {
                DialogHelper.ShowError("保留目录不得为空。");
                return false;
            }

            if (SyncTask.Source.IsSubPath(ReservePath) || SyncTask.Destination.IsSubPath(ReservePath))
            {
                DialogHelper.ShowError("保留目录不得为源目录或目标目录的子目录。");
                return false;
            }

            if (SyncTask.EnableCleanUpBehavior &&
               (!SyncTask.CleanUpBehavior.ReservedPath?.Equals(ReservePath) ?? false))
            {
                return DialogHelper.ShowError("更变保留目录将删除目录清理数据，是否继续？");
            }

            return true;
        }

        private void DumpToTask()
        {
            SyncTask.Name = TaskName;
            SyncTask.FileSyncMode = (FileSyncMode)FileSyncModeIndex;
            SyncTask.EnableCleanUpBehavior = EnableCleanUp;
            SyncTask.CleanUpBehavior.EnableReserve = EnableReserve;
            SyncTask.CleanUpBehavior.ReservedPath = EnableReserve ? ReservePath : null;
            SyncTask.CleanUpBehavior.RetentionPeriod = new TimeSpan(RetentionPeriodDay, RetentionPeriodHour, RetentionPeriodMinute, 0);
            SyncTask.CleanUpBehavior.EnableRetentionPeriod = EnableRetentionPeriod && SyncTask.CleanUpBehavior.RetentionPeriod != TimeSpan.Zero;
            SyncTask.CleanUpBehavior.EnableVersioning = EnableVersioning;
            SyncTask.CleanUpBehavior.EnableMaxVersionRetention = EnableMaxVersionRetention;
            SyncTask.CleanUpBehavior.MaxVersion = MaxVersion;
            SyncTask.EnableExclusionRules = EnableExclusionRules;
            SyncTask.ExclusionRules = new List<ExclusionRule>(ExclusionRules);
            SyncTask.EnableSchedule = EnableSchedule;
            SyncTask.Schedule.OnDriveReady = EnableOnDriveReady;
            Day day = Day.None;
            foreach (int id in ScheduledDays)
            {
                day |= EnumHelper.GetEnumByIndex<Day>(id);
            }
            SyncTask.Schedule.OnScheduledDays = EnableScheduledDays && day != Day.None ? day : Day.None;
            var interval = new TimeSpan(IntervalDay, IntervalHour, IntervalMinute, 0);
            SyncTask.Schedule.OnInterval = EnableOnInterval && interval != TimeSpan.Zero ? interval : TimeSpan.Zero;

            if (SyncTask.EnableCleanUpBehavior &&
               (!SyncTask.CleanUpBehavior.ReservedPath?.Equals(ReservePath) ?? false))
            {
                SyncTask.DeleteDataSet(SyncDataSetType.CleanUpDataSet);
            }

            SyncEngine.LoadTask(SyncTask);
        }

        private void SelectReservePath()
        {
            if (PathSelectorDialog.Select(out var path))
            {
                ReservePath = path;
            }
        }

        private void AddRule()
        {
            if (ExclusionRuleEditorDialog.NewOrModifyRule(null) is ExclusionRule rule)
            {
                ExclusionRules.Add(rule);
            }
        }

        private void ModifyRule(ExclusionRule rule)
        {
            ExclusionRuleEditorDialog.NewOrModifyRule(rule);
        }

        private void RemoveRule(ExclusionRule rule)
        {
            if (DialogHelper.ShowWarning("是否要删除该排除规则？"))
            {
                ExclusionRules.Remove(rule);
            }
        }
    }
}
