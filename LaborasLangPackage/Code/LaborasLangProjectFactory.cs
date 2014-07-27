using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using System;
using System.Runtime.InteropServices;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace LaborasLangPackage
{
    [Guid(GuidList.guidLaborasLangProjectFactoryString)]
    internal sealed class LaborasLangProjectFactory : ProjectFactory
    {
        public LaborasLangProjectFactory(LaborasLangPackagePackage package) :
            base(package)
        {
        }
        
        protected override ProjectNode CreateProject()
        {
            var project = new LaborasLangProjectNode(package);

            var site = (IOleServiceProvider)((IServiceProvider)package).GetService(typeof(IOleServiceProvider));
            project.SetSite(site);

            return project;
        }
    }
}
