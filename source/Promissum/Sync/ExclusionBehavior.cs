using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class ExclusionBehavior
    {
        [DataMember]
        public bool UseExclusion { get; set; }

        [DataMember]
        public ObservableCollection<SyncExclusion> Exclusions { get; set; }

        public ExclusionBehavior()
        {
            UseExclusion = false;
            Exclusions = new ObservableCollection<SyncExclusion>();
        }
    }
}
