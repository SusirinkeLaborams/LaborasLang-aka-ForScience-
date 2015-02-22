namespace Lexer
{
    internal sealed class SourceReader
    {
        private readonly string m_Source;
        private readonly int m_OriginalSourceLength;
        private int m_Index;
        private int m_Column;
        private int m_Row;

        public SourceReader(string source)
        {
            m_OriginalSourceLength = source.Length;
            m_Source = source + '\0';
            m_Index = 0;
            m_Column = 1;
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
            return m_Source[m_Index];
        }

        public Location Location
        {
            get
            {
                return new Location(m_Column, m_Row);
            }
        }


        public char Pop()
        {
            if (m_Index < m_OriginalSourceLength)
            {
                var value = m_Source[m_Index];
                m_Index++;

                if (value == '\n')
                {
                    m_Column = 1;
                    m_Row++;
                }
                else
                {
                    m_Column++;
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
