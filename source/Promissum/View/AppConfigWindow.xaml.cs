using Lekco.Promissum.Control;
using Lekco.Promissum.ViewModel;
using System.Windows.Input;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// AppConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AppConfigWindow : CustomWindow
    {
        private readonly AppConfigWindowVM _vm;
        private AppConfigWindow(AppConfigWindowVM vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
        }

        public static void SetAppConfig()
        {
            new AppConfigWindow(new AppConfigWindowVM()).ShowDialog();
        }
    }
}
