using Lekco.Promissum.Sync;
using Lekco.Promissum.Utility;
using Prism.Commands;
using Prism.Mvvm;
using System.Linq;
using System.Windows;

namespace Lekco.Promissum.ViewModel
{
    public class SyncExclusionWindowVM : BindableBase
    {
        public SyncExclusion? SyncExclusion { get; set; }
        public bool NewExclusion { get; set; }
        public bool UseSearchPattern { get; set; }
        public string SearchPattern { get; set; }
        public bool UseExtensions { get; set; }
        public string Extensions { get; set; }
        public bool UseRegexPattern { get; set; }
        public string RegexPattern { get; set; }
        public bool UseSizeRestriction { get; set; }
        public double MinSize { get; set; }
        public string MinSizeUnit { get; set; }
        public double MaxSize { get; set; }
        public string MaxSizeUnit { get; set; }
        public bool MaxSizeInfinity { get; set; }
        public DelegateCommand<Window> OKCommand { get; set; }
        public DelegateCommand<Window> CancelCommand { get; set; }

        public SyncExclusionWindowVM(SyncExclusion? syncExclusion)
        {
            NewExclusion = syncExclusion == null;
            SyncExclusion = syncExclusion ?? new SyncExclusion();
            UseSearchPattern = SyncExclusion.UseSearchPattern;
            SearchPattern = SyncExclusion.SearchPattern;
            UseExtensions = SyncExclusion.RestrictExtensions;
            Extensions = string.Join(", ", SyncExclusion.RestrictedExtensions);
            UseRegexPattern = SyncExclusion.UseRegexPattern;
            RegexPattern = SyncExclusion.RegexPattern ?? "";
            UseSizeRestriction = SyncExclusion.RestrictSize;
            Functions.FormatBytesLength(SyncExclusion.MinSize, out double minSize, out string minSizeUnit);
            MinSize = minSize;
            MinSizeUnit = minSizeUnit;
            MaxSizeInfinity = SyncExclusion.MaxSize == long.MaxValue;
            MaxSizeUnit = "";
            if (!MaxSizeInfinity)
            {
                Functions.FormatBytesLength(SyncExclusion.MaxSize, out double maxSize, out string maxSizeUnit);
                MaxSize = maxSize;
                MaxSizeUnit = maxSizeUnit;
            }

            OKCommand = new DelegateCommand<Window>(window => OK(window));
            CancelCommand = new DelegateCommand<Window>(window => Cancel(window));
        }

        public bool LoadSyncExclusion()
        {
            long minSize = Functions.FormatBytesLength(MinSize, MinSizeUnit);
            long maxSize = MaxSizeInfinity ? long.MaxValue : Functions.FormatBytesLength(MaxSize, MaxSizeUnit);
            if (minSize >= maxSize)
            {
                return false;
            }

            if (!UseSearchPattern || SearchPattern.Length == 0 || SearchPattern == "*")
            {
                SyncExclusion!.UseSearchPattern = false;
                SyncExclusion!.SearchPattern = "*";
            }
            else
            {
                SyncExclusion!.UseSearchPattern = true;
                SyncExclusion!.SearchPattern = SearchPattern;
            }
            SyncExclusion.RestrictExtensions = UseExtensions && Extensions.Length > 0;
            if (SyncExclusion.RestrictExtensions)
            {
                SyncExclusion.SetRestrictedExtensions(Extensions);
            }
            SyncExclusion.UseRegexPattern = UseRegexPattern && RegexPattern.Length > 0;
            if (UseRegexPattern && RegexPattern.Length > 0)
            {
                SyncExclusion.RegexPattern = RegexPattern;
            }
            SyncExclusion.RestrictSize = UseSizeRestriction;
            SyncExclusion.MinSize = minSize;
            SyncExclusion.MaxSize = maxSize;
            return true;
        }

        public void OK(Window window)
        {
            if (!LoadSyncExclusion())
            {
                MessageBox.Show("MinSize is greater than Maxsize.");
                return;
            }
            window.Close();
        }

        public void Cancel(Window window)
        {
            if (NewExclusion)
            {
                SyncExclusion = null;
            }
            window.Close();
        }
    }
}