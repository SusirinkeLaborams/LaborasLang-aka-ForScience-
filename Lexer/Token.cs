using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lexer
{
    public enum TokenType
    {
        EndOfLine,
        Comment,
        BitwiseAnd,
        BitwiseAndEqual,
        And,
        Dot,
        Plus,
        PlusPlus,
        Minus,
        MinusMinus,
        MinusEqual,
        NotEqual,
        Not,
        Whitespace,
        PlusEqual,
        StringLiteral,
        BitwiseComplementEqual,
        BitwiseComplement,
        BitwiseXor,
        BitwiseXorEqual,
        BitwiseOr,
        Or,
        BitwiseOrEqual,
        LeftShiftEqual,
        LeftShift,
        LessOrEqual,
        Less,
        More,
        RightShift,
        RightShiftEqual,
        MoreOrEqual,
        Divide,
        DivideEqual,
        Multiply,
        MultiplyEqual,
        Remainder,
        RemainderEqual,
        Assignment,
        Equal,
        LeftCurlyBracket,
        RightCurlyBracket,
        LeftBracket,
        RightBracket,
        Unknown,
        Integer,
        Float,
        Long,
        Double,
        MalformedToken,
        Symbol,
    }

    [Serializable]
    public class Token
    {
        public Token()
        {
            Content = "";
        }

        private TokenType m_Type;
        private string m_Content;
        private Location m_Start;
        private Location m_End;

        #region PublicParameters
        public TokenType Type
        {
            get
            {
                return m_Type;
            }
            internal set
            {
                m_Type = value;
            }
        }
        public string Content
        {
            get
            {
                return m_Content;
            }
            internal set
            {
                m_Content = value;
            }
        }
        public Location Start
        {
            get
            {
                return m_Start;
            }
            internal set
            {
                m_Start = value;
            }
        }
        public Location End
        {
            get
            {
                return m_End;
            }
            internal set
            {
                m_End = value;
            }
        }
        #endregion Public parameters

        public static bool operator ==(Token a, Token b)
        {
            return a.Type == b.Type &&
                a.Content == b.Content &&
                a.Start == b.Start &&
                a.End == b.End;
        }

        public static bool operator !=(Token a, Token b)
        {
            return !(a == b);
        }
    }
}
