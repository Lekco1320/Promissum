using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// The base class for describing a record.
    /// </summary>
    [DataContract]
    public abstract class RecordBase
    {
        /// <summary>
        /// Unique ID of the record as a primary key.
        /// </summary>
        [DataMember]
        public int ID { get; private set; }
    }
}
