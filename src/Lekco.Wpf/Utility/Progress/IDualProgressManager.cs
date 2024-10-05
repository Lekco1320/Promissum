namespace Lekco.Wpf.Utility.Progress
{
    /// <summary>
    /// Interface to implements the ability to manage dual reported progress.
    /// </summary>
    public interface IDualProgressManager : IProgressManager
    {
        /// <summary>
        /// Occurs when the first progress's value changed.
        /// </summary>
        /// <param name="value">
        /// The first progress's current value. Value should be limited in [0, 100],
        /// and progress should be indeterminate when value is <see cref="double.NaN" />.
        /// </param>
        public void FirstProgressValueChanged(object? sender, double value);

        /// <summary>
        /// Occurs when the first progress's text chenged.
        /// </summary>
        /// <param name="text">The first progress's current text.</param>
        public void FirstProgressTextChanged(object? sender, string text);

        /// <summary>
        /// Occurs when the second progress's value changed.
        /// </summary>
        /// <param name="value">
        /// The second progress's current value. Value should be limited in [0, 100],
        /// and progress should be indeterminate when value is <see cref="double.NaN" />.
        /// </param>
        public void SecondProgressValueChanged(object? sender, double value);

        /// <summary>
        /// Occurs when the second progress's text chenged.
        /// </summary>
        /// <param name="text">The second progress's current text.</param>
        public void SecondProgressTextChanged(object? sender, string text);
    }
}
