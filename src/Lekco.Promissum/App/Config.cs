using Lekco.Wpf.Utility.Helper;
using System;
using System.Runtime.Serialization;

namespace Lekco.Promissum.App
{
    /// <summary>
    /// The class storing configuration of Lekco Promissum.
    /// </summary>
    [DataContract]
    public sealed class Config
    {
        /// <summary>
        /// Indicate whether Lekco Promissum starts up when PC launched.
        /// </summary>
        [DataMember]
        public bool AutoStartUp { get; set; }

        /// <summary>
        /// Indicate whether always notify users when deleting files.
        /// </summary>
        [DataMember]
        public bool AlwaysNotifyWhenDelete { get; set; }

        /// <summary>
        /// Seconds to count down to close a dialog.
        /// </summary>
        [DataMember]
        public int DialogCountDownSeconds { get; set; }

        /// <summary>
        /// Max parallel count for operating files.
        /// </summary>
        [DataMember]
        public int FileOperationMaxParallelCount { get; set; }

        /// <summary>
        /// Current version of Lekco Promissum.
        /// </summary>
        public Version AppVersion { get; }

        /// <summary>
        /// Instance of the class.
        /// </summary>
        public static Config Instance { get; }

        /// <summary>
        /// File name of the config.
        /// </summary>
        public static string FileName => Promissum.AppDir + @"\config.xml";

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Config()
        {
            try
            {
                Instance = DataContractHelper.DeserilizeFromFile<Config>(FileName);
                Validate();
            }
            catch
            {
                Instance = new Config();
                SaveAsFile();
            }
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        private Config()
        {
            AutoStartUp = true;
            AlwaysNotifyWhenDelete = true;
            DialogCountDownSeconds = 30;
            FileOperationMaxParallelCount = 3;
            AppVersion = Promissum.Version;
        }

        /// <summary>
        /// Validate whether config value are suitable.
        /// </summary>
        private static void Validate()
        {
            bool errorValue = false;
            if (Instance.DialogCountDownSeconds <= 0 || Instance.DialogCountDownSeconds > 300)
            {
                Instance.DialogCountDownSeconds = 30;
                errorValue = true;
            }
            if (Instance.FileOperationMaxParallelCount <= 0 || Instance.FileOperationMaxParallelCount > 10)
            {
                Instance.FileOperationMaxParallelCount = 3;
                errorValue = true;
            }
            if (errorValue)
            {
                SaveAsFile();
            }
        }

        /// <summary>
        /// Save data to the dataset.
        /// </summary>
        public static void SaveAsFile()
        {
            DataContractHelper.SerilizeToFile(Instance, FileName);
        }
    }
}
