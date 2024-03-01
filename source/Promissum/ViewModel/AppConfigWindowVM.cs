using Lekco.Promissum.Apps;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System.Diagnostics;
using System.Windows;

namespace Lekco.Promissum.ViewModel
{
    public class AppConfigWindowVM : BindableBase
    {
        public bool AutoStartUp { get; set; }
        public bool AlwaysTellsMeWhenDeleteFiles { get; set; }
        public bool TryExecuteRepeatedlyAfterFail { get; set; }
        public int MessageWindowWaitingSeconds { get; set; }
        public int MaxParallelCopyCounts { get; set; }

        public DelegateCommand<Window> OKCommand { get; set; }
        public DelegateCommand<Window> CancelCommand { get; set; }

        public AppConfigWindowVM()
        {
            AutoStartUp = Config.AutoStartUp;
            AlwaysTellsMeWhenDeleteFiles = Config.AlwaysTellsMeWhenDeleteFiles;
            TryExecuteRepeatedlyAfterFail = Config.TryExecuteRepeatedlyAfterFail;
            MessageWindowWaitingSeconds = Config.MessageWindowWaitingSeconds;
            MaxParallelCopyCounts = Config.MaxParallelCopyCounts;

            OKCommand = new DelegateCommand<Window>(window => OK(window));
            CancelCommand = new DelegateCommand<Window>(window => window.Close());
        }

        private void OK(Window window)
        {
            if (AutoStartUp != Config.AutoStartUp)
            {
                SwitchAutoStart();
            }
            Config.AlwaysTellsMeWhenDeleteFiles = AlwaysTellsMeWhenDeleteFiles;
            Config.TryExecuteRepeatedlyAfterFail = TryExecuteRepeatedlyAfterFail;
            Config.MessageWindowWaitingSeconds = MessageWindowWaitingSeconds;
            Config.MaxParallelCopyCounts = MaxParallelCopyCounts;
            Config.SaveAsFile();
            window.Close();
        }

        private void SwitchAutoStart()
        {
            var agens = new Process();
            agens.StartInfo.FileName = App.AgensPath;
            agens.StartInfo.Arguments = AutoStartUp ? "-enableAutoStart" : "-disableAutoStart";
            agens.StartInfo.UseShellExecute = true;
            agens.StartInfo.Verb = "runas";
            agens.Start();
            agens.WaitForExit();
            if (agens.ExitCode == 0)
            {
                Config.AutoStartUp = AutoStartUp;
                Config.SaveAsFile();
            }
            else
            {
                MessageWindow.ShowDialog(
                    message: "首选项中“开机自启动”项修改失败。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
        }
    }
}
