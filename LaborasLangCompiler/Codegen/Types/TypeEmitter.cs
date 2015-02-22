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
        const TypeAttributes DefaultTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

        private ConstructorEmitter instanceConstructor;
        private ConstructorEmitter staticConstructor;
        protected readonly TypeDefinition typeDefinition;

        public AssemblyEmitter Assembly { get; private set; }
        public TypeReference BaseType { get { return typeDefinition.BaseType; } }

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

            if (addToAssembly)
            {
                Assembly.AddType(typeDefinition);
            }
        }

        public void AddMethod(MethodDefinition method)
        {
            CheckForDuplicates(method.Name, method.Parameters);
            typeDefinition.Methods.Add(method);
        }

        public void AddField(FieldDefinition field)
        {
            CheckForDuplicates(field.Name);
            typeDefinition.Fields.Add(field);
            AddTypeToAssemblyIfNeeded(field.FieldType);
        }

        public void AddFieldInitializer(FieldDefinition field, IExpressionNode initializer)
        {
            if (field.IsStatic)
            {
                GetStaticConstructor().AddFieldInitializer(field, initializer);
            }
            else
            {
                GetInstanceConstructor().AddFieldInitializer(field, initializer);
            }
        }

        public void AddProperty(PropertyDefinition property, IExpressionNode initializer = null)
        {
            if (property.SetMethod == null && property.GetMethod == null)
            {
                throw new ArgumentException("Property has neither a setter nor a getter!", "property");
            }

            CheckForDuplicates(property.Name);
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

            AddTypeToAssemblyIfNeeded(property.PropertyType);

            foreach (var parameter in (property.GetMethod ?? property.SetMethod).Parameters)
            {
                AddTypeToAssemblyIfNeeded(parameter.ParameterType);
            }
        }

        public void AddDefaultConstructor()
        {
            GetInstanceConstructor();
        }

        private void CheckForDuplicates(string name)
        {
            if (typeDefinition.Fields.Any(field => field.Name == name))
            {
                throw new InvalidOperationException(string.Format("A field with same name already exists in type {0}.", typeDefinition.FullName));
            }
            else if (typeDefinition.Methods.Any(method => method.Name == name))
            {
                throw new InvalidOperationException(string.Format("A method with same name already exists in type {0}.", typeDefinition.FullName));
            }
        }

        private void CheckForDuplicates(string name, IList<ParameterDefinition> parameters)
        {
            var targetParameterTypes = parameters.Select(parameter => parameter.ParameterType.FullName);

            if (typeDefinition.Methods.Any(method => method.Name == name &&
                method.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(targetParameterTypes)))
            {
                throw new InvalidOperationException(string.Format("A method with same name and parameters already exists in type {0}.", typeDefinition.FullName));
            }
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

        private void AddTypeToAssemblyIfNeeded(TypeReference type)
        {
            if (type.IsFunctorType())
            {
                var typeDef = type.Resolve();

                if (typeDef.Scope == null)
                    Assembly.AddTypeIfNotAdded(typeDef);
            }
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
    }
}
