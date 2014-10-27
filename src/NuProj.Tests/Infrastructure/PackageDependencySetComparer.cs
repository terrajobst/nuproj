using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace NuProj.Tests.Infrastructure
{
    public class PackageDependencySetComparer : IEqualityComparer<PackageDependencySet>
    {
        private static readonly StringComparer _stringComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly PackageDependencySetComparer _instance = new PackageDependencySetComparer();

        public static PackageDependencySetComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(PackageDependencySet x, PackageDependencySet y)
        {
            if (x == null && x == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var xDependencies = new HashSet<PackageDependency>(
                x.Dependencies ?? Enumerable.Empty<PackageDependency>(),
                PackageDependencyComparer.Instance);

            var yDependencies = new HashSet<PackageDependency>(
                y.Dependencies ?? Enumerable.Empty<PackageDependency>(),
                PackageDependencyComparer.Instance);

            return x.TargetFramework == y.TargetFramework
                && xDependencies.SetEquals(yDependencies);
        }

        public int GetHashCode(PackageDependencySet obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return obj.TargetFramework == null ? 0 : obj.TargetFramework.GetHashCode();
        }
    }
}