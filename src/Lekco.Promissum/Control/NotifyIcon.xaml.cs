using Lekco.Wpf.MVVM.Command;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// NotifyIcon.xaml 的交互逻辑
    /// </summary>s
    public partial class NotifyIcon : UserControl
    {
        private static readonly App.Promissum _promissum = (App.Promissum)Application.Current;

        public static ICommand ShowMainWindowCommand { get; } = new RelayCommand(_promissum.ShowMainWindow);

        public NotifyIcon()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
