using System.ComponentModel;

namespace Lekco.Promissum.Model.Sync.Execution
{
    /// <summary>
    /// Trigger which execute task.
    /// </summary>
    public enum ExecutionTrigger
    {
        /// <summary>
        /// Executes manually.
        /// </summary>
        [Description("手动触发")]
        Manual,

        /// <summary>
        /// Executes on drives get ready.
        /// </summary>
        [Description("设备就绪")]
        OnDriveReady,

        /// <summary>
        /// On the scheduled days.
        /// </summary>
        [Description("定期触发")]
        OnScheduledDays,

        /// <summary>
        /// On interval.
        /// </summary>
        [Description("间隔触发")]
        OnInterval,
    }
}
