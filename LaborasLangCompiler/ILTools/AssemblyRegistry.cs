using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    internal class AssemblyRegistry
    {
        private HashSet<string> assemblyPaths;             // Keep assembly paths to prevent from registering single assembly twice
        private List<AssemblyDefinition> assemblies;
        private Dictionary<string, TypeDefinition> functorTypes;
        private AssemblyDefinition mscorlib;

        private AssemblyRegistry()
        {
            assemblyPaths = new HashSet<string>();
            assemblies = new List<AssemblyDefinition>();
        }

        public AssemblyRegistry(IEnumerable<string> references) : this()
        {
            if (!references.Any(x => Path.GetFileName(x) == "mscorlib.dll"))
            {
                throw new ArgumentException("Assembly registry must reference mscorlib!");
            }

            RegisterReferences(references);

            mscorlib = assemblies.Single(x => x.Name.Name == "mscorlib");
        }

        public void RegisterReferences(IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                RegisterReference(reference);
            }
        }

        public void RegisterReference(string reference)
        {
            if (assemblyPaths.Contains(reference, StringComparer.InvariantCultureIgnoreCase))
            {
                return;
            }

            assemblyPaths.Add(reference);

            try
            {
                assemblies.Add(AssemblyDefinition.ReadAssembly(reference));
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to load managed assembly from {0}:\r\n\t{1}", reference, e.Message));
            }
        }

        public void RegisterAssembly(AssemblyDefinition assemblyDefinition)
        {
            if (assemblyPaths.Contains(assemblyDefinition.MainModule.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                return;
            }

            assemblyPaths.Add(assemblyDefinition.MainModule.Name);
            assemblies.Add(assemblyDefinition);
        }

        public TypeReference ImportType(Type type)
        {
            return mscorlib.MainModule.Import(type);
        }

        #region Type/Method/Property/Field getters

        private TypeDefinition GetType(IList<TypeDefinition> types, string typeName)
        {
            foreach (var type in types)
            {
                if (type.FullName == typeName)
                {
                    return type;
                }

                if (type.HasNestedTypes)
                {
                    var nestedType = GetType(type.NestedTypes, typeName);
                    if (nestedType != null)
                    {
                        return nestedType;
                    }
                }
            }

            return null;
        }

        public TypeDefinition GetType(string typeName)
        {
            foreach (var assembly in assemblies)
            {
                var type = GetType(assembly.MainModule.Types, typeName);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        public TypeDefinition GetType(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = FunctorTypeEmitter.ComputeNameFromReturnAndArgumentTypes(returnType, arguments);

            if (!functorTypes.ContainsKey(name))
            {
                functorTypes.Add(name, FunctorTypeEmitter.Create(this, assembly, returnType, arguments));
            }

            return functorTypes[name];
        }

        public bool TypeIsKnown(string typeName)
        {
            return GetType(typeName) != null;
        }

        public IList<MethodDefinition> GetMethods(string typeName, string methodName)
        {
            return GetMethods(GetType(typeName), methodName);
        }

        public IList<MethodDefinition> GetMethods(TypeDefinition type, string methodName)
        {
            if (!type.HasMethods)
            {
                return null;
            }

            return type.Methods.Where(x => x.Name == methodName).ToList<MethodDefinition>();
        }

        public PropertyDefinition GetProperty(string typeName, string propertyName)
        {
            return GetProperty(GetType(typeName), propertyName);
        }

        public PropertyDefinition GetProperty(TypeDefinition type, string propertyName)
        {
            if (!type.HasProperties)
            {
                return null;
            }

            return type.Properties.SingleOrDefault(x => x.Name == propertyName);
        }

        public FieldDefinition GetField(string typeName, string fieldName)
        {
            return GetField(GetType(fieldName), fieldName);
        }

        public FieldDefinition GetField(TypeDefinition type, string fieldName)
        {
            if (!type.HasFields)
            {
                return null;
            }

            return type.Fields.SingleOrDefault(x => x.Name == fieldName);
        }

        #endregion
    }
}
