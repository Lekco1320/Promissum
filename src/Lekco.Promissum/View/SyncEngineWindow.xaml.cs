using Lekco.Promissum.ViewModel.Sync;
using Lekco.Wpf.Control;
using System;
using System.Windows;

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

        public static new void Show()
        {
            if (_instance == null)
            {
                var window = new SyncEngineWindow();
                _instance = window;
                ((Window)window).Show();
            }
            else
            {
                ((Window)_instance).Show();
                _instance.Activate();
            }
        }
    }
}
