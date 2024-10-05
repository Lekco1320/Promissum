using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Lekco.Wpf.Utility.Navigation
{
    public class NavigationService : INavigationService
    {
        public object? CurrentKey
        {
            get => _currentKey;
            protected set
            {
                _currentKey = value;
                CurrentKeyChanged?.Invoke(this, value!);
                OnPropertyChanged(nameof(CurrentKey));
            }
        }
        private object? _currentKey;

        public object? CurrentView
        {
            get => _currentView;
            protected set
            {
                _currentView = value;
                CurrentViewChanged?.Invoke(this, value);
                OnPropertyChanged(nameof(CurrentView));
            }
        }
        private object? _currentView;

        public object? DefaultView
        {
            get => _defaultView;
            set
            {
                _defaultView = value;
                CurrentView ??= value;
                OnPropertyChanged(nameof(DefaultView));
            }
        }
        private object? _defaultView;

        public Dictionary<object, NavigationData> NavigationMapping { get; protected set; }

        public IEnumerable<NavigationData> NavigationData => NavigationMapping.Values;

        public event EventHandler<NavigationData>? NavigationKeyRegistered;

        public event EventHandler<NavigationData>? NavigationKeyUnregistered;

        public event EventHandler<object>? CurrentKeyChanged;

        public event EventHandler<object?>? CurrentViewChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public NavigationService()
        {
            NavigationMapping = new Dictionary<object, NavigationData>();
        }

        public void Register(object key, object view, object? vm = null)
        {
            var data = new NavigationData(key, view, vm);
            NavigationMapping[key] = data;
            NavigationKeyRegistered?.Invoke(this, data);

            CurrentKey = key;
            CurrentView = view;
        }

        public void Unregister(object key)
        {
            var data = NavigationMapping[key];
            NavigationMapping.Remove(key);
            NavigationKeyUnregistered?.Invoke(this, data);

            if (CurrentKey == key)
            {
                CurrentKey = NavigationMapping.Keys.FirstOrDefault();
                CurrentView = CurrentKey != null ? NavigationMapping[CurrentKey].View : DefaultView;
            }
        }

        public void Navigate(object key)
        {
            if (NavigationMapping.TryGetValue(key, out NavigationData? value))
            {
                CurrentKey = key;
                CurrentView = value.View ?? DefaultView;
            }
        }

        public object ChangeView(object key, object newView)
        {
            var oldView = NavigationMapping[key].View;
            NavigationMapping[key].View = newView;

            if (CurrentKey == key)
            {
                Navigate(key);
            }
            return oldView;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
