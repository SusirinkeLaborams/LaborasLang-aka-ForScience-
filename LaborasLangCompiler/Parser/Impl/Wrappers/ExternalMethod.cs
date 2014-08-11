using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalMethod : ExternalWrapperBase, MethodWrapper
    {
        public MethodReference MethodReference { get; private set; }
        public TypeWrapper FunctorType 
        {
            get 
            {
                if(functorType == null)
                {
                    functorType = ExternalType.GetFunctorType(Assembly, MethodReference);
                }
                return functorType;
            } 
        }
        public TypeWrapper MethodReturnType { get { return methodReturnType; } }
        public IEnumerable<TypeWrapper> ArgumentTypes
        { 
            get 
            { 
                if(argumentTypes == null)
                {
                    argumentTypes = MethodReference.Parameters.Select(p => new ExternalType(Assembly, p.ParameterType));
                }
                return argumentTypes;
            } 
        }

        private TypeWrapper methodReturnType;
        private TypeWrapper functorType;
        private IEnumerable<TypeWrapper> argumentTypes;
        public ExternalMethod(AssemblyEmitter assembly, MethodReference method) : base(assembly)
        {
            this.MethodReference = method;
            this.methodReturnType = new ExternalType(assembly, method.ReturnType);
        }
    }
}
