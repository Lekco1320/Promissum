using System.Diagnostics;

namespace Lekco.Wpf.Utility
{
    /// <summary>
    /// Represents a pair of items.
    /// </summary>
    /// <typeparam name="T1">Type of <see cref="Item1"/>.</typeparam>
    /// <typeparam name="T2">Type of <see cref="Item2"/>.</typeparam>
    [DebuggerDisplay("{Item1}, {Item2}")]
    public class Pair<T1, T2>
    {
        /// <summary>
        /// First item.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        /// Second item.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="item1">First item.</param>
        /// <param name="item2">Second item.</param>
        public Pair(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
