using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        public static string GetTextBetween(this string text, string start, string end, bool lastIndexOfStart = true, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var i = lastIndexOfStart ? text.LastIndexOf(start, comparisonType) : text.IndexOf(start, comparisonType);
            if (i == -1)
                return null;
            var value = text.Substring(i + start.Length);
            i = value.IndexOf(end, comparisonType);
            return i == -1 ? null : value.Substring(0, i);
        }

        public static string GetMetadata(this ITaskItem taskItem, string metadataName, string defaultValue)
        {
            var metadataValue = taskItem.GetMetadata(metadataName);
            return string.IsNullOrEmpty(metadataValue) ? defaultValue : metadataValue;
        }

        public static FrameworkName GetTargetFramework(this ITaskItem taskItem)
        {
            FrameworkName result = null;
            var metadataValue = taskItem.GetMetadata(Metadata.TargetFramework);
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

        public static FrameworkName GetTargetFrameworkMoniker(this ITaskItem taskItem)
        {
            FrameworkName result = null;
            var metadataValue = taskItem.GetMetadata(Metadata.TargetFrameworkMoniker);
            if (!string.IsNullOrEmpty(metadataValue))
            {
                result = new FrameworkName(metadataValue);
            }
            else
            {
                result = NullFramework;
            }

            return result;
        }

        public static PackageDirectory GetPackageDirectory(this ITaskItem taskItem, PackageDirectory defaultValue = PackageDirectory.Root)
        {
            var packageDirectoryName = taskItem.GetMetadata(Metadata.PackageDirectory);
            if (string.IsNullOrEmpty(packageDirectoryName))
            {
                return defaultValue;
            }

            PackageDirectory result;
            Enum.TryParse(packageDirectoryName, true, out result);
            return result;
        }

        public static void GetTargetPackageDirectory(this ITaskItem taskItem, out PackageDirectory packageDirectory, out string directoryPath)
        {
            var fileTarget = taskItem.GetMetadata(Metadata.FileTarget) ?? string.Empty;
            var fileTargetParts = fileTarget.Split(new[] { '\\', '/' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (fileTargetParts.Length <= 1)
            {
                packageDirectory = PackageDirectory.Root;
                directoryPath = fileTarget;
                return;
            }

            Enum.TryParse(fileTargetParts[0], true, out packageDirectory);
            if (packageDirectory == PackageDirectory.Root)
            {
                directoryPath = fileTarget;
            }
            else
            {
                directoryPath = fileTargetParts[1];
            }
        }

        public static string GetTargetSubdirectory(this ITaskItem taskItem)
        {
            return  taskItem.GetMetadata(Metadata.TargetSubdirectory) ?? string.Empty;
        }

        public static IVersionSpec GetVersion(this ITaskItem taskItem)
        {
            IVersionSpec result = null;
            var metadataValue = taskItem.GetMetadata(Metadata.Version);
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

            if (frameworkName.Identifier == ".NETPortable" && frameworkName.Version.Major == 5 && frameworkName.Version.Minor == 0)
            {
                // Avoid calling GetShortFrameworkName because NuGet throws ArgumentException
                // in this case.
                return "dotnet";
            }

            return VersionUtility.GetShortFrameworkName(frameworkName);
        }

        public static string GetAnalyzersFrameworkName(this FrameworkName frameworkName)
        {
            // At this time there is no host other than Roslyn compiler that can run analyzers. 
            // Therefore, Framework Name and Version should always be specified as 'dotnet' until another host is 
            // implemented that has runtime restrictions.
            return "dotnet";

        }

        public static string ToStringSafe(this object value)
        {
            if (value == null)
            {
                return null;
            }

            return value.ToString();
        }

        public static void UpdateMember<T>(this T target, Expression<Func<T, string>> memberLamda, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }
            
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }
            
            property.SetValue(target, value, null);
        }

        public static void AddRangeToMember<T, TItem>(this T target, Expression<Func<T, List<TItem>>> memberLamda, IEnumerable<TItem> value)
        {
            if (value == null || value.Count() == 0)
            {
                return;
            }
            
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }

            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }

            var list = (List<TItem>)property.GetValue(target) ?? new List<TItem>();
            list.AddRange(value);
            
            property.SetValue(target, list, null);
        }

        public static string Combine(this PackageDirectory packageDirectory, string targetFramework, string targetSubdirectory, string fileName)
        { 
            switch (packageDirectory)
            {
                case PackageDirectory.Root:
                    return Path.Combine(targetSubdirectory, fileName);
                case PackageDirectory.Content:
                    return Path.Combine(Constants.ContentDirectory, targetSubdirectory, fileName);
                case PackageDirectory.ContentFiles:
                    return Path.Combine(Constants.ContentFilesDirectory, targetSubdirectory, fileName);
                case PackageDirectory.Build:
                    return Path.Combine(Constants.BuildDirectory, targetSubdirectory, fileName);
                case PackageDirectory.Lib:
                    return Path.Combine(Constants.LibDirectory, targetFramework, targetSubdirectory, fileName);
                case PackageDirectory.Tools:
                    return Path.Combine(Constants.ToolsDirectory, targetSubdirectory, fileName);
                case PackageDirectory.Analyzers:
                    return Path.Combine(Constants.AnalyzersDirectory, targetFramework, targetSubdirectory, fileName);
                default:
                    return fileName;
            }
        }
    }
}
