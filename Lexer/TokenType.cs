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

        RootNode,

        FullSymbol,
        Type,
        UseNode,

        WhileLoop,
        ForLoop,
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

        StatementNode,
        DeclarationSubnode,
        ValueStatementNode,

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
                case TokenType.Period:
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
    }
    
}
