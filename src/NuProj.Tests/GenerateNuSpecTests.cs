using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.Build.Utilities;
using NuGet;
using NuProj.Tasks;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    public class GenerateNuSpecTests : IDisposable
    {
        private string _projectDirectory;

        public GenerateNuSpecTests()
        {
            var tempPath = Path.GetTempPath();
            var randomFileName = Path.GetRandomFileName();
            _projectDirectory = Path.Combine(tempPath, randomFileName);
        }

        public void Dispose()
        {
            if (Directory.Exists(_projectDirectory))
                Directory.Delete(_projectDirectory, true);
        }

        [Fact]
        public void Task_GenerateNuSpec_OverrideInputFileName()
        {
            var input = Assets.GetScenarioFilePath("Task_GenerateNuSpec", "NuGetPackage.nuspec");
            var output = Path.Combine(_projectDirectory, "OverrideInputFileName.nuspec");

            var target = new GenerateNuSpec();
            target.InputFileName = input;
            target.OutputFileName = output;
            target.Id = "NuGetPackage2";
            target.Version = "2.0.0";
            target.Title = "NuGetPackage2";
            target.Authors = "Pavol";
            target.RequireLicenseAcceptance = true;
            target.Description = "NuGetPackage2";
            target.ReleaseNotes = "Released! (Again)";
            target.Summary = "NuGetPackage2";
            target.Language = "sk-sk";
            target.ProjectUrl = "http://nuproj.net/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuproj.net/LICENSE/changes";
            target.Copyright = "Copyright © Pavol";
            target.Tags = "NuGetPackage2";
            target.DevelopmentDependency = false;


            var fileToAdd = Path.Combine(_projectDirectory, "SomeProject.dll");
            target.Files = new[] {
                new TaskItem(fileToAdd, new Dictionary<string, string>
                    {
                        {Metadata.FileSource, fileToAdd},
                        {Metadata.FileTarget, @"lib\net45"},
                    })
            };
            var result = target.Execute();

            target.OutputFileName = output;
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);
                Assert.Equal(target.Id, manifest.Metadata.Id);
                Assert.Equal(target.Version, manifest.Metadata.Version);
                Assert.Equal(target.Title, manifest.Metadata.Title);
                Assert.Equal(target.Authors, manifest.Metadata.Authors);
                Assert.Equal(target.RequireLicenseAcceptance, manifest.Metadata.RequireLicenseAcceptance);
                Assert.Equal(target.Description, manifest.Metadata.Description);
                Assert.Equal(target.ReleaseNotes, manifest.Metadata.ReleaseNotes);
                Assert.Equal(target.Summary, manifest.Metadata.Summary);
                Assert.Equal(target.Language, manifest.Metadata.Language);
                Assert.Equal(target.ProjectUrl, manifest.Metadata.ProjectUrl);
                Assert.Equal(target.IconUrl, manifest.Metadata.IconUrl);
                Assert.Equal(target.LicenseUrl, manifest.Metadata.LicenseUrl);
                Assert.Equal(target.Copyright, manifest.Metadata.Copyright);
                Assert.Equal(target.Tags, manifest.Metadata.Tags);
                Assert.Equal(true, manifest.Metadata.DevelopmentDependency);

                var expectedFiles = new[] {
                    new ManifestFile
                    {
                        Source = "Readme.txt",
                        Target = "",
                        Exclude = "",
                    },
                    new ManifestFile
                    {
                        Source = fileToAdd,
                        Target = @"lib\net45",
                        Exclude = "",
                    },
                };

                Assert.Equal(expectedFiles,
                    manifest.Files,
                    ManifestFileComparer.Instance);
            }
        }

        [Fact]
        public void Task_GenerateNuSpec_UseInputFileName()
        {
            var input = Assets.GetScenarioFilePath("Task_GenerateNuSpec", "NuGetPackage.nuspec");
            var output = Path.Combine(_projectDirectory, "UseInputFileName.nuspec");

            var target = new GenerateNuSpec();
            target.InputFileName = input;
            target.OutputFileName = output;
            var result = target.Execute();

            target.OutputFileName = output;
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);
                Assert.Equal("NuGetPackage", manifest.Metadata.Id);
                Assert.Equal("1.0.0", manifest.Metadata.Version);
                Assert.Equal("NuGetPackage", manifest.Metadata.Title);
                Assert.Equal("Immo", manifest.Metadata.Authors);
                Assert.Equal(false, manifest.Metadata.RequireLicenseAcceptance);
                Assert.Equal("NuGetPackage", manifest.Metadata.Description);
                Assert.Equal("Released!", manifest.Metadata.ReleaseNotes);
                Assert.Equal("NuGetPackage", manifest.Metadata.Summary);
                Assert.Equal("en-us", manifest.Metadata.Language);
                Assert.Equal("http://nuproj.net/", manifest.Metadata.ProjectUrl);
                Assert.Equal("http://placekitten.com/g/64/64", manifest.Metadata.IconUrl);
                Assert.Equal("http://nuproj.net/LICENSE/", manifest.Metadata.LicenseUrl);
                Assert.Equal("Copyright © Immo", manifest.Metadata.Copyright);
                Assert.Equal("NuGetPackage", manifest.Metadata.Tags);
                Assert.Equal(true, manifest.Metadata.DevelopmentDependency);

                var expectedFrameworkAssemblies = new[] {
                    new ManifestFrameworkAssembly()
                    {
                        AssemblyName = "Microsoft.Build.Framework"
                    }
                };

                var expectedDependencySets = new[] {
                    new ManifestDependencySet
                    {
                        Dependencies = new List<ManifestDependency>
                        {
                            new ManifestDependency
                            {
                                Id = "NuGet.Core",
                                Version = "2.8.2"
                            }
                        }
                    }
                };

                var expectedReferenceSets = new[] {
                    new ManifestReferenceSet
                    {
                        References = new List<ManifestReference>
                        {
                            new ManifestReference
                            {
                                File = "NuGet.Core.dll"
                            }
                        }
                    }
                };

                var expectedFiles = new[] {
                    new ManifestFile
                    {
                        Source = "Readme.txt",
                        Target = "",
                        Exclude = "",
                    }
                };

                Assert.Equal(expectedFrameworkAssemblies,
                    manifest.Metadata.FrameworkAssemblies,
                    ManifestFrameworkAssemblyComparer.Instance);

                Assert.Equal(expectedDependencySets,
                    manifest.Metadata.DependencySets,
                    ManifestDependencySetComparer.Instance);

                Assert.Equal(expectedReferenceSets,
                    manifest.Metadata.ReferenceSets,
                    ManifestReferenceSetComparer.Instance);

                Assert.Equal(expectedFiles,
                    manifest.Files,
                    ManifestFileComparer.Instance);
            }
        }

        [Theory]
        [InlineData("Task_GenerateNuSpec_NoRequiredParameters", @"NuGetPackage\NuGetPackage.nuproj")]
        public async System.Threading.Tasks.Task Task_GenerateNuSpec_NoRequiredParameters(string scenarioName, string projectToBuild)
        {
            var solutionDir = Assets.GetScenarioDirectory(scenarioName);
            var projectPath = Path.Combine(solutionDir, projectToBuild);

            var result = await MSBuild.ExecuteAsync(projectPath, "GenerateNuSpec");
            result.AssertError("The 'Id' property cannot be empty or undefined.");
        }

        [Theory]
        [InlineData("Task_GenerateNuSpec_NoRequiredParameters", @"NuGetPackage\NuGetPackage.nuproj")]
        public async System.Threading.Tasks.Task Task_GenerateNuSpec_NoRequiredParameters_IdOnly(string scenarioName, string projectToBuild)
        {
            var solutionDir = Assets.GetScenarioDirectory(scenarioName);
            var projectPath = Path.Combine(solutionDir, projectToBuild);

            ImmutableDictionary<string, string> properties = MSBuild.Properties.Default
                .Add("Id", "NoRequiredParameters");

            var result = await MSBuild.ExecuteAsync(projectPath, "GenerateNuSpec", properties);
            result.AssertError("The 'Version' property cannot be empty or undefined.");
        }

        [Theory]
        [InlineData("Task_GenerateNuSpec_NoRequiredParameters", @"NuGetPackage\NuGetPackage.nuproj")]
        public async System.Threading.Tasks.Task Task_GenerateNuSpec_NoRequiredParameters_IdAndVersion(string scenarioName, string projectToBuild)
        {
            var solutionDir = Assets.GetScenarioDirectory(scenarioName);
            var projectPath = Path.Combine(solutionDir, projectToBuild);

            ImmutableDictionary<string, string> properties = MSBuild.Properties.Default
                .Add("Id", "NoRequiredParameters")
                .Add("Version", "1.0.0");

            var result = await MSBuild.ExecuteAsync(projectPath, "GenerateNuSpec", properties);
            result.AssertError("required parameter \"Authors\"");
            result.AssertError("required parameter \"Description\"");
        }

        [Theory]
        [InlineData("Task_GenerateNuSpec_NoRequiredParameters", @"NuGetPackage\NuGetPackage.nuproj", @"NuGetPackage\template.nuspec")]
        public async System.Threading.Tasks.Task Task_GenerateNuSpec_NoRequiredParameters_FromTemplate(string scenarioName, string projectToBuild, string templateFile)
        {
            var solutionDir = Assets.GetScenarioDirectory(scenarioName);
            var projectPath = Path.Combine(solutionDir, projectToBuild);
            var templatePath = Path.Combine(solutionDir, templateFile);


            ImmutableDictionary<string, string> properties = MSBuild.Properties.Default
                .Add("NuSpecTemplate", templatePath);

            // load the template manifest so we can compare it later..
            Manifest templateManifest;
            using (var inputStream = File.OpenRead(templatePath))
                templateManifest = Manifest.ReadFrom(inputStream, false);

            var result = await MSBuild.ExecuteAsync(projectPath, "GenerateNuSpec", properties);
            result.AssertSuccessfulBuild();

            Manifest resultManifest;
            using (var inputStream = File.OpenRead(result.Result.ProjectStateAfterBuild.GetPropertyValue("NuSpecPath")))
                resultManifest = Manifest.ReadFrom(inputStream, false);

            Assert.Equal(templateManifest.Metadata.Id, resultManifest.Metadata.Id);
            Assert.Equal(templateManifest.Metadata.Version, resultManifest.Metadata.Version);
            Assert.Equal(templateManifest.Metadata.Authors, resultManifest.Metadata.Authors);
            Assert.Equal(templateManifest.Metadata.Description, resultManifest.Metadata.Description);
        }
    }
}