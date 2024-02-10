using Lekco.Promissum.Control;
using Lekco.Promissum.Sync;
using Lekco.Promissum.ViewModel;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncProjectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncProjectWindow : AnimatedWindow
    {
        public SyncProjectWindowVM _vm;
        private SyncProjectWindow(SyncProjectWindowVM vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static SyncProject? NewOrSetSyncProject(SyncProject? syncProject)
        {
            var vm = new SyncProjectWindowVM(syncProject);
            new SyncProjectWindow(vm).ShowDialog();
            return vm.Project;
        }
    }
}
