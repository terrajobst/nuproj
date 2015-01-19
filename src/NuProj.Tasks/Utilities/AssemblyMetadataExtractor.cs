using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NuProj.Tasks.Utilities
{
    // AssemblyMetadataExtractor borrowed from NuGet.Core, with minor modifications..
    public static class AssemblyMetadataExtractor
    {
        public static Dictionary<string, string> ExtractMetadata(string assemblyPath)
        {
            AppDomainSetup domainSetup = new AppDomainSetup()
            {
                ApplicationBase = Path.GetDirectoryName(typeof(MetadataExtractor).Assembly.Location) 
            };

            AppDomain appDomain = null;
            try
            {
                appDomain = AppDomain.CreateDomain("metadata", AppDomain.CurrentDomain.Evidence, domainSetup);
                return appDomain.CreateInstance<MetadataExtractor>().ExtractMetadata(assemblyPath);
            }
            finally
            {
                if (appDomain != null)
                    AppDomain.Unload(appDomain);
            }
        }

        private static T CreateInstance<T>(this AppDomain domain)
        {
            return (T)domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        private static void ConditionalReplace<K, V>(this IDictionary<K, V> dictionary, K key, V value)
        {
            if (value != null)
                dictionary[key] = value;
        }

        private sealed class MetadataExtractor : MarshalByRefObject
        {
            private static string[] VersionPartNames = new string[]
            {
                "Major",
                "Minor",
                "Build",
                "Revision"
            };

            public Dictionary<string, string> ExtractMetadata(string assemblyPath)
            {
                AssemblyResolver assemblyResolver = new AssemblyResolver(assemblyPath);

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += assemblyResolver.ReflectionOnlyAssemblyResolve;
                try
                {
                    Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                    AssemblyName assemblyName = assembly.GetName();

                    var customAttributes = CustomAttributeData.GetCustomAttributes(assembly);
                    var propertiesDictionary = GetProperties(customAttributes);

                    propertiesDictionary.ConditionalReplace("AssemblyName", assemblyName.Name);
                    propertiesDictionary.ConditionalReplace("AssemblyFileVersion", GetAttributeValueOrDefault<AssemblyFileVersionAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyInformationalVersion", GetAttributeValueOrDefault<AssemblyInformationalVersionAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyTitle", GetAttributeValueOrDefault<AssemblyTitleAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyCompany", GetAttributeValueOrDefault<AssemblyCompanyAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyDescription", GetAttributeValueOrDefault<AssemblyDescriptionAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyCopyright", GetAttributeValueOrDefault<AssemblyCopyrightAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyCulture", GetAttributeValueOrDefault<AssemblyCultureAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyConfiguration", GetAttributeValueOrDefault<AssemblyConfigurationAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyTrademark", GetAttributeValueOrDefault<AssemblyTrademarkAttribute>(customAttributes));
                    propertiesDictionary.ConditionalReplace("AssemblyProduct", GetAttributeValueOrDefault<AssemblyProductAttribute>(customAttributes));

                    propertiesDictionary.ConditionalReplace("AssemblyVersion", assemblyName.Version.ToString());

                    // break AssemblyVersion into Major, Minor, Build and Release properties..
                    foreach (var versionPart in ExpandVersionAttribute("AssemblyVersion", assemblyName.Version.ToString()))
                        propertiesDictionary.ConditionalReplace(versionPart.Key, versionPart.Value);

                    // break AssemblyFileVersion into Major, Minor, Build and Release properties..
                    foreach (var versionPart in ExpandVersionAttribute("AssemblyFileVersion", GetAttributeValueOrDefault<AssemblyFileVersionAttribute>(customAttributes)))
                        propertiesDictionary.ConditionalReplace(versionPart.Key, versionPart.Value);

                    return propertiesDictionary;
                }
                finally
                {
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= assemblyResolver.ReflectionOnlyAssemblyResolve;
                }
            }

            private static IEnumerable<KeyValuePair<string, string>> ExpandVersionAttribute(string baseKey, string attributeValue)
            {
                if (!String.IsNullOrEmpty(attributeValue))
                {
                    // split the version based on periods, only accepting the first four substrings..
                    string[] versionParts = attributeValue.Split(new char[] { '.' }, 5);

                    for (int partIndex = 0; partIndex < Math.Min(versionParts.Length, 4); partIndex++)
                        yield return new KeyValuePair<string, string>(baseKey + "_" + VersionPartNames[partIndex], versionParts[partIndex].Trim());
                }
            }

            private static string GetAttributeValueOrDefault<T>(IList<CustomAttributeData> attributes) where T : Attribute
            {
                foreach (var attribute in attributes)
                {
                    if (attribute.Constructor.DeclaringType == typeof(T))
                    {
                        string value = attribute.ConstructorArguments[0].Value.ToString();
                        // Return the value only if it isn't null or empty so that we can use ?? to fall back
                        if (!String.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                }
                return null;
            }

            private static Dictionary<string, string> GetProperties(IList<CustomAttributeData> attributes)
            {
                var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                properties.Add("One", "ValueOne");

                var attributeName = typeof(AssemblyMetadataAttribute).FullName;
                foreach (var attribute in attributes.Where(x =>
                    x.Constructor.DeclaringType.FullName == attributeName &&
                    x.ConstructorArguments.Count == 2))
                {
                    string key = attribute.ConstructorArguments[0].Value.ToString();
                    string value = attribute.ConstructorArguments[1].Value.ToString();
                    // Return the value only if it isn't null or empty so that we can use ?? to fall back
                    if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
                    {
                        properties[key] = value;
                    }
                }

                return properties;
            }

            private class AssemblyResolver
            {
                private readonly string _lookupPath;

                public AssemblyResolver(string assemblyPath)
                {
                    _lookupPath = Path.GetDirectoryName(assemblyPath);
                }

                public Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs e)
                {
                    var assemblyName = new AssemblyName(AppDomain.CurrentDomain.ApplyPolicy(e.Name));
                    var assemblyPath = Path.Combine(_lookupPath, assemblyName.Name + ".dll");

                    return File.Exists(assemblyPath) ? Assembly.ReflectionOnlyLoadFrom(assemblyPath) : Assembly.ReflectionOnlyLoad(assemblyName.FullName);
                }
            }
        }
    }
}
