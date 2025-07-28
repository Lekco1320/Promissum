using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;

namespace Lekco.Promissum.Control
{
    public class MaterialIconButton : Button
    {
        static MaterialIconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MaterialIconButton), new FrameworkPropertyMetadata(typeof(MaterialIconButton)));
        }

        public PackIconMaterialKind Kind
        {
            get => (PackIconMaterialKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }
        public readonly static DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialKind), typeof(MaterialIconButton));

        public bool Spin
        {
            get => (bool)GetValue(SpinProperty);
            set => SetValue(SpinProperty, value);
        }
        public readonly static DependencyProperty SpinProperty =
            DependencyProperty.Register(nameof(Spin), typeof(bool), typeof(MaterialIconButton));
    }
}
