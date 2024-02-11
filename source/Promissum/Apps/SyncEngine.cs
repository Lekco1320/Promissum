using Lekco.Promissum.Sync;
using Lekco.Promissum.View;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Threading;

namespace Lekco.Promissum.Apps
{
    /// <summary>
    /// The core engine of Lekco Promissum for managing projects, executing tasks and automatic actions. 
    /// This is a singleton class.
    /// </summary>
    public sealed class SyncEngine
    {
        /// <summary>
        /// The dictionary storing tasks whose execution is triggered by disk connection and their parent projects.
        /// </summary>
        public static Dictionary<SyncTask, SyncProject> ConnectDiskTasks { get; }

        /// <summary>
        /// The dictionary storing tasks with periodic plans and theirs parent projects.
        /// </summary>
        public static Dictionary<SyncTask, SyncProject> PeriodicTasks { get; }

        /// <summary>
        /// The dictionary storing interval tasks and their parent projects.
        /// </summary>
        public static Dictionary<SyncTask, SyncProject> IntervalTasks { get; }

        /// <summary>
        /// The dictionary storing pairs of projects and their files' name.
        /// </summary>
        public static Dictionary<string, SyncProject> OpenProjectDictionary { get; }

        /// <summary>
        /// The queue storing executing tasks.
        /// </summary>
        public static ConcurrentQueue<LoadedTask> QueuedTasks { get; }

        /// <summary>
        /// The set storing tasks in the <see cref="QueuedTasks"/>.
        /// </summary>
        public static HashSet<SyncTask> TasksInQueue { get; }

        /// <summary>
        /// The set storing auto-run projects.
        /// </summary>
        public static HashSet<SyncProject> AutoRunProjects { get; }

        /// <summary>
        /// The timer used to trigger periodic tasks.
        /// </summary>
        public static Timer PeriodicTimer { get; }

        /// <summary>
        /// The timer used to trigger interval tasks.
        /// </summary>
        public static Timer IntervalTimer { get; }

        /// <summary>
        /// Specifies whether the engine is executing tasks.
        /// </summary>
        public static bool IsRunning { get => _executionThread != null; }

        /// <summary>
        /// The only instance of this type.
        /// </summary>
        public static SyncEngine Instance { get => _instance; }

        /// <summary>
        /// Occurs when loaded projects changed.
        /// </summary>
        public static event EventHandler? LoadedProjectsChanged;

        /// <summary>
        /// Occurs when queued tasks changed.
        /// </summary>
        public static event EventHandler? QueuedTasksChanged;

        private static Thread? _executionThread;
        private static readonly object _runningLock;
        private static readonly SyncEngine _instance;
        private static readonly ManagementEventWatcher _watcher;

        /// <summary>
        /// The static constructor of this type.
        /// </summary>
        static SyncEngine()
        {
            ConnectDiskTasks = new Dictionary<SyncTask, SyncProject>();
            PeriodicTasks = new Dictionary<SyncTask, SyncProject>();
            IntervalTasks = new Dictionary<SyncTask, SyncProject>();
            QueuedTasks = new ConcurrentQueue<LoadedTask>();
            TasksInQueue = new HashSet<SyncTask>();
            AutoRunProjects = new HashSet<SyncProject>();
            OpenProjectDictionary = new Dictionary<string, SyncProject>();
            _runningLock = new object();
            _instance = new SyncEngine();

            var query = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), @"TargetInstance ISA 'Win32_LogicalDisk'");
            var opt = new ConnectionOptions()
            {
                EnablePrivileges = true,
                Authority = null,
                Authentication = AuthenticationLevel.Default
            };
            var scope = new ManagementScope(@"\root\CIMV2", opt);
            _watcher = new ManagementEventWatcher(scope, query);
            _watcher.EventArrived += TriggerDriveTasks;
            _watcher.Start();

            PeriodicTimer = new Timer(new TimerCallback(TriggerPeriodicTasks), null, RemainSecondsOfToday() * 1000, Timeout.Infinite);
            IntervalTimer = new Timer(new TimerCallback(TriggerIntervalTasks), null, 0, 30 * 1000);
        }

        private SyncEngine()
        {
        }

        /// <summary>
        /// Load auto-run projects from <see cref="Config"/>.
        /// </summary>
        public static void LoadAutoRunProjects()
        {
            foreach (var path in Config.AutoRunProjects)
            {
                if (File.Exists(path))
                {
                    var project = SyncProject.ReadFromFile(path);
                    if (project.AutoRun)
                    {
                        OpenProjectDictionary.Add(path, project);
                        AutoRunProjects.Add(project);
                    }
                    else
                    {
                        Config.AutoRunProjects.Remove(path);
                        Config.SaveAsFile();
                    }
                }
                else
                {
                    MessageWindow.Show(
                        message: $"项目路径“{path}”不存在，备份计划执行失败。此消息将不再提醒。",
                        icon: MessageWindowIcon.Warning,
                        buttonStyle: MessageWindowButtonStyle.OK,
                        location: MessageWindowLocation.RightBottom
                    );
                    Config.AutoRunProjects.Remove(path);
                    Config.SaveAsFile();
                }
            }

            foreach (var project in AutoRunProjects)
            {
                LoadAutoRunProject(project);
            }
        }

        /// <summary>
        /// Checks and loads a specified auto-run project.
        /// </summary>
        /// <param name="project">Specified project.</param>
        public static void LoadAutoRunProject(SyncProject project)
        {
            if (project.AutoRun && project.Tasks != null)
            {
                Config.AutoRunProjects.Add(project.FileName);
                foreach (var task in project.Tasks)
                {
                    LoadPlannedTask(project, task);
                }
                ExecuteQueuedTasks();
                Config.SaveAsFile();
                LoadedProjectsChanged?.Invoke(null, new EventArgs());
            }
        }

        /// <summary>
        /// Checks and loads a specified planned task.
        /// </summary>
        /// <param name="parentProject">The parent project of the task.</param>
        /// <param name="task">The sub task of the project.</param>
        private static void LoadPlannedTask(SyncProject parentProject, SyncTask task)
        {
            if (!task.SyncPlan.UsePlan)
            {
                return;
            }
            if (task.SyncPlan.PeriodicSync)
            {
                PeriodicTasks.TryAdd(task, parentProject);
            }
            if (task.SyncPlan.IntervalSync && !IntervalTasks.ContainsKey(task))
            {
                IntervalTasks.TryAdd(task, parentProject);
            }
            if (task.SyncPlan.WhenConnectDisk && !ConnectDiskTasks.ContainsKey(task))
            {
                ConnectDiskTasks.TryAdd(task, parentProject);
            }
            TriggerPeriodicTasks(null);
            ExecuteQueuedTasks();
        }

        /// <summary>
        /// Enqueue a task into the executing queue.
        /// </summary>
        /// <param name="parentProject">The parent project of the task.</param>
        /// <param name="task">The sub task of the project.</param>
        /// <param name="trigger">The execution trigger of the task.</param>
        /// <returns><see langword="true"/> if adds it into executing queue successfully; otherwise, <see langword="false"/>.</returns>
        public static bool TryExecuteTask(SyncProject parentProject, SyncTask task, ExecutionTrigger trigger)
        {
            if (!TasksInQueue.Add(task))
            {
                QueuedTasks.Enqueue(new LoadedTask(parentProject, task, trigger));
                QueuedTasksChanged?.Invoke(null, new EventArgs());
                ExecuteQueuedTasks();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Triggers all interval tasks.
        /// </summary>
        /// <param name="obj">The parameter.</param>
        private static void TriggerIntervalTasks(object? obj)
        {
            foreach (var pair in IntervalTasks)
            {
                if (DateTime.Now - pair.Key.LastExecuteTime > pair.Key.SyncPlan.IntervalSpan)
                {
                    if (TasksInQueue.Add(pair.Key))
                    {
                        TryExecuteTask(pair.Value, pair.Key, ExecutionTrigger.Interval);
                    }
                }
            }
        }

        /// <summary>
        /// Triggers tasks whose execution is triggered by disk connection.
        /// </summary>
        private static void TriggerDriveTasks(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
            {
                foreach (var pair in ConnectDiskTasks)
                {
                    if (pair.Key.TryMatchPaths())
                    {
                        TryExecuteTask(pair.Value, pair.Key, ExecutionTrigger.ConnectDisk);
                    }
                }
            }
            ExecuteQueuedTasks();
        }

        /// <summary>
        /// Triggers periodic tasks.
        /// </summary>
        /// <param name="obj">The parameter.</param>
        private static void TriggerPeriodicTasks(object? obj)
        {
            PeriodicTimer.Change(RemainSecondsOfToday() * 1000, Timeout.Infinite);
            foreach (var pair in PeriodicTasks)
            {
                var now = DateTime.Now;
                var task = pair.Key;
                var project = pair.Value;
                if (task.LastExecuteTime.Date != now.Date && (
                    (int)task.SyncPlan.SyncPeriod == (int)now.DayOfWeek ||
                    task.SyncPlan.SyncPeriod == SyncPeriod.Month && now.Day == 1 ||
                    task.SyncPlan.SyncPeriod == SyncPeriod.Quarter && now.Month % 3 == 1 && now.Day == 1 ||
                    task.SyncPlan.SyncPeriod == SyncPeriod.Year && now.Month == 1 && now.Day == 1))
                {
                    TryExecuteTask(project, task, ExecutionTrigger.Period);
                }
            }
        }

        /// <summary>
        /// Executes all tasks in the executing queue.
        /// </summary>
        private static void ExecuteQueuedTasks()
        {
            lock (_runningLock)
            {
                if (IsRunning)
                {
                    return;
                }
            }

            _executionThread = new Thread(() =>
            {
                while (!QueuedTasks.IsEmpty)
                {
                    if (QueuedTasks.TryDequeue(out var item))
                    {
                        ExecuteTask(item.ParentProject, item.Task, item.ExecutionTrigger);
                        QueuedTasksChanged?.Invoke(null, new EventArgs());
                    }
                }
                _executionThread = null;
            });
            _executionThread.SetApartmentState(ApartmentState.STA);
            _executionThread.Start();
        }

        /// <summary>
        /// Executes a specified task.
        /// </summary>
        /// <param name="parentProject">The parent project of the task.</param>
        /// <param name="task">The sub task of the project.</param>
        /// <param name="trigger">The execution trigger of the task.</param>
        private static void ExecuteTask(SyncProject parentProject, SyncTask task, ExecutionTrigger trigger)
        {
            var controller = new SyncController(task, trigger);
            SyncTaskExecutingWindow? window = null;
            var thread = new Thread(() =>
            {
                window = new SyncTaskExecutingWindow(controller);
                window.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            try
            {
                task.Execute(parentProject, trigger, controller);
            }
            catch (DirectoryNotFoundException)
            {
                MessageWindow.ShowDialog(
                    message: $"任务“{task.Name}”执行失败：路径非法或硬盘已弹出。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK,
                    location: MessageWindowLocation.RightBottom
                );
            }

            window?.Dispatcher.Invoke(window.Close);
            parentProject.Save();
            TasksInQueue.Remove(task);
        }

        /// <summary>
        /// Opens a project by given path.
        /// </summary>
        /// <param name="path">The file path of the project.</param>
        /// <returns>The project user wants to open.</returns>
        public static SyncProject OpenProject(string path)
        {
            if (OpenProjectDictionary.TryGetValue(path, out SyncProject? project))
            {
                return project;
            }
            project = SyncProject.ReadFromFile(path);
            OpenProjectDictionary.Add(path, project);
            LoadAutoRunProject(project);
            return project;
        }

        /// <summary>
        /// Unloads a specified planned task.
        /// </summary>
        /// <param name="task">The specified task.</param>
        public static void UnloadPlannedTask(SyncTask task)
        {
            IntervalTasks.Remove(task);
            ConnectDiskTasks.Remove(task);
            LoadedProjectsChanged?.Invoke(null, new EventArgs());
        }

        /// <summary>
        /// Unloads a specified auto-run project.
        /// </summary>
        /// <param name="project">The specified project.</param>
        public static void UnloadAutoRunProject(SyncProject project)
        {
            if (!project.AutoRun && project.Tasks != null)
            {
                Config.AutoRunProjects.Remove(project.FileName);
                foreach (var task in project.Tasks)
                {
                    UnloadPlannedTask(task);
                }
                Config.SaveAsFile();
                LoadedProjectsChanged?.Invoke(null, new EventArgs());
            }
        }

        /// <summary>
        /// Close an open project.
        /// </summary>
        /// <param name="project">The specified project.</param>
        public static void CloseProject(SyncProject project)
        {
            if (!AutoRunProjects.Contains(project))
            {
                OpenProjectDictionary.Remove(project.FileName);
            }
        }

        /// <summary>
        /// Calculates remain seconds of today.
        /// </summary>
        /// <returns>Remain seconds of today.</returns>
        private static int RemainSecondsOfToday()
        {
            var now = DateTime.Now;
            return 3600 * 24 - now.Hour * 3600 - now.Minute * 60 - now.Second;
        }

        /// <summary>
        /// Shuts down and releases resources used by engine.
        /// </summary>
        public static void Dispose()
        {
            IntervalTimer.Dispose();
            _watcher.Dispose();
            _executionThread?.Join();
        }
    }
}