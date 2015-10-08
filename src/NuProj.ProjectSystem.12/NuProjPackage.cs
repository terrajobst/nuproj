using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell;

namespace NuProj.ProjectSystem
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(PackageGuid)]
    internal sealed class NuProjPackage : Package
    {
        public const string PackageGuid = "267c66da-0c67-4933-9ae7-a20c53608e49";
        public const string ProjectTypeGuid = "ff286327-c783-4f7a-ab73-9bcbad0d4460";
        public const string ProjectExtension = "nuproj";
        public const string ProjectLanguage = "NuGet";

        public NuProjPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        }

        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));
            base.Initialize();
        }
    }
}
