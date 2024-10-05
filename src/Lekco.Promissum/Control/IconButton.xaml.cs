using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// IconButton.xaml 的交互逻辑
    /// </summary>
    public partial class IconButton : Button
    {
        public PackIconMaterialKind Kind
        {
            get => (PackIconMaterialKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }
        public readonly static DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(PackIconMaterialKind), typeof(IconButton));

        public bool Spin
        {
            get => (bool)GetValue(SpinProperty);
            set => SetValue(SpinProperty, value);
        }
        public readonly static DependencyProperty SpinProperty = DependencyProperty.Register(nameof(Spin), typeof(bool), typeof(IconButton));

        public IconButton()
        {
            InitializeComponent();
        }
    }
}
