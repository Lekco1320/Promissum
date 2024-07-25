using Lekco.Promissum.Apps;
using Lekco.Promissum.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Lekco.Promissum.Sync
{
    /// <summary>
    /// The task specified for syncing files, the smallest executable unit of Lekco Promissum.
    /// </summary>
    [DataContract]
    public class SyncTask : INotifyPropertyChanged
    {
        /// <summary>
        /// Name of the task.
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
        /// Source directory's path of the task.
        /// </summary>
        [DataMember]
        public SyncPath OriginPath
        {
            get => _originPath;
            protected set
            {
                _originPath = value;
                OnPropertyChanged(nameof(OriginPath));
            }
        }
        private SyncPath _originPath;

        /// <summary>
        /// Destination directory's path of the task.
        /// </summary>
        [DataMember]
        public SyncPath DestinationPath
        {
            get => _destinationPath;
            protected set
            {
                _destinationPath = value;
                OnPropertyChanged(nameof(DestinationPath));
            }
        }
        private SyncPath _destinationPath;

        /// <summary>
        /// Mode to specify how to judge files should be synced.
        /// </summary>
        [DataMember]
        public CompareMode CompareMode
        {
            get => _compareMode;
            set
            {
                _compareMode = value;
                OnPropertyChanged(nameof(CompareMode));
            }
        }
        private CompareMode _compareMode;

        /// <summary>
        /// Last time the task was executed.
        /// </summary>
        [DataMember]
        public DateTime LastExecuteTime
        {
            get => _lastExecuteTime;
            set
            {
                _lastExecuteTime = value;
                OnPropertyChanged(nameof(LastExecuteTime));
            }
        }
        private DateTime _lastExecuteTime;

        /// <summary>
        /// Creation time of the task.
        /// </summary>
        [DataMember]
        public DateTime CreationTime { get; private set; }

        [DataMember]
        public SyncPlan SyncPlan
        {
            get => _syncPlan;
            set
            {
                _syncPlan = value;
                OnPropertyChanged(nameof(SyncPlan));
            }
        }
        private SyncPlan _syncPlan;

        /// <summary>
        /// Behavior specifies how to cope with the files which need deleting.
        /// </summary>
        [DataMember]
        public DeletionBehavior DeletionBehavior
        {
            get => _deletionBehavior;
            set
            {
                _deletionBehavior = value;
                OnPropertyChanged(nameof(DeletionBehavior));
            }
        }
        private DeletionBehavior _deletionBehavior;

        /// <summary>
        /// Behavior specifies rules to exclude designative files.
        /// </summary>
        [DataMember]
        public ExclusionBehavior ExclusionBehavior
        {
            get => _exclusionBehavior;
            set
            {
                _exclusionBehavior = value;
                OnPropertyChanged(nameof(ExclusionBehavior));
            }
        }
        private ExclusionBehavior _exclusionBehavior;

        /// <summary>
        /// Whether the database of the task has <see cref="SyncFileRecord"/>.
        /// </summary>
        [DataMember]
        public bool HasSyncFileRecord { get; protected set; }

        /// <summary>
        /// Whether the database of the task has <see cref="DeletionFileRecord"/>.
        /// </summary>
        [DataMember]
        public bool HasDeletionFileRecord { get; protected set; }

        /// <summary>
        /// Whether the database of the task has <see cref="SyncRecord"/>.
        /// </summary>
        [DataMember]
        public bool HasSyncRecord { get; protected set; }

        /// <summary>
        /// A dynamically connected dataset storaging all kinds of records of the task.
        /// </summary>
        public SyncDataSet? SyncDataSet { get; protected set; }

        /// <summary>
        /// Specifies whether the task is occupied for something.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            protected set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        private bool _isBusy;

        /// <summary>
        /// The file name of the dataset of the task.
        /// </summary>
        public string DataSetName { get => $"{DataSetCode}.dat"; }

        /// <summary>
        /// Unique code for each task's database.
        /// </summary>
        [DataMember]
        protected int DataSetCode;

        /// <summary>
        /// Event discribes a property value of this class changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when a property value changed.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected internal virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Create an instance of this type.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="originPath">Source directory's path of the task.</param>
        /// <param name="destinationPath">Destination directory's path of the task.</param>
        public SyncTask(string name, SyncPath originPath, SyncPath destinationPath)
        {
            _name = name;
            _originPath = originPath;
            _destinationPath = destinationPath;
            _exclusionBehavior = new ExclusionBehavior();
            _deletionBehavior = new DeletionBehavior();
            _syncPlan = new SyncPlan();
            DataSetCode = 0;
            IsBusy = false;
            HasSyncFileRecord = false;
            HasDeletionFileRecord = false;
            HasSyncRecord = false;
            CreationTime = DateTime.Now;
        }

        /// <summary>
        /// Try to match the paths of the task.
        /// </summary>
        /// <returns><see langword="true"/> if all paths match; otherwise, returns <see langword="false"/>.</returns>
        public bool TryMatchPaths()
        {
            return OriginPath.TryMatch() && DestinationPath.TryMatch() &&
                (!DeletionBehavior.ReserveFiles ||
                DeletionBehavior.ReserveFiles && !DeletionBehavior.MoveToDeletionPath ||
                DeletionBehavior.ReserveFiles && DeletionBehavior.MoveToDeletionPath && DeletionBehavior.DeletionPath.TryMatch());
        }

        /// <summary>
        /// Execute a sync task.
        /// </summary>
        /// <param name="parentProject">The parent project of the task.</param>
        /// <param name="trigger">The execution trigger of the task.</param>
        /// <param name="syncController">The controller of the task execution progress.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void Execute(SyncProject parentProject, ExecutionTrigger trigger, SyncController syncController)
        {
            // Lock the instance to ensure this task can only be executed once in the same time.
            lock (this)
            {
                if (IsBusy)
                {
                    return;
                }
                IsBusy = true;
            }

            // Try matching the drives.
            if (!TryMatchPaths())
            {
                // If not, this task cannot be executed.
                IsBusy = false;
                throw new DirectoryNotFoundException();
            }

            // Start the controller.
            syncController.Start();

            // Set the last execute-time.
            LastExecuteTime = DateTime.Now;

            // Initialize necessary fields.
            var data = new SyncData();
            var originDir = new DirectoryInfo(OriginPath.FullPath);
            var destinationDir = new DirectoryInfo(DestinationPath.FullPath);

            // Connect the dataset from file.
            ConnectDataSet(parentProject);

            // Try catching possible exceptions when using IO.
            try
            {
                // Tell the controller the task starts querying files.
                syncController.SetQueryingState();
                Query(originDir, destinationDir, data, syncController);

                // Tell the controller the task starts dealing with unexpected files.
                syncController.SetDealingWithUnexpectedFilesState(data.UnexpectedFiles, !DeletionBehavior.ReserveFiles);
                DealWithUnexpectedFiles(data, syncController);

                // Tell the controller the task starts syncing files.
                syncController.SetSyncingFilesState();
                SyncFiles(data, syncController);

                // Tell the controller the task starts managing deletion files.
                syncController.SetManagingDeletingFilesState();
                ManageDeletingFiles(data, syncController);
            }
            catch
            {
                // End syncing and throw the exception back to SyncEngine to show message.
                SaveAndDisconnectDataSet();
                syncController.Interrupt();
                IsBusy = false;
                throw;
            }

            // Add a sync record into dataset.
            SyncDataSet!.AddSyncRecord(trigger, LastExecuteTime, DateTime.Now, data);

            // Update information of the records, save and disconnect the dataset.
            SaveAndDisconnectDataSet();

            // Tell the controller the task has ended.
            IsBusy = false;
            syncController.Success(data.FailedSyncRecords);
        }

        /// <summary>
        /// Query the disk to find files which need syncing, moving or deleting.
        /// </summary>
        /// <param name="originDir">The directory from the origin path.</param>
        /// <param name="destinationDir">The directory from the destination path.</param>
        /// <param name="syncData">Temporary data to record syncing information.</param>
        /// <param name="syncController">The controller of the task execution progress.</param>
        /// <returns>The count of files in the sub-directories of <paramref name="destinationDir"/>.</returns>
        protected int Query(DirectoryInfo originDir, DirectoryInfo destinationDir, SyncData syncData, SyncController syncController)
        {
            // DeltionPath should be ignored.
            if (DeletionBehavior.MoveToDeletionPath && destinationDir.FullName == DeletionBehavior.DeletionPath.FullPath)
            {
                return 0;
            }

            // Get the files passed exclusions from originDir.
            var originFiles = GetOriginFiles(originDir);

            // If destinationDir doesn't exist,
            if (!destinationDir.Exists)
            {
                // it means that all files in originDir need syncing.
                foreach (var originFile in originFiles)
                {
                    string destinationInfo = Functions.CombinePaths(destinationDir, originFile);
                    syncData.NewFiles.TryAdd(originFile, destinationInfo);
                    syncController.AddFoundSyncFile();
                }
            }

            // If destinationDir exists,
            else
            {
                // it means that files in destinationDir may need updating, moving or deleting.

                // Initialize a HashSet of files from destinationDir to optimize querying performance. With
                // this HashSet, function can quickly find out whether a file of the same name exists in
                // destinationDir. If does, compare it with the one in originDir by indicated mode to
                // determine whether need to sync. The remain files in destinationDir shall be added into a
                // list of unexpected files.
                var destinationFilesSet = new HashSet<FileInfo>(destinationDir.GetFiles(), new WeakFileInfoComparer());

                foreach (var originFile in originFiles)
                {
                    // If there is no file in destinationDir has same name as originFile or the file in
                    // destinationDir has same file name as originFile but is older / smaller / different
                    // than the file in originDir, the originFile needs syncing; otherwise, it doesn't.
                    if (!destinationFilesSet.TryGetValue(originFile, out FileInfo? destinationFile) ||
                        Compare(originFile, destinationFile))
                    {
                        // Note: The Name of originFile and destinationFile is same.
                        string destinationInfo = Functions.CombinePaths(destinationDir, originFile);
                        syncData.NewFiles.TryAdd(originFile, destinationInfo);
                        syncController.AddFoundSyncFile();
                    }
                    else
                    {
                        // Deal with the same files.
                        syncData.SameFiles.Add(originFile);
                        destinationFilesSet.Remove(originFile);
                    }
                }
                // If needs, add remain files in destinationDir into list of unexpected files.
                if (DeletionBehavior.NeedFindUnexpectedOnes)
                {
                    foreach (var destinationFile in destinationFilesSet)
                    {
                        syncData.UnexpectedFiles.Add(destinationFile);
                    }
                }
            }

            // Now we should find out whether the sub directories of current directory whether should be synced.
            // Initialize a HashSet of directories from destinationDir to optimize querying performance. With this
            // HashSet function can quickly find out whether a directory of the same name exists in destinationDir.
            // The directory with same name should be queryed recursively and parallelly.
            var subOriDirectories = originDir.GetDirectories();
            var unexpectedDirSet = destinationDir.Exists ?
                new HashSet<DirectoryInfo>(destinationDir.GetDirectories(), new WeakFileInfoComparer()) :
                new HashSet<DirectoryInfo>(new WeakFileInfoComparer());
            Task<int>[] tasks = new Task<int>[subOriDirectories.Length];
            int task_n = 0;
            foreach (var subOriDir in subOriDirectories)
            {
                string subDirName = Functions.CombinePaths(destinationDir, subOriDir);
                var subDestinationDir = new DirectoryInfo(subDirName);
                unexpectedDirSet.Remove(subDestinationDir);
                tasks[task_n++] = Task.Run(() =>
                    Query(subOriDir, subDestinationDir, syncData, syncController)
                );
            }

            // If need to find unexpected files and directories,
            if (DeletionBehavior.NeedFindUnexpectedOnes)
            {
                // all the unexpected directories in destinationDir and its sub files should be added.
                foreach (var unexpectedDir in unexpectedDirSet)
                {
                    // If deleting files need moving to DeletionPath, check the unexpected directory whether is the DeletionPath.
                    if (!DeletionBehavior.MoveToDeletionPath || unexpectedDir.FullName != DeletionBehavior.DeletionPath.FullPath)
                    {
                        syncData.UnexpectedDirectories.Add(unexpectedDir);
                        var subFiles = Functions.GetAllSubFiles(unexpectedDir);
                        foreach (var subFile in subFiles)
                        {
                            syncData.UnexpectedFiles.Add(subFile);
                        }
                    }
                }
            }

            // Wait all the sub tasks end querying.
            Task.WaitAll(tasks);

            // If destinationDir doesn't exist but it should have sub files to be synced, add it into new diectories.
            int addSubDirFiles = originFiles.Count + tasks.Sum(task => task.Result);
            if (!destinationDir.Exists && addSubDirFiles > 0)
            {
                syncData.NewDirectories.Add(destinationDir);
            }

            // Return amount of sub files of oriDir which should be synced.
            return addSubDirFiles;
        }

        /// <summary>
        /// Judge files whether should be synced.
        /// </summary>
        /// <param name="originFile">The file in source directory.</param>
        /// <param name="destinationFile">The file in destination directory.</param>
        /// <returns><see langword="true"/> when the file need synced; otherwise, returns <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        protected bool Compare(FileInfo originFile, FileInfo destinationFile)
        {
            if (CompareMode == CompareMode.ByTime)
            {
                return originFile.LastWriteTime > destinationFile.LastWriteTime;
            }
            if (CompareMode == CompareMode.BySize)
            {
                return originFile.Length > destinationFile.Length;
            }
            if (CompareMode == CompareMode.ByMD5)
            {
                var oTask = Task.Run(() => Functions.GetMD5(originFile.FullName));
                var dTask = Task.Run(() => Functions.GetMD5(destinationFile.FullName));
                Task.WaitAll(oTask, dTask);
                return oTask.Result != dTask.Result;
            }
            throw new ArgumentException("Unknown compare mode.");
        }

        /// <summary>
        /// Get the sub filtered files of specified source directory.
        /// </summary>
        /// <param name="originDir">Source directory.</param>
        /// <returns>A collection of filtered files.</returns>
        protected ICollection<FileInfo> GetOriginFiles(DirectoryInfo originDir)
        {
            var comparer = new WeakFileInfoComparer();
            var oriFiles = new HashSet<FileInfo>(originDir.GetFiles(), comparer);
            if (ExclusionBehavior.UseExclusion)
            {
                foreach (var rest in ExclusionBehavior.Exclusions)
                {
                    oriFiles.ExceptWith(rest.MatchedFiles(originDir));
                }
            }
            return oriFiles;
        }

        /// <summary>
        /// Deal with unexpected files.
        /// </summary>
        /// <param name="syncData">Syncing data.</param>
        /// <param name="syncController">Controller of syncing progress.</param>
        protected void DealWithUnexpectedFiles(SyncData syncData, SyncController syncController)
        {
            // If cannot deal with unexpected files, returns.
            if (!syncController.CanDealWithUnexpectedFiles)
            {
                return;
            }

            // Deal with all the unexpected files by DeletionBehavior.
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxParallelCopyCounts };
            Parallel.ForEach(syncData.UnexpectedFiles, option, file =>
            {
                // Add a DeletionRecord into dataset.
                if (SyncDataSet!.AddDeletionRecord(file, DeletionBehavior, out string? newFileName))
                // If uses DeletionPath,
                {
                    // try to move the file to DeletionPath.
                    if (SyncHelper.MoveTo(file, newFileName, true, out FailedSyncRecord? record))
                    {
                        syncData.MovedFiles.Add(file);
                        syncController.AddDealedUnexpectedFile();
                    }
                    else
                    {
                        syncData.FailedSyncRecords.Add(record);
                    }
                }
                // If desen't use DeletionPath,
                else
                {
                    // try to delete it directly.
                    if (SyncHelper.Delete(file, out FailedSyncRecord? record))
                    {
                        syncData.DeletedFiles.Add(file);
                        syncController.AddDealedUnexpectedFile();
                    }
                    else
                    {
                        syncData.FailedSyncRecords.Add(record);
                    }
                }
            });

            // Try to delete all the unexpected directories eventually.
            foreach (var dir in syncData.UnexpectedDirectories)
            {
                if (SyncHelper.Delete(dir, out FailedSyncRecord? record))
                {
                    syncData.DeletedDirectories.Add(dir);
                }
                else
                {
                    syncData.FailedSyncRecords.Add(record);
                }
            }
        }

        /// <summary>
        /// Sync Files.
        /// </summary>
        /// <param name="syncData">Syncing data.</param>
        /// <param name="syncController">Controller of syncing progress.</param>
        protected void SyncFiles(SyncData syncData, SyncController syncController)
        {
            // First, create new directories.
            foreach (var dir in syncData.NewDirectories)
            {
                if (!SyncHelper.Create(dir, out FailedSyncRecord? record))
                {
                    syncData.FailedSyncRecords.Add(record);
                }
            }

            // Then try to sync the files.
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxParallelCopyCounts };
            Parallel.ForEach(syncData.NewFiles, option, pair =>
            {
                if (SyncHelper.CopyTo(pair.Key, pair.Value, out FileInfo? newFile, out FailedSyncRecord? record))
                {
                    SyncDataSet!.UpdateOrAddSyncFileRecord(newFile);
                    syncController.AddCopiedSyncFile();
                }
                else
                {
                    syncData.FailedSyncRecords.Add(record);
                }
            });

            // At last, check records of files which don't need syncing whether exist in the data set.
            Parallel.ForEach(syncData.SameFiles, file =>
            {
                SyncDataSet!.CheckSyncFileRecordExists(file);
            });
        }

        /// <summary>
        /// Manage deleting files.
        /// </summary>
        /// <param name="syncData">Syncing data.</param>
        /// <param name="syncController">Controller of syncing progress.</param>
        protected void ManageDeletingFiles(SyncData syncData, SyncController syncController)
        {
            // If doesn't need to manage, returns.
            if (!DeletionBehavior.ReserveFiles || !DeletionBehavior.MoveToDeletionPath)
            {
                return;
            }

            // Find all deleting files.
            Parallel.ForEach(SyncDataSet!.DeletionFileDictionary, pair =>
            {
                var deletedFileName = pair.Value.Values.First().FileName;
                if (DeletionBehavior.SetMaxVersion)
                {
                    foreach (var file in SyncDataSet.UpdateDeletionRecordsByVersion(deletedFileName, DeletionBehavior))
                    {
                        syncData.DeletingFileRecords.Add(file);
                    }
                }
                if (DeletionBehavior.UseReserveTerm)
                {
                    foreach (var file in SyncDataSet.UpdateDeletionRecordsByTime(deletedFileName, DeletionBehavior, LastExecuteTime))
                    {
                        syncData.DeletingFileRecords.Add(file);
                    }
                }
            });

            // If can manage deleting files,
            if (syncController.CanManageDeletingFileRecords(syncData.DeletingFileRecords))
            {
                // delete files parallelly.
                var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxParallelCopyCounts };
                Parallel.ForEach(syncData.DeletingFileRecords, option, deletionRecord =>
                {
                    var file = new FileInfo(deletionRecord.NewFileName!);
                    if (SyncHelper.Delete(file, out FailedSyncRecord? failedRecord))
                    {
                        syncData.DeletedFiles.Add(file);
                        deletionRecord.IsDeleted = true;
                        syncController.AddManagedDeletionFile();
                    }
                    else
                    {
                        syncData.FailedSyncRecords.Add(failedRecord);
                    }
                });
            }
        }

        /// <summary>
        /// Connect the dataset.
        /// </summary>
        /// <param name="parentProject">Parent project of the task.</param>
        /// <exception cref="FileNotFoundException"></exception>
        protected void ConnectDataSet(SyncProject parentProject)
        {
            if (!parentProject.Tasks.Contains(this))
            {
                throw new FileNotFoundException();
            }
            string datasetPath = parentProject.TempDirectory + '\\' + DataSetName;
            if (DataSetCode == 0)
            {
                DataSetCode = DateTime.Now.GetHashCode();
                SyncDataSet = new SyncDataSet(datasetPath);
            }
            else
            {
                SyncDataSet = File.Exists(datasetPath) ? SyncDataSet.ReadFromFile(datasetPath) : new SyncDataSet(datasetPath);
            }
        }

        /// <summary>
        /// Get the dataset and do something.
        /// </summary>
        /// <param name="project">Parent project of the task.</param>
        /// <param name="action">Action to do with the dataset.</param>
        public void GetDataSet(SyncProject project, Action<SyncDataSet> action)
        {
            try
            {
                ConnectDataSet(project);
            }
            catch
            {
                throw;
            }
            action(SyncDataSet!);
            SaveAndDisconnectDataSet();
        }

        /// <summary>
        /// Save and disconnect the dataset.
        /// </summary>
        protected void SaveAndDisconnectDataSet()
        {
            HasSyncRecord = SyncDataSet!.SyncRecords.Count > 0;
            HasSyncFileRecord = !SyncDataSet!.SyncFileDictionary.IsEmpty;
            HasDeletionFileRecord = !SyncDataSet!.DeletionFileDictionary.IsEmpty;
            SyncDataSet!.Save();
            SyncDataSet = null;
        }

        /// <summary>
        /// Set the OriginPath a new value.
        /// </summary>
        /// <param name="parentProject">Parent project of the task.</param>
        /// <param name="newPath">New value.</param>
        public void SetOriginPath(SyncProject parentProject, SyncPath newPath)
        {
            // If new value is not same as the old one,
            if (OriginPath != newPath)
            {
                // clear all records in the dataset.
                GetDataSet(parentProject, dataset => dataset.ClearAllRecords());
                HasSyncFileRecord = false;
                HasDeletionFileRecord = false;
                HasSyncRecord = false;
                OriginPath = newPath;
            }
        }

        /// <summary>
        /// Set the DestinationPath a new value.
        /// </summary>
        /// <param name="parentProject">Parent project of the task.</param>
        /// <param name="newPath">New value.</param>
        public void SetDestinationPath(SyncProject parentProject, SyncPath newPath)
        {
            // If new value is not same as the old one,
            if (DestinationPath != newPath)
            {
                // clear all records in the dataset.
                GetDataSet(parentProject, dataset => dataset.ClearAllRecords());
                HasSyncFileRecord = false;
                HasDeletionFileRecord = false;
                HasSyncRecord = false;
                DestinationPath = newPath;
            }
        }

        /// <summary>
        /// Set the DeletionPath a new value.
        /// </summary>
        /// <param name="parentProject">Parent project of the task.</param>
        /// <param name="newPath">New value.</param>
        public void SetDeletionPath(SyncProject parentProject, SyncPath newPath)
        {
            // If new value is not same as the old one,
            if (DeletionBehavior.DeletionPath != newPath)
            {
                // clear all DeletionRecord in the dataset.
                GetDataSet(parentProject, dataset => dataset.ClearDeletionRecords());
                HasDeletionFileRecord = false;
                DeletionBehavior.DeletionPath = newPath;
            }
        }

        /// <summary>
        /// Occupy this task and do something may spend long time.
        /// </summary>
        /// <param name="action">Action to do with the task.</param>
        public void BusyAction(Action<SyncTask> action)
        {
            lock (this)
            {
                if (IsBusy)
                {
                    return;
                }
                IsBusy = true;
            }
            action(this);
            IsBusy = false;
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
    /// Mode to specify how to judge files should be synced.
    /// </summary>
    public enum CompareMode
    {
        /// <summary>
        /// Sync newer files.
        /// </summary>
        ByTime,

        /// <summary>
        /// Sync larger files.
        /// </summary>
        BySize,

        /// <summary>
        /// Sync MD5-different files.
        /// </summary>
        ByMD5,
    }
}