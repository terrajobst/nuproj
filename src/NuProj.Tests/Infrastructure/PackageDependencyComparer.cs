using System;
using System.Collections.Generic;
using NuGet;

namespace NuProj.Tests.Infrastructure
{
    public class PackageDependencyComparer : IEqualityComparer<PackageDependency>
    {
        private static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly PackageDependencyComparer _instance = new PackageDependencyComparer();

        public static PackageDependencyComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(PackageDependency x, PackageDependency y)
        {
            if (x == null && x == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var xVersionSpec = x.VersionSpec == null ? "" : x.VersionSpec.ToString();
            var yVersionSpec = y.VersionSpec == null ? "" : y.VersionSpec.ToString();
            return _stringComparer.Equals(x.Id, y.Id)
                && _stringComparer.Equals(xVersionSpec, yVersionSpec);
        }

        public int GetHashCode(PackageDependency obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var versionSpec = obj.VersionSpec == null ? "" : obj.VersionSpec.ToString();
            return _stringComparer.GetHashCode(obj.Id ?? "") + _stringComparer.GetHashCode(versionSpec);
        }
    }
}