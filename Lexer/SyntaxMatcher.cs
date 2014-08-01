using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public class SyntaxMatcher
    {
        private Dictionary<TokenType, ParseRule> m_ParseRules;
        private List<Token> m_Source;

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
        #endregion

        public SyntaxMatcher(IEnumerable<Token> sourceTokens)
        {
            m_ParseRules = new Dictionary<TokenType, ParseRule>();
            
            ParseRule[] AllRules = 
            {
                #region Syntax rules
                new ParseRule(StatementNode,                    
                    RValue + EndOfLine,
                    DeclarationNode,
                    AssignmentNode,
                    CodeBlockNode,
                    WhileLoop),
            
                new ParseRule(DeclarationNode,
                    OneOrMore(VariableModifier) + Type + Symbol + EndOfLine,
                    Type + Symbol + EndOfLine),
            
                new ParseRule(VariableModifier, 
                    Const,
                    Internal,
                    Private,
                    Public,
                    Protected,
                    Static,
                    Virtual),

                new ParseRule(AssignmentNode,
                    LValue + Assignment + Value + EndOfLine),
            
                new ParseRule(CodeBlockNode,
                    LeftCurlyBracket + OneOrMore(StatementNode) + RightCurlyBracket,
                    LeftCurlyBracket + RightCurlyBracket),

                new ParseRule(Value,  
                    RValue,
                    LValue,                                      
                    ArithmeticNode),

                new ParseRule(LValue,
                    FullSymbol),
 
                new ParseRule(RValue,                    
                    FunctionCall,
                    FullSymbol,
                    Float,
                    Integer,
                    Double,
                    Long),

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
                    FullSymbol + LeftBracket + OneOrMore(Type) + RightBracket,
                    FullSymbol + LeftBracket + RightBracket,
                    FullSymbol),


                new ParseRule(WhileLoop,
                    While + LeftBracket + Value + RightBracket + StatementNode),

                new ParseRule(Operator,
                    Plus, Minus, PlusPlus, MinusMinus, Multiply, Divide, Remainder, BitwiseXor, BitwiseOr, BitwiseComplement, BitwiseXor, Or, And, BitwiseAnd, Not, LeftBracket, RightBracket, LeftShift, RightShift),

                new ParseRule(ArithmeticNode,
                    OneOrMore(ArithmeticSubnode)),

                new ParseRule(ArithmeticSubnode,
                     ZeroOrMore(Operator) + RValue + ZeroOrMore(Operator)),
                #endregion
            };

            foreach(var rule in AllRules)
            {
                m_ParseRules.Add(rule.Result, rule);
            }

            m_Source = sourceTokens.ToList();
        }

        public AstNode Match()
        {
            var tokensConsumed = 0;

            Tuple<AstNode, int> matchedNode = Match(tokensConsumed, new Condition[]{new Condition(TokenType.StatementNode, ConditionType.ZeroOrMore)}.ToList());
            if (matchedNode.Item1 != null)
            {
                matchedNode.Item1.Type = TokenType.RootNode;
                tokensConsumed += matchedNode.Item2;
            }

            if(tokensConsumed != m_Source.Count)
            {
                throw new Exception("Could not match all  tokens");
            }
            return matchedNode.Item1;            
        }

        private Tuple<AstNode, int> Match(int sourceOffset, List<Condition> rule)
        {
            var node = new AstNode();
            
            int tokensConsumed = 0;
            foreach(var token in rule)
            {
                if(token.Token.IsTerminal())
                {
                    if (m_Source[sourceOffset + tokensConsumed].Type == token.Token)
                    {
                        node.AddTerminal(m_Source[sourceOffset + tokensConsumed]);
                        tokensConsumed++; 
                    }
                    else
                    {
                        if (token.Type != ConditionType.ZeroOrMore)
                        {
                            return new Tuple<AstNode, int>(null, 0);
                        }
                    }
                }
                else
                {
                    bool matchFound = false;
                    foreach (var alternative in m_ParseRules[token.Token].RequiredTokens)
                    {
                                                
                        Tuple<AstNode, int> matchedNode = Match(sourceOffset + tokensConsumed, alternative);
                        var subnode = matchedNode.Item1;
                        var consumed = matchedNode.Item2;
                        if(subnode == null)
                        {
                            continue;
                        }
                        else
                        {
                            subnode.Type = token.Token;
                            node.AddChild(subnode);
                            tokensConsumed += consumed;
                            matchFound = true;
                            break;
                        }
                        
                    }
                    if(!matchFound)
                    {
                        if (token.Type != ConditionType.ZeroOrMore)
                        {
                            return new Tuple<AstNode, int>(null, 0);
                        }
                    }
                }
                
                // Second match for same token is optional, it should not return return on failure as it would discard first result, just stop matching
                if (token.Type == ConditionType.OneOrMore || token.Type == ConditionType.ZeroOrMore)
                {
                    while (sourceOffset + tokensConsumed < m_Source.Count)
                    {
                        if (token.Token.IsTerminal())
                        {
                            if (m_Source[sourceOffset + tokensConsumed].Type == token.Token)
                            {
                                node.AddTerminal(m_Source[sourceOffset + tokensConsumed]);
                                tokensConsumed++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            bool matchFound = false;
                            foreach (var alternative in m_ParseRules[token.Token].RequiredTokens)
                            {

                                Tuple<AstNode, int> matchedNode = Match(sourceOffset + tokensConsumed, alternative);
                                var subnode = matchedNode.Item1;
                                var consumed = matchedNode.Item2;
                                if (subnode == null)
                                {
                                    continue;
                                }
                                else
                                {
                                    subnode.Type = token.Token;
                                    node.AddChild(subnode);
                                    tokensConsumed += consumed;
                                    matchFound = true;
                                    break;
                                }

                            }
                            if (!matchFound)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return new Tuple<AstNode, int>(node, tokensConsumed);
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
