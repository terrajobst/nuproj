using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

using Xunit;
using Xunit.Abstractions;

namespace NuProj.Tests.Infrastructure
{
    public static class MSBuild
    {
        public static Task<BuildResultAndLogs> RebuildAsync(string projectPath, string projectName = null, IDictionary<string, string> properties = null, ITestOutputHelper testLogger = null)
        {
            var target = string.IsNullOrEmpty(projectName) ? "Rebuild" : projectName.Replace('.', '_') + ":Rebuild";
            return MSBuild.ExecuteAsync(projectPath, new[] { target }, properties, testLogger);
        }

        public static Task<BuildResultAndLogs> ExecuteAsync(string projectPath, string targetToBuild, IDictionary<string, string> properties = null, ITestOutputHelper testLogger = null)
        {
            return MSBuild.ExecuteAsync(projectPath, new[] { targetToBuild }, properties, testLogger);
        }

        /// <summary>
        /// Builds a project.
        /// </summary>
        /// <param name="projectPath">The absolute path to the project.</param>
        /// <param name="targetsToBuild">The targets to build. If not specified, the project's default target will be invoked.</param>
        /// <param name="properties">The optional global properties to pass to the project. May come from the <see cref="MSBuild.Properties"/> static class.</param>
        /// <returns>A task whose result is the result of the build.</returns>
        public static async Task<BuildResultAndLogs> ExecuteAsync(string projectPath, string[] targetsToBuild = null, IDictionary<string, string> properties = null, ITestOutputHelper testLogger = null)
        {
            targetsToBuild = targetsToBuild ?? new string[0];

            var logger = new EventLogger();
            var logLines = new List<string>();
            var parameters = new BuildParameters
            {
                Loggers = new List<ILogger>
                {
                    new ConsoleLogger(LoggerVerbosity.Detailed, logLines.Add, null, null),
                    new ConsoleLogger(LoggerVerbosity.Minimal, v => testLogger?.WriteLine(v.TrimEnd()), null, null),
                    logger,
                },
            };

            BuildResult result;
            using (var buildManager = new BuildManager())
            {
                buildManager.BeginBuild(parameters);
                try
                {
                    var requestData = new BuildRequestData(projectPath, properties ?? Properties.Default, null, targetsToBuild, null);
                    var submission = buildManager.PendBuildRequest(requestData);
                    result = await submission.ExecuteAsync();
                }
                finally
                {
                    buildManager.EndBuild();
                }
            }

            return new BuildResultAndLogs(result, logger.LogEvents, logLines);
        }

        /// <summary>
        /// Builds a project.
        /// </summary>
        /// <param name="projectInstance">The project to build.</param>
        /// <param name="targetsToBuild">The targets to build. If not specified, the project's default target will be invoked.</param>
        /// <returns>A task whose result is the result of the build.</returns>
        public static async Task<BuildResultAndLogs> ExecuteAsync(ProjectInstance projectInstance, ITestOutputHelper testLogger = null, params string[] targetsToBuild)
        {
            targetsToBuild = (targetsToBuild == null || targetsToBuild.Length == 0) ? projectInstance.DefaultTargets.ToArray() : targetsToBuild;

            var logger = new EventLogger();
            var logLines = new List<string>();
            var parameters = new BuildParameters
            {
                Loggers = new List<ILogger>
                {
                    new ConsoleLogger(LoggerVerbosity.Detailed, logLines.Add, null, null),
                    new ConsoleLogger(LoggerVerbosity.Minimal, v => testLogger?.WriteLine(v.TrimEnd()), null, null),
                    logger,
                },
            };

            BuildResult result;
            using (var buildManager = new BuildManager())
            {
                buildManager.BeginBuild(parameters);
                try
                {
                    var brdFlags = BuildRequestDataFlags.ProvideProjectStateAfterBuild;
                    var requestData = new BuildRequestData(projectInstance, targetsToBuild, null, brdFlags);
                    var submission = buildManager.PendBuildRequest(requestData);
                    result = await submission.ExecuteAsync();
                }
                finally
                {
                    buildManager.EndBuild();
                }
            }

            return new BuildResultAndLogs(result, logger.LogEvents, logLines);
        }

        private static Task<BuildResult> ExecuteAsync(this BuildSubmission submission)
        {
            var tcs = new TaskCompletionSource<BuildResult>();
            submission.ExecuteAsync(s => tcs.SetResult(s.BuildResult), null);
            return tcs.Task;
        }

        /// <summary>
        /// Common properties to pass to a build request.
        /// </summary>
        public static class Properties
        {
            /// <summary>
            /// No properties. The project will be built in its default configuration.
            /// </summary>
            private static readonly ImmutableDictionary<string, string> Empty = ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Gets the global properties to pass to indicate where NuProj imports can be found.
            /// </summary>
            /// <remarks>
            /// For purposes of true verifications, this map of global properties should
            /// NOT include any that are propagated by project references from NuProj
            /// or else their presence here (which does not reflect what the user's solution
            /// typically builds with) may mask over-build errors that would otherwise
            /// be caught by our BuildResultAndLogs.AssertNoTargetsExecutedTwice method.
            /// </remarks>
            public static readonly ImmutableDictionary<string, string> Default = Empty
                .Add("NuProjPath", Assets.NuProjPath)
                .Add("NuProjTasksPath", Assets.NuProjTasksPath)
                .Add("NuGetToolPath", Assets.NuGetToolPath)
                .Add("CustomAfterMicrosoftCommonTargets", Assets.MicrosoftCommonNuProjTargetsPath);

            /// <summary>
            /// The project will build in the same manner as if it were building inside Visual Studio.
            /// </summary>
            public static readonly ImmutableDictionary<string, string> BuildingInsideVisualStudio = Default
                .Add("BuildingInsideVisualStudio", "true");
        }

        public class BuildResultAndLogs
        {
            internal BuildResultAndLogs(BuildResult result, List<BuildEventArgs> events, IReadOnlyList<string> logLines)
            {
                Result = result;
                LogEvents = events;
                LogLines = logLines;
            }

            public BuildResult Result { get; private set; }

            public List<BuildEventArgs> LogEvents { get; private set; }

            public IEnumerable<BuildErrorEventArgs> ErrorEvents
            {
                get { return LogEvents.OfType<BuildErrorEventArgs>(); }
            }

            public IEnumerable<BuildWarningEventArgs> WarningEvents
            {
                get { return LogEvents.OfType<BuildWarningEventArgs>(); }
            }

            public IReadOnlyList<string> LogLines { get; private set; }

            public string EntireLog
            {
                get { return string.Join(string.Empty, LogLines); }
            }

            public void AssertSuccessfulBuild()
            {
                Assert.False(ErrorEvents.Any(), ErrorEvents.Select(e => e.Message).FirstOrDefault());
                this.AssertNoTargetsExecutedTwice();
                Assert.Equal(BuildResultCode.Success, Result.OverallResult);
            }

            public void AssertUnsuccessfulBuild()
            {
                Assert.Equal(BuildResultCode.Failure, Result.OverallResult);
                Assert.True(ErrorEvents.Any(), ErrorEvents.Select(e => e.Message).FirstOrDefault());
            }

            /// <summary>
            /// Verifies that we don't have multi-proc build bugs that may cause
            /// build failures as a result of projects building multiple times.
            /// </summary>
            private void AssertNoTargetsExecutedTwice()
            {
                var projectPathToId = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                var configurations = new Dictionary<long, ProjectStartedEventArgs>();
                foreach (var projectStarted in this.LogEvents.OfType<ProjectStartedEventArgs>())
                {
                    if (!configurations.ContainsKey(projectStarted.BuildEventContext.ProjectInstanceId))
                    {
                        configurations.Add(projectStarted.BuildEventContext.ProjectInstanceId, projectStarted);
                    }

                    long existingId;
                    if (projectPathToId.TryGetValue(projectStarted.ProjectFile, out existingId))
                    {
                        if (existingId != projectStarted.BuildEventContext.ProjectInstanceId)
                        {
                            var originalProjectStarted = configurations[existingId];
                            var originalRequestingProject = configurations[originalProjectStarted.ParentProjectBuildEventContext.ProjectInstanceId].ProjectFile;

                            var requestingProject = configurations[projectStarted.ParentProjectBuildEventContext.ProjectInstanceId].ProjectFile;

                            var globalPropertiesFirst = originalProjectStarted.GlobalProperties.Select(kv => $"{kv.Key}={kv.Value}").ToImmutableHashSet();
                            var globalPropertiesSecond = projectStarted.GlobalProperties.Select(kv => $"{kv.Key}={kv.Value}").ToImmutableHashSet();
                            var inFirstNotSecond = globalPropertiesFirst.Except(globalPropertiesSecond);
                            var inSecondNotFirst = globalPropertiesSecond.Except(globalPropertiesFirst);

                            var messageBuilder = new StringBuilder();
                            messageBuilder.AppendLine($@"Project ""{projectStarted.ProjectFile}"" was built twice. ");
                            messageBuilder.Append($@"The first build request came from ""{originalRequestingProject}""");
                            if (inFirstNotSecond.IsEmpty)
                            {
                                messageBuilder.AppendLine();
                            }
                            else
                            {
                                messageBuilder.AppendLine($" and defined these unique global properties: {string.Join(",", inFirstNotSecond)}");
                            }

                            messageBuilder.Append($@"The subsequent build request came from ""{requestingProject}""");
                            if (inSecondNotFirst.IsEmpty)
                            {
                                messageBuilder.AppendLine();
                            }
                            else
                            {
                                messageBuilder.AppendLine($" and defined these unique global properties: {string.Join(",", inSecondNotFirst)}");
                            }

                            Assert.False(true, messageBuilder.ToString());
                        }
                    }
                    else
                    {
                        projectPathToId.Add(projectStarted.ProjectFile, projectStarted.BuildEventContext.ProjectInstanceId);
                    }
                }
            }

            private static string SerializeProperties(IDictionary<string, string> properties)
            {
                return string.Join(",", properties.Select(kv => $"{kv.Key}={kv.Value}"));
            }
        }

        private class EventLogger : ILogger
        {
            private IEventSource _eventSource;

            internal EventLogger()
            {
                Verbosity = LoggerVerbosity.Normal;
                LogEvents = new List<BuildEventArgs>();
            }

            public LoggerVerbosity Verbosity { get; set; }

            public string Parameters { get; set; }

            public List<BuildEventArgs> LogEvents { get; set; }

            public void Initialize(IEventSource eventSource)
            {
                _eventSource = eventSource;
                _eventSource.AnyEventRaised += EventSourceAnyEventRaised;
            }

            private void EventSourceAnyEventRaised(object sender, BuildEventArgs e)
            {
                LogEvents.Add(e);
            }

            public void Shutdown()
            {
                _eventSource.AnyEventRaised -= EventSourceAnyEventRaised;
            }
        }
    }
}
