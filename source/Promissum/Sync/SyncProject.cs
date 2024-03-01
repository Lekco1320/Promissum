using Lekco.Promissum.Apps;
using Lekco.Promissum.Utility;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    /// <summary>
    /// The project specified for syncing files, composed of multiple tasks.
    /// </summary>
    [DataContract]
    public class SyncProject : INotifyPropertyChanged
    {
        /// <summary>
        /// Name of the project.
        /// </summary>
        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string _name;

        /// <summary>
        /// Specifies whether the project will run automatically.
        /// </summary>
        [DataMember]
        public bool AutoRun
        {
            get => _autoRun;
            set
            {
                _autoRun = value;
                OnPropertyChanged(nameof(AutoRun));
            }
        }
        private bool _autoRun;

        /// <summary>
        /// A collection of sub tasks.
        /// </summary>
        [DataMember]
        public ObservableCollection<SyncTask> Tasks { get; protected set; }

        /// <summary>
        /// Version of current application.
        /// </summary>
        [DataMember]
        public Version Version { get; protected set; }

        /// <summary>
        /// Filename of the project.
        /// </summary>
        public string FileName { get; protected set; }

        /// <summary>
        /// Temporary working directory of the task.
        /// </summary>
        public string TempDirectory { get; protected set; }

        /// <summary>
        /// Temporary working filename of the task.
        /// </summary>
        protected string _tempFileName;

        /// <summary>
        /// Event discribes a property value of this class changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when a property value changed.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="fileName">Filename of the project.</param>
        public SyncProject(string name, string fileName)
        {
            _name = name;
            _autoRun = true;
            Version = App.Version;
            Tasks = new ObservableCollection<SyncTask>();
            FileName = fileName;
            _tempFileName = string.Empty;
            TempDirectory = string.Empty;
        }

        /// <summary>
        /// Delete a specified task which belongs to the project.
        /// </summary>
        /// <param name="task">Specified deleting task.</param>
        public void DeleteTask(SyncTask task)
        {
            SyncEngine.UnloadPlannedTask(task);
            var dsFile = new FileInfo(TempDirectory + '\\' + task.DataSetName);
            if (dsFile.Exists)
            {
                dsFile.Delete();
            }
            Tasks.Remove(task);
        }

        /// <summary>
        /// Get an instance data from a file.
        /// </summary>
        /// <param name="oriFileName">The name of the file.</param>
        /// <returns>An instance specified by the file.</returns>
        public static SyncProject ReadFromFile(string oriFileName)
        {
            string tempDirectory = App.TempDir + $"\\{DateTime.Now.GetHashCode()}";
            ZipFile.ExtractToDirectory(oriFileName, tempDirectory);
            string tempFileName = tempDirectory + @"\project.xml";
            var result = Functions.ReadFromFile<SyncProject>(tempFileName);
            result.FileName = oriFileName;
            result._tempFileName = tempFileName;
            result.TempDirectory = tempDirectory;
            foreach (var task in result.Tasks)
            {
                task.TryMatchPaths();
            }
            return result;
        }

        /// <summary>
        /// Save the project as a file.
        /// </summary>
        /// <exception cref="SubTaskIsBusyException"></exception>
        public void Save()
        {
            foreach (var task in Tasks)
            {
                if (task.IsBusy)
                {
                    throw new SubTaskIsBusyException(this, task);
                }
            }
            Functions.SaveAsFile(this, _tempFileName);
            string tempZipName = TempDirectory + ".prms";
            try
            {
                if (File.Exists(tempZipName))
                {
                    File.Delete(tempZipName);
                }
                ZipFile.CreateFromDirectory(TempDirectory, tempZipName);
                File.Move(tempZipName, FileName, true);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Create a file for <see cref="SyncProject"/>.
        /// </summary>
        public void Create()
        {
            TempDirectory = App.TempDir + $"\\{DateTime.Now.GetHashCode()}";
            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }
            _tempFileName = TempDirectory + @"\project.xml";
            Save();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// The exception that is thrown when doing something with a project whose sub task is busy.
    /// </summary>
    public class SubTaskIsBusyException : Exception
    {
        /// <summary>
        /// The parent project of the task.
        /// </summary>
        public SyncProject ParentProject { get; }

        /// <summary>
        /// The sub task of the project.
        /// </summary>
        public SyncTask SubTask { get; }

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        /// <param name="parentProject">The parent project of the task.</param>
        /// <param name="subTask">The sub task of the project.</param>
        public SubTaskIsBusyException(SyncProject parentProject, SyncTask subTask)
            : base($"Task \"{subTask.Name}\" of the project \"{parentProject.Name}\" is executing.")
        {
            ParentProject = parentProject;
            SubTask = subTask;
        }
    }
}