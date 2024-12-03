using Lekco.Promissum.App;
using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Record;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// The task for sync and clean up files and directories.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Name}")]
    public class SyncTask : TaskBase
    {
        /// <summary>
        /// Source path of the task.
        /// </summary>
        [DataMember]
        public PathBase Source { get; protected set; }

        /// <summary>
        /// Destination path of the task.
        /// </summary>
        [DataMember]
        public PathBase Destination { get; protected set; }

        /// <summary>
        /// Mode indicates how to sync files.
        /// </summary>
        [DataMember]
        public FileSyncMode FileSyncMode { get; set; }

        /// <summary>
        /// Indicates whether the schedule is enabled.
        /// </summary>
        [DataMember]
        public bool EnableSchedule { get; set; }

        /// <summary>
        /// Execution schedule of the task.
        /// </summary>
        [DataMember]
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Indicates whether the exclusion rules are enabled.
        /// </summary>
        [DataMember]
        public bool EnableExclusionRules { get; set; }

        /// <summary>
        /// Exclusion rules of the task.
        /// </summary>
        [DataMember]
        public List<ExclusionRule> ExclusionRules { get; set; }

        /// <summary>
        /// Indicates whether the clean up behavior is enabled.
        /// </summary>
        [DataMember]
        public bool EnableCleanUpBehavior { get; set; }

        /// <summary>
        /// The behavior configuration of the task.
        /// </summary>
        [DataMember]
        public CleanUpBehavior CleanUpBehavior { get; set; }

        /// <summary>
        /// The parent project of the task.
        /// </summary>
        public SyncProject ParentProject { get; set; }

        /// <summary>
        /// Indicates whether is due on interval.
        /// </summary>
        public bool IsOnIntervalDue => EnableSchedule && Schedule.IsOnIntervalDue(LastExecuteTime);

        /// <summary>
        /// Indicates whether is due on days.
        /// </summary>
        public bool IsOnDaysDue => EnableSchedule && Schedule.IsOnDaysDue(LastExecuteTime);

        /// <summary>
        /// The file name of the database.
        /// </summary>
        public string DataBaseFileName => ParentProject.SyncProjectFile.GetWorkFileName($"{ID}.db");

        /// <inheritdoc />
        public override bool IsReady => Source.IsReady && Destination.IsReady &&
                                        (!EnableCleanUpBehavior ||
                                          EnableCleanUpBehavior && !CleanUpBehavior.EnableReserve ||
                                          EnableCleanUpBehavior && CleanUpBehavior.EnableReserve && CleanUpBehavior.ReservedPath!.IsReady);

        /// <summary>
        /// Indicates whether the corresponding database exists.
        /// Value will be updated only when getting context by external code.
        /// </summary>
        protected bool dataBaseExists;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="name">Name of the task.</param>
        /// <param name="source">Source path of the task.</param>
        /// <param name="destination">Destination path of the task.</param>
        public SyncTask(string name, PathBase source, PathBase destination, SyncProject parentProject)
            : base(name)
        {
            Source = source;
            Destination = destination;
            FileSyncMode = FileSyncMode.SyncNewer;
            Schedule = new Schedule();
            ExclusionRules = new List<ExclusionRule>();
            CleanUpBehavior = new CleanUpBehavior();
            ParentProject = parentProject;
        }

        /// <summary>
        /// Check whether the database file exists.
        /// </summary>
        protected void CheckDataBaseExists()
            => dataBaseExists = File.Exists(DataBaseFileName);

        /// <summary>
        /// Get a database context of the task.
        /// </summary>
        /// <param name="pooling">Indicates whether using connection pool.</param>
        /// <returns>Database context of the task.</returns>
        protected SyncDbContext GetDbContext(bool pooling = true)
            => SyncDbContext.GetDbContext(DataBaseFileName, false, pooling);

        /// <summary>
        /// Get a database context of the task.
        /// </summary>
        /// <param name="pooling">Indicates whether using connection pool.</param>
        /// <returns>Database context of the task.</returns>
        public SyncDbContext GetDbContextReadonly(bool pooling = true)
        {
            CheckDataBaseExists();
            if (!dataBaseExists)
                throw new FileNotFoundException($"任务\"{Name}\"的数据库文件不存在。");

            return SyncDbContext.GetDbContext(DataBaseFileName, true, pooling);
        }

        /// <summary>
        /// Executes the task to sync files.
        /// </summary>
        /// <param name="trigger">The trigger of current execution.</param>
        /// <returns>Record of this execution.</returns>
        /// <exception cref="TaskNotReadyException"></exception>
        public ExecutionRecord Execute(ExecutionTrigger trigger)
        {
            if (!IsReady)
                throw new TaskNotReadyException($"任务\"{Name}\"执行失败：未连接设备或尚未就绪。", this);

            if (IsBusy)
                throw new TaskNotReadyException($"任务\"{Name}\"执行失败：正在执行。", this);

            if (IsSuspended)
                throw new TaskSuspendedException($"任务\"{Name}\"执行失败：任务已挂起。", this);

            if (!Source.Exists)
                throw new DirectoryNotFoundException($"任务\"{Name}\"的同步源路径\"{Source.FullPath}\"不存在。");

            if (!Destination.Exists)
                throw new DirectoryNotFoundException($"任务\"{Name}\"的同步至路径\"{Destination.FullPath}\"不存在。");

            var ret = BusyAction(() => ExecuteProtected(trigger));
            ParentProject.Save();

            return ret;
        }

        /// <summary>
        /// Executes the task to sync files. The function is protected.
        /// </summary>
        /// <param name="trigger">The trigger of current execution.</param>
        /// <returns>Record of this execution.</returns>
        protected ExecutionRecord ExecuteProtected(ExecutionTrigger trigger)
        {
            var data = new ExecutionData();
            data.ExecutionBegin();
            LastExecuteTime = DateTime.Now;

            var dbContext = GetDbContext();
            dbContext.EnsureCreated();

            ExecutionRecord executionRecord;
            try
            {
                QueryNeedSyncFiles(Source.Directory, data);
                if (EnableCleanUpBehavior)
                {
                    CleanUpDestinationPath(data);
                    QueryNeedCleanUpReservedFiles(data);
                    if (data.CanCleanUpReservedPath(CleanUpBehavior.ReservedPath!))
                    {
                        CleanUpReservedFiles(data);
                    }
                }
                SyncFiles(data);
            }
            finally
            {
                data.StartTime = LastExecuteTime;
                data.EndTime = DateTime.Now;
                data.ExecutionTrigger = trigger;

                executionRecord = new ExecutionRecord(data);
                dbContext.ExecutionRecords.Add(executionRecord);
                dbContext.Dispose();
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                data.ExecutionEnd();
            }

            return executionRecord;
        }

        /// <summary>
        /// Generate a same absolute path of given file relative to destination path.
        /// </summary>
        /// <param name="entity">A corresponding file in destination path.</param>
        /// <returns>An absolute path of given file relative to destination path.</returns>
        /// <exception cref="ArgumentException" />
        protected FileBase GenerateFilePathOfDestination(FileBase entity)
            => Destination.GetFile(Source.GetRelativePath(entity));

        /// <summary>
        /// Generate a same absolute path of given directory relative to destination path.
        /// </summary>
        /// <param name="entity">A corresponding directory in destination path.</param>
        /// <returns>An absolute path of given directory relative to destination path.</returns>
        /// <exception cref="ArgumentException" />
        protected DirectoryBase GenerateDirectoryPathOfDestination(DirectoryBase entity)
            => Destination.GetDirectory(Source.GetRelativePath(entity));

        /// <summary>
        /// Get filtered files in the source directory.
        /// </summary>
        /// <param name="srcDir">The source directory.</param>
        /// <returns>Filtered files in the source directory.</returns>
        protected IEnumerable<FileBase> GetFilesInSourceDirectory(DirectoryBase srcDir)
        {
            var comparer = new FileNameComparer();
            var files = new HashSet<FileBase>(srcDir.EnumerateFiles(), comparer);
            if (EnableExclusionRules)
            {
                foreach (var rule in ExclusionRules)
                {
                    files.ExceptWith(rule.MatchedFiles(srcDir));
                }
            }
            return files;
        }

        /// <summary>
        /// Indicates whether need to sync given file in source path.
        /// </summary>
        /// <param name="srcFile">Given file in source path.</param>
        /// <param name="dstName">Given file in destination path.</param>
        /// <returns><see langword="true"/> if needs; otherwise, returns <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException" />
        protected bool NeedSync(FileBase srcFile, FileBase dstName)
        {
            return FileSyncMode switch
            {
                FileSyncMode.Always => true,
                FileSyncMode.SyncNewer => srcFile.CompareTo(dstName, FileCompareMode.IsNewer),
                FileSyncMode.SyncOlder => srcFile.CompareTo(dstName, FileCompareMode.IsOlder),
                FileSyncMode.SyncLarger => srcFile.CompareTo(dstName, FileCompareMode.IsLarger),
                FileSyncMode.SyncSmaller => srcFile.CompareTo(dstName, FileCompareMode.IsSmaller),
                _ => throw new ArgumentException("Invalid argument.", nameof(srcFile)),
            };
        }

        /// <summary>
        /// Query the source directory to find files and directories which need to sync or clean up.
        /// </summary>
        /// <param name="srcDir">The source directory.</param>
        /// <param name="data">Execution data.</param>
        /// <returns>The count of files need to be synced.</returns>
        protected void QueryNeedSyncFiles(DirectoryBase srcDir, ExecutionData data)
        {
            data.SetState(ExecutionState.Query);

            // Get filtered files in source directory and destination directory.
            var srcFiles = GetFilesInSourceDirectory(srcDir);
            var destDir = GenerateDirectoryPathOfDestination(srcDir);

            // If destination directory doesn't exist,
            if (!destDir.Exists)
            {
                // adds it and its sub files into data.
                data.NeedSyncDirectories.Add(destDir);
                foreach (var srcfile in srcFiles)
                {
                    var destfile = GenerateFilePathOfDestination(srcfile);
                    data.NeedSyncFiles.Add((srcfile, destfile));
                }
                goto FindSub;
            }

            // If destination directory exists, files in destination directory maybe out-dated and need cleaning
            // up. Therefore, here I initialize a HashSet of files from destination directory to optimize
            // querying performance. Compare all files having same name to files in source directory, compare them
            // to judge whether out-dated. The out-dated and redundant files need to be cleaned up.
            var destFilesSet = new HashSet<FileBase>(destDir.EnumerateFiles(), new FileNameComparer());
            foreach (var srcFile in srcFiles)
            {
                if (destFilesSet.TryGetValue(srcFile, out FileBase? destFile) &&
                    !NeedSync(srcFile, destFile))
                {
                    data.DontNeedSyncFiles.Add(srcFile);
                    destFilesSet.Remove(destFile);
                }
                else
                {
                    destFile ??= GenerateFilePathOfDestination(srcFile);
                    data.NeedSyncFiles.Add((srcFile, destFile));
                }
            }

            // If needs, add remain files in destination directory into data.
            if (EnableCleanUpBehavior)
            {
                foreach (var destFile in destFilesSet)
                {
                    data.NeedCleanUpFiles.Add(destFile);
                }
            }

        FindSub:
            // Now we should find out whether the sub directories of current directory whether should be synced.
            // Initialize a HashSet of directories from destinationDir to optimize querying performance. With this
            // HashSet function can quickly find out whether a directory of the same name exists in destinationDir.
            // The directory with same name should be queryed recursively and parallelly.
            var srcSubDirs = srcDir.EnumerateDirectories();
            var needCleanUpDirSet = destDir.Exists ?
                new HashSet<DirectoryBase>(destDir.EnumerateDirectories(), new FileNameComparer()) :
                new HashSet<DirectoryBase>(new FileNameComparer());
            var tasks = new List<Task>();
            foreach (var srcSubDir in srcSubDirs)
            {
                var destSubDir = GenerateDirectoryPathOfDestination(srcSubDir);
                needCleanUpDirSet.Remove(destSubDir);
                Task task = new Task(() => QueryNeedSyncFiles(srcSubDir, data));
				tasks.Add(task);
                task.Start();
            }

            // If need to clean up,
            if (EnableCleanUpBehavior)
            {
                // all the out-dated directories and its sub files should be added into data.
                foreach (var needCleanUpDir in needCleanUpDirSet)
                {
                    data.NeedCleanUpDirectories.Add(needCleanUpDir);
                    var needCleanUpFiles = needCleanUpDir.EnumerateFiles();
                    foreach (var needCleanUpFile in needCleanUpFiles)
                    {
                        data.NeedCleanUpFiles.Add(needCleanUpFile);
                    }
                }
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Get all directories in reserved path and add them into data.
        /// </summary>
        /// <param name="directory">The current directory in reserved path.</param>
        /// <param name="data">The execution data.</param>
        protected static void GetReservedPathDirectories(DirectoryBase directory, ExecutionData data)
        {
            var tasks = new List<Task>();
            foreach (var dir in directory.EnumerateDirectories())
            {
                data.ReservedPathDirectories.Add(dir.FullName);
                tasks.Add(new Task(() => GetReservedPathDirectories(dir, data)));
            }
            Task.WhenAll(tasks).Wait();
        }

        /// <summary>
        /// Check reserved file's parent directory whether exists. If not, create them.
        /// </summary>
        /// <param name="reservedFile">The reserved file.</param>
        /// <param name="data">The execution data.</param>
        /// <returns><see langword="true"/> if checks successfully; otherwise, returns <see langword="false"/>.</returns>
        protected static bool CheckReservedFilePath(FileBase reservedFile, ExecutionData data)
        {
            var dir = reservedFile.Parent;
            if (data.ReservedPathDirectories.Contains(dir.FullName))
            {
                return true;
            }
            if (!dir.TryCreate(out var exRecord))
            {
                data.ExceptionRecords.Add(exRecord);
                return false;
            }
            while (!data.ReservedPathDirectories.Contains(dir.FullName))
            {
                data.ReservedPathDirectories.Add(dir.FullName);
                dir = dir.Parent;
            }
            return true;
        }

        /// <summary>
        /// Clean up destination path's files and directories if needs.
        /// </summary>
        /// <param name="data">Execution data.</param>
        protected void CleanUpDestinationPath(ExecutionData data)
        {
            data.SetState(ExecutionState.CleanUpDestinationPath);
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.Instance.FileOperationMaxParallelCount };
            Parallel.ForEach(data.NeedCleanUpFiles, option, file =>
            {
                using var dbContext = GetDbContext();
                string relativeName = Destination.GetRelativePath(file);
                ExceptionRecord? exRecord;
                var record = dbContext.CleanUpRecords.FirstOrDefault(record => record.RelativeFileName ==  relativeName);
                int currentVersion = record == null || record.ReservedVersions.Count == 0 ?
                                     1 : record.ReservedVersions[^1] + 1;
                if (CleanUpBehavior.EnableReserve)
                {
                    var reservedFile = CleanUpBehavior.EnableVersioning ? GetReservedFile(file, currentVersion) : GetReservedFile(file);
                    if (!CheckReservedFilePath(reservedFile, data))
                    {
                        return;
                    }
                    if (file.TryMoveTo(reservedFile, true, out exRecord))
                    {
                        if (record == null)
                        {
                            record = new CleanUpRecord(file, relativeName);
                            dbContext.CleanUpRecords.Add(record);
                        }
                        else
                        {
                            record.AddVersion(currentVersion);
                            dbContext.Entry(record).Property(record => record.ReservedVersions).IsModified = true;
                        }
                        data.ReservedFiles.Add(file);
                    }
                }
                else if (file.TryDelete(out exRecord))
                {
                    if (record is null)
                    {
                        record = new CleanUpRecord(file, relativeName);
                        dbContext.CleanUpRecords.Add(record);
                    }
                    data.DeletedDestinationFiles.Add(file);
                }
                if (exRecord is not null)
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            });
            Parallel.ForEach(data.NeedCleanUpDirectories, directory =>
            {
                if (!directory.TryDelete(out var exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            });
        }

        /// <summary>
        /// Query reserved files which need to be cleaned up, such as exceed time or version retention.
        /// </summary>
        /// <param name="data">Execution data.</param>
        protected void QueryNeedCleanUpReservedFiles(ExecutionData data)
        {
            data.SetState(ExecutionState.CleanUpReservedPath);
            if (EnableCleanUpBehavior && CleanUpBehavior.EnableReserve && !CleanUpBehavior.ReservedPath!.Directory.Exists
                && !CleanUpBehavior.ReservedPath!.Directory.TryCreate(out var record))
            {
                data.ExceptionRecords.Add(record);
                return;
            }
            if (!CleanUpBehavior.EnableReserve || !CleanUpBehavior.EnableRetentionPeriod && !CleanUpBehavior.EnableMaxVersionRetention)
            {
                return;
            }

            using var DbContext = GetDbContext();
            DateTime dueTime = DateTime.Now - CleanUpBehavior.RetentionPeriod;
            Parallel.ForEach(DbContext.CleanUpRecords, record =>
            {
                using var dbContext = GetDbContext();
                var reservedFiles = GetReservedFile(record);
                int reservedVersionIndex = 0;
                int reservedMinIndex = reservedFiles.Count - CleanUpBehavior.MaxVersion;
                for (int i = 0; i < reservedFiles.Count; i++)
                {
                    var reservedFile = reservedFiles[i];
                    if (!reservedFile.Exists)
                    {
                        record.RemoveVersion(reservedVersionIndex);
                        dbContext.Entry(record).Property(record => record.ReservedVersions).IsModified = true;
                        continue;
                    }
                    if (CleanUpBehavior.EnableVersioning && CleanUpBehavior.EnableMaxVersionRetention && i < reservedMinIndex
                        || CleanUpBehavior.EnableRetentionPeriod && reservedFile.LastWriteTime < dueTime)
                    {
                        data.NeedCleanUpReservedFiles.Add((record, reservedVersionIndex, reservedFile));
                    }
                    ++reservedVersionIndex;
                }
            });
        }

        /// <summary>
        /// Clean up reserved files need cleaning up.
        /// </summary>
        /// <param name="data">Execution data.</param>
        protected void CleanUpReservedFiles(ExecutionData data)
        {
            foreach (var tuple in data.NeedCleanUpReservedFiles)
            {
                var record = tuple.Item1;
                var index = tuple.Item2;
                var file = tuple.Item3;
                if (file.TryDelete(out var exRecord))
                {
                    data.DeletedReservedFiles.Add(file);
                    record.RemoveVersion(index);
                }
                else
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            }

            Parallel.ForEach(CleanUpBehavior.ReservedPath!.Directory.EnumerateDirectories(),
                dir => DeleteEmptyDirectory(dir, data)
            );
        }

        /// <summary>
        /// Sync all files found need syncing.
        /// </summary>
        /// <param name="data">Execution data.</param>
        protected void SyncFiles(ExecutionData data)
        {
            data.SetState(ExecutionState.SyncFiles);
            foreach (var directory in data.NeedSyncDirectories)
            {
                if (!directory.TryCreate(out var exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            }

            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.Instance.FileOperationMaxParallelCount };
            Parallel.ForEach(data.NeedSyncFiles, option, filePair =>
            {
                using var dbContext = GetDbContext();
                var srcFile = filePair.Item1;
                var dstFile = filePair.Item2;

                string relativeName = Source.GetRelativePath(srcFile);
                if (!srcFile.TryCopyTo(dstFile, out var exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                    return;
                }
                var record = dbContext.FileRecords.FirstOrDefault(record => record.RelativeFileName == relativeName);
                if (record == null)
                {
                    record = new FileRecord(srcFile, relativeName);
                    dbContext.FileRecords.Add(record);
                }
                else
                {
                    record.SyncAndUpdate(srcFile);
                }
                data.SyncedFiles.Add(srcFile);
            });

            data.SetState(ExecutionState.Completion);
            using var dbContext = GetDbContext();
            foreach (var file in data.DontNeedSyncFiles)
            {
                string relativeName = Source.GetRelativePath(file);

                var record = dbContext.FileRecords.FirstOrDefault(record => record.RelativeFileName == relativeName);
                if (record == null)
                {
                    record = new FileRecord(file, relativeName);
                    dbContext.FileRecords.Add(record);
                }
                else
                {
                    record.SyncAndUpdate(file);
                }
                data.SyncedFiles.Add(file);
            }
        }

        /// <summary>
        /// Delete the given directory if it is empty.
        /// </summary>
        /// <param name="directory">Given directory.</param>
        /// <param name="data">Execution data.</param>
        protected static void DeleteEmptyDirectory(DirectoryBase directory, ExecutionData data)
        {
            var directories = directory.EnumerateDirectories();
            var files = directory.EnumerateFiles();
            if (!directories.Any() || !files.Any())
            {
                if (!directory.TryDelete(out var exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                }
                else
                {
                    data.DeletedDirectories.Add(directory);
                }
                return;
            }
            Parallel.ForEach(directories, directory => DeleteEmptyDirectory(directory, data));
        }

        /// <summary>
        /// Get corresponding reserved version of the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>Corresponding reserved file.</returns>
        protected FileBase GetReservedFile(FileBase file)
        {
            string directory = Destination.GetRelativePath(file.Parent);
            string relativeName = Path.Combine(directory, file.Name);
            return CleanUpBehavior.ReservedPath!.GetFile(relativeName);
        }

        /// <summary>
        /// Get corresponding reserved version of the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="version">Specified version.</param>
        /// <returns>Corresponding reserved file.</returns>
        protected FileBase GetReservedFile(FileBase file, int version)
        {
            string directory = Destination.GetRelativePath(file.Parent);
            string newName = file.NameWithoutExtension + "_" + version + file.Extension;
            string relativeName = Path.Combine(directory, newName);
            return CleanUpBehavior.ReservedPath!.GetFile(relativeName);
        }

        /// <summary>
        /// Get all reserved files of the record.
        /// </summary>
        /// <param name="cleanUpRecord">The record.</param>
        /// <returns>All reserved files of the record.</returns>
        protected List<FileBase> GetReservedFile(CleanUpRecord cleanUpRecord)
        {
            var ret = new List<FileBase>();
            if (CleanUpBehavior.EnableVersioning)
            {
                string directory = Path.GetDirectoryName(cleanUpRecord.RelativeFileName)!;
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(cleanUpRecord.RelativeFileName);
                string extension = Path.GetExtension(cleanUpRecord.RelativeFileName);
                foreach (var version in cleanUpRecord.ReservedVersions)
                {
                    string reservedName = nameWithoutExtension + "_" + version + extension;
                    string relativeName = Path.Combine(directory, reservedName);
                    var reservedFile = CleanUpBehavior.ReservedPath!.GetFile(relativeName);
                    ret.Add(reservedFile);
                }
            }
            else
            {
                var reservedFile = CleanUpBehavior.ReservedPath!.GetFile(cleanUpRecord.RelativeFileName);
                ret.Add(reservedFile);
            }
            return ret;
        }

        /// <summary>
        /// Delete specified types of dataset.
        /// </summary>
        /// <param name="dataSetType">Type of dataset.</param>
        public void DeleteDataSet(SyncDataSetType dataSetType)
        {
            CheckDataBaseExists();
            if (!dataBaseExists)
                throw new FileNotFoundException($"任务\"{Name}\"的数据库文件不存在。");

            using var dbContext = GetDbContext(pooling: false);

            if (dataSetType.HasFlag(SyncDataSetType.FileRecordDataSet))
            {
                dbContext.FileRecords.RemoveRange(dbContext.FileRecords);
            }
            if (dataSetType.HasFlag(SyncDataSetType.CleanUpDataSet))
            {
                dbContext.CleanUpRecords.RemoveRange(dbContext.CleanUpRecords);
            }
            if (dataSetType.HasFlag(SyncDataSetType.ExecutionDataSet))
            {
                dbContext.ExecutionRecords.RemoveRange(dbContext.ExecutionRecords);
            }

            dbContext.Dispose();
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            ParentProject.SyncProjectFile.Save();
        }
    }

    /// <summary>
    /// Indicates the mode when syncing files.
    /// </summary>
    public enum FileSyncMode
    {
        /// <summary>
        /// Sync always.
        /// </summary>
        [Description("总是同步")]
        Always,

        /// <summary>
        /// Sync the newer ones.
        /// </summary>
        [Description("同步较新的")]
        SyncNewer,

        /// <summary>
        /// Sync the older ones.
        /// </summary>
        [Description("同步较旧的")]
        SyncOlder,

        /// <summary>
        /// Sync the larger ones.
        /// </summary>
        [Description("同步较大的")]
        SyncLarger,

        /// <summary>
        /// Sync the smaller ones.
        /// </summary>
        [Description("同步较小的")]
        SyncSmaller,
    }

    /// <summary>
    /// Indicates the type of dataset of <see cref="SyncTask"/> database.
    /// </summary>
    [Flags]
    public enum SyncDataSetType
    {
        /// <summary>
        /// The type of dataset of <see cref="FileRecord"/>.
        /// </summary>
        FileRecordDataSet = 0x001,

        /// <summary>
        /// The type of dataset of <see cref="CleanUpRecord"/>.
        /// </summary>
        CleanUpDataSet = 0x010,

        /// <summary>
        /// The type of dataset of <see cref="ExecutionRecord"/>.
        /// </summary>
        ExecutionDataSet = 0x100,

        /// <summary>
        /// The type of all types.
        /// </summary>
        AllDataSets = FileRecordDataSet | CleanUpDataSet | ExecutionDataSet
    }
}
