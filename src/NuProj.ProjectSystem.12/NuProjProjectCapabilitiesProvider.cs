using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace NuProj.ProjectSystem
{
#if Dev12
    [Export(typeof(IProjectCapabilitiesProvider))]
#else
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IProjectCapabilitiesProvider))]
    [AppliesTo(ProjectCapabilities.AlwaysApplicable)]
#endif
    [SupportsFileExtension("." + NuProjPackage.ProjectExtension)]
    internal sealed class NuProjProjectCapabilitiesProvider : IProjectCapabilitiesProvider
    {
        /// <summary>
        /// Gets the capabilities that fit the project in context that this provider contributes.
        /// </summary>
        /// <value>A sequence, possibly empty but never null.</value>
        public Task<IEnumerable<string>> GetCapabilitiesAsync()
        {
            return Task.FromResult<IEnumerable<string>>(NuProjCapabilities.ProjectSystem);
        }
    }
}
