using ConcurrentCollections;
using Lekco.Promissum.App;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Promissum.View;
using Lekco.Wpf.Control;
using Lekco.Wpf.Utility;
using Lekco.Wpf.Utility.Helper;
using Lekco.Wpf.Utility.Progress;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// The class stores data during execution, and reports progress to UI.
    /// </summary>
    public class ExecutionData : IDualProgressReporter
    {
        /// <summary>
        /// The start time of execution.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The end time of execution.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// The trigger of task's execution.
        /// </summary>
        public ExecutionTrigger ExecutionTrigger { get; set; }

        /// <summary>
        /// The bag storing files which need to be synced.
        /// </summary>
        public ConcurrentBag<(FileBase, FileBase)> NeedSyncFiles { get; set; } = new();

        /// <summary>
        /// The bag storing directories which need to be initially synced.
        /// </summary>
        public ConcurrentBag<DirectoryBase> NeedSyncDirectories { get; set; } = new();

        /// <summary>
        /// The bag storing files which need to be cleaned up.
        /// </summary>
        public ConcurrentBag<FileBase> NeedCleanUpFiles { get; set; } = new();

        /// <summary>
        /// The bag storing information of directories which need to be clean up.
        /// </summary>
        public ConcurrentBag<DirectoryBase> NeedCleanUpDirectories { get; set; } = new();

        /// <summary>
        /// The bag storing files which are same to source that don't need to be synced.
        /// </summary>
        public ConcurrentBag<FileBase> DontNeedSyncFiles { get; set; } = new();

        /// <summary>
        /// The bag storing files which were synced successfully.
        /// </summary>
        public ConcurrentBag<FileBase> SyncedFiles { get; set; } = new();

        /// <summary>
        /// The bag storing files which have been reserved.
        /// </summary>
        public ConcurrentBag<FileBase> ReservedFiles { get; set; } = new();

        /// <summary>
        /// The bag storing files which have been deleted.
        /// </summary>
        public ConcurrentBag<FileBase> DeletedDestinationFiles { get; set; } = new();

        /// <summary>
        /// The bag storing tuples of records, corresponding versions and reserved files which need to be cleaned up.
        /// </summary>
        public ConcurrentBag<(CleanUpRecord, int, FileBase)> NeedCleanUpReservedFiles { get; set; } = new();

        /// <summary>
        /// The bag storing reserved files which have been deleted.
        /// </summary>
        public ConcurrentBag<FileBase> DeletedReservedFiles { get; set; } = new();

        /// <summary>
        /// The bag storing directories which have been deleted.
        /// </summary>
        public ConcurrentBag<DirectoryBase> DeletedDirectories { get; set; } = new();

        /// <summary>
        /// The bag storing records of exception during syncing.
        /// </summary>
        public ConcurrentBag<ExceptionRecord> ExceptionRecords { get; set; } = new();

        /// <summary>
        /// The hashset storing all directories in deletion path.
        /// </summary>
        public ConcurrentHashSet<string> ReservedPathDirectories { get; set; } = new();

        /// <summary>
        /// The execution's current state.
        /// </summary>
        private ExecutionState _state;

        /// <summary>
        /// Indicates whether execution is ended.
        /// </summary>
        private bool _isEnded;

        public event EventHandler? OnProgressBegin;

        public event EventHandler? OnProgressEnd;

        public event EventHandler<double>? OnFirstProgressValueChanged;

        public event EventHandler<double>? OnSecondProgressValueChanged;

        public event EventHandler<string>? OnFirstProgressTextChanged;

        public event EventHandler<string>? OnSecondProgressTextChanged;

        private readonly STAThreadHolder<DualProgressDialog> _STAThreadHolder;

        /// <summary>
        /// Create an instance.
        /// </summary>
        public ExecutionData()
        {
            _STAThreadHolder = new STAThreadHolder<DualProgressDialog>(() => new DualProgressDialog(this, location: DialogStartUpLocation.RightBottom));
            SetState(ExecutionState.Prepare);
            
            _STAThreadHolder.SafelyDo(dialog => ReportCallBack());
        }

        protected async void ReportCallBack()
        {
            while (!_isEnded)
            {
                ReportProgress();
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// Set the execution's state.
        /// </summary>
        /// <param name="state">The state of execution.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetState(ExecutionState state)
        {
            _state = state;
            (double, string) tuple = state switch
            {
                ExecutionState.Prepare => (100d / 6d, "(1/6) 准备"),
                ExecutionState.Query => (200d / 6d, "(2/6) 查找待同步文件"),
                ExecutionState.CleanUpDestinationPath => (300d / 6d, "(3/6) 清理目标目录"),
                ExecutionState.CleanUpReservedPath => (400d / 6d, "(4/6) 清理回收目录"),
                ExecutionState.SyncFiles => (500d / 6d, "(5/6) 同步文件"),
                ExecutionState.Completion => (600d / 6d, "(6/6) 完成"),
                _ => throw new InvalidOperationException()
            };
            OnFirstProgressValueChanged?.Invoke(this, tuple.Item1);
            OnFirstProgressTextChanged?.Invoke(this, tuple.Item2);
        }

        /// <summary>
        /// Gets whether clean up reserved path.
        /// </summary>
        public bool CanCleanUpReservedPath(PathBase path)
        {
            if (NeedCleanUpReservedFiles.IsEmpty)
            {
                return false;
            }

            var relativeTo = NeedCleanUpReservedFiles.Select(t => new RelativedFile(t.Item3, path));
            return !Config.Instance.AlwaysNotifyWhenDelete || DialogHelper.ShowInformation(
                message: $"是否清理回收目录中的{NeedCleanUpReservedFiles.Count}份文件？",
                location: DialogStartUpLocation.RightBottom,
                autoCountDown: true,
                link: "查看待清理文件",
                linkAction: new Action(() => FileListWindow.ShowFilesList(relativeTo))
            );
        }

        /// <summary>
        /// Execution begins.
        /// </summary>
        public void ExecutionBegin()
        {
            OnProgressBegin?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Execution ends.
        /// </summary>
        public void ExecutionEnd()
        {
            _isEnded = true;
            OnProgressEnd?.Invoke(this, EventArgs.Empty);
            _STAThreadHolder.Dispose();
        }

        /// <summary>
        /// Report current progress to UI.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected void ReportProgress()
        {
            (double, string) tuple = _state switch
            {
                ExecutionState.Prepare => (double.NaN, "正在读取数据库及准备必要资源……"),
                ExecutionState.Query => (double.NaN, $"已发现{NeedSyncFiles.Count}份文件……"),
                ExecutionState.CleanUpDestinationPath => (100d * (ReservedFiles.Count + DeletedDestinationFiles.Count) / NeedCleanUpFiles.Count,
                                                          $"已回收{ReservedFiles.Count}份文件，清理{DeletedDestinationFiles.Count}份文件……"),
                ExecutionState.CleanUpReservedPath => (100d * DeletedReservedFiles.Count / NeedCleanUpReservedFiles.Count,
                                                       $"已清理{DeletedReservedFiles.Count}份文件……"),
                ExecutionState.SyncFiles => (100d * SyncedFiles.Count / NeedSyncFiles.Count, $"已同步{SyncedFiles.Count}份文件……"),
                ExecutionState.Completion => (double.NaN, "正在保存数据……"),
                _ => throw new InvalidOperationException()
            };
            OnSecondProgressValueChanged?.Invoke(this, tuple.Item1);
            OnSecondProgressTextChanged?.Invoke(this, tuple.Item2);
        }
    }

    /// <summary>
    /// The state of execution.
    /// </summary>
    public enum ExecutionState
    {
        /// <summary>
        /// State of preparing.
        /// </summary>
        Prepare,

        /// <summary>
        /// State of querying.
        /// </summary>
        Query,

        /// <summary>
        /// State of cleaning up destination path.
        /// </summary>
        CleanUpDestinationPath,

        /// <summary>
        /// State of cleaning up reserved path.
        /// </summary>
        CleanUpReservedPath,

        /// <summary>
        /// State of syncing files.
        /// </summary>
        SyncFiles,

        /// <summary>
        /// State of completion.
        /// </summary>
        Completion,
    }
}
