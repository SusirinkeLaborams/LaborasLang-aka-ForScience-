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

namespace LaborasLangCompiler.Parser.Impl
{
    class ArrayAccessNode : ExpressionNode, IArrayAccessNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayAccess; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return true; } }

        public override TypeReference ExpressionReturnType { get { return type; } }
        public IExpressionNode ObjectInstance { get { return array; } }

        public IReadOnlyList<IExpressionNode> Indices { get; private set; }

        private ExpressionNode array;
        private TypeReference type;

        private ArrayAccessNode(ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point) : base(point)
        {
            Contract.Requires(array.ExpressionReturnType is ArrayType);
            this.array = array;
            this.Indices = indices;
            this.type = ((ArrayType)array.ExpressionReturnType).ElementType;
        }

        public static ExpressionNode Parse(ContextNode context, AstNode lexerNode)
        {
            var array = ExpressionNode.Parse(context, lexerNode.Children[0]);
            throw new NotImplementedException();
        }

        public static ExpressionNode Create(ContextNode context, ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
        {
            foreach(var index in indices)
            {
                if(!index.IsGettable || 
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
            var args = indices.Select(a => a.ExpressionReturnType).ToArray();
            var setter = AssemblyRegistry.GetCompatibleMethod(context.Assembly, array.ExpressionReturnType, "set_Item", args);
            var getter = AssemblyRegistry.GetCompatibleMethod(context.Assembly, array.ExpressionReturnType, "get_Item", args);

            if(getter == null && setter == null)
                return null;

            if(getter != null && getter.IsStatic())
            {
                return null;
            }

            if(setter != null && setter.IsStatic())
            {
                return null;
            }

            var getType = getter != null ? getter.ReturnType : null;
            var setType = setter != null ? setter.ReturnType : null;

            if(getType != null && setType != null)
            {
                if(!getType.TypeEquals(setType))
                {
                    return null;
                }
            }

            return new IndexOperatorAccessNode(context, array, getType, getter, setter, indices, point);
        }

        private static ArrayCreationNode AsArrayCreation(ContextNode context, ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
        {
            var type = array as TypeNode;
            if (type == null)
                return null;

            return ArrayCreationNode.Create(context, type.ParsedType, indices, null, point);
        }

        [Pure]
        public static bool IsEmptyIndexer(AstNode lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.IndexNode);
            return lexerNode.Children.AsEnumerable().Count(n => n.Type == Lexer.TokenType.Value) == 0;
        }

        public static int CountEmptyIndexerDims(AstNode lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.IndexNode);
            Contract.Requires(IsEmptyIndexer(lexerNode));

            return lexerNode.Children.AsEnumerable().Count(n => n.Type == Lexer.TokenType.Comma) + 1;
        }

        public static IReadOnlyList<ExpressionNode> ParseIndex(ContextNode context, AstNode lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.IndexNode);
            Contract.Requires(!IsEmptyIndexer(lexerNode));

            return lexerNode.Children.AsEnumerable().Where(n => n.Type == Lexer.TokenType.Value).Select(n => ExpressionNode.Parse(context, n)).ToArray();
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}
