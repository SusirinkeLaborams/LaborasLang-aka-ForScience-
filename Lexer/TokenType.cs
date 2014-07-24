using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{ 
    public enum TokenType
    {
        // Terminals
        EndOfLine = 0,
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
        //Non terminals
        StatementNode = 0x10000000,
        CodeBlockNode,
        DeclarationNode,
        AssignmentNode,
        RootNode,

        FullSymbol,
        SubSymbol,

        Value,
        LValue, 
        RValue,

        Type,

    }

    public static class TokenInfo
    {
        public static bool IsTerminal(this TokenType token)
        {
            return ((int)token < 0x10000000);
        }
    }
    
}
