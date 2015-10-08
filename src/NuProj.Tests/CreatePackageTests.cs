using System.Threading.Tasks;
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
    }
}
