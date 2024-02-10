using Lekco.Promissum.Control;
using Lekco.Promissum.Sync;
using Lekco.Promissum.ViewModel;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// NewTaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewTaskWindow : AnimatedWindow
    {
        private NewTaskWindowVM _vm;
        private NewTaskWindow(NewTaskWindowVM vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static SyncTask? NewTask()
        {
            var vm = new NewTaskWindowVM();
            var window = new NewTaskWindow(vm);
            window.ShowDialog();
            return vm.NewTask;
        }
    }
}
