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
            if (!left.ReturnType.IsNumeralType() || !right.ReturnType.IsNumeralType())
                throw new TypeException(String.Format("Arithmetic operations only allowed on numeric types, {0} and {1} received", left.ReturnType.FullName, right.ReturnType.FullName));
            if (left.ReturnType.IsFloatingPointType() || right.ReturnType.IsFloatingPointType())
                instance.ReturnType = parser.Primitives["float"];
            else
                instance.ReturnType = parser.Primitives["int"];
            //temp code, paskui reiks ziuret pagal tipu dydzius
            try
            {
                instance.BinaryOperatorType = BinaryOperatorNode.Operators[op];
            }
            catch(KeyNotFoundException)
            {
                throw new ParseException("Unkown operator '" + op + "'");
            }
            return instance;
        }
    }
}
