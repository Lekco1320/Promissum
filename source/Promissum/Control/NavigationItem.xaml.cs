using Lekco.Promissum.Sync;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// NavigationItem.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationItem : UserControl
    {
        public NavigationItem()
        {
            InitializeComponent();
            Navigation.SelectedProjectChanged += CheckIsSelected;
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(NavigationItem));

        public SyncProject Project
        {
            get => (SyncProject)GetValue(ProjectProperty);
            set => SetValue(ProjectProperty, value);
        }
        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(SyncProject), typeof(NavigationItem));

        public SyncProject SelectedProject
        {
            get => (SyncProject)GetValue(SelectedProjectProperty);
            set => SetValue(SelectedProjectProperty, value);
        }
        public static readonly DependencyProperty SelectedProjectProperty = DependencyProperty.Register(nameof(SelectedProject), typeof(SyncProject), typeof(NavigationItem));

        public event EventHandler RemoveEvent
        {
            add => AddHandler(RemoveEventHandler, value);
            remove => RemoveHandler(RemoveEventHandler, value);
        }
        public static readonly RoutedEvent RemoveEventHandler = EventManager.RegisterRoutedEvent(nameof(RemoveEvent), RoutingStrategy.Bubble, typeof(EventHandler), typeof(NavigationItem));

        private void ClickCloseButton(object sender, MouseButtonEventArgs e)
        {
            var routedEventArgs = new RoutedEventArgs(RemoveEventHandler, Project);
            RaiseEvent(routedEventArgs);
        }

        private void CheckIsSelected(object? sender, PropertyChangedEventArgs e)
        {
            IsSelected = sender == Project;
        }

        private void Loading(object sender, RoutedEventArgs e)
        {
            IsSelected = SelectedProject == Project;
        }
    }
}
