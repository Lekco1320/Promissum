namespace Lekco.Wpf.Utility
{
    /// <summary>
    /// Interface for implementing dialog functionality in applications of Lekco.Wpf.
    /// </summary>
    public interface IDialog
    {
        /// <summary>
        /// Tthe startup location of the dialog.
        /// </summary>
        public DialogStartUpLocation DialogStartUpLocation { get; }

        /// <summary>
        /// Initializes and displays the dialog.
        /// </summary>
        public void StartUp();
    }

    /// <summary>
    /// Specifies the startup location for a dialog.
    /// </summary>
    public enum DialogStartUpLocation
    {
        /// <summary>
        /// The center position of the window.
        /// </summary>
        CenterWindow,

        /// <summary>
        /// The position of bottom-left corner of the window.
        /// </summary>
        LeftBottom,

        /// <summary>
        /// The position of bottom-right corner of the window.
        /// </summary>
        RightBottom,

        /// <summary>
        /// The position of top-left corner of the window.
        /// </summary>
        LeftTop,

        /// <summary>
        /// The position of top-right corner of the window.
        /// </summary>
        RightTop,
    }
}
