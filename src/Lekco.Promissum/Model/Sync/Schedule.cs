using Lekco.Wpf.Utility;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// The schedule execution of a task.
    /// </summary>
    [DataContract]
    public class Schedule : INotifyPropertyChanged
    {
        /// <summary>
        /// Indicates whether execute a task when its drives get ready.
        /// </summary>
        [DataMember]
        public bool OnDriveReady { get; set; }

        /// <summary>
        /// Days on which the scheduled task will be executed.
        /// </summary>
        [DataMember]
        public Day OnScheduledDays { get; set; }

        /// <summary>
        /// Interval at which the scheduled task will be executed.
        /// </summary>
        [DataMember]
        public TimeSpan OnInterval { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        public Schedule()
        {
        }

        /// <summary>
        /// Indicates whether is due on interval.
        /// </summary>
        /// <param name="lastExecuteTime">Last execute time.</param>
        /// <returns><see langword="true"/> if needs; otherwise, returns <see langword="false"/>.</returns>
        public bool IsOnIntervalDue(DateTime lastExecuteTime)
        {
            var currentTime = DateTime.Now;
            return OnInterval != TimeSpan.Zero && currentTime - lastExecuteTime >= OnInterval;
        }

        /// <summary>
        /// Indicates whether is due on days.
        /// </summary>
        /// <param name="lastExecuteTime">Last execute time.</param>
        /// <returns><see langword="true"/> if needs; otherwise, returns <see langword="false"/>.</returns>
        public bool IsOnDaysDue(DateTime lastExecuteTime)
        {
            var currentTime = DateTime.Now;
            bool dayMatches = ((int)OnScheduledDays & 1 << (int)currentTime.DayOfWeek) != 0;
            bool monthMatches = ((int)OnScheduledDays & 1 << currentTime.Month + 6) != 0;
            return lastExecuteTime.Date != currentTime.Date && (dayMatches || monthMatches);
        }
    }

    /// <summary>
    /// Flags enum for describing days for sync.
    /// </summary>
    [Flags]
    public enum Day
    {
        /// <summary>
        /// None.
        /// </summary>
        [Index(0)]
        None = 0,

        /// <summary>
        /// Every Sunday.
        /// </summary>
        [Index(1)]
        Sunday = 1 << 0,

        /// <summary>
        /// Every Monday.
        /// </summary>
        [Index(2)]
        Monday = 1 << 1,

        /// <summary>
        /// Every Tuesday.
        /// </summary>
        [Index(3)]
        Tuesday = 1 << 2,

        /// <summary>
        /// Every Wednesday.
        /// </summary>
        [Index(4)]
        Wednesday = 1 << 3,

        /// <summary>
        /// Every Thursday.
        /// </summary>
        [Index(5)]
        Thursday = 1 << 4,

        /// <summary>
        /// Every Friday.
        /// </summary>
        [Index(6)]
        Friday = 1 << 5,

        /// <summary>
        /// Every Saturday.
        /// </summary>
        [Index(7)]
        Saturday = 1 << 6,

        /// <summary>
        /// 1st January.
        /// </summary>
        [Index(8)]
        January = 1 << 7,

        /// <summary>
        /// 1st February.
        /// </summary>
        [Index(9)]
        February = 1 << 8,

        /// <summary>
        /// 1st March.
        /// </summary>
        [Index(10)]
        March = 1 << 9,

        /// <summary>
        /// 1st Apirl.
        /// </summary>
        [Index(11)]
        Apirl = 1 << 10,

        /// <summary>
        /// 1st May.
        /// </summary>
        [Index(12)]
        May = 1 << 11,

        /// <summary>
        /// 1st June.
        /// </summary>
        [Index(13)]
        June = 1 << 12,

        /// <summary>
        /// 1st July.
        /// </summary>
        [Index(14)]
        July = 1 << 13,

        /// <summary>
        /// 1st August.
        /// </summary>
        [Index(15)]
        August = 1 << 14,

        /// <summary>
        /// 1st September.
        /// </summary>
        [Index(16)]
        September = 1 << 15,

        /// <summary>
        /// 1st October.
        /// </summary>
        [Index(17)]
        October = 1 << 16,

        /// <summary>
        /// 1st November.
        /// </summary>
        [Index(18)]
        November = 1 << 17,

        /// <summary>
        /// 1st December.
        /// </summary>
        [Index(19)]
        December = 1 << 18,

        /// <summary>
        /// 1st of every month.
        /// </summary>
        [Index(20)]
        Month = January | February | March | Apirl | May | June |
                July | August | September | October | November | December,

        /// <summary>
        /// Every quarter.
        /// </summary>
        [Index(21)]
        Quarter = January | Apirl | July | October,

        /// <summary>
        /// 1st January.
        /// </summary>
        [Index(22)]
        Year = January,
    }
}
