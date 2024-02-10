using System;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class SyncFileRecord : FileRecord
    {
        [DataMember]
        public DateTime LastUpdateTime { get; set; }

        public SyncFileRecord(int id, FileInfo fileInfo)
            : base(id, fileInfo)
        {
            LastUpdateTime = DateTime.Now;
        }

        public void Update(FileInfo fileInfo)
        {
            FileSize = fileInfo.Length;
            CreationTime = fileInfo.CreationTime;
            LastWriteTime = fileInfo.LastWriteTime;
            LastUpdateTime = DateTime.Now;
            ++Version;
        }
    }
}
