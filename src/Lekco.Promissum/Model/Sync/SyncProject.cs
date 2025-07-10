using Lekco.Promissum.Model.Engine;
using Lekco.Wpf.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// The project specified for syncing files, composed of multiple tasks.
    /// </summary>
    [DataContract]
    [KnownType(nameof(KnownTypes))]
    [DebuggerDisplay("{Name,nq}")]
    public class SyncProject : ProjectBase
    {
        /// <summary>
        /// Specifies whether the project will be loaded automatically when the program starts.
        /// </summary>
        [DataMember]
        public bool IsAutoLoad { get; set; }

        /// <summary>
        /// A collection of sub tasks.
        /// </summary>
        [DataMember]
        public List<SyncTask> Tasks { get; protected set; }

        /// <summary>
        /// The file holding the sync project.
        /// </summary>
        public SyncProjectFile ParentFile { get; set; }

        /// <summary>
        /// The file's name of the project.
        /// </summary>
        public string ParentFileName => ParentFile.FileName;

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="fileName">Filename of the project.</param>
#nullable disable
        public SyncProject(string name, string fileName)
            : base(name, fileName)
        {
            IsAutoLoad = true;
            FileName = fileName;
            Tasks = new List<SyncTask>();
        }
#nullable enable

        /// <summary>
        /// Get all known types for the project's seriliazation and deseriliazation.
        /// </summary>
        /// <returns>All known types.</returns>
        protected static IEnumerable<Type> KnownTypes()
            => new Type[] {
                typeof(Disk.DiskDirectory),
                typeof(Disk.DiskDrive),
                typeof(Disk.DiskFile),
                typeof(Disk.DiskPath),
                typeof(MTP.MTPDirectory),
                typeof(MTP.MTPDrive),
                typeof(MTP.MTPFile),
                typeof(MTP.MTPPath),
            };

        /// <summary>
        /// Add a given task to the project.
        /// </summary>
        /// <param name="task">Given task.</param>
        public void AddTask(SyncTask task)
        {
            Tasks.Add(task);
            SaveWhole();
        }

        /// <summary>
        /// Delete a specified task which belongs to the project.
        /// </summary>
        /// <param name="task">Specified deleting task.</param>
        public bool DeleteTask(SyncTask task)
        {
            if (task.IsBusy)
                throw new TaskIsBusyException($"\"{task.Name}\"正忙，任务删除失败，请稍后再试。", task);

            try
            {
                string fileName = ParentFile.GetWorkFileName($"{task.ID}.db");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch
            {
                return false;
            }

            Tasks.Remove(task);
            SaveWhole();
            return true;
        }

        /// <summary>
        /// Rename the project with a new name.
        /// </summary>
        /// <param name="newName">The new name of the project.</param>
        public void RenameProject(string newName)
        {
            Name = newName;
            SaveWhole();
        }

        /// <summary>
        /// Get an instance data from a file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>An instance specified by the file.</returns>
        public static SyncProject ReadFromFile(string fileName)
        {
            var result = ReadFromFile<SyncProject>(fileName);
            result.FileName = fileName;
            foreach (var task in result.Tasks)
            {
                task.ParentProject = result;
            }
            return result;
        }

        /// <summary>
        /// Save the project instance as xml file.
        /// </summary>
        /// <exception cref="TaskIsBusyException"></exception>
        public override void Save()
        {
            if (Tasks.FirstOrDefault(task => task.IsBusy) is SyncTask task)
                throw new TaskIsBusyException($"\"{task.Name}\"正忙，项目保存失败，请稍后再试。", task);

            DataContractHelper.SerilizeToFile(this, FileName, new Dictionary<string, string>()
            {
                { "e", @"http://schemas.datacontract.org/2004/07/Lekco.Promissum.Model.Engine" },
                { "d", @"http://schemas.datacontract.org/2004/07/Lekco.Promissum.Model.Sync.Disk" },
                { "m", @"http://schemas.datacontract.org/2004/07/Lekco.Promissum.Model.Sync.Base" },
            });
        }

        /// <summary>
        /// Save the whole project as a prms file.
        /// </summary>
        /// <returns><see langword="true"/> if success; otherwise, returns <see langword="false"/>.</returns>
        public bool SaveWhole()
        {
            Save();
            return ParentFile.Save();
        }
    }
}