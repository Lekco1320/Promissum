using System.Runtime.InteropServices;

namespace Lekco.Promissum.Utility
{
    /// <summary>
    /// A helper class for getting information of computer.
    /// </summary>
    public static class ComputerHelper
    {
        /// <summary>
        /// Get computer's current memory status and information.
        /// </summary>
        /// <param name="memoryInfo">The information of memory.</param>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatus(ref MemoryInfo memoryInfo);

        /// <summary>
        /// Get computer's current memory information.
        /// </summary>
        /// <returns></returns>
        public static MemoryInfo GetMemoryStatus()
        {
            var memoryInfo = default(MemoryInfo);
            memoryInfo.dwLength = (uint)Marshal.SizeOf(memoryInfo);
            GlobalMemoryStatus(ref memoryInfo);
            return memoryInfo;
        }

        /// <summary>
        /// Get computer's available memory.
        /// </summary>
        /// <returns>Computer's available memory.</returns>
        public static ulong GetAvailableMemory()
        {
            var memoryStatus = GetMemoryStatus();
            return memoryStatus.ullAvailPhys;
        }

        /// <summary>
        /// The information structure of computer's memory.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryInfo
        {
            /// <summary>
            /// Current structure size.
            /// </summary>
            public uint dwLength;

            /// <summary>
            /// Current memory utilization.
            /// </summary>
            public uint dwMemoryLoad;

            /// <summary>
            /// Total physical memory size.
            /// </summary>
            public ulong ullTotalPhys;

            /// <summary>
            /// Available physical memory size.
            /// </summary>
            public ulong ullAvailPhys;

            /// <summary>
            /// Total exchange file size.
            /// </summary>
            public ulong ullTotalPageFile;

            /// <summary>
            /// Total exchange file size.
            /// </summary>
            public ulong ullAvailPageFile;

            /// <summary>
            /// Total virtual memory size.
            /// </summary>
            public ulong ullTotalVirtual;

            /// <summary>
            /// Available virtual memory size.
            /// </summary>
            public ulong ullAvailVirtual;

            /// <summary>
            /// Keep this value always zero.
            /// </summary>
            public ulong ullAvailExtendedVirtual;
        }
    }
}
