﻿using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Promissum.View;
using Lekco.Wpf.MVVM;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Lekco.Promissum.ViewModel.Sync
{
    public class SyncRecordsWindowVM : ViewModelBase
    {
        public string FilterString { get; set; }

        [OnChangedMethod(nameof(SwitchCategory))]
        public int CategoryIndex { get; set; }

        public int RecordsCount { get; set; }

        [OnChangedMethod(nameof(ChangePageSize))]
        public int PageSizeIndex { get; set; }

        public int PageSize => PageSizes[PageSizeIndex];

        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                if (value != _pageIndex && value > 0 && value <= PageCount)
                {
                    _pageIndex = value;
                    OnPropertyChanged(nameof(PageIndex));
                    ChangePageIndex();
                }
            }
        }
        private int _pageIndex = 1;

        public int PageCount { get; set; }

        public bool IsBusy { get; set; }

        public RelayCommand LoadedCommand => new RelayCommand(SwitchCategory);

        public RelayCommand FilterCommand => new RelayCommand(LoadPage);

        public RelayCommand OutputCommand => new RelayCommand(Output);

        public RelayCommand ClearDataBaseCommand => new RelayCommand(ClearDataBase);

        public RelayCommand QuitCommand => new RelayCommand(SyncDbContext.Dispose);

        public static RelayCommand<ExecutionRecord> HyperLinkCommand => new RelayCommand<ExecutionRecord>(ViewExceptions);

        public IEnumerable? CurrentView { get; set; }

        public static readonly int[] PageSizes = [15, 20, 50, 100];

        protected SyncTask SyncTask;

        protected SyncDbContext SyncDbContext;

        protected ObservableCollection<FileRecord> FileRecords;

        protected ObservableCollection<CleanUpRecord> CleanUpRecords;

        protected ObservableCollection<ExecutionRecord> ExecutionRecords;

        public SyncRecordsWindowVM(SyncTask task, SyncDbContext dbContext)
        {
            SyncTask = task;
            FilterString = "";
            SyncDbContext = dbContext;
            FileRecords = new ObservableCollection<FileRecord>();
            CleanUpRecords = new ObservableCollection<CleanUpRecord>();
            ExecutionRecords = new ObservableCollection<ExecutionRecord>();
        }

        protected async Task LoadFileRecords()
        {
            IsBusy = true;

            RecordsCount = await SyncDbContext.FileRecords
                .Where(r => r.RelativeFileName.Contains(FilterString))
                .CountAsync();
            PageCount = (int)Math.Ceiling((double)RecordsCount / PageSize);
            if (PageIndex > PageCount)
            {
                ChangePageIndexInternal(1);
            }
            var records = await SyncDbContext.FileRecords
                .AsNoTracking()
                .Where(r => r.RelativeFileName.Contains(FilterString))
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                FileRecords.Clear();
                foreach (var record in records)
                {
                    FileRecords.Add(record);
                }
                CurrentView = FileRecords;
            });

            IsBusy = false;
        }

        protected async Task LoadCleanUpRecords()
        {
            IsBusy = true;

            RecordsCount = await SyncDbContext.CleanUpRecords
                .Where(r => r.RelativeFileName.Contains(FilterString))
                .CountAsync();
            PageCount = (int)Math.Ceiling((double)RecordsCount / PageSize);
            if (PageIndex > PageCount)
            {
                ChangePageIndexInternal(1);
            }
            var records = await SyncDbContext.CleanUpRecords
                .AsNoTracking()
                .Where(r => r.RelativeFileName.Contains(FilterString))
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CleanUpRecords.Clear();
                foreach (var record in records)
                {
                    CleanUpRecords.Add(record);
                }
                CurrentView = CleanUpRecords;
            });

            IsBusy = false;
        }

        protected async Task LoadExecutionRecords()
        {
            IsBusy = true;

            RecordsCount = await SyncDbContext.ExecutionRecords.CountAsync();
            PageCount = (int)Math.Ceiling((double)RecordsCount / PageSize);
            if (PageIndex > PageCount)
            {
                ChangePageIndexInternal(1);
            }
            var records = await SyncDbContext.ExecutionRecords
                .AsNoTracking()
                .Include(record => record.ExceptionRecords)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ExecutionRecords.Clear();
                foreach (var record in records)
                {
                    ExecutionRecords.Add(record);
                }
                CurrentView = ExecutionRecords;
            });

            IsBusy = false;
        }

        protected async void SwitchCategory()
        {
            if (IsBusy)
            {
                return;
            }

            CurrentView = null;
            FilterString = "";
            ChangePageIndexInternal(1);
            await Task.Run(LoadPage);
        }

        protected async void ChangePageSize()
        {
            if (IsBusy)
            {
                return;
            }

            ChangePageIndexInternal(1);
            await Task.Run(LoadPage);
        }

        protected async void ChangePageIndex()
        {
            if (IsBusy)
            {
                return;
            }

            await Task.Run(LoadPage);
        }

        protected void ChangePageIndexInternal(int index)
        {
            if (index > 0 && index <= PageCount)
            {
                _pageIndex = index;
                OnPropertyChanged(nameof(PageIndex));
            }
        }

        protected async void LoadPage()
        {
            switch (CategoryIndex)
            {
            case 0:
                await Task.Run(LoadFileRecords);
                break;

            case 1:
                await Task.Run(LoadCleanUpRecords);
                break;

            case 2:
                await Task.Run(LoadExecutionRecords);
                break;

            default:
                throw new InvalidOperationException("Unknown category index.");
            }
        }

        protected void Output()
        {
            IsBusy = true;
            var fileRecordsData = SyncDbContext.FileRecords
                .AsNoTracking()
                .Select(record => new
                {
                    record.ID,
                    文件名 = record.RelativeFileName,
                    同步时间 = record.LastSyncTime,
                    同步次数 = record.SyncCount,
                    文件大小 = new FileSize(record.FileSize, FileSizeUnit.Auto).ToString(),
                    创建日期 = record.CreationTime,
                    修改日期 = record.LastWriteTime,
                });
            var cleanUpRecordsData = SyncDbContext.CleanUpRecords
                .AsNoTracking()
                .Select(record => new
                {
                    record.ID,
                    文件名 = record.RelativeFileName,
                    清理时间 = record.LastOperateTime,
                    保留版本 = string.Join(", ", record.ReservedVersions),
                    文件大小 = new FileSize(record.FileSize, FileSizeUnit.Auto).ToString(),
                    创建日期 = record.CreationTime,
                    修改日期 = record.LastWriteTime,
                });
            var executionRecordsData = SyncDbContext.ExecutionRecords
                .AsNoTracking()
                .Select(record => new
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
            var exceptionRecordsData = SyncDbContext.ExecutionRecords
                .AsNoTracking()
                .SelectMany(record => record.ExceptionRecords, (record, exRecord) => new
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
                    using var stream = new FileStream(dialog.FileName, FileMode.Create);
                    MiniExcel.SaveAs(stream, sheets);
                }
                catch (Exception ex)
                {
                    DialogHelper.ShowError(ex);
                }
                DialogHelper.ShowSuccess($"数据文件\"{dialog.FileName}\"导出成功。");
            }
            IsBusy = false;
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
                FileRecords?.Clear();
                CleanUpRecords?.Clear();
                ExecutionRecords?.Clear();
                RecordsCount = 0;
            }
        }
    }
}
