using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lekco.Wpf.Control
{
    [TemplatePart(Name = "PART_EditBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Items", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_Border", Type = typeof(Border))]
    public class BreadcrumbBar : System.Windows.Controls.Control
    {
        static BreadcrumbBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBar), new FrameworkPropertyMetadata(typeof(BreadcrumbBar)));
        }

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register(nameof(Path), typeof(string), typeof(BreadcrumbBar), new PropertyMetadata("", OnPathChanged));

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(BreadcrumbBar), new PropertyMetadata(false));

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }
        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register(nameof(PlaceHolder), typeof(string), typeof(BreadcrumbBar), new PropertyMetadata(""));

        public ICommand? NavigatedCommand
        {
            get => (ICommand?)GetValue(NavigatedCommandProperty);
            set => SetValue(NavigatedCommandProperty, value);
        }
        public static readonly DependencyProperty NavigatedCommandProperty =
            DependencyProperty.Register(nameof(NavigatedCommand), typeof(ICommand), typeof(BreadcrumbBar), new PropertyMetadata(null));

        public event EventHandler<BreadcrumbNavigatedEventArgs> Navigated
        {
            add => AddHandler(NavigatedEvent, value);
            remove => RemoveHandler(NavigatedEvent, value);
        }
        public static readonly RoutedEvent NavigatedEvent
            = EventManager.RegisterRoutedEvent(nameof(Navigated), RoutingStrategy.Bubble, typeof(EventHandler<BreadcrumbNavigatedEventArgs>), typeof(BreadcrumbBar));

        public ObservableCollection<Tuple<string, int>> Segments { get; } = new ObservableCollection<Tuple<string, int>>();

        public ObservableCollection<Tuple<string, int>> DisplaySegments { get; } = new ObservableCollection<Tuple<string, int>>();

        private string _path = "";

        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (BreadcrumbBar)d;
            ctl.Segments.Clear();

            if (e.NewValue is not string path || path.Length == 0)
            {
                return;
            }
            var segments = path.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Length; i++)
            {
                ctl.Segments.Add(new Tuple<string, int>(segments[i], i));
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var items = GetTemplateChild("PART_Items") as ItemsControl;
            items?.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnSegmentClicked));

            if (GetTemplateChild("PART_EditBox") is TextBox textbox)
            {
                textbox.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        IsEditing = false;
                        Path = _path;
                        Navigate(textbox.Text);
                    }
                };
            }

            if (GetTemplateChild("PART_Border") is Border border)
            {
                border.MouseLeftButtonDown += (s, e) =>
                {
                    _path = Path;
                    IsEditing = true;
                };
            }

            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewMouseDown += (s, e) =>
                {
                    if (IsEditing && GetTemplateChild("PART_EditBox") is TextBox textbox)
                    {
                        if (!IsDescendantOf(e.OriginalSource as DependencyObject, textbox))
                        {
                            IsEditing = false;
                            Path = _path;
                        }
                    }
                };
            }

            SizeChanged += (s, e) => UpdateDisplaySegments();
            Segments.CollectionChanged += (s, e) => UpdateDisplaySegments();
            UpdateDisplaySegments();
        }

        private static bool IsDescendantOf(DependencyObject? source, DependencyObject? parent)
        {
            while (source != null)
            {
                if (source == parent)
                {
                    return true;
                }
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }

        private void OnSegmentClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn && btn.DataContext is Tuple<string, int> tuple &&
                tuple.Item2 >= 0 && tuple.Item2 < Segments.Count)
            {
                string path = "";
                for (int i = 0; i <= tuple.Item2; i++)
                {
                    path += Segments[i].Item1;
                    path += System.IO.Path.DirectorySeparatorChar;
                }
                Navigate(path);
            }
        }

        private void Navigate(string fullPath)
        {
            RaiseEvent(new BreadcrumbNavigatedEventArgs(NavigatedEvent, fullPath));

            var command = NavigatedCommand;
            if (command != null)
            {
                if (command.CanExecute(fullPath))
                {
                    command.Execute(fullPath);
                }
            }
        }

        private void UpdateDisplaySegments()
        {
            DisplaySegments.Clear();
            if (Segments.Count == 0)
            {
                DisplaySegments.Add(new Tuple<string, int>(PlaceHolder ?? "", -1));
                return;
            }

            double available = ActualWidth * 0.93 - 30;
            double sum = 0;
            var toShow = new List<Tuple<string, int>>();
            for (int i = Segments.Count - 1; i >= 0; i--)
            {
                double w = MeasureTextWidth(Segments[i].Item1) + 12;
                if (sum + w > available)
                {
                    break;
                }
                sum += w;
                toShow.Add(Segments[i]);
            }

            toShow.Reverse();
            if (toShow.Count < Segments.Count && Segments.Count > 1)
            {
                DisplaySegments.Add(new Tuple<string, int>("…", -1));
            }
            if (toShow.Count == 0)
            {
                DisplaySegments.Add(Segments[^1]);
                return;
            }
            foreach (var seg in toShow)
            {
                DisplaySegments.Add(seg);
            }
        }

        private double MeasureTextWidth(string text)
        {
            var ft = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
            );
            return ft.WidthIncludingTrailingWhitespace;
        }
    }

    public class BreadcrumbNavigatedEventArgs : RoutedEventArgs
    {
        public string FullPath { get; }

        public BreadcrumbNavigatedEventArgs(RoutedEvent routedEvent, string fullPath)
            : base(routedEvent)
        {
            FullPath = fullPath;
        }
    }
}
