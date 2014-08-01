using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace LaborasLangPackage.CoreExtension
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
