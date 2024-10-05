using Lekco.Promissum.ViewModel.Sync;
using Lekco.Wpf.Control;
using System;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncEngineWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncEngineWindow : CustomWindow
    {
        private static SyncEngineWindow? _instance;

        protected SyncEngineWindow()
        {
            InitializeComponent();
            var vm = new SyncEngineWindowVM();

            DataContext = vm;
        }

        protected override void OnClosed(EventArgs e)
        {
            _instance = null;
            base.OnClosed(e);
        }

        public static void ShowSyncEngineWindow()
        {
            if (_instance == null)
            {
                var window = new SyncEngineWindow();
                _instance = window;
                window.Show();
            }
            else
            {
                _instance.Show();
                _instance.Activate();
            }
        }
    }
}
