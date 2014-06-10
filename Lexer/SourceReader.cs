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
        private int m_Collumn;
        private int m_Row;

        public SourceReader(string source)
        {
            m_Source = source;
            m_Index = 0;
            m_Collumn = 1;
            m_Row = 1;
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
            if (m_Source.Length < m_Index)
            {
                return m_Source[m_Index];
            }
            else
            {
                return '\0';
            }

        }

        public Location Location
        {
            get
            {
                return new Location(m_Collumn, m_Row);
            }
        }


        public char Pop()
        {

            if (m_Source.Length < m_Index)
            {
                var value = m_Source[m_Index];
                m_Index++;

                if (value == '\n')
                {
                    m_Collumn = 1;
                    m_Row++;
                }
                else
                {
                    m_Collumn++;
                }

                return value;
            }
            else
            {
                return '\0';
            }
        }

    }
}
