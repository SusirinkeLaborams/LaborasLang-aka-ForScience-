//#define USE_LOOKUP

using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    internal sealed class SyntaxMatcher
    {
        private ParseRule[] m_ParseRules;
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
        private Condition EndOfLine { get { return TokenType.EndOfLine; } }
        private Condition Comma { get { return TokenType.Comma; } }
        private Condition Period { get { return TokenType.Period; } }
        private Condition Comment { get { return TokenType.Comment; } }
        private Condition BitwiseAnd { get { return TokenType.BitwiseAnd; } }
        private Condition BitwiseAndEqual { get { return TokenType.BitwiseAndEqual; } }
        private Condition And { get { return TokenType.And; } }
        private Condition Plus { get { return TokenType.Plus; } }
        private Condition PlusPlus { get { return TokenType.PlusPlus; } }
        private Condition Minus { get { return TokenType.Minus; } }
        private Condition MinusMinus { get { return TokenType.MinusMinus; } }
        private Condition MinusEqual { get { return TokenType.MinusEqual; } }
        private Condition NotEqual { get { return TokenType.NotEqual; } }
        private Condition Not { get { return TokenType.Not; } }
        private Condition Whitespace { get { return TokenType.Whitespace; } }
        private Condition PlusEqual { get { return TokenType.PlusEqual; } }
        private Condition StringLiteral { get { return TokenType.StringLiteral; } }
        private Condition BitwiseComplementEqual { get { return TokenType.BitwiseComplementEqual; } }
        private Condition BitwiseComplement { get { return TokenType.BitwiseComplement; } }
        private Condition BitwiseXor { get { return TokenType.BitwiseXor; } }
        private Condition BitwiseXorEqual { get { return TokenType.BitwiseXorEqual; } }
        private Condition BitwiseOr { get { return TokenType.BitwiseOr; } }
        private Condition Or { get { return TokenType.Or; } }
        private Condition BitwiseOrEqual { get { return TokenType.BitwiseOrEqual; } }
        private Condition LeftShiftEqual { get { return TokenType.LeftShiftEqual; } }
        private Condition LeftShift { get { return TokenType.LeftShift; } }
        private Condition LessOrEqual { get { return TokenType.LessOrEqual; } }
        private Condition Less { get { return TokenType.Less; } }
        private Condition More { get { return TokenType.More; } }
        private Condition RightShift { get { return TokenType.RightShift; } }
        private Condition RightShiftEqual { get { return TokenType.RightShiftEqual; } }
        private Condition MoreOrEqual { get { return TokenType.MoreOrEqual; } }
        private Condition Divide { get { return TokenType.Divide; } }
        private Condition DivideEqual { get { return TokenType.DivideEqual; } }
        private Condition Multiply { get { return TokenType.Multiply; } }
        private Condition MultiplyEqual { get { return TokenType.MultiplyEqual; } }
        private Condition Remainder { get { return TokenType.Remainder; } }
        private Condition RemainderEqual { get { return TokenType.RemainderEqual; } }
        private Condition Assignment { get { return TokenType.Assignment; } }
        private Condition Equal { get { return TokenType.Equal; } }
        private Condition LeftCurlyBracket { get { return TokenType.LeftCurlyBracket; } }
        private Condition RightCurlyBracket { get { return TokenType.RightCurlyBracket; } }
        private Condition LeftBracket { get { return TokenType.LeftBracket; } }
        private Condition RightBracket { get { return TokenType.RightBracket; } }
        private Condition Unknown { get { return TokenType.Unknown; } }
        private Condition Integer { get { return TokenType.Integer; } }
        private Condition Float { get { return TokenType.Float; } }
        private Condition Long { get { return TokenType.Long; } }
        private Condition Double { get { return TokenType.Double; } }
        private Condition MalformedToken { get { return TokenType.MalformedToken; } }
        private Condition Symbol { get { return TokenType.Symbol; } }
        private Condition Abstract { get { return TokenType.Abstract; } }
        private Condition As { get { return TokenType.As; } }
        private Condition Base { get { return TokenType.Base; } }
        private Condition Break { get { return TokenType.Break; } }
        private Condition Case { get { return TokenType.Case; } }
        private Condition Catch { get { return TokenType.Catch; } }
        private Condition Class { get { return TokenType.Class; } }
        private Condition Const { get { return TokenType.Const; } }
        private Condition Continue { get { return TokenType.Continue; } }
        private Condition Default { get { return TokenType.Default; } }
        private Condition Do { get { return TokenType.Do; } }
        private Condition Extern { get { return TokenType.Extern; } }
        private Condition Else { get { return TokenType.Else; } }
        private Condition Enum { get { return TokenType.Enum; } }
        private Condition False { get { return TokenType.False; } }
        private Condition Finally { get { return TokenType.Finally; } }
        private Condition For { get { return TokenType.For; } }
        private Condition Goto { get { return TokenType.Goto; } }
        private Condition If { get { return TokenType.If; } }
        private Condition Interface { get { return TokenType.Interface; } }
        private Condition Internal { get { return TokenType.Internal; } }
        private Condition Is { get { return TokenType.Is; } }
        private Condition New { get { return TokenType.New; } }
        private Condition Null { get { return TokenType.Null; } }
        private Condition Namespace { get { return TokenType.Namespace; } }
        private Condition Operator { get { return TokenType.Operator; } }
        private Condition Out { get { return TokenType.Out; } }
        private Condition Override { get { return TokenType.Override; } }
        private Condition Protected { get { return TokenType.Protected; } }
        private Condition Ref { get { return TokenType.Ref; } }
        private Condition Return { get { return TokenType.Return; } }
        private Condition Switch { get { return TokenType.Switch; } }
        private Condition Sealed { get { return TokenType.Sealed; } }
        private Condition This { get { return TokenType.This; } }
        private Condition Throw { get { return TokenType.Throw; } }
        private Condition Struct { get { return TokenType.Struct; } }
        private Condition True { get { return TokenType.True; } }
        private Condition Try { get { return TokenType.Try; } }
        private Condition Using { get { return TokenType.Using; } }
        private Condition Virtual { get { return TokenType.Virtual; } }
        private Condition While { get { return TokenType.While; } }
        private Condition Static { get { return TokenType.Static; } }
        private Condition Constant { get { return TokenType.Constant; } }
        private Condition Private { get { return TokenType.Private; } }
        private Condition Public { get { return TokenType.Public; } }
        private Condition NonTerminalToken { get { return TokenType.NonTerminalToken; } }
        private Condition StatementNode { get { return TokenType.StatementNode; } }
        private Condition CodeBlockNode { get { return TokenType.CodeBlockNode; } }
        private Condition DeclarationNode { get { return TokenType.DeclarationNode; } }
        private Condition AssignmentNode { get { return TokenType.AssignmentNode; } }
        private Condition RootNode { get { return TokenType.RootNode; } }
        private Condition FullSymbol { get { return TokenType.FullSymbol; } }
        private Condition SubSymbol { get { return TokenType.SubSymbol; } }
        private Condition Value { get { return TokenType.Value; } }
        private Condition LValue { get { return TokenType.LValue; } }
        private Condition RValue { get { return TokenType.RValue; } }
        private Condition Type { get { return TokenType.Type; } }
        private Condition VariableModifier { get { return TokenType.VariableModifier; } }
        private Condition FunctionCall { get { return TokenType.FunctionCall; } }
        private Condition FunctionArgument { get { return TokenType.FunctionArgument; } }
        private Condition FunctionDeclarationArgument { get { return TokenType.FunctionDeclarationArgument; } }
        private Condition FunctionBody { get { return TokenType.FunctionBody; } }
        private Condition WhileLoop { get { return TokenType.WhileLoop; } }
        private Condition ArithmeticNode { get { return TokenType.ArithmeticNode; } }
        private Condition ArithmeticSubnode { get { return TokenType.ArithmeticSubnode; } }
        private Condition TypeArgument { get { return TokenType.TypeArgument; } }
        private Condition Function { get { return TokenType.Function; } }
        private Condition ConditionalSentence { get { return TokenType.ConditionalSentence; } }
        private Condition AssignmentOperator { get { return TokenType.AssignmentOperator; } }
        #endregion

        public SyntaxMatcher(Token[] sourceTokens, RootNode rootNode)
        {
#if USE_LOOKUP
            m_ParsingResults = new Dictionary<Tuple<IEnumerable<Condition>, int>, MatchResult>(new ConditionListComparer());
#endif
            m_ParseRules = new ParseRule[(int)TokenType.TokenTypeCount];
            m_RootNode = rootNode;

            ParseRule[] AllRules = 
            {
                #region Syntax rules
                new ParseRule(StatementNode,                    
                    FunctionCall + EndOfLine,
                    DeclarationNode,
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
                    LValue + AssignmentOperator + Value),

            
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
                    FunctionCall,
                    FullSymbol,
                    Float,
                    Integer,
                    Double,
                    Long,
                    StringLiteral,
                    True,
                    False),

                new ParseRule(FunctionCall,
                    FullSymbol + LeftBracket + RightBracket,
                    FullSymbol + LeftBracket + Value + OneOrMore(FunctionArgument) + RightBracket,
                    FullSymbol + LeftBracket + Value + RightBracket),

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

            m_Source = sourceTokens;
        }

        public AstNode Match()
        {
            var tokensConsumed = 0;

            AstNode matchedNode = Match(0, new Condition[] { new Condition(TokenType.StatementNode, ConditionType.OneOrMore) }, ref tokensConsumed);

            if (matchedNode.IsNull() || tokensConsumed != m_Source.Length)
            {
                throw new Exception(String.Format("Could not match all  tokens, last matched token {0} - {1}, line {2}, column {3}", LastMatched, m_Source[LastMatched - 1].Content, m_Source[LastMatched - 1].Start.Row, m_Source[LastMatched - 1].Start.Column));
            }

            matchedNode.Type = TokenType.RootNode;
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

            if (matchedNode.IsNull())
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

        private Condition OneOrMore(Condition c)
        {
            return new Condition(c, ConditionType.OneOrMore);
        }

        private Condition ZeroOrMore(Condition c)
        {
            return new Condition(c, ConditionType.ZeroOrMore);
        }
    }
}
