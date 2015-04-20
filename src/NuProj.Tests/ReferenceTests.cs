using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    public class ReferenceTests
    {
        [Fact]
        public async Task References_PackagedWithCopyLocal()
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync();
            Assert.NotNull(package.GetFile("A2.dll"));
            Assert.Null(package.GetFile("A3.dll")); // CopyLocal=false
            Assert.Null(package.GetFile("A4.dll")); // ExcludeFromNuPkg=true
        }

        [Fact]
        public async Task References_MultipleFrameworks_ReferenceAll()
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync("References_MultipleFrameworks", "ReferenceAll");
            var expectedFileNames = new[]
            {
                @"lib\net40\net40.dll",
                @"lib\net45\net40.dll",
                @"lib\net45\net45.dll",
                @"lib\net451\net40.dll",
                @"lib\net451\net45.dll",
                @"lib\net451\net451.dll",
                @"Readme.txt",
            };
            var files = package.GetFiles().Select(f => f.Path);
            Assert.Equal(expectedFileNames, files);
        }

        [Fact]
        public async Task References_MultipleFrameworks_ReferenceNet451()
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync("References_MultipleFrameworks", "ReferenceNet451");
            var expectedFileNames = new[]
            {
                @"lib\net451\net40.dll",
                @"lib\net451\net45.dll",
                @"lib\net451\net451.dll",
                @"Readme.txt",
            };
            var files = package.GetFiles().Select(f => f.Path);
            Assert.Equal(expectedFileNames, files);
        }

        [Theory]
        [InlineData("PackageToBuild", new[] { @"build\net45\Tool.dll" }, new string[0])]
        [InlineData("PackageToLib", new[] { @"lib\net45\Tool.dll" }, new string[0])]
        [InlineData("PackageToRoot", new[] { @"Tool.dll", @"Tool.pdb" }, new string[0])]
        [InlineData("PackageToTools", new[] { @"tools\net45\Tool.dll" }, new string[0])]
        [InlineData("PackageDependencyToTools", new[] { @"tools\net45\Tool.dll" }, new[] { "PackageToTools (>= 1.0.0)" })]
        [InlineData("PackageClosureToTools", new[] { @"tools\net45\Tool.dll", @"tools\net45\ToolWithClosure.dll" }, new string[0])]
        [InlineData("PackageToContent", new[] { @"content\Tool.dll", @"content\Tool.pdb" }, new string[0])]
        [InlineData("PackageNuGetDependencyToTools", new[] { @"tools\net451\System.Collections.Immutable.dll", @"tools\net451\ToolWithDependency.dll" }, new string[0])]
        public async Task References_PackageDirectory_ToolIsPackaged(
            string packageId, 
            string[] expectedFiles, 
            string[] expectedDependencies)
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync("References_PackageDirectory", packageId);
            var actualFiles = package.GetFiles().Select(f => f.Path).OrderBy(x => x);
            var actualDependencies = package.DependencySets.NullAsEmpty().Flatten().OrderBy(x => x);
            expectedFiles = expectedFiles.OrderBy(x => x).ToArray();
            expectedDependencies = expectedDependencies.OrderBy(x => x).ToArray();
            Assert.Equal(expectedFiles, actualFiles);
            Assert.Equal(expectedDependencies, actualDependencies);
        }

        [Theory]
        [InlineData("TargetFramework45", new[] { @"lib\net45\Library.dll" })]
        [InlineData("TargetFrameworkAny", new[] { @"lib\Library.dll" })]
        [InlineData("TargetFrameworkMoniker", new[] { @"lib\net40\Library.dll" })]
        public async Task References_TargetFramework_UsesMetadata(string packageId, string[] expectedFiles)
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync("References_TargetFramework", packageId);
            var actualFiles = package.GetFiles().Select(f => f.Path).OrderBy(x => x);
            expectedFiles = expectedFiles.OrderBy(x => x).ToArray();
            Assert.Equal(expectedFiles, actualFiles);
        }
    }
}

