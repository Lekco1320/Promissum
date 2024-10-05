using Lekco.Promissum.Model.Sync;
using Lekco.Promissum.Model.Sync.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lekco.Promissum.ViewModel
{
    public class FileListWindowVM
    {
        public int FilesCount => FilesVM.Count();

        public IEnumerable<FileVM> FilesVM { get; }

        public FileListWindowVM(IEnumerable<FileBase> files)
        {
            int id = 1;
            FilesVM = files.Select(f => new FileVM(id++, f));
        }

        public FileListWindowVM(IEnumerable<RelativedFile> files)
        {
            int id = 1;
            FilesVM = files.Select(f => new FileVM(id++, f));
        }

        public class FileVM
        {
            public int ID { get; }

            public FileBase File { get; }

            public string FileFullName { get; }

            public long FileSize => File.Size;

            public DateTime CreationTime => File.CreationTime;

            public DateTime LastWriteTime => File.LastWriteTime;

            public FileVM(int id, FileBase file)
            {
                ID = id;
                File = file;
                FileFullName = file.FullName;
            }

            public FileVM(int id, RelativedFile relativedFile)
            {
                ID = id;
                File = relativedFile.ActualFile;
                FileFullName = relativedFile.RelativePath;
            }
        }
    }
}
