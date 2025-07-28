using Lekco.Wpf.Control;

namespace Lekco.Promissum.DemoHarness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CustomWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}