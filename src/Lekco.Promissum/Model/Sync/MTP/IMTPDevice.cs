using MediaDevices;

namespace Lekco.Promissum.Model.Sync.MTP
{
    /// <summary>
    /// Interface for classes that using <see cref="MediaDevice"/>.
    /// </summary>
    public interface IMTPDevice
    {
        /// <summary>
        /// Parent device of the directory.
        /// </summary>
        public MediaDevice Device { get; }
    }
}
