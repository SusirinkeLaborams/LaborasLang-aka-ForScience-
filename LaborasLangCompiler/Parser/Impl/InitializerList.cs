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
    class InitializerList : ParserNode
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public IEnumerable<ExpressionNode> Initializers { get; private set; }

        public TypeReference ElementType { get; private set; }
        public IEnumerable<int> Dimmensions { get; private set; }

        private InitializerList(SequencePoint point) : base(point)
        {
        }

        public static InitializerList Create(ContextNode context, IEnumerable<ExpressionNode> expressions, SequencePoint point)
        {
            if (!expressions.Any())
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Initializer list must not be empty");
            foreach(var exp in expressions)
            {
                if(!exp.IsGettable)
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(exp.SequencePoint, "Initializer list items must be gettable");
                }
            }

            var instance = new InitializerList(point);
            instance.ElementType = TypeUtils.GetCommonBaseClass(context.Assembly, expressions.Select(e => e.ExpressionReturnType));
            instance.Dimmensions = new int[]{expressions.Count()};
            instance.Initializers = expressions;
            return instance;
        }

        public static InitializerList Create(ContextNode context, IEnumerable<InitializerList> subLists, SequencePoint point)
        {
            if (!subLists.Any())
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Initializer list must not be empty");
            var first = subLists.First();
            if(!subLists.Skip(1).All(l => l.Dimmensions.SequenceEqual(first.Dimmensions)))
            {
                ErrorCode.MisshapedMatrix.ReportAndThrow(point, "Invalid intializer list structure, all sublists must have same dimmensions");
            }
            var instance = new InitializerList(point);

            instance.Initializers = Utils.Utils.ConcatAll(subLists.Select(s => s.Initializers));
            instance.ElementType = TypeUtils.GetCommonBaseClass(context.Assembly, subLists.Select(s => s.ElementType));
            instance.Dimmensions = first.Dimmensions.Concat(subLists.Count().Enumerate());
            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("InitializerList:");
            foreach (var exp in Initializers)
            {
                builder.AppendLine(exp.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
