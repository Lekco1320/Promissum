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
    public partial class DualProgressDialog : CustomWindow, IDialog, IDualProgressManager, INotifyPropertyChanged
    {
        public double FirstValue
        {
            get => _firstValue;
            set
            {
                _firstValue = value;
                OnPropertyChanged(nameof(FirstValue));
            }
        }
        private double _firstValue;

        public string FirstText
        {
            get => _firstText;
            set
            {
                _firstText = value;
                OnPropertyChanged(nameof(FirstText));
            }
        }
        private string _firstText;

        public bool FirstIsIndeterminate
        {
            get => _firstIsIndeterminate;
            set
            {
                _firstIsIndeterminate = value;
                OnPropertyChanged(nameof(FirstIsIndeterminate));
            }
        }
        private bool _firstIsIndeterminate;

        public double SecondValue
        {
            get => _secondValue;
            set
            {
                _secondValue = value;
                OnPropertyChanged(nameof(SecondValue));
            }
        }
        private double _secondValue;

        public string SecondText
        {
            get => _secondText;
            set
            {
                _secondText = value;
                OnPropertyChanged(nameof(SecondText));
            }
        }
        private string _secondText;

        public bool SecondIsIndeterminate
        {
            get => _secondIsIndeterminate;
            set
            {
                _secondIsIndeterminate = value;
                OnPropertyChanged(nameof(SecondIsIndeterminate));
            }
        }

        public DialogStartUpLocation DialogStartUpLocation { get; }

        private bool _secondIsIndeterminate;

        protected IDualProgressReporter reporter;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DualProgressDialog(
            IDualProgressReporter reporter,
            string title = "进度",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow
        ) {
            reporter.OnProgressBegin += ProgressBegin;
            reporter.OnProgressEnd += ProgressEnd;
            reporter.OnFirstProgressValueChanged += FirstProgressValueChanged;
            reporter.OnFirstProgressTextChanged += FirstProgressTextChanged;
            reporter.OnSecondProgressValueChanged += SecondProgressValueChanged;
            reporter.OnSecondProgressTextChanged += SecondProgressTextChanged;

            this.reporter = reporter;
            _firstText = _secondText = "";
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
                reporter.OnFirstProgressValueChanged -= FirstProgressValueChanged;
                reporter.OnFirstProgressTextChanged -= FirstProgressTextChanged;
                reporter.OnSecondProgressValueChanged -= SecondProgressValueChanged;
                reporter.OnSecondProgressTextChanged -= SecondProgressTextChanged;

                Close();
            });
        }

        public void FirstProgressValueChanged(object? sender, double value)
        {
            this.SafelyDo(() =>
            {
                FirstValue = double.IsNormal(value) ? value : 0d;
                FirstIsIndeterminate = double.IsNaN(value);
            });
        }

        public void FirstProgressTextChanged(object? sender, string text)
        {
            this.SafelyDo(() => FirstText = text);
        }

        public void SecondProgressValueChanged(object? sender, double value)
        {
            this.SafelyDo(() =>
            {
                SecondValue = double.IsNormal(value) ? value : 0d;
                SecondIsIndeterminate = double.IsNaN(value);
            });
        }

        public void SecondProgressTextChanged(object? sender, string text)
        {
            this.SafelyDo(() => SecondText = text);
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
