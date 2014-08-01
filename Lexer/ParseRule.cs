using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    public enum ConditionType
    {
        One,
        OneOrMore,
        ZeroOrMore,
    }

    struct Condition
    {
        public TokenType Token;
        public ConditionType Type;

        public Condition(Condition token, ConditionType type)
        {
            Token = token.Token;
            Type = type;
        }

        public Condition(TokenType token, ConditionType type)
        {
            Token = token;
            Type = type;
        }

        public static implicit operator Condition(TokenType token)
        {
            return new Condition(token, ConditionType.One);
        }

        public static implicit operator Condition[](Condition token)
        {
            return new Condition[] { token };
        }
        
        public static Condition[] operator + (Condition a, Condition b)
        {
            var array = new Condition[] { a, b };
            return array;
        }

        public static Condition[] operator + (Condition[] a, Condition b)
        {
            var array = new Condition[a.Length + 1];
            a.CopyTo(array, 0);
            array[a.Length] = b;
            return array;
        }
    }
    struct ParseRule
    {
        public TokenType Result;
        public IEnumerable<List<Condition>> RequiredTokens;
        public ParseRule(Condition result, params Condition[][] requiredTokens)
        {
            Result = result.Token;
            RequiredTokens = requiredTokens.Select(x => x.ToList());
        }

        public ParseRule(TokenType result, params Condition[][] requiredTokens)
        {
            Result = result;
            RequiredTokens = requiredTokens.Select(x => x.ToList());
        }
    }
}
