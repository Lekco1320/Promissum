using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Lekco.Promissum.Model.Sync.Base;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// A comparer just compare the name of file, not full name.
    /// </summary>
    public class FileNameComparer : IEqualityComparer<FileSystemBase>
    {
        /// <inheritdoc />
        public bool Equals(FileSystemBase? x, FileSystemBase? y)
        {
            return x?.Name == y?.Name;
        }

        /// <inheritdoc />
        public int GetHashCode([DisallowNull] FileSystemBase file)
        {
            return file.Name.GetHashCode();
        }
    }
}
