using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Execution;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Promissum.View;
using Lekco.Wpf.MVVM;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using MiniExcelLibs;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lekco.Promissum.ViewModel.Sync
{
    public class SyncRecordsWindowVM : ViewModelBase
    {
        public string FilterString { get; set; }

        [OnChangedMethod(nameof(SwitchSource))]
        [OnChangedMethod(nameof(SwitchColumns))]
        [OnChangedMethod(nameof(ResetFilter))]
        public int CategoryIndex { get; set; }

        public int RecordsCount { get; set; }

        public RelayCommand FilterCommand => new RelayCommand(Filter);

        public RelayCommand OutputCommand => new RelayCommand(Output);

        public RelayCommand ClearDataBaseCommand => new RelayCommand(ClearDataBase);

        public static RelayCommand<ExecutionRecord> HyperLinkCommand => new RelayCommand<ExecutionRecord>(ViewExceptions);

        public ListCollectionView CurrentView { get; set; }

        public List<DataGridColumn>? DataGridColumns { get; set; }

        protected SyncTask SyncTask;

        protected List<FileRecord> FileRecords;

        protected List<CleanUpRecord> CleanUpRecords;

        protected List<ExecutionRecord> ExecutionRecords;

        protected ListCollectionView FileRecordsView;

        protected ListCollectionView CleanUpRecordsView;

        protected ListCollectionView ExecutionRecordsView;

        protected readonly List<DataGridColumn> FileRecordsColumns = new()
        {
            new DataGridTextColumn
            {
                Header = "ID",
                Binding = new Binding(nameof(FileRecord.ID)),
                MinWidth = 30,
            },
            new DataGridTextColumn
            {
                Header = "文件名",
                Binding = new Binding(nameof(FileRecord.RelativeFileName)),
                CellStyle = FileNameCellStyle,
                Width = 200,
            },
            new DataGridTextColumn
            {
                Header = "同步时间",
                Binding = new Binding(nameof(FileRecord.LastSyncTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(FileRecord.LastSyncTime),
                Width = 120,
            },
            new DataGridTextColumn
            {
                Header = "同步次数",
                Binding = new Binding(nameof(FileRecord.SyncCount)),
                Width = 60,
            },
            new DataGridTextColumn
            {
                Header = "文件大小",
                Binding = new Binding(nameof(FileRecord.FileSize)) { Converter = FileSizeFormatter },
                SortMemberPath = nameof(FileRecord.FileSize),
                Width = 60,
            },
            new DataGridTextColumn
            {
                Header = "创建日期",
                Binding = new Binding(nameof(FileRecord.CreationTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(FileRecord.CreationTime),
                Width = 120,
            },
            new DataGridTextColumn
            {
                Header = "修改日期",
                Binding = new Binding(nameof(FileRecord.LastWriteTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(FileRecord.LastWriteTime),
                Width = 120,
            }
        };

        protected readonly List<DataGridColumn> CleanUpRecordsColumns = new()
        {
            new DataGridTextColumn
            {
                Header = "ID",
                Binding = new Binding(nameof(CleanUpRecord.ID)),
                MinWidth = 30,
            },
            new DataGridTextColumn
            {
                Header = "文件名",
                Binding = new Binding(nameof(CleanUpRecord.RelativeFileName)),
                CellStyle = FileNameCellStyle,
                Width = 200,
            },
            new DataGridTextColumn
            {
                Header = "操作时间",
                Binding = new Binding(nameof(CleanUpRecord.LastOperateTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(CleanUpRecord.LastOperateTime),
                Width = 120,
            },
            new DataGridTextColumn
            {
                Header = "保留版本",
                Binding = new Binding(nameof(CleanUpRecord.ReservedVersionList)) { Converter = IEnumerableJoiner, ConverterParameter = ", ", StringFormat = "{{{0}}}" },
                Width = 60,
            },
            new DataGridTextColumn
            {
                Header = "文件大小",
                Binding = new Binding(nameof(CleanUpRecord.FileSize)) { Converter = FileSizeFormatter },
                SortMemberPath = nameof(CleanUpRecord.FileSize),
                Width = 60,
            },
            new DataGridTextColumn
            {
                Header = "创建日期",
                Binding = new Binding(nameof(CleanUpRecord.CreationTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(CleanUpRecord.CreationTime),
                Width = 120,
            },
            new DataGridTextColumn
            {
                Header = "修改日期",
                Binding = new Binding(nameof(CleanUpRecord.LastWriteTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(CleanUpRecord.LastWriteTime),
                Width = 120,
            }
        };

        protected readonly List<DataGridColumn> ExecutionRecordsColumns = new()
        {
            new DataGridTextColumn
            {
                Header = "ID",
                Binding = new Binding(nameof(ExecutionRecord.ID)),
                MinWidth = 30,
            },
            new DataGridTextColumn
            {
                Header = "执行原因",
                Binding = new Binding(nameof(ExecutionRecord.ExecutionTrigger)) { Converter = EnumDiscriptionGetter },
                Width = 60,
            },
            new DataGridTextColumn
            {
                Header = "开始时间",
                Binding = new Binding(nameof(ExecutionRecord.StartTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(ExecutionRecord.StartTime),
                Width = 120,
            },
            new DataGridTextColumn
            {
                Header = "结束时间",
                Binding = new Binding(nameof(ExecutionRecord.EndTime)) { Converter = DateTimeFormatter },
                SortMemberPath = nameof(ExecutionData.EndTime),
                Width = 120,
            },
            new DataGridTextColumn
            {
                Header = "同步文件数",
                Binding = new Binding(nameof(ExecutionRecord.SyncedFilesCount)),
                Width = 80,
            },
            new DataGridTextColumn
            {
                Header = "暂存文件数",
                Binding = new Binding(nameof(ExecutionRecord.ReservedFilesCount)),
                Width = 80,
            },
            new DataGridTextColumn
            {
                Header = "清理文件数",
                Binding = new Binding(nameof(ExecutionRecord.DeletedFilesCount)),
                Width = 80,
            },
            new DataGridTextColumn
            {
                Header = "清理文件夹数",
                Binding = new Binding(nameof(ExecutionRecord.DeletedDirectoriesCount)),
                Width = 85,
            },
            new DataGridTextColumn
            {
                Header = "异常信息",
                Binding = new Binding(nameof(ExecutionRecord.ExceptionRecords)) { Converter = IEnumerableCountGetter, StringFormat = "{0} 条…", Mode = BindingMode.OneWay },
                CellStyle = HyperLinkCellStyle,
                Width = 80,
            }
        };

        protected static readonly Style LeftCellStyle = (Style)Application.Current.FindResource("DataGridLeftCellStyle");

        protected static readonly Style HyperLinkCellStyle = (Style)Application.Current.FindResource("DataGridHyperLinkCellStyle");

        protected static readonly Style FileNameCellStyle = (Style)Application.Current.FindResource("DataGridFileNameCellStyle");

        protected static readonly IValueConverter DateTimeFormatter = (IValueConverter)Application.Current.FindResource("DateTimeFormatter");

        protected static readonly IValueConverter FileSizeFormatter = (IValueConverter)Application.Current.FindResource("FileSizeFormatter");

        protected static readonly IValueConverter EnumDiscriptionGetter = (IValueConverter)Application.Current.FindResource("EnumDiscriptionGetter");

        protected static readonly IValueConverter IEnumerableJoiner = (IValueConverter)Application.Current.FindResource("IEnumerableJoiner");

        protected static readonly IValueConverter IEnumerableCountGetter = (IValueConverter)Application.Current.FindResource("IEnumerableCountGetter");

        public SyncRecordsWindowVM(SyncTask task, List<FileRecord> fileRecords, List<CleanUpRecord> cleanUpRecords, List<ExecutionRecord> executionRecords)
        {
            SyncTask = task;
            FilterString = "";
            FileRecords = fileRecords;
            ExecutionRecords = executionRecords;
            CleanUpRecords = cleanUpRecords;
            RecordsCount = FileRecords.Count;
            FileRecordsView = new ListCollectionView(FileRecords);
            CleanUpRecordsView = new ListCollectionView(CleanUpRecords);
            ExecutionRecordsView = new ListCollectionView(ExecutionRecords);
            CurrentView = FileRecordsView;
            DataGridColumns = FileRecordsColumns;
        }

        protected void ResetFilter()
        {
            FilterString = "";
        }

        protected void SwitchSource()
        {
            CurrentView = CategoryIndex switch
            {
                0 => FileRecordsView,
                1 => CleanUpRecordsView,
                2 => ExecutionRecordsView,
                _ => throw new InvalidOperationException("Unknown category.")
            };
            RecordsCount = CurrentView.Count;
        }

        protected void SwitchColumns()
        {
            DataGridColumns = CategoryIndex switch
            {
                0 => FileRecordsColumns,
                1 => CleanUpRecordsColumns,
                2 => ExecutionRecordsColumns,
                _ => null,
            };
            CurrentView.Refresh();
        }

        protected void Filter()
        {
            CurrentView.Filter = CategoryIndex switch
            {
                0 => obj => ((FileRecord)obj).RelativeFileName.Contains(FilterString),
                1 => obj => ((CleanUpRecord)obj).RelativeFileName.Contains(FilterString),
                2 => obj => ((ExecutionRecord)obj).StartTime.ToString().Contains(FilterString),
                _ => null
            };
            CurrentView.Refresh();
        }

        protected void Output()
        {
            var fileRecordsData = FileRecords.Select(record => new
            {
                record.ID,
                文件名 = record.RelativeFileName,
                同步时间 = record.LastSyncTime,
                同步次数 = record.SyncCount,
                文件大小 = new FileSize(record.FileSize).ToString(),
                创建日期 = record.CreationTime,
                修改日期 = record.LastWriteTime,
            });
            var cleanUpRecordsData = CleanUpRecords.Select(record => new
            {
                record.ID,
                文件名 = record.RelativeFileName,
                清理时间 = record.LastOperateTime,
                保留版本 = string.Join(", ", record.ReservedVersions),
                文件大小 = new FileSize(record.FileSize).ToString(),
                创建日期 = record.CreationTime,
                修改日期 = record.LastWriteTime,
            });
            var executionRecordsData = ExecutionRecords.Select(record => new
            {
                record.ID,
                执行原因 = record.ExecutionTrigger.GetDiscription(),
                开始时间 = record.StartTime,
                结束时间 = record.EndTime,
                同步文件数 = record.SyncedFilesCount,
                暂存文件数 = record.ReservedFilesCount,
                清理文件数 = record.DeletedFilesCount,
                清理文件夹数 = record.DeletedDirectoriesCount,
            });
            var exceptionRecordsData = ExecutionRecords.SelectMany(record => record.ExceptionRecords, (record, exRecord) => new
            {
                执行记录ID = record.ID,
                异常记录ID = exRecord.ID,
                文件名 = exRecord.FileFullName,
                发生时间 = exRecord.OccurredTime,
                操作类型 = exRecord.OperationType.GetDiscription(),
                异常类型 = exRecord.ExceptionType.GetDiscription(),
                异常消息 = exRecord.ExceptionMessage,
            });
            var sheets = new Dictionary<string, object>()
            {
                { "同步记录", fileRecordsData },
                { "清理记录", cleanUpRecordsData },
                { "执行记录", executionRecordsData },
                { "异常记录", exceptionRecordsData },
            };

            var dialog = new System.Windows.Forms.SaveFileDialog()
            {
                AddExtension = true,
                FileName = $"{SyncTask.Name} 数据",
                Filter = "Excel 文档|*.xlsx",
                FilterIndex = 1,
                Title = "保存数据",
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var stream = new FileStream(dialog.FileName, FileMode.Create);
                    MiniExcel.SaveAs(stream, sheets);
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowError(ex);
                }
                DialogHelper.ShowSuccess($"数据文件\"{dialog.FileName}\"导出成功。");
            }
        }

        protected static void ViewExceptions(ExecutionRecord record)
        {
            var vm = new ExceptionRecordsWindowVM(record);
            new ExceptionRecordsWindow(vm).Show();
        }

        protected void ClearDataBase()
        {
            if (DialogHelper.ShowWarning("是否要清除该任务关联的所有数据？此操作将无法撤销。"))
            {
                SyncTask.DeleteDataSet(SyncDataSetType.AllDataSets);
                FileRecords.Clear();
                CleanUpRecords.Clear();
                ExecutionRecords.Clear();
                CurrentView.Refresh();
                RecordsCount = 0;
            }
        }
    }
}
