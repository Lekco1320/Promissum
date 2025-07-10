using Lekco.Wpf.MVVM.Command;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Lekco.Wpf.Control
{
    /// <summary>
    /// The custom window class for Lekco WPF applications.
    /// </summary>
    public class CustomWindow : Window
    {
        /// <summary>
        /// Initialize static fields.
        /// </summary>
        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        /// <summary>
        /// Indicates whether enable minimizing the window.
        /// </summary>
        public bool EnableMinimize
        {
            get => (bool)GetValue(EnableMinimizeProperty);
            set => SetValue(EnableMinimizeProperty, value);
        }
        public readonly static DependencyProperty EnableMinimizeProperty
            = DependencyProperty.Register(nameof(EnableMinimize), typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether enable maximizing the window.
        /// </summary>
        public bool EnableMaximize
        {
            get => (bool)GetValue(EnableMaximizeProperty);
            set => SetValue(EnableMaximizeProperty, value);
        }
        public readonly static DependencyProperty EnableMaximizeProperty
            = DependencyProperty.Register(nameof(EnableMaximize), typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether enable closing the window.
        /// </summary>
        public bool EnableClose
        {
            get => (bool)GetValue(EnableCloseProperty);
            set => SetValue(EnableCloseProperty, value);
        }
        public readonly static DependencyProperty EnableCloseProperty
            = DependencyProperty.Register(nameof(EnableClose), typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        /// <summary>
        /// Indicates whether enable restoring the window.
        /// </summary>
        public bool EnableRestore
        {
            get => (bool)GetValue(EnableRestoreProperty);
            set => SetValue(EnableRestoreProperty, value);
        }
        public readonly static DependencyProperty EnableRestoreProperty
            = DependencyProperty.Register(nameof(EnableRestore), typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        /// <summary>
        /// The command for overriding the behavior of minimizing the window.
        /// </summary>
        public ICommand MinimizeCommand
        {
            get => (ICommand)GetValue(MinimizeCommandProperty);
            set => SetValue(MinimizeCommandProperty, value);
        }
        public readonly static DependencyProperty MinimizeCommandProperty
            = DependencyProperty.Register(nameof(MinimizeCommand), typeof(ICommand), typeof(CustomWindow));

        /// <summary>
        /// The command's parameter for overriding the behavior of minimizing the window.
        /// </summary>
        public object MinimizeCommandParameter
        {
            get => (object)GetValue(MinimizeCommandParameterProperty);
            set => SetValue(MinimizeCommandParameterProperty, value);
        }
        public readonly static DependencyProperty MinimizeCommandParameterProperty
            = DependencyProperty.Register(nameof(MinimizeCommandParameter), typeof(object), typeof(CustomWindow));

        /// <summary>
        /// The command's target for overriding the behavior of minimizing the window.
        /// </summary>
        public IInputElement MinimizeCommandTarget
        {
            get => (IInputElement)GetValue(MinimizeCommandTargetProperty);
            set => SetValue(MinimizeCommandTargetProperty, value);
        }
        public readonly static DependencyProperty MinimizeCommandTargetProperty
            = DependencyProperty.Register(nameof(MinimizeCommandTarget), typeof(IInputElement), typeof(CustomWindow));

        /// <summary>
        /// The command for overriding the behavior of maximizing the window.
        /// </summary>
        public ICommand MaximizeCommand
        {
            get => (ICommand)GetValue(MaximizeCommandProperty);
            set => SetValue(MaximizeCommandProperty, value);
        }
        public readonly static DependencyProperty MaximizeCommandProperty
            = DependencyProperty.Register(nameof(MaximizeCommand), typeof(ICommand), typeof(CustomWindow));

        /// <summary>
        /// The command's parameter for overriding the behavior of maximizing the window.
        /// </summary>
        public object MaximizeCommandParameter
        {
            get => (object)GetValue(MaximizeCommandParameterProperty);
            set => SetValue(MaximizeCommandParameterProperty, value);
        }
        public readonly static DependencyProperty MaximizeCommandParameterProperty
            = DependencyProperty.Register(nameof(MaximizeCommandParameter), typeof(object), typeof(CustomWindow));

        /// <summary>
        /// The command's target for overriding the behavior of maximizing the window.
        /// </summary>
        public IInputElement MaximizeCommandTarget
        {
            get => (IInputElement)GetValue(MaximizeCommandTargetProperty);
            set => SetValue(MaximizeCommandTargetProperty, value);
        }
        public readonly static DependencyProperty MaximizeCommandTargetProperty
            = DependencyProperty.Register(nameof(MaximizeCommandTarget), typeof(IInputElement), typeof(CustomWindow));

        /// <summary>
        /// The command for overriding the behavior of closing the window.
        /// </summary>
        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }
        public readonly static DependencyProperty CloseCommandProperty
            = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(CustomWindow));

        /// <summary>
        /// The command's parameter for overriding the behavior of closing the window.
        /// </summary>
        public object CloseCommandParameter
        {
            get => (object)GetValue(CloseCommandParameterProperty);
            set => SetValue(CloseCommandParameterProperty, value);
        }
        public readonly static DependencyProperty CloseCommandParameterProperty
            = DependencyProperty.Register(nameof(CloseCommandParameter), typeof(object), typeof(CustomWindow));

        /// <summary>
        /// The command's target for overriding the behavior of closing the window.
        /// </summary>
        public IInputElement CloseCommandTarget
        {
            get => (IInputElement)GetValue(CloseCommandTargetProperty);
            set => SetValue(CloseCommandTargetProperty, value);
        }
        public readonly static DependencyProperty CloseCommandTargetProperty
            = DependencyProperty.Register(nameof(CloseCommandTarget), typeof(IInputElement), typeof(CustomWindow));

        /// <summary>
        /// The command for overriding the behavior of restoring the window.
        /// </summary>
        public ICommand RestoreCommand
        {
            get => (ICommand)GetValue(RestoreCommandProperty);
            set => SetValue(RestoreCommandProperty, value);
        }
        public readonly static DependencyProperty RestoreCommandProperty
            = DependencyProperty.Register(nameof(RestoreCommand), typeof(ICommand), typeof(CustomWindow));

        /// <summary>
        /// The command's parameter for overriding the behavior of restoring the window.
        /// </summary>
        public object RestoreCommandParameter
        {
            get => (object)GetValue(RestoreCommandParameterProperty);
            set => SetValue(RestoreCommandParameterProperty, value);
        }
        public readonly static DependencyProperty RestoreCommandParameterProperty
            = DependencyProperty.Register(nameof(RestoreCommandParameter), typeof(object), typeof(CustomWindow));

        /// <summary>
        /// The command's target for overriding the behavior of restoring the window.
        /// </summary>
        public IInputElement RestoreCommandTarget
        {
            get => (IInputElement)GetValue(RestoreCommandTargetProperty);
            set => SetValue(RestoreCommandTargetProperty, value);
        }
        public readonly static DependencyProperty RestoreCommandTargetProperty
            = DependencyProperty.Register(nameof(RestoreCommandTarget), typeof(IInputElement), typeof(CustomWindow));

        /// <summary>
        /// Create an instance.
        /// </summary>
        public CustomWindow()
        {
            WindowChrome.SetWindowChrome(this, new WindowChrome()
            {
                CaptionHeight = SystemParameters.WindowNonClientFrameThickness.Top + 3,
                GlassFrameThickness = new Thickness(1),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness(5),
                UseAeroCaptionButtons = false,
            });

            MinimizeCommand = new RelayCommand(() => WindowState = WindowState.Minimized);
            CloseCommand = new RelayCommand(Close);
            MaximizeCommand = new RelayCommand(() => WindowState = WindowState.Maximized);
            RestoreCommand = new RelayCommand(() => WindowState = WindowState.Normal);
        }
    }
}
