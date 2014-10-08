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
        private Dictionary<KeyValuePair<TypeReference, MethodReference>, TypeDefinition> functorImplementationTypes;
        private AssemblyDefinition mscorlib;

        private AssemblyRegistry()
        {
            instance = this;

            assemblyPaths = new HashSet<string>();
            assemblies = new List<AssemblyDefinition>();
            functorTypes = new Dictionary<string, TypeDefinition>();
            functorImplementationTypes = new Dictionary<KeyValuePair<TypeReference, MethodReference>, TypeDefinition>();
        }

        private AssemblyRegistry(IEnumerable<string> references)
            : this()
        {
            if (!references.Any(reference => Path.GetFileName(reference) == "mscorlib.dll"))
            {
                throw new ArgumentException("Assembly registry must reference mscorlib!");
            }

            RegisterReferences(references);

            mscorlib = assemblies.Single(assembly => assembly.Name.Name == "mscorlib");
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

        private static MethodReference GetBestMatch(IReadOnlyList<TypeReference> arguments, List<MethodReference> methods)
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
            return FindTypeInternal(typeName) != null;
        }

        public static bool IsNamespaceKnown(string namespaze)
        {
            var altNamespace = namespaze[namespaze.Length - 1] == '.' ? namespaze : namespaze + ".";

            for (int i = 0; i < instance.assemblies.Count; i++)
            {
                var types = instance.assemblies[i].MainModule.Types;
                for (int j = 0; j < types.Count; j++)
                {
                    if (types[j].Namespace == namespaze || types[j].Namespace.StartsWith(altNamespace))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static TypeReference FindType(AssemblyEmitter assemblyScope, string typeName)
        {
            var type = FindTypeInternal(typeName);

            if (type == null)
            {
                return null;
            }

            return ScopeToAssembly(assemblyScope, type);
        }

        public static TypeReference GetFunctorType(AssemblyEmitter assembly, MethodReference containedMethod)
        {
            var parameters = containedMethod.Parameters.Select(parameter => parameter.ParameterType).ToList();
            return GetFunctorType(assembly, containedMethod.ReturnType, parameters);
        }

        public static TypeReference GetFunctorType(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = TypeEmitter.ComputeNameFromReturnAndArgumentTypes(returnType, arguments);

            if (!instance.functorTypes.ContainsKey(name))
            {
                instance.functorTypes.Add(name, FunctorBaseTypeEmitter.Create(assembly, returnType, arguments));
            }

            return instance.functorTypes[name];
        }

        public static TypeReference GetImplementationFunctorType(AssemblyEmitter assembly, TypeEmitter declaringType, MethodReference targetMethod)
        {
            return GetImplementationFunctorType(assembly, declaringType.Get(assembly), targetMethod);
        }

        public static TypeReference GetImplementationFunctorType(AssemblyEmitter assembly, TypeReference declaringType, MethodReference targetMethod)
        {
            var key = new KeyValuePair<TypeReference, MethodReference>(declaringType, targetMethod);

            if (!instance.functorImplementationTypes.ContainsKey(key))
            {
                instance.functorImplementationTypes.Add(key, FunctorImplementationTypeEmitter.Create(assembly, declaringType, targetMethod));
            }

            return instance.functorImplementationTypes[key];
        }
        
        public static MethodReference GetMethod(AssemblyEmitter assembly, string typeName, string methodName)
        {
            return GetMethods(assembly, FindTypeInternal(typeName), methodName).Single();
        }

        public static MethodReference GetMethod(AssemblyEmitter assembly, TypeEmitter type, string methodName)
        {
            return GetMethods(assembly, type.Get(assembly), methodName).Single();
        }

        public static MethodReference GetMethod(AssemblyEmitter assembly, TypeReference type, string methodName)
        {
            return GetMethods(assembly, type, methodName).Single();
        }

        public static List<MethodReference> GetMethods(AssemblyEmitter assembly, string typeName, string methodName)
        {
            return GetMethods(assembly, FindTypeInternal(typeName), methodName);
        }

        public static List<MethodReference> GetMethods(AssemblyEmitter assembly, TypeEmitter type, string methodName)
        {
            return GetMethods(assembly, type.Get(assembly), methodName);
        }

        public static List<MethodReference> GetMethods(AssemblyEmitter assembly, TypeReference type, string methodName)
        {
            var resolvedType = type.Resolve();

            if (!resolvedType.HasMethods)
            {
                return new List<MethodReference>();
            }

            return resolvedType.Methods.Where(methodDef => methodDef.Name == methodName)
                                       .Select(methodDef => ScopeToAssembly(assembly, methodDef)).ToList<MethodReference>();
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, string type,
            string methodName, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, FindTypeInternal(type), methodName, arguments);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeEmitter type,
            string methodName, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, type.Get(assembly), methodName, arguments);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, string type,
            string methodName, IReadOnlyList<TypeReference> arguments)
        {
            var typeRef = FindTypeInternal(type);

            if (typeRef == null)
            {
                throw new Exception(string.Format("Could not find type: {0}.", type));
            }

            return GetCompatibleMethod(assembly, typeRef, methodName, arguments);
        }
        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeEmitter type,
            string methodName, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(assembly, type.Get(assembly), methodName, arguments);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeReference type,
            string methodName, IReadOnlyList<string> arguments)
        {
            var argumentTypes = arguments.Select(arg => FindTypeInternal(arg)).ToList();
            return GetCompatibleMethod(assembly, type, methodName, argumentTypes);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeReference type,
            string methodName, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(GetMethods(assembly, type, methodName), arguments);
        }
            
        public static MethodReference GetCompatibleMethod(IEnumerable<MethodReference> methods, IReadOnlyList<TypeReference> arguments)
        {
            var filtered = methods.Where(methodRef => methodRef.MatchesArgumentList(arguments)).ToList();

            if (filtered.Count > 1)
            {
                // More than one is compatible, so one must match exactly, or we have ambiguity
                return GetBestMatch(arguments, filtered);
            }
            else if (filtered.Count == 0)
            {
                return null;
            }

            return filtered.Single();
        }

        public static MethodReference GetConstructor(AssemblyEmitter assembly, string typeName, bool staticCtor = false)
        {
            return staticCtor ? GetMethod(assembly, typeName, ".cctor") : GetMethod(assembly, typeName, ".ctor");
        }

        public static MethodReference GetConstructor(AssemblyEmitter assembly, TypeEmitter type, bool staticCtor = false)
        {
            return staticCtor ? GetMethod(assembly, type, ".cctor") : GetMethod(assembly, type, ".ctor");
        }

        public static MethodReference GetConstructor(AssemblyEmitter assembly, TypeReference type, bool staticCtor = false)
        {
            return staticCtor ? GetMethod(assembly, type, ".cctor") : GetMethod(assembly, type, ".ctor");
        }

        public static List<MethodReference> GetConstructors(AssemblyEmitter assembly, string typeName)
        {
            return GetMethods(assembly, typeName, ".ctor");
        }

        public static List<MethodReference> GetConstructors(AssemblyEmitter assembly, TypeEmitter type)
        {
            return GetMethods(assembly, type, ".ctor");
        }

        public static List<MethodReference> GetConstructors(AssemblyEmitter assembly, TypeReference type)
        {
            return GetMethods(assembly, type, ".ctor");
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, string typeName, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, typeName, ".ctor", arguments);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeEmitter type, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeReference type, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, string typeName, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(assembly, typeName, ".ctor", arguments);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeEmitter type, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeReference type, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments);
        }        

        public static PropertyReference GetProperty(AssemblyEmitter assembly, string typeName, string propertyName)
        {
            return GetProperty(assembly, FindTypeInternal(typeName), propertyName);
        }

        public static PropertyReference GetProperty(AssemblyEmitter assembly, TypeEmitter type, string propertyName)
        {
            return GetProperty(assembly, type.Get(assembly), propertyName);
        }

        public static PropertyReference GetProperty(AssemblyEmitter assembly, TypeReference type, string propertyName)
        {
            var resolvedType = type.Resolve();

            if (!resolvedType.HasProperties)
            {
                return null;
            }

            return resolvedType.Properties.SingleOrDefault(property => property.Name == propertyName);
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
            return GetField(assembly, FindTypeInternal(typeName), fieldName);
        }
        public static FieldReference GetField(AssemblyEmitter assembly, TypeEmitter type, string fieldName)
        {
            return GetField(assembly, type.Get(assembly), fieldName);
        }

        public static FieldReference GetField(AssemblyEmitter assembly, TypeReference type, string fieldName)
        {
            var resolvedType = type.Resolve();

            if (!resolvedType.HasFields)
            {
                return null;
            }

            var field = resolvedType.Fields.SingleOrDefault(fieldDef => fieldDef.Name == fieldName);

            if (field == null)
            {
                return null;
            }

            return ScopeToAssembly(assembly, field);
        }

        #endregion

        #region Privates

        private TypeDefinition FindTypeInternal(IList<TypeDefinition> types, string typeName)
        {
            foreach (var type in types)
            {
                if (type.FullName == typeName)
                {
                    return type;
                }

                if (type.HasNestedTypes)
                {
                    var nestedType = FindTypeInternal(type.NestedTypes, typeName);
                    if (nestedType != null)
                    {
                        return nestedType;
                    }
                }
            }

            return null;
        }

        private static TypeDefinition FindTypeInternal(string typeName)
        {
            foreach (var assembly in instance.assemblies)
            {
                var type = instance.FindTypeInternal(assembly.MainModule.Types, typeName);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static int CompareMatches(IReadOnlyList<TypeReference> arguments, MethodReference a, MethodReference b)
        {
            var argumentNames = arguments.Select(arg => arg.FullName);

            if (a.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(argumentNames))
            {
                return 1;
            }

            if (b.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(argumentNames))
            {
                return -1;
            }

            List<TypeReference> aParameters, bParameters;

            var aIsParamsMethod = a.IsParamsMethod();
            var bIsParamsMethod = b.IsParamsMethod();

            if (aIsParamsMethod)
            {
                aParameters = a.Parameters.Take(a.Parameters.Count - 1).Select(parameter => parameter.ParameterType).ToList();

                var paramsType = a.Parameters.Last().ParameterType;
                for (int i = 0; i < arguments.Count - a.Parameters.Count + 1; i++)
                {
                    aParameters.Add(paramsType);
                }
            }
            else
            {
                aParameters = a.Parameters.Select(parameter => parameter.ParameterType).ToList();
            }
            
            if (bIsParamsMethod)
            {
                bParameters = b.Parameters.Take(b.Parameters.Count - 1).Select(parameter => parameter.ParameterType).ToList();

                var paramsType = b.Parameters.Last().ParameterType.GetElementType();
                for (int i = 0; i < arguments.Count - b.Parameters.Count + 1; i++)
                {
                    bParameters.Add(paramsType);
                }
            }
            else
            {
                bParameters = b.Parameters.Select(parameter => parameter.ParameterType).ToList();
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
