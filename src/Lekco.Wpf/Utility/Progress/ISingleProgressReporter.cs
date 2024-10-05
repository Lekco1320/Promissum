using System;

namespace Lekco.Wpf.Utility.Progress
{
    public interface ISingleProgressReporter : IProgressReporter
    {
        /// <summary>
        /// The value of the progress, value should be limited in [0, 100].
        /// Progress will be indeterminate when value is <see cref="double.NaN" />.
        /// </summary>
        public event EventHandler<double>? OnProgressValueChanged;

        /// <summary>
        /// The text of the progress.
        /// </summary>
        public event EventHandler<string>? OnProgressTextChanged;
    }
}
