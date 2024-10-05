using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using System;
using System.Windows;
using System.Windows.Input;

namespace Lekco.Wpf.Control
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : CustomWindow, IDialog, IInteractive
    {
        public string Description { get; }

        public string Input { get; set; }

        public Predicate<string>? Validator { get; }

        public Func<string, string>? Messager { get; }

        public Converter<string, object>? Converter { get; }

        public object? Result { get; protected set; }

        public bool IsOK { get; protected set; }

        public ICommand OKCommand => new RelayCommand(OK);

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public DialogStartUpLocation DialogStartUpLocation { get; }

        protected InputDialog(
            string description,
            string? title,
            Predicate<string>? validator,
            Func<string, string>? messager,
            Converter<string, object>? converter,
            DialogStartUpLocation location
        )
        {
            Input = "";
            Validator = validator;
            Description = description;
            Messager = messager;
            Converter = converter;
            DialogStartUpLocation = location;

            InitializeComponent();
            DataContext = this;

            Title = title;
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

        private bool Validate()
        {
            if (!Validator?.Invoke(Input) ?? false)
            {
                DialogHelper.ShowError(Messager?.Invoke(Input) ?? $"非法输入：\"{Input}\"。");
                return false;
            }
            Result = Input;
            return true;
        }

        private bool Convert()
        {
            if (Converter == null)
            {
                return true;
            }

            try
            {
                Result = Converter.Invoke(Input);
            }
            catch
            {
                DialogHelper.ShowError(Messager?.Invoke(Input) ?? $"目标转换失败：\"{Input}\"。");
                return false;
            }

            return true;
        }

        private void OK()
        {
            if (Validate() && Convert())
            {
                IsOK = true;
                Close();
                return;
            }
            DialogHelper.ShowError(Messager?.Invoke(Input) ?? $"非法输入：\"{Input}\"。");
        }

        private void Cancel()
            => Close();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            StartUp();
        }

        public static bool ShowDialog<T>(
            string description,
            out T result,
            string? title = null,
            Predicate<string>? validator = null,
            Func<string, string>? messager = null,
            Converter<string, T>? converter = null,
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow
        )
        {
            Converter<string, object>? cvt = converter == null ? null : (s) => converter(s)!;
            var dialog = new InputDialog(description, title, validator, messager, cvt, location);
            dialog.ShowDialog();
#nullable disable
            result = dialog.IsOK ? (T)dialog.Result : default;
#nullable enable
            return dialog.IsOK;
        }
    }
}
