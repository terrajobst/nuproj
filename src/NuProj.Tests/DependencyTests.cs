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
        public async Task Dependency_NoDependencies_Fails()
        {
            var solutionPath = Assets.GetScenarioSolutionPath("Dependency_NoDependencies_Fails");
            var result = await MSBuild.RebuildAsync(solutionPath);
            var error = result.ErrorEvents.Single();

            var expectedMessage = "Cannot create a package that has no dependencies nor content.";
            var actualMessage = error.Message;

            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task Dependency_Content_IsNotFiltered()
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync();
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
            var package = await Scenario.RestoreAndBuildSinglePackageAsync();
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
            var package = await Scenario.RestoreAndBuildSinglePackageAsync(packageId: "A.nuget");
            var files = package.GetFiles();

            Assert.DoesNotContain(files, x => x.Path.Contains("Newtonsoft.Json.dll"));
            Assert.DoesNotContain(files, x => x.Path.Contains("ServiceModel.Composition.dll"));
            Assert.DoesNotContain(files, x => x.Path.Contains("B3.dll"));
        }

        [Fact]
        public async Task Dependency_DirectDependencies_AreNotPackaged()
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync(packageId: "A.nuget");
            var files = package.GetFiles();

            Assert.DoesNotContain(files, x => x.Path.Contains("Newtonsoft.Json.dll"));
            Assert.DoesNotContain(files, x => x.Path.Contains("ServiceModel.Composition.dll"));
            Assert.DoesNotContain(files, x => x.Path.Contains("B3.dll"));
        }

        [Fact]
        public async Task Dependency_Versions_AreAggregated()
        {
            var expectedDepndencies = new[] 
            {
                "Microsoft.Bcl.Immutable (>= 1.1.20-beta) (net45)",
                "Microsoft.Bcl.Metadata (>= 1.0.12-alpha) (net45)",
                "Microsoft.CodeAnalysis.Common (>= 0.2 && <= 1.0) (net45)",
                "Microsoft.CodeAnalysis.CSharp (>= 0.2 && <= 1.0) (net45)",
            };

            var package = await Scenario.RestoreAndBuildSinglePackageAsync();
            var actualDependencies = package.DependencySets.Flatten();

            Assert.Equal(expectedDepndencies, actualDependencies);
        }

        [Fact]
        public async Task Dependency_MultipleFrameworks_AreResolved()
        {
            var expectedDepndencies = new[]
            {
                "Dependency.nuget (>= 1.0.0) (net40)",
                "Dependency.nuget (>= 1.0.0) (net45)",
            };

            var package = await Scenario.RestoreAndBuildSinglePackageAsync(packageId: "Dependent.nuget");
            var actualDependencies = package.DependencySets.Flatten();

            Assert.Equal(expectedDepndencies, actualDependencies);
        }

        [Fact]
        public async Task Dependency_OmitDevelopmentDependencies()
        {
            var expectedDepndencies = new[] { "Microsoft.Tpl.Dataflow (>= 4.5.24) (net452)" };

            var package = await Scenario.RestoreAndBuildSinglePackageAsync();
            var actualDepedndencies = package.DependencySets.Flatten();

            Assert.Equal(expectedDepndencies, actualDepedndencies);
        }

        [Fact]
        public async Task Dependency_NuProjDependencyVersion()
        {
            var expectedDependencies = new[]
            {
                "DefaultVersion (>= 1.0.0)",
                "ExactVersion (= 1.0.0)",
                "MaxVersion", // as per docs, empty = latest version
                "RangeVersion (>= 1.0.0 && < 2.0)",
            };

            var package = await Scenario.RestoreAndBuildSinglePackageAsync(packageId: "Dependent");
            var actualDependencies = package.DependencySets.Flatten();

            Assert.Equal(expectedDependencies, actualDependencies);
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
                Assert.Equal(4, warnings.Length);
                Assert.Equal("CsProj dependency Target Called: Clean", warnings[0].Message);
                Assert.Equal("NuProj dependency Target Called: Clean", warnings[1].Message);
                Assert.Equal("CsProj dependency Target Called: Build", warnings[2].Message);
                Assert.Equal("NuProj dependency Target Called: Build", warnings[3].Message);
            }
            else
            {
                Assert.Equal(2, warnings.Length);
                Assert.Equal("CsProj dependency Target Called: " + target, warnings[0].Message);
                Assert.Equal("NuProj dependency Target Called: " + target, warnings[1].Message);
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
