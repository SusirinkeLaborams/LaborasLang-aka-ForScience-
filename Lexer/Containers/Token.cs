using System.Diagnostics;

namespace Lexer.Containers
{
    internal unsafe struct Token
    {
        private readonly InternalToken* m_TokenPtr;

        internal Token(InternalToken* tokenPtr) : this()
        {
            m_TokenPtr = tokenPtr;
        }

        public TokenType Type
        {
            get
            {
                return m_TokenPtr->type;
            }
            internal set
            {
                m_TokenPtr->type = value;
            }
        }

        public FastString Content
        {
            get
            {
                return m_TokenPtr->content;
            }
            internal set
            {
                m_TokenPtr->content = value;
            }
        }

        public Location Start
        {
            get
            {
                return m_TokenPtr->start;
            }
            internal set
            {
                m_TokenPtr->start = value;
            }
        }

        public Location End
        {
            get
            {
                return m_TokenPtr->end;
            }
            internal set
            {
                m_TokenPtr->end = value;
            }
        }

        public static bool operator ==(Token a, object b)
        {
            if (object.ReferenceEquals(b, null))
            {
                return a.m_TokenPtr == null;
            }

            var bToken = (b as Token?).GetValueOrDefault();

            if (bToken != null)
            {
                return a.Type == bToken.Type &&
                    a.Content == bToken.Content &&
                    a.Start == bToken.Start &&
                    a.End == bToken.End;
            }

            return false;
        }

        public static bool operator !=(Token a, object b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Start: {1}, End: {2}, Content: \"{3}\"", Type, Start, End, Content);
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            var typeHash = (int)m_TokenPtr->type;
            var contentHash = m_TokenPtr->content.GetHashCode();
            var startHash = m_TokenPtr->start.GetHashCode();
            var endHash = m_TokenPtr->end.GetHashCode();
            
            return (typeHash << 24) ^ (contentHash << 16) + (startHash ^ endHash);
        }

        [DebuggerDisplay("Token, type = {m_Type}")]
        internal struct InternalToken
        {
            public TokenType type;
            public Location start;
            public Location end;
            public FastString content;
        }
    }
}
