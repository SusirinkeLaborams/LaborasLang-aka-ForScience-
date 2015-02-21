using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ArrayCreatioNode : ExpressionNode, IArrayCreationNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayCreation; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return true; } }
        public override TypeReference ExpressionReturnType { get { return type; } }

        public IReadOnlyList<IExpressionNode> Dimensions { get; private set; }
        public IReadOnlyList<IExpressionNode> Initializers { get { return initializer.Initializers.ToArray(); } }

        private InitializerList initializer;
        private TypeReference type;

        private ArrayCreatioNode(SequencePoint point)
            : base(point)
        { 
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}
