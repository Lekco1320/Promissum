using System;
using System.ComponentModel;
using System.IO;

namespace Lekco.Promissum.Sync
{
    public class FailedSyncRecord
    {
        public FileSystemInfo Info { get; set; }
        public DateTime ExceptionTime { get; set; }
        public SyncAction Action { get; set; }
        public SyncFailedFlag Flag { get; set; }

        public FailedSyncRecord(FileSystemInfo info, SyncAction action, SyncFailedFlag flag)
        {
            ExceptionTime = DateTime.Now;
            Info = info;
            Action = action;
            Flag = flag;
        }
    }

    public enum SyncAction
    {
        [Description("复制")]
        Copy,
        [Description("移动")]
        Move,
        [Description("创建")]
        Create,
        [Description("删除")]
        Delete,
    }

    public enum SyncFailedFlag
    {
        [Description("文件已存在")]
        FileAlreadyExists,
        [Description("路径非法或硬盘已断开")]
        InvalidPath,
        [Description("无文件访问权限")]
        NoPermission,
        [Description("目标文件只读")]
        UnauthorizedAccess,
        [Description("找不到文件")]
        FileNotFound,
        [Description("找不到目录")]
        DirectoryNotFound,
        [Description("路径过长")]
        PathTooLong,
    }
}
