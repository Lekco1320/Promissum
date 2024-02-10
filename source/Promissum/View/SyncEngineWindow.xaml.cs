using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncEngineWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncEngineWindow : AnimatedWindow
    {
        private static bool _hasShown;
        private static readonly SyncEngineWindowVM _vm;

        static SyncEngineWindow()
        {
            _hasShown = false;
            _vm = new SyncEngineWindowVM();
        }

        public SyncEngineWindow()
        {
            InitializeComponent();

            DataContext = _vm;
        }

        public new void Close()
        {
            _hasShown = false;
        }

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void ShowSyncEngineWindow()
        {
            if (!_hasShown)
            {
                _hasShown = true;
                new SyncEngineWindow().Show();
            }
        }
    }
}
