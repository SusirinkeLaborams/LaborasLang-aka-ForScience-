using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalMethod : MethodWrapper
    {
        public MethodReference MethodReference { get; private set; }
        public TypeReference ReturnType { get { return ILTools.AssemblyRegistry.GetFunctorType(assembly, MethodReference); } }
        public TypeReference MethodReturnType { get { return MethodReference.ReturnType; } }
        public IEnumerable<TypeReference> ArgumentTypes { get { return MethodReference.Parameters.Select(p => p.ParameterType); } }

        private AssemblyEmitter assembly;
        public ExternalMethod(MethodReference method, AssemblyEmitter assembly)
        {
            this.MethodReference = method;
            this.assembly = assembly;
        }
    }
}
