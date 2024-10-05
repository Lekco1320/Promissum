using Lekco.Wpf.Utility.Navigation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Lekco.Wpf.Control
{
    public class NavigatedFrame : Frame, INavigated
    {
        public new INavigationService NavigationService
        {
            get => (INavigationService)GetValue(NavigationServiceProperty);
            set => SetValue(NavigationServiceProperty, value);
        }

        public readonly static DependencyProperty NavigationServiceProperty
            = DependencyProperty.Register(nameof(NavigationService), typeof(INavigationService), typeof(NavigatedFrame), new PropertyMetadata(null, OnNavigationServiceChanged));

        public bool EnableCache
        {
            get => (bool)GetValue(EnableCacheProperty);
            set => SetValue(EnableCacheProperty, value);
        }
        public readonly static DependencyProperty EnableCacheProperty
            = DependencyProperty.Register(nameof(EnableCache), typeof(bool), typeof(NavigatedFrame), new PropertyMetadata(false));

        public NavigatedFrame()
        {
            NavigationUIVisibility = NavigationUIVisibility.Hidden;
        }

        private static void OnNavigationServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navigatedFrame = (NavigatedFrame)d;
            if (e.OldValue is INavigationService oldService)
            {
                oldService.PropertyChanged -= navigatedFrame.OnServicePropertyChanged;
            }
            if (e.NewValue is INavigationService newService)
            {
                newService.PropertyChanged += navigatedFrame.OnServicePropertyChanged;
                navigatedFrame.ChangeView(newService.CurrentView);
            }
        }

        private static void OnDefaultViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NavigatedFrame frame && frame.Content != null && e.NewValue != null)
            {
                frame.Content = e.NewValue;
            }
        }

        private void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NavigationService.CurrentView))
            {
                ChangeView(NavigationService.CurrentView);
                ClearCache();
            }
        }

        public void ChangeView(object? newView)
        {
            Content = newView;
        }

        private void ClearCache()
        {
            if (!EnableCache)
            {
                while (CanGoBack)
                {
                    RemoveBackEntry();
                }
            }
        }
    }
}
