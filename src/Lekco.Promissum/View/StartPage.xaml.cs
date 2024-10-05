using Lekco.Promissum.ViewModel;
using System.Windows.Controls;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage(StartPageVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
