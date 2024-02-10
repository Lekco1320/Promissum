using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncDeletionRecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DeletionRecordWindow : AnimatedWindow
    {
        private DeletionRecordWindowVM _vm;
        public DeletionRecordWindow(DeletionRecordWindowVM vm)
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
