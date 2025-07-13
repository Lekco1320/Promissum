using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for describing entity of a file system.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{FullName,nq}")]
    public abstract partial class FileSystemBase
    {
        /// <summary>
        /// Full name of the entity.
        /// </summary>
        [DataMember]
        public string FullName { get; protected set; }

        /// <summary>
        /// Name of entity.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Whether the entity exists.
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
        /// The parent drive of the entity.
        /// </summary>
        public abstract DriveBase Drive { get; }

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
        public static string FormatFullName(string fullName)
        {
            ReadOnlySpan<char> span = fullName;

            if (fullName.EndsWith('\\') && !fullName.EndsWith(@":\"))
            {
                span = span[..^1];
            }
            if (fullName.StartsWith('\\') && span.Length > 1)
            {
                span = span[1..];
            }
            return span.ToString();
        }

        /// <summary>
        /// Translate wildcard string to regex.
        /// </summary>
        /// <param name="wildcardStr">Wildcard string.</param>
        /// <param name="ignoreCase">Whether ingore case.</param>
        /// <returns>Regex form of given wildcard string.</returns>
        public static Regex WildcardToRegex(string wildcardStr, bool ignoreCase = true)
        {
            Regex replace = WildcardRegex();
            string regexStr = replace.Replace(wildcardStr, (Match m) => m.Value switch
            {
                "?" => ".?",
                "*" => ".*",
                _ => "\\" + m.Value,
            }) + "$";
            return new Regex(regexStr, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        /// <summary>
        /// Match <see cref="FullName"/> and given wildcard pattern in regex form.
        /// </summary>
        /// <param name="wildcardRegex">Given wildcard regex.</param>
        /// <returns><see langword="true"/> if matches; otherwise, returns <see langword="false"/>.</returns>
        public bool MatchWildcardRegex(Regex wildcardRegex)
            => wildcardRegex.IsMatch(FullName);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is FileSystemBase other)
            {
                return FullName == other.FullName;
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => FullName.GetHashCode();

        /// <inheritdoc />
        public override string ToString()
            => FullName;

        /// <inheritdoc />
        [GeneratedRegex("[.$^{\\[(|)*+?\\\\]")]
        private static partial Regex WildcardRegex();
    }
}
