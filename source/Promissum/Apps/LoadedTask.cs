using Lekco.Promissum.Sync;

namespace Lekco.Promissum.Apps
{
    public class LoadedTask
    {
        public SyncProject ParentProject { get; set; }
        public SyncTask Task { get; set; }
        public ExecutionTrigger ExecutionTrigger { get; set; }

        public LoadedTask(SyncProject parentProject, SyncTask syncTask, ExecutionTrigger trigger)
        {
            ParentProject = parentProject;
            Task = syncTask;
            ExecutionTrigger = trigger;
        }
    }
}
