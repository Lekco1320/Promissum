using System.ComponentModel;

namespace Lekco.Promissum.Model.Sync.Base
{
    /// <summary>
    /// Defines constants for drive types.
    /// </summary>
    public enum DriveType
    {
        /// <summary>
        /// Unkown type.
        /// </summary>
        [Description("未知驱动器类型")]
        Unknown = 0,

        /// <summary>
        /// Drive without root directory.
        /// </summary>
        [Description("无根目录的驱动器")]
        NoRootDirectory = 1,

        /// <summary>
        /// Removable drive.
        /// </summary>
        [Description("可移动驱动器")]
        Removable = 2,

        /// <summary>
        /// Fixed drive.
        /// </summary>
        [Description("固定驱动器")]
        Fixed = 3,

        /// <summary>
        /// Network drive.
        /// </summary>
        [Description("网络驱动器")]
        Network = 4,

        /// <summary>
        /// CD-ROM drive.
        /// </summary>
        [Description("光盘驱动器")]
        CDRom = 5,

        /// <summary>
        /// RAM drive.
        /// </summary>
        [Description("RAM磁盘驱动器")]
        Ram = 6,

        /// <summary>
        /// Generic MTP device.
        /// </summary>
        [Description("通用MTP设备")]
        GenericMTPDevice = 7,

        /// <summary>
        /// Camera device.
        /// </summary>
        [Description("照相机")]
        Camera = 8,

        /// <summary>
        /// Media player device.
        /// </summary>
        [Description("媒体播放器")]
        MediaPlayer = 9,

        /// <summary>
        /// Phone device.
        /// </summary>
        [Description("手机")]
        Phone = 10,

        /// <summary>
        /// A video device.
        /// </summary>
        [Description("视频设备")]
        Video = 11,

        /// <summary>
        /// Personal information manager device.
        /// </summary>
        [Description("个人信息管理器")]
        PersonalInformationManager = 12,

        /// <summary>
        /// Audio recorder.
        /// </summary>
        [Description("音频录音设备")]
        AudioRecorder = 13,
    }
}
