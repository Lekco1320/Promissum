using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Lekco.Wpf.Control
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog : CustomWindow, IDialog, IInteractive, INotifyPropertyChanged
    {
        public string Message { get; }

        public string? Link { get; }

        public MessageDialogIcon DialogIcon { get; }

        public MessageDialogButtonStyle DialogButtonStyle { get; }

        public DialogStartUpLocation DialogStartUpLocation { get; }

        public ICommand OKCommand => new RelayCommand(OK);

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public RelayCommand? LinkCommand { get; }

        public bool AutoCountDown { get; }

        public bool IsOK { get; protected set; }

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

        private int _remainSeconds;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MessageDialog(
            string message,
            MessageDialogIcon icon,
            MessageDialogButtonStyle buttonStyle,
            string? title,
            DialogStartUpLocation location,
            bool autoCountDown,
            Size size,
            string? link,
            Action? linkAction)
        {
            InitializeComponent();

            Message = message;
            DialogIcon = icon;
            DialogStartUpLocation = location;
            DialogButtonStyle = buttonStyle;
            AutoCountDown = autoCountDown;
            Title = title;
            Link = link;
            LinkCommand = linkAction != null ? new RelayCommand(linkAction) : null;
            _remainSeconds = 30;
            _countDownString = "";
            Width = size.Width;
            Height = size.Height;

            DataContext = this;
            StartUp();
        }

        public void SetDefaultReturnValue()
        {
            IsOK = DialogButtonStyle switch
            {
                MessageDialogButtonStyle.OK => true,
                MessageDialogButtonStyle.Cancel => false,
                MessageDialogButtonStyle.OKCancel => false,
                _ => throw new InvalidOperationException(),
            };
        }

        private void OK()
        {
            IsOK = true;
            Close();
        }

        private void Cancel()
        {
            IsOK = false;
            Close();
        }

        protected new void Close()
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
                CountDownString = $"{_remainSeconds--}秒后自动{(DialogButtonStyle == MessageDialogButtonStyle.OK ? "确定" : "取消")}";
                await Task.Delay(1000);
            }
            SetDefaultReturnValue();
            Close();
        }

        public void StartUp()
        {
            var workingArea = SystemParameters.WorkArea;
            (Left, Top) = DialogStartUpLocation switch
            {
                DialogStartUpLocation.LeftTop => (workingArea.Left, workingArea.Top),
                DialogStartUpLocation.LeftBottom => (workingArea.Left, workingArea.Bottom - Height),
                DialogStartUpLocation.RightTop => (workingArea.Right - Width, workingArea.Top),
                DialogStartUpLocation.RightBottom => (workingArea.Right - Width, workingArea.Bottom - Height),
                DialogStartUpLocation.CenterWindow => (workingArea.Left + (workingArea.Width - Width) / 2, workingArea.Top + (workingArea.Height - Height) / 2),
                _ => (Left, Top),
            };
        }
    }

    public enum MessageDialogIcon
    {
        Information,
        Warning,
        Error,
        Success,
    }

    public enum MessageDialogButtonStyle
    {
        OK,
        Cancel,
        OKCancel,
    }
}
