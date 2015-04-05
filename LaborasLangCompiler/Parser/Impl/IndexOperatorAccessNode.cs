using LaborasLangCompiler.Parser.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class IndexOperatorAccessNode : ExpressionNode, IArrayAccessNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayAccess; } }

        public override TypeReference ExpressionReturnType { get { return type; } }
        public IExpressionNode ObjectInstance { get { return array; } }

        public IReadOnlyList<IExpressionNode> Indices { get; private set; }

        public override bool IsSettable
        {
            get { return setter != null && TypeUtils.IsAccessbile(setter, context.GetClass().TypeReference); }
        }

        public override bool IsGettable
        {
            get { return getter != null && TypeUtils.IsAccessbile(getter, context.GetClass().TypeReference); }
        }

        private ExpressionNode array;
        private TypeReference type;
        private MethodReference getter;
        private MethodReference setter;
        private ContextNode context;

        internal IndexOperatorAccessNode(ContextNode context, ExpressionNode array, TypeReference elementType, 
            MethodReference getter, MethodReference setter, IReadOnlyList<ExpressionNode> indices, SequencePoint point)
            : base(point)
        {
            this.context = context;
            this.array = array;
            this.Indices = indices;
            this.type = elementType;
            this.getter = getter;
            this.setter = setter;
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}
