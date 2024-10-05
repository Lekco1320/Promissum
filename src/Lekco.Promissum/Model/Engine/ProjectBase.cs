using Lekco.Wpf.Utility.Helper;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Engine
{
    /// <summary>
    /// The base class for describing project in Lekco Promissum.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Name}")]
    public abstract class ProjectBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Name of the project.
        /// </summary>
        [DataMember]
        public string Name { get; protected set; }

        /// <summary>
        /// Actual filename of the project.
        /// </summary>
        public string FileName { get; protected set; }

        /// <summary>
        /// The file's version of the project.
        /// </summary>
        [DataMember]
        public Version Version { get; protected set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="name">Name of the project.</param>
        /// <param name="fileName">File name of the project.</param>
        protected ProjectBase(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
            Version = App.Promissum.Version;
        }

        /// <summary>
        /// Save the project as a file.
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// Read from file and initialize a object of the project.
        /// </summary>
        /// <typeparam name="T">Real type of project.</typeparam>
        /// <param name="fileName">The file's name of project.</param>
        /// <returns>The object of project.</returns>
        public static T ReadFromFile<T>(string fileName)
            where T : ProjectBase
        {
            T ret = DataContractHelper.DeserilizeFromFile<T>(fileName);
            ret.FileName = fileName;
            return ret;
        }
    }
}
