using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    [ContractClass(typeof(ContextNodeContract))]
    abstract class ContextNode : ParserNode
    {
        public ContextNode Parent { get; private set; }
        public Parser Parser { get; private set; }
        public AssemblyEmitter Assembly { get { return Parser.Assembly; } }

        [Pure]
        public abstract FunctionDeclarationNode GetMethod();
        public abstract ClassNode GetClass();
        public abstract bool IsStaticContext();
        public abstract ExpressionNode GetSymbol(string name, ContextNode scope, SequencePoint point);

        protected ContextNode(Parser parser, ContextNode parent, SequencePoint point) : base(point)
        {
            Parser = parser;
            Parent = parent;
        }
    }

    [ContractClassFor(typeof(ContextNode))]
    abstract class ContextNodeContract : ContextNode
    {
        private ContextNodeContract() : base(null, null, null) { }
        public override ClassNode GetClass()
        {
            Contract.Ensures(Contract.Result<ClassNode>() != null);
            throw new NotImplementedException();
        }

        public override FunctionDeclarationNode GetMethod()
        {
            Contract.Ensures(GetMethod().MethodReturnType != null);
            throw new NotImplementedException();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Parser != null);
        }
    }
}
