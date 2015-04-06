using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using Lexer.Containers;
using System.Diagnostics.Contracts;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class ArrayAccessNode : ExpressionNode, IArrayAccessNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayAccess; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return true; } }

        public override TypeReference ExpressionReturnType { get { return type; } }
        public IExpressionNode ObjectInstance { get { return array; } }

        public IReadOnlyList<IExpressionNode> Indices { get { return indices; } }

        private readonly ExpressionNode array;
        private readonly TypeReference type;
        private readonly IReadOnlyList<ExpressionNode> indices;

        private ArrayAccessNode(ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point) : base(point)
        {
            Contract.Requires(array.ExpressionReturnType is ArrayType);
            this.array = array;
            this.indices = indices;
            this.type = ((ArrayType)array.ExpressionReturnType).ElementType;
        }

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Children[1].Type == Lexer.TokenType.IndexNode);
            var point = context.Parser.GetSequencePoint(lexerNode);
            var array = ExpressionNode.Parse(context, lexerNode.Children[0]);
            var indexer = lexerNode.Children[1];

            if (IsEmptyIndexer(indexer))
            {
                ErrorCode.MissingArraySize.ReportAndThrow(point, "Can only use implicit array size with initialization");
            }

            var init = ParseIndex(context, indexer);
            return Create(context, array, init, point);
        }

        public static ExpressionNode Create(ContextNode context, ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
        {
            foreach (var index in indices)
            {
                if (!index.IsGettable ||
                    !(index.ExpressionReturnType.IsAssignableTo(context.Parser.Int32) || index.ExpressionReturnType.IsAssignableTo(context.Parser.UInt32)))
                {
                    ErrorCode.InvalidIndexType.ReportAndThrow(index.SequencePoint, "Invalid index, must be a gettable integer");
                }
            }

            if (!array.IsGettable)
                ErrorCode.NotAnRValue.ReportAndThrow(point, "Left operand for [] operator must be gettable");

            ExpressionNode result = AsArrayCreation(context, array, indices, point);
            if (result != null)
                return result;

            result = AsIndexOp(context, array, indices, point);
            if (result != null)
                return result;

            result = AsArray(context, array, indices, point);
            if (result != null)
                return result;

            ErrorCode.CannotIndex.ReportAndThrow(point, "Cannot use operator[], not type, array or object with overloaded [] operator");
            return null;//unreachable
        }

        private static ArrayAccessNode AsArray(ContextNode context, ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
        {
            var type = array.ExpressionReturnType as ArrayType;
            if (type == null)
                return null;

            if (indices.Count != type.Rank)
                ErrorCode.InvalidIndexCount.ReportAndThrow(point, "Invalid array indexing, rank is {0}, index count is {1}", type.Rank, indices.Count);

            return new ArrayAccessNode(array, indices, point);
        }

        private static IndexOperatorAccessNode AsIndexOp(ContextNode context, ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
        {
            var itemProperty = AssemblyRegistry.GetCompatibleProperty(array.ExpressionReturnType, "Item", indices.Select(i => i.ExpressionReturnType).ToArray());
            if(itemProperty != null)
            {
                return new IndexOperatorAccessNode(context, array, itemProperty, indices, point);
            }
            else
            {
                return null;
            }
        }

        private static ArrayCreationNode AsArrayCreation(ContextNode context, ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
        {
            var type = array as TypeNode;
            if (type == null)
                return null;

            return ArrayCreationNode.Create(context, type.ParsedType, indices, null, point);
        }

        [Pure]
        public static bool IsEmptyIndexer(IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.IndexNode);
            return lexerNode.Children.AsEnumerable().Count(n => n.Type == Lexer.TokenType.Value) == 0;
        }

        public static int CountEmptyIndexerDims(IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.IndexNode);
            Contract.Requires(IsEmptyIndexer(lexerNode));

            return lexerNode.Children.AsEnumerable().Count(n => n.Type == Lexer.TokenType.Comma) + 1;
        }

        public static IReadOnlyList<ExpressionNode> ParseIndex(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.IndexNode);
            Contract.Requires(!IsEmptyIndexer(lexerNode));

            return lexerNode.Children.AsEnumerable().Where(n => n.Type == Lexer.TokenType.Value).Select(n => ExpressionNode.Parse(context, n)).ToArray();
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("ArrayAccess:");
            builder.Indent(indent + 1).AppendLine("Array:");
            builder.Append(array.ToString(indent + 2)).AppendLine();
            builder.Indent(indent + 1).AppendFormat("ElementType: {0}", type.FullName).AppendLine();
            builder.Indent(indent + 1).AppendLine("Indices:");
            foreach (var ind in indices)
            {
                builder.AppendLine(ind.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
