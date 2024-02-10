using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class SyncExclusion : INotifyPropertyChanged
    {
        [DataMember]
        public bool UseSearchPattern
        {
            get => _useSearchPattern;
            set
            {
                _useSearchPattern = value;
                OnPropertyChanged(nameof(UseSearchPattern));
            }
        }
        private bool _useSearchPattern;

        [DataMember]
        public string SearchPattern
        {
            get => _searchPattern;
            set
            {
                _searchPattern = value;
                OnPropertyChanged(nameof(SearchPattern));
            }
        }
        private string _searchPattern;

        [DataMember]
        public bool RestrictExtensions
        {
            get => _restrictExtensions;
            set
            {
                _restrictExtensions = value;
                OnPropertyChanged(nameof(RestrictExtensions));
            }
        }
        private bool _restrictExtensions;

        [DataMember]
        public List<string> RestrictedExtensions
        {
            get => _restrictedExtensions;
            protected set
            {
                _restrictedExtensions = value;
                OnPropertyChanged(nameof(RestrictedExtensions));
            }
        }
        private List<string> _restrictedExtensions;

        [DataMember]
        public bool UseRegexPattern
        {
            get => _useRegexPattern;
            set
            {
                _useRegexPattern = value;
                OnPropertyChanged(nameof(UseRegexPattern));
            }
        }
        private bool _useRegexPattern;

        [DataMember]
        public string RegexPattern
        {
            get => _regexPattern;
            set
            {
                _regexPattern = value;
                OnPropertyChanged(nameof(RegexPattern));
            }
        }
        private string _regexPattern;

        [DataMember]
        public bool RestrictSize
        {
            get => _restrictSize;
            set
            {
                _restrictSize = value;
                OnPropertyChanged(nameof(RestrictSize));
            }
        }
        private bool _restrictSize;

        [DataMember]
        public long MinSize
        {
            get => _minSize;
            set
            {
                _minSize = value;
                OnPropertyChanged(nameof(MinSize));
            }
        }
        private long _minSize;

        [DataMember]
        public long MaxSize
        {
            get => _maxSize;
            set
            {
                _maxSize = value;
                OnPropertyChanged(nameof(MaxSize));
            }
        }
        private long _maxSize;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public SyncExclusion()
        {
            _useSearchPattern = false;
            _searchPattern = "*";
            _useRegexPattern = false;
            _regexPattern = "";
            _restrictExtensions = false;
            _restrictedExtensions = new List<string>();
            _minSize = 0;
            _maxSize = long.MaxValue;
        }

        public void SetRestrictedExtensions(string restrictExtensions)
        {
            RestrictedExtensions = restrictExtensions.Split(',').Select(x =>
            {
                string ret = x.Trim();
                return ret.Replace("*", null);
            }).ToList();
        }

        public ICollection<FileInfo> MatchedFiles(DirectoryInfo directory)
        {
            var result = new List<FileInfo>();
            foreach (var file in directory.GetFiles(SearchPattern))
            {
                if (Match(file))
                {
                    result.Add(file);
                }
            }
            return result;
        }

        public bool Match(FileInfo fileInfo)
        {
            if (RestrictExtensions)
            {
                foreach (var extension in RestrictedExtensions)
                {
                    if (fileInfo.Name.ToLower().EndsWith(extension.ToLower()))
                    {
                        break;
                    }
                    return false;
                }
            }
            if (UseRegexPattern && !Regex.IsMatch(fileInfo.FullName, RegexPattern))
            {
                return false;
            }
            if (RestrictSize && (fileInfo.Length < MinSize || fileInfo.Length > MaxSize))
            {
                return false;
            }
            return true;
        }
    }
}
