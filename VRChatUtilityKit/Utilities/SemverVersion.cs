using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Utilities
{
    public struct SemverVersion : IEquatable<SemverVersion>
    {
        /// <summary>
        /// The major number of the version.
        /// </summary>
        public int Major { get; set; }
        /// <summary>
        /// The minor number of the version.
        /// </summary>
        public int Minor { get; set; }
        /// <summary>
        /// The patch number of the version.
        /// </summary>
        public int Patch { get; set; }
        /// <summary>
        /// The release type of the version.
        /// </summary>
        public ReleaseType Type { get; set; }

        public SemverVersion(int major, int minor, int patch, ReleaseType releaseType)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Type = releaseType;
        }
        public SemverVersion(string version)
        {
            string[] versionNumbers = version.Split('.', ' ');

            if (versionNumbers.Length != 3 && versionNumbers.Length != 4)
                throw new ArgumentException("Version string in incorrect format");

            Major = int.Parse(versionNumbers[0]);
            Minor = int.Parse(versionNumbers[1]);
            Patch = int.Parse(versionNumbers[2]);

            if (versionNumbers.Length == 4)
            {
                switch (versionNumbers[3].ToLower())
                {
                    case "alpha":
                        Type = ReleaseType.Alpha;
                        break;
                    case "beta":
                        Type = ReleaseType.Beta;
                        break;
                    case "release":
                        Type = ReleaseType.Release;
                        break;
                    default:
                        Type = ReleaseType.Unknown;
                        break;
                }
            }
            else
            {
                Type = ReleaseType.Unknown;
            }
        }

        public override string ToString()
        {
            if (Type != ReleaseType.Unknown)
                return $"v{Major}.{Minor}.{Patch} {Type}";
            else
                return $"v{Major}.{Minor}.{Patch}";
        }
        public override bool Equals(object obj)
        {
            return Equals((SemverVersion)obj);
        }

        public bool Equals(SemverVersion version)
        {
            return version.Major == Major && version.Minor == Minor && version.Patch == Patch;
        }

        public static bool operator ==(SemverVersion lVersion, SemverVersion rVersion)
        {
            return lVersion.Equals(rVersion);
        }
        public static bool operator !=(SemverVersion lVersion, SemverVersion rVersion)
        {
            return !lVersion.Equals(rVersion);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator >(SemverVersion lVersion, SemverVersion rVersion)
        {
            if (lVersion.Major > rVersion.Major)
                return true;

            if (lVersion.Major == rVersion.Major && lVersion.Minor > rVersion.Minor)
                return true;

            if (lVersion.Major == rVersion.Major && lVersion.Minor == rVersion.Minor && lVersion.Patch > rVersion.Patch)
                return true;

            return false;
        }
        public static bool operator <(SemverVersion lVersion, SemverVersion rVersion)
        {
            return !(lVersion > rVersion);
        }

        public static implicit operator SemverVersion(string value)
        {
            return new SemverVersion(value);
        }


        public enum ReleaseType
        {
            Unknown = 0,
            Release = 1,
            Beta = 2,
            Alpha = 4
        }
    }
}
