using Lekco.Promissum.Sync;
using Lekco.Promissum.Utility;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lekco.Promissum.ViewModel
{
    public class DeletionRecordWindowVM : BindableBase
    {
        public SyncProject Project { get; set; }
        public SyncTask Task { get; set; }
        public DateTime SyncDataSetCreationTime { get; set; }
        public DateTime SyncDataSetLastWriteTime { get; set; }
        public int RecordsCount { get => ((ICollection)DeletionRecords.Source).Count; }
        public CollectionViewSource DeletionRecords { get; set; }
        public DelegateCommand<TextBox> FilterCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand OutputCommand { get; set; }
        public DelegateCommand<Window> CloseCommand { get; set; }

        public DeletionRecordWindowVM(SyncProject parentProject, SyncTask syncTask)
        {
            Project = parentProject;
            Task = syncTask;

            var deletionFileRecords = new ObservableCollection<DeletionFileRecord>();
            Task.GetDataSet(parentProject, dataset =>
            {
                SyncDataSetCreationTime = dataset.CreationTime;
                SyncDataSetLastWriteTime = dataset.LastWriteTime;
                foreach (var dictionary in dataset.DeletionFileDictionary.Values)
                {
                    deletionFileRecords.AddRange(dictionary.Values);
                }
            });

            DeletionRecords = new CollectionViewSource()
            {
                Source = deletionFileRecords
            };
            DeletionRecords.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));

            FilterCommand = new DelegateCommand<TextBox>(box => FilterPath(box));
            ClearCommand = new DelegateCommand(ClearData);
            OutputCommand = new DelegateCommand(OutputData);
            CloseCommand = new DelegateCommand<Window>(window => window.Close());
        }

        private void FilterPath(TextBox box)
        {
            string path = box.Text;
            DeletionRecords.View.Filter = record => ((DeletionFileRecord)record).FileName.Contains(path)
                 || (((DeletionFileRecord)record).NewFileName ?? "").Contains(path);
            DeletionRecords.View.Refresh();
        }

        private void ClearData()
        {
            if (MessageWindow.ShowDialog(
                    message: $"是否要清除任务“{Task.Name}”的移动记录？",
                    icon: MessageWindowIcon.Warning,
                    buttonStyle: MessageWindowButtonStyle.OKCancel
                ))
            {
                try
                {
                    var deletionFileRecords = new ObservableCollection<DeletionFileRecord>();
                    Task.GetDataSet(Project, dataset =>
                    {
                        dataset.ClearDeletionRecords();
                        SyncDataSetCreationTime = dataset.CreationTime;
                        SyncDataSetLastWriteTime = dataset.LastWriteTime;
                        foreach (var dictionary in dataset.DeletionFileDictionary.Values)
                        {
                            deletionFileRecords.AddRange(dictionary.Values);
                        }
                        DeletionRecords.Source = deletionFileRecords;
                    });
                    RaisePropertyChanged(nameof(RecordsCount));
                    RaisePropertyChanged(nameof(SyncDataSetCreationTime));
                    RaisePropertyChanged(nameof(SyncDataSetLastWriteTime));

                    MessageWindow.ShowDialog(
                        message: $"任务“{Task.Name}”的移动记录已清除完毕。",
                        icon: MessageWindowIcon.Success,
                        buttonStyle: MessageWindowButtonStyle.OK
                    );
                }
                catch
                {
                    MessageWindow.ShowDialog(
                        message: $"任务“{Task.Name}”已被删除，无法清除数据。",
                        icon: MessageWindowIcon.Error,
                        buttonStyle: MessageWindowButtonStyle.OK
                    );
                }
            }
        }

        private void OutputData()
        {
            string path = Task.Name + "移动记录";
            if (!NPOIHelper.SaveAsXlsx("导出移动记录", ref path))
            {
                return;
            }

            string[] headers = { "序号", "文件原路径", "文件新路径", "文件大小", "创建时间", "修改时间", "版本号", "已删除" };//, "是否移除" };
            Func<object, dynamic>[] funcs =
            {
                new(obj => ((DeletionFileRecord)obj).Id),
                new(obj => Task.DestinationPath.DriveName + ((DeletionFileRecord)obj).FileName),
                new(obj => ((DeletionFileRecord)obj).NewFileName ?? ""),
                new(obj => Functions.FormatBytesLength(((DeletionFileRecord)obj).FileSize)),
                new(obj => ((DeletionFileRecord)obj).CreationTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new(obj => ((DeletionFileRecord)obj).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new(obj => ((DeletionFileRecord)obj).Version),
                new(obj => ((DeletionFileRecord)obj).IsDeleted),
            };

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                NPOIHelper.GenerateWorkbook("移动记录", DeletionRecords.View, headers, funcs, new int[] { 1, 2 }).Write(fileStream);
                MessageWindow.ShowDialog(
                    message: $"任务“{Task.Name}”的移动记录已导出至“{path}”。",
                    icon: MessageWindowIcon.Success,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
            catch (Exception ex)
            {
                MessageWindow.ShowDialog(
                    message: $"任务“{Task.Name}”的移动记录导出失败：“{ex.Message}”。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
        }
    }
}
