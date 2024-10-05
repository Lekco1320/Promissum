using Lekco.Promissum.Model.Sync.Base;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Record
{
    /// <summary>
    /// Record for describing exceptions during sync.
    /// </summary>
    [DataContract]
    public class ExceptionRecord : RecordBase
    {
        /// <summary>
        /// Full name of file.
        /// </summary>
        [DataMember]
        public string FileFullName { get; set; }

        /// <summary>
        /// Time the exception occurred.
        /// </summary>
        [DataMember]
        public DateTime OccurredTime { get; set; }

        /// <summary>
        /// Type of the operation when exception occurred.
        /// </summary>
        [DataMember]
        public OperationType OperationType { get; set; }

        /// <summary>
        /// Type of exception.
        /// </summary>
        [DataMember]
        public ExceptionType ExceptionType { get; set; }

        /// <summary>
        /// The message of the exception.
        /// </summary>
        [DataMember]
        public string ExceptionMessage { get; set; }

        public ExecutionRecord? ExecutionRecord { get; set; }

        public int ExecutionID { get; set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public ExceptionRecord()
        {
            FileFullName = "";
            ExceptionMessage = "";
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fileFullName">Full name of file.</param>
        /// <param name="actionType">Type of taken action when exception occurred.</param>
        /// <param name="exceptionType">Type of exception.</param>
        /// <param name="exceptionMessage">The message of the exception.</param>
        public ExceptionRecord(string fileFullName, OperationType actionType, ExceptionType exceptionType, string exceptionMessage)
        {
            FileFullName = fileFullName;
            OccurredTime = DateTime.Now;
            OperationType = actionType;
            ExceptionType = exceptionType;
            ExceptionMessage = exceptionMessage;
        }
    }

    /// <summary>
    /// Type of taken action during sync.
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Copy files or directories.
        /// </summary>
        [Description("复制")]
        Copy,

        /// <summary>
        /// Move files or directories.
        /// </summary>
        [Description("移动")]
        Move,

        /// <summary>
        /// Create files or directories.
        /// </summary>
        [Description("创建")]
        Create,

        /// <summary>
        /// Delete files or directories.
        /// </summary>
        [Description("删除")]
        Delete,
    }

    /// <summary>
    /// Type of occurred exception during sync.
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// Unknown exception.
        /// </summary>
        [Description("未知异常")]
        Unknown,

        /// <summary>
        /// Exception that the file already exists.
        /// </summary>
        [Description("文件已存在")]
        FileAlreadyExists,

        /// <summary>
        /// Exception that given path is invalid.
        /// </summary>
        [Description("路径非法或硬盘已断开")]
        InvalidPath,

        /// <summary>
        /// Exception that do not have permission to access.
        /// </summary>
        [Description("无文件访问权限")]
        NoPermission,

        /// <summary>
        /// Exception that file is read only or access is unauthorized.
        /// </summary>
        [Description("目标文件只读")]
        UnauthorizedAccess,

        /// <summary>
        /// Exception that file doesn't exist.
        /// </summary>
        [Description("找不到文件")]
        FileNotFound,

        /// <summary>
        /// Exception that directory doesn't exist.
        /// </summary>
        [Description("找不到目录")]
        DirectoryNotFound,

        /// <summary>
        /// Exception that path is too long.
        /// </summary>
        [Description("路径过长")]
        PathTooLong,

        /// <summary>
        /// Exception that file is occupied.
        /// </summary>
        [Description("文件被占用")]
        FileOccupied,

        /// <summary>
        /// Connection to drive is missing.
        /// </summary>
        [Description("驱动器连接丢失")]
        DriveMissing,
    }
}
