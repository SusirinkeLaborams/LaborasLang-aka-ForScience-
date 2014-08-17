using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class ExternalType : TypeWrapper
    {
        public override TypeReference TypeReference { get { return typeReference; } }

        public override string FullName { get { return typeReference.FullName; } }

        public override TypeWrapper FunctorReturnType
        {
            get 
            { 
                if(functorReturnType == null)
                {
                    functorReturnType = new ExternalType(Assembly, ILHelpers.GetFunctorReturnType(Assembly, typeReference));
                }
                return functorReturnType;
            }
        }

        public override IEnumerable<TypeWrapper> FunctorArgumentTypes
        {
            get 
            { 
                if(functorArgumentTypes == null)
                {
                    var tmp = new List<TypeReference>();
                    ILHelpers.GetFunctorReturnTypeAndArguments(Assembly, TypeReference, out tmp);
                    functorArgumentTypes = tmp.Select(t => new ExternalType(Assembly, t));
                }
                return functorArgumentTypes;
            }
        }

        private TypeReference typeReference;
        private TypeWrapper functorReturnType;
        private IEnumerable<TypeWrapper> functorArgumentTypes;

        public ExternalType(AssemblyEmitter assembly, TypeReference type) : base(assembly)
        {
            this.typeReference = type;
        }

        public ExternalType(AssemblyEmitter assembly, Type type) : base(assembly)
        {
            this.typeReference = assembly.TypeToTypeReference(type);
        }

        public override TypeWrapper GetContainedType(string name)
        {
            var type = AssemblyRegistry.FindType(Assembly, TypeReference.FullName + "." + name);
            if (type != null)
                return new ExternalType(Assembly, type);
            else
                return null;
        }
        public override FieldWrapper GetField(string name)
        {
            var field = AssemblyRegistry.GetField(Assembly, TypeReference, name);
            if (field != null)
                return new ExternalField(Assembly, field);
            else
                return null;
        }
        public override MethodWrapper GetMethod(string name)
        {
            var method = AssemblyRegistry.GetMethod(Assembly, TypeReference, name);
            if(method != null)
                return new ExternalMethod(Assembly, method);
            return null;
        }
        public override IEnumerable<MethodWrapper> GetMethods(string name)
        {
            return AssemblyRegistry.GetMethods(Assembly, TypeReference, name).Select(m => new ExternalMethod(Assembly, m));
        }
    }
}
