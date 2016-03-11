using System;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NuProj.Tasks
{

    public class GenerateVersion : Task
    {

        [Required]
        public string AssemblyName { get; set; }

        [Output]
        public string TargetName
            => $"{AssemblyName}.{Version}";

        [Output]
        public string Version { get; set; }

        public ITaskItem[] Files { get; set; }

        public override bool Execute()
        {
            try
            {
                GetVersion();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }
        private void GetVersion()
        {
            if (Version != "$version$" && !string.IsNullOrEmpty(Version))
            {
                Log.LogMessageFromText($"Using explicitly provided version {Version}",
                                       MessageImportance.Low);
                return;
            }
            string mainVersion;
            var mainProject = GetAssemblyInfo(out mainVersion);
            if (string.IsNullOrWhiteSpace(mainVersion))
            {
                Log.LogError("Unable to automatically generate version: Ensure main project contains valid Properties/AssemblyInfo.cs file");
                return;
            }
            Version = mainVersion;
            Log.LogMessageFromText($"Generated version {mainVersion} from main project {mainProject}",
                                   MessageImportance.High);
        }

        private string GetAssemblyInfo(out string version)
        {
            foreach (var projectItem in Files)
            {
                var projectFileName = projectItem.ItemSpec;
                if (string.IsNullOrEmpty(projectFileName))
                    continue;
                var assemblyInfoFileName = Path.Combine(Path.GetDirectoryName(projectFileName), "Properties",
                                                        "AssemblyInfo.cs");
                if (!File.Exists(assemblyInfoFileName))
                    continue;

                var fileContents = File.ReadAllText(assemblyInfoFileName);
                var fileVersion = GetAssemblyAttribute(fileContents, "Version");
                if (string.IsNullOrWhiteSpace(fileVersion))
                    continue;
                version = fileVersion;
                return Path.GetFileNameWithoutExtension(projectFileName);
            }
            version = null;
            return null;
        }
        public string GetAssemblyAttribute(string text, string attribute)
        {
            if (text == null)
                return null;
            var start = "Assembly" + attribute + "(";
            text = text.Replace(start + " ", start).Replace("\" )]", "\")]");
            var value = text.GetTextBetween(start + "\"", "\")]");
            return string.IsNullOrEmpty(value) ? null : value.TrimStart(new char[] { ' ', '"' });
        }
    }
}