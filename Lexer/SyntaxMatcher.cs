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
        private IEnumerable<Token> enumerable;

        public SyntaxMatcher(IEnumerable<Token> sourceTokens)
        {
            m_ParseRules = new Dictionary<TokenType, ParseRule>();
            
            ParseRule[] AllRules = 
            {
                #region Syntax rules
                new ParseRule(TokenType.StatementNode,                    
                    new Condition[]{TokenType.RValue, TokenType.EndOfLine},
                    new Condition[]{TokenType.DeclarationNode},
                    new Condition[]{TokenType.AssignmentNode},
                    new Condition[]{TokenType.CodeBlockNode}),
            
                new ParseRule(TokenType.DeclarationNode,
                    new Condition[]{new Condition(TokenType.VariableModifier, ConditionType.OneOrMore), TokenType.Type, TokenType.Symbol, TokenType.EndOfLine},
                    new Condition[]{TokenType.Type, TokenType.Symbol, TokenType.EndOfLine}),
            
                new ParseRule(TokenType.VariableModifier, 
                    new Condition[]{TokenType.Const},
                    new Condition[]{TokenType.Internal},
                    new Condition[]{TokenType.Private},
                    new Condition[]{TokenType.Public},
                    new Condition[]{TokenType.Protected},
                    new Condition[]{TokenType.Static},
                    new Condition[]{TokenType.Virtual}),

                new ParseRule(TokenType.AssignmentNode,
                    new Condition[]{TokenType.LValue, TokenType.Assignment, TokenType.Value, TokenType.EndOfLine}),
            
                new ParseRule(TokenType.CodeBlockNode,
                    new Condition[]{TokenType.LeftCurlyBracket, new Condition(TokenType.StatementNode, ConditionType.OneOrMore), TokenType.RightCurlyBracket},
                    new Condition[]{TokenType.LeftCurlyBracket, TokenType.StatementNode, TokenType.RightCurlyBracket}),

                new ParseRule(TokenType.Value,                    
                    new Condition[]{TokenType.RValue},
                    new Condition[]{TokenType.LValue}),

                new ParseRule(TokenType.LValue,
                    new Condition[]{TokenType.FullSymbol}),
 
                new ParseRule(TokenType.RValue,                    
                    new Condition[]{TokenType.FunctionCall},
                    new Condition[]{TokenType.FullSymbol}),

                new ParseRule(TokenType.FunctionCall,
                    new Condition[]{TokenType.FullSymbol, TokenType.LeftBracket, TokenType.RightBracket},
                    new Condition[]{TokenType.FullSymbol, TokenType.LeftBracket, TokenType.Value, new Condition(TokenType.FunctionArgument, ConditionType.OneOrMore), TokenType.RightBracket},
                    new Condition[]{TokenType.FullSymbol, TokenType.LeftBracket, TokenType.Value, TokenType.RightBracket}),

                new ParseRule(TokenType.FunctionArgument,
                    new Condition[]{TokenType.Comma, TokenType.Value}),

                new ParseRule(TokenType.FullSymbol,
                    new Condition[]{TokenType.Symbol, new Condition(TokenType.SubSymbol, ConditionType.OneOrMore)},
                    new Condition[]{TokenType.Symbol}),

                new ParseRule(TokenType.SubSymbol,
                    new Condition[]{TokenType.Period, TokenType.Symbol}),

                new ParseRule(TokenType.Type,
                    new Condition[]{TokenType.FullSymbol, TokenType.LeftBracket, new Condition(TokenType.Type, ConditionType.OneOrMore), TokenType.RightBracket},
                    new Condition[]{TokenType.FullSymbol, TokenType.LeftBracket, TokenType.RightBracket},
                    new Condition[]{TokenType.FullSymbol}),

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

            Tuple<AstNode, int> matchedNode = Match(tokensConsumed, new Condition[]{new Condition(TokenType.StatementNode, ConditionType.OneOrMore)}.ToList());
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
                // First match is required
                if(token.Token.IsTerminal())
                {
                    if (m_Source[sourceOffset + tokensConsumed].Type == token.Token)
                    {
                        node.AddTerminal(m_Source[sourceOffset + tokensConsumed]);
                        tokensConsumed++; 
                    }
                    else
                    {
                        return new Tuple<AstNode, int>(null, 0);
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
                        return new Tuple<AstNode, int>(null, 0);
                    }
                }
                
                // Second match for same token is optional, it should not return return on failure as it would discard first result, just stop matching
                if (token.Type == ConditionType.OneOrMore)
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
    }
}
