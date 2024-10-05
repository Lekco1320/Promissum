using System.Windows.Input;

namespace Lekco.Wpf.Utility
{
    /// <summary>
    /// Interface for implementing interactive functions in applications of Lekco.Wpf.
    /// </summary>
    public interface IInteractive
    {
        /// <summary>
        /// Indicating whether the dialog result is OK.
        /// </summary>
        public bool IsOK { get; }

        /// <summary>
        /// The command executed when the OK button is clicked.
        /// </summary>
        public ICommand OKCommand { get; }

        /// <summary>
        /// The command executed when the Cancel button is clicked.
        /// </summary>
        public ICommand CancelCommand { get; }
    }
}
