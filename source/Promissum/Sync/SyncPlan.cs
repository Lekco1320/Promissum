using System;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class SyncPlan
    {
        [DataMember]
        public bool UsePlan { get; set; }

        [DataMember]
        public bool WhenConnectDisk { get; set; }

        [DataMember]
        public bool PeriodicSync { get; set; }

        [DataMember]
        public SyncPeriod SyncPeriod { get; set; }

        [DataMember]
        public bool IntervalSync { get; set; }

        [DataMember]
        public TimeSpan IntervalSpan { get; set; }

        public SyncPlan()
        {
            UsePlan = false;
            WhenConnectDisk = false;
            IntervalSync = false;
            PeriodicSync = false;
            IntervalSpan = TimeSpan.FromHours(1d);
        }
    }

    public enum SyncPeriod
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Month,
        Quarter,
        Year,
    }
}
