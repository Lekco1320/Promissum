using Lekco.Wpf.Utility.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lekco.Wpf.Control
{
    public class Navigator : ItemsControl, INavigator
    {
        public HashSet<NavigationData> NavigationData { get; }

        public INavigationService NavigationService
        {
            get => (INavigationService)GetValue(NavigationServiceProperty);
            set => SetValue(NavigationServiceProperty, value);
        }
        public readonly static DependencyProperty NavigationServiceProperty
            = DependencyProperty.Register(nameof(NavigationService), typeof(INavigationService), typeof(Navigator), new PropertyMetadata(null, OnNavigationServiceChanged));

        public ObservableCollection<NavigationItem> NavigationItems
        {
            get => (ObservableCollection<NavigationItem>)GetValue(NavigationItemsProperty);
            set => SetValue(NavigationItemsProperty, value);
        }
        public readonly static DependencyProperty NavigationItemsProperty
            = DependencyProperty.Register(nameof(NavigationItems), typeof(ObservableCollection<NavigationItem>), typeof(Navigator), new PropertyMetadata(null));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        public readonly static DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Navigator), new PropertyMetadata(Orientation.Horizontal));

        static Navigator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Navigator), new FrameworkPropertyMetadata(typeof(Navigator)));
        }

        public Navigator()
        {
            NavigationItems = new ObservableCollection<NavigationItem>();
            NavigationData = new HashSet<NavigationData>(new NavigationDataComparer());
            AddHandler(NavigationItem.ClickEvent, new RoutedEventHandler(OnNavigationItemClick));
            AddHandler(NavigationItem.RemoveEvent, new RoutedEventHandler(OnNavigationItemRemove));
        }

        private static void OnNavigationServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navigator = (Navigator)d;
            if (e.OldValue is INavigationService oldService)
            {
                oldService.NavigationKeyRegistered -= navigator.RegisterNavigationKey;
                oldService.NavigationKeyUnregistered -= navigator.UnregisterNavigationKey;
                oldService.PropertyChanged -= navigator.OnServicePropertyChanged;
            }
            if (e.NewValue is INavigationService newService)
            {
                newService.NavigationKeyRegistered += navigator.RegisterNavigationKey;
                newService.NavigationKeyUnregistered += navigator.UnregisterNavigationKey;
                newService.PropertyChanged += navigator.OnServicePropertyChanged;
                navigator.InitializeNavigationItem();
                navigator.SetNavigationItem(newService.CurrentKey);
            }
        }

        private void InitializeNavigationItem()
        {
            NavigationItems.Clear();
            NavigationData.Clear();
            if (NavigationService == null)
            {
                return;
            }

            foreach (var data in NavigationService.NavigationData)
            {
                NavigationData.Add(data);
                NavigationItems.Add(new NavigationItem() { Key = data.Key, DataContext = data.ViewModel });
            }
            ItemsSource = NavigationItems;
        }

        private void SetNavigationItem(object? navigationKey)
        {
            if (navigationKey == null)
            {
                return;
            }

            foreach (var item in Items)
            {
                if (item is NavigationItem navigationItem)
                {
                    navigationItem.IsSelected = navigationItem.Key == navigationKey;
                }
            }
        }

        private void OnNavigationItemClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is NavigationItem clickedItem)
            {
                NavigationService?.Navigate(clickedItem.Key);

                foreach (var item in Items)
                {
                    if (item is NavigationItem navigationItem)
                    {
                        navigationItem.IsSelected = navigationItem.Key == clickedItem.Key;
                    }
                }
            }
        }

        private void OnNavigationItemRemove(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is NavigationItem clickedItem)
            {
                NavigationService?.Unregister(clickedItem.Key);
            }
        }

        public void RegisterNavigationKey(object? sender, NavigationData data)
        {
            NavigationData.Add(data);
            NavigationItems.Add(new NavigationItem() { Key = data.Key, DataContext = data.ViewModel });
        }

        public void UnregisterNavigationKey(object? sender, NavigationData data)
        {
            NavigationData.Remove(data);
            NavigationItems.Remove(NavigationItems.First(item => item.Key == data.Key));
        }

        public void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NavigationService.CurrentKey))
            {
                SetNavigationItem(NavigationService.CurrentKey);
            }
        }
    }
}
