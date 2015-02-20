using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lexer
{
    internal sealed class SyntaxMatcher
    {
        private static ParseRule[] m_ParseRules;
        private Token[] m_Source;
        private RootNode m_RootNode;

        private int m_LastMatched = 0;
        private int LastMatched
        {
            get
            {
                return m_LastMatched;
            }
            set
            {
                m_LastMatched = m_LastMatched < value ? value : m_LastMatched;
            }
        }

        internal static ParseRule[] ParseRulePool
        {
            get
            {
                return new ParseRule[]
                {

                #region Syntax rules

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
                    LeftBracket + Value + ZeroOrMore(CommaAndValue) + RightBracket,
                    LeftBracket + RightBracket),

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

                    ParseRule(TypeParameters,
                        LeftParenthesis + Type + ZeroOrMore(TypeSubnode) + RightParenthesis,
                        LeftParenthesis + Type + Symbol + ZeroOrMore(TypeAndSymbolSubnode) + RightParenthesis,
                        LeftParenthesis + RightParenthesis,
                        IndexNode,
                        LeftBracket + RightBracket
                    ),

                    ParseRule(Type,                        
                       FullSymbol + ZeroOrMore(TypeParameters)),
                       
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

                #endregion

                };
            }
        }

        static SyntaxMatcher()
        {
            m_ParseRules = new ParseRule[(int)TokenType.TokenTypeCount];

            foreach (var rule in ParseRulePool)
            {
                m_ParseRules[(int)rule.Result] = rule;
            }
        }

        public SyntaxMatcher(Token[] sourceTokens, RootNode rootNode)
        {
            m_RootNode = rootNode;
            m_Source = sourceTokens;
        }
        
        public AstNode Match()
        {
            var defaultConditions = new Condition[] { new Condition(TokenType.StatementNode, ConditionType.OneOrMore) };
            var tokensConsumed = 0;

            AstNode matchedNode = Match(0, defaultConditions, ref tokensConsumed);
            if (matchedNode.IsNull)
            {
                matchedNode = m_RootNode.NodePool.ProvideNode();
            }

            while(tokensConsumed < m_Source.Length) 
            {
                var tokensSkipped = 0;
                matchedNode.AddChild(m_RootNode, SkipToRecovery(tokensConsumed, ref tokensSkipped));

                tokensConsumed += tokensSkipped;

                var consumed = 0;
                var matchResult = Match(tokensConsumed, defaultConditions, ref consumed);
                
                tokensConsumed += consumed;

                if (!matchResult.IsNull)
                {
                    foreach(var child in matchResult.Children) {
                        matchedNode.AddChild(m_RootNode, child);
                    }
                }                
            }

            var token = m_RootNode.ProvideToken();

            token.Start = m_Source[0].Start;
            token.End = m_Source[tokensConsumed - 1].End;
            token.Content = FastString.Empty;
            token.Type = TokenType.RootNode;

            matchedNode.Token = token;
            
            m_RootNode.SetNode(matchedNode);
            return matchedNode;
        }

        private AstNode SkipToRecovery(int offset, ref int tokensConsumed)
        {
            var node = m_RootNode.NodePool.ProvideNode();
            
            tokensConsumed = 1;
            for (int i = offset; i < m_Source.Length - 1; i++, tokensConsumed++)
            {                
                node.AddTerminal(m_RootNode, m_Source[i]);
                if (m_Source[i].Type.IsRecoveryPoint())
                {
                    break;
                }
            }

            var token = m_RootNode.ProvideToken();
            
            token.Start = m_Source[offset].Start;
            var lastToken = offset + tokensConsumed - 1;
            
            token.End = m_Source[lastToken].End;
            token.Content = FastString.Empty;
            token.Type = TokenType.UnknownNode;

            node.Token = token;
            return node;
        }

        private AstNode Match(int sourceOffset, Condition[] rule, ref int tokensConsumed)
        {
#if DEBUG
            Debug.Assert(rule.Length > 0, "Rule count must be more than 0!");
#endif

            var node = m_RootNode.NodePool.ProvideNode();

            // PERF: use normal loop instead of foreach
            for (int i = 0; i < rule.Length; i++)
            {
                if (!MatchRule(rule[i], sourceOffset, ref node, ref tokensConsumed))
                {
                    if (rule[i].Type == ConditionType.OptionalFromThis)
                    {
                        break;
                    }
                    else
                    {
                        node.Cleanup(m_RootNode);
                        return default(AstNode);
                    }
                }
            }

            var token = m_RootNode.ProvideToken();
            token.Start = m_Source[sourceOffset].Start;
            token.End = m_Source[sourceOffset + tokensConsumed - 1].End;
            token.Content = FastString.Empty;
            node.Token = token;

            return node;
        }

        private bool MatchTerminal(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            if (m_Source[sourceOffset + tokensConsumed].Type == token.Token)
            {
                node.AddTerminal(m_RootNode, m_Source[sourceOffset + tokensConsumed]);
                tokensConsumed++;
                LastMatched = sourceOffset + tokensConsumed;

                return true;
            }

            return false;
        }

        private bool MatchNonTerminal(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            var collapsableLevel = m_ParseRules[(int)token.Token].CollapsableLevel;

            foreach (var alternative in m_ParseRules[(int)token.Token].RequiredTokens)
            {
                if (MatchCondition(token, sourceOffset, alternative, collapsableLevel, ref node, ref tokensConsumed))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchRule(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            if (token.Type < ConditionType.OneOrMore)   // Either One or OptionalFromThis
            {
                return sourceOffset + tokensConsumed < m_Source.Length
                    && (token.Token.IsTerminal()
                        ? MatchTerminal(token, sourceOffset, ref node, ref tokensConsumed)
                        : MatchNonTerminal(token, sourceOffset, ref node, ref tokensConsumed));
            }

            return token.Token.IsTerminal()
                ? MatchTerminals(token, sourceOffset, ref node, ref tokensConsumed)
                : MatchNonTerminals(token, sourceOffset, ref node, ref tokensConsumed);
        }


        private bool MatchTerminals(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            bool success = token.Type == ConditionType.ZeroOrMore;

            while (sourceOffset + tokensConsumed < m_Source.Length
                && MatchTerminal(token, sourceOffset, ref node, ref tokensConsumed))
            {
                success = true;
            }

            return success;
        }


        private bool MatchNonTerminals(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            bool success = token.Type == ConditionType.ZeroOrMore;

            while (sourceOffset + tokensConsumed < m_Source.Length
                && MatchNonTerminal(token, sourceOffset, ref node, ref tokensConsumed))
            {
                success = true;
            }

            return success;
        }


        private bool MatchCondition(Condition token, int sourceOffset, Condition[] alternative, ParseRuleCollapsableLevel collapsableLevel,
            ref AstNode node, ref int tokensConsumed)
        {
            var lookupTokensConsumed = 0;
            AstNode matchedNode = Match(sourceOffset + tokensConsumed, alternative, ref lookupTokensConsumed);

            if (matchedNode.IsNull)
            {
                return false;
            }
            else
            {
                var childrenCount = matchedNode.ChildrenCount;

                if (collapsableLevel == ParseRuleCollapsableLevel.Always ||
                    (collapsableLevel == ParseRuleCollapsableLevel.OneChild && childrenCount == 1))
                {
                    for (int i = 0; i < childrenCount; i++)
                    {
                        node.AddChild(m_RootNode, matchedNode.Children[i]);
                    }
                }
                else
                {
                    matchedNode.Type = token.Token;
                    node.AddChild(m_RootNode, matchedNode);
                }

                tokensConsumed += lookupTokensConsumed;
                LastMatched = tokensConsumed;
                return true;
            }
        }

        private static Condition OneOrMore(Condition c)
        {
            return new Condition(c, ConditionType.OneOrMore);
        }

        private static Condition ZeroOrMore(Condition c)
        {
            return new Condition(c, ConditionType.ZeroOrMore);
        }

        private static ConditionList Optional(ConditionList conditions)
        {
#if DEBUG
            Debug.Assert(conditions.Count > 0);
#endif
            var firstCondition = conditions[0];
            firstCondition.Type = ConditionType.OptionalFromThis;
            conditions[0] = firstCondition;
            return conditions;
        }

        private static ParseRule ParseRule(Condition result, params List<Condition>[] requiredTokens)
        {
#if DEBUG
            Debug.Assert(requiredTokens.Length > 0);
#endif

            return new ParseRule(result, ParseRuleCollapsableLevel.Never, requiredTokens);
        }

        private static ParseRule CollapsableParseRule(Condition result, params List<Condition>[] requiredTokens)
        {
#if DEBUG
            Debug.Assert(requiredTokens.Length > 0);
#endif

            return new ParseRule(result, ParseRuleCollapsableLevel.OneChild, requiredTokens);
        }

        private static ParseRule AlwaysCollapsableParseRule(Condition result, List<Condition> requiredTokens)
        {
            return new ParseRule(result, ParseRuleCollapsableLevel.Always, requiredTokens);
        }

        private static ParseRule AlwaysCollapsableParseRule(Condition result, params List<Condition>[] requiredTokens)
        {
#if DEBUG
            Debug.Assert(requiredTokens.Length > 0);
#endif

            return new ParseRule(result, ParseRuleCollapsableLevel.Always, requiredTokens);
        }

        #region TokenProperties
        private static Condition EndOfLine { get { return TokenType.EndOfLine; } }
        private static Condition Comma { get { return TokenType.Comma; } }
        private static Condition Period { get { return TokenType.Period; } }
        private static Condition BitwiseAnd { get { return TokenType.BitwiseAnd; } }
        private static Condition BitwiseAndEqual { get { return TokenType.BitwiseAndEqual; } }
        private static Condition Plus { get { return TokenType.Plus; } }
        private static Condition PlusPlus { get { return TokenType.PlusPlus; } }
        private static Condition Minus { get { return TokenType.Minus; } }
        private static Condition MinusMinus { get { return TokenType.MinusMinus; } }
        private static Condition MinusEqual { get { return TokenType.MinusEqual; } }
        private static Condition NotEqual { get { return TokenType.NotEqual; } }
        private static Condition Not { get { return TokenType.Not; } }
        private static Condition PlusEqual { get { return TokenType.PlusEqual; } }
        private static Condition StringLiteral { get { return TokenType.StringLiteral; } }
        private static Condition BitwiseComplement { get { return TokenType.BitwiseComplement; } }
        private static Condition BitwiseXor { get { return TokenType.BitwiseXor; } }
        private static Condition BitwiseXorEqual { get { return TokenType.BitwiseXorEqual; } }
        private static Condition BitwiseOr { get { return TokenType.BitwiseOr; } }
        private static Condition BitwiseOrEqual { get { return TokenType.BitwiseOrEqual; } }
        private static Condition LeftShiftEqual { get { return TokenType.LeftShiftEqual; } }
        private static Condition LeftShift { get { return TokenType.LeftShift; } }
        private static Condition LessOrEqual { get { return TokenType.LessOrEqual; } }
        private static Condition Less { get { return TokenType.Less; } }
        private static Condition LogicalAnd { get { return TokenType.LogicalAnd; } }
        private static Condition LogicalAndEqual { get { return TokenType.LogicalAndEqual; } }
        private static Condition LogicalOr { get { return TokenType.LogicalOr; } }
        private static Condition LogicalOrEqual { get { return TokenType.LogicalOrEqual; } }
        private static Condition More { get { return TokenType.More; } }
        private static Condition RightShift { get { return TokenType.RightShift; } }
        private static Condition RightShiftEqual { get { return TokenType.RightShiftEqual; } }
        private static Condition MoreOrEqual { get { return TokenType.MoreOrEqual; } }
        private static Condition Divide { get { return TokenType.Divide; } }
        private static Condition DivideEqual { get { return TokenType.DivideEqual; } }
        private static Condition Multiply { get { return TokenType.Multiply; } }
        private static Condition MultiplyEqual { get { return TokenType.MultiplyEqual; } }
        private static Condition Remainder { get { return TokenType.Remainder; } }
        private static Condition RemainderEqual { get { return TokenType.RemainderEqual; } }
        private static Condition Assignment { get { return TokenType.Assignment; } }
        private static Condition Equal { get { return TokenType.Equal; } }
        private static Condition LeftCurlyBrace { get { return TokenType.LeftCurlyBrace; } }
        private static Condition RightCurlyBrace { get { return TokenType.RightCurlyBrace; } }
        private static Condition LeftParenthesis { get { return TokenType.LeftParenthesis; } }
        private static Condition RightParenthesis { get { return TokenType.RightParenthesis; } }
        private static Condition Integer { get { return TokenType.Integer; } }
        private static Condition Float { get { return TokenType.Float; } }
        private static Condition Long { get { return TokenType.Long; } }
        private static Condition Double { get { return TokenType.Double; } }
        private static Condition Symbol { get { return TokenType.Symbol; } }
        private static Condition Const { get { return TokenType.Const; } }
        private static Condition Else { get { return TokenType.Else; } }
        private static Condition False { get { return TokenType.False; } }
        private static Condition If { get { return TokenType.If; } }
        private static Condition Internal { get { return TokenType.Internal; } }

        private static Condition Protected { get { return TokenType.Protected; } }
        private static Condition Return { get { return TokenType.Return; } }
        private static Condition True { get { return TokenType.True; } }
        private static Condition Use { get { return TokenType.Use; } }
        private static Condition Virtual { get { return TokenType.Virtual; } }
        private static Condition While { get { return TokenType.While; } }
        private static Condition NoInstance { get { return TokenType.NoInstance; } }
        private static Condition Private { get { return TokenType.Private; } }
        private static Condition Public { get { return TokenType.Public; } }
        private static Condition NonTerminalToken { get { return TokenType.NonTerminalToken; } }
        private static Condition StatementNode { get { return TokenType.StatementNode; } }
        private static Condition CodeBlockNode { get { return TokenType.CodeBlockNode; } }
        private static Condition DeclarationNode { get { return TokenType.DeclarationNode; } }
        private static Condition DeclarationSubnode { get { return TokenType.DeclarationSubnode; } }
        private static Condition UseNode { get { return TokenType.UseNode; } }
        private static Condition ValueStatementNode { get { return TokenType.ValueStatementNode; } }
        private static Condition ReturnNode { get { return TokenType.ReturnNode; } }
        private static Condition RootNode { get { return TokenType.RootNode; } }
        private static Condition FullSymbol { get { return TokenType.FullSymbol; } }
        private static Condition SubSymbol { get { return TokenType.SubSymbol; } }
        private static Condition Value { get { return TokenType.Value; } }
        private static Condition Type { get { return TokenType.Type; } }
        public static Condition TypeParameters { get { return TokenType.TypeParameters; } }
        private static Condition VariableModifier { get { return TokenType.VariableModifier; } }
        private static Condition WhileLoop { get { return TokenType.WhileLoop; } }
        private static Condition TypeSubnode { get { return TokenType.TypeSubnode; } }
        private static Condition TypeAndSymbolSubnode { get { return TokenType.TypeAndSymbolSubnode; } }        
        private static Condition Function { get { return TokenType.Function; } }
        private static Condition ConditionalSentence { get { return TokenType.ConditionalSentence; } }
        private static Condition AssignmentOperator { get { return TokenType.AssignmentOperator; } }
        private static Condition CommaAndValue { get { return TokenType.CommaAndValue; } }
        private static Condition AssignmentOperatorNode { get { return TokenType.AssignmentOperatorNode; } }
        private static Condition LogicalOrNode { get { return TokenType.LogicalOrNode; } }
        private static Condition LogicalOrSubnode { get { return TokenType.LogicalOrSubnode; } }
        private static Condition LogicalAndNode { get { return TokenType.LogicalAndNode; } }
        private static Condition LogicalAndSubnode { get { return TokenType.LogicalAndSubnode; } }
        private static Condition BitwiseOrNode { get { return TokenType.BitwiseOrNode; } }
        private static Condition BitwiseOrSubnode { get { return TokenType.BitwiseOrSubnode; } }
        private static Condition BitwiseXorNode { get { return TokenType.BitwiseXorNode; } }
        private static Condition BitwiseXorSubnode { get { return TokenType.BitwiseXorSubnode; } }
        private static Condition BitwiseAndNode { get { return TokenType.BitwiseAndNode; } }
        private static Condition BitwiseAndSubnode { get { return TokenType.BitwiseAndSubnode; } }

        private static Condition EqualityOperatorNode { get { return TokenType.EqualityOperatorNode; } }
        private static Condition RelationalOperatorNode { get { return TokenType.RelationalOperatorNode; } }
        private static Condition ShiftOperatorNode { get { return TokenType.ShiftOperatorNode; } }
        private static Condition AdditiveOperatorNode { get { return TokenType.AdditiveOperatorNode; } }
        private static Condition MultiplicativeOperatorNode { get { return TokenType.MultiplicativeOperatorNode; } }
        private static Condition ParenthesesNode { get { return TokenType.ParenthesesNode; } }

        private static Condition MultiplicativeOperatorSubnode { get { return TokenType.MultiplicativeOperatorSubnode; } }
        private static Condition AdditiveOperatorSubnode { get { return TokenType.AdditiveOperatorSubnode; } }
        private static Condition ShiftOperatorSubnode { get { return TokenType.ShiftOperatorSubnode; } }
        private static Condition RelationalOperatorSubnode { get { return TokenType.RelationalOperatorSubnode; } }
        private static Condition EqualityOperatorSubnode { get { return TokenType.EqualityOperatorSubnode; } }
        
        private static Condition EqualityOperator { get { return TokenType.EqualityOperator; } }
        private static Condition RelationalOperator { get { return TokenType.RelationalOperator; } }
        private static Condition ShiftOperator { get { return TokenType.ShiftOperator; } }
        private static Condition AdditiveOperator { get { return TokenType.AdditiveOperator; } }
        private static Condition MultiplicativeOperator { get { return TokenType.MultiplicativeOperator; } }

        private static Condition PeriodNode { get { return TokenType.PeriodNode; } }
        private static Condition PeriodSubnode { get { return TokenType.PeriodSubnode; } }
        private static Condition Operand { get { return TokenType.Operand; } }
        private static Condition PrefixNode { get { return TokenType.PrefixNode; } }
        private static Condition PostfixNode { get { return TokenType.PostfixNode; } }
        private static Condition PostfixOperator { get { return TokenType.PostfixOperator; } }
        private static Condition PrefixOperator { get { return TokenType.PrefixOperator; } }
        private static Condition InlineFunctionCallNode { get { return TokenType.InlineFunctionCallNode; } }
        private static Condition FunctionCallNode { get { return TokenType.FunctionCallNode; } }
        private static Condition FunctionArgumentsList { get { return TokenType.FunctionArgumentsList; } }
        private static Condition LiteralNode { get { return TokenType.LiteralNode; } }
        private static Condition Entry { get { return TokenType.Entry; } }
        private static Condition Mutable { get { return TokenType.Mutable; } }
        private static Condition UnknownNode { get { return TokenType.UnknownNode; } }
        private static Condition ArrayLiteral { get { return TokenType.ArrayLiteral; } }
        private static Condition IndexNode { get { return TokenType.IndexNode; } }
        private static Condition IndexAccessNode { get { return TokenType.IndexAccessNode; } }
        private static Condition RightBracket { get { return TokenType.RightBracket; } }
        private static Condition LeftBracket { get { return TokenType.LeftBracket; } }
        #endregion

    }
}
