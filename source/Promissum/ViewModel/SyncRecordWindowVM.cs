using Lekco.Promissum.Sync;
using Lekco.Promissum.Utility;
using Lekco.Promissum.View;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace Lekco.Promissum.ViewModel
{
    public class SyncRecordWindowVM : BindableBase
    {
        public SyncProject Project { get; set; }
        public SyncTask Task { get; set; }
        public DateTime SyncDataSetCreationTime { get; set; }
        public DateTime SyncDataSetLastWriteTime { get; set; }
        public int RecordsCount { get => ((ICollection)SyncRecords.Source).Count; }
        public CollectionViewSource SyncRecords { get; set; }
        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand OutputCommand { get; set; }
        public DelegateCommand<Window> CloseCommand { get; set; }

        public SyncRecordWindowVM(SyncProject parentProject, SyncTask syncTask)
        {
            Project = parentProject;
            Task = syncTask;
            SyncRecords = new CollectionViewSource();

            Task.GetDataSet(parentProject, dataset =>
            {
                SyncDataSetCreationTime = dataset.CreationTime;
                SyncDataSetLastWriteTime = dataset.LastWriteTime;
                SyncRecords.Source = dataset.SyncRecords;
            });

            SyncRecords.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));

            ClearCommand = new DelegateCommand(ClearData);
            OutputCommand = new DelegateCommand(OutputData);
            CloseCommand = new DelegateCommand<Window>(window => window.Close());
        }

        private void ClearData()
        {
            if (MessageWindow.ShowDialog(
                    message: $"是否要清除任务“{Task.Name}”的执行记录？",
                    icon: MessageWindowIcon.Warning,
                    buttonStyle: MessageWindowButtonStyle.OKCancel
                ))
            {
                try
                {
                    Task.GetDataSet(Project, dataset =>
                    {
                        dataset.ClearSyncRecords();
                        SyncDataSetCreationTime = dataset.CreationTime;
                        SyncDataSetLastWriteTime = dataset.LastWriteTime;
                        SyncRecords.Source = dataset.SyncRecords;
                    });
                    RaisePropertyChanged(nameof(RecordsCount));
                    RaisePropertyChanged(nameof(SyncDataSetCreationTime));
                    RaisePropertyChanged(nameof(SyncDataSetLastWriteTime));

                    MessageWindow.ShowDialog(
                        message: $"任务“{Task.Name}”的执行记录已清除完毕。",
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
            string path = Task.Name + "执行记录";
            if (!NPOIHelper.SaveAsXlsx("导出执行记录", ref path))
            {
                return;
            }

            string[] headers = { "序号", "触发形式", "开始时间", "结束时间", "备份文件数", "备份文件夹数", "移动文件数", "删除文件数", "删除文件夹数" };
            Func<object, dynamic>[] funcs =
            {
                new(obj => ((SyncRecord)obj).Id),
                new(obj => Functions.GetEnumDescription(((SyncRecord)obj).ExecutionTrigger)),
                new(obj => ((SyncRecord)obj).SyncStartTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new(obj => ((SyncRecord)obj).SyncEndTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new(obj => ((SyncRecord)obj).NewFilesCount),
                new(obj => ((SyncRecord)obj).NewDirectoriesCount),
                new(obj => ((SyncRecord)obj).MovedFilesCount),
                new(obj => ((SyncRecord)obj).DeletedFilesCount),
                new(obj => ((SyncRecord)obj).DeletedDirectoriesCount)
            };

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                NPOIHelper.GenerateWorkbook("执行记录", SyncRecords.View, headers, funcs).Write(fileStream);
                MessageWindow.ShowDialog(
                    message: $"任务“{Task.Name}”的执行记录已导出至“{path}”。",
                    icon: MessageWindowIcon.Success,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
            catch (Exception ex)
            {
                MessageWindow.ShowDialog(
                    message: $"任务“{Task.Name}”的执行记录导出失败：“{ex.Message}”。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
        }
    }
}
