#nullable disable

using Lekco.Promissum.Sync;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// Navigation.xaml 的交互逻辑
    /// </summary>
    public partial class Navigation : UserControl, INotifyPropertyChanged
    {
        public Navigation()
        {
            InitializeComponent();
            ItemsControl.ItemContainerGenerator.ItemsChanged += ItemSourceChanged;
        }

        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public readonly static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(Navigation));

        public SyncProject SelectedProject
        {
            get => (SyncProject)GetValue(SelectedProjectProperty);
            set => SetValue(SelectedProjectProperty, value);
        }
        public static readonly DependencyProperty SelectedProjectProperty = DependencyProperty.Register(nameof(SelectedProject), typeof(SyncProject), typeof(Navigation));

        public ICommand ChangeCommand
        {
            get => (ICommand)GetValue(ChangeCommandProperty);
            set => SetValue(ChangeCommandProperty, value);
        }
        public static readonly DependencyProperty ChangeCommandProperty = DependencyProperty.Register(nameof(ChangeCommand), typeof(ICommand), typeof(Navigation));

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(Navigation));

        private NavigationItem _preItem;

        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static event PropertyChangedEventHandler SelectedProjectChanged;
        protected internal virtual void OnSelectedProjectChanged()
        {
            if (ItemsControl.ItemContainerGenerator.Items.Count > 0)
            {
                SelectedProject = (SyncProject)ItemsControl.ItemContainerGenerator.Items.Last();
                SelectedProjectChanged?.Invoke(SelectedProject, new PropertyChangedEventArgs(nameof(SyncProject)));
            }
            else
            {
                SelectedProject = null;
            }
        }

        private void ItemMouseDown(object sender, MouseButtonEventArgs e)
        {

            if (!ReferenceEquals(_preItem, sender) && ((NavigationItem)sender).DataContext is SyncProject project)
            {
                SelectedProject = project;
                SelectedProjectChanged?.Invoke(SelectedProject, new PropertyChangedEventArgs(nameof(SyncProject)));
                ChangeCommand?.Execute(this);
            }
        }

        private void RemoveItem(object sender, EventArgs e)
        {
            var removedProject = (e as RoutedEventArgs)!.OriginalSource;
            ItemsSource.Remove(removedProject);
            RemoveCommand?.Execute(removedProject);
            OnSelectedProjectChanged();
        }

        private void ItemSourceChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            OnSelectedProjectChanged();
        }
    }
}
