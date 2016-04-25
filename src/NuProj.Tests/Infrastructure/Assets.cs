using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

using Microsoft.Build.Construction;

namespace NuProj.Tests.Infrastructure
{
    public static class Assets
    {
        private static readonly string ProjectDirectory = ComputeProjectDirectory();

        public static ProjectRootElement FromTemplate()
        {
            return GetResourceProjectRootElement("FromTemplate.nuproj");
        }

        public static string NuProjPath
        {
            get { return Path.Combine(ProjectDirectory, @"src\NuProj.Targets"); }
        }

        public static string NuProjTasksPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"NuProj.Tasks.dll"); }
        }

        public static string NuGetToolPath
        {
            get { return Path.Combine(ProjectDirectory, @"src\packages\NuGet.CommandLine.3.3.0\tools"); }
        }

        public static string NuGetExePath
        {
            get { return Path.Combine(NuGetToolPath, "nuget.exe"); }
        }

        public static string MicrosoftCommonNuProjTargetsPath
        {
            get { return Path.Combine(NuProjPath, @"Microsoft.Common.NuProj.targets"); }
        }

        public static string ScenariosDirectory
        {
            get { return Path.Combine(ProjectDirectory, "src", "NuProj.Tests", "Scenarios"); }
        }

        public static string GetScenarioDirectory([CallerMemberName] string scenarioName = null)
        {
            return Path.Combine(ScenariosDirectory, scenarioName);
        }

        public static string GetScenarioSolutionPath([CallerMemberName] string scenarioName = null)
        {
            var solutionDirectory = GetScenarioDirectory(scenarioName);
            return Path.Combine(solutionDirectory, scenarioName + ".sln");
        }

        public static string GetScenarioFilePath(string scenarioName, string filePath)
        {
            var solutionDirectory = GetScenarioDirectory(scenarioName);
            return Path.Combine(solutionDirectory, filePath);
        }

        private static string ComputeProjectDirectory()
        {
            // When running inside the IDE, the tests are shadow copied. In order to find the
            // original location, we can use AppDomain.CurrentDomain.BaseDirectory.

            var appDomainBase = AppDomain.CurrentDomain.BaseDirectory;

            // For IDE runs, the base directory will be something like
            //
            //      <ProjectDir>\src\NuProj.Tests\bin\Debug\
            //
            // When running from the automated build (command line or Visual Studio Online) we
            // run the tests from the output directory, which means the base directory will
            // look like this:
            //
            //      <ProjectDir>\bin\raw\
            //
            // This means we either have to go up 4 or 2 levels. In order to decide we simply
            // check whether the base directory is "raw"  -- which is a fixed part, even if
            // $(OutDir) is redirected.

            var isBuildOutput = string.Equals(Path.GetFileName(appDomainBase), "raw", StringComparison.OrdinalIgnoreCase);
            var parentPath = isBuildOutput ? @"..\.." : @"..\..\..\..";

            return Path.GetFullPath(Path.Combine(appDomainBase, parentPath));
        }

        private static ProjectRootElement GetResourceProjectRootElement(string name)
        {
            return ProjectRootElement.Create(GetResourceXmlReader(name));
        }

        private static XmlReader GetResourceXmlReader(string name)
        {
            return XmlReader.Create(GetResourceStream(name));
        }

        private static Stream GetResourceStream(string name)
        {
            var assembly = typeof(Assets).Assembly;
            var resourceNamespace = typeof(Assets).Namespace;
            var qualifiedResourceName = resourceNamespace + ".Assets." + name;
            return assembly.GetManifestResourceStream(qualifiedResourceName);
        }
    }
}
