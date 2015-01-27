using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    /*class FunctorTypeWrapper : TypeWrapper
    {
        public override TypeReference TypeReference { get { return typeReference.Value; } }
        public override string FullName { get { return TypeReference.FullName; } }
        public override TypeWrapper FunctorReturnType { get { return functorReturnType; } }
        public override IEnumerable<TypeWrapper> FunctorParamTypes { get { return functorParamTypes; } }
        public override TypeWrapper DeclaringType
        {
            get { return declaringType; }
        }

        private TypeWrapper declaringType;
        private Lazy<TypeReference> typeReference;
        private TypeWrapper functorReturnType;
        private IEnumerable<TypeWrapper> functorParamTypes;

        public FunctorTypeWrapper(AssemblyEmitter assembly, TypeWrapper returnType, IEnumerable<TypeWrapper> parameters) : base(assembly)
        {
            this.functorReturnType = returnType;
            this.functorParamTypes = parameters;
            this.typeReference = new Lazy<TypeReference>(() => AssemblyRegistry.GetFunctorType(Assembly, FunctorReturnType.TypeReference, FunctorParamTypes.Select(t => t.TypeReference).ToList()));
            this.declaringType = ExternalType.CreateType(assembly, TypeReference.DeclaringType);
        }

        //tipo optimizacija
        public override bool IsFunctorType()
        { 
            return true; 
        }

        #region class stuff
        //pagal parser, functor nelabai tipas
        public override FieldWrapper GetField(string name) { return null; }
        public override TypeWrapper GetContainedType(string name) { return null; }
        public override IEnumerable<MethodWrapper> GetMethods(string name) { return new List<MethodWrapper>(); }
        #endregion class stuff
    }*/
}
