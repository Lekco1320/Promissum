using Lekco.Promissum.Apps;
using Lekco.Promissum.Sync;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Lekco.Promissum.ViewModel
{
    public class MainWindowVM : BindableBase
    {
        public SyncProject? ThisProject
        {
            get => _thisProject;
            set
            {
                _thisProject = value;
                ThisPage = _thisProject == null ? StartPage : PagePairs[_thisProject];
            }
        }
        private SyncProject? _thisProject;

        public Page ThisPage
        {
            get => _thisPage;
            set
            {
                _thisPage = value;
                RaisePropertyChanged(nameof(ThisPage));
            }
        }
        private Page _thisPage;

        public Dictionary<SyncProject, SyncProjectContentPage> PagePairs { get; set; }
        public ObservableCollection<SyncProject> OpenProjects { get; set; }
        public Page StartPage { get; set; }

        public DelegateCommand NewProjectCommand { get; set; }
        public DelegateCommand OpenProjectCommand { get; set; }
        public DelegateCommand QuitCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand<Window> CloseWindowCommand { get; set; }
        public DelegateCommand<Window> MinimizeWindowCommand { get; set; }
        public DelegateCommand<SyncProject> RemovePageCommand { get; set; }
        public DelegateCommand ProjectSettingCommand { get; set; }
        public DelegateCommand ShowSyncEngineWindowCommand { get; set; }
        public DelegateCommand SetAppConfigCommand { get; set; }
        public DelegateCommand ShowAboutWindowCommand { get; set; }

        public MainWindowVM()
        {
            OpenProjects = new ObservableCollection<SyncProject>();
            PagePairs = new Dictionary<SyncProject, SyncProjectContentPage>();
            _thisPage = StartPage = new StartPage();

            NewProjectCommand = new DelegateCommand(NewProject);
            OpenProjectCommand = new DelegateCommand(OpenProject);
            QuitCommand = new DelegateCommand(App.Quit);
            RemovePageCommand = new DelegateCommand<SyncProject>(proj => RemoveProject(proj));
            SaveCommand = new DelegateCommand(SaveProject);
            ProjectSettingCommand = new DelegateCommand(ProjectSetting);
            ShowSyncEngineWindowCommand = new DelegateCommand(SyncEngineWindow.ShowSyncEngineWindow);
            SetAppConfigCommand = new DelegateCommand(AppConfigWindow.SetAppConfig);
            ShowAboutWindowCommand = new DelegateCommand(AboutWindow.ShowAbout);
            CloseWindowCommand = new DelegateCommand<Window>(window => window.Close());
            MinimizeWindowCommand = new DelegateCommand<Window>(window => window.WindowState = WindowState.Minimized);
        }

        private void SaveProject()
        {
            try
            {
                ThisProject?.Save();
            }
            catch (IOException)
            {
                MessageWindow.ShowDialog(
                    message: $"项目“{ThisProject?.Name}”保存失败：文件正被占用。",
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

        private void RemoveProject(SyncProject project)
        {
            PagePairs.Remove(project);
            SyncEngine.CloseProject(project);
        }

        private void ProjectSetting()
        {
            if (ThisProject != null)
            {
                SyncProjectWindow.NewOrSetSyncProject(ThisProject);
            }
        }

        private void NewProject()
        {
            SyncProject? newProject = SyncProjectWindow.NewOrSetSyncProject(null);
            if (newProject != null)
            {
                SyncEngine.LoadAutoRunProject(newProject);
                PagePairs.Add(newProject, new SyncProjectContentPage(newProject));
                OpenProjects.Add(newProject);
                ThisProject = newProject;
            }
        }

        public void OpenProject()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Lekco Promissum 备份项目|*.prms",
                FilterIndex = 1,
                Title = "打开项目",
                CheckFileExists = true,
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenProject(openFileDialog.FileName);
            }
        }

        public void OpenProject(string projectPath)
        {
            var project = SyncEngine.OpenProject(projectPath);
            if (!OpenProjects.Any(proj => proj.FileName == project.FileName))
            {
                PagePairs.Add(project, new SyncProjectContentPage(project));
                OpenProjects.Add(project);
            }
            ThisProject = project;
        }
    }
}
