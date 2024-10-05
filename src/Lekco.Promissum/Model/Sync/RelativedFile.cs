using Lekco.Promissum.Model.Sync.Base;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// The class represents a file relative to a specified path.
    /// </summary>
    public class RelativedFile
    {
        /// <summary>
        /// The actual file.
        /// </summary>
        public FileBase ActualFile { get; }

        /// <summary>
        /// The relative base of the file in the path form.
        /// </summary>
        public PathBase RelativeBase { get; }

        /// <summary>
        /// Relative path of the file to a specified base.
        /// </summary>
        public string RelativePath => RelativeBase.GetRelativePath(ActualFile);

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="actualFile">The actual file.</param>
        /// <param name="relativeBase">The relative base of the file in the path form.</param>
        public RelativedFile(FileBase actualFile, PathBase relativeBase)
        {
            ActualFile = actualFile;
            RelativeBase = relativeBase;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="actualFile">The actual file.</param>
        /// <param name="drive">The drive the file relative to.</param>
        public RelativedFile(FileBase actualFile, DriveBase drive)
        {
            ActualFile = actualFile;
            RelativeBase = drive.RootPath;
        }
    }
}
