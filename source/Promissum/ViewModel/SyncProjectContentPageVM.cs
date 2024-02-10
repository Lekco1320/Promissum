using Lekco.Promissum.Apps;
using Lekco.Promissum.Sync;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;

namespace Lekco.Promissum.ViewModel
{
    public class SyncProjectContentPageVM : BindableBase
    {
        public TaskPage? ThisPage
        {
            get => _thisPage;
            set
            {
                _thisPage = value;
                RaisePropertyChanged(nameof(ThisPage));
            }
        }
        private TaskPage? _thisPage;

        public SyncTask? ThisTask
        {
            get => _thisTask;
            set
            {
                _thisTask = value;
                ThisPage = _thisTask == null ? null : PagePairs[_thisTask];
                RaisePropertyChanged(nameof(ThisTask));
            }
        }
        private SyncTask? _thisTask;

        public SyncProject Project { get; set; }
        public Dictionary<SyncTask, TaskPage> PagePairs { get; set; }

        public DelegateCommand ChangeAutoRunCommand { get; set; }
        public DelegateCommand NewTaskCommand { get; set; }
        public DelegateCommand DeleteTaskCommand { get; set; }
        public DelegateCommand ProjectSettingCommand { get; set; }

        public SyncProjectContentPageVM(SyncProject project)
        {
            Project = project;
            PagePairs = new Dictionary<SyncTask, TaskPage>();

            foreach (var task in Project.Tasks)
            {
                PagePairs.Add(task, new TaskPage(project, task));
            }
            ThisTask = Project.Tasks.Count > 0 ? Project.Tasks[0] : null;

            ChangeAutoRunCommand = new DelegateCommand(ChangeAutoRun);
            NewTaskCommand = new DelegateCommand(NewTask);
            DeleteTaskCommand = new DelegateCommand(DeleteTask);
            ProjectSettingCommand = new DelegateCommand(ProjectSetting);
        }

        private void ChangeAutoRun()
        {
            SyncEngine.LoadAutoRunProject(Project);
            SyncEngine.UnloadAutoRunProject(Project);
            try
            {
                Project.Save();
            }
            catch (IOException)
            {
                MessageWindow.ShowDialog(
                    message: $"项目“{Project.Name}”保存失败：文件正被占用。",
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
        }

        private void NewTask()
        {
            SyncTask? task = NewTaskWindow.NewTask();
            if (task != null)
            {
                Project.Tasks.Add(task);
                PagePairs.Add(task, new TaskPage(Project, task));
                ThisTask = Project.Tasks[^1];
            }
        }

        private void DeleteTask()
        {
            if (ThisTask == null)
            {
                return;
            }
            if (MessageWindow.ShowDialog(
                    message: $"是否删除任务“{ThisTask.Name}”？",
                    icon: MessageWindowIcon.Warning,
                    buttonStyle: MessageWindowButtonStyle.OKCancel
                ))
            {
                Project.DeleteTask(ThisTask);
                ThisTask = Project.Tasks.Count > 0 ? Project.Tasks[0] : null;
            }
        }

        private void ProjectSetting()
        {
            SyncProjectWindow.NewOrSetSyncProject(Project);
        }
    }
}