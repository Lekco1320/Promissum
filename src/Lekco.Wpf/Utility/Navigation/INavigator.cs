using System.Collections.Generic;
using System.ComponentModel;

namespace Lekco.Wpf.Utility.Navigation
{
    public interface INavigator
    {
        public HashSet<NavigationData> NavigationData { get; }

        public INavigationService NavigationService { get; set; }

        public void RegisterNavigationKey(object? sender, NavigationData data);

        public void UnregisterNavigationKey(object? sender, NavigationData data);

        public void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e);
    }
}
