using Lekco.Promissum.Utility;
using System;
using System.Runtime.Serialization;

namespace Lekco.Promissum.Apps
{
    [DataContract]
    public sealed class Promissum
    {
        [DataMember]
        public string ProgramName { get; }

        [DataMember]
        public Version Version { get; }

        [DataMember]
        public object? MetaData { get; }

        public Promissum(object? metaData = null)
        {
            ProgramName = App.Name;
            Version = App.Version;
            MetaData = metaData;
        }

        public void Check()
        {
            if (ProgramName != App.Name)
            {
                throw new ProgramNameNotMatchException(ProgramName);
            }
            if (Version < App.Version)
            {
                throw new LowerVersionException(Version);
            }
            if (Version > App.Version)
            {
                throw new HigherVersionException(Version);
            }
        }

        public static Promissum ReadFromFile(string fileName)
        {
            return Functions.ReadFromFile<Promissum>(fileName);
        }
    }

    public class LowerVersionException : Exception
    {
        public Version ThisVersion { get; }

        public LowerVersionException(Version thisVersion)
            : base("This version is lower than the version of current application.")
        {
            ThisVersion = thisVersion;
        }
    }

    public class HigherVersionException : Exception
    {
        public Version ThisVersion { get; }

        public HigherVersionException(Version thisVersion)
            : base("This version is higher than the version of current application.")
        {
            ThisVersion = thisVersion;
        }
    }

    public class ProgramNameNotMatchException : Exception
    {
        public string ThisProgramName { get; }

        public ProgramNameNotMatchException(string thisProgramName)
            : base("This program's name doesn't match.")
        {
            ThisProgramName = thisProgramName;
        }
    }
}
