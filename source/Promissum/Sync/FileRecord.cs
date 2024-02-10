using Lekco.Promissum.Utility;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public abstract class FileRecord
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public long FileSize { get; set; }

        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public DateTime LastWriteTime { get; set; }

        [DataMember]
        public int Version { get; set; }

        public FileRecord(int id, FileInfo fileInfo)
        {
            Id = id;
            FileName = Functions.RemoveDiskName(fileInfo.FullName);
            FileSize = fileInfo.Length;
            CreationTime = fileInfo.CreationTime;
            LastWriteTime = fileInfo.LastWriteTime;
            Version = 1;
        }
    }
}
