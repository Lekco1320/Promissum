namespace Lekco.Wpf.Utility.Progress
{
    /// <summary>
    /// Interface to implements the ability to manage single reported progress.
    /// </summary>
    public interface ISingleProgressManager : IProgressManager
    {
        /// <summary>
        /// Occurs when the progress's value changed.
        /// </summary>
        /// <param name="value">
        /// The progress's current value. Value should be limited in [0, 100],
        /// and progress should be indeterminate when value is <see cref="double.NaN" />.
        /// </param>
        public void ProgressValueChanged(object? sender, double value);

        /// <summary>
        /// Occurs when the progress's text chenged.
        /// </summary>
        /// <param name="text">The progress's current text.</param>
        public void ProgressTextChanged(object? sender, string text);
    }
}
