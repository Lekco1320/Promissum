using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    /// <summary>
    /// Represents a record storing the trigger, time, and key infomation of a sync execution.
    /// </summary>
    [DataContract]
    public class SyncRecord
    {
        /// <summary>
        /// The id of the record.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// The trigger of the sync execution.
        /// </summary>
        [DataMember]
        public ExecutionTrigger ExecutionTrigger { get; set; }

        /// <summary>
        /// The start time of the sync execution.
        /// </summary>
        [DataMember]
        public DateTime SyncStartTime { get; set; }

        /// <summary>
        /// The end time of the sync execution.
        /// </summary>
        [DataMember]
        public DateTime SyncEndTime { get; set; }

        /// <summary>
        /// The count of synced files in the sync execution.
        /// </summary>
        [DataMember]
        public int NewFilesCount { get; set; }

        /// <summary>
        /// The count of created directories in the sync execution.
        /// </summary>
        [DataMember]
        public int NewDirectoriesCount { get; set; }

        /// <summary>
        /// The count of moved files.
        /// </summary>
        [DataMember]
        public int MovedFilesCount { get; set; }

        /// <summary>
        /// The count of deleted files.
        /// </summary>
        [DataMember]
        public int DeletedFilesCount { get; set; }

        /// <summary>
        /// The count of deleted directories.
        /// </summary>
        [DataMember]
        public int DeletedDirectoriesCount { get; set; }

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        /// <param name="id">The id of the record.</param>
        /// <param name="trigger">The trigger of the sync execution.</param>
        /// <param name="syncStartTime">The start time of the sync execution.</param>
        /// <param name="syncEndTime">The end time of the sync execution.</param>
        /// <param name="syncData">Syncing data.</param>
        public SyncRecord(int id, ExecutionTrigger trigger, DateTime syncStartTime, DateTime syncEndTime, SyncData syncData)
        {
            Id = id;
            ExecutionTrigger = trigger;
            SyncStartTime = syncStartTime;
            SyncEndTime = syncEndTime;
            NewFilesCount = syncData.NewFiles.Count;
            NewDirectoriesCount = syncData.NewDirectories.Count;
            MovedFilesCount = syncData.MovedFiles.Count; 
            DeletedFilesCount = syncData.DeletedFiles.Count;
            DeletedDirectoriesCount = syncData.DeletedDirectories.Count;
        }
    }

    /// <summary>
    /// Represents what execute the sync task.
    /// </summary>
    public enum ExecutionTrigger
    {
        /// <summary>
        /// Execute the task manually.
        /// </summary>
        [Description("手动执行")]
        Manual,

        /// <summary>
        /// Execute the task by periodic plan.
        /// </summary>
        [Description("定期计划")]
        Period,

        /// <summary>
        /// Execute the task by interval plan.
        /// </summary>
        [Description("隔期计划")]
        Interval,

        /// <summary>
        /// Execute the task by the trigger of disk connection.
        /// </summary>
        [Description("硬盘接入")]
        ConnectDisk,
    }
}