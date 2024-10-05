using Lekco.Promissum.Model.Sync;
using Lekco.Wpf.MVVM;
using System;
using System.Collections.Generic;
using Lekco.Promissum.Model.Engine;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Control;
using System.Linq;

namespace Lekco.Promissum.ViewModel.Sync
{
    public class SyncEngineWindowVM : ViewModelBase
    {
        public IEnumerable<SyncTask> LoadedTasks { get; set; }

        public IEnumerable<SyncTaskVM> LoadedTaskVMs { get; set; }

        public IEnumerable<SyncEngine.QueuedTask> QueuedTasks { get; set; }

        public IEnumerable<QueuedTaskVM> QueuedTaskVMs { get; set; }

        public RelayCommand<CustomWindow> CloseCommand => new RelayCommand<CustomWindow>(Close);

        public SyncEngineWindowVM()
        {
            int id = 1;
            LoadedTasks = SyncEngine.GetLoadedTasks();
            LoadedTaskVMs = LoadedTasks.Select(task => new SyncTaskVM(id++, task));
            id = 1;
            QueuedTasks = SyncEngine.GetQueuedTasks();
            QueuedTaskVMs = QueuedTasks.Select(task => new QueuedTaskVM(id++, task));
            SyncEngine.LoadedTasksChanged += OnLoadedTasksChanged;
            SyncEngine.QueuedTasksChanged += OnQueuedTasksChanged;
        }

        private void OnLoadedTasksChanged(object? sender, IEnumerable<SyncTask> e)
        {
            int id = 1;
            LoadedTasks = e;
            LoadedTaskVMs = LoadedTasks.Select(task => new SyncTaskVM(id++, task));
        }

        private void OnQueuedTasksChanged(object? sender, IEnumerable<SyncEngine.QueuedTask> e)
        {
            int id = 1;
            QueuedTasks = e;
            QueuedTaskVMs = QueuedTasks.Select(task => new QueuedTaskVM(id++, task));
        }

        private void Close(CustomWindow window)
        {
            SyncEngine.LoadedTasksChanged -= OnLoadedTasksChanged;
            SyncEngine.QueuedTasksChanged -= OnQueuedTasksChanged;
            window.Close();
        }

        public class SyncTaskVM : ViewModelBase
        {
            public SyncTask SyncTask { get; }

            public int ID { get; }

            public string ProjectName => SyncTask.ParentProject.Name;

            public string TaskName => SyncTask.Name;

            public string TriggerConditions
            {
                get
                {
                    if (!SyncTask.EnableSchedule)
                    {
                        return "无";
                    }
                    var triggers = new List<string>();
                    if (SyncTask.Schedule.OnDriveReady)
                    {
                        triggers.Add("设备就绪");
                    }
                    if (SyncTask.Schedule.OnScheduledDays != Day.None)
                    {
                        triggers.Add("定期触发");
                    }
                    if (SyncTask.Schedule.OnInterval != TimeSpan.Zero)
                    {
                        triggers.Add("间隔触发");
                    }
                    return triggers.Count > 0 ? string.Join("，", triggers) : "无";
                }
            }

            public string TaskState
            {
                get
                {
                    if (SyncTask.IsBusy)
                    {
                        return "正在忙碌";
                    }
                    if (SyncTask.IsSuspended)
                    {
                        return "已挂起";
                    }
                    return SyncTask.IsReady ? "就绪" : "尚未就绪";
                }
            }

            public SyncTaskVM(int id, SyncTask syncTask)
            {
                SyncTask = syncTask;
                ID = id;

                RegisterNotifier(SyncTask.ParentProject, nameof(SyncProject.Name), nameof(ProjectName));
                RegisterNotifier(SyncTask, nameof(SyncTask.Name), nameof(TaskName));
                RegisterNotifier(SyncTask, nameof(SyncTask.EnableSchedule), nameof(TriggerConditions));
                RegisterNotifier(SyncTask.Schedule, nameof(Schedule.OnScheduledDays), nameof(TriggerConditions));
                RegisterNotifier(SyncTask.Schedule, nameof(Schedule.OnDriveReady), nameof(TriggerConditions));
                RegisterNotifier(SyncTask.Schedule, nameof(Schedule.OnInterval), nameof(TriggerConditions));
                RegisterNotifier(SyncTask, nameof(SyncTask.IsReady), nameof(TaskState));
                RegisterNotifier(SyncTask, nameof(SyncTask.IsBusy), nameof(TaskState));
                RegisterNotifier(SyncTask, nameof(SyncTask.IsSuspended), nameof(TaskState));
            }
        }

        public class QueuedTaskVM
        {
            public SyncEngine.QueuedTask QueuedTask { get; }

            public int ID { get; }

            public string ProjectName => QueuedTask.SyncTask.ParentProject.Name;

            public string TaskName => QueuedTask.SyncTask.Name;

            public ExecutionTrigger ExecutionTrigger => QueuedTask.ExecutionTrigger;

            public QueuedTaskVM(int id, SyncEngine.QueuedTask queuedTask)
            {
                ID = id;
                QueuedTask = queuedTask;
            }
        }
    }
}
