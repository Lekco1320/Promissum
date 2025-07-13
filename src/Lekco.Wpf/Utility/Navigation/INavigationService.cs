using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Lekco.Wpf.Utility.Navigation
{
    public interface INavigationService : INotifyPropertyChanged
    {
        public Dictionary<object, NavigationData> NavigationMapping { get; }

        public object? CurrentKey { get; }

        public FrameworkElement? CurrentView { get; }

        public IEnumerable<NavigationData> NavigationData { get; }

        public event EventHandler<NavigationData>? NavigationKeyRegistered;

        public event EventHandler<NavigationData>? NavigationKeyUnregistered;

        public event EventHandler<object>? CurrentKeyChanged;

        public event EventHandler<FrameworkElement?>? CurrentViewChanged;

        public void Register(object key, FrameworkElement view, object? vm = null);

        public void Unregister(object key);

        public void Navigate(object key);

        public FrameworkElement ChangeView(object key, FrameworkElement newView);
    }
}
