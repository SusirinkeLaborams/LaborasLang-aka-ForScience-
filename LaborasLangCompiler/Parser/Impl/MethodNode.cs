using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class MethodNode : RValueNode, IMethodNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Function; } }
        public override TypeWrapper TypeWrapper { get { return method.FunctorType; } }
        public IExpressionNode ObjectInstance { get { return instance; } }
        public MethodReference Method { get { return method.MethodReference; } }
        public MethodWrapper MethodWrapper { get { return method; } }

        private MethodWrapper method;
        private ExpressionNode instance;

        public MethodNode(MethodWrapper method, ExpressionNode instance, SequencePoint point)
            : base(point)
        {
            this.method = method;
            this.instance = instance;
        }
        public static MethodNode Parse(Parser parser, ContainerNode parent, AstNode lexerNode, string name = null)
        {
            var method = FunctionDeclarationNode.Parse(parser, parent, lexerNode, name);
            return new MethodNode(method, null, method.SequencePoint);
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Method:");
            if (instance != null)
            {
                builder.Indent(indent + 1).AppendLine("Instance:");
                builder.AppendLine(instance.ToString(indent + 2));
            }
            builder.Indent(indent + 1).AppendLine("Method:");
            builder.Indent(indent + 2).AppendLine(Method.ToString());
            return builder.ToString();
        }
    }
}
