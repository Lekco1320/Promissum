using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CustomWindow
    {
        public MainWindowVM _vm;
        public MainWindow(MainWindowVM vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
        }
    }
}
