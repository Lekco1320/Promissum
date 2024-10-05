using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System;
using System.Xml.Linq;

namespace Lekco.Wpf.MVVM
{
    /// <summary>
    /// The base class for view model, with having the ability to notify other objects
    /// when its properties change and listen to other <see cref="INotifyPropertyChanged"/>s
    /// when their properties raise event <see cref="INotifyPropertyChanged.PropertyChanged"/>.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IWeakEventListener
    {
        /// <summary>
        /// The dictionary storing notifier and the action to take when notify is listened.
        /// </summary>
        private readonly Dictionary<NotifierProperty, Action?> _notifierDictionary;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        public ViewModelBase()
        {
            _notifierDictionary = new Dictionary<NotifierProperty, Action?>();
        }

        /// <summary>
        /// Register a notifier to the class.
        /// </summary>
        /// <param name="notifier">Notifier object.</param>
        /// <param name="propertyName">Name of notifier's property.</param>
        /// <param name="callBack">Action to take when notify is listened.</param>
        protected void RegisterNotifier(INotifyPropertyChanged notifier, string propertyName, Action? callBack = null)
        {
            var _notifier = new NotifierProperty(notifier, propertyName);
            _notifierDictionary.TryAdd(_notifier, callBack);
            PropertyChangedEventManager.AddListener(notifier, this, propertyName);
        }

        /// <summary>
        /// Register a notifier to the class.
        /// </summary>
        /// <param name="notifier">Notifier object.</param>
        /// <param name="propertyName">Name of notifier's property.</param>
        /// <param name="vmName">Name in the viewmodel that will be raise <see cref="PropertyChanged"/> when notify is listened.</param>
        protected void RegisterNotifier(INotifyPropertyChanged notifier, string propertyName, string vmName)
        {
            RegisterNotifier(notifier, propertyName, () => OnPropertyChanged(vmName));
        }

        /// <summary>
        /// Register a notifier to the class.
        /// </summary>
        /// <param name="notifier">Notifier object.</param>
        /// <param name="propertyName">Name of notifier's property.</param>
        /// <param name="vmNames">Names in the viewmodel that will be raise <see cref="PropertyChanged"/> when notify is listened.</param>
        protected void RegisterNotifier(INotifyPropertyChanged notifier, string propertyName, IEnumerable<string> vmNames)
        {
            void callback()
            {
                foreach (var name in vmNames)
                {
                    OnPropertyChanged(name);
                }
            }
            RegisterNotifier(notifier, propertyName, callback);
        }

        /// <summary>
        /// Unregister a notifier to the class.
        /// </summary>
        /// <param name="notifier">Notifier object.</param>
        /// <param name="propertyName">Name of notifier's property.</param>
        protected void UnregisterNotifier(INotifyPropertyChanged notifier, string propertyName)
        {
            var _notifier = new NotifierProperty(notifier, propertyName);
            _notifierDictionary.Remove(_notifier);
            PropertyChangedEventManager.RemoveListener(notifier, this, propertyName);
        }

        /// <summary>
        /// Try matching the notifier in registered ones and call action if matches successfully.
        /// </summary>
        /// <param name="notifier">Notifier object.</param>
        /// <param name="propertyName">Name of notifier's property.</param>
        private void MatchNotifier(INotifyPropertyChanged notifier, string propertyName)
        {
            var _notifier = new NotifierProperty(notifier, propertyName);
            if (_notifierDictionary.TryGetValue(_notifier, out var action))
            {
                action?.Invoke();
            }
        }

        /// <inheritdoc />
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (e is PropertyChangedEventArgs propertyEventArgs)
            {
                MatchNotifier((INotifyPropertyChanged)sender, propertyEventArgs.PropertyName ?? "");
            }
            return true;
        }

        /// <summary>
        /// Raises when property changed.
        /// </summary>
        /// <param name="propertyName">Name of notifier's property.</param>
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Describing a property of notifier which should implement <see cref="INotifyPropertyChanged"/>.
        /// </summary>
        public class NotifierProperty : IEquatable<NotifierProperty>
        {
            /// <summary>
            /// Real type of the notifier.
            /// </summary>
            public Type NotifierType { get; }

            /// <summary>
            /// Name of notifier's property.
            /// </summary>
            public string PropertyName { get; }

            /// <summary>
            /// Create an instance.
            /// </summary>
            /// <param name="notifier">Notifier object.</param>
            /// <param name="propertyName">Name of notifier's property.</param>
            public NotifierProperty(INotifyPropertyChanged notifier, string propertyName)
            {
                NotifierType = notifier.GetType();
                PropertyName = propertyName;
            }

            /// <inheritdoc />
            public bool Equals(NotifierProperty? other)
            {
                if (other == null)
                {
                    return false;
                }
                return PropertyName == other.PropertyName && NotifierType == other.NotifierType;
            }

            /// <inheritdoc />
            public override bool Equals(object? obj)
            {
                return Equals(obj as NotifierProperty);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return HashCode.Combine(NotifierType, PropertyName);
            }
        }
    }
}
