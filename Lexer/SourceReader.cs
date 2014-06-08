using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    class SourceReader
    {
        private string m_Source;
        private int m_Index;

        public SourceReader(string source)
        {
            m_Source = source;
            m_Index = 0;
        }

        public string Source
        {
            get
            {
                return m_Source;
            }
        }

        public char Peek()
        {
            try
            {
                return m_Source[m_Index];
            }
            catch
            {
                return '\0';
            }

        }

        public char Pop()
        {

            try
            {
                var value = m_Source[m_Index];
                m_Index++;
                return value;
            }
            catch
            {
                return '\0';
            }
        }

    }
}
