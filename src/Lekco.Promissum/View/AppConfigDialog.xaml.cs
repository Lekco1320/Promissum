using Lekco.Promissum.App;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using System.Diagnostics;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// AppConfigDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AppConfigDialog : CustomWindow
    {
        public bool AutoStartUp { get; set; }

        public bool AlwaysNotifyWhenDeleteFiles { get; set; }

        public int DialogCountDownSeconds { get; set; }

        public int FileOperationMaxParallelCount { get; set; }

        public RelayCommand OKCommand => new RelayCommand(OK);

        public RelayCommand CancelCommand => new RelayCommand(Cancel);

        public AppConfigDialog()
        {
            AutoStartUp = Config.Instance.AutoStartUp;
            AlwaysNotifyWhenDeleteFiles = Config.Instance.AlwaysNotifyWhenDelete;
            DialogCountDownSeconds = Config.Instance.DialogCountDownSeconds;
            FileOperationMaxParallelCount = Config.Instance.FileOperationMaxParallelCount;

            InitializeComponent();
            DataContext = this;
        }

        protected void OK()
        {
            Config.Instance.AlwaysNotifyWhenDelete = AlwaysNotifyWhenDeleteFiles;
            Config.Instance.DialogCountDownSeconds = DialogCountDownSeconds;
            Config.Instance.FileOperationMaxParallelCount = FileOperationMaxParallelCount;
            if (Config.Instance.AutoStartUp != AutoStartUp)
            {
                SwitchAutoStart();
            }
            Config.SaveAsFile();
            Close();
        }

        protected void Cancel()
        {
            Close();
        }

        private void SwitchAutoStart()
        {
            var agens = new Process();
            agens.StartInfo.FileName = App.Promissum.AgensPath;
            agens.StartInfo.Arguments = AutoStartUp ? "-enableAutoStart" : "-disableAutoStart";
            agens.StartInfo.UseShellExecute = true;
            agens.StartInfo.Verb = "runas";
            agens.Start();
            agens.WaitForExit();
            if (agens.ExitCode == 0)
            {
                Config.Instance.AutoStartUp = AutoStartUp;
            }
            else
            {
                DialogHelper.ShowError("首选项中“开机自启动”项修改失败。");
            }
        }
    }
}
