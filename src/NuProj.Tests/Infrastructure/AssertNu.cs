using System;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;

using Xunit;

namespace NuProj.Tests.Infrastructure
{
    /// <summary>
    /// NuProj specific assertions.
    /// </summary>
    public static class AssertNu
    {
        /// <summary>
        /// Asserts that a build completed successfully and without logging any errors.
        /// </summary>
        public static void SuccessfulBuild(MSBuild.BuildResultAndLogs result)
        {
            result.AssertSuccessfulBuild();
        }

        /// <summary>
        /// Asserts that a NuPkg archive contains a file with the specified relative path.
        /// </summary>
        /// <param name="nupkgPath">The path to the zip archive.</param>
        /// <param name="packedFile">The path to the file that should exist in the archive. Case sensitive.</param>
        public static void PackageContains(string nupkgPath, string packedFile)
        {
            var package = NuPkg.GetPackage(nupkgPath);
            Assert.NotNull(package.GetFile(packedFile));
        }

        public static async Task PackageContainsContentItemsAsync(Project nuProj)
        {
            var package = await nuProj.GetPackageAsync();
            
            foreach (var contentItem in nuProj.GetItems("Content"))
            {
                var expectedPath = GetExpectedPackagePathForContent(contentItem);
                Assert.NotNull(package.GetFile(expectedPath));
            }
        }

        private static string GetExpectedPackagePathForContent(ProjectItem item)
        {
            // TODO: account for items with Link metadata.
            return item.EvaluatedInclude;
        }
    }
}
