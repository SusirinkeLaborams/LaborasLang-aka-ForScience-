using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
    {
        partial class RulePool
        {
            public static ParseRule[] LaborasLangRuleset = new ParseRule[]
                {
                    AlwaysCollapsableParseRule(StatementNode,
                        UseNode,
                        DeclarationNode,
                        ValueStatementNode,
                        CodeBlockNode,
                        WhileLoop,
                        ReturnNode,
                        ConditionalSentence),
            
                    ParseRule(UseNode,
                        Use + FullSymbol + EndOfLine),

                    ParseRule(DeclarationNode,
                        DeclarationSubnode + EndOfLine),
            
                    AlwaysCollapsableParseRule(DeclarationSubnode,
                        ZeroOrMore(VariableModifier) + Type + Symbol + Optional(Assignment + Value)),

                    AlwaysCollapsableParseRule(ValueStatementNode,
                        Value + EndOfLine),

                    ParseRule(ReturnNode,
                        Return + Value + EndOfLine,
                        Return + EndOfLine),

                    ParseRule(VariableModifier, 
                        Const,
                        Internal,
                        Private,
                        Public,
                        Protected,
                        NoInstance,
                        Virtual,
                        Entry,
                        Mutable),
                                
                    AlwaysCollapsableParseRule(AssignmentOperator,
                        Assignment,
                        PlusEqual,
                        MinusEqual,
                        DivideEqual,
                        MultiplyEqual,
                        RemainderEqual,
                        LeftShiftEqual,
                        RightShiftEqual,
                        LogicalAndEqual,
                        LogicalOrEqual,
                        BitwiseAndEqual,
                        BitwiseXorEqual,
                        BitwiseOrEqual),

                    AlwaysCollapsableParseRule(EqualityOperator,
                        Equal,
                        NotEqual),

                    AlwaysCollapsableParseRule(RelationalOperator,
                        More,
                        Less,
                        MoreOrEqual,
                        LessOrEqual),

                    AlwaysCollapsableParseRule(ShiftOperator,
                        LeftShift,
                        RightShift),

                    AlwaysCollapsableParseRule(AdditiveOperator,
                        Plus,
                        Minus),

                    AlwaysCollapsableParseRule(MultiplicativeOperator,
                        Multiply,
                        Divide,
                        Remainder),

                    AlwaysCollapsableParseRule(PostfixOperator,
                        PlusPlus, 
                        MinusMinus),

                    AlwaysCollapsableParseRule(PrefixOperator,
                        PlusPlus, 
                        MinusMinus, 
                        Minus, 
                        Not,
                        BitwiseComplement),
                        
                    CollapsableParseRule(CodeBlockNode,
                        LeftCurlyBrace + ZeroOrMore(StatementNode) + RightCurlyBrace),
                    
                    ParseRule(IndexNode,
                    LeftBracket + Value + ZeroOrMore(CommaAndValue) + RightBracket),

                    #region Operators

                    /* Operator precedence:
                            Parentheses
                            Period
                            PostfixOperator
                            PrefixOperator
                            MultiplicativeOperator (Remainder, Division, Multiplication)
                            AdditiveOperator (Minus, Plus)
                            ShiftOperator (LeftShift, RightShift)
                            RelationalOperator (LessOrEqual, MoreOrEqual, Less, More)
                            EqualityOperator (Equal, NotEqual)
                            BitwiseAnd
                            BitwiseXor
                            BitwiseOr
                            And
                            Or
                            Assignment operator
                        */
                    
                    CollapsableParseRule(ParenthesesNode,
                        LeftParenthesis + Value + RightParenthesis,
                        Operand),

                    CollapsableParseRule(PeriodNode,
                        ParenthesesNode + ZeroOrMore(PeriodSubnode)),

                    AlwaysCollapsableParseRule(PeriodSubnode,
                        Period + ParenthesesNode),

                    CollapsableParseRule(IndexAccessNode,
                        PeriodNode + ZeroOrMore(IndexNode)),

                    CollapsableParseRule(PostfixNode,
                        IndexAccessNode + ZeroOrMore(PostfixOperator)),

                    CollapsableParseRule(PrefixNode,
                        ZeroOrMore(PrefixOperator) + PostfixNode),

                    CollapsableParseRule(MultiplicativeOperatorNode,
                        PrefixNode + ZeroOrMore(MultiplicativeOperatorSubnode)),

                    AlwaysCollapsableParseRule(MultiplicativeOperatorSubnode,
                        MultiplicativeOperator + PrefixNode),

                    CollapsableParseRule(AdditiveOperatorNode,
                        MultiplicativeOperatorNode + ZeroOrMore(AdditiveOperatorSubnode)),

                    AlwaysCollapsableParseRule(AdditiveOperatorSubnode,
                        AdditiveOperator + MultiplicativeOperatorNode),

                    CollapsableParseRule(ShiftOperatorNode,
                        AdditiveOperatorNode + ZeroOrMore(ShiftOperatorSubnode)),

                    AlwaysCollapsableParseRule(ShiftOperatorSubnode,
                        ShiftOperator + AdditiveOperatorNode),

                    CollapsableParseRule(RelationalOperatorNode,
                        ShiftOperatorNode + ZeroOrMore(RelationalOperatorSubnode)),

                    AlwaysCollapsableParseRule(RelationalOperatorSubnode,
                        RelationalOperator + ShiftOperatorNode),

                    CollapsableParseRule(EqualityOperatorNode,
                        RelationalOperatorNode + ZeroOrMore(EqualityOperatorSubnode)),

                    AlwaysCollapsableParseRule(EqualityOperatorSubnode,
                        EqualityOperator + RelationalOperatorNode),
                        
                    CollapsableParseRule(BitwiseAndNode,
                        EqualityOperatorNode + ZeroOrMore(BitwiseAndSubnode)),

                    AlwaysCollapsableParseRule(BitwiseAndSubnode,
                        BitwiseAnd + EqualityOperatorNode),
                        
                    CollapsableParseRule(BitwiseXorNode,
                        BitwiseAndNode + ZeroOrMore(BitwiseXorSubnode)),

                    AlwaysCollapsableParseRule(BitwiseXorSubnode,
                        BitwiseXor + BitwiseAndNode),
                        
                    CollapsableParseRule(BitwiseOrNode,
                        BitwiseXorNode + ZeroOrMore(BitwiseOrSubnode)),

                    AlwaysCollapsableParseRule(BitwiseOrSubnode,
                        BitwiseOr + BitwiseXorNode),
                        
                    CollapsableParseRule(LogicalAndNode,
                        BitwiseOrNode + ZeroOrMore(LogicalAndSubnode)),

                    AlwaysCollapsableParseRule(LogicalAndSubnode,
                        LogicalAnd + BitwiseOrNode),
                        
                    CollapsableParseRule(LogicalOrNode,
                        LogicalAndNode + ZeroOrMore(LogicalOrSubnode)),

                    AlwaysCollapsableParseRule(LogicalOrSubnode,
                        LogicalOr + LogicalAndNode),


                    // Assignment operator is evaluated right to left
                    CollapsableParseRule(AssignmentOperatorNode,
                        LogicalOrNode + AssignmentOperator + AssignmentOperatorNode,
                        LogicalOrNode),
 
                    #endregion

                    ParseRule(Value,
                        AssignmentOperatorNode),
                       
                    AlwaysCollapsableParseRule(Operand,
                        ArrayLiteral,
                        InlineFunctionCallNode,
                        Function,
                        FunctionCallNode,
                        FullSymbol,
                        Type,
                        LiteralNode),

                    ParseRule(LiteralNode,                       
                        Float,
                        Integer,
                        Double,
                        Long,
                        StringLiteral,
                        True,
                        False),
                    
                    ParseRule(ArrayLiteral, 
                        Type + LeftCurlyBrace + Value + ZeroOrMore(CommaAndValue) + RightCurlyBrace,
                        LeftCurlyBrace + Value + ZeroOrMore(CommaAndValue) + RightCurlyBrace,
                        LeftCurlyBrace + RightCurlyBrace),
                        
                    ParseRule(InlineFunctionCallNode,
                        Function + OneOrMore(FunctionArgumentsList)),

                    ParseRule(FunctionCallNode,
                        FullSymbol + OneOrMore(FunctionArgumentsList)),

                    ParseRule(FunctionArgumentsList,
                        LeftParenthesis + RightParenthesis,
                        LeftParenthesis + Value + ZeroOrMore(CommaAndValue) + RightParenthesis),

                    AlwaysCollapsableParseRule(CommaAndValue,
                        Comma + Value),

                    ParseRule(FullSymbol,
                        Symbol + ZeroOrMore(SubSymbol)),

                    AlwaysCollapsableParseRule(SubSymbol,
                        Period + Symbol),

                    ParseRule(ParameterList,
                        FunctorParameters,   
                        ArrayTypeParameters                     
                    ),

                    ParseRule(ArrayTypeParameters,
                        LeftBracket + ZeroOrMore(Comma) + RightBracket
                    ),

                    ParseRule(FunctorParameters,
                        LeftParenthesis + Type + ZeroOrMore(TypeSubnode) + RightParenthesis,
                        LeftParenthesis + Type + Symbol + ZeroOrMore(TypeAndSymbolSubnode) + RightParenthesis,
                        LeftParenthesis + RightParenthesis),

                    ParseRule(Type,                        
                        FullSymbol + ZeroOrMore(ParameterList)),
                       
                    AlwaysCollapsableParseRule(TypeSubnode,
                        Comma + Type),

                    AlwaysCollapsableParseRule(TypeAndSymbolSubnode,
                        Comma + Type + Symbol),

                    ParseRule(Function,
                        Type + CodeBlockNode),                    

                    ParseRule(WhileLoop,
                        While + LeftParenthesis + Value + RightParenthesis + StatementNode),

                    ParseRule(ConditionalSentence,
                        If + LeftParenthesis + Value + RightParenthesis + StatementNode + Optional(Else + StatementNode)),
            };

    }
}
