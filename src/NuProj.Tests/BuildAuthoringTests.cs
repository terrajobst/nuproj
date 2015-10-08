using System;
using System.Threading.Tasks;
using NuProj.Tests.Infrastructure;
using Xunit;

namespace NuProj.Tests
{
    public class BuildAuthoringTests
    {
        [Fact]
        public async Task BuildAuthoring_Version_IsDeterminedAtBuildTime()
        {
            var package = await Scenario.RestoreAndBuildSinglePackageAsync();
            Assert.Equal(new Version(2, 0, 3, 0), package.Version.Version);
        }
    }
}
