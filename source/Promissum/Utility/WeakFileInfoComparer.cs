using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Lekco.Promissum.Utility
{
    public class WeakFileInfoComparer
        : IEqualityComparer<FileSystemInfo>
    {
        public bool Equals(FileSystemInfo? x, FileSystemInfo? y)
        {
            return x?.Name == y?.Name;
        }

        public int GetHashCode([DisallowNull] FileSystemInfo file)
        {
            return file.Name.GetHashCode();
        }
    }
}
