using System;
using System.ComponentModel;
using System.Windows;

namespace Lekco.Wpf.Control
{
    public class DateTimePicker : System.Windows.Controls.Control, INotifyPropertyChanged
    {
        static DateTimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker), new FrameworkPropertyMetadata(typeof(DateTimePicker)));
        }

        public DateTime SelectedDateTime
        {
            get => (DateTime)GetValue(SelectedDateTimeProperty);
            set => SetValue(SelectedDateTimeProperty, value);
        }
        public readonly static DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register(nameof(SelectedDateTime), typeof(DateTime), typeof(DateTimePicker));

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }
        public readonly static DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime), typeof(DateTimePicker),
                new PropertyMetadata((sender, e) => ((DateTimePicker)sender).UpdateSelectedDateTime()));

        public string Text => _text;
        private string _text = string.Empty;

        public int Hour
        {
            get => _hour;
            set
            {
                if (_hour == value)
                {
                    return;
                }
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 23)
                {
                    value = 23;
                }
                _hour = value;
                OnPropertyChanged(nameof(Hour));
                UpdateSelectedDateTime();
            }
        }
        private int _hour;

        public int Minute
        {
            get => _minute;
            set
            {
                if (_minute == value)
                {
                    return;
                }
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 59)
                {
                    value = 59;
                }
                _minute = value;
                OnPropertyChanged(nameof(Minute));
                UpdateSelectedDateTime();
            }
        }
        private int _minute;

        public int Second
        {
            get => _second;
            set
            {
                if (_second == value)
                {
                    return;
                }
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 59)
                {
                    value = 59;
                }
                _second = value;
                OnPropertyChanged(nameof(Second));
                UpdateSelectedDateTime();
            }
        }
        private int _second;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void UpdateSelectedDateTime()
        {
            SelectedDateTime = new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, Hour, Minute, Second);
            _text = SelectedDateTime.ToString();
            OnPropertyChanged(nameof(Text));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _text = SelectedDateTime.ToString();
            OnPropertyChanged(nameof(Text));
        }

        protected internal virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
