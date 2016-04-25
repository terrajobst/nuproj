using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuProj.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace NuProj.Tests
{
    public class DotNetTests
    {
        private readonly ITestOutputHelper logger;

        public DotNetTests(ITestOutputHelper logger)
        {
            this.logger = logger;
        }

        [Fact]
        public async Task DotNet_CanPackage()
        {
             var package = await Scenario.RestoreAndBuildSinglePackageAsync(testLogger: logger);
        }
    }
}
