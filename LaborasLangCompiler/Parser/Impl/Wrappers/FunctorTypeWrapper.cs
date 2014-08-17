using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class FunctorTypeWrapper : TypeWrapper
    {
        public override TypeReference TypeReference 
        {
            get
            {
                if(typeReference == null)
                {
                    typeReference = AssemblyRegistry.GetFunctorType(Assembly, FunctorReturnType.TypeReference, FunctorArgumentTypes.Select(t => t.TypeReference).ToList());
                }
                return typeReference;
            }
        }
        public override string FullName { get { return TypeReference.FullName; } }
        public override TypeWrapper FunctorReturnType { get { return functorReturnType; } }
        public override IEnumerable<TypeWrapper> FunctorArgumentTypes { get { return functorArgumentTypes; } }

        private TypeReference typeReference;
        private TypeWrapper functorReturnType;
        private IEnumerable<TypeWrapper> functorArgumentTypes;

        public FunctorTypeWrapper(AssemblyEmitter assembly, TypeWrapper returnType, IEnumerable<TypeWrapper> args) : base(assembly)
        {
            this.functorReturnType = returnType;
            this.functorArgumentTypes = args;
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
        public override MethodWrapper GetMethod(string name) { return null; }
        public override IEnumerable<MethodWrapper> GetMethods(string name) { return new List<MethodWrapper>(); }
        #endregion class stuff
    }
}
