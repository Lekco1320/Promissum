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
    public sealed class SyncEngine
    {
        public static Dictionary<SyncTask, SyncProject> ConnectDiskTasks { get; }
        public static Dictionary<SyncTask, SyncProject> PeriodicTasks { get; }
        public static Dictionary<SyncTask, SyncProject> IntervalTasks { get; }
        public static Dictionary<string, SyncProject> OpenProjectDictionary { get; }
        public static ConcurrentQueue<LoadedTask> QueuedTasks { get; }
        public static HashSet<SyncTask> TasksInQueue { get; }
        public static HashSet<SyncProject> AutoRunProjects { get; }
        public static Timer PeriodicTimer { get; }
        public static Timer IntervalTimer { get; }
        public static bool IsRunning { get => _executionThread != null; }
        public static SyncEngine Instance { get => _instance; }

        public static event EventHandler? LoadedChanged;
        public static event EventHandler? QueuedChanged;

        private static Thread? _executionThread;
        private static readonly object _runningLock;
        private static readonly SyncEngine _instance;
        private static readonly ManagementEventWatcher _watcher;

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

        public static void LoadAutoRunProject(SyncProject project)
        {
            if (project.AutoRun && project.Tasks != null)
            {
                Config.AutoRunProjects.Add(project.FileName);
                foreach (var task in project.Tasks)
                {
                    LoadSpyedTask(project, task);
                }
                ExecuteQueuedTasks();
                Config.SaveAsFile();
                LoadedChanged?.Invoke(null, new EventArgs());
            }
        }

        private static void LoadSpyedTask(SyncProject parentProject, SyncTask task)
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

        private static void EnqueueTask(SyncProject parentProject, SyncTask task, ExecutionTrigger trigger)
        {
            QueuedTasks.Enqueue(new LoadedTask(parentProject, task, trigger));
            QueuedChanged?.Invoke(null, new EventArgs());
        }

        public static bool TryExecuteTask(SyncProject parentProject, SyncTask task, ExecutionTrigger trigger)
        {
            if (!TasksInQueue.Add(task))
            {
                return false;
            }
            EnqueueTask(parentProject, task, trigger);
            ExecuteQueuedTasks();
            return true;
        }

        private static void TriggerIntervalTasks(object? obj)
        {
            foreach (var pair in IntervalTasks)
            {
                if (DateTime.Now - pair.Key.LastExecuteTime > pair.Key.SyncPlan.IntervalSpan)
                {
                    if (TasksInQueue.Add(pair.Key))
                    {
                        EnqueueTask(pair.Value, pair.Key, ExecutionTrigger.Interval);
                    }
                }
            }
            ExecuteQueuedTasks();
        }

        private static void TriggerDriveTasks(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
            {
                foreach (var pair in ConnectDiskTasks)
                {
                    if (pair.Key.TryMatchPaths())
                    {
                        EnqueueTask(pair.Value, pair.Key, ExecutionTrigger.ConnectDisk);
                    }
                }
            }
            ExecuteQueuedTasks();
        }

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
                    EnqueueTask(project, task, ExecutionTrigger.Period);
                }
            }
        }

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
                        QueuedChanged?.Invoke(null, new EventArgs());
                    }
                }
                _executionThread = null;
            });
            _executionThread.SetApartmentState(ApartmentState.STA);
            _executionThread.Start();
        }

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

        public static void UnloadSpyedTask(SyncTask task)
        {
            IntervalTasks.Remove(task);
            ConnectDiskTasks.Remove(task);
            LoadedChanged?.Invoke(null, new EventArgs());
        }

        public static void UnloadAutoRunProject(SyncProject project)
        {
            if (!project.AutoRun && project.Tasks != null)
            {
                Config.AutoRunProjects.Remove(project.FileName);
                foreach (var task in project.Tasks)
                {
                    UnloadSpyedTask(task);
                }
                Config.SaveAsFile();
                LoadedChanged?.Invoke(null, new EventArgs());
            }
        }

        public static void CloseProject(SyncProject project)
        {
            if (!AutoRunProjects.Contains(project))
            {
                OpenProjectDictionary.Remove(project.FileName);
            }
        }

        private static int RemainSecondsOfToday()
        {
            var now = DateTime.Now;
            return 3600 * 24 - now.Hour * 3600 - now.Minute * 60 - now.Second;
        }

        public static void Dispose()
        {
            IntervalTimer.Dispose();
            _watcher.Dispose();
            _executionThread?.Join();
        }
    }
}