using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lekco.Wpf.Control
{
    public class IconButton : Button
    {
        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        public Geometry IconData
        {
            get => (Geometry)GetValue(IconDataProperty);
            set => SetValue(IconDataProperty, value);
        }
        public static readonly DependencyProperty IconDataProperty
            = DependencyProperty.Register(nameof(IconData), typeof(Geometry), typeof(IconButton), new PropertyMetadata(default(Geometry)));
    }
}
