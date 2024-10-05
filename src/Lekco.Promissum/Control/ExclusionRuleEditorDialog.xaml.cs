using Lekco.Promissum.Model.Sync;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    /// <summary>
    /// ExclusionRuleEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ExclusionRuleEditorDialog : CustomWindow, IInteractive
    {
        public ExclusionRule ExclusionRule { get; }

        public bool IsNew { get; }

        public bool IsOK { get; protected set; }

        public bool EnableSearchPattern { get; set; }

        public string SearchPattern { get; set; }

        public bool EnableExtensionFilter { get; set; }

        public string ExtensionFilter { get; set; }

        public bool EnableRegexPattern { get; set; }

        public string RegexPattern { get; set; }

        public bool EnableSizeFilter { get; set; }

        public double MinSize { get; set; }

        public int MinSizeUnit { get; set; }

        public double MaxSize { get; set; }

        public int MaxSizeUnit { get; set; }

        public bool MaxSizeInfinity { get; set; }

        public ICommand OKCommand => new RelayCommand(OK);

        public ICommand CancelCommand => new RelayCommand(Cancel);

        protected ExclusionRuleEditorDialog(ExclusionRule? exclusionRule)
        {
            ExclusionRule = exclusionRule ?? new ExclusionRule();
            IsNew = exclusionRule == null;

            EnableSearchPattern = ExclusionRule.EnableSearchPattern;
            SearchPattern = ExclusionRule.SearchPattern;
            EnableExtensionFilter = ExclusionRule.EnableExtensionFilter;
            ExtensionFilter = string.Join(", ", ExclusionRule.ExtensionFilter);
            EnableRegexPattern = ExclusionRule.EnableRegexPattern;
            RegexPattern = ExclusionRule.RegexPattern;
            EnableSizeFilter = ExclusionRule.EnableSizeFilter;
            MinSize = new FileSize(ExclusionRule.SizeMinLimit).Fit(out var minSizeUnit);
            MinSizeUnit = (int)minSizeUnit;
            var maxSize = new FileSize(ExclusionRule.SizeMaxLimit).Fit(out var maxSizeUnit);
            MaxSizeUnit = (int)maxSizeUnit;
            MaxSizeInfinity = ExclusionRule.SizeMaxLimit == long.MaxValue;
            MaxSize = MaxSizeInfinity ? 0d : maxSize;

            DataContext = this;
            InitializeComponent();
        }

        [GeneratedRegex(@"^(\*?\.[a-zA-Z0-9]+)$")]
        private static partial Regex ExtensionRegex();

        private bool DumpToRule()
        {
            if (EnableSizeFilter)
            {
                var minSize = new FileSize(MinSize, (FileSizeUnit)MinSizeUnit);
                var maxSize = MaxSizeInfinity ? new FileSize(long.MaxValue, FileSizeUnit.B)
                                              : new FileSize(MaxSize, (FileSizeUnit)MaxSizeUnit);
                if (minSize > maxSize)
                {
                    DialogHelper.ShowError("文件大小限制的下限值不得大于上限值。");
                    return false;
                }
                ExclusionRule.SizeMaxLimit = maxSize.SizeInBytes;
                ExclusionRule.SizeMinLimit = minSize.SizeInBytes;
            }
            ExclusionRule.EnableSizeFilter = EnableSizeFilter;

            if (EnableExtensionFilter)
            {
                ExclusionRule.ExtensionFilter = ExtensionFilter.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                               .Where(ext => ExtensionRegex().IsMatch(ext))
                                                               .Select(ext => ext.StartsWith('*') ? ext[1..] : ext)
                                                               .ToList();
            }
            ExclusionRule.EnableExtensionFilter = EnableExtensionFilter && ExclusionRule.ExtensionFilter.Count > 0;
            ExclusionRule.EnableSearchPattern = EnableSearchPattern && SearchPattern.Length > 0 && SearchPattern != "*";
            ExclusionRule.SearchPattern = EnableSearchPattern ? SearchPattern : "*";
            ExclusionRule.EnableRegexPattern = EnableRegexPattern;
            ExclusionRule.RegexPattern = RegexPattern;

            return true;
        }

        private void OK()
        {
            if (!DumpToRule())
            {
                return;
            }
            IsOK = true;
            Close();
        }

        private void Cancel()
        {
            if (DialogHelper.ShowWarning("尚有未保存的更改，是否仍要返回？"))
            {
                Close();
            }
        }

        public static ExclusionRule? NewOrModifyRule(ExclusionRule? rule)
        {
            var editor = new ExclusionRuleEditorDialog(rule);
            editor.ShowDialog();
            if (editor.IsNew && !editor.IsOK)
            {
                return null;
            }
            return editor.ExclusionRule;
        }
    }
}
