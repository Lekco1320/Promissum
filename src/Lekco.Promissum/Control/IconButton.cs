using Lekco.Wpf.Control;
using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;

namespace Lekco.Promissum.Control
{
    public class IconButton : Button
    {
        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        public PackIconMaterialKind Kind
        {
            get => (PackIconMaterialKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }
        public readonly static DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialKind), typeof(IconButton));

        public bool Spin
        {
            get => (bool)GetValue(SpinProperty);
            set => SetValue(SpinProperty, value);
        }
        public readonly static DependencyProperty SpinProperty =
            DependencyProperty.Register(nameof(Spin), typeof(bool), typeof(IconButton));
    }
}
