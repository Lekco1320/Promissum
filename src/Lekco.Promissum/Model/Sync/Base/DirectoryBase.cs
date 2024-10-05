using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Lekco.Promissum.Model.Sync.Record;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for directory for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName}")]
    public abstract class DirectoryBase : FileSystemBase, INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fullName">Full name of the directory.</param>
        public DirectoryBase(string fullName)
            : base(FormatFullName(fullName))
        {
        }

        /// <summary>
        /// A local implement to get the name of a directory correctly.
        /// </summary>
        /// <param name="fullName">The full name of a directory.</param>
        /// <returns>The name of directory.</returns>
        protected virtual string GetName(string fullName)
        {
            if (string.IsNullOrEmpty(FullName))
            {
                return fullName;
            }

            int ptr = fullName.Length - 1;
            while (ptr >= 0 && fullName[ptr] != '\\')
            {
                --ptr;
            }
            ptr = ptr < 0 ? 0 : ptr;

            return fullName[(ptr + 1)..];
        }

        protected static string FormatFullName(string fullName)
        {
            if (fullName.EndsWith('\\') && !fullName.EndsWith(@":\"))
            {
                return fullName[..^1];
            }
            else
            {
                return fullName;
            }
        }

        /// <summary>
        /// Turn search pattern of wildcard to regex expression.
        /// </summary>
        /// <param name="searchPattern">Search pattern of wildcard.</param>
        /// <returns>Regex expression.</returns>
        protected static string WildcardToRegex(string searchPattern)
            => "^" + Regex.Escape(searchPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";

        /// <summary>
        /// Enumerates all files in the directory.
        /// </summary>
        /// <param name="searchPattern">Specified search pattern.</param>
        /// <returns>A list of files in directory.</returns>
        public abstract IEnumerable<FileBase> EnumerateFiles(string searchPattern = "*");

        /// <summary>
        /// Enumerates all directories in the directory.
        /// </summary>
        /// <param name="searchPattern">Specified search pattern.</param>
        /// <returns>A list of directories in directory.</returns>
        public abstract IEnumerable<DirectoryBase> EnumerateDirectories(string searchPattern = "*");

        /// <summary>
        /// Try to create the directory.
        /// </summary>
        /// <param name="exRecord">Exception record when operation fails.</param>
        /// <returns><see langword="true"/> when create successfully; otherwise, returns <see langword="false"/>.</returns>
        public abstract bool TryCreate([MaybeNullWhen(true)] out ExceptionRecord exRecord);

        /// <summary>
        /// Try to delete the directory.
        /// </summary>
        /// <param name="exRecord">Exception record when operation fails.</param>
        /// <returns><see langword="true"/> when delete successfully; otherwise, returns <see langword="false"/>.</returns>
        public abstract bool TryDelete([MaybeNullWhen(true)] out ExceptionRecord exRecord);
    }
}
