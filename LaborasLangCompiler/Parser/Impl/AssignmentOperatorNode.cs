using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class AssignmentOperatorNode : RValueNode, IAssignmentOperatorNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.AssignmentOperator; } }
        public override TypeReference ReturnType { get; set; }
        public ILValueNode LeftOperand { get; private set; }
        public IExpressionNode RightOperand { get; private set; }
        public static new AssignmentOperatorNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new AssignmentOperatorNode();
            instance.LeftOperand = LValueNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[0]);
            instance.RightOperand = ExpressionNode.Parse(parser, parentClass, parentBlock, lexerNode.Children[2]);
            instance.ReturnType = instance.LeftOperand.ReturnType;

            var op = parser.ValueOf(lexerNode.Children[1]);
            if (op != "=")
                throw new NotImplementedException("Only parsing simple assignment");

            if (instance.RightOperand.ReturnType.IsAssignableTo(instance.LeftOperand.ReturnType))
                return instance;
            else
                throw new TypeException(String.Format("Assigned {0} to {1}", instance.RightOperand.ReturnType, instance.LeftOperand.ReturnType));
        }
    }
}
