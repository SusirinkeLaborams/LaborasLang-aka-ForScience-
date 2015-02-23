using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Codegen.Methods
{
    [ContractClass(typeof(IMethodEmitterContract))]
    interface IMethodEmitter
    {
        bool Parsed { get; }
        [Pure]
        MethodReference Get();
        void ParseTree(ICodeBlockNode tree);
        void SetAsEntryPoint();
        ParameterDefinition AddArgument(ParameterDefinition parameter);
    }

    [ContractClassFor(typeof(IMethodEmitter))]
    abstract class IMethodEmitterContract : IMethodEmitter
    {
        public abstract bool Parsed { get; }

        public MethodReference Get()
        {
            Contract.Ensures(Contract.Result<MethodReference>() != null);
            throw new NotImplementedException();
        }

        public void ParseTree(ICodeBlockNode tree)
        {
            Contract.Requires(!Parsed, "Can't set same method twice.");
            throw new NotImplementedException();
        }

        public void SetAsEntryPoint()
        {
            Contract.Requires(!Get().HasThis, "Entry point must be static.");
            throw new NotImplementedException();
        }

        public abstract ParameterDefinition AddArgument(ParameterDefinition parameter);
    }
}
