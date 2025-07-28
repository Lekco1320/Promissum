using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// NewSyncProjectDialog.xaml 的交互逻辑
    /// </summary>
    public partial class NewSyncProjectDialog : CustomWindow, INotifyPropertyChanged
    {
        public string ProjectPath { get; set; }

        public string ProjectName { get; set; }

        public bool IsAutoLoad { get; set; }

        public bool IsOK { get; protected set; }

        public RelayCommand SelectPathCommand => new RelayCommand(SelectPath);

        public RelayCommand OKCommand => new RelayCommand(OK);

        public RelayCommand CancelCommand => new RelayCommand(Cancel);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected NewSyncProjectDialog()
        {
            ProjectPath = ProjectName = "";
            IsAutoLoad = true;

            DataContext = this;
            InitializeComponent();
        }

        private void SelectPath()
        {
            var sfd = new SaveFileDialog()
            {
                AddExtension = true,
                FileName = "新建项目名",
                Filter = "Lekco Promissum 同步项目|*.prms",
                FilterIndex = 1,
                Title = "新建项目"
            };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProjectPath = sfd.FileName;
                ProjectName = Path.GetFileNameWithoutExtension(ProjectPath);
            }
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(ProjectPath))
            {
                DialogHelper.ShowError("项目路径非法：空值或不存在。");
                return false;
            }
            if (ProjectName == "")
            {
                DialogHelper.ShowError("项目名不得为空。");
                return false;
            }
            return true;
        }

        public void OK()
        {
            if (!Validate())
            {
                return;
            }
            IsOK = true;
            Close();
        }

        public void Cancel()
            => Close();

        public static bool NewProject([MaybeNullWhen(false)] out SyncProjectFile newProjectFile)
        {
            var dialog = new NewSyncProjectDialog();
            dialog.ShowDialog();
            newProjectFile = dialog.IsOK ? new SyncProjectFile(dialog.ProjectPath, dialog.ProjectName, dialog.IsAutoLoad)
                                         : null;
            return dialog.IsOK;
        }
    }
}
