using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace MultiRPC.JsonClasses
{
    public class Version
    {
        public Version()
        {
        }

        [JsonConstructor]
        public Version(int major, int minor, int build, int revision, int majorRevision, int minorRevision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
            MajorRevision = majorRevision;
            MinorRevision = minorRevision;
        }

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public int Revision { get; private set; }
        public int MajorRevision { get; private set; }
        public int MinorRevision { get; private set; }

        public static Version SysVersionToMultiVersion(System.Version version)
        {
            return new Version
            {
                Major = version.Major,
                Minor = version.Minor,
                Build = version.Build,
                Revision = version.Revision,
                MajorRevision = version.MajorRevision,
                MinorRevision = version.MinorRevision
            };
        }

        public static bool operator <=(Version a, Version b)
        {
            if (a.Major <= b.Major)
            {
                return true;
            }
            if (a.Minor <= b.Minor)
            {
                return true;
            }
            if (a.Build <= b.Build)
            {
                return true;
            }
            if (a.Revision <= b.Revision)
            {
                return true;
            }
            if (a.MajorRevision <= b.MajorRevision)
            {
                return true;
            }
            if (a.MinorRevision <= b.MinorRevision)
            {
                return true;
            }

            return false;
        }

        public static bool operator >=(Version a, Version b)
        {
            if (a.Major >= b.Major)
            {
                return true;
            }
            if (a.Minor >= b.Minor)
            {
                return true;
            }
            if (a.Build >= b.Build)
            {
                return true;
            }
            if (a.Revision >= b.Revision)
            {
                return true;
            }
            if (a.MajorRevision >= b.MajorRevision)
            {
                return true;
            }
            if (a.MinorRevision >= b.MinorRevision)
            {
                return true;
            }

            return false;
        }
    }
}
