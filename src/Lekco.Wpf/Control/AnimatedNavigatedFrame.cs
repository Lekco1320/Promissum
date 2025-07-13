using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Lekco.Wpf.Control
{
    public class AnimatedNavigatedFrame : NavigatedFrame
    {
        public AnimationTransitionType AnimationType
        {
            get => (AnimationTransitionType)GetValue(AnimationTypeProperty);
            set => SetValue(AnimationTypeProperty, value);
        }
        public static readonly DependencyProperty AnimationTypeProperty
            = DependencyProperty.Register(nameof(AnimationType), typeof(AnimationTransitionType), typeof(AnimatedNavigatedFrame),
                new PropertyMetadata(AnimationTransitionType.FadeOut));

        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }
        public static readonly DependencyProperty AnimationDurationProperty
            = DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(AnimatedNavigatedFrame),
                new PropertyMetadata(TimeSpan.FromMilliseconds(200)));

        public double AnimationDurationMs => AnimationDuration.TotalMilliseconds;

        public override void ChangeView(FrameworkElement? newView)
        {
            if (AnimationType == AnimationTransitionType.None)
            {
                base.ChangeView(newView);
                return;
            }

            if (Content is not FrameworkElement oldView)
            {
                Content = newView;
                return;
            }

            switch (AnimationType)
            {
            case AnimationTransitionType.FadeOut:
                FadeOutAnimation(oldView, newView);
                break;

            case AnimationTransitionType.SlideOut:
                SlideOutAnimation(oldView, newView);
                break;

            case AnimationTransitionType.Blur:
                BlurAnimation(oldView, newView);
                break;

            case AnimationTransitionType.Zoom:
                ZoomAnimation(oldView, newView);
                break;

            case AnimationTransitionType.ElasticZoom:
                ElasticZoomAnimation(oldView, newView);
                break;
            }
        }

        protected virtual void FadeOutAnimation(FrameworkElement oldView, FrameworkElement? newView)
        {
            var fadeOut = new DoubleAnimation(1, 0, AnimationDuration);
            fadeOut.Completed += (s, e) =>
            {
                Content = newView;

                if (newView != null)
                {
                    newView.Opacity = 0;
                    var fadeIn = new DoubleAnimation(0, 1, AnimationDuration);
                    newView.BeginAnimation(OpacityProperty, fadeIn);
                }
            };
            oldView.BeginAnimation(OpacityProperty, fadeOut);
        }

        // Best Duration: 300ms
        protected virtual void SlideOutAnimation(FrameworkElement oldView, FrameworkElement? newView)
        {
            // Ensure oldView has RenderTransform
            var oldTrans = new TranslateTransform(0, 0);
            oldView.RenderTransform = oldTrans;

            // Slide out and slide to the left
            var sbOut = new Storyboard();
            var fadeOut = new DoubleAnimation(1, 0, AnimationDuration);
            Storyboard.SetTarget(fadeOut, oldView);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            sbOut.Children.Add(fadeOut);

            var slideOut = new DoubleAnimation(0, -ActualWidth, AnimationDuration);
            Storyboard.SetTarget(slideOut, oldView);
            Storyboard.SetTargetProperty(slideOut, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            sbOut.Children.Add(slideOut);

            sbOut.Completed += (s, e) =>
            {
                // Change content
                Content = newView;
                if (newView == null)
                {
                    return;
                }

                // Initial state of newView: transparent and positioned right
                newView.Opacity = 0;
                var newTrans = new TranslateTransform(ActualWidth, 0);
                newView.RenderTransform = newTrans;

                // Fade in and slide from the right
                var sbIn = new Storyboard();
                var fadeIn = new DoubleAnimation(0, 1, AnimationDuration);
                Storyboard.SetTarget(fadeIn, newView);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
                sbIn.Children.Add(fadeIn);

                var slideIn = new DoubleAnimation(ActualWidth, 0, AnimationDuration);
                Storyboard.SetTarget(slideIn, newView);
                Storyboard.SetTargetProperty(slideIn, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                sbIn.Children.Add(slideIn);

                sbIn.Begin();
            };

            sbOut.Begin();
        }

        protected virtual void BlurAnimation(FrameworkElement oldView, FrameworkElement? newView)
        {
            TimeSpan duration1 = TimeSpan.FromMilliseconds(AnimationDurationMs * 1.5);

            oldView.Effect = new BlurEffect { Radius = 0 };
            var blurAnim = new DoubleAnimation(0, 10, AnimationDuration);
            oldView.Effect.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);

            // Fade out at the same time
            oldView.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, duration1));

            if (newView == null)
            {
                Content = null;
                return;
            }

            // Change content after animation completed,
            // and make newView from blur to clear
            newView.Effect = new BlurEffect { Radius = 10 };
            newView.Opacity = 0;
            Content = newView;

            newView.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration1));
            newView.Effect.BeginAnimation(BlurEffect.RadiusProperty, new DoubleAnimation(10, 0, duration1));
        }

        protected virtual void ZoomAnimation(FrameworkElement oldView, FrameworkElement? newView)
        {
            var oldScale = new ScaleTransform(1, 1);
            oldView.RenderTransformOrigin = new Point(0.5, 0.5);
            oldView.RenderTransform = oldScale;

            var fadeOut = new DoubleAnimation(1, 0, AnimationDuration);
            var scaleDown = new DoubleAnimation(1, 0.9, AnimationDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var sbOut = new Storyboard();
            Storyboard.SetTarget(fadeOut, oldView);
            Storyboard.SetTarget(scaleDown, oldView);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            Storyboard.SetTargetProperty(scaleDown, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            sbOut.Children.Add(fadeOut);
            sbOut.Children.Add(scaleDown);

            // Y-Scale
            var scaleDownY = scaleDown.Clone();
            Storyboard.SetTarget(scaleDownY, oldView);
            Storyboard.SetTargetProperty(scaleDownY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            sbOut.Children.Add(scaleDownY);

            sbOut.Completed += (s, e) =>
            {
                Content = newView;
                if (newView == null)
                {
                    return;
                }

                newView.Opacity = 0;
                var newScale = new ScaleTransform(0.9, 0.9);
                newView.RenderTransformOrigin = new Point(0.5, 0.5);
                newView.RenderTransform = newScale;

                var fadeIn = new DoubleAnimation(0, 1, AnimationDuration);
                var scaleUp = new DoubleAnimation(0.9, 1, AnimationDuration)
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                var sbIn = new Storyboard();
                Storyboard.SetTarget(fadeIn, newView);
                Storyboard.SetTarget(scaleUp, newView);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
                Storyboard.SetTargetProperty(scaleUp, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                sbIn.Children.Add(fadeIn);
                sbIn.Children.Add(scaleUp);

                // Y-Axis Scale
                var scaleUpY = scaleUp.Clone();
                Storyboard.SetTarget(scaleUpY, newView);
                Storyboard.SetTargetProperty(scaleUpY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
                sbIn.Children.Add(scaleUpY);

                sbIn.Begin();
            };

            sbOut.Begin();
        }

        protected virtual void ElasticZoomAnimation(FrameworkElement oldView, FrameworkElement? newView)
        {
            TimeSpan duration1 = TimeSpan.FromMilliseconds(AnimationDurationMs * 1.5);
            TimeSpan duration2 = TimeSpan.FromMilliseconds(AnimationDurationMs * 2);

            var oldScale = new ScaleTransform(1, 1);
            oldView.RenderTransformOrigin = new Point(0.5, 0.5);
            oldView.RenderTransform = oldScale;

            // scale down and fade out
            var sbOut = new Storyboard();
            var fadeOut = new DoubleAnimation(1, 0, AnimationDuration);
            Storyboard.SetTarget(fadeOut, oldView);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            sbOut.Children.Add(fadeOut);

            var scaleOut = new DoubleAnimation(1, 0.8, AnimationDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(scaleOut, oldView);
            Storyboard.SetTargetProperty(scaleOut, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            sbOut.Children.Add(scaleOut);
            // Synchronize Y and X
            var scaleOutY = scaleOut.Clone();
            Storyboard.SetTargetProperty(scaleOutY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            sbOut.Children.Add(scaleOutY);

            sbOut.Completed += (s, e) =>
            {
                Content = newView;
                if (newView == null)
                {
                    return;
                }

                newView.Opacity = 0;
                var newScale = new ScaleTransform(0.8, 0.8);
                newView.RenderTransformOrigin = new Point(0.5, 0.5);
                newView.RenderTransform = newScale;

                // Scale up and fade in and elastic effect
                var sbIn = new Storyboard();
                var fadeIn = new DoubleAnimation(0, 1, duration1);
                Storyboard.SetTarget(fadeIn, newView);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
                sbIn.Children.Add(fadeIn);

                var scaleIn = new DoubleAnimation(0.8, 1, duration2)
                {
                    EasingFunction = new ElasticEase
                    {
                        Oscillations = 1,
                        Springiness = 3,
                        EasingMode = EasingMode.EaseOut
                    }
                };
                Storyboard.SetTarget(scaleIn, newView);
                Storyboard.SetTargetProperty(scaleIn, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                sbIn.Children.Add(scaleIn);
                var scaleInY = scaleIn.Clone();
                Storyboard.SetTargetProperty(scaleInY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
                sbIn.Children.Add(scaleInY);

                sbIn.Begin();
            };

            sbOut.Begin();
        }

        public enum AnimationTransitionType
        {
            None,
            FadeOut,
            SlideOut,
            Blur,
            Zoom,
            ElasticZoom,
        }
    }
}
