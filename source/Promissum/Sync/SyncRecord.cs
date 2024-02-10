using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class SyncRecord
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public ExecutionTrigger ExecutionTrigger { get; set; }

        [DataMember]
        public DateTime SyncStartTime { get; set; }

        [DataMember]
        public DateTime SyncEndTime { get; set; }

        [DataMember]
        public int NewFilesCount { get; set; }

        [DataMember]
        public int NewDirectoriesCount { get; set; }

        [DataMember]
        public int MovedFilesCount { get; set; }

        [DataMember]
        public int DeletedFilesCount { get; set; }

        [DataMember]
        public int DeletedDirectoriesCount { get; set; }

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

    public enum ExecutionTrigger
    {
        [Description("手动执行")]
        Manual,

        [Description("定期计划")]
        Period,

        [Description("隔期计划")]
        Interval,

        [Description("硬盘接入")]
        ConnectDisk,
    }
}
