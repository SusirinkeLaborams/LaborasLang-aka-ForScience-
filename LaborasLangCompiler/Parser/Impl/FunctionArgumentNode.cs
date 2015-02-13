using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil.Cil;

namespace LaborasLangCompiler.Parser.Impl
{
    class FunctionArgumentNode : ExpressionNode, IMethodParamNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get; private set; }
        public override TypeReference ExpressionReturnType { get { return Param.ParameterType; } }
        public string Name { get { return Param.Name; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return true; }
        }


        public FunctionArgumentNode(ParameterDefinition param, SequencePoint point)
            : base(point)
        {
            this.Param = param;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Param:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(ExpressionReturnType.FullName);
            return builder.ToString();
        }
    }
}
