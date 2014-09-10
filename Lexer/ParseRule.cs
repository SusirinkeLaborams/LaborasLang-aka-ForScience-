using System.Collections.Generic;
using System.Diagnostics;

namespace Lexer
{
    public enum ConditionType
    {
        One,
        OneOrMore,
        ZeroOrMore,
    }

    [DebuggerDisplay("Condition, {Type} {Token}")]
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

        public static implicit operator List<Condition>(Condition token)
        {
            return new List<Condition>(8) { token };
        }

        public static List<Condition> operator +(Condition a, Condition b)
        {
            return new List<Condition>(8) { a, b };
        }

        public static List<Condition> operator +(List<Condition> list, Condition token)
        {
            list.Add(token);
            return list;
        }
    }
    struct ParseRule
    {
        public TokenType Result;
        public Condition[][] RequiredTokens { get; private set; }

        public ParseRule(Condition result, params List<Condition>[] requiredTokens) : this()
        {
            Result = result.Token;
            RequiredTokens = new Condition[requiredTokens.Length][];

            for (int i = 0; i < requiredTokens.Length; i++)
            {
                RequiredTokens[i] = new Condition[requiredTokens[i].Count];

                for (int j = 0; j < requiredTokens[i].Count; j++)
                {
                    RequiredTokens[i][j] = requiredTokens[i][j];
                }
            }
        }

        public ParseRule(TokenType result, params List<Condition>[] requiredTokens)
            : this()
        {
            Result = result;
            RequiredTokens = new Condition[requiredTokens.Length][];

            for (int i = 0; i < requiredTokens.Length; i++)
            {
                RequiredTokens[i] = new Condition[requiredTokens[i].Count];

                for (int j = 0; j < requiredTokens[i].Count; j++)
                {
                    RequiredTokens[i][j] = requiredTokens[i][j];
                }
            }
        }
    }
}
