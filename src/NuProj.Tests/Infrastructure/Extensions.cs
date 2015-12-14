using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace NuProj.Tests.Infrastructure
{
    public static class Extensions
    {
        public static IEnumerable<string> Flatten(this IEnumerable<PackageDependencySet> dependencySets)
        {
            return from formattedDependency in
                       (from dependencySet in dependencySets
                        from dependency in dependencySet.Dependencies
                        let dependencyString = dependency.ToString().Replace("\u2265", ">=").Replace("\u2264", "<=")
                        select dependencySet.TargetFramework == null
                        ? dependencyString
                        : string.Format("{0} ({1})", dependencyString, VersionUtility.GetShortFrameworkName(dependencySet.TargetFramework)))
                   orderby formattedDependency
                   select formattedDependency;
        }

        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<T>();
            }

            return source;
        }
    }
}