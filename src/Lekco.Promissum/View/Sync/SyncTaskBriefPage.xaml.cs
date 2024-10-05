using Lekco.Promissum.ViewModel.Sync;
using System.Windows.Controls;

namespace Lekco.Promissum.View.Sync
{
    /// <summary>
    /// SyncTaskBriefPage.xaml 的交互逻辑
    /// </summary>
    public partial class SyncTaskBriefPage : Page
    {
        public SyncTaskBriefPage(SyncTaskBriefPageVM vm)
        {
            InitializeComponent();

            DataContext = vm;
        }
    }
}
