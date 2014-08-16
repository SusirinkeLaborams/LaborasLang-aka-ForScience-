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
        public FunctorTypeWrapper FunctorType 
        {
            get 
            {
                if(functorType == null)
                {
                    functorType = new FunctorTypeWrapper(Assembly, MethodReturnType, ArgumentTypes);
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
        public bool IsStatic { get { return MethodReference.Resolve().IsStatic; } }

        private TypeWrapper methodReturnType;
        private FunctorTypeWrapper functorType;
        private IEnumerable<TypeWrapper> argumentTypes;
        public ExternalMethod(AssemblyEmitter assembly, MethodReference method) : base(assembly)
        {
            this.MethodReference = method;
            this.methodReturnType = new ExternalType(assembly, method.ReturnType);
        }
    }
}
