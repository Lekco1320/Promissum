using ConcurrentCollections;
using Lekco.Promissum.Model.Sync;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;

namespace Lekco.Promissum.Model.Engine
{
    /// <summary>
    /// The core engine of Lekco Promissum for managing projects, executing tasks and automatic actions. 
    /// This is a singleton class.
    /// </summary>
    public static class SyncEngine
    {
        /// <summary>
        /// The set storing all ready tasks.
        /// </summary>
        private static readonly ConcurrentHashSet<SyncTask> ReadyScheduledTasks = new();

        /// <summary>
        /// The set storing all not ready tasks.
        /// </summary>
        private static readonly ConcurrentHashSet<SyncTask> UnreadyScheduledTasks = new();

        /// <summary>
        /// The set storing all tasks doesn't need triggering.
        /// </summary>
        private static readonly ConcurrentHashSet<SyncTask> UntriggerTasks = new();

        /// <summary>
        /// The set storing all projects which need be loaded automatically when Promissum launched.
        /// </summary>
        private static readonly ConcurrentHashSet<string> AutoLoadProjectPaths;

        /// <summary>
        /// The dictionary storing pairs of projects and their files' name.
        /// </summary>
        private static readonly ConcurrentDictionary<string, SyncProject> LoadedProjects = new();

        /// <summary>
        /// The queue storing executing tasks.
        /// </summary>
        private static readonly ConcurrentQueue<QueuedTask> QueuedTasks= new();

        /// <summary>
        /// The set storing tasks in the <see cref="QueuedTasks"/>.
        /// </summary>
        private static readonly ConcurrentHashSet<SyncTask> TasksInQueue = new();

        /// <summary>
        /// The timer used to trigger periodic tasks.
        /// </summary>
        private static readonly Timer DailyTimer;

        /// <summary>
        /// The timer used to trigger interval tasks.
        /// </summary>
        private static readonly Timer IntervalTimer;

        /// <summary>
        /// The watcher for watching the change of drives.
        /// </summary>
        private static readonly ManagementEventWatcher DrivesWatcher;

        /// <summary>
        /// The name of the file which stores config of the engine.
        /// </summary>
        private static readonly string FileName = App.Promissum.AppDir + @"\SyncEngineConfig.xml";

        /// <summary>
        /// Specifies whether the engine is executing tasks.
        /// </summary>
        public static bool IsRunning => _executionThread != null;

        /// <summary>
        /// The count of all loaded projects.
        /// </summary>
        public static int LoadedProjectsCount => LoadedProjects.Count;

        /// <summary>
        /// Occurs when drives changed.
        /// </summary>
        public static event EventHandler? DrivesChanged;

        /// <summary>
        /// Occurs when loaded tasks changes.
        /// </summary>
        public static event EventHandler<IEnumerable<SyncTask>>? LoadedTasksChanged;

        /// <summary>
        /// Occurs when queued tasks changes.
        /// </summary>
        public static event EventHandler<IEnumerable<QueuedTask>>? QueuedTasksChanged;

        /// <summary>
        /// The thread on which tasks are being executed.
        /// </summary>
        private static Thread? _executionThread;

        /// <summary>
        /// The cancel token source of <see cref="_executionThread"/>.
        /// </summary>
        private static readonly CancellationTokenSource _threadCTS = new();

        /// <summary>
        /// The thread-safety lock of <see cref="_executionThread"/>.
        /// </summary>
        private static readonly object _executionLock = new();

        /// <summary>
        /// Static constructor.
        /// </summary>
        static SyncEngine()
        {
            try
            {
                AutoLoadProjectPaths = DataContractHelper.DeserilizeFromFile<ConcurrentHashSet<string>>(FileName);
            }
            catch
            {
                AutoLoadProjectPaths = new ConcurrentHashSet<string>();
                SaveConfigFile();
            }

            var query = new WqlEventQuery("Win32_DeviceChangeEvent", TimeSpan.FromMilliseconds(100));
            DrivesWatcher = new ManagementEventWatcher(query);
            var throttler = new EventThrottler<EventArgs>(TimeSpan.FromSeconds(1));
            throttler.ThrottledEvent += (sender, e) => DrivesChanged?.Invoke(sender, e);
            throttler.ThrottledEvent += TriggerOnDriveReadyTasks;
            DrivesWatcher.EventArrived += throttler.RaiseEvent;
            DrivesWatcher.Start();

            DateTime now = DateTime.Now;
            DateTime nextDay = now.Date.AddDays(1);
            TimeSpan initialDelay = nextDay - now;
            DailyTimer = new Timer(TriggerOnDaysTasks, null, initialDelay, TimeSpan.FromDays(1));
            IntervalTimer = new Timer(TriggerOnIntervalTasks, null, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Save the config file of the engine.
        /// </summary>
        private static void SaveConfigFile()
            => DataContractHelper.SerilizeToFile(AutoLoadProjectPaths, FileName);

        /// <summary>
        /// Load an auto-run project to the engine.
        /// </summary>
        /// <param name="project">A project.</param>
        public static void LoadProject(SyncProject project)
        {
            if (project.IsAutoLoad)
            {
                if (AutoLoadProjectPaths.Add(project.ProjectFileName))
                {
                    SaveConfigFile();
                }
                if (!LoadedProjects.TryGetValue(project.ProjectFileName, out var _))
                {
                    LoadedProjects.TryAdd(project.ProjectFileName, project);
                    foreach (var task in project.Tasks)
                    {
                        LoadTask(task);
                    }
                }
            }
        }

        /// <summary>
        /// Unload an auto-run project from the engine.
        /// </summary>
        /// <param name="project">A project.</param>
        public static void UnloadProject(SyncProject project)
        {
            if (AutoLoadProjectPaths.TryRemove(project.ProjectFileName))
            {
                SaveConfigFile();
            }
            if (LoadedProjects.TryGetValue(project.ProjectFileName, out var _))
            {
                foreach (var task in project.Tasks)
                {
                    UnloadTask(task);
                }
            }
        }

        /// <summary>
        /// Get all loaded tasks in the engine.
        /// </summary>
        /// <returns>All loaded tasks in the engine.</returns>
        public static IEnumerable<SyncTask> GetLoadedTasks()
        {
            var loadedTasks = new List<SyncTask>();
            foreach (var task in ReadyScheduledTasks)
            {
                loadedTasks.Add(task);
            }
            foreach (var task in UnreadyScheduledTasks)
            {
                loadedTasks.Add(task);
            }
            return loadedTasks;
        }

        /// <summary>
        /// Invokes when loaded tasks changed.
        /// </summary>
        private static void OnLoadedTasksChanged()
            => LoadedTasksChanged?.Invoke(null, GetLoadedTasks());

        /// <summary>
        /// Load a scheduled task into the engine.
        /// </summary>
        /// <param name="task">A task.</param>
        public static void LoadTask(SyncTask task)
        {
            if (task.ParentProject.IsAutoLoad && task.EnableSchedule)
            {
                (task.IsReady ? ReadyScheduledTasks : UnreadyScheduledTasks).Add(task);
                OnLoadedTasksChanged();
            }
        }

        /// <summary>
        /// Unload a scheduled task from the engine.
        /// </summary>
        /// <param name="task">A task.</param>
        public static void UnloadTask(SyncTask task)
        {
            bool successed = false;
            successed |= ReadyScheduledTasks.TryRemove(task);
            successed |= UnreadyScheduledTasks.TryRemove(task);
            successed |= UntriggerTasks.TryRemove(task);
            if (successed)
            {
                OnLoadedTasksChanged();
            }
        }

        /// <summary>
        /// Load all auto-load projects recorded in the file.
        /// </summary>
        public static void AutoLoadProjects()
        {
            var list = AutoLoadProjectPaths.ToList();
            foreach (var path in list)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var syncProjectFile = new SyncProjectFile(path);
                        var project = syncProjectFile.SyncProject;
                        LoadProject(project);
                    }
                    catch (Exception ex)
                    {
                        DialogHelper.ShowError(
                            message: $"路径为“{path}”的项目文件受损，加载失败：{ex.Message}。",
                            location: DialogStartUpLocation.RightBottom
                        );
                    }
                }
                else
                {
                    DialogHelper.ShowError(
                        message: $"项目路径“{path}”不存在，备份计划执行失败。此消息将不再提醒。",
                        location: DialogStartUpLocation.RightBottom
                    );
                    AutoLoadProjectPaths.TryRemove(path);
                    SaveConfigFile();
                }
            }
        }

        /// <summary>
        /// Open a project from a specified file.
        /// </summary>
        /// <param name="fileName">File name of the project.</param>
        /// <returns>The sync project.</returns>
        public static SyncProject OpenProject(string fileName)
        {
            if (!LoadedProjects.TryGetValue(fileName, out var project))
            {
                var syncProjectFile = new SyncProjectFile(fileName);
                project = syncProjectFile.SyncProject;
            }
            return project;
        }

        /// <summary>
        /// Get all queued tasks in the engine.
        /// </summary>
        /// <returns>All queued tasks in the engine.</returns>
        public static IEnumerable<QueuedTask> GetQueuedTasks()
        {
            var queuedTasks = new List<QueuedTask>();
            foreach (var task in QueuedTasks)
            {
                queuedTasks.Add(task);
            }
            return queuedTasks;
        }

        /// <summary>
        /// Get all queued tasks in the engine.
        /// </summary>
        private static void OnQueuedTasksChanged()
            => QueuedTasksChanged?.Invoke(null, GetQueuedTasks());

        /// <summary>
        /// Enqueue a task into the executing queue.
        /// </summary>
        /// <param name="task">The sub task of the project.</param>
        /// <param name="trigger">The execution trigger of the task.</param>
        /// <returns><see langword="true"/> if adds it into executing queue successfully; otherwise, <see langword="false"/>.</returns>
        public static bool TryExecuteTask(SyncTask task, ExecutionTrigger trigger)
        {
            if (!task.IsSuspended && TasksInQueue.Add(task))
            {
                QueuedTasks.Enqueue(new QueuedTask(task, trigger));
                OnQueuedTasksChanged();
                ExecuteQueuedTasks();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Triggers all interval tasks.
        /// </summary>
        /// <param name="obj">The parameter.</param>
        private static void TriggerOnIntervalTasks(object? obj)
        {
            foreach (var task in ReadyScheduledTasks)
            {
                if (!task.IsSuspended && !UntriggerTasks.Contains(task) && task.IsOnIntervalDue)
                {
                    if (task.IsReady)
                    {
                        TryExecuteTask(task, ExecutionTrigger.OnInterval);
                        continue;
                    }
                    HandleUnreadyScheduledTask(task);
                }
            }
        }

        /// <summary>
        /// Triggers periodic tasks.
        /// </summary>
        /// <param name="obj">The parameter.</param>
        private static void TriggerOnDaysTasks(object? obj)
        {
            foreach (var task in ReadyScheduledTasks)
            {
                if (!task.IsSuspended && !UntriggerTasks.Contains(task) && task.IsOnDaysDue)
                {
                    if (task.IsReady)
                    {
                        TryExecuteTask(task, ExecutionTrigger.OnScheduledDays);
                        continue;
                    }
                    HandleUnreadyScheduledTask(task);
                }
            }
        }

        /// <summary>
        /// Handle the unready scheduled task.
        /// </summary>
        /// <param name="task">Unready scheduled task.</param>
        private static void HandleUnreadyScheduledTask(SyncTask task)
        {
            if (!DialogHelper.ShowInformationAsync(
                message: $"任务“{task}”已被触发执行，但相关设备并未就绪，请检查设备是否已接入计算机。点击“取消”后，10分钟内不再触发此任务。",
                buttonStyle: Wpf.Control.MessageDialogButtonStyle.OKCancel,
                title: "通知",
                location: DialogStartUpLocation.RightBottom,
                autoCountDown: true
            ))
            {
                UntriggerTasks.Add(task);
                var timer = new Timer(state =>
                {
                    UntriggerTasks.TryRemove(task);
                    ((Timer?)state)?.Dispose();
                });
                timer.Change(600000, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Triggers tasks whose execution is triggered by disk connection.
        /// </summary>
        private static void TriggerOnDriveReadyTasks(object? sender, EventArgs e)
        {
            foreach (var task in ReadyScheduledTasks)
            {
                if (!task.IsReady)
                {
                    ReadyScheduledTasks.TryRemove(task);
                    UnreadyScheduledTasks.Add(task);
                }
            }
            foreach (var task in UnreadyScheduledTasks)
            {
                if (task.IsReady)
                {
                    UnreadyScheduledTasks.TryRemove(task);
                    ReadyScheduledTasks.Add(task);
                    TryExecuteTask(task, ExecutionTrigger.OnDriveReady);
                }
            }
        }

        /// <summary>
        /// Executes all tasks in the executing queue.
        /// </summary>
        private static void ExecuteQueuedTasks()
        {
            lock (_executionLock)
            {
                if (IsRunning)
                {
                    return;
                }
            }

            _executionThread = new Thread(() =>
            {
                while (!_threadCTS.Token.IsCancellationRequested && QueuedTasks.TryDequeue(out var item))
                {
                    ExecuteTask(item.SyncTask, item.ExecutionTrigger);
                    TasksInQueue.TryRemove(item.SyncTask);
                    OnQueuedTasksChanged();
                }
                _executionThread = null;
            });
            _executionThread.SetApartmentState(ApartmentState.STA);
            _executionThread.IsBackground = true;
            _executionThread.Start();
        }

        /// <summary>
        /// Executes a specified task.
        /// </summary>
        /// <param name="task">The sub task of the project.</param>
        /// <param name="trigger">The execution trigger of the task.</param>
        private static void ExecuteTask(SyncTask task, ExecutionTrigger trigger)
        {
            try
            {
                task.Execute(trigger);
            }
            catch (Exception ex)
            {
                DialogHelper.ShowErrorAsync(ex, location: DialogStartUpLocation.RightBottom);
            }
            finally
            {
                task.ParentProject.SyncProjectFile.Save();
            }
            DialogHelper.ShowSuccessAsync(
                message: $"任务\"{task.Name}\"执行成功。",
                location: DialogStartUpLocation.RightBottom,
                autoCountDown: true
            );
        }

        /// <summary>
        /// Shuts down and releases resources used by engine.
        /// </summary>
        public static void Dispose()
        {
            IntervalTimer.Dispose();
            DailyTimer.Dispose();
            DrivesWatcher.Dispose();
            if (_executionThread != null && _executionThread.IsAlive)
            {
                _threadCTS.Cancel();
                _executionThread.Join();
            }
        }

        /// <summary>
        /// Represents a task that is queued to be executed.
        /// </summary>
        public class QueuedTask
        {
            /// <summary>
            /// The task to be executed.
            /// </summary>
            public SyncTask SyncTask { get; }

            /// <summary>
            /// Trigger which execute the task.
            /// </summary>
            public ExecutionTrigger ExecutionTrigger { get; }

            /// <summary>
            /// Create an instance.
            /// </summary>
            /// <param name="syncTask">The task to be executed.</param>
            /// <param name="trigger">Trigger which execute the task.</param>
            public QueuedTask(SyncTask syncTask, ExecutionTrigger trigger)
            {
                SyncTask = syncTask;
                ExecutionTrigger = trigger;
            }
        }
    }
}