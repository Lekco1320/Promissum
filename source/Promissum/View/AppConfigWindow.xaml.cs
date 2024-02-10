using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// AppConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AppConfigWindow : AnimatedWindow
    {
        private readonly AppConfigWindowVM _vm;
        private AppConfigWindow(AppConfigWindowVM vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void SetAppConfig()
        {
            new AppConfigWindow(new AppConfigWindowVM()).ShowDialog();
        }
    }
}
