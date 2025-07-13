using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lekco.Wpf.Control
{
    [TemplatePart(Name = "PART_IncreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DecreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class NumericUpDown : Slider
    {
        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
            SmallChangeProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(1d));
        }

        private RepeatButton? _increaseButton;

        private RepeatButton? _decreaseButton;

        private TextBox? _textBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _increaseButton = GetTemplateChild("PART_IncreaseButton") as RepeatButton;
            _decreaseButton = GetTemplateChild("PART_DecreaseButton") as RepeatButton;
            _textBox = GetTemplateChild("PART_TextBox") as TextBox;

            if (_textBox != null)
            {
                _textBox.KeyDown += (s, e) =>
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        if (double.TryParse(_textBox.Text, out double newValue))
                        {
                            Value = newValue;
                        }
                        e.Handled = true;
                    }
                };
            }

            ValueChanged += (s, e) => UpdateButtonState();
            ValueChanged += (s, e) => ThresholdValue(e.NewValue);
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_increaseButton != null)
            {
                _increaseButton.IsEnabled = Value < Maximum;
            }
            if (_decreaseButton != null)
            {
                _decreaseButton.IsEnabled = Value > Minimum;
            }
        }

        private void ThresholdValue(double newValue)
        {
            if (newValue < Minimum)
            {
                Value = Minimum;
            }
            else if (newValue > Maximum)
            {
                Value = Maximum;
            }
        }
    }
}
