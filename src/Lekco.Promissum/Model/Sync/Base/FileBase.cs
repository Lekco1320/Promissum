using Lekco.Promissum.Model.Sync.Record;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for file for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName}")]
    public abstract class FileBase : FileSystemBase, INotifyPropertyChanged
    {
        /// <summary>
        /// The file's name without extension.
        /// </summary>
        public abstract string NameWithoutExtension { get; }

        /// <summary>
        /// Extension of the file.
        /// </summary>
        public abstract string Extension { get; }

        /// <summary>
        /// Size of the file.
        /// </summary>
        [DataMember]
        public long Size { get; protected set; }

        /// <summary>
        /// Creation time of the file.
        /// </summary>
        [DataMember]
        public DateTime CreationTime { get; protected set; }

        /// <summary>
        /// Stream of the file.
        /// </summary>
        public abstract FileStream Stream { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fullName">Full name of the file.</param>
        protected FileBase(string fullName)
            : base(fullName)
        {
            FullName = fullName;
        }

        /// <summary>
        /// Compare with other file to judge which one is newer.
        /// </summary>
        /// <param name="otherFile">Other file.</param>
        /// <param name="compareMode">The compare mode.</param>
        /// <returns><see langword="true"/> when the mode matches; otherwise, returns <see langword="false"/>.</returns>
        public virtual bool CompareTo(FileBase otherFile, FileCompareMode compareMode)
        {
            return compareMode switch
            {
                FileCompareMode.IsNewer => LastWriteTime > otherFile.LastWriteTime,
                FileCompareMode.IsOlder => LastWriteTime < otherFile.LastWriteTime,
                FileCompareMode.IsLarger => Size > otherFile.Size,
                FileCompareMode.IsSmaller => Size < otherFile.Size,
                _ => throw new ArgumentException("Unknown compare mode."),
            };
        }

        /// <summary>
        /// Get a path relative from a base directory.
        /// </summary>
        /// <param name="baseDirectory">Base directory.</param>
        /// <returns>A relative path.</returns>
        public virtual string GetRelativePath(DirectoryBase baseDirectory)
            => Path.GetRelativePath(baseDirectory.FullName, FullName);

        /// <summary>
        /// Try to copy the file to a new location and potentially a new file name.
        /// </summary>
        /// <param name="file">New file.</param>
        /// <param name="exRecord">Exception record when operation fails.</param>
        /// <returns><see langword="true"/> when copy successfully; otherwise, returns <see langword="false"/>.</returns>
        public abstract bool TryCopyTo(FileBase file, [MaybeNullWhen(true)] out ExceptionRecord exRecord);

        /// <summary>
        /// Try to move the file to a new location and potentially a new file name.
        /// </summary>
        /// <param name="file">New file.</param>
        /// <param name="overwrite">Indicates whether needs to overwrite.</param>
        /// <param name="exRecord">Exception record when operation fails.</param>
        /// <returns><see langword="true"/> when move successfully; otherwise, returns <see langword="false"/>.</returns>
        public abstract bool TryMoveTo(FileBase file, bool overwrite, [MaybeNullWhen(true)] out ExceptionRecord exRecord);

        /// <summary>
        /// Try to delete the entity.
        /// </summary>
        /// <param name="exRecord">Exception record when operation fails.</param>
        /// <returns><see langword="true"/> when delete successfully; otherwise, returns <see langword="false"/>.</returns>
        public abstract bool TryDelete([MaybeNullWhen(true)] out ExceptionRecord exRecord);
    }

    /// <summary>
    /// Indicates the mode to compare two files.
    /// </summary>
    public enum FileCompareMode
    {
        /// <summary>
        /// Whether the file is newer.
        /// </summary>
        IsNewer,

        /// <summary>
        /// Whether the file is older.
        /// </summary>
        IsOlder,

        /// <summary>
        /// Whether the file is larger.
        /// </summary>
        IsLarger,

        /// <summary>
        /// Whether the file is smaller.
        /// </summary>
        IsSmaller,
    }
}
