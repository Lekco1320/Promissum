using Lekco.Promissum.Control;
using Lekco.Promissum.Sync;
using Lekco.Promissum.ViewModel;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// SyncExclusionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SyncExclusionWindow : CustomWindow
    {
        private SyncExclusionWindowVM _vm;
        public SyncExclusionWindow(SyncExclusionWindowVM vm)
        {
            InitializeComponent();

            DataContext = vm;
            _vm = vm;
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
