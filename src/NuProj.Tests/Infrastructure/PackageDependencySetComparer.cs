using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace NuProj.Tests.Infrastructure
{
    public class PackageDependencySetComparer : IEqualityComparer<PackageDependencySet>
    {
        private static PackageDependencySetComparer _instance = new PackageDependencySetComparer(StringComparer.OrdinalIgnoreCase);
        private PackageDependencyComparer _packageDependencyComparer;
        private StringComparer _stringComparer;

        public PackageDependencySetComparer(StringComparer stringComparer)
        {
            if (stringComparer == null)
            {
                throw new ArgumentNullException("stringComparer");
            }
            _stringComparer = stringComparer;
            _packageDependencyComparer = new PackageDependencyComparer(stringComparer);
        }

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
                _packageDependencyComparer);

            var yDependencies = new HashSet<PackageDependency>(
                y.Dependencies ?? Enumerable.Empty<PackageDependency>(),
                _packageDependencyComparer);

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