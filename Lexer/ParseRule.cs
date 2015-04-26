using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Lexer
{
    internal enum ConditionType
    {   
        ZeroOrOne,
        OptionalFromThis,
        One,
        OneOrMore,
        ZeroOrMore,
    }

    [DebuggerDisplay("Condition, {Type} {Token}")]
    internal struct Condition
    {
        public readonly TokenType Token;
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

        public static implicit operator ConditionList(Condition token)
        {
            return new ConditionList(8) { token };
        }

        public static ConditionList operator +(Condition a, Condition b)
        {
            return new ConditionList(8) { a, b };
        }

        public static ConditionList operator +(ConditionList list, Condition token)
        {
            Contract.Requires(list != null);

            list.Add(token);
            return list;
        }
    }

    internal class ConditionList : List<Condition>
    {
        public ConditionList()
        {
        }

        public ConditionList(int capacity) :
            base(capacity)
        {
            Contract.Requires(capacity >= 0);
        }
        
        public static ConditionList operator +(ConditionList list, ConditionList tokens)
        {
            Contract.Requires(tokens != null);

            foreach (var token in tokens)
            {
                list.Add(token);
            }

            return list;
        }
    }
    internal enum ParseRuleCollapsableLevel
    {
        Never,
        OneChild,
        Always
    }

    internal struct ParseRule
    {
        public readonly TokenType Result;
        public Condition[][] RequiredTokens { get; private set; }
        public ParseRuleCollapsableLevel CollapsableLevel { get; private set; }

        public ParseRule(Condition result, ParseRuleCollapsableLevel collapsableLevel, params List<Condition>[] requiredTokens)
            : this()
        {
            Contract.Requires(requiredTokens != null);

            Result = result.Token;
            CollapsableLevel = collapsableLevel;
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
