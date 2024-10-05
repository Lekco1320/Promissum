using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using Lekco.Wpf.Utility.Progress;
using System;
using System.ComponentModel;
using System.Windows;

namespace Lekco.Wpf.Control
{
    /// <summary>
    /// DualProgress.xaml 的交互逻辑
    /// </summary>
    public partial class SingleProgressDialog : CustomWindow, IDialog, ISingleProgressManager, INotifyPropertyChanged
    {
        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        private double _value;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        private string _text;

        public DialogStartUpLocation DialogStartUpLocation { get; }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }
        private bool _isIndeterminate;

        protected ISingleProgressReporter reporter;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SingleProgressDialog(
            ISingleProgressReporter reporter,
            string title = "进度",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow
        ) {
            reporter.OnProgressBegin += ProgressBegin;
            reporter.OnProgressEnd += ProgressEnd;
            reporter.OnProgressValueChanged += ProgressValueChanged;
            reporter.OnProgressTextChanged += ProgressTextChanged;

            this.reporter = reporter;
            _text = "";
            Title = title;
            DialogStartUpLocation = location;

            DataContext = this;
            InitializeComponent();
        }

        public void ProgressBegin(object? sender, EventArgs e)
        {
            this.SafelyDo(Show);
        }

        public void ProgressEnd(object? sender, EventArgs e)
        {
            this.SafelyDo(() =>
            {
                reporter.OnProgressBegin -= ProgressBegin;
                reporter.OnProgressEnd -= ProgressEnd;
                reporter.OnProgressValueChanged -= ProgressValueChanged;
                reporter.OnProgressTextChanged -= ProgressTextChanged;

                Close();
            });
        }

        public void ProgressValueChanged(object? sender, double value)
        {
            this.SafelyDo(() =>
            {
                Value = double.IsNormal(value) ? value : 0d;
                IsIndeterminate = double.IsNaN(value);
            });
        }

        public void ProgressTextChanged(object? sender, string text)
        {
            this.SafelyDo(() => Text = text);
        }

        public void StartUp()
        {
            this.SafelyDo(() =>
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
            });
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            StartUp();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
