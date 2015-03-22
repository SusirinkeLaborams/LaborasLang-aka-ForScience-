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

namespace LaborasLangCompiler.Parser.Impl
{
    class ArrayAccessNode : ExpressionNode, IArrayAccessNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayAccess; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return false; } }

        public override TypeReference ExpressionReturnType { get { return type; } }
        public IExpressionNode Array { get { return array; } }

        public IReadOnlyList<IExpressionNode> Indices { get; private set; }

        private ExpressionNode array;
        private TypeReference type;

        private ArrayAccessNode(ExpressionNode array, IReadOnlyList<ExpressionNode> indices, TypeReference type, SequencePoint point) : base(point)
        {
            this.array = array;
            this.Indices = indices;
            this.type = type;
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
                    ErrorCode.InvalidIndex.ReportAndThrow(index.SequencePoint, "Invalid index, must be a gettable integer");
                }
            }

            var arrayType = array.ExpressionReturnType as ArrayType;
            throw new NotImplementedException();
            //fuck it
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}
