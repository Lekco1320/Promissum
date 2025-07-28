using Lekco.Wpf.MVVM.Filter;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lekco.Wpf.Control
{
    [TemplatePart(Name = "PART_ButtonDock", Type = typeof(Border))]
    public class PropertyFilterView : TreeView
    {
        static PropertyFilterView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyFilterView), new FrameworkPropertyMetadata(typeof(PropertyFilterView)));
        }

        public ICommand? AddFilterCommand
        {
            get => (ICommand?)GetValue(AddFilterCommandProperty);
            set => SetValue(AddFilterCommandProperty, value);
        }
        public static readonly DependencyProperty AddFilterCommandProperty =
            DependencyProperty.Register(nameof(AddFilterCommand), typeof(ICommand), typeof(PropertyFilterView));

        public ICommand? AddFilterGroupCommand
        {
            get => (ICommand?)GetValue(AddFilterGroupProperty);
            set => SetValue(AddFilterGroupProperty, value);
        }
        public static readonly DependencyProperty AddFilterGroupProperty =
            DependencyProperty.Register(nameof(AddFilterGroupCommand), typeof(ICommand), typeof(PropertyFilterView));

        public ICommand? RemoveFilterCommand
        {
            get => (ICommand?)GetValue(RemoveFilterCommandProperty);
            set => SetValue(RemoveFilterCommandProperty, value);
        }
        public static readonly DependencyProperty RemoveFilterCommandProperty =
            DependencyProperty.Register(nameof(RemoveFilterCommand), typeof(ICommand), typeof(PropertyFilterView));

        public static readonly RoutedCommand AddFilter =
            new RoutedCommand(nameof(AddFilter), typeof(PropertyFilterView));

        public static readonly RoutedCommand AddFilterGroup =
            new RoutedCommand(nameof(AddFilterGroup), typeof(PropertyFilterView));

        public static readonly RoutedCommand RemoveFilter =
            new RoutedCommand(nameof(RemoveFilter), typeof(PropertyFilterView));

        public LambdaExpression Expression
        {
            get => (LambdaExpression)GetValue(ExpressionProperty);
            set => SetValue(ExpressionProperty, value);
        }
        public static readonly DependencyProperty ExpressionProperty =
            DependencyProperty.Register(nameof(Expression), typeof(LambdaExpression), typeof(PropertyFilterView));

        public IFilterGroupNode FilterGroupNode
        {
            get => (IFilterGroupNode)GetValue(FilterGroupNodeProperty);
            set => SetValue(FilterGroupNodeProperty, value);
        }
        public static readonly DependencyProperty FilterGroupNodeProperty =
            DependencyProperty.Register(nameof(FilterGroupNode), typeof(IFilterGroupNode), typeof(PropertyFilterView),
                new PropertyMetadata(null, OnFilterGroupNodeChanged));

        public IFilterNode? SelectedFilterNode { get; set; }

        private static IFilterGroupNode? GetFilterGroupNode(IFilterNode? node)
        {
            if (node == null)
            {
                return null;
            }
            if (node is IFilterGroupNode groupNode)
            {
                return groupNode;
            }
            return node.Parent;
        }

        private static void AddFilterNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PropertyFilterView view)
            {
                var node = (view.SelectedItem ?? view.FilterGroupNode) as IFilterNode;
                var param = GetFilterGroupNode(node);
                view.AddFilterCommand?.Execute(param);
            }
        }

        private static void AddFilterGroupNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PropertyFilterView view)
            {
                var node = (view.SelectedItem ?? view.FilterGroupNode) as IFilterNode;
                var param = GetFilterGroupNode(node);
                if (view.AddFilterGroupCommand != null)
                {
                    view.AddFilterGroupCommand.Execute(param);
                    return;
                }
                param?.AddChildGroup();
            }
        }

        private static void RemoveFilterNode(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PropertyFilterView view)
            {
                var node = (view.SelectedItem ?? view.FilterGroupNode) as IFilterNode;
                if (view.RemoveFilterCommand != null)
                {
                    view.RemoveFilterCommand.Execute(node);
                    return;
                }
                node?.Remove();
            }
        }

        private static void OnFilterGroupNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PropertyFilterView view && e.NewValue is IFilterGroupNode newNode)
            {
                view.ItemsSource = newNode.Children;
            }
        }

        protected static bool ClearSelectedNode(ItemCollection collection, ItemContainerGenerator generator)
        {
            if (collection == null || generator == null)
            {
                return false;
            }
            for (int i = 0; i < collection.Count; i++)
            {
                if (generator.ContainerFromIndex(i) is TreeViewItem item)
                {
                    if (item.IsSelected)
                    {
                        item.IsSelected = false;
                        return true;
                    }
                    if (ClearSelectedNode(item.Items, item.ItemContainerGenerator))
                    {
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
                    ClearSelectedNode(Items, ItemContainerGenerator);
                };
            }

            CommandBindings.Add(new CommandBinding(AddFilter, AddFilterNode));
            CommandBindings.Add(new CommandBinding(AddFilterGroup, AddFilterGroupNode));
            CommandBindings.Add(new CommandBinding(RemoveFilter, RemoveFilterNode));
        }
    }
}
