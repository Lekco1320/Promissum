using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lekco.Wpf.Control
{
    public class LabeledProgressBar : ProgressBar
    {
        static LabeledProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledProgressBar), new FrameworkPropertyMetadata(typeof(LabeledProgressBar)));
        }

        public string FormatString
        {
            get => (string)GetValue(FormatStringProperty);
            set => SetValue(FormatStringProperty, value);
        }
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register(nameof(FormatString), typeof(string), typeof(LabeledProgressBar), new PropertyMetadata("{0:0}%"));

        public double LabelSpacing
        {
            get => (double)GetValue(LabelSpacingProperty);
            set => SetValue(LabelSpacingProperty, value);
        }
        public static readonly DependencyProperty LabelSpacingProperty =
            DependencyProperty.Register(nameof(LabelSpacing), typeof(double), typeof(LabeledProgressBar), new PropertyMetadata(2d));

        public Brush InsideTextBrush
        {
            get => (Brush)GetValue(InsideTextBrushProperty);
            set => SetValue(InsideTextBrushProperty, value);
        }
        public static readonly DependencyProperty InsideTextBrushProperty =
            DependencyProperty.Register(nameof(InsideTextBrush), typeof(Brush), typeof(LabeledProgressBar), new PropertyMetadata(Brushes.White));

        public Brush OutsideTextBrush
        {
            get => (Brush)GetValue(OutsideTextBrushProperty);
            set => SetValue(OutsideTextBrushProperty, value);
        }
        public static readonly DependencyProperty OutsideTextBrushProperty
            = DependencyProperty.Register(nameof(OutsideTextBrush), typeof(Brush), typeof(LabeledProgressBar), new PropertyMetadata(Brushes.Black));

        private TextBlock? _label = null;

        private Grid? _indicator = null;

        private Border? _track = null;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _label = GetTemplateChild("PART_Label") as TextBlock;
            _indicator = GetTemplateChild("PART_Indicator") as Grid;
            _track = GetTemplateChild("PART_Track") as Border;

            SizeChanged += (s, e) => UpdateIndicatorAndLabel();
            ValueChanged += (s, e) => UpdateIndicatorAndLabel();

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(UpdateIndicatorAndLabel));
        }

        private void UpdateIndicatorAndLabel()
        {
            if (_indicator == null || _track == null || _label == null)
            {
                return;
            }

            if (IsIndeterminate)
            {
                _label.Visibility = Visibility.Collapsed;
                return;
            }

            string text = string.Format(FormatString, Value);
            _label.Text = text;
            double width = MeasureTextWidth(text) + LabelSpacing;

            if (_track.Width < width)
            {
                _label.Visibility = Visibility.Collapsed;
                return;
            }
            _label.Visibility = Visibility.Visible;

            if (_indicator.ActualWidth - width >= 0)
            {
                _label.Margin = new Thickness(_indicator.ActualWidth - width, 0, 0, 0);
                _label.Foreground = InsideTextBrush;
            }
            else
            {
                _label.Margin = new Thickness(_indicator.ActualWidth + LabelSpacing, 0, 0, 0);
                _label.Foreground = OutsideTextBrush;
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
}
