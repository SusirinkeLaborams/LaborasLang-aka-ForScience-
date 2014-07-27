using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LaborasLangPackage
{
    internal sealed class LaborasLangProjectNode : ProjectNode
    {
        private Package m_Package;
        private static ImageList s_ImageList;
        internal static int s_ImageIndex;

        static LaborasLangProjectNode()
        {
            s_ImageList = Utilities.GetImageList(typeof(LaborasLangProjectNode).Assembly.GetManifestResourceStream("LaborasLangPackage.Resources.Project.bmp"));
        }
        
        public override int ImageIndex
        {
            get { return s_ImageIndex; }
        }

        public LaborasLangProjectNode(Package package)
        {
            this.m_Package = package;

            s_ImageIndex = this.ImageHandler.ImageList.Images.Count;

            foreach (Image img in s_ImageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
        }

        public override Guid ProjectGuid
        {
            get { return GuidList.guidLaborasLangProjectFactory; }
        }

        public override string ProjectType
        {
            get { return "LaborasLangProjectType"; }
        }

        public override void AddFileFromTemplate(string source, string target)
        {
            this.FileTemplateProcessor.UntokenFile(source, target);
            this.FileTemplateProcessor.Reset();
        }
    }
}
