using Lekco.Promissum.Apps;
using Lekco.Promissum.Utility;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Sync
{
    [DataContract]
    public class SyncProject : INotifyPropertyChanged
    {
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

        [DataMember]
        public ObservableCollection<SyncTask> Tasks { get; protected set; }

        [DataMember]
        public Version Version { get; protected set; }

        public string FileName { get; protected set; }

        public string TempDirectory { get; protected set; }

        protected string _tempFileName;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        public void DeleteTask(SyncTask task)
        {
            SyncEngine.UnloadSpyedTask(task);
            var dsFile = new FileInfo(TempDirectory + '\\' + task.DataSetName);
            if (dsFile.Exists)
            {
                dsFile.Delete();
            }
            Tasks.Remove(task);
        }

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
                task.CanMatchPaths();
            }
            return result;
        }

        public void Save()
        {
            foreach (var task in Tasks)
            {
                if (task.IsExecuting)
                {
                    throw new SubTaskIsRunningException(this, task);
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
    }

    public class SubTaskIsRunningException : Exception
    {
        public SyncProject ParentProject { get; }
        public SyncTask SubTask { get; }

        public SubTaskIsRunningException(SyncProject parentProject, SyncTask subTask)
            : base($"Task \"{subTask.Name}\" of the project \"{parentProject.Name}\" is executing.")
        {
            ParentProject = parentProject;
            SubTask = subTask;
        }
    }
}