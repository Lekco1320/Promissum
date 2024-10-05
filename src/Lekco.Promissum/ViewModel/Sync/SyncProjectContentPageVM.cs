using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.View.Sync;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using Lekco.Wpf.Utility.Navigation;
using System;
using System.Linq;
using System.Windows.Input;

namespace Lekco.Promissum.ViewModel.Sync
{
    public class SyncProjectContentPageVM : ViewModelBase
    {
        public SyncProject Project { get; }

        public bool IsAutoLoad { get; set; }

        public SyncTask? CurrentTask { get; set; }

        public NavigationService NavigationService { get; set; }

        public ICommand ChangeIsAutoLoadCommand => new RelayCommand(ChangeIsAutoLoad);

        public ICommand NewTaskCommand => new RelayCommand(NewSyncTask);

        public ICommand DeleteTaskCommand => new RelayCommand(DeleteTask);

        public ICommand RenameProjectCommand => new RelayCommand(RenameProject);

        public SyncProjectContentPageVM(SyncProject project)
        {
            Project = project;
            IsAutoLoad = Project.IsAutoLoad;

            NavigationService = new NavigationService();
            NavigationServiceManager.Register(Project, NavigationService);
            NavigationService.CurrentKeyChanged += (sender, e) => CurrentTask = (SyncTask?)e;

            var page = new SyncTaskNullPage();
            NavigationService.DefaultView = page;

            foreach (var task in Project.Tasks)
            {
                var vm = new SyncTaskBriefPageVM(task);
                NavigationService.Register(task, new SyncTaskBriefPage(vm), vm);
            }

            var firstKey = Project.Tasks.FirstOrDefault();
            if (firstKey != null)
            {
                NavigationService.Navigate(firstKey);
            }
        }

        private void ChangeIsAutoLoad()
        {
            if (Project.IsAutoLoad && !DialogHelper.ShowInformation("是否要关闭计划任务？"))
            {
                return;
            }
            
            Project.IsAutoLoad = !Project.IsAutoLoad;
            try
            {
                Project.SyncProjectFile.Save();
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError(ex.Message);
                Project.IsAutoLoad = !Project.IsAutoLoad;
                return;
            }

            if (Project.IsAutoLoad)
            {
                SyncEngine.LoadProject(Project);
            }
            else
            {
                SyncEngine.UnloadProject(Project);
            }
            IsAutoLoad = Project.IsAutoLoad;
        }

        private void NewSyncTask()
        {
            if (NewSyncTaskDialog.NewTask(Project, out var newTask))
            {
                Project.AddTask(newTask);
                var vm = new SyncTaskBriefPageVM(newTask);
                NavigationService.Register(newTask, new SyncTaskBriefPage(vm), vm);
            }
        }

        private void DeleteTask()
        {
            if (CurrentTask == null)
            {
                return;
            }
            if (DialogHelper.ShowWarning($"是否删除任务“{CurrentTask.Name}”？") &&
                Project.DeleteTask(CurrentTask))
            {
                SyncEngine.UnloadTask(CurrentTask);
                NavigationService.Unregister(CurrentTask);
            }
        }

        private void RenameProject()
        {
            if (InputDialog.ShowDialog(
                description: "项目名：",
                result: out string newName,
                title: "重命名项目",
                validator: s => !string.IsNullOrEmpty(s),
                messager: s => "项目名不得为空。"
            ))
            {
                Project.RenameProject(newName);
            }
        }
    }
}