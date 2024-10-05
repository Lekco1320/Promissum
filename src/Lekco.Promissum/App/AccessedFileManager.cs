using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Lekco.Promissum.App
{
    /// <summary>
    /// The class for managing all accessed files of Lekco Promissum.
    /// </summary>
    public static class AccessedFileManager
    {
        /// <summary>
        /// All accessed files.
        /// </summary>
        public static ObservableCollection<AccessedFile> AccessedFiles { get; }

        /// <summary>
        /// The static command for removing an accessed file.
        /// </summary>
        public static ICommand RemoveFileCommand => new RelayCommand<AccessedFile>(RemoveAccessedFile);

        /// <summary>
        /// The file name of the dataset.
        /// </summary>
        private static readonly string DataSetFileName = Promissum.AppDir + @"\AccessedFiles.xml";

        /// <summary>
        /// Static constructor.
        /// </summary>
        static AccessedFileManager()
        {
            try
            {
                AccessedFiles = DataContractHelper.DeserilizeFromFile<ObservableCollection<AccessedFile>>(DataSetFileName);
            }
            catch
            {
                AccessedFiles = new ObservableCollection<AccessedFile>();
                SaveToDataSet();
            }
            foreach (var file in AccessedFiles.ToList())
            {
                if (!file.Exist)
                {
                    AccessedFiles.Remove(file);
                }
            }
        }

        /// <summary>
        /// Save data to the dataset.
        /// </summary>
        public static void SaveToDataSet()
            => DataContractHelper.SerilizeToFile(AccessedFiles, DataSetFileName);

        /// <summary>
        /// Add an accessed file into the dataset.
        /// </summary>
        /// <param name="filePath">The file's path.</param>
        public static void AddAccessedFile(string filePath)
        {
            for (int i = 0; i < AccessedFiles.Count; i++)
            {
                var file = AccessedFiles[i];
                if (file.FullName == filePath)
                {
                    file.LastAccessTime = DateTime.Now;
                    AccessedFiles.Move(i, 0);
                    SaveToDataSet();
                    return;
                }
            }

            var accessedFile = new AccessedFile(filePath);
            AccessedFiles.Insert(0, accessedFile);
            SaveToDataSet();
        }

        /// <summary>
        /// Remove a specified accessed file from the dataset.
        /// </summary>
        /// <param name="file">Specified file.</param>
        public static void RemoveAccessedFile(AccessedFile file)
        {
            AccessedFiles.Remove(file);
            SaveToDataSet();
        }
    }
}
