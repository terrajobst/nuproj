using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NuProj.Tests.Infrastructure
{
    public static class NuGetHelper
    {
        private static readonly object RestoreTasksLock = new object();
        private static readonly Dictionary<string, Task> RestorePackagesTasks = new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);

        public static Task RestorePackagesAsync(string path)
        {
            Task restorePackagesTask;
            lock (RestoreTasksLock)
            {
                if (!RestorePackagesTasks.TryGetValue(path, out restorePackagesTask))
                {
                    restorePackagesTask = NuGetExeRestoreAsync(path);
                    RestorePackagesTasks[path] = restorePackagesTask;
                }
            }

            return restorePackagesTask;
        }

        private static Task<int> NuGetExeRestoreAsync(string path)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Assets.NuGetExePath,
                Arguments = "restore",
                WorkingDirectory = path,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;

            var outputLines = new List<string>();

            process.ErrorDataReceived += (s, e) =>
            {
                lock (outputLines)
                    outputLines.Add(e.Data);
            };
            process.OutputDataReceived += (s, e) =>
            {
                lock (outputLines)
                    outputLines.Add(e.Data);
            };

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) =>
            {
                try
                {
                    if (process.ExitCode != 0)
                    {
                        var output = String.Join(Environment.NewLine, outputLines);
                        var message = String.Format("NuGet package restore failed.{0}{1}", Environment.NewLine, output);
                        tcs.SetException(new Exception(message));
                    }
                    else
                    {
                        tcs.SetResult(process.ExitCode);
                    }
                }
                finally
                {
                    process.Dispose();
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return tcs.Task;
        }
    }
}
