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

        private readonly Lazy<TypeReference> functorType;

        internal MethodNode(MethodReference method, ExpressionNode instance, ContextNode context, SequencePoint point)
            : base(method, GetInstance(method, instance, context, point), context, point)
        {
            this.Method = method;
            this.functorType = new Lazy<TypeReference>(() => AssemblyRegistry.GetFunctorType(context.Parser.Assembly, Method));
        }

        public static MethodNode Parse(ContextNode context, AstNode lexerNode)
        {
            var method = FunctionDeclarationNode.ParseAsFunctor(context, lexerNode);
            return new MethodNode(method.MethodReference, null, context, method.SequencePoint);
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
