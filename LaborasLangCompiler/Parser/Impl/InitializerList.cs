using LaborasLangCompiler.Common;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;

namespace LaborasLangCompiler.Parser.Impl
{
    class InitializerList : ParserNode, IInitializerList
    {
        public override NodeType Type { get { return NodeType.InitializerList; } }
        public IReadOnlyList<IExpressionNode> Initializers { get { return expressions; } }

        public TypeReference ElementType { get; private set; }
        public IReadOnlyList<int> Dimmensions { get; private set; }

        private IReadOnlyList<ExpressionNode> expressions;
        private InitializerList(SequencePoint point) : base(point)
        {
        }

        public static InitializerList Create(ContextNode context, IEnumerable<ExpressionNode> expressions, SequencePoint point)
        {
            foreach(var exp in expressions)
            {
                if(!exp.IsGettable)
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(exp.SequencePoint, "Initializer list items must be gettable");
                }
            }

            var instance = new InitializerList(point);
            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("InitializerList:");
            foreach(var exp in expressions)
            {
                builder.AppendLine(exp.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
