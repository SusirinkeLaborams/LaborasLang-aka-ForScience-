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
    class LocalVariableNode : ExpressionNode, ILocalVariableNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LocalVariable; } }
        public VariableDefinition LocalVariable { get; private set; }
        public override TypeReference ExpressionReturnType { get { return LocalVariable.VariableType; } }
        public string Name { get { return LocalVariable.Name; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return !isConst; }
        }

        private bool isConst;
        public LocalVariableNode(SequencePoint point, VariableDefinition variable, bool isConst)
            : base(point)
        {
            this.LocalVariable = variable;
            this.isConst = isConst;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Local:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(ExpressionReturnType.FullName);
            return builder.ToString();
        }
    }
}
