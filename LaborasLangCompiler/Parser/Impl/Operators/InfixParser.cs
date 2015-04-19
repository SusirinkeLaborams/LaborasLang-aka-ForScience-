using LaborasLangCompiler.Common;
using Lexer;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Operators
{
    class InfixParser
    {
        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == TokenType.InfixNode);
            var op = lexerNode.Children[2].Type;
            if (op.IsBinaryOp())
            {
                return BinaryOperatorNode.Parse(context, lexerNode);
            }

            if(op.IsAssignmentOp())
            {
                return AssignmentOperatorNode.Parse(context, lexerNode);
            }

            if(op.IsPeriodOp())
            {
                return DotOperatorNode.Parse(context, lexerNode);
            }

            ContractsHelper.AssertUnreachable("Unknown infix operator {0}", op);
            return null;//unreachable
        }
    }
}
