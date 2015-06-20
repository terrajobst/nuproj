using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NuGet;

namespace NuProj.Tasks
{
    public class ReadPackagesConfig : Task
    {
        [Required]
        public ITaskItem[] Projects { get; set; }

        [Output]
        public ITaskItem[] PackageReferences { get; set; }

        public override bool Execute()
        {
            var packageReferences = from project in Projects
                                    let packageReferenceFile = PackageReferenceFile.CreateFromProject(project.GetMetadata("FullPath"))
                                    from packageReference in packageReferenceFile.GetPackageReferences()
                                    select ConvertPackageElement(project, packageReference);
            PackageReferences = packageReferences.ToArray();
            return true;
        }

        protected ITaskItem ConvertPackageElement(ITaskItem project, PackageReference packageReference)
        {
            var id = packageReference.Id;
            var version = packageReference.Version;
            var targetFramework = packageReference.TargetFramework;
            var isDevelopmentDependency = packageReference.IsDevelopmentDependency;
            var requireReinstallation = packageReference.RequireReinstallation;
            var versionConstraint = packageReference.VersionConstraint;

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
                item.SetMetadata(Metadata.TargetFramework, targetFramework.GetShortFrameworkName());

            if (versionConstraint != null)
                item.SetMetadata("VersionConstraint", versionConstraint.ToString());
            else
                item.SetMetadata("VersionConstraint", "*");

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