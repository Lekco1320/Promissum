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
    [DataContract]
    public class SyncDataSet
    {
        [DataMember]
        public DateTime CreationTime { get; protected set; }

        [DataMember]
        public DateTime LastWriteTime { get; protected set; }

        [DataMember]
        public ConcurrentDictionary<string, SyncFileRecord> SyncFileDictionary { get; protected set; }

        [DataMember]
        public ConcurrentDictionary<string, SortedDictionary<int, DeletionFileRecord>> DeletionFileDictionary { get; protected set; }

        [DataMember]
        public List<SyncRecord> SyncRecords { get; protected set; }

        [DataMember]
        private int _syncFileRecordIdPtr;

        [DataMember]
        private int _deletionRecordIdPtr;

        private static readonly object _syncRecordIdPtrLock;
        private static readonly object _deletionRecordPtrLock;

        protected string _fileName;

        static SyncDataSet()
        {
            _syncRecordIdPtrLock = new object();
            _deletionRecordPtrLock = new object();
        }

        public SyncDataSet(string fileName)
        {
            CreationTime = DateTime.Now;
            LastWriteTime = DateTime.Now;
            SyncFileDictionary = new ConcurrentDictionary<string, SyncFileRecord>();
            DeletionFileDictionary = new ConcurrentDictionary<string, SortedDictionary<int, DeletionFileRecord>>();
            SyncRecords = new List<SyncRecord>();
            _fileName = fileName;
            _syncFileRecordIdPtr = 1;
            _deletionRecordIdPtr = 1;
        }

        protected void UpdateWriteTime()
        {
            LastWriteTime = DateTime.Now;
        }

        protected void AddSyncRecord(FileInfo fileInfo)
        {
            int id;
            lock (_syncRecordIdPtrLock)
            {
                id = _syncFileRecordIdPtr;
                Interlocked.Increment(ref _syncFileRecordIdPtr);
            }
            var newRecord = new SyncFileRecord(id, fileInfo);
            SyncFileDictionary.TryAdd(newRecord.FileName, newRecord);
            UpdateWriteTime();
        }

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

        public bool AddDeletionRecord(FileInfo deletedInfo, string? deletionFolder, bool markVersion, [MaybeNullWhen(false)] out string newFileName)
        {
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
                if (markVersion)
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
            int id;
            lock (_deletionRecordPtrLock)
            {
                id = _deletionRecordIdPtr;
                Interlocked.Increment(ref _deletionRecordIdPtr);
            }
            var newRecord = new DeletionFileRecord(id, deletedInfo, newFileName, version);
            deletionRecords.Add(deletionRecords.Count, newRecord);
            UpdateWriteTime();
            return newFileName != null;
        }

        public IEnumerable<DeletionFileRecord> UpdateDeletionRecordsByVersion(string deletedFileName, int maxVersion)
        {
            var result = new List<DeletionFileRecord>();
            if (!TryGetDeletionRecords(deletedFileName, out SortedDictionary<int, DeletionFileRecord>? deletionRecords))
            {
                return result;
            }
            int deletedNum = 0, endNum = deletionRecords.Count - maxVersion;
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

        public IEnumerable<DeletionFileRecord> UpdateDeletionRecordsByTime(string deletedFileName, TimeSpan maxDuration, DateTime refTime)
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
                    if (refTime - record.LastWriteTime > maxDuration)
                    {
                        result.Add(record);
                    }
                }
            }
            UpdateWriteTime();
            return result;
        }

        public void AddSyncRecord(ExecutionTrigger trigger, DateTime syncStartTime, DateTime syncEndTime, SyncData syncData)
        {
            SyncRecords.Add(new SyncRecord(SyncRecords.Count + 1, trigger, syncStartTime, syncEndTime, syncData));
            UpdateWriteTime();
        }

        public void ClearSyncFileRecords()
        {
            SyncFileDictionary.Clear();
            _syncFileRecordIdPtr = 1;
            UpdateWriteTime();
        }

        public void ClearSyncRecords()
        {
            SyncRecords.Clear();
            UpdateWriteTime();
        }

        public void ClearDeletionRecords()
        {
            DeletionFileDictionary.Clear();
            _deletionRecordIdPtr = 1;
            UpdateWriteTime();
        }

        public void ClearAllRecords()
        {
            ClearSyncFileRecords();
            ClearSyncRecords();
            ClearDeletionRecords();
            UpdateWriteTime();
        }

        public static SyncDataSet ReadFromFile(string fileName)
        {
            var result = Functions.ReadFromFile<SyncDataSet>(fileName);
            result._fileName = fileName;
            return result;
        }

        public void Save()
        {
            Functions.SaveAsFile(this, _fileName);
        }
    }
}
