using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncRecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncRecordWindow : AnimatedWindow
    {
        private SyncRecordWindowVM _vm;
        public SyncRecordWindow(SyncRecordWindowVM vm)
        {
            InitializeComponent();

            DataContext = vm;
            _vm = vm;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
