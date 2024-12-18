﻿using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for describing entity of a file system.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName}")]
    public abstract class FileSystemBase
    {
        /// <summary>
        /// Full name of the directory.
        /// </summary>
        [DataMember]
        public string FullName { get; protected set; }

        /// <summary>
        /// Name of directory.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Whether the directory exists.
        /// </summary>
        public abstract bool Exists { get; }

        /// <summary>
        /// Last write time of the entity.
        /// </summary>
        [DataMember]
        public abstract DateTime LastWriteTime { get; protected set; }

        /// <summary>
        /// The parent directory of the entity.
        /// </summary>
        public abstract DirectoryBase Parent { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fullName">Full name of the directory.</param>
        protected FileSystemBase(string fullName)
        {
            FullName = FormatFullName(fullName);
        }

        /// <summary>
        /// Format full name of a entity in a general format.
        /// </summary>
        /// <param name="fullName">Full name of this entity.</param>
        /// <returns>Full name of this entity in a general format.</returns>
        protected virtual string FormatFullName(string fullName)
        {
            if (fullName.EndsWith('\\') && !fullName.EndsWith(@":\"))
            {
                fullName = fullName[..^1];
            }
            if (fullName.StartsWith('\\'))
            {
                fullName = fullName[1..];
            }
            return fullName;
        }


        /// <inheritdoc />
        public override string ToString()
            => FullName;
    }
}
