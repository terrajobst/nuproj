using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NuGet;

namespace NuProj.Tasks
{
    public class AssignTargetFramework : Task
    {
        [Required]
        public ITaskItem[] OutputsWithTargetFrameworkInformation { get; set; }

        [Output]
        public ITaskItem[] PackageFiles { get; set; }

        public override bool Execute()
        {
            var stuff = OutputsWithTargetFrameworkInformation.Select(x => new
            {
                x.ItemSpec,
                Metadata = x.MetadataNames.Cast<string>().ToDictionary(y => y, x.GetMetadata)
            }).ToArray();

            var seenPackagePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            PackageFiles = (from item in OutputsWithTargetFrameworkInformation.Select(ConvertToPackageFile)
                         let packagePath = item.GetMetadata(Metadata.FileTarget)
                         where seenPackagePaths.Add(packagePath)
                         select item).ToArray();


            return true;
        }

        private static ITaskItem ConvertToPackageFile(ITaskItem output)
        {
            var fileName = output.ItemSpec;
            var targetPath = output.GetMetadata("TargetPath");
            targetPath = string.IsNullOrEmpty(targetPath) ? Path.GetFileName(fileName) : targetPath;
            var frameworkNameMoniker = output.GetTargetFrameworkMoniker();
            var packageDirectory = output.GetPackageDirectory();
            var targetFramework = frameworkNameMoniker.GetShortFrameworkName();
            var metadata = output.CloneCustomMetadata();
            metadata[Metadata.TargetFramework] = targetFramework;
            metadata[Metadata.PackageDirectory] = packageDirectory.ToString();
            metadata[Metadata.FileTarget] = packageDirectory.Combine(targetFramework, targetPath);
            return new TaskItem(fileName, metadata);
        }
    }
}