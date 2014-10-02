using System.Linq;

using NuGet;

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
            task.ProjectPath = projectPath;
            var result = task.Execute();

            var expectedVersionConstraint = VersionUtility.ParseVersionSpec("[1,2]").ToString();

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
            task.ProjectPath = projectPath;
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
            task.ProjectPath = projectPath;
            var result = task.Execute();

            Assert.True(result);
            Assert.Empty(task.PackageReferences);
        }
    }
}