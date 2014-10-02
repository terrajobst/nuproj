using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NuGet;

namespace NuProj.Tasks
{
    public class ReadPackagesConfig : Task
    {
        [Required]
        public string ProjectPath { get; set; }

        [Output]
        public ITaskItem[] PackageReferences { get; set; }

        public override bool Execute()
        {
            var packageReferenceFile = PackageReferenceFile.CreateFromProject(ProjectPath);
            PackageReferences = packageReferenceFile.GetPackageReferences().Select(ConvertPackageElement).ToArray();
            return true;
        }

        protected ITaskItem ConvertPackageElement(PackageReference packageReference)
        {
            var id = packageReference.Id;
            var version = packageReference.Version;
            var targetFramework = packageReference.TargetFramework;
            var isDevelopmentDependency = packageReference.IsDevelopmentDependency;
            var requireReinstallation = packageReference.RequireReinstallation;
            var versionConstraint = packageReference.VersionConstraint;

            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var packageDirectoryPath = GetPackageDirectoryPath(ProjectPath, id, version);
            metadata.Add("PackageDirectoryPath", packageDirectoryPath);
            metadata.Add("ProjectPath", ProjectPath);

            metadata.Add("IsDevelopmentDependency", isDevelopmentDependency.ToString());
            metadata.Add("RequireReinstallation", requireReinstallation.ToString());

            if (version != null)
                metadata.Add("Version", version.ToString());

            if (targetFramework != null)
                metadata.Add("TargetFramework", VersionUtility.GetShortFrameworkName(targetFramework));

            if (versionConstraint != null)
                metadata.Add("VersionConstraint", versionConstraint.ToString());

            var item = new TaskItem(id, metadata);
            return item;
        }

        private static string GetPackageDirectoryPath(string packagesConfigPath, string packageId, SemanticVersion packageVersion)
        {
            var packageDirectoryName = packageId + "." + packageVersion;
            var candidateFolder = Path.GetDirectoryName(packagesConfigPath);
            while (candidateFolder != null)
            {
                var packageDirectoryPath = Path.Combine(candidateFolder, "packages", packageDirectoryName);
                if (Directory.Exists(packageDirectoryPath))
                    return packageDirectoryPath;

                candidateFolder = Path.GetDirectoryName(candidateFolder);
            }

            return string.Empty;
        }
    }
}