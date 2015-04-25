using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace LaborasLangCompiler.Codegen.Types
{
    internal class TypeEmitter
    {
        public const TypeAttributes DefaultTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

        private ConstructorEmitter instanceConstructor;
        private ConstructorEmitter staticConstructor;
        protected readonly TypeDefinition typeDefinition;

        public AssemblyEmitter Assembly { get; private set; }
        public TypeReference BaseType { get { return typeDefinition.BaseType; } }
        public bool IsValueType { get { return typeDefinition.IsValueType; } }

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

            Assembly = assembly;

            if (baseType == null)
            {
                baseType = Assembly.TypeSystem.Object;
            }

            typeDefinition = new TypeDefinition(@namespace, className, typeAttributes, baseType);

            // Structs without fields must have specified class and packing size parameters
            if (typeDefinition.IsValueType)
            {
                typeDefinition.IsSequentialLayout = true;
                typeDefinition.ClassSize = 1;
                typeDefinition.PackingSize = 0;
            }

            if (addToAssembly)
            {
                Assembly.AddType(typeDefinition);
            }

            Assembly.AddTypeUsage(baseType);
        }

        public bool AddMethod(MethodDefinition method)
        {
            if (HasMember(method.Name, MemberCheck.FieldAndProperties))
                return false;

            typeDefinition.Methods.Add(method);
            return true;
        }

        public bool AddField(FieldDefinition field)
        {
            if (HasMember(field.Name, MemberCheck.FullCheck))
                return false;

            // If we add any field, reset its class and packing size parameters to unspecified again
            if (typeDefinition.IsValueType && typeDefinition.Fields.Count == 0)
            {
                typeDefinition.ClassSize = -1;
                typeDefinition.PackingSize = -1;
            }

            typeDefinition.Fields.Add(field);
            Assembly.AddTypeUsage(field.FieldType);

            return true;
        }

        public void AddFieldInitializer(FieldDefinition field, IExpressionNode initializer)
        {
            Contract.Assert(field.DeclaringType.FullName == typeDefinition.FullName);
            if (field.IsStatic)
            {
                GetStaticConstructor().AddFieldInitializer(field, initializer);
            }
            else
            {
                GetInstanceConstructor().AddFieldInitializer(field, initializer);
            }
        }

        public bool AddProperty(PropertyDefinition property, IExpressionNode initializer = null)
        {
            Contract.Requires(property.SetMethod != null || property.GetMethod != null, "Property has neither a setter nor a getter!");

            if (HasMember(property.Name, MemberCheck.FullCheck))
                return false;

            typeDefinition.Properties.Add(property);

            bool isStatic = (property.SetMethod != null && property.SetMethod.IsStatic) ||
                (property.GetMethod != null && property.GetMethod.IsStatic);

            if (initializer != null)
            {
                if (isStatic)
                {
                    GetStaticConstructor().AddPropertyInitializer(property, initializer);
                }
                else
                {
                    GetInstanceConstructor().AddPropertyInitializer(property, initializer);
                }
            }

            Assembly.AddTypeUsage(property.PropertyType);
            
            var accessor = property.GetMethod ?? property.SetMethod;

            foreach (var type in accessor.GetParameterTypes())
            {
                Assembly.AddTypeUsage(type);
            }

            return true;
        }

        public void AddDefaultConstructor()
        {
            GetInstanceConstructor();
        }

        public static string ComputeNameFromReturnAndArgumentTypes(TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = new StringBuilder("$" + returnType.FullName);

            foreach (var argument in arguments)
            {
                name.Append("*" + argument.FullName);
            }

            name.Replace('.', '_');
            name.Append('?');

            return name.ToString();
        }

        public static string ComputeNameArgumentTypes(IReadOnlyList<TypeReference> arguments)
        {
            var name = new StringBuilder();

            foreach (var argument in arguments)
            {
                name.Append("$" + argument.FullName);
            }

            name.Replace('.', '_');
            return name.ToString();
        }

        public TypeReference Get(AssemblyEmitter assembly)
        {
            Contract.Ensures(Contract.Result<TypeReference>() != null);
            return AssemblyRegistry.FindType(assembly, typeDefinition.FullName);
        }

        private ConstructorEmitter GetInstanceConstructor()
        {
            if (instanceConstructor == null)
            {
                instanceConstructor = new ConstructorEmitter(this, false);
            }

            return instanceConstructor;
        }

        private ConstructorEmitter GetStaticConstructor()
        {
            if (staticConstructor == null)
            {
                staticConstructor = new ConstructorEmitter(this, true);
            }

            return staticConstructor;
        }

        private enum MemberCheck
        {
            Fields = 0x1,
            Properties = 0x2,
            Methods = 0x4,
            FieldAndProperties = Fields | Properties,
            FullCheck = Fields | Properties | Methods
        }

        private bool HasMember(string name, MemberCheck check)
        {
            if ((check & MemberCheck.Fields) != 0)
            {
                if (typeDefinition.Fields.Any(field => field.Name == name))
                    return true;
            }

            if ((check & MemberCheck.Properties) != 0)
            {
                if (typeDefinition.Properties.Any(property => property.Name == name))
                    return true;
            }

            if ((check & MemberCheck.Methods) != 0)
            {
                if (typeDefinition.Methods.Any(method => method.Name == name))
                    return true;
            }

            return false;
        }
    }
}
