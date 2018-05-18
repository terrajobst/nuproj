using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NuGet.Packaging;
using NuGet.Versioning;

namespace NuProj.Tasks
{
    public class ReadPackagesConfig : Task
    {
        public string SolutionDir { get; set; }

        [Required]
        public ITaskItem[] Projects { get; set; }

        [Output]
        public ITaskItem[] PackageReferences { get; set; }

        public override bool Execute()
        {
            var packageReferences = from project in Projects
                                    let projectPackageReferences = GetInstalledPackageReferences(project.GetMetadata("FullPath"))
                                    from packageReference in projectPackageReferences
                                    select ConvertPackageElement(project, packageReference);
            PackageReferences = packageReferences.ToArray();
            return true;
        }

        protected ITaskItem ConvertPackageElement(ITaskItem project, PackageReference packageReference)
        {
            var id = packageReference.PackageIdentity.Id;
            var version = packageReference.PackageIdentity.Version;
            var targetFramework = packageReference.TargetFramework;
            var isDevelopmentDependency = packageReference.IsDevelopmentDependency;
            var requireReinstallation = packageReference.RequireReinstallation;
            var versionConstraint = packageReference.AllowedVersions;

            var item = new TaskItem(id);
            project.CopyMetadataTo(item);

            var packageDirectoryPath = GetPackageDirectoryPath(project.GetMetadata("FullPath"), id, version);
            item.SetMetadata("PackageDirectoryPath", packageDirectoryPath);
            item.SetMetadata("ProjectPath", project.GetMetadata("FullPath"));

            item.SetMetadata("IsDevelopmentDependency", isDevelopmentDependency.ToString());
            item.SetMetadata("RequireReinstallation", requireReinstallation.ToString());

            if (version != null)
                item.SetMetadata(Metadata.Version, version.ToString());

            if (targetFramework != null)
                item.SetMetadata(Metadata.TargetFramework, targetFramework.GetShortFolderName());

            if (versionConstraint != null)
                item.SetMetadata("VersionConstraint", versionConstraint.ToString());

            return item;
        }

        private string GetPackageDirectoryPath(string packagesConfigPath, string packageId, SemanticVersion packageVersion)
        {
            var packageDirectoryName = packageId + "." + packageVersion;

            if (!string.IsNullOrWhiteSpace(SolutionDir))
            {
                var packageDirectoryPath = Path.Combine(SolutionDir, "packages", packageDirectoryName);
                if (Directory.Exists(packageDirectoryPath))
                    return packageDirectoryPath;
            }

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

        private static IEnumerable<PackageReference> GetInstalledPackageReferences(string projectPath)
        {
            var projectDirectory = Path.GetDirectoryName(projectPath);
            var projectName = Path.GetFileNameWithoutExtension(projectPath);

            var projectConfigFilePath = Path.Combine(projectDirectory, $"packages.{projectName}.config");
            if (File.Exists(projectConfigFilePath))
            {
                return GetInstalledPackageReferencesFromConfigFile(projectConfigFilePath, false);
            }

            projectConfigFilePath = Path.Combine(projectDirectory, "packages.config");
            if (File.Exists(projectConfigFilePath))
            {
                return GetInstalledPackageReferencesFromConfigFile(projectConfigFilePath, false);
            }

            return Enumerable.Empty<PackageReference>();

        }

        private static IEnumerable<PackageReference> GetInstalledPackageReferencesFromConfigFile(
            string projectConfigFilePath,
            bool allowDuplicatePackageIds)
        {
            using (var file = File.Open(projectConfigFilePath, FileMode.Open, FileAccess.Read))
            {
                var reader = new PackagesConfigReader(file);
                return reader.GetPackages(allowDuplicatePackageIds);

            }
        }

    }
}