using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Lexer;
using System.Collections.Generic;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class RuleValidation
    {
        [TestMethod, TestCategory("Lexer")]
        public void TestRulePoolContainsAllRules()
        {
            var missingRules = new List<TokenType>();
            var notRequiredRules = new TokenType[] { TokenType.RootNode, TokenType.TokenTypeCount, TokenType.LexerInternalTokens, TokenType.NonTerminalToken };
            var allTokens = System.Enum.GetValues(typeof(TokenType)).Cast<TokenType>();
            var requiredRules = allTokens.Where(t => !t.IsTerminal() && !notRequiredRules.Contains(t));
            foreach (var token in requiredRules)
            {
                var contains = SyntaxMatcher.ParseRulePool.Any(item => item.Result == token);
                if(!contains)
                {
                    missingRules.Add(token);
                }
            }
            Assert.AreEqual(0, missingRules.Count);
        }

        [TestMethod, TestCategory("Lexer")]
        public void TestNoUnreachableRules()
        {
            var rules = new ParseRule[(int)TokenType.TokenTypeCount];

            foreach (var rule in SyntaxMatcher.ParseRulePool)
            {
                rules[(int)rule.Result] = rule;
            }
                   

            var visited = new bool[(int)TokenType.TokenTypeCount];
            visited[(int)TokenType.RootNode] = true;
            visited[(int)TokenType.LexerInternalTokens] = true;
            visited[(int)TokenType.NonTerminalToken] = true;
            
            var stack = new Stack<TokenType>();
            stack.Push(TokenType.StatementNode);
            
            while(stack.Count != 0)
            {
                var current = stack.Pop();
                
                visited[(int)current] = true;
                if (!current.IsTerminal())
                {
                    foreach (var neighbourhood in rules[(int)current].RequiredTokens)
                    {
                        foreach (var neighbour in neighbourhood)
                        {
                            if (!visited[(int)neighbour.Token])
                            {
                                stack.Push(neighbour.Token);
                            }
                        }
                    }
                }
            }

            var unreachableTokens = new List<TokenType>();
            for(int i = 0; i < visited.Length; i++)
            {
                if(!visited[i])
                {
                    var token = (TokenType)i;
                    if (!token.IsTerminal())
                    {
                        unreachableTokens.Add(token);
                    }
                }
            }

            var stringified = unreachableTokens.Aggregate("", (a, b) => a + " " + b.ToString());
            Assert.AreEqual(0, unreachableTokens.Count, "Unreachable rules: " + stringified);
        }
    }
}
