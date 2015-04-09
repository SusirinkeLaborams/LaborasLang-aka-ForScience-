using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
    {
        public partial class RulePool
        {
            public static ParseRule[] LaborasLangRuleset = new ParseRule[]
                {
                    AlwaysCollapsableParseRule(StatementNode,
                        UseNode,
                        DeclarationNode,
                        ValueStatementNode,
                        CodeBlockNode,
                        WhileLoop,
                        ForLoop,
                        ReturnNode,
                        ConditionalSentence,
                        EndOfLine),
            
                    ParseRule(UseNode,
                        Use + FullSymbol + EndOfLine),

                    ParseRule(DeclarationNode,
                        DeclarationSubnode + EndOfLine),
            
                    AlwaysCollapsableParseRule(DeclarationSubnode,
                        ZeroOrMore(VariableModifier) + Type + Symbol + OptionalTail(Assignment + Value)),

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
                                
                    ParseRule(InfixOperator,
                        Period,          
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
                        BitwiseOrEqual,              
                        LeftShift,
                        RightShift,
                        Plus,
                        Minus,
                        Multiply,
                        Divide,
                        Remainder,
                        BitwiseAnd,
                        BitwiseOr,
                        BitwiseXor,
                        BitwiseComplement,
                        Equal,
                        NotEqual,
                        More,
                        Less,
                        MoreOrEqual,
                        LessOrEqual,      
                        LogicalAnd,
                        LogicalOr
               ),

                    ParseRule(PostfixOperator,
                        PlusPlus, 
                        MinusMinus,
                        IndexNode,
                        FunctionArgumentsList),

                    ParseRule(PrefixOperator,
                        PlusPlus, 
                        MinusMinus, 
                        Minus, 
                        Not,
                        BitwiseComplement,
                        CastOperator),

                    ParseRule(CastOperator,
                    LeftParenthesis + Type +  RightParenthesis),
                        
                    CollapsableParseRule(CodeBlockNode,
                        LeftCurlyBrace + ZeroOrMore(StatementNode) + RightCurlyBrace),
                    
                    ParseRule(IndexNode,
                    LeftBracket + Value + ZeroOrMore(CommaAndValue) + RightBracket,
                    LeftBracket + ZeroOrMore(Comma) + RightBracket),

                    
                    ParseRule(ParenthesesNode,
                        LeftParenthesis + Value + RightParenthesis,
                        Operand),
                        
                    /*PrefixNode is transformed using PrefixResolver. It is transformed to a recursive list {T, PrefixOperator}
                      where T can be PrefixNode, ParenthesesNode, Operand. T will be as specific as possible.*/
                    AlwaysCollapsableParseRule(PrefixNode,
                         ZeroOrMore(PrefixOperator) + ParenthesesNode),
                         
                    /*PostfixNode is transformed using InfixResolver. It is transformed to a recursive list {T, PostfixOperator}
                      where T can be PostfixNode, PrefixNode, ParenthesesNode, Operand. T will be as specific as possible.*/
                    AlwaysCollapsableParseRule(PostfixNode,
                        PrefixNode + ZeroOrMore(PostfixOperator)),

                    /*InfixNode is transformed using InfixResolver. It is transformed to a tree with structure {A, B, InfixOperator}.
                      A, B can have types of InfixNode, PostfixNode, PrefixNode, ParenthesesNode, Operand. A, B will be as specific as possible.*/
                    AlwaysCollapsableParseRule(InfixNode,
                        PostfixNode + ZeroOrMore(InfixSubnode)),

                   AlwaysCollapsableParseRule(InfixSubnode,
                        InfixOperator + PostfixNode),

                    ParseRule(Value,
                        InfixNode),
                       
                    AlwaysCollapsableParseRule(Operand,
                        ArrayLiteral,
                        Function,
                        Type,
                        Symbol,
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
                        Type + InitializerList,
                        InitializerList),

                    ParseRule(InitializerList,     
                        LeftCurlyBrace + Value + ZeroOrMore(CommaAndValue) + RightCurlyBrace,
                        LeftCurlyBrace + RightCurlyBrace),

                    ParseRule(FunctionArgumentsList,
                        LeftParenthesis + RightParenthesis,
                        LeftParenthesis + Value + ZeroOrMore(CommaAndValue) + RightParenthesis),

                    AlwaysCollapsableParseRule(CommaAndValue,
                        Comma + Value),

                    /*Tail of FullSymbol is transformed using FullSymbolPostProcessor.
                        It is transformed to a recursive list of {Symbol, FullSymbol(Optional)}*/
                    ParseRule(FullSymbol,
                        Symbol + ZeroOrMore(SubSymbol)),

                    AlwaysCollapsableParseRule(SubSymbol,
                        Period + Symbol),

                    ParseRule(ParameterList,
                        FunctorParameters,   
                        IndexNode                     
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

                    ParseRule(ForLoop,
                        For + LeftParenthesis + DeclarationSubnode + In + Value + RightParenthesis + StatementNode,
                        For + LeftParenthesis + Optional(Value) + EndOfLine + Optional(Value) + EndOfLine + Optional(Value) + RightParenthesis + StatementNode,
                        For + LeftParenthesis + Optional(DeclarationSubnode) + EndOfLine + Optional(Value) + EndOfLine + Optional(Value) + RightParenthesis + StatementNode
                     ),

                    ParseRule(ConditionalSentence,
                        If + LeftParenthesis + Value + RightParenthesis + StatementNode + OptionalTail(Else + StatementNode)),
            };

    }
}
