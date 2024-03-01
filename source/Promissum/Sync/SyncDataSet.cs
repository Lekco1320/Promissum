using Lekco.Promissum.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;

namespace Lekco.Promissum.Sync
{
    /// <summary>
    /// The data set storing all information and records of a sync task.
    /// </summary>
    [DataContract]
    public class SyncDataSet
    {
        /// <summary>
        /// Creation time of the task.
        /// </summary>
        [DataMember]
        public DateTime CreationTime { get; protected set; }

        /// <summary>
        /// Last time the data set was written.
        /// </summary>
        [DataMember]
        public DateTime LastWriteTime { get; protected set; }

        /// <summary>
        /// The dictionary storing sync records and their file names removed disk names.
        /// </summary>
        [DataMember]
        public ConcurrentDictionary<string, SyncFileRecord> SyncFileDictionary { get; protected set; }

        /// <summary>
        /// The dictionary storing files' name and related dictionaries which stores history versions of a specified file.
        /// </summary>
        [DataMember]
        public ConcurrentDictionary<string, SortedDictionary<int, DeletionFileRecord>> DeletionFileDictionary { get; protected set; }

        /// <summary>
        /// The list storing sync records.
        /// </summary>
        [DataMember]
        public List<SyncRecord> SyncRecords { get; protected set; }

        /// <summary>
        /// Current index of sync records in this data set.
        /// </summary>
        [DataMember]
        private int _syncFileRecordIdPtr;

        /// <summary>
        /// Current index of deletion records in this data set.
        /// </summary>
        [DataMember]
        private int _deletionRecordIdPtr;

        /// <summary>
        /// The file name of the data set.
        /// </summary>
        protected string _fileName;

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        /// <param name="fileName">The file name of the data set.</param>
        public SyncDataSet(string fileName)
        {
            CreationTime = DateTime.Now;
            LastWriteTime = DateTime.Now;
            SyncFileDictionary = new ConcurrentDictionary<string, SyncFileRecord>();
            DeletionFileDictionary = new ConcurrentDictionary<string, SortedDictionary<int, DeletionFileRecord>>();
            SyncRecords = new List<SyncRecord>();
            _fileName = fileName;
        }

        /// <summary>
        /// Update <see cref="LastWriteTime"/>.
        /// </summary>
        protected void UpdateWriteTime()
        {
            LastWriteTime = DateTime.Now;
        }

        /// <summary>
        /// Add a new <see cref="SyncFileRecord"/>.
        /// </summary>
        /// <param name="fileInfo">The specified file info.</param>
        protected void AddSyncRecord(FileInfo fileInfo)
        {
            int id = Interlocked.Increment(ref _syncFileRecordIdPtr);
            var newRecord = new SyncFileRecord(id, fileInfo);
            SyncFileDictionary.TryAdd(newRecord.FileName, newRecord);
            UpdateWriteTime();
        }

        /// <summary>
        /// Try to get a sync 
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="syncRecord"></param>
        /// <returns></returns>
        protected bool TryGetSyncFileRecord(FileInfo fileInfo, [MaybeNullWhen(false)] out SyncFileRecord syncRecord)
        {
            string removedName = Functions.RemoveDiskName(fileInfo.FullName);
            if (SyncFileDictionary.TryGetValue(removedName, out SyncFileRecord? result))
            {
                syncRecord = result;
                return true;
            }
            syncRecord = null;
            return false;
        }

        /// <summary>
        /// Update or add a record by a specified file info.
        /// </summary>
        /// <param name="fileInfo">The specified file info.</param>
        public void UpdateOrAddSyncFileRecord(FileInfo fileInfo)
        {
            if (TryGetSyncFileRecord(fileInfo, out SyncFileRecord? syncFileRecord))
            {
                syncFileRecord.Update(fileInfo);
                UpdateWriteTime();
                return;
            }
            AddSyncRecord(fileInfo);
            UpdateWriteTime();
        }

        /// <summary>
        /// Try to get a dictionary storing related deletion records by a specifed file name.
        /// </summary>
        /// <param name="deletedFileName">The specified file name.</param>
        /// <param name="deletionRecords">Expected dictionary.</param>
        /// <returns><see langword="true"/> if find successfully; otherwise, returns <see langword="false"/>.</returns>
        protected bool TryGetDeletionRecords(string deletedFileName, [MaybeNullWhen(false)] out SortedDictionary<int, DeletionFileRecord> deletionRecords)
        {
            string removedName = Functions.RemoveDiskName(deletedFileName);
            if (DeletionFileDictionary.TryGetValue(removedName, out SortedDictionary<int, DeletionFileRecord>? result))
            {
                deletionRecords = result;
                return true;
            }
            deletionRecords = null;
            return false;
        }

        /// <summary>
        /// Add a new <see cref="DeletionFileRecord"/>.
        /// </summary>
        /// <param name="deletedInfo">The specified file info.</param>
        /// <param name="deletionBehavior">The bahavior of deletion.</param>
        /// <param name="newFileName">The new file name.</param>
        /// <returns><see langword="true"/> if add successfully; otherwise, returns <see langword="false"/>.</returns>
        public bool AddDeletionRecord(FileInfo deletedInfo, DeletionBehavior deletionBehavior, [MaybeNullWhen(false)] out string newFileName)
        {
            string? deletionFolder = deletionBehavior.ReserveFiles ? deletionBehavior.DeletionPath.FullPath : null;
            if (!TryGetDeletionRecords(deletedInfo.FullName, out SortedDictionary<int, DeletionFileRecord>? deletionRecords))
            {
                var newDictionary = new SortedDictionary<int, DeletionFileRecord>();
                DeletionFileDictionary.TryAdd(Functions.RemoveDiskName(deletedInfo.FullName), newDictionary);
                deletionRecords = newDictionary;
            }
            newFileName = null;
            int version = deletionRecords.Count + 1;
            if (deletionFolder != null)
            {
                if (deletionBehavior.MarkVersion)
                {
                    string extension = deletedInfo.Extension == "." ? "" : deletedInfo.Extension;
                    newFileName = deletionFolder + '\\' + Path.GetFileNameWithoutExtension(deletedInfo.Name)
                                  + $"_{version}" + extension;
                }
                else
                {
                    newFileName = deletionFolder + deletedInfo.Name;
                }
            }
            int id = Interlocked.Increment(ref _deletionRecordIdPtr);
            var newRecord = new DeletionFileRecord(id, deletedInfo, newFileName, version);
            deletionRecords.Add(deletionRecords.Count, newRecord);
            UpdateWriteTime();
            return newFileName != null;
        }

        /// <summary>
        /// Manage and update deletion records by files' version.
        /// </summary>
        /// <param name="deletedFileName">The specified file's name.</param>
        /// <param name="deletionBehavior">The behavior of deletion.</param>
        /// <returns>A collection of records need deleting.</returns>
        public IEnumerable<DeletionFileRecord> UpdateDeletionRecordsByVersion(string deletedFileName, DeletionBehavior deletionBehavior)
        {
            var result = new List<DeletionFileRecord>();
            if (!TryGetDeletionRecords(deletedFileName, out SortedDictionary<int, DeletionFileRecord>? deletionRecords))
            {
                return result;
            }
            int deletedNum = 0, endNum = deletionRecords.Count - deletionBehavior.MaxVersion;
            foreach (var deletionPair in deletionRecords)
            {
                var record = deletionPair.Value;
                if (record.NewFileName != null)
                {
                    record.IsDeleted = !File.Exists(record.NewFileName);
                    if (++deletedNum <= endNum && !record.IsDeleted)
                    {
                        result.Add(record);
                    }
                }
            }
            UpdateWriteTime();
            return result;
        }

        /// <summary>
        /// Manage and update deletion records by files' last write time.
        /// </summary>
        /// <param name="deletedFileName">The specified file's name.</param>
        /// <param name="deletionBehavior">The behavior of deletion.</param>
        /// <param name="refTime">The execution time of current task.</param>
        /// <returns>A collection of records need deleting.</returns>
        public IEnumerable<DeletionFileRecord> UpdateDeletionRecordsByTime(string deletedFileName, DeletionBehavior deletionBehavior, DateTime refTime)
        {
            var result = new List<DeletionFileRecord>();
            if (!TryGetDeletionRecords(deletedFileName, out SortedDictionary<int, DeletionFileRecord>? deletionRecords))
            {
                return result;
            }
            foreach (var deletionPair in deletionRecords)
            {
                var record = deletionPair.Value;
                if (record.NewFileName != null && !record.IsDeleted)
                {
                    var info = new FileInfo(record.NewFileName);
                    record.IsDeleted = !info.Exists;
                    if (refTime - record.LastWriteTime > deletionBehavior.ReserveTerm)
                    {
                        result.Add(record);
                    }
                }
            }
            UpdateWriteTime();
            return result;
        }

        /// <summary>
        /// Add a <see cref="SyncRecord"/>.
        /// </summary>
        /// <param name="trigger">The execution trigger of the task.</param>
        /// <param name="syncStartTime">The start time of execution.</param>
        /// <param name="syncEndTime">The end time of execution.</param>
        /// <param name="syncData">Data of sync execution.</param>
        public void AddSyncRecord(ExecutionTrigger trigger, DateTime syncStartTime, DateTime syncEndTime, SyncData syncData)
        {
            SyncRecords.Add(new SyncRecord(SyncRecords.Count + 1, trigger, syncStartTime, syncEndTime, syncData));
            UpdateWriteTime();
        }

        /// <summary>
        /// Clear all <see cref="SyncFileRecord"/>.
        /// </summary>
        public void ClearSyncFileRecords()
        {
            SyncFileDictionary.Clear();
            _syncFileRecordIdPtr = 0;
            UpdateWriteTime();
        }

        /// <summary>
        /// Clear all <see cref="SyncRecord"/>.
        /// </summary>
        public void ClearSyncRecords()
        {
            SyncRecords.Clear();
            UpdateWriteTime();
        }

        /// <summary>
        /// Clear all <see cref="DeletionFileRecord"/>.
        /// </summary>
        public void ClearDeletionRecords()
        {
            DeletionFileDictionary.Clear();
            _deletionRecordIdPtr = 0;
            UpdateWriteTime();
        }

        /// <summary>
        /// Clear all records.
        /// </summary>
        public void ClearAllRecords()
        {
            ClearSyncFileRecords();
            ClearSyncRecords();
            ClearDeletionRecords();
            UpdateWriteTime();
        }
        /// <summary>
        /// Get an instance data from a file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>An instance specified by the file.</returns>
        public static SyncDataSet ReadFromFile(string fileName)
        {
            var result = Functions.ReadFromFile<SyncDataSet>(fileName);
            result._fileName = fileName;
            return result;
        }

        /// <summary>
        /// Save the data set as a file.
        /// </summary>
        public void Save()
        {
            Functions.SaveAsFile(this, _fileName);
        }
    }
}