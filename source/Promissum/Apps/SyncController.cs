using Lekco.Promissum.Sync;
using Lekco.Promissum.Utility;
using Lekco.Promissum.View;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Lekco.Promissum.Apps
{
    public class SyncController : BindableBase
    {
        public ExecutionTrigger Trigger { get; protected set; }
        public string TaskName { get; protected set; }

        public SyncProgressState SyncProgressState
        {
            get => SyncProgressState;
            protected set
            {
                _syncProgressState = value;
                RaisePropertyChanged(nameof(SyncProgressState));
            }
        }
        private SyncProgressState _syncProgressState;

        public string Description1
        {
            get => _description1;
            protected set
            {
                _description1 = value;
                RaisePropertyChanged(nameof(Description1));
            }
        }
        private string _description1;

        public string Description2
        {
            get => _description2;
            protected set
            {
                _description2 = value;
                RaisePropertyChanged(nameof(Description2));
            }
        }
        private string _description2;

        public double Value1
        {
            get => _value1;
            protected set
            {
                _value1 = value;
                RaisePropertyChanged(nameof(Value1));
            }
        }
        private double _value1;

        public double Value2
        {
            get => _value2;
            protected set
            {
                _value2 = value;
                RaisePropertyChanged(nameof(Value2));
            }
        }
        private double _value2;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            protected set
            {
                _isIndeterminate = value;
                RaisePropertyChanged(nameof(IsIndeterminate));
            }
        }
        private bool _isIndeterminate;

        public bool CanDealWithUnexpectedFiles { get; protected set; }

        private int _foundSyncFilesCount;
        private int _copiedSyncFilesCount;
        private int _unexpectedFilesCount;
        private int _dealedUnexpectedFilesCount;
        private int _managedDeletingFilesCount;
        private int _foundDeletingFilesCount;

        public SyncController(SyncTask syncTask, ExecutionTrigger trigger)
        {
            Trigger = trigger;
            TaskName = syncTask.Name;
            _description1 = "";
            _description2 = "";
        }

        public void Success(IReadOnlyCollection<FailedSyncRecord> failedRecords)
        {
            string message = $"由{Functions.GetEnumDescription(Trigger)}触发的任务“{TaskName}”已执行完毕。";
            string? link = null;
            Action? linkAction = null;
            if (failedRecords.Count > 0)
            {
                message += $"其中{failedRecords.Count}份文件备份时出现异常。";
                link = "查看备份异常";
                linkAction = () => FailedSyncRecordWindow.ShowFailedRecords(failedRecords);
            }
            MessageWindow.ShowDialog(
                message: message,
                icon: MessageWindowIcon.Success,
                buttonStyle: MessageWindowButtonStyle.OK,
                location: MessageWindowLocation.RightBottom,
                autoCountDown: true,
                mandatoryTopMost: true,
                link: link,
                linkAction: linkAction
            );
        }

        public void SetQueryingState()
        {
            _syncProgressState = SyncProgressState.Querying;
            _description1 = "(1/4) 正在查找待备份文件...";
            _description2 = "已找到0份文件...";
            _value1 = 25d;
            _value2 = 0d;
            _isIndeterminate = true;
        }

        public void SetDealingWithUnexpectedFilesState(IReadOnlyCollection<FileInfo> unexpectedFiles, bool deleteUnexpectedFiles)
        {
            _syncProgressState = SyncProgressState.DealingWithUnexpectedFiles;
            Description1 = "(2/4) 正在处理目标目录中与源目录不同的文件...";
            _unexpectedFilesCount = unexpectedFiles.Count;
            Value1 = 50d;
            IsIndeterminate = false;
            UpdateDescription();

            CanDealWithUnexpectedFiles = !deleteUnexpectedFiles || deleteUnexpectedFiles && _unexpectedFilesCount > 0
                && (!Config.AlwaysTellsMeWhenDeleteFiles ||
                 MessageWindow.ShowDialog(
                     message: $"{_unexpectedFilesCount}份文件将，是否继续？",
                     icon: MessageWindowIcon.Information,
                     buttonStyle: MessageWindowButtonStyle.OKCancel,
                     location: MessageWindowLocation.RightBottom,
                     autoCountDown: true,
                     mandatoryTopMost: true,
                     link: "查看待删除文件",
                     linkAction: new Action(() => FilesListWindow.ShowFilesList(unexpectedFiles))
                ));
        }

        public void SetCopyingState()
        {
            _syncProgressState = SyncProgressState.Copying;
            Description1 = "(3/4) 正在备份文件...";
            Value1 = 75d;
            IsIndeterminate = false;
            UpdateDescription();
        }

        public void SetManagingDeletingFilesState()
        {
            _syncProgressState = SyncProgressState.ManagingDeletionFiles;
            Description1 = "(4/4) 正在处理过期文件...";
            Value1 = 100d;
            IsIndeterminate = true;
            UpdateDescription();
        }

        public bool CanManageDeletingFileRecords(IReadOnlyCollection<DeletionFileRecord> deletingFileRecords)
        {
            _foundDeletingFilesCount = deletingFileRecords.Count;
            IsIndeterminate = false;
            UpdateDescription();
            var filesList = deletingFileRecords.Select(record => new FileInfo(record.NewFileName!));
            return _foundDeletingFilesCount > 0 && (!Config.AlwaysTellsMeWhenDeleteFiles ||
                 MessageWindow.ShowDialog(
                     message: $"本次备份将删除{_foundDeletingFilesCount}份过期文件，是否删除？",
                     icon: MessageWindowIcon.Information,
                     buttonStyle: MessageWindowButtonStyle.OKCancel,
                     location: MessageWindowLocation.RightBottom,
                     autoCountDown: true,
                     mandatoryTopMost: true,
                     link: "查看待删除文件",
                     linkAction: new Action(() => FilesListWindow.ShowFilesList(filesList))
                ));
        }

        public void UpdateDescription()
        {
            Description2 = _syncProgressState switch
            {
                SyncProgressState.Querying => $"已找到{_foundSyncFilesCount}份文件...",
                SyncProgressState.DealingWithUnexpectedFiles => $"已处理{_dealedUnexpectedFilesCount}/{_unexpectedFilesCount}份文件...",
                SyncProgressState.Copying => $"已备份{_copiedSyncFilesCount}/{_foundSyncFilesCount}份文件...",
                SyncProgressState.ManagingDeletionFiles => $"已删除{_managedDeletingFilesCount}/{_foundDeletingFilesCount}份过期文件...",
                _ => throw new NotSupportedException()
            };
            Value2 = _syncProgressState switch
            {
                SyncProgressState.Querying => 100d,
                SyncProgressState.DealingWithUnexpectedFiles => _unexpectedFilesCount != 0 ? (double)_dealedUnexpectedFilesCount / _unexpectedFilesCount * 100 : 100d,
                SyncProgressState.Copying => _foundSyncFilesCount != 0 ? (double)_copiedSyncFilesCount / _foundSyncFilesCount * 100 : 100d,
                SyncProgressState.ManagingDeletionFiles => _foundDeletingFilesCount != 0 ? (double)_managedDeletingFilesCount / _foundDeletingFilesCount : 0d,
                _ => throw new NotSupportedException()
            };
        }

        public void AddFoundSyncFile()
        {
            Interlocked.Increment(ref _foundSyncFilesCount);
            UpdateDescription();
        }

        public void AddDealedUnexpectedFile()
        {
            Interlocked.Increment(ref _dealedUnexpectedFilesCount);
            UpdateDescription();
        }

        public void AddCopiedSyncFile()
        {
            Interlocked.Increment(ref _copiedSyncFilesCount);
            UpdateDescription();
        }

        public void AddManagedDeletionFile()
        {
            Interlocked.Increment(ref _managedDeletingFilesCount);
            UpdateDescription();
        }
    }

    public enum SyncProgressState
    {
        Querying,
        DealingWithUnexpectedFiles,
        Copying,
        ManagingDeletionFiles,
    }
}
