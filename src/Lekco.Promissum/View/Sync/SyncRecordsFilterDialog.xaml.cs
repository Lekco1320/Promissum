using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.MVVM.Filter;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Filter;
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

        public List<string> PropertyNames { get; }

        public List<string> DisplayNames { get; }

        public RelayCommand<IFilterGroupNode> AddFilterCommand { get; }

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

        private readonly Func<PropertyInformation, IPropertyFilterVM> ConsturctFunc;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SyncRecordsFilterDialog(SyncDataSetType dataSetType, IPropertyFilterVM? rootNode)
        {
            IsFilter = rootNode != null;
            PropertyName = "";
            OKCommand = new RelayCommand(OK);
            CancelCommand = new RelayCommand(Cancel);
            AddFilterCommand = new RelayCommand<IFilterGroupNode>(NewFilter);

            switch (dataSetType)
            {
            case SyncDataSetType.FileRecordDataSet:
                var vm1 = (PropertyFilterGroupVM<FileRecord>?)rootNode ?? new PropertyFilterGroupVM<FileRecord>();
                PropertyFilterVM = vm1;
                PropertyInfo = FileRecordsPropertyInfo;
                PropertyNames = FileRecordsPropertyInfo.Select(i => i.PropertyName).ToList();
                DisplayNames = FileRecordsPropertyInfo.Select(i => i.DisplayName).ToList();
                ConsturctFunc = NewRecordFilterVM<FileRecord>;
                break;

            case SyncDataSetType.CleanUpDataSet:
                var vm2 = (PropertyFilterGroupVM<CleanUpRecord>?)rootNode ?? new PropertyFilterGroupVM<CleanUpRecord>();
                PropertyFilterVM = vm2;
                PropertyInfo = CleanUpRecordsPropertyNames;
                PropertyNames = CleanUpRecordsPropertyNames.Select(i => i.PropertyName).ToList();
                DisplayNames = CleanUpRecordsPropertyNames.Select(i => i.DisplayName).ToList();
                ConsturctFunc = NewRecordFilterVM<CleanUpRecord>;
                break;

            case SyncDataSetType.ExecutionDataSet:
                var vm3 = (PropertyFilterGroupVM<ExecutionRecord>?)rootNode ?? new PropertyFilterGroupVM<ExecutionRecord>();
                PropertyFilterVM = vm3;
                PropertyInfo = ExecutionRecordsPropertyNames;
                PropertyNames = ExecutionRecordsPropertyNames.Select(i => i.PropertyName).ToList();
                DisplayNames = ExecutionRecordsPropertyNames.Select(i => i.DisplayName).ToList();
                ConsturctFunc = NewRecordFilterVM<ExecutionRecord>;
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

        private void NewFilter(IFilterGroupNode node)
        {
            var dialog = new SelectPropertyDialog(DisplayNames);
            dialog.ShowDialog();
            if (!dialog.IsOK)
            {
                return;
            }

            int selectedIndex = dialog.SelectedIndex;
            var newNode = ConsturctFunc(PropertyInfo[selectedIndex]);
            node.AddChild((IFilterNode)newNode);
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
