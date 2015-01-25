﻿using LaborasLangCompiler.ILTools;
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

        public override IEnumerable<TypeWrapper> FunctorParamTypes
        {
            get 
            { 
                if(functorParamTypes == null)
                {
                    var tmp = new List<TypeReference>();
                    ILHelpers.GetFunctorReturnTypeAndArguments(Assembly, TypeReference, out tmp);
                    functorParamTypes = tmp.Select(t => new ExternalType(Assembly, t));
                }
                return functorParamTypes;
            }
        }

        public override TypeWrapper DeclaringType
        {
            get { return declaringType; }
        }

        private TypeWrapper declaringType;
        private TypeReference typeReference;
        private TypeWrapper functorReturnType;
        private IEnumerable<TypeWrapper> functorParamTypes;

        public ExternalType(AssemblyEmitter assembly, TypeReference type) : base(assembly)
        {
            this.typeReference = type;
            this.declaringType = CreateType(assembly, typeReference.DeclaringType);
        }

        public ExternalType(AssemblyEmitter assembly, Type type) : base(assembly)
        {
            this.typeReference = assembly.TypeToTypeReference(type);
            this.declaringType = CreateType(assembly, typeReference.DeclaringType);
        }

        public static ExternalType CreateType(AssemblyEmitter assembly, TypeReference type)
        {
            if(type != null)
            {
                return new ExternalType(assembly, type);
            }
            else
            {
                return null;
            }
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

        public override IEnumerable<MethodWrapper> GetMethods(string name)
        {
            return AssemblyRegistry.GetMethods(Assembly, TypeReference, name).Select(m => new ExternalMethod(Assembly, m));
        }
    }
}
