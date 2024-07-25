using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncDeletionRecordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DeletionRecordWindow : CustomWindow
    {
        private DeletionRecordWindowVM _vm;
        public DeletionRecordWindow(DeletionRecordWindowVM vm)
        {
            InitializeComponent();

            DataContext = vm;
            _vm = vm;
        }
    }
}
