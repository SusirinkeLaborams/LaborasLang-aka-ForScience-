using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    [ContractClass(typeof(IAmbiguousNodeContract))]
    interface IAmbiguousNode : IExpressionNode
    {
        ExpressionNode RemoveAmbiguity(ContextNode context, TypeReference expectedType);
    }

    [ContractClassFor(typeof(IAmbiguousNode))]
    abstract class IAmbiguousNodeContract : IAmbiguousNode
    {
        public ExpressionNode RemoveAmbiguity(ContextNode context, TypeReference expectedType)
        {
            Contract.Ensures(Contract.Result<ExpressionNode>() != null);
            throw new NotImplementedException();
        }

        public abstract ExpressionNodeType ExpressionType { get; }
        public abstract TypeReference ExpressionReturnType { get; }
        public abstract NodeType Type { get; }
        public abstract Mono.Cecil.Cil.SequencePoint SequencePoint { get; }
    }
}
