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

            new AssemblyRegistry(references);   // Sue me
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
        
        #region Type/Method/Property/Field getters

        public static bool IsTypeKnown(string typeName)
        {
            return GetTypeInternal(typeName) != null;
        }

        public static TypeReference GetType(AssemblyEmitter assemblyScope, string typeName)
        {
            var type = GetTypeInternal(typeName);

            if (type == null)
            {
                return null;
            }

            return ScopeToAssembly(assemblyScope, type);
        }

        public static TypeDefinition GetFunctorType(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = FunctorTypeEmitter.ComputeNameFromReturnAndArgumentTypes(returnType, arguments);

            if (!instance.functorTypes.ContainsKey(name))
            {
                instance.functorTypes.Add(name, FunctorTypeEmitter.Create(assembly, returnType, arguments));
            }

            return instance.functorTypes[name];
        }

        public static IList<MethodReference> GetMethods(AssemblyEmitter assembly, string typeName, string methodName)
        {
            return GetMethods(assembly, GetTypeInternal(typeName), methodName);
        }

        public static IList<MethodReference> GetMethods(AssemblyEmitter assembly, TypeReference type, string methodName)
        {
            var resolvedType = type.Resolve();

            if (!resolvedType.HasMethods)
            {
                return null;
            }            

            return resolvedType.Methods.Where(x => x.Name == methodName).Select(x => ScopeToAssembly(assembly, x)).ToList<MethodReference>();
        }

        public static PropertyDefinition GetProperty(AssemblyEmitter assembly, string typeName, string propertyName)
        {
            return GetProperty(assembly, GetTypeInternal(typeName), propertyName);
        }

        public static PropertyDefinition GetProperty(AssemblyEmitter assembly, TypeDefinition type, string propertyName)
        {
            if (!type.HasProperties)
            {
                return null;
            }

            return type.Properties.SingleOrDefault(x => x.Name == propertyName);
        }

        public static MethodReference GetPropertyGetter(AssemblyEmitter assembly, PropertyReference property)
        {
            var resolvedProperty = property.Resolve();

            if (resolvedProperty.GetMethod == null)
            {
                return null;
            }

            return ScopeToAssembly(assembly, resolvedProperty.GetMethod);
        }

        public static MethodReference GetPropertySetter(AssemblyEmitter assembly, PropertyReference property)
        {
            var resolvedProperty = property.Resolve();

            if (resolvedProperty.SetMethod == null)
            {
                return null;
            }

            return ScopeToAssembly(assembly, resolvedProperty.SetMethod);
        }

        public static FieldReference GetField(AssemblyEmitter assembly, string typeName, string fieldName)
        {
            return GetField(assembly, GetTypeInternal(fieldName), fieldName);
        }

        public static FieldReference GetField(AssemblyEmitter assembly, TypeDefinition type, string fieldName)
        {
            if (!type.HasFields)
            {
                return null;
            }

            return ScopeToAssembly(assembly, type.Fields.SingleOrDefault(x => x.Name == fieldName));
        }

        #endregion

        #region Privates

        private TypeDefinition GetTypeInternal(IList<TypeDefinition> types, string typeName)
        {
            foreach (var type in types)
            {
                if (type.FullName == typeName)
                {
                    return type;
                }

                if (type.HasNestedTypes)
                {
                    var nestedType = GetTypeInternal(type.NestedTypes, typeName);
                    if (nestedType != null)
                    {
                        return nestedType;
                    }
                }
            }

            return null;
        }

        private static TypeDefinition GetTypeInternal(string typeName)
        {
            foreach (var assembly in instance.assemblies)
            {
                var type = instance.GetTypeInternal(assembly.MainModule.Types, typeName);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static TypeReference ScopeToAssembly(AssemblyEmitter assemblyScope, TypeReference reference)
        {
            var module = assemblyScope.MainModule;

            if ((reference.Scope.MetadataScopeType != MetadataScopeType.ModuleDefinition) || (ModuleDefinition)reference.Scope != module)
            {
                return module.Import(reference);
            }
            else
            {
                return reference.Resolve();
            }
        }

        private static MethodReference ScopeToAssembly(AssemblyEmitter assemblyScope, MethodReference reference)
        {
            var module = assemblyScope.MainModule;

            if ((reference.DeclaringType.Scope.MetadataScopeType != MetadataScopeType.ModuleDefinition) ||
                    (ModuleDefinition)reference.DeclaringType.Scope != module)
            {
                return module.Import(reference);
            }
            else
            {
                return reference.Resolve();
            }
        }

        private static FieldReference ScopeToAssembly(AssemblyEmitter assemblyScope, FieldReference reference)
        {
            var module = assemblyScope.MainModule;

            if ((reference.DeclaringType.Scope.MetadataScopeType != MetadataScopeType.ModuleDefinition) ||
                    (ModuleDefinition)reference.DeclaringType.Scope != module)
            {
                return module.Import(reference);
            }
            else
            {
                return reference.Resolve();
            }
        }

        #endregion
    }
}
