using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    public class ExpandPropertiesTests
    {
        [Fact]
        public async Task Tasks_ExpandProperties()
        {
            var solutionPath = Assets.GetScenarioSolutionPath("Task_ExpandProperties");
            var projectPath = Path.GetDirectoryName(solutionPath);

            var result1 = await MSBuild.ExecuteAsync(solutionPath, "Build");
            result1.AssertSuccessfulBuild();

            Manifest manifest;

            using (var manifestStream = File.OpenRead(Path.Combine(projectPath, @"NuGetPackage\obj\debug\NuGetPackage.nuspec")))
                manifest = Manifest.ReadFrom(manifestStream, false);

            Assert.Equal(manifest.Metadata.Version, "1.2.3.4");
            Assert.Equal(manifest.Metadata.Title, "ClassLibrary Title");
            Assert.Equal(manifest.Metadata.Authors, "ClassLibrary Product | 5.6");
            Assert.Equal(manifest.Metadata.Description, @"http://example.com");
        }
    }
}
