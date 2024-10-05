using System;

namespace Lekco.Wpf.Utility.Progress
{
    /// <summary>
    /// Interface to implements the ability to manage reported progress.
    /// </summary>
    public interface IProgressManager
    {
        /// <summary>
        /// Occurs when progress begined.
        /// </summary>
        public void ProgressBegin(object? sender, EventArgs e);

        /// <summary>
        /// Occurs when progress ended.
        /// </summary>
        public void ProgressEnd(object? sender, EventArgs e);
    }
}
