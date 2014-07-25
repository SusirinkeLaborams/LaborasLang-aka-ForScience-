using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace LaborasLangPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideEditorExtension(typeof(EditorFactory), ".ll", 50, 
              ProjectGuid = "{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3}", 
              TemplateDir = "Templates", 
              NameResourceID = 105,
              DefaultName = "LaborasLangPackage")]
    [ProvideService(typeof(LaborasLangService),ServiceName = "LaborasLang Service")]
    [ProvideLanguageService(typeof(LaborasLangService), LaborasLangConstants.LanguageName, 106,
                         CodeSense = false,              // Supports IntelliSense (not yet)
                         RequestStockColors = true,
                         EnableCommenting = true,        // Supports commenting out code
                         EnableAsyncCompletion = false   // Supports background parsing (not yet)
                         )]
    [ProvideLanguageExtension(typeof(LaborasLangService), ".ll")]
    [ProvideKeyBindingTable(GuidList.guidLaborasLangPackageEditorFactoryString, 102)]
    [ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.Code_string)]
    [ProvideEditorLogicalView(typeof(EditorFactory), VSConstants.LOGVIEWID.Debugging_string)]
    [Guid(GuidList.guidLaborasLangPackagePkgString)]
    public sealed class LaborasLangPackagePackage : Package, IOleComponent
    {
        private uint m_ComponentId;
        private LaborasLangService m_LanguageService;

        public LaborasLangPackagePackage()
        {
        }
        
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var serviceContainer = (IServiceContainer)this;
            m_LanguageService = new LaborasLangService();
            m_LanguageService.SetSite(this);
            serviceContainer.AddService(typeof(LaborasLangService), m_LanguageService, true);

            var componentManager = (IOleComponentManager)GetService(typeof(SOleComponentManager));

            OLECRINFO[] crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)(_OLECRF.olecrfNeedIdleTime | _OLECRF.olecrfNeedPeriodicIdleTime);
            crinfo[0].grfcadvf = (uint)(_OLECADVF.olecadvfModal | _OLECADVF.olecadvfRedrawOff | _OLECADVF.olecadvfWarningsOff);
            crinfo[0].uIdleTimeInterval = 0;

            int hr = componentManager.FRegisterComponent(this, crinfo, out m_ComponentId);
            ErrorHandler.ThrowOnFailure(hr);

            // Create Editor Factory. Note that the base Package class will call Dispose on it.
            base.RegisterEditorFactory(new EditorFactory(this));
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (m_ComponentId != 0)
                {
                    var componentManager = (IOleComponentManager)GetService(typeof(SOleComponentManager));

                    if (componentManager != null)
                    {
                        ErrorHandler.ThrowOnFailure(componentManager.FRevokeComponent(m_ComponentId));
                    }

                    m_ComponentId = 0;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #region IOleComponent implementation

        public int FContinueMessageLoop(uint loopReason, IntPtr loopData, MSG[] peekedMessage)
        {
            return 1;
        }

        public int FDoIdle(uint idleTaskFlags)
        {
            bool isPeriodic = (idleTaskFlags & (uint)_OLEIDLEF.oleidlefPeriodic) != 0;
            m_LanguageService.OnIdle(isPeriodic);
            return 0;
        }

        public int FPreTranslateMessage(MSG[] message)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }

        #endregion
    }
}
