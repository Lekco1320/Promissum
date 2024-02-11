using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;

namespace Lekco.Promissum.Sync
{
    public static class SyncHelper
    {
        public static bool MoveTo(FileInfo file, string newFileName, bool overwrite, [MaybeNullWhen(true)] out FailedSyncRecord record)
        {
            record = null;
            try
            {
                file.MoveTo(newFileName, overwrite);
                return true;
            }
            catch (SecurityException)
            {
                record = new FailedSyncRecord(file, SyncAction.Move, SyncFailedFlag.NoPermission);
            }
            catch (UnauthorizedAccessException)
            {
                record = new FailedSyncRecord(file, SyncAction.Move, SyncFailedFlag.UnauthorizedAccess);
            }
            catch (FileNotFoundException)
            {
                record = new FailedSyncRecord(file, SyncAction.Move, SyncFailedFlag.FileNotFound);
            }
            catch (DirectoryNotFoundException)
            {
                record = new FailedSyncRecord(file, SyncAction.Move, SyncFailedFlag.DirectoryNotFound);
            }
            catch (PathTooLongException)
            {
                record = new FailedSyncRecord(file, SyncAction.Move, SyncFailedFlag.PathTooLong);
            }
            return false;
        }

        public static bool Delete(FileInfo file, [MaybeNullWhen(true)] out FailedSyncRecord record)
        {
            record = null;
            try
            {
                file.Delete();
                return true;
            }
            catch (IOException)
            {
                record = new FailedSyncRecord(file, SyncAction.Delete, SyncFailedFlag.InvalidPath);
            }
            catch (SecurityException)
            {
                record = new FailedSyncRecord(file, SyncAction.Delete, SyncFailedFlag.NoPermission);
            }
            catch (UnauthorizedAccessException)
            {
                record = new FailedSyncRecord(file, SyncAction.Delete, SyncFailedFlag.UnauthorizedAccess);
            }
            return false;
        }

        public static bool Delete(DirectoryInfo directory, [MaybeNullWhen(true)] out FailedSyncRecord record)
        {
            record = null;
            try
            {
                directory.Delete(true);
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                record = new FailedSyncRecord(directory, SyncAction.Delete, SyncFailedFlag.DirectoryNotFound);
            }
            catch (IOException)
            {
                record = new FailedSyncRecord(directory, SyncAction.Delete, SyncFailedFlag.InvalidPath);
            }
            catch (SecurityException)
            {
                record = new FailedSyncRecord(directory, SyncAction.Delete, SyncFailedFlag.NoPermission);
            }
            catch (UnauthorizedAccessException)
            {
                record = new FailedSyncRecord(directory, SyncAction.Delete, SyncFailedFlag.UnauthorizedAccess);
            }
            return false;
        }

        public static bool Create(DirectoryInfo directory, [MaybeNullWhen(true)] out FailedSyncRecord record)
        {
            record = null;
            try
            {
                directory.Create();
                return true;
            }
            catch
            {
                record = new FailedSyncRecord(directory, SyncAction.Create, SyncFailedFlag.NoPermission);
            }
            return false;
        }

        public static bool CopyTo(FileInfo file, string newFileName, [MaybeNullWhen(false)] out FileInfo newFile, [MaybeNullWhen(true)] out FailedSyncRecord record)
        {
            newFile = null;
            record = null;
            try
            {
                newFile = file.CopyTo(newFileName, true);
                return true;
            }
            catch (SecurityException)
            {
                record = new FailedSyncRecord(file, SyncAction.Copy, SyncFailedFlag.NoPermission);
            }
            catch (DirectoryNotFoundException)
            {
                record = new FailedSyncRecord(file, SyncAction.Copy, SyncFailedFlag.DirectoryNotFound);
            }
            catch (PathTooLongException)
            {
                record = new FailedSyncRecord(file, SyncAction.Copy, SyncFailedFlag.PathTooLong);
            }
            catch (IOException)
            {
                record = new FailedSyncRecord(file, SyncAction.Copy, SyncFailedFlag.FileOccupied);
            }
            catch (UnauthorizedAccessException)
            {
                record = new FailedSyncRecord(file, SyncAction.Copy, SyncFailedFlag.UnauthorizedAccess);
            }
            return false;
        }
    }
}
