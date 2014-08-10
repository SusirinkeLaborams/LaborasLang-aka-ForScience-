namespace Lexer
{ 
    public enum TokenType
    {
        Unknown = 0,

        // Terminals
        EndOfLine,
        Comma,
        Period,
        Comment,
        BitwiseAnd,
        BitwiseAndEqual,
        And,
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
        Integer,
        Float,
        Long,
        Double,
        MalformedToken,
        Symbol,
        Abstract,
        As,
        Base,
        Break,
        Case,
        Catch,
        Class,
        Const,
        Continue,
        Default,
        Do,
        Extern,
        Else,
        Enum,
        False,
        Finally,
        For,
        Goto,
        If,
        Interface,
        Internal,
        Is,
        New,
        Null,
        Namespace,
        Out,
        Override,
        Protected,
        Ref,
        Return,
        Switch,
        Sealed,
        This,
        Throw,
        Struct,
        True,
        Try,
        Using,
        Virtual,
        While,


        Static,
        Constant,
        Private,
        Public,
        
        //Non terminals
        NonTerminalToken,


        StatementNode,
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
        VariableModifier,
        TypeArgument,

        FunctionCall,
        FunctionArgument,
        FunctionDeclarationArgument,
        FunctionBody,

        WhileLoop,

        Operator,
        ArithmeticSubnode,
        ArithmeticNode,

        Function,
        FunctionSegment,
        FunctionArgumentList,

        ConditionalSentence,
        AssignmentOperator,

        TokenTypeCount
    }

    public static class TokenInfo
    {
        public static bool IsTerminal(this TokenType token)
        {
            // PERF: CompareTo is expensive
            return (int)token < (int)TokenType.NonTerminalToken;
        }
    }
    
}
