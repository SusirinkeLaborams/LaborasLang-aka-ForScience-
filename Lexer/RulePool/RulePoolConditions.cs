using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    partial class RulePool
    {
        private static Condition OneOrMore(Condition c)
        {
            return new Condition(c, ConditionType.OneOrMore);
        }

        private static Condition ZeroOrMore(Condition c)
        {
            return new Condition(c, ConditionType.ZeroOrMore);
        }

        private static ConditionList OptionalTail(ConditionList conditions)
        {
            Contract.Requires(conditions.Count > 0);

            var firstCondition = conditions[0];
            firstCondition.Type = ConditionType.OptionalFromThis;
            conditions[0] = firstCondition;
            return conditions;
        }

        private static ConditionList Optional(Condition c)
        {
            return new Condition(c, ConditionType.ZeroOrOne);
        }
        private static ParseRule ParseRule(Condition result, params List<Condition>[] requiredTokens)
        {
            Contract.Requires(requiredTokens.Length > 0);
            return new ParseRule(result, ParseRuleCollapsableLevel.Never, requiredTokens);
        }

        private static ParseRule CollapsableParseRule(Condition result, params List<Condition>[] requiredTokens)
        {
            Contract.Requires(requiredTokens.Length > 0);
            return new ParseRule(result, ParseRuleCollapsableLevel.OneChild, requiredTokens);
        }

        private static ParseRule AlwaysCollapsableParseRule(Condition result, List<Condition> requiredTokens)
        {
            return new ParseRule(result, ParseRuleCollapsableLevel.Always, requiredTokens);
        }

        private static ParseRule AlwaysCollapsableParseRule(Condition result, params List<Condition>[] requiredTokens)
        {
            Contract.Requires(requiredTokens.Length > 0);
            return new ParseRule(result, ParseRuleCollapsableLevel.Always, requiredTokens);
        }        
    }
}
