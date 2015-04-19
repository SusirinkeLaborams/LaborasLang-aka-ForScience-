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

                    AlwaysCollapsableParseRule(CodeConstruct,
                        CodeBlockNode,
                        WhileLoop,
                        ForLoop,
                        ConditionalSentence,
                        StatementWithEndOfLine),

                    AlwaysCollapsableParseRule(StatementNode,
                        UseNode,
                        DeclarationNode,
                        Value,
                        ReturnNode),

                    ParseRule(StatementWithEndOfLine,
                        Optional(StatementNode) + EndOfLine),
            
                    ParseRule(UseNode,
                        Use + FullSymbol),

                    ParseRule(DeclarationNode,
                        ZeroOrMore(VariableModifier) + Type + Symbol + OptionalTail(Assignment + Value)),

                    ParseRule(ReturnNode,
                        Return + OptionalTail(Value)),

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
                        LeftCurlyBrace + ZeroOrMore(CodeConstruct) + RightCurlyBrace),
                    
                    ParseRule(IndexNode,
                    LeftBracket + Value + ZeroOrMore(CommaAndValue) + RightBracket,
                    LeftBracket + ZeroOrMore(Comma) + RightBracket),

                    
                    ParseRule(ParenthesesNode,
                        LeftParenthesis + Value + RightParenthesis,
                        Operand),
                        
                    /*PrefixNode is transformed using PrefixResolver. It is transformed to a recursive list {T, PrefixOperator}
                      where T can be PrefixNode, ParenthesesNode, Operand. T will be as specific as possible.*/
                    AlwaysCollapsableParseRule(PrefixNode,
                         PrefixOperator + PrefixNode,
                         PrefixOperator + ParenthesesNode,
                         // (foo()) is both a valid cast operator and a valid parenthesis node. have to add parenthesis node to make sure that if it fails
                         // as prefix it will be handled as parenthesis node
                         ParenthesesNode),
                         
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
                        While + LeftParenthesis + Value + RightParenthesis + CodeConstruct),

                    ParseRule(ForLoop,
                        For + LeftParenthesis + DeclarationNode + In + Value + RightParenthesis + CodeConstruct,
                        For + LeftParenthesis + Optional(Value) + EndOfLine + Optional(Value) + EndOfLine + Optional(Value) + RightParenthesis + CodeConstruct,
                        For + LeftParenthesis + Optional(DeclarationNode) + EndOfLine + Optional(Value) + EndOfLine + Optional(Value) + RightParenthesis + CodeConstruct
                     ),

                    ParseRule(ConditionalSentence,
                        If + LeftParenthesis + Value + RightParenthesis + CodeConstruct + OptionalTail(Else + CodeConstruct)),
            };

    }
}
