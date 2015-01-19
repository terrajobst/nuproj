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
    public class RevaluatePropertiesTests
    {
        public async Task Tasks_RevaluateProperties()
        {
            var solutionPath = Assets.GetScenarioSolutionPath("Task_RevaluateProperties");
            var projectPath = Path.GetDirectoryName(solutionPath);



        }
    }
}
