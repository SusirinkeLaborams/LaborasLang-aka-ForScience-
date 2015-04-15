using LaborasLangCompiler.Common;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;
using Lexer.Containers;
using System.Diagnostics.Contracts;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class InitializerList : ParserNode
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public IEnumerable<ExpressionNode> Initializers { get; private set; }

        public TypeReference ElementType { get; private set; }
        public IReadOnlyList<int> Dimensions { get; private set; }

        private InitializerList(SequencePoint point) : base(point)
        {
        }

        public static InitializerList Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.InitializerList);
            Contract.Ensures(Contract.Result<InitializerList>() != null);
            var point = context.Parser.GetSequencePoint(lexerNode);
            var instance = new InitializerList(point);
            var members = new List<ExpressionNode>();
            foreach(var node in lexerNode.Children)
            {
                switch(node.Type)
                {
                    case Lexer.TokenType.LeftCurlyBrace:
                    case Lexer.TokenType.RightCurlyBrace:
                    case Lexer.TokenType.Comma:
                        break;
                    case Lexer.TokenType.Value:
                        members.Add(ExpressionNode.Parse(context, node));
                        break;
                }
            }

            var arrays = members.Select(m => m as ArrayCreationNode);
            //implicit = initializer list
            if(arrays.Any(a => a != null && a.IsImplicit))
            {
                if(arrays.Any(a => a == null || !a.IsImplicit))
                {
                    ErrorCode.MisshapedMatrix.ReportAndThrow(point, "An initializer list can only contain another initializer list if all members are lists of the same dimmensions and types");
                }

                var lists = arrays.Select(a => a.InitializerList);
                return Create(context, lists, point);
            }

            return Create(context, members, point);
        }

        public static InitializerList Create(ContextNode context, IEnumerable<ExpressionNode> expressions, SequencePoint point)
        {
            Contract.Ensures(Contract.Result<InitializerList>() != null);
            if (!expressions.Any())
                return CreateEmpty(context, point);

            foreach(var exp in expressions)
            {
                if(!exp.IsGettable)
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(exp.SequencePoint, "Initializer list items must be gettable");
                }
            }

            var instance = new InitializerList(point);
            instance.ElementType = TypeUtils.GetCommonBaseClass(context.Assembly, expressions.Select(e => e.ExpressionReturnType));
            instance.Dimensions = new int[]{expressions.Count()};
            instance.Initializers = expressions;
            return instance;
        }

        private static InitializerList CreateEmpty(ContextNode context, SequencePoint point)
        {
            var instace = new InitializerList(point);
            instace.Dimensions = new int[] { 0 };
            instace.Initializers = Enumerable.Empty<ExpressionNode>();
            instace.ElementType = context.Parser.Object;
            return instace;
        }

        public static InitializerList Create(ContextNode context, IEnumerable<InitializerList> subLists, SequencePoint point)
        {
            //cant create a matrix if the list is {}
            Contract.Ensures(subLists.Any());
            var first = subLists.First();
            if(!subLists.Skip(1).All(l => l.Dimensions.SequenceEqual(first.Dimensions)))
            {
                ErrorCode.MisshapedMatrix.ReportAndThrow(point, "Invalid intializer list structure, all sublists must have same dimmensions");
            }
            var instance = new InitializerList(point);

            instance.Initializers = Utils.Utils.ConcatAll(subLists.Select(s => s.Initializers));
            instance.ElementType = TypeUtils.GetCommonBaseClass(context.Assembly, subLists.Select(s => s.ElementType));
            instance.Dimensions = subLists.Count().Enumerate().Concat(first.Dimensions).ToArray();
            return instance;
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("InitializerList:");
            builder.Indent(indent + 1).AppendFormat("Element type: {0}", ElementType.FullName).AppendLine();
            builder.Indent(indent + 1).AppendLine("Dimensions:");
            builder.Indent(indent + 2).AppendLine(String.Join(", ", Dimensions));
            builder.Indent(indent + 1).AppendLine("Values:");
            if (Initializers.Any())
            {
                foreach (var exp in Initializers)
                {
                    builder.AppendLine(exp.ToString(indent + 2));
                }
            }
            else
            {
                builder.Indent(indent + 2).AppendLine("Empty");
            }
            return builder.ToString();
        }
    }
}
