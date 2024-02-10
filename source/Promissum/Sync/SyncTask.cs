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
    [DataContract]
    public class SyncTask : INotifyPropertyChanged
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

        [DataMember]
        public int DataSetCode { get; protected set; }

        [DataMember]
        public bool HasSyncFileRecord { get; protected set; }

        [DataMember]
        public bool HasDeletionFileRecord { get; protected set; }

        [DataMember]
        public bool HasSyncRecord { get; protected set; }

        public SyncDataSet? SyncDataSet { get; protected set; }

        public Version Version { get; protected set; }

        public bool IsExecuting
        {
            get => _isExecuting;
            protected set
            {
                _isExecuting = value;
                OnPropertyChanged(nameof(IsExecuting));
            }
        }
        private bool _isExecuting;

        public string DataSetName { get => $"{DataSetCode}.dat"; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected internal virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public SyncTask(string name, SyncPath originPath, SyncPath destinationPath)
        {
            _name = name;
            _originPath = originPath;
            _destinationPath = destinationPath;
            _exclusionBehavior = new ExclusionBehavior();
            _deletionBehavior = new DeletionBehavior();
            _syncPlan = new SyncPlan();
            DataSetCode = 0;
            IsExecuting = false;
            HasSyncFileRecord = false;
            HasDeletionFileRecord = false;
            HasSyncRecord = false;
            CreationTime = DateTime.Now;
            Version = (Version)App.Version.Clone();
        }

        public bool CanMatchPaths()
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
        /// <exception cref="InvalidOperationException"></exception>
        public void Execute(SyncProject parentProject, ExecutionTrigger trigger, SyncController syncController)
        {
            // Lock the instance to ensure this task can only be executed once in the same time.
            lock (this)
            {
                if (IsExecuting)
                {
                    return;
                }
                IsExecuting = true;
            }

            // Try matching the drives.
            if (!CanMatchPaths())
            {
                // If not, this task cannot be executed.
                IsExecuting = false;
                throw new DirectoryNotFoundException();
            }

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
                syncController.SetCopyingState();
                CopyFilesAndUpdateSyncFileRecords(data, syncController);

                // Tell the controller the task starts managing deletion files.
                syncController.SetManagingDeletingFilesState();
                ManageDeletingFiles(LastExecuteTime, data, syncController);
            }
            catch
            {
                // End syncing and throw the exception back to SyncEngine to show message.
                SaveAndDisconnectDataSet();
                IsExecuting = false;
                throw;
            }

            // Add a sync record into dataset.
            SyncDataSet!.AddSyncRecord(trigger, LastExecuteTime, DateTime.Now, data);

            HasSyncRecord = SyncDataSet.SyncRecords.Count > 0;
            HasSyncFileRecord = !SyncDataSet.SyncFileDictionary.IsEmpty;
            HasDeletionFileRecord = !SyncDataSet.DeletionFileDictionary.IsEmpty;
            SaveAndDisconnectDataSet();
            IsExecuting = false;

            // Tell the controller the task has ended.
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
            if (DeletionBehavior.NeedFindUnexpectedOnes)
            {
                foreach (var unexpectedDir in unexpectedDirSet)
                {
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
            Task.WaitAll(tasks);
            int addSubDirFiles = originFiles.Count + tasks.Sum(task => task.Result);
            if (!destinationDir.Exists && addSubDirFiles > 0)
            {
                syncData.NewDirectories.Add(destinationDir);
            }
            return addSubDirFiles;
        }

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

        protected void DealWithUnexpectedFiles(SyncData syncData, SyncController syncController)
        {
            if (!syncController.CanDealWithUnexpectedFiles)
            {
                return;
            }

            string? deletionFolder = DeletionBehavior.ReserveFiles ? DeletionBehavior.DeletionPath.FullPath : null;
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxParallelCopyCounts };
            Parallel.ForEach(syncData.UnexpectedFiles, option, file =>
            {
                if (SyncDataSet!.AddDeletionRecord(file, deletionFolder, DeletionBehavior.MarkVersion, out string? newFileName))
                {
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
                else
                {
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

        protected void CopyFilesAndUpdateSyncFileRecords(SyncData syncData, SyncController syncController)
        {
            if (SyncDataSet == null)
            {
                throw new InvalidOperationException(nameof(SyncDataSet));
            }
            foreach (var dir in syncData.NewDirectories)
            {
                if (!SyncHelper.Create(dir, out FailedSyncRecord? record))
                {
                    syncData.FailedSyncRecords.Add(record);
                }
            }
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.MaxParallelCopyCounts };
            Parallel.ForEach(syncData.NewFiles, option, pair =>
            {
                if (SyncHelper.CopyTo(pair.Key, pair.Value, out FileInfo? newFile, out FailedSyncRecord? record))
                {
                    SyncDataSet.UpdateOrAddSyncFileRecord(newFile);
                    syncController.AddCopiedSyncFile();
                }
                else
                {
                    syncData.FailedSyncRecords.Add(record);
                }
            });
        }

        protected void ManageDeletingFiles(DateTime startTime, SyncData syncData, SyncController syncController)
        {
            if (!DeletionBehavior.ReserveFiles || !DeletionBehavior.MoveToDeletionPath)
            {
                return;
            }

            Parallel.ForEach(SyncDataSet!.DeletionFileDictionary, pair =>
            {
                var deletedFileName = pair.Value.Values.First().FileName;
                if (DeletionBehavior.SetMaxVersion)
                {
                    foreach (var file in SyncDataSet.UpdateDeletionRecordsByVersion(deletedFileName, DeletionBehavior.MaxVersion))
                    {
                        syncData.DeletingFiles.Add(file);
                    }
                }
                if (DeletionBehavior.UseReserveTerm)
                {
                    foreach (var file in SyncDataSet.UpdateDeletionRecordsByTime(deletedFileName, DeletionBehavior.ReserveTerm, startTime))
                    {
                        syncData.DeletingFiles.Add(file);
                    }
                }
            });

            if (syncController.CanManageDeletingFiles(syncData.DeletingFiles))
            {
                Parallel.ForEach(syncData.DeletingFiles, file =>
                {
                    if (SyncHelper.Delete(file, out FailedSyncRecord? record))
                    {
                        syncData.DeletedFiles.Add(file);
                        syncController.AddManagedDeletionFile();
                    }
                    else
                    {
                        syncData.FailedSyncRecords.Add(record);
                    }
                });
            }
        }

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

        protected void SaveAndDisconnectDataSet()
        {
            HasSyncFileRecord = !SyncDataSet!.SyncFileDictionary.IsEmpty;
            HasDeletionFileRecord = !SyncDataSet!.DeletionFileDictionary.IsEmpty;
            SyncDataSet!.Save();
            SyncDataSet = null;
        }

        public void SetOriginPath(SyncProject parentProject, SyncPath newPath)
        {
            if (OriginPath != newPath)
            {
                GetDataSet(parentProject, dataset => dataset.ClearAllRecords());
                HasSyncFileRecord = false;
                HasDeletionFileRecord = false;
                HasSyncRecord = false;
                OriginPath = newPath;
            }
        }

        public void SetDestinationPath(SyncProject parentProject, SyncPath newPath)
        {
            if (DestinationPath != newPath)
            {
                GetDataSet(parentProject, dataset => dataset.ClearAllRecords());
                HasSyncFileRecord = false;
                HasDeletionFileRecord = false;
                HasSyncRecord = false;
                DestinationPath = newPath;
            }
        }

        public void SetDeletionPath(SyncProject parentProject, SyncPath newPath)
        {
            if (DeletionBehavior.DeletionPath != newPath)
            {
                GetDataSet(parentProject, dataset => dataset.ClearDeletionRecords());
                HasDeletionFileRecord = false;
                DeletionBehavior.DeletionPath = newPath;
            }
        }

        public void BusyAction(Action<SyncTask> action)
        {
            lock (this)
            {
                if (IsExecuting)
                {
                    return;
                }
                IsExecuting = true;
            }
            action(this);
            IsExecuting = false;
        }
    }

    public enum CompareMode
    {
        ByTime,
        BySize,
        ByMD5,
    }
}