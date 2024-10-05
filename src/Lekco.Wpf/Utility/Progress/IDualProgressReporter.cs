using System;

namespace Lekco.Wpf.Utility.Progress
{
    /// <summary>
    /// Interface to implements the ability to report dual progress to corresponding UI components.
    /// </summary>
    public interface IDualProgressReporter : IProgressReporter
    {
        /// <summary>
        /// The value of the first progress, value should be limited in [0, 100].
        /// Progress will be indeterminate when value is <see cref="double.NaN" />.
        /// </summary>
        public event EventHandler<double>? OnFirstProgressValueChanged;

        /// <summary>
        /// The value of the second progress, value should be limited in [0, 100].
        /// Progress will be indeterminate when value is <see cref="double.NaN" />.
        /// </summary>
        public event EventHandler<double>? OnSecondProgressValueChanged;

        /// <summary>
        /// The text of the first progress.
        /// </summary>
        public event EventHandler<string>? OnFirstProgressTextChanged;

        /// <summary>
        /// The text of the second progress.
        /// </summary>
        public event EventHandler<string>? OnSecondProgressTextChanged;
    }
}
