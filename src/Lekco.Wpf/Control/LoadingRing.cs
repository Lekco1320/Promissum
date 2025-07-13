using System.Windows;
using System.Windows.Media;

namespace Lekco.Wpf.Control
{
    [TemplatePart(Name = "PART_Arc", Type = typeof(Arc))]
    public class LoadingRing : System.Windows.Controls.Control
    {
        static LoadingRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingRing), new FrameworkPropertyMetadata(typeof(LoadingRing)));
        }

        public double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }
        public static readonly DependencyProperty ThicknessProperty
            = DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(LoadingRing), new PropertyMetadata(2.5d));

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }
        public static readonly DependencyProperty StretchProperty
            = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(LoadingRing));
    }
}
