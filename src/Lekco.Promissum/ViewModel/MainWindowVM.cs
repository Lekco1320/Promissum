using Lekco.Promissum.App;
using Lekco.Promissum.Control;
using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Disk;
using Lekco.Promissum.View;
using Lekco.Promissum.View.Sync;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using Lekco.Wpf.Utility.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace Lekco.Promissum.ViewModel
{
    public class MainWindowVM : ViewModelBase
    {
        public HashSet<SyncProject> OpenProjects { get; protected set; }

        public NavigationService NavigationService { get; }

        public SyncProject? CurrentProject { get; protected set; }

        public RelayCommand NewProjectCommand => new RelayCommand(NewProject);

        public RelayCommand OpenProjectCommand => new RelayCommand(OpenProject);

        public RelayCommand ExplorerCommand => new RelayCommand(OpenExplorer);

        public RelayCommand SaveCommand => new RelayCommand(SaveProject);

        public RelayCommand RenameProjectCommand => new RelayCommand(RenameProject);

        public MainWindowVM()
        {
            OpenProjects = new HashSet<SyncProject>();
            NavigationService = new NavigationService();
            NavigationService.NavigationKeyUnregistered += RemoveProject;
            NavigationService.CurrentKeyChanged += (sender, e) => CurrentProject = (SyncProject)e;
            NavigationServiceManager.Register("Mainwindow", NavigationService);
        }

        private void SaveProject()
        {
            if (CurrentProject == null)
            {
                return;
            }

            try
            {
                CurrentProject.SyncProjectFile.Save();
                DialogHelper.ShowSuccess($"项目“{CurrentProject.Name}”保存成功。");
            }
            catch (Exception ex)
            {
                DialogHelper.ShowError(ex);
            }
        }

        private void RenameProject()
        {
            if (CurrentProject != null && InputDialog.ShowDialog(
                description: "项目名：",
                result: out string newName,
                title: "重命名项目",
                validator: s => !string.IsNullOrEmpty(s),
                messager: s => "项目名不得为空。"
            ))
            {
                CurrentProject.RenameProject(newName);
            }
        }

        private void NewProject()
        {
            if (NewSyncProjectDialog.NewProject(out var newProjectFile))
            {
                var newProject = newProjectFile.SyncProject;
                SyncEngine.LoadProject(newProject);
                OpenProjects.Add(newProject);
                if (newProject.IsAutoLoad)
                {
                    SyncEngine.LoadProject(newProject);
                }
                NavigationService.Register(newProject, new SyncProjectContentPage(newProject));
            }
        }

        public void OpenProject()
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Lekco Promissum 同步项目|*.prms",
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
            AccessedFileManager.AddAccessedFile(projectPath);
            if (OpenProjects.Any(proj => proj.ProjectFileName == projectPath))
            {
                var project = OpenProjects.Select(proj => proj.ProjectFileName == projectPath);
                NavigationService.Navigate(project);
            }
            else
            {
                try
                {
                    var project = SyncEngine.OpenProject(projectPath);
                    SyncEngine.LoadProject(project);
                    OpenProjects.Add(project);
                    NavigationService.Register(project, new SyncProjectContentPage(project));
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowError(message: $"路径为“{projectPath}”的项目文件受损，加载失败：{ex.Message}。");
                }
            }
        }

        private void RemoveProject(object? sender, NavigationData e)
        {
            if (e != null)
            {
                OpenProjects.Remove((SyncProject)e.Key);
            }
        }

        private void OpenExplorer()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var directoryInfo = new DirectoryInfo(path);
            var driveInfo = new DriveInfo(directoryInfo.FullName[..2]);
            var drive = new DiskDrive(driveInfo);
            var directory = new DiskDirectory(directoryInfo);
            new ExplorerWindow(drive, directory).Show();
        }
    }
}
