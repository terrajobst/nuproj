using System.Threading.Tasks;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    using System.Collections.Generic;

    public class PublishPackageTests
    {
        [Fact]
        public async Task PublishPackage_LibraryWithXmlDoc_ShouldNotFail()
        {
            var solutionPath = Assets.GetScenarioSolutionPath("PublishPackage_LibraryWithXmlDoc_ShouldNotFail");
            var result = await MSBuild.RebuildAsync(solutionPath);            
            result.AssertSuccessfulBuild();
        } 
    }
}
