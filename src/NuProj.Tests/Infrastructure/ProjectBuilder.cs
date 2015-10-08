using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace NuProj.Tests.Infrastructure
{
    public static class ProjectBuilder
    {
        public static ProjectRootElement AssignNuProjDirectory(this ProjectRootElement nuProj)
        {
            var tempPath = Path.GetTempPath();
            var randomFileName = Path.GetRandomFileName();
            var projectDirectory = Path.Combine(tempPath, randomFileName);
            Directory.CreateDirectory(projectDirectory);
            nuProj.FullPath = Path.Combine(projectDirectory, "test.nuproj");
            return nuProj;
        }

        public static Project ToProject(this ProjectRootElement nuProj)
        {
            var project = new Project(
                nuProj,
                MSBuild.Properties.Default,
                toolsVersion: null);
            return project;
        }

        public static Project CreateMockContentFiles(this Project nuProj)
        {
            var random = new Random();
            foreach (var item in nuProj.GetItems("Content"))
            {
                var randomData = new byte[10];
                random.NextBytes(randomData);
                var randataDataBase64 = Convert.ToBase64String(randomData);
                var filePath = item.GetMetadataValue("FullPath");
                File.WriteAllText(filePath, randataDataBase64);
            }

            return nuProj;
        }

        public static void Cleanup(Project nuProj)
        {
            var projectDirectory = Path.GetDirectoryName(nuProj.FullPath);
            Directory.Delete(projectDirectory, recursive: true);
        }

        public static async Task<string> GetNuPkgPathAsync(this Project nuProj)
        {
            var result = await MSBuild.ExecuteAsync(nuProj.CreateProjectInstance(), "EstablishNuGetPaths");
            AssertNu.SuccessfulBuild(result);
            return result.Result.ProjectStateAfterBuild.GetPropertyValue("NuGetOutputPath");
        }

        public static async Task<string> GetNuSpecPathAsync(this Project nuProj)
        {
            var result = await MSBuild.ExecuteAsync(nuProj.CreateProjectInstance(), "EstablishNuGetPaths");
            AssertNu.SuccessfulBuild(result);
            return result.Result.ProjectStateAfterBuild.GetPropertyValue("NuSpecPath");
        }
    }
}
