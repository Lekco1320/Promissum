using Lekco.Wpf.Utility.Helper;
using System;
using System.IO;
using System.IO.Compression;

namespace Lekco.Promissum.Model.Sync
{
    public class SyncProjectFile
    {
        public string FileName { get; }

        public string WorkDirectory { get; }

        public SyncProject SyncProject { get; }

        public SyncProjectFile(string fileName)
        {
            FileName = fileName;
            WorkDirectory = $"{App.Promissum.TempDir}\\{DateTime.Now.GetHashCode()}";
            CheckDirectory();
            string tempFileName = WorkDirectory + @"\project.xml";
            SyncProject = SyncProject.ReadFromFile(tempFileName);
            SyncProject.ParentFile = this;
        }

        public SyncProjectFile(string fileName, string projectName, bool isAutoLoad)
        {
            FileName = fileName;
            WorkDirectory = $"{App.Promissum.TempDir}\\{DateTime.Now.GetHashCode()}";
            CheckDirectory();
            SyncProject = new SyncProject(projectName, WorkDirectory + @"\project.xml")
            {
                IsAutoLoad = isAutoLoad,
                ParentFile = this,
            };
            SyncProject.Save();
            Save();
        }

        protected void CheckDirectory()
        {
            if (!Directory.Exists(WorkDirectory))
            {
                if (File.Exists(FileName))
                {
                    ZipFile.ExtractToDirectory(FileName, WorkDirectory);
                }
                else
                {
                    Directory.CreateDirectory(WorkDirectory);
                }
            }
        }

        public bool Save()
        {
            CheckDirectory();
            try
            {
                using var fileStream = new FileStream(FileName, FileMode.Create);
                ZipFile.CreateFromDirectory(WorkDirectory, fileStream);
                return true;
            }
            catch (IOException)
            {
                DialogHelper.ShowErrorAsync("保存失败：文件被占用或已丢失。");
            }
            return false;
        }

        public string GetWorkFileName(string relativeName)
            => Path.Combine(WorkDirectory, relativeName);
    }
}
