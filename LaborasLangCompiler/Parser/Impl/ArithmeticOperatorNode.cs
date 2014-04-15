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
            string op = parser.GetNodeValue(lexerNode.Children[1]);
            if (!ILHelpers.IsNumeralType(left.ReturnType) || !ILHelpers.IsNumeralType(right.ReturnType))
                throw new TypeException(String.Format("Arithmetic operations only allowed on numeric types, {0} and {1} received", left.ReturnType.FullName, right.ReturnType.FullName));
            if (ILHelpers.IsFloatingPointType(left.ReturnType) || ILHelpers.IsFloatingPointType(right.ReturnType))
                instance.ReturnType = parser.Primitives["float"];
            else
                instance.ReturnType = parser.Primitives["int"];
            //temp code, paskui reiks ziuret pagal tipu dydzius
            switch (op)
            {
                case "+":
                    instance.BinaryOperatorType = BinaryOperatorNodeType.Addition;
                    break;
                case "-":
                    instance.BinaryOperatorType = BinaryOperatorNodeType.Subtraction;
                    break;
                case "*":
                    instance.BinaryOperatorType = BinaryOperatorNodeType.Multiplication;
                    break;
                case "/":
                    instance.BinaryOperatorType = BinaryOperatorNodeType.Division;
                    break;
                default:
                    throw new ParseException("Unkown operator '" + op + "'");
            }
            return instance;
        }
    }
}
