//#define USE_LOOKUP

using Lexer.Containers;
using System;
using System.Diagnostics;

namespace Lexer
{
    internal sealed class SyntaxMatcher
    {
        private static ParseRule[] m_ParseRules;
        private Token[] m_Source;
        private RootNode m_RootNode;

#if USE_LOOKUP
        private Dictionary<Tuple<IEnumerable<Condition>, int>, MatchResult> m_ParsingResults;
#endif

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
        
#if USE_LOOKUP
        class ConditionListComparer : IEqualityComparer<Tuple<IEnumerable<Condition>, int>>
        {
            public bool Equals(Tuple<IEnumerable<Condition>, int> x, Tuple<IEnumerable<Condition>, int> y)
            {
                return x.Item2 == y.Item2 && x.Item1.SequenceEqual(y.Item1);
            }

            public int GetHashCode(Tuple<IEnumerable<Condition>, int> obj)
            {
                int hashcode = obj.Item2;
                foreach (Condition t in obj.Item1)
                {
                    hashcode ^= t.GetHashCode();
                }
                return hashcode;
            }
        }
#endif
                        
        #region TokenProperties
        private static Condition EndOfLine { get { return TokenType.EndOfLine; } }
        private static Condition Comma { get { return TokenType.Comma; } }
        private static Condition Period { get { return TokenType.Period; } }
        private static Condition Comment { get { return TokenType.Comment; } }
        private static Condition BitwiseAnd { get { return TokenType.BitwiseAnd; } }
        private static Condition BitwiseAndEqual { get { return TokenType.BitwiseAndEqual; } }
        private static Condition And { get { return TokenType.And; } }
        private static Condition Plus { get { return TokenType.Plus; } }
        private static Condition PlusPlus { get { return TokenType.PlusPlus; } }
        private static Condition Minus { get { return TokenType.Minus; } }
        private static Condition MinusMinus { get { return TokenType.MinusMinus; } }
        private static Condition MinusEqual { get { return TokenType.MinusEqual; } }
        private static Condition NotEqual { get { return TokenType.NotEqual; } }
        private static Condition Not { get { return TokenType.Not; } }
        private static Condition Whitespace { get { return TokenType.Whitespace; } }
        private static Condition PlusEqual { get { return TokenType.PlusEqual; } }
        private static Condition StringLiteral { get { return TokenType.StringLiteral; } }
        private static Condition BitwiseComplementEqual { get { return TokenType.BitwiseComplementEqual; } }
        private static Condition BitwiseComplement { get { return TokenType.BitwiseComplement; } }
        private static Condition BitwiseXor { get { return TokenType.BitwiseXor; } }
        private static Condition BitwiseXorEqual { get { return TokenType.BitwiseXorEqual; } }
        private static Condition BitwiseOr { get { return TokenType.BitwiseOr; } }
        private static Condition Or { get { return TokenType.Or; } }
        private static Condition BitwiseOrEqual { get { return TokenType.BitwiseOrEqual; } }
        private static Condition LeftShiftEqual { get { return TokenType.LeftShiftEqual; } }
        private static Condition LeftShift { get { return TokenType.LeftShift; } }
        private static Condition LessOrEqual { get { return TokenType.LessOrEqual; } }
        private static Condition Less { get { return TokenType.Less; } }
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
        private static Condition LeftCurlyBracket { get { return TokenType.LeftCurlyBracket; } }
        private static Condition RightCurlyBracket { get { return TokenType.RightCurlyBracket; } }
        private static Condition LeftBracket { get { return TokenType.LeftBracket; } }
        private static Condition RightBracket { get { return TokenType.RightBracket; } }
        private static Condition Unknown { get { return TokenType.Unknown; } }
        private static Condition Integer { get { return TokenType.Integer; } }
        private static Condition Float { get { return TokenType.Float; } }
        private static Condition Long { get { return TokenType.Long; } }
        private static Condition Double { get { return TokenType.Double; } }
        private static Condition MalformedToken { get { return TokenType.MalformedToken; } }
        private static Condition Symbol { get { return TokenType.Symbol; } }
        private static Condition Abstract { get { return TokenType.Abstract; } }
        private static Condition As { get { return TokenType.As; } }
        private static Condition Base { get { return TokenType.Base; } }
        private static Condition Break { get { return TokenType.Break; } }
        private static Condition Case { get { return TokenType.Case; } }
        private static Condition Catch { get { return TokenType.Catch; } }
        private static Condition Class { get { return TokenType.Class; } }
        private static Condition Const { get { return TokenType.Const; } }
        private static Condition Continue { get { return TokenType.Continue; } }
        private static Condition Default { get { return TokenType.Default; } }
        private static Condition Do { get { return TokenType.Do; } }
        private static Condition Extern { get { return TokenType.Extern; } }
        private static Condition Else { get { return TokenType.Else; } }
        private static Condition Enum { get { return TokenType.Enum; } }
        private static Condition False { get { return TokenType.False; } }
        private static Condition Finally { get { return TokenType.Finally; } }
        private static Condition For { get { return TokenType.For; } }
        private static Condition Goto { get { return TokenType.Goto; } }
        private static Condition If { get { return TokenType.If; } }
        private static Condition Interface { get { return TokenType.Interface; } }
        private static Condition Internal { get { return TokenType.Internal; } }
        private static Condition Is { get { return TokenType.Is; } }
        private static Condition New { get { return TokenType.New; } }
        private static Condition Null { get { return TokenType.Null; } }
        private static Condition Namespace { get { return TokenType.Namespace; } }
        private static Condition Operator { get { return TokenType.Operator; } }
        private static Condition Out { get { return TokenType.Out; } }
        private static Condition Override { get { return TokenType.Override; } }
        private static Condition Protected { get { return TokenType.Protected; } }
        private static Condition Ref { get { return TokenType.Ref; } }
        private static Condition Return { get { return TokenType.Return; } }
        private static Condition Switch { get { return TokenType.Switch; } }
        private static Condition Sealed { get { return TokenType.Sealed; } }
        private static Condition This { get { return TokenType.This; } }
        private static Condition Throw { get { return TokenType.Throw; } }
        private static Condition Struct { get { return TokenType.Struct; } }
        private static Condition True { get { return TokenType.True; } }
        private static Condition Try { get { return TokenType.Try; } }
        private static Condition Using { get { return TokenType.Using; } }
        private static Condition Virtual { get { return TokenType.Virtual; } }
        private static Condition While { get { return TokenType.While; } }
        private static Condition Static { get { return TokenType.Static; } }
        private static Condition Constant { get { return TokenType.Constant; } }
        private static Condition Private { get { return TokenType.Private; } }
        private static Condition Public { get { return TokenType.Public; } }
        private static Condition NonTerminalToken { get { return TokenType.NonTerminalToken; } }
        private static Condition StatementNode { get { return TokenType.StatementNode; } }
        private static Condition CodeBlockNode { get { return TokenType.CodeBlockNode; } }
        private static Condition DeclarationNode { get { return TokenType.DeclarationNode; } }
        private static Condition AssignmentNode { get { return TokenType.AssignmentNode; } }
        private static Condition RootNode { get { return TokenType.RootNode; } }
        private static Condition FullSymbol { get { return TokenType.FullSymbol; } }
        private static Condition SubSymbol { get { return TokenType.SubSymbol; } }
        private static Condition Value { get { return TokenType.Value; } }
        private static Condition LValue { get { return TokenType.LValue; } }
        private static Condition RValue { get { return TokenType.RValue; } }
        private static Condition Type { get { return TokenType.Type; } }
        private static Condition VariableModifier { get { return TokenType.VariableModifier; } }
        private static Condition FunctionCall { get { return TokenType.FunctionCall; } }
        private static Condition FunctionArgument { get { return TokenType.FunctionArgument; } }
        private static Condition FunctionDeclarationArgument { get { return TokenType.FunctionDeclarationArgument; } }
        private static Condition FunctionBody { get { return TokenType.FunctionBody; } }
        private static Condition WhileLoop { get { return TokenType.WhileLoop; } }
        private static Condition ArithmeticNode { get { return TokenType.ArithmeticNode; } }
        private static Condition ArithmeticSubnode { get { return TokenType.ArithmeticSubnode; } }
        private static Condition TypeArgument { get { return TokenType.TypeArgument; } }
        private static Condition Function { get { return TokenType.Function; } }
        private static Condition ConditionalSentence { get { return TokenType.ConditionalSentence; } }
        private static Condition AssignmentOperator { get { return TokenType.AssignmentOperator; } }
        private static Condition FunctionSegment { get { return TokenType.FunctionSegment; } }
        private static Condition FunctionArgumentList { get { return TokenType.FunctionArgumentList; } }
        #endregion

        static SyntaxMatcher()
        {
            m_ParseRules = new ParseRule[(int)TokenType.TokenTypeCount];
            ParseRule[] AllRules = 
            {
                #region Syntax rules
                new ParseRule(StatementNode,       
                    DeclarationNode,                                 
                    FunctionCall + EndOfLine,
                    AssignmentNode + EndOfLine,
                    CodeBlockNode,
                    WhileLoop,
                    Return + Value + EndOfLine,
                    ConditionalSentence
                    ),
            
                new ParseRule(DeclarationNode,
                    OneOrMore(VariableModifier) + Type + FullSymbol + EndOfLine,
                    OneOrMore(VariableModifier) + Type + FullSymbol + Assignment + Value + EndOfLine,
                    Type + FullSymbol + EndOfLine,
                    Type + FullSymbol + Assignment + Value + EndOfLine ),
            
                new ParseRule(VariableModifier, 
                    Const,
                    Internal,
                    Private,
                    Public,
                    Protected,
                    Static,
                    Virtual),

                new ParseRule(AssignmentNode,
                    LValue + AssignmentOperator + Value,
                    FunctionCall + AssignmentOperator + Value),

            
                new ParseRule(AssignmentOperator,
                    Assignment,
                    BitwiseAndEqual,
                    MinusEqual,
                    NotEqual,
                    PlusEqual,
                    BitwiseComplementEqual,
                    BitwiseXorEqual,
                    BitwiseOrEqual,
                    LeftShiftEqual,
                    LessOrEqual,
                    RightShiftEqual,
                    MoreOrEqual,
                    DivideEqual,
                    MultiplyEqual,
                    RemainderEqual,
                    Equal),

                new ParseRule(CodeBlockNode,
                    LeftCurlyBracket + OneOrMore(StatementNode) + RightCurlyBracket,
                    LeftCurlyBracket + RightCurlyBracket),

                new ParseRule(Value,                                                                               
                    OneOrMore(ArithmeticNode),                    
                    RValue,           
                    LValue),

                new ParseRule(LValue,
                    FullSymbol),
 
                new ParseRule(RValue,                     
                    AssignmentNode,
                    Function,
                    FullSymbol,
                    Float,
                    Integer,
                    Double,
                    Long,
                    StringLiteral,
                    True,
                    False,                    
                    FunctionCall),

                new ParseRule(FunctionCall,
                    OneOrMore(FunctionSegment)),


                new ParseRule(FunctionSegment,
                    Period,
                    FunctionArgumentList,
                    Function,
                    LeftBracket + ArithmeticNode + RightBracket,
                    Symbol),

                new ParseRule(FunctionArgumentList,
                    LeftBracket + RightBracket,
                    LeftBracket + Value + ZeroOrMore(FunctionArgument) + RightBracket
                    ),

                new ParseRule(FunctionArgument,
                    Comma + Value),

                new ParseRule(FullSymbol,
                    Symbol + OneOrMore(SubSymbol),
                    Symbol),

                new ParseRule(SubSymbol,
                    Period + Symbol),

                new ParseRule(Type,
                    FullSymbol + LeftBracket + Type + ZeroOrMore(TypeArgument) + RightBracket,
                    FullSymbol + LeftBracket + Type + FullSymbol + ZeroOrMore(TypeArgument) + RightBracket,
                    FullSymbol + LeftBracket + RightBracket,
                    FullSymbol),

                new ParseRule(TypeArgument,
                    Comma + Type + FullSymbol,
                    Comma + Type),

                new ParseRule(Function,
                    Type + CodeBlockNode),
                    

                new ParseRule(WhileLoop,
                    While + LeftBracket + Value + RightBracket + StatementNode),

                new ParseRule(Operator,
                    Plus, Minus, PlusPlus, MinusMinus, Multiply, Divide, Remainder, BitwiseXor, BitwiseOr, BitwiseComplement, BitwiseXor, Or, And, BitwiseAnd, Not, LeftShift, RightShift, Equal, NotEqual, More, MoreOrEqual, Less, LessOrEqual),

                new ParseRule(ArithmeticNode,
                    LeftBracket + OneOrMore(ArithmeticNode) + RightBracket,
                    OneOrMore(ArithmeticSubnode)),

                new ParseRule(ArithmeticSubnode,
                    RValue,
                    Operator),

                new ParseRule(ConditionalSentence,
                    If + LeftBracket + Value + RightBracket + StatementNode + Else + StatementNode,
                    If + LeftBracket + Value + RightBracket + StatementNode),
                #endregion
            };

            foreach (var rule in AllRules)
            {
                m_ParseRules[(int)rule.Result] = rule;
            }
        }

        public SyntaxMatcher(Token[] sourceTokens, RootNode rootNode)
        {
#if USE_LOOKUP
            m_ParsingResults = new Dictionary<Tuple<IEnumerable<Condition>, int>, MatchResult>(new ConditionListComparer());
#endif
            m_RootNode = rootNode;
            m_Source = sourceTokens;
        }

        public AstNode Match()
        {
            var tokensConsumed = 0;

            AstNode matchedNode = Match(0, new Condition[] { new Condition(TokenType.StatementNode, ConditionType.OneOrMore) }, ref tokensConsumed);

            if (matchedNode.IsNull || tokensConsumed != m_Source.Length)
            {
                throw new Exception(String.Format("Could not match all  tokens, last matched token {0} - {1}, line {2}, column {3}", LastMatched, m_Source[LastMatched - 1].Content, m_Source[LastMatched - 1].Start.Row, m_Source[LastMatched - 1].Start.Column));
            }

            matchedNode.Type = TokenType.RootNode;
            m_RootNode.SetNode(matchedNode);
            return matchedNode;
        }

        private AstNode MatchWithLookup(int sourceOffset, Condition[] rule, ref int tokensConsumed)
        {
#if !USE_LOOKUP
            return Match(sourceOffset, rule, ref tokensConsumed);
#else
            MatchResult value = new MatchResult(new AstNode(null, Unknown.Token), 0);
            if (m_ParsingResults.TryGetValue(new Tuple<IEnumerable<Condition>, int>(rule, sourceOffset), out value))
            {
                return value;
            }
            else
            {
                var result = Match(sourceOffset, rule);
                m_ParsingResults[new Tuple<IEnumerable<Condition>, int>(rule, sourceOffset)] = result;
                return result;
            }
#endif
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
                    node.Cleanup(m_RootNode);
                    return default(AstNode);
                }
            }

            var token = m_RootNode.ProvideToken();
            token.Start = node.Children[0].Token.Start;
            token.End = node.Children[node.ChildrenCount - 1].Token.End;
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
            foreach (var alternative in m_ParseRules[(int)token.Token].RequiredTokens)
            {
                if (MatchCondition(token, sourceOffset, alternative, ref node, ref tokensConsumed))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchRule(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            if (token.Type == ConditionType.One)
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


        private bool MatchCondition(Condition token, int sourceOffset, Condition[] alternative, ref AstNode node, ref int tokensConsumed)
        {
            var lookupTokensConsumed = 0;
            AstNode matchedNode = MatchWithLookup(sourceOffset + tokensConsumed, alternative, ref lookupTokensConsumed);

            if (matchedNode.IsNull)
            {
                return false;
            }
            else
            {
                matchedNode.Type = token.Token;
                node.AddChild(m_RootNode, matchedNode);
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
    }
}
