using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    [Obsolete]
    class ExternalMethod : ExternalWrapperBase, MethodWrapper
    {
        public MethodReference MethodReference { get; private set; }
        public TypeReference FunctorType 
        {
            get 
            {
                if(functorType == null)
                {
                    functorType = AssemblyRegistry.GetFunctorType(Assembly, MethodReturnType, ParamTypes.ToList());
                }
                return functorType;
            } 
        }
        public TypeReference MethodReturnType { get; private set; }
        public IEnumerable<TypeReference> ParamTypes
        { 
            get 
            { 
                if(paramTypes == null)
                {
                    paramTypes = MethodReference.Parameters.Select(p => p.ParameterType);
                }
                return paramTypes;
            } 
        }
        public bool IsStatic { get { return MethodReference.Resolve().IsStatic; } }
        public MemberReference MemberReference { get { return MethodReference; } }
        public TypeReference DeclaringType { get; private set; }

        private TypeReference functorType;
        private IEnumerable<TypeReference> paramTypes;

        public ExternalMethod(AssemblyEmitter assembly, MethodReference method) : base(assembly)
        {
            this.MethodReference = method;
            MethodReturnType = method.ReturnType;
            DeclaringType = method.DeclaringType;

        }
    }
}
