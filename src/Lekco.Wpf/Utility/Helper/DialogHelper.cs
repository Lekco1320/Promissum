using Lekco.Wpf.Control;
using System;
using System.Windows;

namespace Lekco.Wpf.Utility.Helper
{
    public static class DialogHelper
    {
        public static bool ShowMessage(
            string message,
            MessageDialogIcon icon,
            MessageDialogButtonStyle buttonStyle,
            string? title = "消息",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            var dialog = new MessageDialog(
                message,
                icon,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            );
            dialog.SafelyDo(dialog.ShowDialog);
            return dialog.IsOK;
        }

        public static bool ShowMessageAsync(
            string message,
            MessageDialogIcon icon,
            MessageDialogButtonStyle buttonStyle,
            string? title = "消息",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            using var holder = new STAThreadHolder<MessageDialog>(() => new MessageDialog(
                message,
                icon,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            ));
            holder.SafelyDo(dialog => dialog.ShowDialog());
            return holder.SafelyDo(dialog => dialog.IsOK);
        }

        public static bool ShowError(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OK,
            string? title = "错误",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            var dialog = new MessageDialog(
                message,
                icon: MessageDialogIcon.Error,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            );
            dialog.SafelyDo(dialog.ShowDialog);
            return dialog.IsOK;
        }

        public static bool ShowError(
            Exception exception,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OK,
            string? title = "错误",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            string message =
                "发生了未经处理的异常：" + Environment.NewLine +
                exception.Message + Environment.NewLine +
                "堆栈跟踪：" + Environment.NewLine +
                exception.StackTrace;

            var dialog = new MessageDialog(
                message,
                icon: MessageDialogIcon.Error,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(460, 210),
                link,
                linkAction
            );
            dialog.SafelyDo(dialog.ShowDialog);
            return dialog.IsOK;
        }

        public static bool ShowErrorAsync(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OK,
            string? title = "错误",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            using var holder = new STAThreadHolder<MessageDialog>(() => new MessageDialog(
                message,
                icon: MessageDialogIcon.Error,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            ));
            holder.SafelyDo(dialog => dialog.ShowDialog());
            return holder.SafelyDo(dialog => dialog.IsOK);
        }

        public static bool ShowErrorAsync(
            Exception exception,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OK,
            string? title = "错误",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            string message =
                "发生了未经处理的异常：" + Environment.NewLine +
                exception.Message + Environment.NewLine +
                "堆栈跟踪：" + Environment.NewLine +
                exception.StackTrace;

            using var holder = new STAThreadHolder<MessageDialog>(() => new MessageDialog(
                message,
                icon: MessageDialogIcon.Error,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(460, 210),
                link,
                linkAction
            ));
            holder.SafelyDo(dialog => dialog.ShowDialog());
            return holder.SafelyDo(dialog => dialog.IsOK);
        }

        public static bool ShowWarning(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OKCancel,
            string? title = "警告",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            var dialog = new MessageDialog(
                message,
                icon: MessageDialogIcon.Warning,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            );
            dialog.SafelyDo(dialog.ShowDialog);
            return dialog.IsOK;
        }

        public static bool ShowWarningAsync(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OKCancel,
            string? title = "警告",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            using var holder = new STAThreadHolder<MessageDialog>(() => new MessageDialog(
                message,
                icon: MessageDialogIcon.Warning,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            ));
            holder.SafelyDo(dialog => dialog.ShowDialog());
            return holder.SafelyDo(dialog => dialog.IsOK);
        }

        public static bool ShowInformation(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OKCancel,
            string? title = "信息",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            var dialog = new MessageDialog(
                message,
                icon: MessageDialogIcon.Information,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            );
            dialog.SafelyDo(dialog.ShowDialog);
            return dialog.IsOK;
        }

        public static bool ShowInformationAsync(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OKCancel,
            string? title = "信息",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            using var holder = new STAThreadHolder<MessageDialog>(() => new MessageDialog(
                message,
                icon: MessageDialogIcon.Information,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            ));
            holder.SafelyDo(dialog => dialog.ShowDialog());
            return holder.SafelyDo(dialog => dialog.IsOK);
        }

        public static bool ShowSuccess(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OK,
            string? title = "完成",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            var dialog = new MessageDialog(
                message,
                icon: MessageDialogIcon.Success,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            );
            dialog.SafelyDo(dialog.ShowDialog);
            return dialog.IsOK;
        }

        public static bool ShowSuccessAsync(
            string message,
            MessageDialogButtonStyle buttonStyle = MessageDialogButtonStyle.OK,
            string? title = "完成",
            DialogStartUpLocation location = DialogStartUpLocation.CenterWindow,
            bool autoCountDown = false,
            Size? size = null,
            string? link = null,
            Action? linkAction = null
        )
        {
            using var holder = new STAThreadHolder<MessageDialog>(() => new MessageDialog(
                message,
                icon: MessageDialogIcon.Success,
                buttonStyle,
                title,
                location,
                autoCountDown,
                size ?? new Size(320, 150),
                link,
                linkAction
            ));
            holder.SafelyDo(dialog => dialog.ShowDialog());
            return holder.SafelyDo(dialog => dialog.IsOK);
        }
    }
}
