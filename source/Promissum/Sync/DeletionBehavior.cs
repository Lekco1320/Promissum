using System;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class DeletionBehavior
    {
        [DataMember]
        public bool ReserveFiles { get; set; }

        [DataMember]
        public bool MoveToDeletionPath { get; set; }

        [DataMember]
        public SyncPath DeletionPath { get; set; }

        [DataMember]
        public bool UseReserveTerm { get; set; }

        [DataMember]
        public TimeSpan ReserveTerm { get; set; }

        [DataMember]
        public bool MarkVersion { get; set; }

        [DataMember]
        public bool SetMaxVersion { get; set; }

        [DataMember]
        public int MaxVersion { get; set; }

        public bool NeedFindUnexpectedOnes { get => !ReserveFiles || MoveToDeletionPath; }

        public DeletionBehavior()
        {
            ReserveFiles = true;
            MarkVersion = false;
            MoveToDeletionPath = false;
            SetMaxVersion = false;
            MaxVersion = 3;
            ReserveTerm = TimeSpan.FromHours(1d);
            DeletionPath = new SyncPath(@"C:\");
        }
    }
}
