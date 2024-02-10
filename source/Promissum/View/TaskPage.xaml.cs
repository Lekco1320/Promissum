using Lekco.Promissum.Sync;
using Lekco.Promissum.ViewModel;
using System.Windows.Controls;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// TaskPage.xaml 的交互逻辑
    /// </summary>
    public partial class TaskPage : Page
    {
        private TaskPageVM _vm;
        public TaskPage(SyncProject project, SyncTask task)
        {
            InitializeComponent();

            _vm = new TaskPageVM(project, task);
            DataContext = _vm;
        }
    }
}
