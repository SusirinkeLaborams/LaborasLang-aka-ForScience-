using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
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
        public override TypeWrapper TypeWrapper { get { return type; } }
        public ILValueNode LeftOperand { get { return left; } }
        public IExpressionNode RightOperand { get { return right; } }

        private TypeWrapper type;
        private LValueNode left;
        private ExpressionNode right;
        protected AssignmentOperatorNode(SequencePoint point) : base(point) { }
        public static AssignmentOperatorNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode)
        {
            var instance = new AssignmentOperatorNode(parser.GetSequencePoint(lexerNode));
            var left = DotOperatorNode.Parse(parser, parent, lexerNode.Children[0]).ExtractLValue();
            var right = ExpressionNode.Parse(parser, parent, lexerNode.Children[2]);
            instance.type = left.TypeWrapper;

            var op = parser.ValueOf(lexerNode.Children[1]);
            if (op != "=")
                right = BinaryOperatorNode.Parse(parser, op.Remove(op.Length - 1), left, right);

            if (!right.TypeWrapper.IsAssignableTo(left.TypeWrapper))
                throw new TypeException(instance.SequencePoint, "Assigned {0} to {1}", instance.right.TypeWrapper, instance.left.TypeWrapper);
            instance.right = right;
            instance.left = left;
            return instance;    
        }
        public override string ToString()
        {
            return String.Format("(Assignment: {0} = {1})", LeftOperand, RightOperand);
        }
    }
}
