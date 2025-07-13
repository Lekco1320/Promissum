using System.Windows;

namespace Lekco.Wpf.Utility.Navigation
{
    public class NavigationData
    {
        public object Key { get; set; }

        public FrameworkElement View { get; set; }

        public object? ViewModel { get; set; }

        public NavigationData(object key, FrameworkElement view, object? viewModel = null)
        {
            Key = key;
            View = view;
            ViewModel = viewModel;
        }
    }
}
