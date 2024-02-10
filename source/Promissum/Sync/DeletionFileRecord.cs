using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class DeletionFileRecord : FileRecord
    {
        [DataMember]
        public string? NewFileName { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        public DeletionFileRecord(int id, FileInfo deletedInfo, string? newFileName, int version)
            : base(id, deletedInfo)
        {
            NewFileName = newFileName;
            IsDeleted = false;
            Version = version;
        }
    }
}
