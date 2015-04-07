using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lexer.PostProcessors
{
    class OperatorColapser : PostProcessor
    {
        public override void Transform(AbstractSyntaxTree astNode)
        {
            switch (astNode.Type)
            {
                case TokenType.InfixOperator:
                case TokenType.PostfixOperator:
                case TokenType.PrefixOperator:
                    astNode.Collapse();
                    return;
                default:
                    return;
            }
        }
    }
}
