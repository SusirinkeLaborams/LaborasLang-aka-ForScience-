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
        public static new ArithmeticOperatorNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new ArithmeticOperatorNode();
            var left = instance.LeftOperand = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            var right = instance.RightOperand = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[2]);
            string op = parser.ValueOf(lexerNode.Children[1]);
            if ((left.ReturnType.isStringType() && right.ReturnType.isStringType() && op == "+")
                ||
                (left.ReturnType.IsNumericType() && right.ReturnType.IsNumericType()))
            {
                if (left.ReturnType.IsAssignableTo(right.ReturnType))
                    instance.ReturnType = right.ReturnType;
                else if (right.ReturnType.IsAssignableTo(left.ReturnType))
                    instance.ReturnType = left.ReturnType;
                else
                    throw new TypeException(String.Format("Incompatible operand types, {0} and {1} received", left.ReturnType.FullName, right.ReturnType.FullName));
                try
                {
                    instance.BinaryOperatorType = BinaryOperatorNode.Operators[op];
                }
                catch (KeyNotFoundException)
                {
                    throw new ParseException("Unkown operator '" + op + "'");
                }
                return instance;
            }
            else
            {
                throw new TypeException(String.Format("Incompatible operand types, {0} and {1} for operator {2}", left.ReturnType.FullName, right.ReturnType.FullName, op));
            }
        }
    }
}
