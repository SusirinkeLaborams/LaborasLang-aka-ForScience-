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
    class MethodNode : SymbolNode, IMethodNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Function; } }
        public override TypeWrapper TypeWrapper { get { return method.FunctorType; } }
        public IExpressionNode ObjectInstance { get { return instance; } }
        public MethodReference Method { get { return method.MethodReference; } }
        public MethodWrapper MethodWrapper { get { return method; } }
        public override bool IsGettable { get { return true; } }

        private MethodWrapper method;
        private ExpressionNode instance;

        public MethodNode(MethodWrapper method, ExpressionNode instance, TypeReference scope, SequencePoint point)
            : base(method.MethodReference.FullName, scope, point)
        {
            this.method = method;
            this.instance = instance;
            Utils.VerifyAccessible(Method, Scope, point);
        }
        public static new MethodNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var method = FunctionDeclarationNode.ParseAsFunctor(parser, parent, lexerNode);
            return new MethodNode(method, null, parent.GetClass().TypeReference, method.SequencePoint);
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
