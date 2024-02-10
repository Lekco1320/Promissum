using Lekco.Promissum.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Apps
{
    [DataContract]
    public class Config
    {
        public static HashSet<string> AutoRunProjects { get; protected set; }
        public static bool AutoStartUp { get; set; }
        public static bool AlwaysTellsMeWhenDeleteFiles { get; set; }
        public static int MessageWindowWaitingSeconds { get; set; }
        public static int MaxParallelCopyCounts { get; set; }
        public static Version AppVersion { get; set; }

        public static readonly string FileName = App.AppDir + @"\config.xml";

        static Config()
        {
            Config config;
            if (File.Exists(FileName))
            {
                config = Functions.ReadFromFile<Config>(FileName);
                AutoRunProjects = config._autoRunProjects;
                AutoStartUp = config._autoStartUp;
                AlwaysTellsMeWhenDeleteFiles = config._alwaysTellsMeWhenDeleteFiles;
                MessageWindowWaitingSeconds = config._messageWindowWaitingSeconds;
                MaxParallelCopyCounts = config._maxParallelCopyCounts;
                AppVersion = config._appVersion;
                return;
            }
            AutoRunProjects = new HashSet<string>();
            AutoStartUp = true;
            AlwaysTellsMeWhenDeleteFiles = true;
            MessageWindowWaitingSeconds = 30;
            MaxParallelCopyCounts = 3;
            AppVersion = App.Version;
        }

        [DataMember]
        private HashSet<string> _autoRunProjects;

        [DataMember]
        private bool _autoStartUp;

        [DataMember]
        private bool _alwaysTellsMeWhenDeleteFiles;

        [DataMember]
        private int _messageWindowWaitingSeconds;

        [DataMember]
        private int _maxParallelCopyCounts;

        [DataMember]
        private Version _appVersion;

        private Config()
        {
            _autoRunProjects = AutoRunProjects;
            _autoStartUp = AutoStartUp;
            _alwaysTellsMeWhenDeleteFiles = AlwaysTellsMeWhenDeleteFiles;
            _messageWindowWaitingSeconds = MessageWindowWaitingSeconds;
            _maxParallelCopyCounts = MaxParallelCopyCounts;
            _appVersion = AppVersion;
        }

        public static void SaveAsFile()
        {
            Functions.SaveAsFile(new Config(), FileName);
        }
    }
}
