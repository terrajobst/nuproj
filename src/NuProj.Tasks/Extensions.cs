using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using NuGet;

namespace NuProj.Tasks
{
    public static class Extensions
    {
        private static readonly FrameworkName NullFramework = new FrameworkName("Null,Version=v1.0");

        public static bool GetBoolean(this ITaskItem taskItem, string metadataName, bool defaultValue = false)
        {
            bool result = false;
            var metadataValue = taskItem.GetMetadata(metadataName);
            bool.TryParse(metadataValue, out result);
            return result;
        }

        public static FrameworkName GetTargetFramework(this ITaskItem taskItem)
        {
            FrameworkName result = null;
            var metadataValue = taskItem.GetMetadata("TargetFramework");
            if (!string.IsNullOrEmpty(metadataValue))
            {
                result = VersionUtility.ParseFrameworkName(metadataValue);
            }
            else
            {
                result = NullFramework;
            }

            return result;
        }

        public static IVersionSpec GetVersion(this ITaskItem taskItem)
        {
            IVersionSpec result = null;
            var metadataValue = taskItem.GetMetadata("Version");
            if (!string.IsNullOrEmpty(metadataValue))
            {
                VersionUtility.TryParseVersionSpec(metadataValue, out result);
            }

            return result;
        }

        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<T>();
            }

            return source;
        }

        public static string GetShortFrameworkName(this FrameworkName frameworkName)
        {
            if (frameworkName == null || frameworkName == NullFramework)
            {
                return null;
            }

            return VersionUtility.GetShortFrameworkName(frameworkName);
        }

        public static string ToStringSafe(this object value)
        {
            if (value == null)
            {
                return null;
            }

            return value.ToString();
        }
    }
}
