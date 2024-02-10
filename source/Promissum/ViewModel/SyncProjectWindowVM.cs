using Lekco.Promissum.Apps;
using Lekco.Promissum.Sync;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Lekco.Promissum.ViewModel
{
    public class SyncProjectWindowVM : BindableBase
    {
        public SyncProject? Project { get; set; }
        public string Title { get; set; }
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                _projectPath = value;
                RaisePropertyChanged(nameof(ProjectPath));
            }
        }
        private string _projectPath;
        public string ProjectName { get; set; }
        public bool AutoRun { get; set; }
        public bool NewProject { get; protected set; }

        public DelegateCommand SetProjectPathCommand { get; set; }
        public DelegateCommand<Window> OKCommand { get; set; }
        public DelegateCommand<Window> CancelCommand { get; set; }

        public SyncProjectWindowVM(SyncProject? project)
        {
            NewProject = project == null;
            Title = NewProject ? "新建项目" : "项目设置";
            Project = project;
            
            if (NewProject)
            {
                _projectPath = "";
                ProjectName = "";
                AutoRun = true;
            }
            else
            {
                _projectPath = Project!.FileName;
                ProjectName = Project!.Name;
                AutoRun = Project!.AutoRun;
            }

            SetProjectPathCommand = new DelegateCommand(SetProjectPath);
            OKCommand = new DelegateCommand<Window>(window => OK(window));
            CancelCommand = new DelegateCommand<Window>(window => Cancel(window));
        }

        private void SetProjectPath()
        {
            var sfd = new SaveFileDialog()
            {
                AddExtension = true,
                FileName = "新建项目名",
                Filter = "Lekco Promissum 备份项目|*.prms",
                FilterIndex = 1,
                Title = "新建项目"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ProjectPath = sfd.FileName;
            }
        }

        private void OK(Window window)
        {
            if (ProjectPath == "")
            {
                MessageWindow.ShowDialog(
                    message: "项目位置不得为空",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
                return;
            }
            if (ProjectName == "")
            {
                MessageWindow.ShowDialog(
                    message: "项目名不得为空",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
                return;
            }
            if (NewProject)
            {
                Project = new SyncProject(ProjectName, ProjectPath) { AutoRun = AutoRun };
                Project.Create();
            }
            else
            {
                Project!.Name = ProjectName;
                Project!.AutoRun = AutoRun;
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
            window.Close();
        }

        private static void Cancel(Window window)
        {
            window.Close();
        }
    }
}
