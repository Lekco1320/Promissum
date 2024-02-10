using Lekco.Promissum.Sync;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Forms;

namespace Lekco.Promissum.ViewModel
{
    public class NewTaskWindowVM : BindableBase
    {
        public string Name { get; set; }
        public SyncPath? OriginPath
        {
            get => _originPath;
            set
            {
                _originPath = value;
                RaisePropertyChanged(nameof(OriginPath));
            }
        }
        private SyncPath? _originPath;
        public SyncPath? DestinationPath
        {
            get => _destinationPath;
            set
            {
                _destinationPath = value;
                RaisePropertyChanged(nameof(DestinationPath));
            }
        }
        private SyncPath? _destinationPath;
        public bool IsByTimeChecked { get; set; }
        public bool IsBySizeChecked { get; set; }
        public bool IsByMD5Checked { get; set; }
        public SyncTask? NewTask { get; set; }

        public DelegateCommand SetOriginPathCommand { get; set; }
        public DelegateCommand SetDestinationPathCommand { get; set; }
        public DelegateCommand<Window> OKCommand { get; set; }
        public DelegateCommand<Window> CancelCommand { get; set; }

        public NewTaskWindowVM()
        {
            Name = "";
            IsByTimeChecked = true;

            SetOriginPathCommand = new DelegateCommand(SetOriginPath);
            SetDestinationPathCommand = new DelegateCommand(SetDestinationPath);
            OKCommand = new DelegateCommand<Window>(OK);
            CancelCommand = new DelegateCommand<Window>(Cancel);
        }

        private void SetOriginPath()
        {
            var browserDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "选择备份任务的目标文件夹"
            };
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                OriginPath = new SyncPath(browserDialog.SelectedPath);
            }
        }

        private void SetDestinationPath()
        {
            var browserDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                UseDescriptionForTitle = true,
                Description = "选择备份任务的目标文件夹"
            };
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                DestinationPath = new SyncPath(browserDialog.SelectedPath);
            }
        }

        private void OK(Window window)
        {
            if (OriginPath == null || DestinationPath == null)
            {
                MessageWindow.ShowDialog(
                    message: "目录不得为空。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
                return;
            }

            if (SyncPath.IsSubPath(OriginPath, DestinationPath))
            {
                MessageWindow.ShowDialog(
                    message: "目标目录不得为源目录或其子目录。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
                return;
            }

            var info = DestinationPath.GetPathInfo();
            if ((info.GetFiles().Length == 0 && info.GetDirectories().Length == 0) ||
                ((info.GetFiles().Length > 0 || info.GetDirectories().Length > 0) &&
                MessageWindow.ShowDialog(
                    message: "目标目录中含有文件或文件夹，备份时可能会删除这些文件，建议您选择空目录作为目标目录。是否继续？",
                    icon: MessageWindowIcon.Warning,
                    buttonStyle: MessageWindowButtonStyle.OKCancel
                )))
            {
                NewTask = new SyncTask(Name, OriginPath, DestinationPath);
                if (IsByTimeChecked)
                {
                    NewTask.CompareMode = CompareMode.ByTime;
                }
                else if (IsBySizeChecked)
                {
                    NewTask.CompareMode = CompareMode.BySize;
                }
                else if (IsByMD5Checked)
                {
                    NewTask.CompareMode = CompareMode.ByMD5;
                }
                window.Close();
            }
        }

        private void Cancel(Window window)
        {
            window.Close();
        }
    }
}