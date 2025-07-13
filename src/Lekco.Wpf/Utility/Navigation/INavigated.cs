using System.Windows;

namespace Lekco.Wpf.Utility.Navigation
{
    public interface INavigated
    {
        public INavigationService NavigationService { get; set; }

        public void ChangeView(FrameworkElement? newView);
    }
}
