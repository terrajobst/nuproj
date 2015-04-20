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
            var targetFramework = "";
            if (output.UseTargetFrameworkMoniker())
            {
                var frameworkNameMoniker = output.GetTargetFrameworkMoniker();
                targetFramework = frameworkNameMoniker.GetShortFrameworkName();
            }
            else
            {
                var frameworkName = output.GetTargetFramework();
                targetFramework = frameworkName.GetShortFrameworkName();
            }

            var packageDirectory = output.GetPackageDirectory();
            var metadata = output.CloneCustomMetadata();
            metadata[Metadata.TargetFramework] = targetFramework;
            metadata[Metadata.PackageDirectory] = packageDirectory.ToString();
            metadata[Metadata.FileTarget] = packageDirectory.Combine(targetFramework, Path.GetFileName(fileName));
            return new TaskItem(fileName, metadata);
        }
    }
}