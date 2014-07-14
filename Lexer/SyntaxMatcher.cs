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
                 new ParseRule(TokenType.RootNode,
                    new Condition[]{TokenType.StatementNode, TokenType.StatementNode}.ToList()),            

                new ParseRule(TokenType.StatementNode,
                    new Condition[]{TokenType.DeclarationNode}.ToList(),
                    new Condition[]{TokenType.AssignmentNode}.ToList(),
                    new Condition[]{TokenType.CodeBlockNode}.ToList()),
            
                new ParseRule(TokenType.DeclarationNode,
                    new Condition[]{TokenType.Symbol, TokenType.Symbol, TokenType.EndOfLine}.ToList()),
            
                new ParseRule(TokenType.AssignmentNode,
                    new Condition[]{TokenType.Symbol, TokenType.Assignment, TokenType.Symbol, TokenType.EndOfLine}),
            
                new ParseRule(TokenType.CodeBlockNode,
                    new Condition[]{TokenType.LeftCurlyBracket, new Condition(TokenType.StatementNode, ConditionType.ZeroOrMore), TokenType.RightCurlyBracket}),

            };

            foreach(var rule in AllRules)
            {
                m_ParseRules.Add(rule.Result, rule);
            }

            m_Source = sourceTokens.ToList();
        }

        public AstNode Match()
        {
            var node = new AstNode();
            var tokensConsumed = 0;
            foreach(var alternative in m_ParseRules[TokenType.RootNode].RequiredTokens)
            {
                Tuple<AstNode, int> matchedNode = Match(tokensConsumed, alternative);
                if (matchedNode.Item1 != null)
                {
                    node.AddChild(matchedNode.Item1);
                    tokensConsumed += matchedNode.Item2;
                }
            }
            Debug.Assert(tokensConsumed == m_Source.Count);
            return node;            
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
            }
            return new Tuple<AstNode, int>(node, tokensConsumed);
        } 
    }
}
