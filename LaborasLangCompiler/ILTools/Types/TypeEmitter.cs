using LaborasLangCompiler.ILTools.Methods;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.Types
{
    internal class TypeEmitter
    {
        const TypeAttributes DefaultTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;

        private ConstructorEmitter instanceConstructor;
        private ConstructorEmitter staticConstructor;
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
            CheckForDuplicates(method.Name);
            typeDefinition.Methods.Add(method);
        }

        public void AddField(FieldDefinition field, IExpressionNode initializer = null)
        {
            CheckForDuplicates(field.Name);
            typeDefinition.Fields.Add(field);

            if (initializer != null)
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
        }

        public void AddProperty(PropertyDefinition property, IExpressionNode initializer = null)
        {
            if (property.SetMethod == null && property.GetMethod == null)
            {
                throw new ArgumentException("Property has neither a setter nor a getter!", "property");
            }

            CheckForDuplicates(property.Name);
            typeDefinition.Properties.Add(property);

            if (initializer != null)
            {
                bool isStatic = (property.SetMethod != null && property.SetMethod.IsStatic) || 
                    (property.GetMethod != null && property.GetMethod.IsStatic);

                if (isStatic)
                {
                    GetStaticConstructor().AddPropertyInitializer(property, initializer);
                }
                else
                {
                    GetInstanceConstructor().AddPropertyInitializer(property, initializer);
                }
            }
        }

        private void CheckForDuplicates(string name)
        {
            if (typeDefinition.Methods.Any(x => x.Name == name))
            {
                throw new InvalidOperationException(string.Format("A method with same name already exists in type {0}.", typeDefinition.FullName));
            }
            else if (typeDefinition.Fields.Any(x => x.Name == name))
            {
                throw new InvalidOperationException(string.Format("A field with same name already exists in type {0}.", typeDefinition.FullName));
            }
            else if (typeDefinition.Methods.Any(x => x.Name == name))
            {
                throw new InvalidOperationException(string.Format("A method with same name already exists in type {0}.", typeDefinition.FullName));
            }
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
