using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AnimatedWindow
    {
        public MainWindowVM _vm;
        public MainWindow(MainWindowVM vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
        }

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
