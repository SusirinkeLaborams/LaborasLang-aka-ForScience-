using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace LaborasLangPackage.CoreExtension
{
    /// <summary>
    /// Factory for creating our editor object. Extends from the IVsEditoryFactory interface
    /// </summary>
    [Guid(GuidList.guidLaborasLangPackageEditorFactoryString)]
    public sealed class EditorFactory : IVsEditorFactory, IDisposable
    {
        private LaborasLangPackagePackage m_EditorPackage;
        private ServiceProvider m_VSServiceProvider;

        public EditorFactory(LaborasLangPackagePackage package)
        {
            this.m_EditorPackage = package;
        }

        public void Dispose()
        {
            if (m_VSServiceProvider != null)
            {
                m_VSServiceProvider.Dispose();
            }
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
        {
            m_VSServiceProvider = new ServiceProvider(serviceProvider);
            return VSConstants.S_OK;
        }

        public object GetService(Type serviceType)
        {
            return m_VSServiceProvider.GetService(serviceType);
        }

        // This method is called by the Environment (inside IVsUIShellOpenDocument::
        // OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a 
        // PHYSICAL view. A LOGICAL view identifies the purpose of the view that is
        // desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a 
        // view appropriate for text view manipulation as by navigating to a find
        // result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type 
        // of view implementation that an IVsEditorFactory can create. 
        //
        // NOTE: Physical views are identified by a string of your choice with the 
        // one constraint that the default/primary physical view for an editor  
        // *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
        //
        // NOTE: It is essential that the implementation of MapLogicalView properly
        // validates that the LogicalView desired is actually supported by the editor.
        // If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
        //
        // NOTE: The special Logical Views supported by an Editor Factory must also 
        // be registered in the local registry hive. LOGVIEWID_Primary is implicitly 
        // supported by all editor types and does not need to be registered.
        // For example, an editor that supports a ViewCode/ViewDesigner scenario
        // might register something like the following:
        //        HKLM\Software\Microsoft\VisualStudio\<version>\Editors\
        //            {...guidEditor...}\
        //                LogicalViews\
        //                    {...LOGVIEWID_TextView...} = s ''
        //                    {...LOGVIEWID_Code...} = s ''
        //                    {...LOGVIEWID_Debugging...} = s ''
        //                    {...LOGVIEWID_Designer...} = s 'Form'
        //
        public int MapLogicalView(ref Guid logicalView, out string physicalView)
        {
            physicalView = null;

            if (logicalView == VSConstants.LOGVIEWID_Primary ||
                logicalView == VSConstants.LOGVIEWID_Debugging ||
                logicalView == VSConstants.LOGVIEWID_Code ||
                logicalView == VSConstants.LOGVIEWID_TextView)
            {
                return VSConstants.S_OK;
            }

            return VSConstants.E_NOTIMPL;
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Used by the editor factory to create an editor instance. the environment first determines the 
        /// editor factory with the highest priority for opening the file and then calls 
        /// IVsEditorFactory.CreateEditorInstance. If the environment is unable to instantiate the document data 
        /// in that editor, it will find the editor with the next highest priority and attempt to so that same 
        /// thing. 
        /// NOTE: The priority of our editor is 32 as mentioned in the attributes on the package class.
        /// 
        /// Since our editor supports opening only a single view for an instance of the document data, if we 
        /// are requested to open document data that is already instantiated in another editor, or even our 
        /// editor, we return a value VS_E_INCOMPATIBLEDOCDATA.
        /// </summary>
        /// <param name="createEditorFlags">Flags determining when to create the editor. Only open and silent flags 
        /// are valid
        /// </param>
        /// <param name="documentMoniker">path to the file to be opened</param>
        /// <param name="physicalView">name of the physical view</param>
        /// <param name="hierarchy">pointer to the IVsHierarchy interface</param>
        /// <param name="itemid">Item identifier of this editor instance</param>
        /// <param name="docDataExisting">This parameter is used to determine if a document buffer 
        /// (DocData object) has already been created
        /// </param>
        /// <param name="docView">Pointer to the IUnknown interface for the DocView object</param>
        /// <param name="docData">Pointer to the IUnknown interface for the DocData object</param>
        /// <param name="editorCaption">Caption mentioned by the editor for the doc window</param>
        /// <param name="commandUIGuid">the Command UI Guid. Any UI element that is visible in the editor has 
        /// to use this GUID. This is specified in the .vsct file
        /// </param>
        /// <param name="createDocumentWindowFlags">Flags for CreateDocumentWindow</param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public int CreateEditorInstance(
                        uint createEditorFlags,
                        string documentMoniker,
                        string physicalView,
                        IVsHierarchy hierarchy,
                        uint itemid,
                        System.IntPtr docDataExisting,
                        out System.IntPtr docView,
                        out System.IntPtr docData,
                        out string editorCaption,
                        out Guid commandUIGuid,
                        out int createDocumentWindowFlags)
        {
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = GuidList.guidLaborasLangPackageEditorFactory;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            if ((createEditorFlags & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            if (docDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            IVsTextLines textBuffer = GetTextBuffer(docDataExisting);

            Type codeWindowType = typeof(IVsCodeWindow);
            var interfaceId = codeWindowType.GUID;
            var classId = typeof(VsCodeWindowClass).GUID;
            var window = (IVsCodeWindow)m_EditorPackage.CreateInstance(ref classId, ref interfaceId, codeWindowType);

            ErrorHandler.ThrowOnFailure(window.SetBuffer(textBuffer));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            docView = Marshal.GetIUnknownForObject(window);

            if (docDataExisting != IntPtr.Zero)
            {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            }
            else
            {
                docData = Marshal.GetIUnknownForObject(textBuffer);
            }

            return VSConstants.S_OK;
        }


        #region Helpers

        private IVsTextLines GetTextBuffer(IntPtr docDataExisting)
        {
            IVsTextLines textLines;

            if (docDataExisting == IntPtr.Zero)
            {
                // Create buffer if data doesn't exist
                Type textLinesType = typeof(IVsTextLines);
                Guid riid = textLinesType.GUID;
                Guid clsid = typeof(VsTextBufferClass).GUID;
                textLines = (IVsTextLines)m_EditorPackage.CreateInstance(ref clsid, ref riid, textLinesType);

                // Set the buffer's site
                ((IObjectWithSite)textLines).SetSite(m_VSServiceProvider.GetService(typeof(IOleServiceProvider)));
            }
            else
            {
                // Use the existing text buffer
                Object dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
                textLines = dataObject as IVsTextLines;

                if (textLines == null)
                {
                    // Try get the text buffer from textbuffer provider
                    IVsTextBufferProvider textBufferProvider = dataObject as IVsTextBufferProvider;
                    if (textBufferProvider != null)
                    {
                        textBufferProvider.GetTextBuffer(out textLines);
                    }
                }

                if (textLines == null)
                {
                    ErrorHandler.ThrowOnFailure((int)VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }
            }

            return textLines;
        }

        #endregion
    }
}
