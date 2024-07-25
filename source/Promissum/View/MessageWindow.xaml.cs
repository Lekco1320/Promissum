using Lekco.Promissum.Apps;
using Lekco.Promissum.Control;
using Lekco.Promissum.Utility;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Lekco.Promissum.View
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow : CustomWindow, INotifyPropertyChanged
    {
        public string Caption { get; set; }
        public string Message { get; set; }
        public string? Link { get; set; }
        public Visibility OKButtonVisibility { get; set; }
        public Visibility CancelButtonVisibility { get; set; }
        public Visibility CountDownVisibility { get; set; }
        public MessageWindowIcon WindowIcon { get; set; }
        public MessageWindowButtonStyle WindowButtonStyle { get; set; }
        public MessageWindowLocation WindowLocation { get; protected set; }
        public BitmapImage ImageSource { get; set; }
        public DelegateCommand OKCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand? LinkCommand { get; set; }
        public bool AutoCountDown { get; protected set; }
        public bool ReturnValue { get; set; }

        public string CountDownString
        {
            get => _countDownString;
            set
            {
                _countDownString = value;
                OnPropertyChanged(nameof(CountDownString));
            }
        }
        private string _countDownString;

        private bool _mandatoryTopMost;
        private int _remainSeconds = Config.MessageWindowWaitingSeconds;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MessageWindow(
            string message,
            MessageWindowIcon icon,
            MessageWindowButtonStyle buttonStyle,
            string? caption,
            MessageWindowLocation location,
            bool autoCountDown,
            bool mandatoryTopMost,
            double width,
            double height,
            string? link,
            Action? linkAction)
        {
            Message = message;
            WindowIcon = icon;
            WindowLocation = location;
            WindowButtonStyle = buttonStyle;
            AutoCountDown = autoCountDown;
            _mandatoryTopMost = mandatoryTopMost;
            _countDownString = "";
            Width = width;
            Height = height;
            CountDownVisibility = AutoCountDown ? Visibility.Visible : Visibility.Collapsed;

            if (caption != null)
            {
                Caption = caption;
            }
            else
            {
                Caption = WindowIcon switch
                {
                    MessageWindowIcon.Information => "提示",
                    MessageWindowIcon.Warning => "警告",
                    MessageWindowIcon.Error => "错误",
                    MessageWindowIcon.Success => "提示",
                    _ => throw new NotSupportedException()
                };
            }

            if (link != null && linkAction != null)
            {
                Link = link;
                LinkCommand = new DelegateCommand(linkAction);
            }

            ImageSource = icon switch
            {
                MessageWindowIcon.Information => new BitmapImage(new Uri("pack://application:,,,/Resources/Information.png")),
                MessageWindowIcon.Warning => new BitmapImage(new Uri("pack://application:,,,/Resources/Warning.png")),
                MessageWindowIcon.Error => new BitmapImage(new Uri("pack://application:,,,/Resources/Error.png")),
                MessageWindowIcon.Success => new BitmapImage(new Uri("pack://application:,,,/Resources/Success.png")),
                _ => throw new NotSupportedException(),
            };
            OKButtonVisibility = buttonStyle switch
            {
                MessageWindowButtonStyle.OK => Visibility.Visible,
                MessageWindowButtonStyle.Cancel => Visibility.Collapsed,
                MessageWindowButtonStyle.OKCancel => Visibility.Visible,
                _ => throw new NotSupportedException(),
            };
            CancelButtonVisibility = buttonStyle switch
            {
                MessageWindowButtonStyle.OK => Visibility.Collapsed,
                MessageWindowButtonStyle.Cancel => Visibility.Visible,
                MessageWindowButtonStyle.OKCancel => Visibility.Visible,
                _ => throw new NotSupportedException(),
            };

            OKCommand = new DelegateCommand(OK, () => buttonStyle != MessageWindowButtonStyle.Cancel);
            CancelCommand = new DelegateCommand(Cancel, () => buttonStyle != MessageWindowButtonStyle.OK);

            InitializeComponent();
            DataContext = this;
        }

        public void SetDefaultReturnValue()
        {
            ReturnValue = WindowButtonStyle switch
            {
                MessageWindowButtonStyle.OK => true,
                MessageWindowButtonStyle.Cancel => false,
                MessageWindowButtonStyle.OKCancel => false,
                _ => throw new NotSupportedException(),
            };
        }

        private void OK()
        {
            ReturnValue = true;
            Close();
        }

        private void Cancel()
        {
            ReturnValue = false;
            Close();
        }

        public new void Close()
        {
            Dispatcher.BeginInvoke(() =>
            {
                base.Close();
            });
        }

        public new void Show()
        {
            if (AutoCountDown)
            {
                CountDown();
            }
            base.Show();
        }

        public new void ShowDialog()
        {
            if (AutoCountDown)
            {
                CountDown();
            }
            base.ShowDialog();
        }

        public async void CountDown()
        {
            while (_remainSeconds > 0)
            {
                if (_mandatoryTopMost)
                {
                    MandatoryTopMost();
                }
                CountDownString = $"{_remainSeconds--}秒后自动{(WindowButtonStyle == MessageWindowButtonStyle.OK ? "确定" : "取消")}";
                await Task.Delay(1000);
            }
            SetDefaultReturnValue();
            Close();
        }

        private void MandatoryTopMost()
        {
            Dispatcher.BeginInvoke(() =>
            {
                var handle = Functions.GetWindowHandle(this);
                Functions.SetTop(handle);
            });
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (WindowLocation == MessageWindowLocation.RightBottom)
            {
                var desktopWorkingArea = SystemParameters.WorkArea;
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
            }
            if (_mandatoryTopMost)
            {
                MandatoryTopMost();
            }
        }

        public static void Show(
            string message,
            MessageWindowIcon icon,
            MessageWindowButtonStyle buttonStyle,
            string? caption = null,
            MessageWindowLocation location = MessageWindowLocation.CenterWindow,
            bool autoCountDown = false,
            bool mandatoryTopMost = false,
            double width = 330,
            double height = 150,
            string? link = null,
            Action? linkAction = null)
        {
            var window = new MessageWindow(message, icon, buttonStyle, caption, location, autoCountDown, mandatoryTopMost, width, height, link, linkAction);
            window.Show();
        }

        public static bool ShowDialog(
            string message,
            MessageWindowIcon icon,
            MessageWindowButtonStyle buttonStyle,
            string? caption = null,
            MessageWindowLocation location = MessageWindowLocation.CenterWindow,
            bool autoCountDown = false,
            bool mandatoryTopMost = false,
            double width = 330,
            double height = 150,
            string? link = null,
            Action? linkAction = null)
        {
            var window = new MessageWindow(message, icon, buttonStyle, caption, location, autoCountDown, mandatoryTopMost, width, height, link, linkAction);
            window.ShowDialog();
            return window.ReturnValue;
        }
    }

    public enum MessageWindowIcon
    {
        Information,
        Warning,
        Error,
        Success,
    }

    public enum MessageWindowButtonStyle
    {
        OK,
        Cancel,
        OKCancel,
    }

    public enum MessageWindowLocation
    {
        CenterWindow,
        RightBottom,
    }
}
