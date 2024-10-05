using System;
using Lekco.Wpf.Utility.Helper;

namespace Lekco.Wpf.Utility
{
    /// <summary>
    /// A struct for describing size of file.
    /// </summary>
    public struct FileSize
    {
        /// <summary>
        /// Unit of the size.
        /// </summary>
        public FileSizeUnit Unit { get; set; }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long SizeInBytes { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="bytes">Size in bytes.</param>
        /// <param name="unit">Unit of the size.</param>
        public FileSize(long bytes, FileSizeUnit unit = FileSizeUnit.Auto)
            : this()
        {
            SizeInBytes = bytes;
            Unit = unit;
        }

        public FileSize(double fitted, FileSizeUnit unit)
            : this()
        {
            SizeInBytes = unit switch
            {
                FileSizeUnit.B => (long)fitted,
                FileSizeUnit.KB => (long)(fitted * 1024),
                FileSizeUnit.MB => (long)(fitted * 1024 * 1024),
                FileSizeUnit.GB => (long)(fitted * 1024 * 1024 * 1024),
                FileSizeUnit.TB => (long)(fitted * 1024 * 1024 * 1024 * 1024),
                _ => throw new ArgumentException("Invalid unit of file size.", nameof(unit)),
            };
            Unit = unit;
        }

        public readonly double Fit(out FileSizeUnit unit)
        {
            if (SizeInBytes == long.MaxValue)
            {
                unit = FileSizeUnit.B;
                return SizeInBytes;
            }
            double value = SizeInBytes;
            int index = 1;
            while (value >= 1024d && index < 5)
            {
                value /= 1024d;
                ++index;
            }
            unit = (FileSizeUnit)index;
            return value;
        }

        /// <inheritdoc />
        public override readonly string ToString()
        {
            if (SizeInBytes == long.MaxValue)
            {
                return "∞";
            }
            if (Unit != FileSizeUnit.Auto)
            {
                return $"{SizeInBytes / Math.Pow(1024, (int)Unit):0.##} {Unit.GetDiscription()}";
            }
            double value = Fit(out FileSizeUnit unit);
            return $"{value:0.##} {unit}";
        }

        public readonly string ToString(int digits)
        {
            if (SizeInBytes == long.MaxValue)
            {
                return "∞";
            }
            double value;
            if (Unit != FileSizeUnit.Auto)
            {
                value = SizeInBytes / Math.Pow(1024, (int)Unit);
                value = digits > 0 ? Math.Round(value, digits) : Math.Ceiling(value);
                return $"{value} {Unit.GetDiscription()}";
            }
            value = Fit(out FileSizeUnit unit);
            value = digits > 0 ? Math.Round(value, digits) : Math.Ceiling(value);
            return $"{value} {unit}";
        }

        public static bool operator >(FileSize left, FileSize right)
            => left.SizeInBytes > right.SizeInBytes;

        public static bool operator <(FileSize left, FileSize right)
            => left.SizeInBytes < right.SizeInBytes;

        public static bool operator >=(FileSize left, FileSize right)
            => left.SizeInBytes >= right.SizeInBytes;

        public static bool operator <=(FileSize left, FileSize right)
            => left.SizeInBytes <= right.SizeInBytes;

        public static bool operator ==(FileSize left, FileSize right)
            => left.SizeInBytes == right.SizeInBytes && left.Unit == right.Unit;

        public static bool operator !=(FileSize left, FileSize right)
            => left.SizeInBytes != right.SizeInBytes || left.Unit != right.Unit;

        /// <inheritdoc />
        public override readonly bool Equals(object? obj)
        {
            if (obj is FileSize other)
            {
                return this == other;
            }
            return false;
        }

        /// <inheritdoc />
        public override readonly int GetHashCode()
            => HashCode.Combine(SizeInBytes, Unit);
    }

    /// <summary>
    /// Define units of file size.
    /// </summary>
    public enum FileSizeUnit
    {
        /// <summary>
        /// Auto
        /// </summary>
        Auto = 0,

        /// <summary>
        /// In bytes.
        /// </summary>
        B = 1,

        /// <summary>
        /// In kilobytes.
        /// </summary>
        KB = 2,

        /// <summary>
        /// In megabytes.
        /// </summary>
        MB = 3,

        /// <summary>
        /// In gigabytes.
        /// </summary>
        GB = 4,

        /// <summary>
        /// In terabytes.
        /// </summary>
        TB = 5,
    }
}
