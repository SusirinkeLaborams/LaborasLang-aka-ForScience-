using LaborasLangCompiler.Parser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;

namespace LaborasLangCompiler.Parser.Impl.Operators
{
    class ShiftOperatorNode : BinaryOperatorNode
    {
        public static new ShiftOperatorNode Parse(Parser parser, string op, IExpressionNode left, IExpressionNode right)
        {
            var instance = new ShiftOperatorNode();
            instance.LeftOperand = left;
            instance.RightOperand = right;
            instance.BinaryOperatorType = BinaryOperatorNode.Operators[op];
            instance.ReturnType = left.ReturnType;
            if (right.ReturnType.FullName != parser.Primitives[Parser.Int].FullName)
                throw new TypeException("Right shift operand must be of signed 32bit integer type");
            if (!left.ReturnType.IsIntegerType())
                throw new TypeException("Left shift operand must be of integer type");
            return instance;
        }
    }
}
