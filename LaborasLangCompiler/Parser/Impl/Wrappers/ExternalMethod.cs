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
                    functorType = new FunctorTypeWrapper(Assembly, MethodReturnType, ParamTypes);
                }
                return functorType;
            } 
        }
        public TypeWrapper MethodReturnType { get { return methodReturnType; } }
        public IEnumerable<TypeWrapper> ParamTypes
        { 
            get 
            { 
                if(paramTypes == null)
                {
                    paramTypes = MethodReference.Parameters.Select(p => new ExternalType(Assembly, p.ParameterType));
                }
                return paramTypes;
            } 
        }
        public bool IsStatic { get { return MethodReference.Resolve().IsStatic; } }
        public TypeWrapper DeclaringType { get; private set; }

        private TypeWrapper methodReturnType;
        private FunctorTypeWrapper functorType;
        private IEnumerable<TypeWrapper> paramTypes;
        public ExternalMethod(AssemblyEmitter assembly, MethodReference method) : base(assembly)
        {
            this.MethodReference = method;
            this.methodReturnType = new ExternalType(assembly, method.ReturnType);
            this.DeclaringType = new ExternalType(assembly, method.DeclaringType);
        }
    }
}
