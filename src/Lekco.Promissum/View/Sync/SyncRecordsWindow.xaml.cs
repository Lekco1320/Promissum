using Lekco.Promissum.ViewModel.Sync;
using Lekco.Wpf.Control;

namespace Lekco.Promissum.View.Sync
{
    /// <summary>
    /// SyncRecordsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncRecordsWindow : CustomWindow
    {
        public SyncRecordsWindow(SyncRecordsWindowVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
