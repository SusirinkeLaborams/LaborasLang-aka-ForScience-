using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangPackage
{
    class LaborasLangService : LanguageService
    {
        private LaborasLangPreferences m_LanguagePreferences;
        private LaborasLangScanner m_Scanner;
        private LaborasLangAuthoringScope m_AuthoringScope;

        public override string GetFormatFilterList()
        {
            return "LL files (*.ll)\n*.ll";
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (m_LanguagePreferences == null)
            {
                m_LanguagePreferences = new LaborasLangPreferences();
                m_LanguagePreferences.Init();
            }

            return m_LanguagePreferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            if (m_Scanner == null)
            {
                m_Scanner = new LaborasLangScanner(buffer);
            }

            return m_Scanner;
        }

        public override string Name
        {
            get { return "LaborasLang"; }
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            if (m_AuthoringScope == null)
            {
                m_AuthoringScope = new LaborasLangAuthoringScope();
            }

            return m_AuthoringScope;
        }
    }
}
