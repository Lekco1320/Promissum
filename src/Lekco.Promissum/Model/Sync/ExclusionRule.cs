using Lekco.Promissum.Model.Sync.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// Rule for excluding files during sync operations.
    /// </summary>
    [DataContract]
    public partial class ExclusionRule : INotifyPropertyChanged
    {
        /// <summary>
        /// Indicates whether enable search pattern.
        /// </summary>
        [DataMember]
        public bool EnableSearchPattern { get; set; }

        /// <summary>
        /// Search pattern.
        /// </summary>
        [DataMember]
        public string SearchPattern { get; set; }

        /// <summary>
        /// Indicates whether enable extension filter.
        /// </summary>
        [DataMember]
        public bool EnableExtensionFilter { get; set; }

        /// <summary>
        /// Extension filter.
        /// </summary>
        [DataMember]
        public List<string> ExtensionFilter { get; set; }

        /// <summary>
        /// Indicates whether enable regex pattern.
        /// </summary>
        [DataMember]
        public bool EnableRegexPattern { get; set; }

        /// <summary>
        /// Regex pattern.
        /// </summary>
        [DataMember]
        public string RegexPattern { get; set; }

        /// <summary>
        /// Indicates whether enable size filter.
        /// </summary>
        [DataMember]
        public bool EnableSizeFilter { get; set; }

        /// <summary>
        /// Min limit of size filter.
        /// </summary>
        [DataMember]
        public long SizeMinLimit { get; set; }

        /// <summary>
        /// Max limit of size filter.
        /// </summary>
        [DataMember]
        public long SizeMaxLimit { get; set; }

        /// <summary>
        /// Regex form of search pattern.
        /// </summary>
        protected Regex SearchPatternRegex
        {
            get
            {
                _searchPatternRegex ??= FileSystemBase.WildcardToRegex(SearchPattern);
                return _searchPatternRegex;
            }
        }
        private Regex? _searchPatternRegex;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        public ExclusionRule()
        {
            SearchPattern = "*";
            RegexPattern = string.Empty;
            ExtensionFilter = new List<string>();
            SizeMaxLimit = long.MaxValue;
        }

        /// <summary>
        /// Indicates whether given file matches the rule.
        /// </summary>
        /// <param name="file">Given file.</param>
        /// <returns><see langword="true"/> if matches; otherwise, returns <see langword="false"/>.</returns>
        public bool Matches(FileBase file)
        {
            if (EnableSearchPattern && !file.MatchWildcardRegex(SearchPatternRegex))
            {
                return false;
            }
            if (EnableExtensionFilter)
            {
                foreach (var extension in ExtensionFilter)
                {
                    if (file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        goto Regex;
                    }
                }
                return false;
            }
        Regex:
            if (EnableRegexPattern && !Regex.IsMatch(file.FullName, RegexPattern))
            {
                return false;
            }
            if (EnableSizeFilter && (file.Size < SizeMinLimit || file.Size > SizeMaxLimit))
            {
                return false;
            }
            return true;
        }
    }
}
