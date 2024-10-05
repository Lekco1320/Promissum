using Lekco.Promissum.ViewModel.Sync;
using Lekco.Wpf.Control;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// ExceptionRecordsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExceptionRecordsWindow : CustomWindow
    {
        public ExceptionRecordsWindow(ExceptionRecordsWindowVM vm)
        {
            InitializeComponent();

            DataContext = vm;
        }
    }
}
