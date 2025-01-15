using Lekco.Promissum.ViewModel.Sync;
using System.Windows.Controls;

namespace Lekco.Promissum.View.Sync
{
    /// <summary>
    /// SyncTaskEditPage.xaml 的交互逻辑
    /// </summary>
    public partial class SyncTaskEditPage : Page
    {
        public SyncTaskEditPage(SyncTaskEditPageVM vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
