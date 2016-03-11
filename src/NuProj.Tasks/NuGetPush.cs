using System;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NuProj.Tasks
{
    using System.ComponentModel;

    public class NuGetPush : ToolTask
    {
        [Required]
        public string PackagePath { get; set; }
        
        public string Source { get; set; }

        public string ApiKey { get; set; }

        public bool Success { get; set; } = false;

        protected override string ToolName
        {
            get { return "NuGet -push"; }
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(Source))
            {
                Log.LogWarning("Skipping publishing: No Source Repository specified");
                return false;
            }

            return base.Execute();
        }
        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolExe);
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            builder.AppendSwitch("push");
            builder.AppendFileNameIfNotNull(PackagePath);
            builder.AppendSwitchIfNotNull("-Source ", Source);
            builder.AppendSwitchIfNotNull("-ApiKey ", ApiKey);

            return builder.ToString();
        }

        protected override MessageImportance StandardErrorLoggingImportance
        {
            get { return MessageImportance.High; }
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (singleLine.Contains("Your package was pushed."))
                Success = true;
            if (messageImportance == MessageImportance.High)
            {
                Log.LogError(singleLine);
                return;
            }

            if (singleLine.StartsWith("Issue:") || singleLine.StartsWith("Description:") || singleLine.StartsWith("Solution:"))
            {
                Log.LogWarning(singleLine);
                return;
            }

            base.LogEventsFromTextOutput(singleLine, messageImportance);
        }
    }
}