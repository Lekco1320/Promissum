using Lekco.Promissum.Model.Sync.Base;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Record
{
    /// <summary>
    /// Record for describing sync's execution.
    /// </summary>
    [DataContract]
    public class ExecutionRecord : RecordBase
    {
        /// <summary>
        /// The trigger executes the task.
        /// </summary>
        [DataMember]
        public ExecutionTrigger ExecutionTrigger { get; set; }

        /// <summary>
        /// Start time of the execution.
        /// </summary>
        [DataMember]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time of the execution.
        /// </summary>
        [DataMember]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Count of synced files during the execution.
        /// </summary>
        [DataMember]
        public int SyncedFilesCount { get; set; }

        /// <summary>
        /// Count of reserved files during the execution.
        /// </summary>
        [DataMember]
        public int ReservedFilesCount { get; set; }

        /// <summary>
        /// Count of deleted files during the execution.
        /// </summary>
        [DataMember]
        public int DeletedFilesCount { get; set; }

        /// <summary>
        /// Count of deleted directories during the execution.
        /// </summary>
        [DataMember]
        public int DeletedDirectoriesCount { get; set; }

        /// <summary>
        /// List of exception records during the execution.
        /// </summary>
        [DataMember]
        public List<ExceptionRecord> ExceptionRecords { get; set; }

        public ExecutionRecord()
        {
            ExceptionRecords = new List<ExceptionRecord>();
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="id">Unique ID of the record.</param>
        /// <param name="data">Execution data.</param>
        public ExecutionRecord(ExecutionData data)
        {
            StartTime = data.StartTime;
            EndTime = data.EndTime;
            ExecutionTrigger = data.ExecutionTrigger;
            SyncedFilesCount = data.SyncedFiles.Count;
            ReservedFilesCount = data.ReservedFiles.Count;
            DeletedFilesCount = data.DeletedDestinationFiles.Count + data.DeletedReservedFiles.Count;
            DeletedDirectoriesCount = data.DeletedDirectories.Count;

            ExceptionRecords = new List<ExceptionRecord>();
            foreach (var record in data.ExceptionRecords)
            {
                record.ExecutionRecord = this;
                ExceptionRecords.Add(record);
            }
        }
    }
}
