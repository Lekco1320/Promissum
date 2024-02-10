using System;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    /// <summary>
    /// The plan for execution of a task.
    /// </summary>
    [DataContract]
    public class SyncPlan
    {
        /// <summary>
        /// Specifies whether the plan is used.
        /// </summary>
        [DataMember]
        public bool UsePlan { get; set; }

        /// <summary>
        /// Specifies whether the task will be executed when its paths match.
        /// </summary>
        [DataMember]
        public bool WhenConnectDisk { get; set; }

        /// <summary>
        /// Specifies whether the plan should be periodic.
        /// </summary>
        [DataMember]
        public bool PeriodicSync { get; set; }

        /// <summary>
        /// The period of sync.
        /// </summary>
        [DataMember]
        public SyncPeriod SyncPeriod { get; set; }

        /// <summary>
        /// Specifies whether the task should be executed by interval.
        /// </summary>
        [DataMember]
        public bool IntervalSync { get; set; }

        /// <summary>
        /// The time span of the interval.
        /// </summary>
        [DataMember]
        public TimeSpan IntervalSpan { get; set; }

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        public SyncPlan()
        {
            UsePlan = false;
            WhenConnectDisk = false;
            IntervalSync = false;
            PeriodicSync = false;
            IntervalSpan = TimeSpan.FromHours(1d);
        }
    }

    /// <summary>
    /// Represents the period of <see cref="SyncPlan"/>.
    /// </summary>
    public enum SyncPeriod
    {
        /// <summary>
        /// Every Sunday.
        /// </summary>
        Sunday,

        /// <summary>
        /// Every Monday.
        /// </summary>
        Monday,

        /// <summary>
        /// Every Tuesday.
        /// </summary>
        Tuesday,

        /// <summary>
        /// Every Wednesday.
        /// </summary>
        Wednesday,

        /// <summary>
        /// Every Thursday.
        /// </summary>
        Thursday,

        /// <summary>
        /// Every Friday.
        /// </summary>
        Friday,

        /// <summary>
        /// Every Saturday.
        /// </summary>
        Saturday,

        /// <summary>
        /// Every Month.
        /// </summary>
        Month,

        /// <summary>
        /// Every Quarter.
        /// </summary>
        Quarter,

        /// <summary>
        /// Every Year.
        /// </summary>
        Year,
    }
}
