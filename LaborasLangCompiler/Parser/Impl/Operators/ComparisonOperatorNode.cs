using LaborasLangCompiler.Parser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;

namespace LaborasLangCompiler.Parser.Impl.Operators
{
    class ComparisonOperatorNode : BinaryOperatorNode
    {
        public static new ComparisonOperatorNode Parse(Parser parser, string op, IExpressionNode left, IExpressionNode right)
        {
            var instance = new ComparisonOperatorNode();
            instance.LeftOperand = left;
            instance.RightOperand = right;
            instance.ReturnType = parser.Primitives[Parser.Bool];
            BinaryOperatorNodeType type = BinaryOperatorNode.Operators[op];

            bool comparable = left.ReturnType.IsNumericType() && right.ReturnType.IsNumericType();

            if (!comparable)
                comparable = left.ReturnType.IsStringType() && right.ReturnType.IsStringType();

            if(!comparable)
                comparable = left.ReturnType.IsBooleanType() && right.ReturnType.IsBooleanType();

            if (comparable)
                comparable = left.ReturnType.IsAssignableTo(right.ReturnType) || right.ReturnType.IsAssignableTo(left.ReturnType);

            if (!comparable)
                throw new TypeException(String.Format("Types {0} and {1} cannot be compared with op {2}", left.ReturnType, right.ReturnType, op));

            return instance;
        }
    }
}
