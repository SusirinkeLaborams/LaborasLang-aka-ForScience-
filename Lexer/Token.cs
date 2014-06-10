using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        RightBracket
    }

    public class Token
    {
        public Token()
        {
            Content = "";
        }

        public TokenType Type { get; internal set; }
        public string Content { get; internal set; }
        public Location Start { get; internal set; }
        public Location End { get; internal set; }
    }


}
