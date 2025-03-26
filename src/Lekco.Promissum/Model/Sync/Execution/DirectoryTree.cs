using Lekco.Promissum.Model.Sync.Base;
using Lekco.Wpf.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Lekco.Promissum.Model.Sync.Execution
{
    /// <summary>
    /// Represents the tree structure of a directory.
    /// It is designed for comparing directory structures.
    /// </summary>
    [DebuggerDisplay("{FullName,nq}")]
    public class DirectoryTree
    {
        /// <summary>
        /// Name of directory.
        /// </summary>
        public string Name => Directory.Name;

        /// <summary>
        /// Full name of directory.
        /// </summary>
        public string FullName => Directory.FullName;

        /// <summary>
        /// Intrinsic directory.
        /// </summary>
        public DirectoryBase Directory { get; }

        /// <summary>
        /// Rules for file exclusion.
        /// </summary>
        public List<ExclusionRule>? ExclusionRules { get; }

        /// <summary>
        /// Indicates whether the comparison of path is case-sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; }

        /// <summary>
        /// Files in the directory.
        /// </summary>
        public Dictionary<string, FileBase> Files { get; protected set; }

        /// <summary>
        /// Directories in the directory.
        /// </summary>
        public Dictionary<string, DirectoryBase> Directories { get; protected set; }

        /// <summary>
        /// Directory trees in the directory.
        /// </summary>
        public Dictionary<string, DirectoryTree> DirectoryTrees { get; protected set; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="directory">Intrinsic directory.</param>
        /// <param name="rules">Rules for file exclusion.</param>
        public DirectoryTree(DirectoryBase directory, List<ExclusionRule>? rules)
        {
            Directory = directory;
            ExclusionRules = rules;
            Files = new Dictionary<string, FileBase>();
            Directories = new Dictionary<string, DirectoryBase>();
            DirectoryTrees = new Dictionary<string, DirectoryTree>();
        }

        /// <summary>
        /// Construct the whole directory tree structure.
        /// </summary>
        public void Construct()
        {
            Files = Directory.EnumerateFiles()
                             .Where(f => !MatchRules(f))
                             .ToDictionary(f => f.Name);
            Directories = Directory.EnumerateDirectories()
                                   .ToDictionary(d => d.Name);
            DirectoryTrees = Directories.Values.Select(d => new DirectoryTree(d, ExclusionRules))
                                               .ToDictionary(t => t.Name);
            var option = new ParallelOptions() { MaxDegreeOfParallelism = App.Config.Instance.FileOperationMaxParallelCount };
            Parallel.ForEach(DirectoryTrees.Values, option, tree => tree.Construct());
        }

        /// <summary>
        /// Match given file with exclusion rules.
        /// </summary>
        /// <param name="file">Given file.</param>
        /// <returns><see langword="true"/> if matches; otherwise, returns <see langword="false"/>.</returns>
        public bool MatchRules(FileBase file)
        {
            if (ExclusionRules == null)
            {
                return false;
            }

            foreach (var rule in ExclusionRules)
            {
                if (rule.Matches(file))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handled as a new directory tree in comparison.
        /// Add all new entities into result recursively.
        /// </summary>
        /// <param name="result">Result of comparison.</param>
        protected void HandledAsNewTree(ComparisonResult result)
        {
            result.NewDirectories.Add(Directory);
            foreach (var file in Files.Values)
            {
                result.NewFiles.Add(file);
            }
            foreach (var tree in DirectoryTrees.Values)
            {
                tree.HandledAsNewTree(result);
            }
        }

        /// <summary>
        /// Handled as a new directory tree in comparison.
        /// Add all deleted entities into result recursively.
        /// </summary>
        /// <param name="result">Result of comparison.</param>
        protected void HandledAsDeleteTree(ComparisonResult result)
        {
            result.DeletedDirectories.Add(Directory);
            foreach (var file in Files.Values)
            {
                result.DeletedFiles.Add(file);
            }
            foreach (var tree in DirectoryTrees.Values)
            {
                tree.HandledAsDeleteTree(result);
            }
        }

        /// <summary>
        /// Compare with a given tree.
        /// </summary>
        /// <param name="other">Given tree.</param>
        /// <param name="compareMode">Mode for comparing files.</param>
        /// <returns>Result of comparison.</returns>
        public ComparisonResult CompareTo(DirectoryTree other, FileCompareMode compareMode, bool caseSensitive)
        {
            var result = new ComparisonResult();
            CompareTo(result, other, compareMode, caseSensitive);
            return result;
        }

        /// <summary>
        /// Compare with a given tree.
        /// </summary>
        /// <param name="result">Result of comparison.</param>
        /// <param name="other">Given tree.</param>
        /// <param name="compareMode">Mode for comparing files.</param>
        protected void CompareTo(ComparisonResult result, DirectoryTree other, FileCompareMode compareMode, bool caseSensitive)
        {
            var data = new ComparisonData(this, other, caseSensitive);
            var tasks = new List<Task>();
            foreach (var pair in DirectoryTrees)
            {
                string thisDirName = pair.Key;
                DirectoryTree thisTree = pair.Value;
                if (data.OtherDirectoryTrees.TryGetValue(thisDirName, out DirectoryTree? otherTree))
                {
                    var compareTask = new Task(() => thisTree.CompareTo(result, otherTree, compareMode, caseSensitive));
                    tasks.Add(compareTask);
                    compareTask.Start();
                    data.UnshotDirectories.Remove(otherTree.Name);
                }
                else
                {
                    result.NewDirectories.Add(thisTree.Directory);
                    thisTree.HandledAsNewTree(result);
                }
            }
            foreach (string unshotDirName in data.UnshotDirectories)
            {
                result.DeletedDirectories.Add(data.OtherDirectories[unshotDirName]);
                data.OtherDirectoryTrees[unshotDirName].HandledAsDeleteTree(result);
            }

            foreach (var pair in Files)
            {
                string thisFileName = pair.Key;
                FileBase thisFile = pair.Value;
                if (data.OtherFiles.TryGetValue(thisFileName, out FileBase? otherFile))
                {
                    if (thisFile.CompareTo(otherFile, compareMode))
                    {
                        result.DifferentFiles.Add(new Pair<FileBase, FileBase>(thisFile, otherFile));
                    }
                    else
                    {
                        result.SameFiles.Add(thisFile);
                    }
                    data.UnshotFiles.Remove(otherFile.Name);
                }
                else
                {
                    result.NewFiles.Add(thisFile);
                }
            }
            foreach (string unshotFileName in data.UnshotFiles)
            {
                result.DeletedFiles.Add(data.OtherFiles[unshotFileName]);
            }
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Query all files.
        /// </summary>
        /// <returns>All files in the directory tree.</returns>
        public IEnumerable<FileBase> QueryFiles()
            => QueryFiles(_ => true);

        /// <summary>
        /// Query files that satisfy the given criteria.
        /// </summary>
        /// <param name="criteria">Given criteria.</param>
        /// <returns>All files satify the given criteria in the directory tree.</returns>
        public IEnumerable<FileBase> QueryFiles(Func<FileBase, bool> criteria)
        {
            var result = new ConcurrentBag<FileBase>();
            QueryFiles(result, criteria);
            return result.ToList();
        }

        /// <summary>
        /// Internal implement for querying files.
        /// </summary>
        /// <param name="result">Query result.</param>
        /// <param name="criteria">Given criteria.</param>
        protected void QueryFiles(ConcurrentBag<FileBase> result, Func<FileBase, bool> criteria)
        {
            foreach (var file in Files.Values.Where(criteria))
            {
                result.Add(file);
            }
            Parallel.ForEach(DirectoryTrees.Values, tree => tree.QueryFiles(result, criteria));
        }

        /// <summary>
        /// Query all empty directories.
        /// </summary>
        /// <returns>All empty directories.</returns>
        public IEnumerable<DirectoryBase> QueryEmptyDirectories()
        {
            var result = new List<DirectoryBase>();
            QueryEmptyDirectories(result);
            return result;
        }

        /// <summary>
        /// Internal implement for querying empty directories.
        /// </summary>
        /// <param name="result">Query result.</param>
        /// <returns><see langword="true"/> if empty; otherwise, returns <see langword="false"/>.</returns>
        protected bool QueryEmptyDirectories(List<DirectoryBase> result)
        {
            if (Files.Count > 0)
            {
                return false;
            }
            foreach (var tree in DirectoryTrees.Values)
            {
                if (!tree.QueryEmptyDirectories(result))
                {
                    return false;
                }
            }
            result.AddRange(Directories.Values);
            return true;
        }

        /// <summary>
        /// Stores result of <see cref="CompareTo(DirectoryTree, FileCompareMode, bool)"/>.
        /// </summary>
        public class ComparisonResult
        {
            /// <summary>
            /// New files in source directory tree.
            /// </summary>
            public ConcurrentBag<FileBase> NewFiles { get; } = new();

            /// <summary>
            /// New directories in source directory tree.
            /// </summary>
            public ConcurrentBag<DirectoryBase> NewDirectories { get; } = new();

            /// <summary>
            /// Same files judged by givn criteria.
            /// </summary>
            public ConcurrentBag<FileBase> SameFiles { get; } = new();

            /// <summary>
            /// Different files with same name in two directories.
            /// </summary>
            public ConcurrentBag<Pair<FileBase, FileBase>> DifferentFiles { get; } = new();

            /// <summary>
            /// Deleted files in source directory tree.
            /// </summary>
            public ConcurrentBag<FileBase> DeletedFiles { get; } = new();

            /// <summary>
            /// Deleted directories in source directory tree.
            /// </summary>
            public ConcurrentBag<DirectoryBase> DeletedDirectories { get; } = new();
        }

        /// <summary>
        /// Stores data of <see cref="CompareTo(DirectoryTree, FileCompareMode, bool)"/>.
        /// </summary>
        public class ComparisonData
        {
            /// <summary>
            /// Indicates whether comparison is case-sensitive.
            /// </summary>
            public bool IsCaseSensitive { get; }

            /// <summary>
            /// Files in this directory.
            /// </summary>
            public FrozenDictionary<string, FileBase> ThisFiles { get; }

            /// <summary>
            /// Directories in this directory.
            /// </summary>
            public FrozenDictionary<string, DirectoryBase> ThisDirectories { get; }

            /// <summary>
            /// Directory trees in this directory.
            /// </summary>
            public FrozenDictionary<string, DirectoryTree> ThisDirectoryTrees { get; }

            /// <summary>
            /// Files in this directory.
            /// </summary>
            public FrozenDictionary<string, FileBase> OtherFiles { get; }

            /// <summary>
            /// Directories in this directory.
            /// </summary>
            public FrozenDictionary<string, DirectoryBase> OtherDirectories { get; }

            /// <summary>
            /// Directory trees in this directory.
            /// </summary>
            public FrozenDictionary<string, DirectoryTree> OtherDirectoryTrees { get; }

            /// <summary>
            /// Unshot files.
            /// </summary>
            public HashSet<string> UnshotFiles { get; }

            /// <summary>
            /// Unshot directories.
            /// </summary>
            public HashSet<string> UnshotDirectories { get; }

            /// <summary>
            /// Create an instance.
            /// </summary>
            /// <param name="caseSensitive">Indicates whether comparison is case-sensitive.</param>
            public ComparisonData(DirectoryTree thisTree, DirectoryTree otherTree, bool caseSensitive)
            {
                IsCaseSensitive = caseSensitive;
                var comparer = IsCaseSensitive ? null : new CaseInsensitiveStringComparer();
                ThisFiles = thisTree.Files.ToFrozenDictionary(comparer);
                ThisDirectories = thisTree.Directories.ToFrozenDictionary(comparer);
                ThisDirectoryTrees = thisTree.DirectoryTrees.ToFrozenDictionary(comparer);
                OtherFiles = otherTree.Files.ToFrozenDictionary(comparer);
                OtherDirectories = otherTree.Directories.ToFrozenDictionary(comparer);
                OtherDirectoryTrees = otherTree.DirectoryTrees.ToFrozenDictionary(comparer);
                UnshotFiles = new HashSet<string>(OtherFiles.Keys, comparer);
                UnshotDirectories = new HashSet<string>(OtherDirectories.Keys, comparer);
            }
        }
    }
}
