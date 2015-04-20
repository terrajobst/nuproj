﻿using System;
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
        private static readonly FrameworkName AnyFramework = new FrameworkName("Any,Version=v1.0");

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
            var metadataValue = taskItem.GetMetadata(Metadata.TargetFramework);
            if (string.IsNullOrEmpty(metadataValue))
            {
                result = AnyFramework;
            }
            else
            {
                result = VersionUtility.ParseFrameworkName(metadataValue);
            }

            return result;
        }

        public static bool UseTargetFrameworkMoniker(this ITaskItem taskItem)
        {
            var metadataValue = taskItem.GetMetadata(Metadata.TargetFramework);
            return string.Equals(metadataValue, Metadata.TargetFrameworkMoniker, StringComparison.OrdinalIgnoreCase);
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
                result = AnyFramework;
            }

            return result;
        }

        public static PackageDirectory GetPackageDirectory(this ITaskItem taskItem)
        {
            var packageDirectoryName = taskItem.GetMetadata(Metadata.PackageDirectory);
            if (string.IsNullOrEmpty(packageDirectoryName))
            {
                return PackageDirectory.Lib;
            }

            PackageDirectory result;
            Enum.TryParse(packageDirectoryName, true, out result);
            return result;
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
            if (frameworkName == null || frameworkName == AnyFramework)
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

        public static string Combine(this PackageDirectory packageDirectory, string targetFramework, string fileName)
        { 
            switch (packageDirectory)
            {
                case PackageDirectory.Root:
                    return fileName;
                case PackageDirectory.Content:
                    return Combine(Constants.ContentDirectory, fileName);
                case PackageDirectory.Build:
                    return Combine(Constants.BuildDirectory, targetFramework, fileName);
                case PackageDirectory.Lib:
                    return Combine(Constants.LibDirectory, targetFramework, fileName);
                case PackageDirectory.Tools:
                    return Combine(Constants.ToolsDirectory, targetFramework, fileName);
                default:
                    return fileName;
            }
        }

        private static string Combine(params string[] paths)
        {
            return Path.Combine(paths.Where(x => !string.IsNullOrEmpty(x)).ToArray());
        }
    }
}
