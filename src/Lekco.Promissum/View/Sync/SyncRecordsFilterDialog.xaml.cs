using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.MVVM.Filter;
using Lekco.Wpf.MVVM.Sorter;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Filter;
using Lekco.Wpf.Utility.Sorter;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Lekco.Promissum.View.Sync
{
    public partial class SyncRecordsFilterDialog : CustomWindow, IInteractive, INotifyPropertyChanged
    {
        public bool IsFilter { get; set; }

        public bool IsSort { get; set; }

        public bool IsAscending { get; set; }

        public string PropertyName { get; set; }

        public IPropertyFilterVM? PropertyFilterVM { get; set; }

        public IPropertySorterGroupVM? PropertySorterGroupVM { get; set; }

        public List<string> PropertyNames { get; }

        public List<string> DisplayNames { get; }

        public RelayCommand<IFilterGroupNode> AddFilterCommand { get; }

        public RelayCommand<ISorterGroup> AddSorterCommand { get; }

        public bool IsOK { get; set; }

        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }

        private static readonly PropertyInformation[] FileRecordsPropertyInfo =
        [
            new PropertyInformation(nameof(FileRecord.ID), "ID", typeof(int)),
            new PropertyInformation(nameof(FileRecord.RelativeFileName), "相对路径", typeof(string)),
            new PropertyInformation(nameof(FileRecord.FileSize), "文件大小", typeof(FileSize)),
            new PropertyInformation(nameof(FileRecord.CreationTime), "创建时间", typeof(DateTime)),
            new PropertyInformation(nameof(FileRecord.LastWriteTime), "修改时间", typeof(DateTime)),
            new PropertyInformation(nameof(FileRecord.SyncCount), "同步次数", typeof(int)),
            new PropertyInformation(nameof(FileRecord.LastSyncTime), "同步时间", typeof(DateTime))
        ];

        private static readonly PropertyInformation[] CleanUpRecordsPropertyNames =
        [
            new PropertyInformation(nameof(CleanUpRecord.ID), "ID", typeof(int)),
            new PropertyInformation(nameof(CleanUpRecord.RelativeFileName), "相对路径", typeof(string)),
            new PropertyInformation(nameof(CleanUpRecord.FileSize), "文件大小", typeof(FileSize)),
            new PropertyInformation(nameof(CleanUpRecord.CreationTime), "创建时间", typeof(DateTime)),
            new PropertyInformation(nameof(CleanUpRecord.LastWriteTime), "修改时间", typeof(DateTime)),
            new PropertyInformation(nameof(CleanUpRecord.ReservedVersions), "保留版本", typeof(string))
        ];

        private static readonly PropertyInformation[] ExecutionRecordsPropertyNames =
        [
            new PropertyInformation(nameof(ExecutionRecord.ID), "ID", typeof(int)),
            new PropertyInformation(nameof(ExecutionRecord.StartTime), "开始时间", typeof(DateTime)),
            new PropertyInformation(nameof(ExecutionRecord.EndTime), "结束时间", typeof(DateTime)),
            new PropertyInformation(nameof(ExecutionRecord.SyncedFilesCount), "同步文件数", typeof(int)),
            new PropertyInformation(nameof(ExecutionRecord.ReservedFilesCount), "保留文件数", typeof(int)),
            new PropertyInformation(nameof(ExecutionRecord.DeletedFilesCount), "删除文件数", typeof(int)),
            new PropertyInformation(nameof(ExecutionRecord.DeletedDirectoriesCount), "删除文件夹数", typeof(int))
        ];

        private readonly PropertyInformation[] PropertyInfo;

        private readonly Func<PropertyInformation, IPropertyFilterVM> FilterVMConstructer;

        private readonly Func<PropertyInformation, IPropertySorterVM> SorterVMConstructer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SyncRecordsFilterDialog(SyncDataSetType dataSetType, IPropertyFilterVM? rootNode, IPropertySorterGroupVM? sorter)
        {
            IsFilter = rootNode != null;
            IsSort = sorter != null;
            PropertyName = "";
            OKCommand = new RelayCommand(OK);
            CancelCommand = new RelayCommand(Cancel);
            AddFilterCommand = new RelayCommand<IFilterGroupNode>(NewFilter);
            AddSorterCommand = new RelayCommand<ISorterGroup>(NewSorter);

            switch (dataSetType)
            {
            case SyncDataSetType.FileRecordDataSet:
                PropertyFilterVM = rootNode ?? new PropertyFilterGroupVM<FileRecord>();
                PropertySorterGroupVM = sorter ?? new PropertySorterGroupVM<FileRecord>();
                PropertyInfo = FileRecordsPropertyInfo;
                PropertyNames = FileRecordsPropertyInfo.Select(i => i.PropertyName).ToList();
                DisplayNames = FileRecordsPropertyInfo.Select(i => i.DisplayName).ToList();
                FilterVMConstructer = NewRecordFilterVM<FileRecord>;
                SorterVMConstructer = NewRecordSorterVM<FileRecord>;
                break;

            case SyncDataSetType.CleanUpDataSet:
                PropertyFilterVM = rootNode ?? new PropertyFilterGroupVM<CleanUpRecord>();
                PropertySorterGroupVM = sorter ?? new PropertySorterGroupVM<CleanUpRecord>();
                PropertyInfo = CleanUpRecordsPropertyNames;
                PropertyNames = CleanUpRecordsPropertyNames.Select(i => i.PropertyName).ToList();
                DisplayNames = CleanUpRecordsPropertyNames.Select(i => i.DisplayName).ToList();
                FilterVMConstructer = NewRecordFilterVM<CleanUpRecord>;
                SorterVMConstructer = NewRecordSorterVM<CleanUpRecord>;
                break;

            case SyncDataSetType.ExecutionDataSet:
                PropertyFilterVM = rootNode ?? new PropertyFilterGroupVM<ExecutionRecord>();
                PropertySorterGroupVM = sorter ?? new PropertySorterGroupVM<ExecutionRecord>();
                PropertyInfo = ExecutionRecordsPropertyNames;
                PropertyNames = ExecutionRecordsPropertyNames.Select(i => i.PropertyName).ToList();
                DisplayNames = ExecutionRecordsPropertyNames.Select(i => i.DisplayName).ToList();
                FilterVMConstructer = NewRecordFilterVM<ExecutionRecord>;
                SorterVMConstructer = NewRecordSorterVM<ExecutionRecord>;
                break;

            default:
                throw new ArgumentException("Invalid type of dataset.", nameof(dataSetType));
            }

            InitializeComponent();
            DataContext = this;
        }

        private void OK()
        {
            if (!IsFilter)
            {
                PropertyFilterVM = null;
            }
            IsOK = true;
            Close();
        }

        private void Cancel()
        {
            Close();
        }

        private static IPropertyFilterVM<T> NewRecordFilterVM<T>(PropertyInformation info)
        {
            PropertyFilter<T> filter;
            if (info.Type == typeof(int))
            {
                filter = new PropertyIntFilter<T>() { PropertyName = info.PropertyName };
            }
            else if (info.Type == typeof(string))
            {
                filter = new PropertyStringFilter<T>() { PropertyName = info.PropertyName };
            }
            else if (info.Type == typeof(FileSize))
            {
                filter = new PropertyFileSizeFilter<T>() { PropertyName = info.PropertyName };
            }
            else if (info.Type == typeof(DateTime))
            {
                filter = new PropertyDateTimeFilter<T>() { PropertyName = info.PropertyName };
            }
            else
            {
                throw new ArgumentException("Invalid type.", nameof(info));
            }
            var factory = new PropertyFilterVMFactory<T>();
            var vm = factory.Create(filter);
            vm.DisplayName = info.DisplayName;
            return vm;
        }

        private static IPropertySorterVM<T> NewRecordSorterVM<T>(PropertyInformation info)
        {
            var sorter = new PropertySorter<T>(info.PropertyName, false);
            return new PropertySorterVM<T>(sorter, info.DisplayName);
        }

        private void NewFilter(IFilterGroupNode node)
        {
            var dialog = new SelectPropertyDialog(DisplayNames);
            dialog.Owner = this;
            dialog.ShowDialog();
            if (!dialog.IsOK)
            {
                return;
            }

            int selectedIndex = dialog.SelectedIndex;
            var newNode = FilterVMConstructer(PropertyInfo[selectedIndex]);
            node.AddChild((IFilterNode)newNode);
        }

        private void NewSorter(ISorterGroup group)
        {
            var dialog = new SelectPropertyDialog(DisplayNames);
            dialog.Owner = this;
            dialog.ShowDialog();
            if (!dialog.IsOK)
            {
                return;
            }

            int selectedIndex = dialog.SelectedIndex;
            var newItem = SorterVMConstructer(PropertyInfo[selectedIndex]);
            group.AddChild((ISorterItem)newItem);
        }
    }

    internal class PropertyInformation
    {
        public string PropertyName { get; }

        public string DisplayName { get; }

        public Type Type { get; }

        public PropertyInformation(string propertyName, string displayName, Type type)
        {
            PropertyName = propertyName;
            DisplayName = displayName;
            Type = type;
        }
    }
}
