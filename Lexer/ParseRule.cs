using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public enum ConditionType
    {
        Optional,
        Zero,
        One,
        OneOrMore,
        ZeroOrMore,
    }

    struct Condition
    {
        public TokenType Token;
        public ConditionType Type;

        public Condition(TokenType token, ConditionType type)
        {
            Token = token;
            Type = type;
        }

        public static implicit operator Condition(TokenType token)
        {
            return new Condition(token, ConditionType.One);
        }
        
    }
    struct ParseRule
    {
        public TokenType Result;
        public IEnumerable<IEnumerable<Condition>> RequiredTokens;
        public ParseRule(TokenType result, IEnumerable<IEnumerable<Condition>> requiredTokens)
        {
            Result = result;
            RequiredTokens = requiredTokens;
        }

        public ParseRule(TokenType result, IEnumerable<Condition> requiredTokens)
        {
            Result = result;
            RequiredTokens = new[] { requiredTokens };
        }

        private ParseRule()
        {
            Result = TokenType.Unknown;
            RequiredTokens = null;
        }
    }
}
