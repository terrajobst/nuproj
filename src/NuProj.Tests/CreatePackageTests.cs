using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    public class CreatePackageTests
    {
        [Fact]
        public async Task CreatePackage_LibraryWithXmlDoc_ShouldNotFail()
        {
            var solutionPath = Assets.GetScenarioSolutionPath("CreatePackage_LibraryWithXmlDoc_ShouldNotFail");
            var result = await MSBuild.RebuildAsync(solutionPath);

            result.AssertSuccessfulBuild();
        }

        [Fact]
        public async Task CreatePackage_SubstituteMacros()
        {
            var properties = MSBuild.Properties.Default
                .Add("NuSpecProperties", "SummaryMacro=TestSummary");
            var package = await Scenario.RestoreAndBuildSinglePackageAsync(properties: properties);
            Assert.Equal("TestSummary", package.Summary);
        }

        [Fact]
        public Task CreatePackage_ContentFilesError()
        {
            return Scenario.RestoreAndFailBuildAsync();
        }

        [Fact]
        public async Task CreatePackage_SetContentFilesMetadata()
        {
            var result = await Scenario.RestoreAndBuildSinglePackageAsync();
            var solutionPath = Assets.GetScenarioSolutionPath();
            var projectPath = Path.GetDirectoryName(solutionPath);
            var nuspecFile = Path.Combine(projectPath, @"Package\obj\Debug\Package.nuspec");
            Manifest manifest;
            using (var stream = File.OpenRead(nuspecFile))
            {
                manifest = Manifest.ReadFrom(stream, false);
            }

            var expected = new[]
            {
                new ManifestContentFiles { Include=@"any\any\1x1.png", BuildAction="EmbeddedResource", CopyToOutput="false", Flatten="false"},
                new ManifestContentFiles { Include=@"any\any\TextFile.txt", BuildAction="None", CopyToOutput="true", Flatten="false"},
                new ManifestContentFiles { Include=@"cs\any\Class.cs.pp", CopyToOutput="false", Flatten="false"},
                new ManifestContentFiles { Include=@"cs\any\Class1.cs.pp", CopyToOutput="false", Flatten="false"}
            };

            Assert.Equal<ManifestContentFiles>(
                expected, 
                manifest.Metadata.ContentFiles.OrderBy(x => x.Include), 
                ManifestContentFilesComparer.Instance);
        }
    }
}