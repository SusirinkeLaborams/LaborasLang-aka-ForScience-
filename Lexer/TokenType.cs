﻿namespace Lexer
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
        Private,
        Public,
        
        //Non terminals
        NonTerminalToken,


        StatementNode,
        CodeBlockNode,
        DeclarationNode,
        RootNode,

        FullSymbol,
        SubSymbol,

        Value,
        VariableModifier,

        Type,
        TypeSubnode,
        TypeAndSymbolSubnode,

        WhileLoop,

        Function,

        ConditionalSentence,
        AssignmentOperator,

        CommaAndValue,

        EqualityOperator,
        RelationalOperator,
        ShiftOperator,
        AdditiveOperator,
        MultiplicativeOperator,

        AssignmentOperatorNode,
        OrNode,
        OrSubnode,
        AndNode,
        AndSubnode,
        BitwiseOrNode,
        BitwiseOrSubnode,
        BitwiseXorNode,
        BitwiseXorSubnode,
        BitwiseAndNode,
        BitwiseAndSubnode,
        PeriodNode,
        PeriodSubnode,
        Operand,
        PrefixNode, 
        PostfixNode,
        PrefixOperator,
        PostfixOperator,
        InlineFunctionCallNode,
        FunctionCallNode,
        FunctionArgumentsList,
        EqualityOperatorNode,
        RelationalOperatorNode,
        ShiftOperatorNode,
        AdditiveOperatorNode,
        MultiplicativeOperatorNode,
        ParenthesesNode,
        
        MultiplicativeOperatorSubnode,
        AdditiveOperatorSubnode,
        ShiftOperatorSubnode,
        RelationalOperatorSubnode,
        EqualityOperatorSubnode,

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
