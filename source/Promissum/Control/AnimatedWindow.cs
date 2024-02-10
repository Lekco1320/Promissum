using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lekco.Promissum.Control
{
    public class AnimatedWindow : Window
    {
        private bool _needClose = false;

        public AnimatedWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Colors.Transparent);
            ResizeMode = ResizeMode.CanMinimize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            FadeIn();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_needClose)
            {
                _needClose = true;
                e.Cancel = true;
                FadeOut();
            }
            base.OnClosing(e);
        }

        protected void FadeIn()
        {
            Opacity = 0;
            var backBoardAnimation = new DoubleAnimation
            {
                BeginTime = TimeSpan.FromMilliseconds(0),
                FillBehavior = FillBehavior.HoldEnd,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                From = 0,
                To = 1
            };
            BeginAnimation(OpacityProperty, backBoardAnimation);
        }

        protected void FadeOut()
        {
            Opacity = 1;
            var backBoardAnimation = new DoubleAnimation
            {
                BeginTime = TimeSpan.FromMilliseconds(0),
                FillBehavior = FillBehavior.HoldEnd,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                From = 1,
                To = 0
            };
            backBoardAnimation.Completed += TrulyClose;
            BeginAnimation(OpacityProperty, backBoardAnimation);
        }

        private void TrulyClose(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
