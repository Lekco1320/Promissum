using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.App
{
    /// <summary>
    /// The class storing information of file accessed by Lekco Promissum.
    /// </summary>
    [DataContract]
    public class AccessedFile : INotifyPropertyChanged
    {
        /// <summary>
        /// Full name of this file.
        /// </summary>
        [DataMember]
        public string FullName { get; protected set; }

        /// <summary>
        /// Name of this file.
        /// </summary>
        public string Name => Path.GetFileName(FullName);

        /// <summary>
        /// Extension of this file.
        /// </summary>
        public string Extension => Path.GetExtension(FullName);

        /// <summary>
        /// Indicate whether this file exists.
        /// </summary>
        public bool Exist => File.Exists(FullName);

        /// <summary>
        /// Last time accessed this file.
        /// </summary>
        [DataMember]
        public DateTime LastAccessTime { get; set; }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="fullName">Full name of this file.</param>
        public AccessedFile(string fullName)
        {
            FullName = fullName;
            LastAccessTime = DateTime.Now;
        }
    }
}
