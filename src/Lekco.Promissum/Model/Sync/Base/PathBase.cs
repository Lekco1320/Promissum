using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for describing source and target path for sync.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullPath,nq}")]
    public abstract class PathBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Drive of the path depends on.
        /// </summary>
        [DataMember]
        public DriveBase Drive { get; protected set; }

        /// <summary>
        /// Relative path to the root of drive.
        /// </summary>
        [DataMember]
        public string RelativePath { get; protected set; }

        /// <summary>
        /// Full name of the path.
        /// </summary>
        public abstract string FullPath { get; }

        /// <summary>
        /// The directory form of the path.
        /// </summary>
        public abstract DirectoryBase Directory { get; }

        /// <summary>
        /// Indicates whether the path's drive is ready.
        /// </summary>
        public bool IsReady => Drive.IsReady;

        /// <summary>
        /// Whether the path exists.
        /// </summary>
        public bool Exists => Directory.Exists;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="drive">Drive of the path depends on.</param>
        /// <param name="relativePath">Path relative from the root of drive.</param>
        protected PathBase(DriveBase drive, string relativePath)
        {
            Drive = drive;
            RelativePath = relativePath;
        }

        /// <summary>
        /// Returns a relative path from one path to another.
        /// </summary>
        /// <param name="relativeTo">The base path.</param>
        /// <param name="path">The other path.</param>
        /// <returns>A relative path.</returns>
        public virtual string GetRelativePath(string path)
        {
            string ret = Path.GetRelativePath(FullPath, path);
            return ret != "." ? ret : "";
        }

        /// <summary>
        /// Indicates whether given path is a sub path of the path.
        /// </summary>
        /// <param name="path">Given path.</param>
        /// <returns><see langword="true"/> if it is a sub path; otherwise, <see langword="false"/>.</returns>
        public virtual bool IsSubPath(string path)
            => path.StartsWith(RelativePath, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether given path is a sub path of the path.
        /// </summary>
        /// <param name="path">Given path.</param>
        /// <returns><see langword="true"/> if it is a sub path; otherwise, <see langword="false"/>.</returns>
        public virtual bool IsSubPath(PathBase path)
            => Drive.Equals(path.Drive) && IsSubPath(path.RelativePath);

        /// <summary>
        /// Get given file's relative path to this path.
        /// </summary>
        /// <param name="entity">Given entity of file system.</param>
        /// <returns>Relative path to this path.</returns>
        public abstract string GetRelativePath(FileSystemBase entity);

        /// <summary>
        /// Get a file of given path relative to the path.
        /// </summary>
        /// <param name="relativePath">A path relative to the path.</param>
        /// <returns>File of path relative to the path.</returns>
        public abstract FileBase GetFile(string relativePath);

        /// <summary>
        /// Get a directory of given path relative to the path.
        /// </summary>
        /// <param name="relativePath">A path relative to the path.</param>
        /// <returns>Directory of path relative to the path.</returns>
        public abstract DirectoryBase GetDirectory(string relativePath);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is PathBase other)
            {
                return Equals(Drive, other.Drive) && RelativePath == other.RelativePath;
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(Drive, RelativePath);

        /// <inheritdoc />
        public override string ToString()
            => FullPath;
    }
}
