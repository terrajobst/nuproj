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
            PackageFiles = (from item in OutputsWithTargetFrameworkInformation.Select(x => ConvertToPackageFile(x, this))
                         let packagePath = item?.GetMetadata(Metadata.FileTarget)
                         where seenPackagePaths.Add(packagePath)
                         select item).ToArray();


            return true;
        }

        private static ITaskItem ConvertToPackageFile(ITaskItem output, Task task)
        {
            var fileName = output.ItemSpec;
            var targetPath = output.GetMetadata("TargetPath");
            targetPath = string.IsNullOrEmpty(targetPath) ? Path.GetFileName(fileName) : targetPath;
            var frameworkNameMoniker = output.GetTargetFrameworkMoniker();
            var packageDirectory = output.GetPackageDirectory();
            var targetFramework = frameworkNameMoniker.GetShortFrameworkName();
            var metadata = output.CloneCustomMetadata();

            task.Log.LogMessage(
                "ConvertToPackageFile: fileName={0} => targetFramework={3}, frameworkName={4}, frameworkProfile={5}, targetPath={1}, packageDirectory={2}", fileName,
                targetPath, packageDirectory, targetFramework, frameworkNameMoniker.FullName, frameworkNameMoniker.Profile);

            if (string.IsNullOrEmpty(targetFramework))
            {
                task.Log.LogError("Unable to determine targetFramework for {0}", targetPath);

                return null;
            }

            metadata[Metadata.TargetFramework] = targetFramework;
            metadata[Metadata.PackageDirectory] = packageDirectory.ToString();
            metadata[Metadata.FileTarget] = packageDirectory.Combine(targetFramework, targetPath);
            return new TaskItem(fileName, metadata);
        }
    }
}