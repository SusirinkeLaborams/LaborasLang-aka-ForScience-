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
        public static TypeDefinition CreateTypeDefinition(AssemblyEmitter assembly, string @namespace, string className, 
                                                            TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, 
                                                            TypeReference baseType = null)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            
            TypeDefinition typeDefinition;
            
            if (baseType == null)
            {
                baseType = assembly.ImportType(typeof(object));
            }

            typeDefinition = new TypeDefinition(@namespace, className, typeAttributes, baseType);

            assembly.AddType(typeDefinition);

            return typeDefinition;
        }
    }
}
