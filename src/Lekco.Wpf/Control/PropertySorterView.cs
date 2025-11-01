using Lekco.Wpf.MVVM.Sorter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lekco.Wpf.Control
{
    public class PropertySorterView : ListView
    {
        static PropertySorterView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertySorterView), new FrameworkPropertyMetadata(typeof(PropertySorterView)));
        }

        public ICommand? AddSorterCommand
        {
            get => (ICommand?)GetValue(AddSorterCommandProperty);
            set => SetValue(AddSorterCommandProperty, value);
        }
        public static readonly DependencyProperty AddSorterCommandProperty =
            DependencyProperty.Register(nameof(AddSorterCommand), typeof(ICommand), typeof(PropertySorterView));

        public ICommand? RemoveSorterCommand
        {
            get => (ICommand?)GetValue(RemoveSorterCommandProperty);
            set => SetValue(RemoveSorterCommandProperty, value);
        }
        public static readonly DependencyProperty RemoveSorterCommandProperty =
            DependencyProperty.Register(nameof(RemoveSorterCommand), typeof(ICommand), typeof(PropertySorterView));

        public ISorterGroup SorterGroupItem
        {
            get => (ISorterGroup)GetValue(SorterGroupItemProperty);
            set => SetValue(SorterGroupItemProperty, value);
        }
        public static readonly DependencyProperty SorterGroupItemProperty =
            DependencyProperty.Register(nameof(SorterGroupItem), typeof(ISorterGroup), typeof(PropertySorterView),
                new PropertyMetadata(null, OnSorterGroupItemChanged));

        public static readonly RoutedCommand AddSorter =
            new RoutedCommand(nameof(AddSorter), typeof(PropertySorterView));

        public static readonly RoutedCommand RemoveSorter =
            new RoutedCommand(nameof(RemoveSorter), typeof(PropertySorterView));

        public static readonly RoutedCommand UpMoveSorter =
            new RoutedCommand(nameof(UpMoveSorter), typeof(PropertySorterView));

        public static readonly RoutedCommand DownMoveSorter =
            new RoutedCommand(nameof(DownMoveSorter), typeof(PropertySorterView));

        private static void OnSorterGroupItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertySorterView view && e.NewValue is ISorterGroup newItem)
            {
                view.ItemsSource = newItem.Children;
            }
        }

        private static void AddSorterItem(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PropertySorterView view)
            {
                view.AddSorterCommand?.Execute(view.SorterGroupItem);
            }
        }

        private static void RemoveSorterItem(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PropertySorterView view)
            {
                var item = view.SelectedItem as ISorterItem;
                if (view.RemoveSorterCommand != null)
                {
                    view.RemoveSorterCommand.Execute(item);
                    return;
                }
                if (item != null)
                {
                    view.SorterGroupItem.RemoveChild(item);
                }
            }
        }

        private static void UpMoveSorterItem(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PropertySorterView view && view.SorterGroupItem != null)
            {
                int target = view.SelectedIndex - 1;
                if (target > -1 && view.SelectedItem is ISorterItem item)
                {
                    view.SorterGroupItem.RemoveChild(item);
                    view.SorterGroupItem.InsertChild(target, item);
                }
            }
        }

        private static void DownMoveSorterItem(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PropertySorterView view && view.SorterGroupItem != null)
            {
                int target = view.SelectedIndex + 1;
                if (target < view.SorterGroupItem.Children.Count && view.SelectedItem is ISorterItem item)
                {
                    view.SorterGroupItem.RemoveChild(item);
                    view.SorterGroupItem.InsertChild(target, item);
                }
            }
        }

        protected static bool ClearSelectedItem(ItemCollection collection, ItemContainerGenerator generator)
        {
            if (collection == null || generator == null)
            {
                return false;
            }
            for (int i = 0; i < collection.Count; i++)
            {
                if (generator.ContainerFromIndex(i) is ListViewItem item)
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                        return true;
                    }
                }
            }
            return false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_ButtonDock") is Border buttonDock)
            {
                buttonDock.MouseLeftButtonDown += (s, e) =>
                {
                    ClearSelectedItem(Items, ItemContainerGenerator);
                };
            }

            CommandBindings.Add(new CommandBinding(AddSorter, AddSorterItem));
            CommandBindings.Add(new CommandBinding(RemoveSorter, RemoveSorterItem));
            CommandBindings.Add(new CommandBinding(UpMoveSorter, UpMoveSorterItem));
            CommandBindings.Add(new CommandBinding(DownMoveSorter, DownMoveSorterItem));
        }
    }
}