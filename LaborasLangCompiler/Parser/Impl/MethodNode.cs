using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Utils;
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
    class MethodNode : MemberNode, IMethodNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Function; } }
        public override TypeReference ExpressionReturnType { get { return functorType.Value; } }
        public MethodReference Method { get; private set; }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return false; } }

        private Lazy<TypeReference> functorType;

        internal MethodNode(Parser parser, MethodReference method, ExpressionNode instance, ContextNode parent, SequencePoint point)
            : base(method, GetInstance(method, instance, parent, point), parent, point)
        {
            this.Method = method;
            this.functorType = new Lazy<TypeReference>(() => AssemblyRegistry.GetFunctorType(parser.Assembly, Method));
        }

        public static MethodNode Parse(Parser parser, ContextNode parent, AstNode lexerNode)
        {
            var method = FunctionDeclarationNode.ParseAsFunctor(parser, parent, lexerNode);
            return new MethodNode(parser, method.MethodReference, null, parent, method.SequencePoint);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Method:");
            builder.Indent(indent + 1).AppendLine("Instance:");
            if (Instance == null)
            {
                builder.Indent(indent + 2).AppendLine("null");
            }
            else
            {
                builder.AppendLine(Instance.ToString(indent + 2));
            }
            builder.Indent(indent + 1).AppendLine("Method:");
            builder.Indent(indent + 2).AppendLine(Method.ToString());
            return builder.ToString();
        }
    }
}
