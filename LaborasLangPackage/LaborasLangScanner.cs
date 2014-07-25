using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangPackage
{
    public sealed class LaborasLangScanner : IScanner
    {
        private IVsTextBuffer m_Buffer;
        string m_Source;

        public LaborasLangScanner(IVsTextBuffer buffer)
        {
            m_Buffer = buffer;
        }

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            tokenInfo.Type = TokenType.Unknown;
            tokenInfo.Color = TokenColor.Text;
            return false;
        }

        public void SetSource(string source, int offset)
        {
            m_Source = source.Substring(offset);
        }
    }
}
