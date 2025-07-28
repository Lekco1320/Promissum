using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Lekco.Wpf.Control
{
    public class IconToggleButton : ToggleButton
    {
        static IconToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconToggleButton), new FrameworkPropertyMetadata(typeof(IconToggleButton)));
        }

        public Geometry IconData
        {
            get => (Geometry)GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }
        public static readonly DependencyProperty IconDataProperty
            = DependencyProperty.Register(nameof(IconData), typeof(Geometry), typeof(IconToggleButton), new PropertyMetadata(default(Geometry)));
    }
}
