using Lekco.Promissum.View;
using Prism.Commands;
using System.Windows.Controls;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// NotifyIcon.xaml 的交互逻辑
    /// </summary>
    public partial class NotifyIcon : UserControl
    {
        private readonly App _app;
        public DelegateCommand LeftClickCommand { get; set; }
        public DelegateCommand ShowSyncEngineWindowCommand { get; set; }
        public DelegateCommand QuitCommand { get; set; }
        public NotifyIcon(App app)
        {
            InitializeComponent();

            _app = app;
            LeftClickCommand = new DelegateCommand(_app.ShowMainWindow);
            ShowSyncEngineWindowCommand = new DelegateCommand(SyncEngineWindow.ShowSyncEngineWindow);
            QuitCommand = new DelegateCommand(App.Quit);
            DataContext = this;
        }
    }
}
