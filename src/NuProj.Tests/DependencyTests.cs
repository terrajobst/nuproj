using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using NuGet;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    public class DependencyTests
    {
        [Fact]
        public async Task Dependency_Content_IsNotFiltered()
        {
            var package = await Scenario.RestoreAndBuildSinglePackage("Dependency_Content_IsNotFiltered");
            var expectedFileNames = new[]
            {
                @"content\jquery-2.1.1.js",
                @"lib\net45\ClassLibrary1.dll"
            };
            var files = package.GetFiles().Select(f => f.Path);
            Assert.Equal(expectedFileNames, files);
        }

        [Fact]
        public async Task Dependency_Tools_IsNotFiltered()
        {
            var package = await Scenario.RestoreAndBuildSinglePackage("Dependency_Tools_IsNotFiltered");
            var expectedFileNames = new[]
            {
                @"lib\net45\ClassLibrary1.dll",
                @"tools\Microsoft.CodeAnalysis.CSharp.Desktop.dll",
                @"tools\Microsoft.CodeAnalysis.CSharp.dll",
                @"tools\Microsoft.CodeAnalysis.Desktop.dll",
                @"tools\Microsoft.CodeAnalysis.dll",
            };
            var files = package.GetFiles().Select(f => f.Path);
            Assert.Equal(expectedFileNames, files);
        }

        [Fact]
        public async Task Dependency_IndirectDependencies_AreNotPackaged()
        {
            var package = await Scenario.RestoreAndBuildSinglePackage("Dependency_IndirectDependencies_AreNotPackaged", "A");
            var files = package.GetFiles();

            Assert.None(files, x => x.Path.Contains("Newtonsoft.Json.dll"));
            Assert.None(files, x => x.Path.Contains("ServiceModel.Composition.dll"));
            Assert.None(files, x => x.Path.Contains("B3.dll"));
        }

        [Fact]
        public async Task Dependency_DirectDependencies_AreNotPackaged()
        {
            var package = await Scenario.RestoreAndBuildSinglePackage("Dependency_DirectDependencies_AreNotPackaged", "A");
            var files = package.GetFiles();

            Assert.None(files, x => x.Path.Contains("Newtonsoft.Json.dll"));
            Assert.None(files, x => x.Path.Contains("ServiceModel.Composition.dll"));
            Assert.None(files, x => x.Path.Contains("B3.dll"));
        }

        [Fact]
        public async Task Dependency_Versions_AreAggregated()
        {
            var package = await Scenario.RestoreAndBuildSinglePackage("Dependency_Versions_AreAggregated");
            var expectedVersions = new[]
            {
                    "1.1.20-beta",
                    "1.0.12-alpha",
                    "[0.2, 1.0]",
                    "[0.2, 1.0]",
            };
            var versionSpecs = package.DependencySets
                .SelectMany(x => x.Dependencies)
                .Select(x => x.VersionSpec.ToString());
            Assert.Equal(expectedVersions, versionSpecs);
        }

        [Fact]
        public async Task Dependency_MultipleFrameworks_AreResolved()
        {
            var package = await Scenario.RestoreAndBuildSinglePackage(
                "Dependency_MultipleFrameworks_AreResolved", 
                "Dependent");
            var dependencySet = new []{
                new PackageDependencySet(VersionUtility.ParseFrameworkName("net40"), new List<PackageDependency>
                    {
                        new PackageDependency("Dependency", new VersionSpec
                            {
                                IsMinInclusive = true,
                                MinVersion = new SemanticVersion("1.0.0")
                            })
                    }),
                new PackageDependencySet(VersionUtility.ParseFrameworkName("net45"), new List<PackageDependency>
                    {
                        new PackageDependency("Dependency", new VersionSpec
                            {
                                IsMinInclusive = true,
                                MinVersion = new SemanticVersion("1.0.0")
                            })
                    }),
            };

            Assert.Equal(dependencySet, package.DependencySets, PackageDependencySetComparer.Instance);
        }

        [Theory]
        [InlineData("Build")]
        [InlineData("Clean")]
        [InlineData("Rebuild")]
        public async Task Dependency_IsBuilt_WhenNotBuildingInsideVisualStudio(string target)
        {
            var projectPath = Assets.GetScenarioFilePath(
                "Dependency_IsBuilt_WhenNotBuildingInsideVisualStudio",
                @"NuGetPackage\NuGetPackage.nuproj");
            var properties = MSBuild.Properties.Default;
            var result = await MSBuild.ExecuteAsync(projectPath, target, properties);
            result.AssertSuccessfulBuild();

            var warnings = result.WarningEvents.ToArray();

            if (target == "Rebuild")
            {
                Assert.Equal(2, warnings.Length);
                Assert.Equal("Dependency Target Called: Clean", warnings[0].Message);
                Assert.Equal("Dependency Target Called: Build", warnings[1].Message);
            }
            else
            {
                Assert.Equal(1, warnings.Length);
                Assert.Equal("Dependency Target Called: " + target, warnings[0].Message);
            }
        }

        [Theory]
        [InlineData("Build")]
        [InlineData("Clean")]
        [InlineData("Rebuild")]
        public async Task Dependency_IsNotBuilt_WhenBuildingInsideVisualStudio(string target)
        {
            var projectPath = Assets.GetScenarioFilePath(
                "Dependency_IsNotBuilt_WhenBuildingInsideVisualStudio",
                @"NuGetPackage\NuGetPackage.nuproj");
            var properties = MSBuild.Properties.Default.AddRange(MSBuild.Properties.BuildingInsideVisualStudio);
            var result = await MSBuild.ExecuteAsync(projectPath, target, properties);
            result.AssertSuccessfulBuild();
        }
    }
}
