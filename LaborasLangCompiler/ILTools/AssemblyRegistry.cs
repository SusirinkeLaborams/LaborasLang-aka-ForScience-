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

            CreateAndOverrideIfNeeded(references);
        }

        internal static void CreateAndOverrideIfNeeded(IEnumerable<string> references)
        {
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

        public static MethodReference GetBestMatch(IReadOnlyList<TypeReference> arguments, List<MethodReference> methods)
        {
            if (methods.Count > 1)
            {
                methods.Sort((x, y) => CompareMatches(arguments, y, x));

                if (CompareMatches(arguments, methods[0], methods[1]) != 1)
                {
                    var matches = new List<string>
                    {
                        methods[0].FullName,
                        methods[1].FullName
                    };

                    int i = 2;
                    while (i < methods.Count && CompareMatches(arguments, methods[i - 1], methods[i]) == 0)
                    {
                        matches.Add(methods[i].FullName);
                    }

                    throw new Exception(string.Format("Method is ambigous. Could be: \r\n{0}", string.Join("\r\n", matches)));
                }

            }

            return methods[0];
        }

        #region Type/Method/Property/Field getters

        public static bool IsTypeKnown(string typeName)
        {
            return GetTypeInternal(typeName) != null;
        }

        public static bool IsNamespaceKnown(string namespaze)
        {
            return instance.assemblies.Any(x => x.MainModule.Types.Any(y => y.Namespace.StartsWith(namespaze)));
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

        public static TypeReference GetFunctorType(AssemblyEmitter assembly, MethodReference containedMethod)
        {
            return GetFunctorType(assembly, containedMethod.ReturnType, containedMethod.Parameters.Select(x => x.ParameterType).ToList());
        }

        public static TypeReference GetFunctorType(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = TypeEmitter.ComputeNameFromReturnAndArgumentTypes(returnType, arguments);

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
                return new List<MethodReference>();
            }

            return resolvedType.Methods.Where(x => x.Name == methodName).Select(x => ScopeToAssembly(assembly, x)).ToList<MethodReference>();
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, string type,
            string methodName, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, GetTypeInternal(type), methodName, arguments.Select(x => GetTypeInternal(x)).ToList());
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, string type,
            string methodName, IReadOnlyList<TypeReference> arguments)
        {
            var typeRef = GetTypeInternal(type);

            if (typeRef == null)
            {
                throw new Exception(string.Format("Could not find type: {0}.", type));
            }

            return GetCompatibleMethod(assembly, typeRef, methodName, arguments);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeReference type,
            string methodName, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, type, methodName, arguments.Select(x => GetTypeInternal(x)).ToList());
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeReference type,
            string methodName, IReadOnlyList<TypeReference> arguments)
        {
            var methods = GetMethods(assembly, type, methodName).Where(x => x.MatchesArgumentList(arguments)).ToList();
            
            if (methods.Count > 1)
            {
                // More than one is compatible, so one must match exactly, or we have ambiguity
                return GetBestMatch(arguments, methods);
            }
            else if (methods.Count == 0)
            {
                return null;
            }

            return methods.Single();
        }

        public static PropertyReference GetProperty(AssemblyEmitter assembly, string typeName, string propertyName)
        {
            return GetProperty(assembly, GetTypeInternal(typeName), propertyName);
        }

        public static PropertyReference GetProperty(AssemblyEmitter assembly, TypeReference type, string propertyName)
        {
            var resolvedType = type.Resolve();

            if (!resolvedType.HasProperties)
            {
                return null;
            }

            return resolvedType.Properties.SingleOrDefault(x => x.Name == propertyName);
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

        public static FieldReference GetField(AssemblyEmitter assembly, TypeReference type, string fieldName)
        {
            var resolvedType = type.Resolve();

            if (!resolvedType.HasFields)
            {
                return null;
            }

            return ScopeToAssembly(assembly, resolvedType.Fields.SingleOrDefault(x => x.Name == fieldName));
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

        private static int CompareMatches(IReadOnlyList<TypeReference> arguments, MethodReference a, MethodReference b)
        {
            if (a.Parameters.Select(x => x.ParameterType.FullName).SequenceEqual(arguments.Select(x => x.FullName)))
            {
                return 1;
            }

            if (b.Parameters.Select(x => x.ParameterType.FullName).SequenceEqual(arguments.Select(x => x.FullName)))
            {
                return -1;
            }

            List<TypeReference> aParameters, bParameters;
            
            var aIsParamsMethod = a.Resolve().Parameters.Last().CustomAttributes.Any(x => x.AttributeType.FullName == "System.ParamArrayAttribute");
            if (aIsParamsMethod)
            {
                aParameters = a.Parameters.Take(a.Parameters.Count - 1).Select(x => x.ParameterType).ToList();

                var paramsType = a.Parameters.Last().ParameterType;
                for (int i = 0; i < arguments.Count - a.Parameters.Count + 1; i++)
                {
                    aParameters.Add(paramsType);
                }
            }
            else
            {
                aParameters = a.Parameters.Select(x => x.ParameterType).ToList();
            }

            var bIsParamsMethod = b.Resolve().Parameters.Last().CustomAttributes.Any(x => x.AttributeType.FullName == "System.ParamArrayAttribute");
            if (bIsParamsMethod)
            {
                bParameters = b.Parameters.Take(b.Parameters.Count - 1).Select(x => x.ParameterType).ToList();

                var paramsType = b.Parameters.Last().ParameterType.GetElementType();
                for (int i = 0; i < arguments.Count - b.Parameters.Count + 1; i++)
                {
                    bParameters.Add(paramsType);
                }
            }
            else
            {
                bParameters = b.Parameters.Select(x => x.ParameterType).ToList();
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                var aParameter = aParameters[i];
                var bParameter = bParameters[i];

                while (argument != null)
                {
                    if (argument.FullName == aParameter.FullName || argument.FullName == bParameter.FullName)
                    {
                        if (aParameter.FullName != bParameter.FullName)
                        {
                            if (argument.FullName == aParameter.FullName)
                            {
                                return 1;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    argument = argument.Resolve().BaseType;
                }
            }
            
            if (aIsParamsMethod && !bIsParamsMethod)
            {
                return -1;
            }
            else if (!aIsParamsMethod && bIsParamsMethod)
            {
                return 1;
            }

            return 0;
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
                return reference;
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
