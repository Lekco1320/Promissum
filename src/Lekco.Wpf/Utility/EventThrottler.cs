using System;
using System.Diagnostics;

namespace Lekco.Wpf.Utility
{
    /// <summary>
    /// The class throttle events which are invoked very frequently to limit them.
    /// </summary>
    /// <typeparam name="TEventArgs">Type of the event arguments.</typeparam>
    public class EventThrottler<TEventArgs> where TEventArgs : EventArgs
    {
        private DateTime _lastInvokeTime;
        private readonly TimeSpan _throttledTimeSpan;
        private readonly object _lock = new object();

        /// <summary>
        /// Events need to be throttled.
        /// </summary>
        public event EventHandler<TEventArgs>? ThrottledEvent;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="throttledTimeSpan">Time span to throttle the event</param>
        public EventThrottler(TimeSpan throttledTimeSpan)
        {
            _throttledTimeSpan = throttledTimeSpan;
        }

        /// <summary>
        /// Raise the event if throttling conditions are met.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        public void RaiseEvent(object sender, TEventArgs e)
        {
            Debug.WriteLine($"{DateTime.Now} Entered.");
            lock (_lock)
            {
                var now = DateTime.Now;
                if (now - _lastInvokeTime <= _throttledTimeSpan)
                {
                    return;
                }
                _lastInvokeTime = now;
            }
            Debug.WriteLine($"{DateTime.Now} Invoked.");
            ThrottledEvent?.Invoke(sender, e);
        }
    }
}
