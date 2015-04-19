using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Lexer;
using System.Collections.Generic;
using System.Text;

namespace LaborasLangCompilerUnitTests.LexerTests
{
    [TestClass]
    public class RuleValidation
    {
        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: Rule validation")]
        public void TestRulePoolContainsAllRules()
        {
            var missingRules = new List<TokenType>();
            var notRequiredRules = new TokenType[] { TokenType.RootNode, TokenType.TokenTypeCount, TokenType.LexerInternalTokens, TokenType.NonTerminalToken, TokenType.UnknownNode };
            var allTokens = System.Enum.GetValues(typeof(TokenType)).Cast<TokenType>();
            var requiredRules = allTokens.Where(t => !t.IsTerminal() && !notRequiredRules.Contains(t));
            foreach (var token in requiredRules)
            {
                var contains = RulePool.LaborasLangRuleset.Any(item => item.Result == token);
                if(!contains)
                {
                    missingRules.Add(token);
                }
            }
            Assert.AreEqual(0, missingRules.Count);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: Rule validation")]
        public void TestNoUnreachableRules()
        {
            var rules = new ParseRule[(int)TokenType.TokenTypeCount];

            foreach (var rule in RulePool.LaborasLangRuleset)
            {
                rules[(int)rule.Result] = rule;
            }
                   

            var visited = new bool[(int)TokenType.TokenTypeCount];
            visited[(int)TokenType.RootNode] = true;
            visited[(int)TokenType.LexerInternalTokens] = true;
            visited[(int)TokenType.NonTerminalToken] = true;
            visited[(int)TokenType.MalformedToken] = true;
            visited[(int)TokenType.UnknownNode] = true;
            visited[(int)TokenType.Empty] = true;
            var stack = new Stack<TokenType>();
            stack.Push(TokenType.CodeConstruct);
            
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
                    unreachableTokens.Add(token);
                }
            }

            var stringified = unreachableTokens.Aggregate("", (a, b) => a + "\n" + b.ToString());
            Assert.AreEqual(0, unreachableTokens.Count, "Unreachable rules: " + stringified);
        }

        [TestMethod, TestCategory("Lexer"), TestCategory("Lexer: Rule validation")]
        public void TestNoInfiniteRecursionInRules()
        {
            var rules = new ParseRule[(int)TokenType.TokenTypeCount];
            var visited = new bool[rules.Length];

            foreach (var rule in RulePool.LaborasLangRuleset)
            {
                rules[(int)rule.Result] = rule;
            }

            foreach (var rule in RulePool.LaborasLangRuleset)
            {
                TraverseRules(rules, visited, new Stack<ParseRule>(), (int)rule.Result);
            }
        }


        void TraverseRules(ParseRule[] rules, bool[] visited, Stack<ParseRule> traverseStack, int token)
        {
            traverseStack.Push(rules[token]);
            visited[token] = true;
            {
                var rule = rules[token];

                if (rule.RequiredTokens != null)
                {
                    foreach (var dependency in rule.RequiredTokens)
                    {
                        var nextToken = (int)dependency[0].Token;
                        if (!visited[nextToken])
                        {
                            TraverseRules(rules, visited, traverseStack, nextToken);
                        }
                        else
                        {
                            var error = new StringBuilder();
                            error.AppendLine("Circular dependency found:");
                            traverseStack.Push(rules[nextToken]);

                            do
                            {
                                error.AppendLine(string.Format("\t{0}", traverseStack.Pop().Result));
                            }
                            while (traverseStack.Peek().Result != (TokenType)nextToken);

                            error.AppendLine(string.Format("\t{0}", traverseStack.Pop().Result));
                            throw new Exception(error.ToString());
                        }
                    }
                }
            }
            visited[token] = false;
            traverseStack.Pop();
        }
    }
}
