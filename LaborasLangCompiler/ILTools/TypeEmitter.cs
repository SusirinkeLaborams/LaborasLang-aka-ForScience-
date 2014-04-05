using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    internal class TypeEmitter
    {
        const TypeAttributes DefaultTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

        private TypeDefinition typeDefinition;

        public ModuleDefinition Module { get { return typeDefinition.Module; } }

        public TypeEmitter(AssemblyEmitter assembly, string className, string @namespace = "",
                            TypeAttributes typeAttributes = DefaultTypeAttributes, TypeReference baseType = null)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
                                    
            if (baseType == null)
            {
                baseType = assembly.ImportType(typeof(object));
            }

            typeDefinition = new TypeDefinition(@namespace, className, typeAttributes, baseType);

            assembly.AddType(typeDefinition);
        }

        public void AddMethod(MethodDefinition method)
        {
            typeDefinition.Methods.Add(method);
        }
    }
}
