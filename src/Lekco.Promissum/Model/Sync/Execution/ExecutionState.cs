namespace Lekco.Promissum.Model.Sync.Execution
{
    /// <summary>
    /// The state of execution.
    /// </summary>
    public enum ExecutionState
    {
        /// <summary>
        /// State of preparing.
        /// </summary>
        Prepare,

        /// <summary>
        /// State of constructing directory trees.
        /// </summary>
        ConstructTree,

        /// <summary>
        /// State of cleaning up destination path.
        /// </summary>
        CleanUpDestinationPath,

        /// <summary>
        /// State of cleaning up reserved path.
        /// </summary>
        CleanUpReservedPath,

        /// <summary>
        /// State of syncing files.
        /// </summary>
        SyncFiles,

        /// <summary>
        /// State of completion.
        /// </summary>
        Completion,
    }
}
