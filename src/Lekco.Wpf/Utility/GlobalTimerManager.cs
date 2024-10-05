using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Lekco.Wpf.Utility
{
    public static class GlobalTimerManager
    {
        private static Thread? _timerThread;

        private static bool stopRequested;

        private static readonly object _lock = new object();

        private static readonly TimeSpan IntervalMilliseconds = TimeSpan.FromMilliseconds(0.5);

        private static readonly ConcurrentDictionary<TimerTask, TaskRunInfo> TimerTasks = new ConcurrentDictionary<TimerTask, TaskRunInfo>();

        static GlobalTimerManager()
        {
            StartUp();
        }

        public static void StartUp()
        {
            lock (_lock)
            {
                if (!stopRequested)
                {
                    _timerThread = new Thread(Run) { IsBackground = true };
                    _timerThread.Start();
                    stopRequested = false;
                }
            }
        }

        private static void Run()
        {
            while (!stopRequested)
            {
                var now = DateTime.Now;
                foreach (var pair in TimerTasks)
                {
                    var task = pair.Key;
                    var info = pair.Value;
                    if (!info.HasDued && (now - info.LastRanTime).Milliseconds >= task.DueTime)
                    {
                        RunTask(task, info);
                    }
                    if (info.HasDued && (now - info.LastRanTime).Milliseconds >= task.Period)
                    {
                        RunTask(task, info);
                    }                    
                }
                Thread.Sleep(IntervalMilliseconds);
            }
        }

        private static void RunTask(TimerTask task, TaskRunInfo taskInfo)
        {
            var thread = new Thread(task.Callback);
            thread.Start();
            taskInfo.LastRanTime = DateTime.Now;
        }

        public static void Stop()
        {
            stopRequested = true;
        }

        public static void AddTask(TimerTask task)
        {
            TimerTasks.TryAdd(task, new TaskRunInfo());
        }

        public static void RemoveTask(TimerTask task)
        {
            TimerTasks.TryRemove(task, out var _);
        }

        protected class TaskRunInfo
        {
            public bool HasDued { get; set; }

            public DateTime LastRanTime { get; set; }
        }
    }

    public class TimerTask
    {
        public ThreadStart Callback { get; }

        public int DueTime { get; }

        public int Period { get; }

        public TimerTask(ThreadStart callback, int dueTime, int period)
        {
            Callback = callback;
            DueTime = dueTime;
            Period = period;
        }
    }
}
