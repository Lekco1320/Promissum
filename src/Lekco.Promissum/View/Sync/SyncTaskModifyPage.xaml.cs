using Lekco.Promissum.ViewModel.Sync;
using System.Windows.Controls;

namespace Lekco.Promissum.View.Sync
{
    /// <summary>
    /// SyncTaskModifyPage.xaml 的交互逻辑
    /// </summary>
    public partial class SyncTaskModifyPage : Page
    {
        public SyncTaskModifyPage(SyncTaskModifyPageVM vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
