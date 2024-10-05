using Lekco.Promissum.Model.Engine;
using Lekco.Promissum.Model.Sync.Base;
using System;

namespace Lekco.Promissum.Model.Sync
{
    /// <summary>
    /// Represents an exception that is thrown when a drive is not ready.
    /// </summary>
    public class DriveNotReadyException : Exception
    {
        /// <summary>
        /// Get the drive that caused the exception.
        /// </summary>
        public DriveBase? Drive { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public DriveNotReadyException()
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DriveNotReadyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="drive">The drive that caused the exception.</param>
        public DriveNotReadyException(string message, DriveBase drive)
            : base(message)
        {
            Drive = drive;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public DriveNotReadyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents an exception that is thrown when a task is not ready.
    /// </summary>
    public class TaskNotReadyException : Exception
    {
        /// <summary>
        /// Get the task that caused the exception.
        /// </summary>
        public TaskBase? Task { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public TaskNotReadyException()
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TaskNotReadyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="task">The task that caused the exception.</param>
        public TaskNotReadyException(string message, TaskBase task)
            : base(message)
        {
            Task = task;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public TaskNotReadyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents an exception that is thrown when a task is busy.
    /// </summary>
    public class TaskIsBusyException : Exception
    {
        /// <summary>
        /// Get the task that caused the exception.
        /// </summary>
        public TaskBase? Task { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public TaskIsBusyException()
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TaskIsBusyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="task">The task that caused the exception.</param>
        public TaskIsBusyException(TaskBase task)
            : this($"\"{task.Name}\"正忙。", task)
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="task">The task that caused the exception.</param>
        public TaskIsBusyException(string message, TaskBase task)
            : base(message)
        {
            Task = task;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public TaskIsBusyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Represents an exception that is thrown when a task is suspended.
    /// </summary>
    public class TaskSuspendedException : Exception
    {
        /// <summary>
        /// Get the task that caused the exception.
        /// </summary>
        public TaskBase? Task { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        public TaskSuspendedException()
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TaskSuspendedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="task">The task that caused the exception.</param>
        public TaskSuspendedException(string message, TaskBase task)
            : base(message)
        {
            Task = task;
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public TaskSuspendedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
