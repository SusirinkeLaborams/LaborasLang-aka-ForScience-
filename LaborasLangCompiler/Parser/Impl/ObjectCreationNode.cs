using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ObjectCreationNode : SymbolNode, IObjectCreationNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ObjectCreation; } }
        public override TypeWrapper TypeWrapper { get { return type; } }
        public IReadOnlyList<IExpressionNode> Args { get { return args; } }
        public MethodReference Constructor { get { return constructor.MethodReference; } }
        public override bool IsGettable { get { return true; } }

        private TypeWrapper type;
        private List<ExpressionNode> args;
        private MethodWrapper constructor;

        public ObjectCreationNode(TypeWrapper type, List<ExpressionNode> args, MethodWrapper constructor, Context scope, SequencePoint point)
            :base(type.FullName, scope, point)
        {
            this.type = type;
            this.args = args;
            this.constructor = constructor;
            Utils.VerifyAccessible(Constructor, Scope, point);
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("ObjectCreation:");
            builder.Indent(indent + 1).AppendFormat("Type: {0}", TypeWrapper).AppendLine();
            builder.Indent(indent + 1).AppendLine("Args:");
            foreach (var arg in args)
            {
                builder.AppendLine(arg.ToString(indent + 2));
            }
            builder.Indent(indent + 1).AppendLine("Constructor:");
            builder.Indent(indent + 2).AppendLine(Constructor.ToString());
            return builder.ToString();
        }
    }
}
