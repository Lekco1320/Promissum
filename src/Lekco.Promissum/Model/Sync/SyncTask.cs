using Lekco.Promissum.App;
using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Execution;
using Lekco.Promissum.Model.Sync.Record;
using Lekco.Wpf.Utility;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    [DebuggerDisplay("{Name,nq}")]
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
        /// Indicates whether the comparison of path is case-sensitive.
        /// </summary>
        [DataMember]
        public bool IsCaseSensitive { get; set; }

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
        public string DataBaseFileName => ParentProject.ParentFile.GetWorkFileName($"{ID}.db");

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
                QueryNeedSyncFiles(data);
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
                dbContext.SaveChanges();
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
        /// Generate the name of a file whose version is given.
        /// </summary>
        /// <param name="fileName">File's name.</param>
        /// <param name="version">file's version.</param>
        /// <returns>The file name annotated with version.</returns>
        protected static string GenerateVersionedFileName(string fileName, int version)
        {
            string directory = Path.GetDirectoryName(fileName) ?? "";
            string newName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string newFileName = $"{newName}_{version}{extension}";
            return Path.Combine(directory, newFileName);
        }

        /// <summary>
        /// Query the source directory to find files and directories which need to sync or clean up.
        /// </summary>
        /// <param name="data">Execution data.</param>
        /// <returns>The count of files need to be synced.</returns>
        protected void QueryNeedSyncFiles(ExecutionData data)
        {
            // Construct directory trees in the source and destination path.
            data.SetState(ExecutionState.ConstructTree);
            var rules = EnableExclusionRules ? ExclusionRules : null;
            var srcDirTree = new DirectoryTree(Source.Directory, rules);
            var dstDirTree = new DirectoryTree(Destination.Directory, rules);
            var srcConTask = new Task(srcDirTree.Construct);
            var dstConTask = new Task(dstDirTree.Construct);
            srcConTask.Start();
            dstConTask.Start();
            Task.WaitAll(srcConTask, dstConTask);
            var result = srcDirTree.CompareTo(dstDirTree, (FileCompareMode)FileSyncMode, IsCaseSensitive);

            // Generate corresponding path of file in destination path.
            data.NeedSyncFiles = result.DifferentFiles;
            data.NeedCleanUpFiles = result.DeletedFiles;
            data.NeedCleanUpDirectories = result.DeletedDirectories;
            data.DontNeedSyncFiles = result.SameFiles;
            foreach (var newFile in result.NewFiles)
            {
                FileBase newSyncFile = GenerateFilePathOfDestination(newFile);
                data.NeedSyncFiles.Add(new Pair<FileBase, FileBase>(newFile, newSyncFile));
            }
            foreach (var newDir in result.NewDirectories)
            {
                DirectoryBase newSyncDir = GenerateDirectoryPathOfDestination(newDir);
                data.NeedSyncDirectories.Add(newSyncDir);
            }
        }

        /// <summary>
        /// Clean up destination path's files and directories if needs.
        /// </summary>
        /// <param name="data">Execution data.</param>
        protected void CleanUpDestinationPath(ExecutionData data)
        {
            // Try to get corresponding `CleanUpRecord` of files need cleaning up.
            data.SetState(ExecutionState.CleanUpDestinationPath);
            var records = new ConcurrentBag<Tuple<FileBase, string, CleanUpRecord?>>();
            var cleanUpFiles = data.NeedCleanUpFiles.ToList();
            cleanUpFiles.AddRange(data.NeedSyncFiles.Select(p => p.Item2));
            Parallel.ForEach(cleanUpFiles, file =>
            {
                string relativeFileName = Destination.GetRelativePath(file);
                using var dbContext = GetDbContext();
                var record = dbContext.CleanUpRecords.AsNoTracking()
                                                     .FirstOrDefault(r => r.RelativeFileName == relativeFileName);
                records.Add(new Tuple<FileBase, string, CleanUpRecord?>(file, relativeFileName, record));
            });

            // Clean up files in destination path.
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.Instance.FileOperationMaxParallelCount };
            Parallel.ForEach(records, option, tuple =>
            {
                var cleanFile = tuple.Item1;
                var relativeName = tuple.Item2;
                var record = tuple.Item3;
                if (!cleanFile.Exists)
                {
                    return;
                }

                ExceptionRecord? exRecord;
                using var dbContext = GetDbContext();

                // If disable reservation,
                if (!CleanUpBehavior.EnableReserve)
                {
                    // delete it directly and finish.
                    if (cleanFile.TryDelete(out exRecord))
                    {
                        var newRecord = new CleanUpRecord(cleanFile, relativeName) { LastOperateTime = DateTime.Now };
                        dbContext.CleanUpRecords.Add(newRecord);
                        dbContext.SaveChanges();
                        data.DeletedDestinationFiles.Add(cleanFile);
                    }
                    else
                    {
                        data.ExceptionRecords.Add(exRecord);
                    }
                    return;
                }

                // If enable reservation, generate path of reserved file.
                FileBase reservedFile;
                var verList = new List<int>();
                string reservedPath;
                if (CleanUpBehavior.EnableVersioning)
                {
                    verList = record?.ReservedVersionList ?? verList;
                    int version = verList.LastOrDefault() + 1;
                    string relativePath = record?.RelativeFileName ?? Destination.GetRelativePath(cleanFile);
                    reservedPath = GenerateVersionedFileName(relativePath, version);
                    reservedFile = CleanUpBehavior.ReservedPath!.GetFile(reservedPath);
                    verList.Add(version);
                }
                else
                {
                    reservedPath = Destination.GetRelativePath(cleanFile);
                    reservedFile = CleanUpBehavior.ReservedPath!.GetFile(reservedPath);
                }

                // Create parent directory of reserved file if doesn't exist.
                var parent = reservedFile.Parent;
                if (!parent.Exists && !parent.TryCreate(out exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                    return;
                }

                // Move files for reservation.
                if (cleanFile.TryMoveTo(reservedFile, true, out exRecord))
                {
                    if (record == null)
                    {
                        record = new CleanUpRecord(cleanFile, relativeName);
                        dbContext.CleanUpRecords.Add(record);
                    }
                    else
                    {
                        dbContext.CleanUpRecords.Update(record);
                    }
                    record.LastOperateTime = DateTime.Now;
                    record.ReservedVersionList = verList;
                    dbContext.SaveChanges();
                    data.ReservedFiles.Add(reservedFile);
                }
                else
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            });

            // Delete empty directories in destination path.
            Parallel.ForEach(data.NeedCleanUpDirectories, option, directory =>
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
            if (CleanUpBehavior.EnableReserve &&
                !CleanUpBehavior.ReservedPath!.Directory.Exists &&
                !CleanUpBehavior.ReservedPath!.Directory.TryCreate(out var record))
            {
                data.ExceptionRecords.Add(record);
                return;
            }
            if (!CleanUpBehavior.EnableReserve ||
                !CleanUpBehavior.EnableRetentionPeriod && !CleanUpBehavior.EnableMaxVersionRetention)
            {
                return;
            }

            // Construct a tree of reserved path and group all reserved files by their versions if needs.
            var reservedTree = new DirectoryTree(CleanUpBehavior.ReservedPath!.Directory, null);
            reservedTree.Construct();
            var reservedFileGroups = reservedTree.QueryFiles().Select(file =>
            {
                int version = 0;
                string noVersionName = CleanUpBehavior.EnableVersioning
                                     ? CleanUpBehavior.ExtractVersion(file.FullName, out version)
                                     : file.FullName;
                return new Tuple<FileBase, string, int>(file, noVersionName, version);
            }).GroupBy(t => t.Item2);

            Parallel.ForEach(reservedFileGroups, group =>
            {
                // Try querying corresponding record from dataset by relative path of the file.
                string relativeFileName = CleanUpBehavior.ReservedPath!.GetRelativePath(group.Key);
                using var dbContext = GetDbContext();
                var record = dbContext.CleanUpRecords.AsNoTracking()
                                                     .FirstOrDefault(r => r.RelativeFileName == relativeFileName);
                if (record == null)
                {
                    return;
                }

                // Judge the file whether needs to be cleaned up.
                int maxVersion = group.Max(t => t.Item3);
                var verList = record.ReservedVersionList;
                var verSet = new HashSet<int>(verList);
                bool modified = false;
                foreach (var tuple in group)
                {
                    int version = tuple.Item3;
                    int versionIndex = maxVersion - version + 1;
                    if (!verSet.Remove(version))
                    {
                        verList.Add(version);
                        modified = true;
                    }
                    if (CleanUpBehavior.NeedCleanUp(tuple.Item1, versionIndex))
                    {
                        data.NeedCleanUpReservedFiles.Add(new ReservedFileInfo(tuple.Item1, record, tuple.Item3));
                    }
                }

                // Remove all versions which have been deleted already.
                foreach (int ver in verSet)
                {
                    verList.Remove(ver);
                    modified = true;
                }

                // Save record to database if modified.
                if (modified)
                {
                    record.ReservedVersionList = verList;
                    dbContext.CleanUpRecords.Update(record);
                    dbContext.SaveChanges();
                }
            });
        }

        /// <summary>
        /// Clean up reserved files need cleaning up.
        /// </summary>
        /// <param name="data">Execution data.</param>
        protected void CleanUpReservedFiles(ExecutionData data)
        {
            // Try to clean up reserved files.
            var modifiedRecords = new ConcurrentBag<CleanUpRecord>();
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.Instance.FileOperationMaxParallelCount };
            Parallel.ForEach(data.NeedCleanUpReservedFiles, option, info =>
            {
                var file = info.File;
                var record = info.CleanUpRecord;
                if (file.TryDelete(out var exRecord))
                {
                    data.DeletedReservedFiles.Add(file);
                    var verList = record.ReservedVersionList;
                    verList.Remove(info.Version);
                    record.ReservedVersionList = verList;
                    record.LastOperateTime = DateTime.Now;
                    modifiedRecords.Add(record);
                }
                else
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            });

            // Delete empty directories in reserved path.
            var reservedTree = new DirectoryTree(CleanUpBehavior.ReservedPath!.Directory, null);
            reservedTree.Construct();
            var emptyDirs = reservedTree.QueryEmptyDirectories();
            Parallel.ForEach(emptyDirs, option, emptyDir =>
            {
                if (!emptyDir.TryDelete(out var exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            });

            // Save changes to database.
            using var dbContext = GetDbContext();
            foreach (var record in modifiedRecords)
            {
                dbContext.CleanUpRecords.Update(record);
            }
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Sync all files found need syncing.
        /// </summary>
        /// <param name="data">Execution data.</param>
        protected void SyncFiles(ExecutionData data)
        {
            // Create parent directory of syncing file if doesn't exist.
            data.SetState(ExecutionState.SyncFiles);
            var option = new ParallelOptions() { MaxDegreeOfParallelism = Config.Instance.FileOperationMaxParallelCount };
            Parallel.ForEach(data.NeedSyncDirectories, option, directory =>
            {
                if (!directory.TryCreate(out var exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                }
            });

            // Sync files.
            Parallel.ForEach(data.NeedSyncFiles, option, filePair =>
            {
                using var dbContext = GetDbContext();
                var srcFile = filePair.Item1;
                var dstFile = filePair.Item2;
                if (!srcFile.TryCopyTo(dstFile, out var exRecord))
                {
                    data.ExceptionRecords.Add(exRecord);
                    return;
                }

                string relativeName = Source.GetRelativePath(srcFile);
                var record = dbContext.FileRecords.FirstOrDefault(record => record.RelativeFileName == relativeName);
                if (record == null)
                {
                    record = new FileRecord(srcFile, relativeName);
                    dbContext.FileRecords.Add(record);
                }
                else
                {
                    record.UpdateFileInfo(srcFile);
                    record.SyncCount += 1;
                    record.LastSyncTime = DateTime.Now;
                }
                dbContext.SaveChanges();
                data.SyncedFiles.Add(srcFile);
            });

            // Update records which files don't need syncing.
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
                    record.UpdateFileInfo(file);
                    record.SyncCount += 1;
                    record.LastSyncTime = DateTime.Now;
                }
                data.SyncedFiles.Add(file);
            }
            dbContext.SaveChanges();
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

            var dbContext = GetDbContext(pooling: false);
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

            dbContext.SaveChanges();
            dbContext.Dispose();
            SqliteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            ParentProject.SaveWhole();
        }
    }
}
