using IconPark.Xaml;
using System.Windows;
using System.Windows.Controls;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// IconButton.xaml 的交互逻辑
    /// </summary>
    public partial class IconButton : Button
    {
        public IconKind Kind
        {
            get => (IconKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }
        public readonly static DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(IconKind), typeof(IconButton));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public readonly static DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(IconButton));

        public IconButton()
        {
            InitializeComponent();
        }
    }
}
