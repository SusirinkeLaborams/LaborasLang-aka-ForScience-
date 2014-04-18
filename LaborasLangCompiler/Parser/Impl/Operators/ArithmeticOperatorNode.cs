using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ArithmeticOperatorNode : BinaryOperatorNode
    {
        public static new ArithmeticOperatorNode Parse(string op, IExpressionNode left, IExpressionNode right)
        {
            var instance = new ArithmeticOperatorNode();
            BinaryOperatorNodeType type;
            try
            {
                type = BinaryOperatorNode.Operators[op];
            }
            catch (KeyNotFoundException)
            {
                throw new ParseException("Unkown operator '" + op + "'");
            }
            instance.LeftOperand = left;
            instance.RightOperand = right;
            instance.BinaryOperatorType = type;
            if ((left.ReturnType.IsStringType() && right.ReturnType.IsStringType() && type == BinaryOperatorNodeType.Addition)
                ||
                (left.ReturnType.IsNumericType() && right.ReturnType.IsNumericType()))
            {
                if (left.ReturnType.IsAssignableTo(right.ReturnType))
                    instance.ReturnType = right.ReturnType;
                else if (right.ReturnType.IsAssignableTo(left.ReturnType))
                    instance.ReturnType = left.ReturnType;
                else
                    throw new TypeException(String.Format("Incompatible operand types, {0} and {1} received", left.ReturnType.FullName, right.ReturnType.FullName));

                return instance;
            }
            else
            {
                throw new TypeException(String.Format("Incompatible operand types, {0} and {1} for operator {2}", left.ReturnType.FullName, right.ReturnType.FullName, op));
            }
        }
    }
}
