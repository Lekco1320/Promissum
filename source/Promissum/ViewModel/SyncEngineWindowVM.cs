using Lekco.Promissum.Apps;
using Lekco.Promissum.Sync;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lekco.Promissum.ViewModel
{
    public class SyncEngineWindowVM : BindableBase
    {
        public static int LoadedProjects { get => SyncEngine.AutoRunProjects.Count; }
        public static int LoadedTasks { get => SyncEngine.ConnectDiskTasks.Count + SyncEngine.IntervalTasks.Count + SyncEngine.PeriodicTasks.Count; }
        public static int QueuedTasks { get => SyncEngine.QueuedTasks.Count; }
        public ObservableCollection<LoadedTask> LoadedList { get; set; }
        public ObservableCollection<LoadedTask> QueuedList { get; set; }
        public DelegateCommand<Window> CloseCommand { get; set; }

        public SyncEngineWindowVM()
        {
            LoadedList = new ObservableCollection<LoadedTask>();
            QueuedList = new ObservableCollection<LoadedTask>();

            CloseCommand = new DelegateCommand<Window>(window => window.Close());

            SyncEngine.LoadedChanged += SyncEngineLoadedChanged;
            SyncEngine.QueuedChanged += SyncEngineQueuedChanged;

            RefreshLoadedList();
            RefreshQueuedList();
        }

        private void SyncEngineLoadedChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(LoadedProjects));
            RaisePropertyChanged(nameof(LoadedTasks));
            Application.Current.Dispatcher.Invoke(RefreshLoadedList);
        }

        private void SyncEngineQueuedChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(QueuedTasks));
            Application.Current.Dispatcher.Invoke(RefreshQueuedList);
        }

        private void RefreshLoadedList()
        {
            LoadedList.Clear();
            foreach (var item in SyncEngine.ConnectDiskTasks)
            {
                LoadedList.Add(new LoadedTask(item.Value, item.Key, ExecutionTrigger.ConnectDisk));
            }
            foreach (var item in SyncEngine.PeriodicTasks)
            {
                LoadedList.Add(new LoadedTask(item.Value, item.Key, ExecutionTrigger.Period));
            }
            foreach (var item in SyncEngine.IntervalTasks)
            {
                LoadedList.Add(new LoadedTask(item.Value, item.Key, ExecutionTrigger.Interval));
            }
        }

        private void RefreshQueuedList()
        {
            QueuedList.Clear();
            foreach (var item in SyncEngine.QueuedTasks)
            {
                QueuedList.Add(item);
            }
        }
    }
}
