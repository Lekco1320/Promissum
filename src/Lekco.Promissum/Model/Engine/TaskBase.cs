using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Model.Engine
{
    /// <summary>
    /// The base class for describing task in Lekco Promissum.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Name}")]
    public abstract class TaskBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Name of the task.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Creation time of the task.
        /// </summary>
        [DataMember]
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// Last time the task was executed.
        /// </summary>
        [DataMember]
        public DateTime LastExecuteTime { get; protected set; }

        /// <summary>
        /// Indicates whether the task is ready to execute.
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Indicates whether the task is suspended.
        /// </summary>
        [DataMember]
        public bool IsSuspended { get; set; }

        /// <summary>
        /// Indicates whether the task is busy.
        /// </summary>
        public bool IsBusy { get; private set; }

        /// <summary>
        /// The unique ID of the task for loading its datasets.
        /// </summary>
        [DataMember]
        public string ID { get; protected set; }

        /// <summary>
        /// Lock object to ensure that only one thread can execute the task at a time.
        /// </summary>
        private object _taskLock;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="name">Name of the task.</param>
        protected TaskBase(string name)
        {
            Name = name;
            CreationTime = DateTime.Now;
            ID = Guid.NewGuid().ToString()[..8];
            _taskLock = new object();
        }

        /// <summary>
        /// Called after being deserialized to complete construction.
        /// </summary>
        /// <param name="context">Context of deserialization.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            _taskLock = new object();
        }

        /// <summary>
        /// Take busy action with the task.
        /// </summary>
        /// <param name="action">Busy action.</param>
        public void BusyAction(Action action)
        {
            lock (_taskLock)
            {
                IsBusy = true;
                action();
                IsBusy = false;
            }
        }

        /// <summary>
        /// Take busy function with the task.
        /// </summary>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="func">Busy function.</param>
        /// <returns>Return value of the function.</returns>
        public TResult BusyAction<TResult>(Func<TResult> func)
        {
            TResult result;
            lock (_taskLock)
            {
                IsBusy = true;
                result = func();
                IsBusy = false;
            }
            return result;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
