using Lekco.Promissum.Control;
using Lekco.Promissum.Sync;
using Lekco.Promissum.ViewModel;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncExclusionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncExclusionWindow : AnimatedWindow
    {
        private SyncExclusionWindowVM _vm;
        public SyncExclusionWindow(SyncExclusionWindowVM vm)
        {
            InitializeComponent();

            DataContext = vm;
            _vm = vm;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static SyncExclusion? NewOrModifySyncExclusion(SyncExclusion? syncExclusion)
        {
            var vm = new SyncExclusionWindowVM(syncExclusion);
            var window = new SyncExclusionWindow(vm);
            window.ShowDialog();
            return vm.SyncExclusion;
        }
    }
}
