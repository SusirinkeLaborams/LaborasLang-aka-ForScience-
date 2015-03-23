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
        public IExpressionNode Array { get { return array; } }

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

        public static ArrayAccessNode Create(ContextNode context, ExpressionNode array, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
        {
            foreach(var index in indices)
            {
                if(!index.IsGettable || !index.ExpressionReturnType.IsIntegerType())
                {
                    ErrorCode.InvalidIndexType.ReportAndThrow(index.SequencePoint, "Invalid index, must be a gettable integer");
                }
            }

            var arrayType = array.ExpressionReturnType as ArrayType;
            throw new NotImplementedException();
            //fuck it
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

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}
