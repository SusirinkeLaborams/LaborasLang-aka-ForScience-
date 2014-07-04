using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    class SyntaxMatcher
    {
        private ParseRule[] ParseRules = 
        {
            new ParseRule(TokenType.StatementNode, new[]{new Condition[]{TokenType.DeclarationNode}, new Condition[]{TokenType.AssignmentNode}, new Condition[]{TokenType.CodeBlockNode}}),
            new ParseRule(TokenType.DeclarationNode, new Condition[]{TokenType.Symbol, TokenType.Symbol, TokenType.EndOfLine}),
            new ParseRule(TokenType.AssignmentNode, new Condition[]{TokenType.Symbol, TokenType.Assignment, TokenType.Symbol, TokenType.EndOfLine}),
            new ParseRule(TokenType.CodeBlockNode, new Condition[]{TokenType.LeftCurlyBracket, new Condition(TokenType.StatementNode, ConditionType.ZeroOrMore), TokenType.RightCurlyBracket}),

        };
        public AstNode Match(IEnumerable<Token> tokens)
        {
            throw new NotImplementedException();
        }
    }
}
