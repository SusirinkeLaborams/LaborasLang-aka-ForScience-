namespace Lexer
{
    public enum TokenType
    {

        // Terminals
        EndOfLine = 0,
        Comma,
        Period,
        BitwiseAnd,
        BitwiseAndEqual,
        Plus,
        PlusPlus,
        Minus,
        MinusMinus,
        MinusEqual,
        NotEqual,
        Not,
        PlusEqual,
        CharLiteral,
        StringLiteral,
        BitwiseComplement,
        BitwiseXor,
        BitwiseXorEqual,
        BitwiseOr,
        BitwiseOrEqual,
        LeftShiftEqual,
        LeftShift,
        LessOrEqual,
        Less,
        LogicalAnd,
        LogicalAndEqual,
        LogicalOr,
        LogicalOrEqual,
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
        LeftCurlyBrace,
        RightCurlyBrace,
        LeftParenthesis,
        RightParenthesis,
        Integer,
        Float,
        Long,
        Double,
        MalformedToken,
        Symbol,
        Const,
        Else,
        Entry,
        False,
        If,
        Internal,
        Protected,
        Return,
        True,
        Use,
        Virtual,
        While,
        For, 
        In,
        Null,

        Empty,//empty node

        NoInstance,
        Private,
        Public,
        Mutable,

        LeftBracket,
        RightBracket,

        //Non terminals
        NonTerminalToken,

        ArrayLiteral,
        InitializerList,

        CodeBlockNode,
        DeclarationNode,
        ReturnNode,
        Value,
        SpecialValue,

        RootNode,

        FullSymbol,
        Type,
        UseNode,

        CodeConstruct,
        WhileLoop,
        ForLoop,
        ForEachLoop,
        Function,
        ConditionalSentence,

        CastOperator,
        PrefixNode,
        PostfixNode,
        FunctionArgumentsList,
        ParenthesesNode,
        LiteralNode,
        UnknownNode,
        IndexNode,        
        FunctorParameters,
        InfixNode,
        InfixOperator,
        InfixSubnode,
        LexerInternalTokens,    // Lexer internal-only tokens start from here

        StatementWithEndOfLine,
        StatementNode,

        ParameterList,
        TypeSubnode,
        TypeAndSymbolSubnode,
        CommaAndValue,
        SubSymbol,

        Operand,
        VariableModifier,

                                                                                        
        PrefixOperator,
        PostfixOperator,
                                                
        TokenTypeCount,
    }

    public static class TokenInfo
    {
        public static bool IsTerminal(this TokenType token)
        {
            // PERF: CompareTo is expensive
            return (int)token < (int)TokenType.NonTerminalToken;
        }

        public static bool IsRecoveryPoint(this TokenType token)
        {            
            switch (token)
            {
                case TokenType.EndOfLine:
                case TokenType.RightCurlyBrace:
                case TokenType.LeftCurlyBrace:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMeaningful(this TokenType token)
        {
            return true;
        }

        public static bool IsRightAssociative(this TokenType token)
        {
            switch (token)
            {
                case TokenType.Assignment:
                case TokenType.PlusEqual:
                case TokenType.MinusEqual:
                case TokenType.DivideEqual:
                case TokenType.MultiplyEqual:
                case TokenType.RemainderEqual:
                case TokenType.LeftShiftEqual:
                case TokenType.RightShiftEqual:
                case TokenType.LogicalAndEqual:
                case TokenType.LogicalOrEqual:
                case TokenType.BitwiseAndEqual:
                case TokenType.BitwiseXorEqual:
                case TokenType.BitwiseOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAssignmentOp(this TokenType token)
        {
            switch(token)
            {
                case TokenType.Assignment:
                case TokenType.PlusEqual:
                case TokenType.MinusEqual:
                case TokenType.DivideEqual:
                case TokenType.MultiplyEqual:
                case TokenType.RemainderEqual:
                case TokenType.LeftShiftEqual:
                case TokenType.RightShiftEqual:
                case TokenType.LogicalAndEqual:
                case TokenType.LogicalOrEqual:
                case TokenType.BitwiseAndEqual:
                case TokenType.BitwiseXorEqual:
                case TokenType.BitwiseOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPeriodOp(this TokenType token)
        {
            return token == TokenType.Period;
        }

        public static bool IsBinaryOp(this TokenType token)
        {
            switch (token)
            {
                case TokenType.LeftShift:
                case TokenType.RightShift:
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Remainder:
                case TokenType.BitwiseAnd:
                case TokenType.BitwiseOr:
                case TokenType.BitwiseXor:
                case TokenType.BitwiseComplement:
                case TokenType.Equal:
                case TokenType.NotEqual:
                case TokenType.More:
                case TokenType.Less:
                case TokenType.MoreOrEqual:
                case TokenType.LessOrEqual:
                case TokenType.LogicalAnd:
                case TokenType.LogicalOr: 
                    return true;
                default:
                    return false;
            }
        }
    }
    
}
