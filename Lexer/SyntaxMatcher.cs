using Lexer.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Lexer
{
    internal sealed class SyntaxMatcher
    {
        private static readonly ParseRule[] m_ParseRules;
        private readonly Token[] m_Source;
        private readonly RootNode m_RootNode;

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

        static SyntaxMatcher()
        {
            m_ParseRules = new ParseRule[(int)TokenType.TokenTypeCount];

            foreach (var rule in RulePool.LaborasLangRuleset)
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
            var defaultConditions = new Condition[] { 
                new Condition(TokenType.CodeConstruct, ConditionType.OneOrMore)};
            return Match(defaultConditions);

        }

        public AstNode Match(Condition[] defaultConditions)
        {
            var tokensConsumed = 0;

            AstNode matchedNode = Match(0, defaultConditions, ref tokensConsumed);
            if (matchedNode.IsNull)
            {
                matchedNode = m_RootNode.NodePool.ProvideNode();
            }

            while(tokensConsumed < m_Source.Length) 
            {
                Contract.Assume(tokensConsumed >= 0);

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
            Contract.Requires(tokensConsumed >= 0);
            Contract.Ensures(tokensConsumed >= 1);

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
            Contract.Requires(rule.Length > 0, "Rule count must be more than 0!");
            Contract.Requires(sourceOffset >= 0);
            Contract.Requires(tokensConsumed >= 0);

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

            Contract.Assume(tokensConsumed > 0);

            var token = m_RootNode.ProvideToken();
            token.Start = m_Source[sourceOffset].Start;
            token.End = m_Source[sourceOffset + tokensConsumed - 1].End;
            token.Content = FastString.Empty;
            node.Token = token;

            return node;
        }

        private bool MatchTerminal(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            Contract.Requires(tokensConsumed >= 0);

            if (m_Source[sourceOffset + tokensConsumed].Type == token.Token)
            {
                if (token.Token.IsMeaningful())
                {
                    node.AddTerminal(m_RootNode, m_Source[sourceOffset + tokensConsumed]);
                }
                
                tokensConsumed++;
                LastMatched = sourceOffset + tokensConsumed;

                return true;
            }
            bool success = token.Type == ConditionType.ZeroOrOne;
            return success;
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
            bool success = token.Type == ConditionType.ZeroOrOne;
            return success;
        }

        private bool MatchRule(Condition token, int sourceOffset, ref AstNode node, ref int tokensConsumed)
        {
            Contract.Requires(tokensConsumed >= 0);
            Contract.Ensures(tokensConsumed >= Contract.OldValue(tokensConsumed));

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
    }
}
