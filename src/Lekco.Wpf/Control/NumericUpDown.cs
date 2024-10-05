using System.Windows;
using System.Windows.Controls;

namespace Lekco.Wpf.Control
{
    public class NumericUpDown : Slider
    {
        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
            SmallChangeProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(1d));
        }
    }
}
