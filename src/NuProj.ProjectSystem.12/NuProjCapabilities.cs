using System;

#if Dev12
using Microsoft.Collections.Immutable;
#else
using System.Collections.Immutable;
#endif
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace NuProj.ProjectSystem
{
    internal static class NuProjCapabilities
    {
        public const string NuProj = "NuProj";

        public static readonly ImmutableHashSet<string> ProjectSystem = Empty.CapabilitiesSet.Union(new[]
        {
            NuProj,
#if !Dev12
            ProjectCapabilities.ProjectConfigurationsDeclaredAsItems,
#endif
            ProjectCapabilities.ReferencesFolder,
        });
    }
}
