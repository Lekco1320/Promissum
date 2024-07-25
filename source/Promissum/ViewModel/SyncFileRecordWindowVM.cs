using Lekco.Promissum.Sync;
using Lekco.Promissum.Utility;
using Lekco.Promissum.View;
using NPOI.XSSF.UserModel;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lekco.Promissum.ViewModel
{
    public class SyncFileRecordWindowVM : BindableBase
    {
        public SyncProject Project { get; set; }
        public SyncTask Task { get; set; }
        public DateTime SyncDataSetCreationTime { get; set; }
        public DateTime SyncDataSetLastWriteTime { get; set; }
        public int RecordsCount { get => ((ICollection)SyncFileRecords.Source).Count; }
        public CollectionViewSource SyncFileRecords { get; set; }
        public DelegateCommand<TextBox> FilterCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand OutputCommand { get; set; }
        public DelegateCommand<Window> CloseCommand { get; set; }

        public SyncFileRecordWindowVM(SyncProject parentProject, SyncTask syncTask)
        {
            Project = parentProject;
            Task = syncTask;
            SyncFileRecords = new CollectionViewSource();
            Task.GetDataSet(parentProject, dataset =>
            {
                SyncDataSetCreationTime = dataset.CreationTime;
                SyncDataSetLastWriteTime = dataset.LastWriteTime;
                SyncFileRecords.Source = dataset.SyncFileDictionary.Values;
            });

            SyncFileRecords.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));

            FilterCommand = new DelegateCommand<TextBox>(box => FilterPath(box));
            ClearCommand = new DelegateCommand(ClearData);
            OutputCommand = new DelegateCommand(OutputData);
            CloseCommand = new DelegateCommand<Window>(window => window.Close());
        }

        private void FilterPath(TextBox box)
        {
            SyncFileRecords.View.Filter = record => ((SyncFileRecord)record).FileName.Contains(box.Text);
            SyncFileRecords.View.Refresh();
        }
        
        private void ClearData()
        {
            if (MessageWindow.ShowDialog(
                    message: $"是否要清除任务“{Task.Name}”的备份记录？",
                    icon: MessageWindowIcon.Warning,
                    buttonStyle: MessageWindowButtonStyle.OKCancel
                ))
            {
                try
                {
                    Task.GetDataSet(Project, dataset =>
                    {
                        dataset.ClearSyncFileRecords();
                        SyncDataSetCreationTime = dataset.CreationTime;
                        SyncDataSetLastWriteTime = dataset.LastWriteTime;
                        SyncFileRecords.Source = dataset.SyncFileDictionary.Values;
                    });
                    RaisePropertyChanged(nameof(RecordsCount));
                    RaisePropertyChanged(nameof(SyncDataSetCreationTime));
                    RaisePropertyChanged(nameof(SyncDataSetLastWriteTime));

                    MessageWindow.ShowDialog(
                        message: $"任务“{Task.Name}”的备份记录已清除完毕。",
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
            string path = Task.Name + "备份记录";
            if (!NPOIHelper.SaveAsXlsx("导出备份记录", ref path))
            {
                return;
            }

            var wb = new XSSFWorkbook();
            var ws = wb.CreateSheet("备份记录");
            int row_id = 0;
            var row = (XSSFRow)ws.CreateRow(row_id++);

            string[] headers = { "序号", "备份文件路径", "文件大小", "创建时间", "修改时间", "最后同步时间", "备份次数" };
            Func<object, dynamic>[] funcs =
            {
                new(obj => ((SyncFileRecord)obj).Id),
                new(obj => Task.DestinationPath.DriveName + ((SyncFileRecord)obj).FileName),
                new(obj => Functions.FormatBytesLength(((SyncFileRecord)obj).FileSize)),
                new(obj => ((SyncFileRecord)obj).CreationTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new(obj => ((SyncFileRecord)obj).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new(obj => ((SyncFileRecord)obj).LastUpdateTime.ToString("yyyy/MM/dd HH:mm:ss")),
                new(obj => ((SyncFileRecord)obj).Version),
            };

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                NPOIHelper.GenerateWorkbook("备份记录", SyncFileRecords.View, headers, funcs, new int[] { 1 }).Write(fileStream);
                MessageWindow.ShowDialog(
                    message: $"任务“{Task.Name}”的备份记录已导出至“{path}”。",
                    icon: MessageWindowIcon.Success,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
            catch (Exception ex)
            {
                MessageWindow.ShowDialog(
                    message: $"任务“{Task.Name}”的备份记录导出失败：“{ex.Message}”。",
                    icon: MessageWindowIcon.Error,
                    buttonStyle: MessageWindowButtonStyle.OK
                );
            }
        }
    }
}
