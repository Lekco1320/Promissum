using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Lekco.Wpf.Control
{
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    public class NavigationItem : ButtonBase
    {
        public object Key
        {
            get => (object)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }
        public readonly static DependencyProperty KeyProperty
            = DependencyProperty.Register(nameof(Key), typeof(object), typeof(NavigationItem));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public readonly static DependencyProperty IsSelectedProperty
            = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(NavigationItem), new PropertyMetadata(false));

        public static RoutedCommand RemoveCommand { get; }

        public event RoutedEventHandler Remove
        {
            add => AddHandler(RemoveEvent, value);
            remove => RemoveHandler(RemoveEvent, value);
        }
        public static readonly RoutedEvent RemoveEvent = EventManager.RegisterRoutedEvent(nameof(Remove), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavigationItem));

        static NavigationItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationItem), new FrameworkPropertyMetadata(typeof(NavigationItem)));
            RemoveCommand = new RoutedCommand(nameof(RemoveCommand), typeof(NavigationItem));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_CloseButton") is Button closeButton)
            {
                closeButton.Click += (sender, e) => RaiseEvent(new RoutedEventArgs(RemoveEvent));
            }
        }
    }
}
