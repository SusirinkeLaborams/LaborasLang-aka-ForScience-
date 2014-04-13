using LaborasLangCompiler.Parser.Tree;
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

        protected TypeDefinition typeDefinition;

        public ModuleDefinition Module { get { return typeDefinition.Module; } }

        public TypeEmitter(AssemblyEmitter assembly, string className, string @namespace = "",
                            TypeAttributes typeAttributes = DefaultTypeAttributes, TypeReference baseType = null) :
            this(assembly, className, @namespace, typeAttributes, baseType, true)
        {
        }

        protected TypeEmitter(AssemblyEmitter assembly, string className, string @namespace, TypeAttributes typeAttributes,
            TypeReference baseType, bool addToAssembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            if (baseType == null)
            {
                baseType = assembly.ImportType(typeof(object));
            }
            else
            {
                baseType = assembly.ImportType(baseType);
            }

            typeDefinition = new TypeDefinition(@namespace, className, typeAttributes, baseType);

            if (addToAssembly)
            {
                assembly.AddType(typeDefinition);
            }
        }

        public void AddMethod(MethodDefinition method)
        {
            typeDefinition.Methods.Add(method);
        }

        public void AddField(FieldDefinition field)
        {
            typeDefinition.Fields.Add(field);
        }

        public void AddProperty(PropertyDefinition property)
        {
            typeDefinition.Properties.Add(property);
        }

        public void AddInitializer(FieldDefinition field, IExpressionNode initializer)
        {
            throw new NotImplementedException();
        }

        public void AddInitializer(PropertyDefinition property, IExpressionNode initializer)
        {
            throw new NotImplementedException();
        }

        public static string ComputeNameFromReturnAndArgumentTypes(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = new StringBuilder("$" + returnType.FullName);

            foreach (var argument in arguments)
            {
                name.Append("$" + argument.FullName);
            }

            name.Replace('.', '_');
            return name.ToString();
        }
    }
}
