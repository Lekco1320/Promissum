using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lekco.Wpf.Control
{
    [TemplatePart(Name = "PART_Indicator", Type = typeof(Border))]
    [TemplatePart(Name = "PART_Track", Type = typeof(Border))]
    public class AnimatedProgressBar : ProgressBar
    {
        static AnimatedProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedProgressBar), new FrameworkPropertyMetadata(typeof(AnimatedProgressBar)));
        }

        public double AnimationSpeed
        {
            get => (double)GetValue(AnimationSpeedProperty);
            set => SetValue(AnimationSpeedProperty, value);
        }
        public static readonly DependencyProperty AnimationSpeedProperty
            = DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(AnimatedProgressBar), new PropertyMetadata(10d));

        private Border? _indicator;

        private Border? _track;

        private Storyboard? _diagonalStoryboard;

        private DoubleAnimation? _animation;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Loaded += (s, e) => SetupAnimation();
        }

        private void SetupAnimation()
        {
            _indicator = GetTemplateChild("PART_Indicator") as Border;
            _track = GetTemplateChild("PART_Track") as Border;

            // Setup value-change animation
            SetupDiagonalAnimation();
            UpdateIndicatorWidth();

            // Initial layout
            ValueChanged += (s, e) => UpdateIndicatorWidth();
            SizeChanged += (s, e) => UpdateIndicatorWidth();
        }

        private void SetupDiagonalAnimation()
        {
            if (_indicator == null)
            {
                return;
            }

            // 若已有Storyboard，则停止并清除
            _diagonalStoryboard?.Remove(_indicator);
            _diagonalStoryboard = new Storyboard();

            _animation = new DoubleAnimation
            {
                From = 0,
                To = _indicator.ActualHeight,
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = TimeSpan.FromSeconds(_indicator.ActualWidth / AnimationSpeed),
            };
            Storyboard.SetTarget(_animation, _indicator);
            Storyboard.SetTargetProperty(_animation, new PropertyPath("(Border.Background).(VisualBrush.RelativeTransform).(TranslateTransform.X)"));

            _diagonalStoryboard.Children.Add(_animation);
            _diagonalStoryboard.Begin(_indicator, true);

            _indicator.SizeChanged += (s, e) => UpdateAnimationDuration();
            ValueChanged += (s, e) => UpdateAnimationDuration();
        }

        private void UpdateAnimationDuration()
        {
            if (_animation == null || _indicator == null || _diagonalStoryboard == null)
            {
                return;
            }

            TimeSpan newDuration = TimeSpan.FromSeconds(_indicator.ActualWidth / AnimationSpeed);
            if (_animation.Duration == newDuration)
            {
                return;
            }

            _animation.To = _indicator.ActualHeight;
            _animation.Duration = newDuration;
            _diagonalStoryboard.Begin(_indicator, true);
        }

        private void UpdateIndicatorWidth()
        {
            if (_indicator == null || _track == null ||
                (IsIndeterminate && _track.ActualWidth == _indicator.Width))
            {
                return;
            }

            double trackWidth = _track.ActualWidth;
            double targetWidth = IsIndeterminate ? trackWidth : trackWidth * (Value - Minimum) / (Maximum - Minimum);

            if (double.IsNaN(targetWidth) || double.IsNaN(targetWidth) || targetWidth < 0)
            {
                return;
            }

            var anim = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            _indicator.BeginAnimation(WidthProperty, anim);
        }
    }
}
