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
        public static TypeDefinition CreateTypeDefinition(string @namespace, string className, TypeAttributes typeAttributes, TypeReference baseType = null)
        {
            TypeDefinition typeDefinition;

            if (baseType != null)
            {
                typeDefinition = new TypeDefinition(@namespace, className, typeAttributes, baseType);
            }
            else
            {
                typeDefinition = new TypeDefinition(@namespace, className, typeAttributes);
            }

            typeDefinition.IsPublic = true;
            typeDefinition.IsClass = true;

            return typeDefinition;
        }
    }
}
