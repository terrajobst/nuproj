using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;
using Microsoft.Build.Utilities;
using NuProj.Tasks.Utilities;

namespace NuProj.Tasks
{
    public class ExpandAssemblyProperties : Task
    {
        private const BindingFlags GetProjectInstanceBindingFlags =
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy;

        [Required]
        public ITaskItem[] Libraries { get; set; }

        [Required]
        public ITaskItem[] ExpandableProperties { get; set; }

        [Output]
        public ITaskItem[] ExpandedProperties { get; set; }

        public override bool Execute()
        {
            try
            {
                // build a dictionary of built libraries, where the value for each built library is a dictionary
                // of assembly property names and values. 

                var knownAssemblies = GetAssemblyProperties(Libraries);

                // enumerate through each provided property item, expanding it into a new item, returning
                // the item in the expandedProperties list..

                List<ITaskItem> expandedProperties = new List<ITaskItem>();

                foreach (var expandableProperty in ExpandableProperties)
                    expandedProperties.Add(ExpandPropertyItem(new TaskItem(expandableProperty), knownAssemblies));

                // return the expanded properties..
                ExpandedProperties = expandedProperties.ToArray();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }

        private Dictionary<string, List<Dictionary<string, string>>> GetAssemblyProperties(ITaskItem[] assemblies)
        {
            // keys are project names and are case insensitive..
            var knownAssemblies = new Dictionary<string, List<Dictionary<string, string>>>(StringComparer.OrdinalIgnoreCase);

            foreach (var propertyItem in assemblies)
            {
                Dictionary<string, string> assemblyProperties;

                try
                {
                    // get the dictionary of properties associated with the assembly..
                    assemblyProperties = AssemblyMetadataExtractor.ExtractMetadata(propertyItem.ItemSpec);
                }
                catch (Exception ex)
                {
                    // treat failures to read the assembly's metadata as warnings..
                    Log.LogWarningFromException(ex);
                    continue;
                }

                // Each assembly is added to the known assemblies multiple times. This should allow projects with the
                // same name to coexist, while still letting the user just use the project name in the usual case.
                //
                //   1. Project name without extension as key
                //   2. Project name with extension as key
                //   3. Project name without extension and target framework moniker as key.
                //   4. Project name with extension and target framework moniker as key.

                var projectName = Path.GetFileNameWithoutExtension(propertyItem.GetMetadata(Metadata.SourceProjectFile));
                var projectNameWithExtension = Path.GetFileName(propertyItem.GetMetadata(Metadata.SourceProjectFile));

                AddAssemblyProperties(knownAssemblies, projectName, assemblyProperties);
                AddAssemblyProperties(knownAssemblies, projectNameWithExtension, assemblyProperties);
                AddAssemblyProperties(knownAssemblies, projectName + "." + propertyItem.GetMetadata(Metadata.TargetFramework), assemblyProperties);
                AddAssemblyProperties(knownAssemblies, projectNameWithExtension + "." + propertyItem.GetMetadata(Metadata.TargetFramework), assemblyProperties);

                foreach (var k in knownAssemblies)
                    Log.LogMessage(MessageImportance.High, "{0}", k.Key);
            }

            return knownAssemblies;
        }

        private ITaskItem ExpandPropertyItem(ITaskItem expandedProperty, Dictionary<string, List<Dictionary<string, string>>> assemblyProperties)
        {
            StringBuilder expandedPropertyValue = new StringBuilder();

            foreach (var expansionToken in new ExpansionTokenizer(expandedProperty.GetMetadata(Metadata.PropertyValue)))
            {
                switch (expansionToken.Type)
                {
                    case ExpansionTokenType.Text:
                        expandedPropertyValue.Append(expansionToken.Value);
                        break;

                    case ExpansionTokenType.Variable:
                        expandedPropertyValue.Append(GetAssemblyProperty(assemblyProperties, expansionToken.Value));
                        break;
                }
            }

            expandedProperty.SetMetadata(Metadata.PropertyValue, expandedPropertyValue.ToString());
            return expandedProperty;
        }

        private string GetAssemblyProperty(Dictionary<string, List<Dictionary<string, string>>> knownAssemblies, string variableKey)
        {
            int separatorPos = 0;
            string ambiguousKey = null;

            while (true)
            {
                separatorPos = variableKey.IndexOf('_', separatorPos);
                if (separatorPos < 0)
                    break;

                string projectKey = variableKey.Substring(0, separatorPos);
                string propertyKey = variableKey.Substring(separatorPos + 1, variableKey.Length - separatorPos - 1);

                if ((projectKey.Length > 0) && (propertyKey.Length > 0))
                {
                    List<Dictionary<string, string>> assemblyProperties;

                    if (knownAssemblies.TryGetValue(projectKey, out assemblyProperties))
                    {
                        if (assemblyProperties.Count == 1)
                        {
                            string propertyValue;

                            // lookup the property value..
                            if (assemblyProperties[0].TryGetValue(propertyKey, out propertyValue))
                                return propertyValue;

                            // if we reach this point, the property value is not defined..
                            ambiguousKey = null;
                            break;
                        }

                        // if we reach this point, the variable is (possibly) ambiguous..
                        ambiguousKey = projectKey;
                    }
                }

                separatorPos++;
            }

            if (ambiguousKey != null)
                Log.LogError("The replacement token '{0}' is ambiguous.", variableKey);
            else
                // property is not defined, treat as warning..
                Log.LogWarning("The replacement token '{0}' does not match a defined assembly property.", variableKey);

            return String.Empty;
        }

        private void AddAssemblyProperties(Dictionary<string, List<Dictionary<string, string>>> knownAssemblies, string assemblyKey, Dictionary<string, string> assemblyProperties)
        {
            List<Dictionary<string, string>> propertyList;

            if (!knownAssemblies.TryGetValue(assemblyKey, out propertyList))
                knownAssemblies.Add(assemblyKey, propertyList = new List<Dictionary<string, string>>());

            propertyList.Add(assemblyProperties);
        }
    }
}
