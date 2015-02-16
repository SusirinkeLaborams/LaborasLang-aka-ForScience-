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

        NoInstance,
        Private,
        Public,
        Mutable,

        LeftBracket,
        RightBracket,

        //Non terminals
        NonTerminalToken,

        ArrayLiteral,

        CodeBlockNode,
        DeclarationNode,
        ReturnNode,
        Value,

        RootNode,

        FullSymbol,
        Type,
        UseNode,

        WhileLoop,
        Function,
        ConditionalSentence,

        AssignmentOperatorNode,
        LogicalOrNode,
        LogicalAndNode,
        BitwiseOrNode,
        BitwiseXorNode,
        BitwiseAndNode,
        PeriodNode,
        PrefixNode,
        PostfixNode,
        InlineFunctionCallNode,
        FunctionCallNode,
        FunctionArgumentsList,
        EqualityOperatorNode,
        RelationalOperatorNode,
        ShiftOperatorNode,
        AdditiveOperatorNode,
        MultiplicativeOperatorNode,
        ParenthesesNode,
        LiteralNode,
        UnknownNode,
        LexerInternalTokens,    // Lexer internal-only tokens start from here

        StatementNode,
        DeclarationSubnode,
        ValueStatementNode,

        TypeParameters,
        TypeSubnode,
        TypeAndSymbolSubnode,
        CommaAndValue,
        SubSymbol,

        Operand,
        VariableModifier,

        PeriodSubnode,
        MultiplicativeOperatorSubnode,
        AdditiveOperatorSubnode,
        ShiftOperatorSubnode,
        RelationalOperatorSubnode,
        EqualityOperatorSubnode,
        BitwiseAndSubnode,
        BitwiseXorSubnode,
        BitwiseOrSubnode,
        LogicalAndSubnode,
        LogicalOrSubnode,

        PrefixOperator,
        PostfixOperator,
        MultiplicativeOperator,
        AdditiveOperator,
        ShiftOperator,
        RelationalOperator,
        EqualityOperator,
        AssignmentOperator,

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
    }
    
}
