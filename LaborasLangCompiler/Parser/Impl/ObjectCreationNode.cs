using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;

namespace LaborasLangCompiler.Parser.Impl
{
    class ObjectCreationNode : MemberNode, IObjectCreationNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ObjectCreation; } }
        public override TypeReference ExpressionReturnType { get { return type; } }
        public IReadOnlyList<IExpressionNode> Args { get { return args; } }
        public MethodReference Constructor { get; private set; }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return false; } }

        private TypeReference type;
        private List<ExpressionNode> args;

        public ObjectCreationNode(List<ExpressionNode> args, MethodReference constructor, Context scope, SequencePoint point)
            :base(constructor, scope, point)
        {
            this.type = constructor.DeclaringType;
            this.args = args;
            this.Constructor = constructor;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("ObjectCreation:");
            builder.Indent(indent + 1).AppendFormat("Type: {0}", type).AppendLine();
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
