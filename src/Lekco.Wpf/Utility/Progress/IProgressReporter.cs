using System;

namespace Lekco.Wpf.Utility.Progress
{
    /// <summary>
    /// Interface to implements the ability to report progress to corresponding UI components.
    /// </summary>
    public interface IProgressReporter
    {
        /// <summary>
        /// Raises when progress begins.
        /// </summary>
        public event EventHandler? OnProgressBegin;

        /// <summary>
        /// Raises when progress ends.
        /// </summary>
        public event EventHandler? OnProgressEnd;
    }
}
