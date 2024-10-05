using System;

namespace Lekco.Wpf.Utility
{
    /// <summary>
    /// Specifies an index for explicitly defining the index of a field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// Explicitly defined index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="index">Explicitly defined index.</param>
        public IndexAttribute(int index)
        {
            Index = index;
        }
    }
}
