using LaborasLangCompiler.ILTools.Types;
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
        private static AssemblyRegistry instance;

        private HashSet<string> assemblyPaths;             // Keep assembly paths to prevent from registering single assembly twice
        private List<AssemblyDefinition> assemblies;
        private Dictionary<string, TypeDefinition> functorTypes;
        private AssemblyDefinition mscorlib;

        private AssemblyRegistry()
        {
            instance = this;

            assemblyPaths = new HashSet<string>();
            assemblies = new List<AssemblyDefinition>();
            functorTypes = new Dictionary<string, TypeDefinition>();
        }

        private AssemblyRegistry(IEnumerable<string> references)
            : this()
        {
            if (!references.Any(x => Path.GetFileName(x) == "mscorlib.dll"))
            {
                throw new ArgumentException("Assembly registry must reference mscorlib!");
            }

            RegisterReferences(references);

            mscorlib = assemblies.Single(x => x.Name.Name == "mscorlib");
        }

        public static void Create(IEnumerable<string> references)
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Assembly registry is already created!");
            }

            new AssemblyRegistry(references);
        }

        public static void RegisterReferences(IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                RegisterReference(reference);
            }
        }

        public static void RegisterReference(string reference)
        {
            if (instance.assemblyPaths.Contains(reference, StringComparer.InvariantCultureIgnoreCase))
            {
                return;
            }

            try
            {
                instance.assemblies.Add(AssemblyDefinition.ReadAssembly(reference));
                instance.assemblyPaths.Add(reference);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Unable to load managed assembly from {0}:\r\n\t{1}", reference, e.Message));
            }
        }

        public static void RegisterAssembly(AssemblyDefinition assemblyDefinition)
        {
            if (instance.assemblyPaths.Contains(assemblyDefinition.MainModule.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                return;
            }

            instance.assemblyPaths.Add(assemblyDefinition.MainModule.Name);
            instance.assemblies.Add(assemblyDefinition);
        }

        public static TypeReference ImportType(Type type)
        {
            return instance.mscorlib.MainModule.Import(type);
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

        public static TypeDefinition GetType(string typeName)
        {
            foreach (var assembly in instance.assemblies)
            {
                var type = instance.GetType(assembly.MainModule.Types, typeName);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        public static TypeDefinition GetType(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = FunctorTypeEmitter.ComputeNameFromReturnAndArgumentTypes(returnType, arguments);

            if (!instance.functorTypes.ContainsKey(name))
            {
                instance.functorTypes.Add(name, FunctorTypeEmitter.Create(assembly, returnType, arguments));
            }

            return instance.functorTypes[name];
        }

        public static bool TypeIsKnown(string typeName)
        {
            return GetType(typeName) != null;
        }

        public static IList<MethodDefinition> GetMethods(string typeName, string methodName)
        {
            return GetMethods(GetType(typeName), methodName);
        }

        public static IList<MethodDefinition> GetMethods(TypeDefinition type, string methodName)
        {
            if (!type.HasMethods)
            {
                return null;
            }

            return type.Methods.Where(x => x.Name == methodName).ToList<MethodDefinition>();
        }

        public static PropertyDefinition GetProperty(string typeName, string propertyName)
        {
            return GetProperty(GetType(typeName), propertyName);
        }

        public static PropertyDefinition GetProperty(TypeDefinition type, string propertyName)
        {
            if (!type.HasProperties)
            {
                return null;
            }

            return type.Properties.SingleOrDefault(x => x.Name == propertyName);
        }

        public static FieldDefinition GetField(string typeName, string fieldName)
        {
            return GetField(GetType(fieldName), fieldName);
        }

        public static FieldDefinition GetField(TypeDefinition type, string fieldName)
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
