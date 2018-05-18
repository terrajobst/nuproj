using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;
using NuGet;
using NuGet.Versioning;
using NuProj.Tasks;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    public class ReadPackagesConfigTests
    {
        [Fact]
        public void Task_ReadPackagesConfig_ParseProjectoryConfig()
        {
            var projectPath = Assets.GetScenarioFilePath("Task_ReadPackagesConfig_ParseProjectoryConfig", @"A.csproj");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };
            var result = task.Execute();

            var expectedVersionConstraint = VersionRange.Parse("[1,2]").ToString();

            Assert.True(result);
            var fodyItem = new AssertTaskItem(task.PackageReferences, "Fody", items => Assert.Single(items)) {
                {"Version", "1.25.0"},
                {"TargetFramework", "net45"},
                {"VersionConstraint", expectedVersionConstraint},
                {"RequireReinstallation", bool.FalseString},
                {"IsDevelopmentDependency", bool.TrueString},
            };

            Assert.Single(fodyItem);
        }

        [Fact]
        public void Task_ReadPackagesConfig_ParseDirectoryConfig()
        {
            var projectPath = Assets.GetScenarioFilePath("Task_ReadPackagesConfig_ParseDirectoryConfig", @"B.csproj");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };
            var result = task.Execute();

            Assert.True(result);

            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.Bcl.Immutable"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.Bcl.Metadata"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.CodeAnalysis.Common"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.CodeAnalysis.CSharp"));
        }

        [Fact]
        public void Task_ReadPackagesConfig_NoConfig()
        {
            var projectPath = Assets.GetScenarioFilePath("Task_ReadPackagesConfig_NoConfig", @"C.csproj");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };
            var result = task.Execute();

            Assert.True(result);
            Assert.Empty(task.PackageReferences);
        }

        [Fact]
        public void Task_ReadPackagesConfig_NonStandardSolutionLocation()
        {
            var scenarioDirectory = Assets.GetScenarioDirectory("Task_ReadPackagesConfig_NonStandardSolutionLocation");
            var projectPath = Path.Combine(scenarioDirectory, @"projects\D\D.csproj");
            var solutionDir = Path.Combine(scenarioDirectory, "solution");
            var packagesDir = Path.Combine(solutionDir, "packages");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };
            task.SolutionDir = solutionDir;

            var result = task.Execute();

            Assert.True(result);
            Assert.NotEmpty(task.PackageReferences);

            var expectedItem = new AssertTaskItem(task.PackageReferences, "TestProj", items => Assert.Single(items)) {
                {"Version", "1.2.34"},
                {"TargetFramework", "net45"},
                {"PackageDirectoryPath", Path.Combine(packagesDir, "TestProj.1.2.34")},
            };

            Assert.Single(expectedItem);
        }

        [Fact]
        public void Task_ReadPackagesConfig_NonStandardSolutionLocation_NoSolutionContext()
        {
            var scenarioDirectory = Assets.GetScenarioDirectory("Task_ReadPackagesConfig_NonStandardSolutionLocation");
            var projectPath = Path.Combine(scenarioDirectory, @"projects\D\D.csproj");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };

            var result = task.Execute();

            Assert.True(result);
            Assert.NotEmpty(task.PackageReferences);

            var expectedItem = new AssertTaskItem(task.PackageReferences, "TestProj", items => Assert.Single(items)) {
                {"Version", "1.2.34"},
                {"TargetFramework", "net45"},
                {"PackageDirectoryPath", string.Empty},
            };

            Assert.Single(expectedItem);
        }
    }
}