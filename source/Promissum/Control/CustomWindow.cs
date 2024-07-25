using Prism.Commands;
using System.Windows;
using System.Windows.Shell;

namespace Lekco.Promissum.Control
{
    public partial class CustomWindow : Window
    {
        public static readonly Style CustomWindowStyle;

        public bool ShowMinimizeWindowButton
        {
            get => (bool)GetValue(ShowMinimizeWindowButtonProperty);
            set => SetValue(ShowMinimizeWindowButtonProperty, value);
        }
        public readonly static DependencyProperty ShowMinimizeWindowButtonProperty = DependencyProperty.Register(nameof(ShowMinimizeWindowButton), typeof(bool), typeof(CustomWindow));

        public bool ShowCloseWindowButton
        {
            get => (bool)GetValue(ShowCloseWindowButtonProperty);
            set => SetValue(ShowCloseWindowButtonProperty, value);
        }
        public readonly static DependencyProperty ShowCloseWindowButtonProperty = DependencyProperty.Register(nameof(ShowCloseWindowButton), typeof(bool), typeof(CustomWindow));

        public bool ShowMaximizeRestoreWindowButton
        {
            get => (bool)GetValue(ShowMaximizeRestoreWindowButtonProperty);
            set => SetValue(ShowMaximizeRestoreWindowButtonProperty, value);
        }
        public readonly static DependencyProperty ShowMaximizeRestoreWindowButtonProperty = DependencyProperty.Register(nameof(ShowMaximizeRestoreWindowButton), typeof(bool), typeof(CustomWindow));

        public DelegateCommand MinimizeWindow { get; set; }
        public DelegateCommand CloseWindow { get; set; }
        public DelegateCommand MaximizeWindow { get; set; }
        public DelegateCommand RestoreWindow { get; set; }

        static CustomWindow()
        {
            CustomWindowStyle = (Style)Application.Current.Resources["CustomWindowStyle"];
        }

        public CustomWindow()
        {
            Style = CustomWindowStyle;
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = SystemParameters.WindowNonClientFrameThickness.Top + 3,
                GlassFrameThickness = new Thickness(1),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness(5),
                UseAeroCaptionButtons = false
            });

            MinimizeWindow = new DelegateCommand(() => WindowState = WindowState.Minimized);
            CloseWindow = new DelegateCommand(Close);
            MaximizeWindow = new DelegateCommand(() => WindowState = WindowState.Maximized);
            RestoreWindow = new DelegateCommand(() => WindowState = WindowState.Normal);
            ShowMinimizeWindowButton = true;
            ShowCloseWindowButton = true;
            ShowMaximizeRestoreWindowButton = true;
        }
    }
}
