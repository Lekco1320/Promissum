using System.Windows;
using System.Windows.Controls;

namespace Lekco.Wpf.Control
{
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    [TemplatePart(Name = "PART_CheckBox", Type = typeof(CheckBox))]
    public class MultiComboBoxItem : ComboBoxItem
    {
        /// <summary>
        /// Initialize static fields.
        /// </summary>
        static MultiComboBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBoxItem), new FrameworkPropertyMetadata(typeof(MultiComboBoxItem)));
        }

        public new bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly new DependencyProperty IsSelectedProperty
            = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(MultiComboBoxItem), new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        public event RoutedEventHandler IsSelectedChanged
        {
            add => AddHandler(IsSelectedChangedEvent, value);
            remove => RemoveHandler(IsSelectedChangedEvent, value);
        }
        public static readonly RoutedEvent IsSelectedChangedEvent = EventManager.RegisterRoutedEvent(nameof(IsSelectedChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MultiComboBoxItem));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var checkBox = Template.FindName("PART_CheckBox", this) as CheckBox;
            if (checkBox != null)
            {
                checkBox.Checked += OnCheckedChanged;
                checkBox.Unchecked += OnCheckedChanged;
            }

            if (Template.FindName("PART_Border", this) is Border border)
            {
                border.MouseLeftButtonDown += (sender, e) =>
                {
                    if (checkBox != null)
                    {
                        checkBox.IsChecked = !checkBox.IsChecked;
                    }
                };
            }
        }

        private void OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                IsSelected = checkBox.IsChecked ?? false;
                RaiseEvent(new RoutedEventArgs(IsSelectedChangedEvent));
            }
        }

        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MultiComboBoxItem mItem)
            {
                mItem.RaiseEvent(new RoutedEventArgs(IsSelectedChangedEvent));
            }
        }
    }
}
