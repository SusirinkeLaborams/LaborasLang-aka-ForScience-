using LaborasLangCompiler.Codegen.Types;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace LaborasLangCompiler.Codegen
{
    internal class AssemblyRegistry
    {
        private struct FunctorImplementationTypesKey
        {
            public readonly TypeReference declaringType;
            public readonly MethodReference targetMethod;

            public FunctorImplementationTypesKey(TypeReference declaringType, MethodReference targetMethod)
            {
                this.declaringType = declaringType;
                this.targetMethod = targetMethod;
            }
        }

        private struct ArrayTypeKey
        {
            private readonly TypeReference elementType;
            private readonly int rank;

            public ArrayTypeKey(TypeReference elementType, int rank)
            {
                this.elementType = elementType;
                this.rank = rank;
            }
        }

        private struct ArrayInitializerKey
        {
            private readonly ulong hash1, hash2;

            public unsafe ArrayInitializerKey(byte[] hash)
            {
                fixed (byte* hashPtr = hash)
                {
                    hash1 = *(ulong*)hashPtr;
                    hash2 = *(ulong*)(hashPtr + sizeof(ulong));
                }
            }
        }

        private struct DoubleTypeReferenceKey
        {
            private readonly TypeReference first, second;
            
            public DoubleTypeReferenceKey(TypeReference first, TypeReference second)
            {
                this.first = first;
                this.second = second;
            }
        }

        private static AssemblyRegistry instance;

        private readonly HashSet<string> assemblyPaths;             // Keep assembly paths to prevent from registering single assembly twice
        private readonly List<AssemblyDefinition> assemblies;
        private readonly Dictionary<string, TypeDefinition> functorTypes;
        private readonly Dictionary<FunctorImplementationTypesKey, TypeDefinition> functorImplementationTypes;
        private readonly Dictionary<ArrayTypeKey, ArrayType> arrayTypes;
        private readonly Dictionary<ArrayType, MethodReference> arrayConstructors;
        private readonly Dictionary<ArrayType, MethodReference> arrayLoadElementMethods;
        private readonly Dictionary<ArrayType, MethodReference> arrayLoadElementAddressMethods;
        private readonly Dictionary<ArrayType, MethodReference> arrayStoreElementMethods;
        private readonly Dictionary<ArrayInitializerKey, FieldDefinition> arrayInitializers;
        private readonly Dictionary<DoubleTypeReferenceKey, MethodReference> getEnumeratorMethods;
        private readonly Dictionary<DoubleTypeReferenceKey, MethodReference> enumeratorCurrentMethods;
        private readonly Dictionary<DoubleTypeReferenceKey, MethodReference> enumeratorMoveNextMethods;
        private readonly AssemblyDefinition mscorlib;

        private AssemblyRegistry()
        {
            instance = this;

            assemblyPaths = new HashSet<string>();
            assemblies = new List<AssemblyDefinition>();
            functorTypes = new Dictionary<string, TypeDefinition>();
            functorImplementationTypes = new Dictionary<FunctorImplementationTypesKey, TypeDefinition>();
            arrayTypes = new Dictionary<ArrayTypeKey, ArrayType>();
            arrayConstructors = new Dictionary<ArrayType, MethodReference>();
            arrayLoadElementMethods = new Dictionary<ArrayType, MethodReference>();
            arrayLoadElementAddressMethods = new Dictionary<ArrayType, MethodReference>();
            arrayStoreElementMethods = new Dictionary<ArrayType, MethodReference>();
            arrayInitializers = new Dictionary<ArrayInitializerKey, FieldDefinition>();
            getEnumeratorMethods = new Dictionary<DoubleTypeReferenceKey, MethodReference>();
            enumeratorCurrentMethods = new Dictionary<DoubleTypeReferenceKey, MethodReference>();
            enumeratorMoveNextMethods = new Dictionary<DoubleTypeReferenceKey, MethodReference>();
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

            return MetadataHelpers.ScopeToAssembly(assemblyScope, type);
        }

        public static TypeReference FindType(ModuleDefinition module, string typeName)
        {
            var type = FindTypeInternal(typeName);

            if (type == null)
            {
                return null;
            }

            return MetadataHelpers.ScopeToAssembly(module, type);
        }

        public static TypeReference GetFunctorType(AssemblyEmitter assembly, MethodReference containedMethod)
        {
            var parameters = containedMethod.GetParameterTypes();
            return GetFunctorType(assembly, containedMethod.GetReturnType(), parameters);
        }

        public static TypeReference GetFunctorType(AssemblyEmitter assembly, TypeReference returnType, IReadOnlyList<TypeReference> arguments)
        {
            var name = TypeEmitter.ComputeNameFromReturnAndArgumentTypes(returnType, arguments);
            TypeDefinition value;

            if (instance.functorTypes.TryGetValue(name, out value))
                return value;

            value = FunctorBaseTypeEmitter.Create(assembly, returnType, arguments);
            instance.functorTypes.Add(name, value);
            return value;
        }

        public static TypeReference GetImplementationFunctorType(AssemblyEmitter assembly, TypeEmitter declaringType, MethodReference targetMethod)
        {
            return GetImplementationFunctorType(assembly, declaringType.Get(assembly), targetMethod);
        }

        public static TypeReference GetImplementationFunctorType(AssemblyEmitter assembly, TypeReference declaringType, MethodReference targetMethod)
        {
            var key = new FunctorImplementationTypesKey(declaringType, targetMethod);
            TypeDefinition value;

            if (instance.functorImplementationTypes.TryGetValue(key, out value))
                return value;

            value = FunctorImplementationTypeEmitter.Create(assembly, declaringType, targetMethod);
            instance.functorImplementationTypes.Add(key, value);

            return value;
        }

        public static FieldDefinition GetArrayInitializerField(AssemblyEmitter assembly, TypeReference elementType, IReadOnlyList<IExpressionNode> arrayInitializer)
        {
            var md5 = MD5.Create();
            var initializerBytes = GetArrayInitializerBytes(elementType, arrayInitializer);

            var hash = md5.ComputeHash(initializerBytes);
            var key = new ArrayInitializerKey(hash);

            FieldDefinition field;

            if (!instance.arrayInitializers.TryGetValue(key, out field))
            {
                field = ArrayInitializerEmitter.Emit(assembly, initializerBytes);
                instance.arrayInitializers.Add(key, field);
            }

            return field;
        }

        public static ArrayType GetArrayType(TypeReference elementType, int rank)
        {
            Contract.Requires(rank > 0);
            var key = new ArrayTypeKey(elementType, rank);
            ArrayType arrayType;

            if (instance.arrayTypes.TryGetValue(key, out arrayType))
                return arrayType;

            arrayType = new ArrayType(elementType, rank);

            if (rank > 1)
            {
                for (int i = 0; i < rank; i++)
                {
                    arrayType.Dimensions[i] = new ArrayDimension(0, null);
                }
            }

            instance.arrayTypes.Add(key, arrayType);

            return arrayType;
        }

        public static MethodReference GetMethod(AssemblyEmitter assembly, string typeName, string methodName, bool searchBaseType = true)
        {
            return GetMethods(assembly, FindType(assembly, typeName), methodName, searchBaseType).Single();
        }

        public static MethodReference GetMethod(AssemblyEmitter assembly, TypeEmitter type, string methodName, bool searchBaseType = true)
        {
            return GetMethods(assembly, type.Get(assembly), methodName, searchBaseType).Single();
        }

        public static MethodReference GetMethod(AssemblyEmitter assembly, TypeReference type, string methodName, bool searchBaseType = true)
        {
            return GetMethods(assembly, type, methodName, searchBaseType).Single();
        }

        public static IReadOnlyList<MethodReference> GetMethods(AssemblyEmitter assembly, string typeName, string methodName, bool searchBaseType = true)
        {
            return GetMethods(assembly, FindType(assembly, typeName), methodName, searchBaseType);
        }

        public static IReadOnlyList<MethodReference> GetMethods(AssemblyEmitter assembly, TypeEmitter type, string methodName, bool searchBaseType = true)
        {
            return GetMethods(assembly, type.Get(assembly), methodName, searchBaseType);
        }

        public static IReadOnlyList<MethodReference> GetMethods(AssemblyEmitter assembly, TypeReference type, string methodName, bool searchBaseType = true)
        {
            return GetMethodsImpl(type, methodName, searchBaseType);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, string type,
            string methodName, IReadOnlyList<string> arguments, bool searchBaseType = true)
        {
            return GetCompatibleMethod(assembly, FindType(assembly, type), methodName, arguments, searchBaseType);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeEmitter type,
            string methodName, IReadOnlyList<string> arguments, bool searchBaseType = true)
        {
            return GetCompatibleMethod(assembly, type.Get(assembly), methodName, arguments, searchBaseType);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, string type,
            string methodName, IReadOnlyList<TypeReference> arguments, bool searchBaseType = true)
        {
            var typeRef = FindType(assembly, type);

            if (typeRef == null)
            {
                throw new Exception(string.Format("Could not find type: {0}.", type));
            }

            return GetCompatibleMethod(assembly, typeRef, methodName, arguments, searchBaseType);
        }
        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeEmitter type,
            string methodName, IReadOnlyList<TypeReference> arguments, bool searchBaseType = true)
        {
            return GetCompatibleMethod(assembly, type.Get(assembly), methodName, arguments, searchBaseType);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeReference type,
            string methodName, IReadOnlyList<string> arguments, bool searchBaseType = true)
        {
            var argumentTypes = arguments.Select(arg => FindType(assembly, arg)).ToArray();
            return GetCompatibleMethod(assembly, type, methodName, argumentTypes, searchBaseType);
        }

        public static MethodReference GetCompatibleMethod(AssemblyEmitter assembly, TypeReference type,
            string methodName, IReadOnlyList<TypeReference> arguments, bool searchBaseType = true)
        {
            return GetCompatibleMethod(GetMethods(assembly, type, methodName, searchBaseType), arguments);
        }

        public static MethodReference GetCompatibleMethod(IEnumerable<MethodReference> methods, IReadOnlyList<TypeReference> arguments)
        {
            var filtered = methods.Where(methodRef => methodRef.MatchesArgumentList(arguments)).ToArray();

            if (filtered.Length > 1)
                return GetBestMatch(arguments, filtered); // More than one is compatible, so one must match exactly, or we have ambiguity

            if (filtered.Length == 0)
                return null;

            return filtered[0];
        }

        public static MethodReference GetConstructor(AssemblyEmitter assembly, string typeName, bool staticCtor = false)
        {
            return staticCtor ? GetMethod(assembly, typeName, ".cctor", false) : GetMethod(assembly, typeName, ".ctor", false);
        }

        public static MethodReference GetConstructor(AssemblyEmitter assembly, TypeEmitter type, bool staticCtor = false)
        {
            return staticCtor ? GetMethod(assembly, type, ".cctor", false) : GetMethod(assembly, type, ".ctor", false);
        }

        public static MethodReference GetConstructor(AssemblyEmitter assembly, TypeReference type, bool staticCtor = false)
        {
            return staticCtor ? GetMethod(assembly, type, ".cctor", false) : GetMethod(assembly, type, ".ctor", false);
        }

        public static IReadOnlyList<MethodReference> GetConstructors(AssemblyEmitter assembly, string typeName)
        {
            return GetMethods(assembly, typeName, ".ctor", false);
        }

        public static IReadOnlyList<MethodReference> GetConstructors(AssemblyEmitter assembly, TypeEmitter type)
        {
            return GetMethods(assembly, type, ".ctor", false);
        }

        public static IReadOnlyList<MethodReference> GetConstructors(AssemblyEmitter assembly, TypeReference type)
        {
            return GetMethods(assembly, type, ".ctor", false);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, string typeName, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, typeName, ".ctor", arguments, false);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeEmitter type, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments, false);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeReference type, IReadOnlyList<string> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments, false);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, string typeName, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(assembly, typeName, ".ctor", arguments, false);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeEmitter type, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments, false);
        }

        public static MethodReference GetCompatibleConstructor(AssemblyEmitter assembly, TypeReference type, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleMethod(assembly, type, ".ctor", arguments, false);
        }

        public static MethodReference GetArrayConstructor(ArrayType arrayType)
        {
            Contract.Requires(!arrayType.IsVector);

            MethodReference constructor;

            if (instance.arrayConstructors.TryGetValue(arrayType, out constructor))
                return constructor;

            constructor = new MethodReference(".ctor", arrayType.Module.TypeSystem.Void, arrayType);
            constructor.HasThis = true;

            for (int i = 0; i < arrayType.Rank; i++)
            {
                constructor.Parameters.Add(new ParameterDefinition(arrayType.Module.TypeSystem.Int32));
            }

            instance.arrayConstructors.Add(arrayType, constructor);
            return constructor;
        }

        public static MethodReference GetArrayLoadElement(ArrayType arrayType)
        {
            Contract.Requires(!arrayType.IsVector);

            MethodReference loadElement;

            if (instance.arrayLoadElementMethods.TryGetValue(arrayType, out loadElement))
                return loadElement;

            loadElement = new MethodReference("Get", arrayType.ElementType, arrayType);
            loadElement.HasThis = true;

            for (int i = 0; i < arrayType.Rank; i++)
            {
                loadElement.Parameters.Add(new ParameterDefinition(arrayType.Module.TypeSystem.Int32));
            }

            instance.arrayLoadElementMethods.Add(arrayType, loadElement);
            return loadElement;
        }

        public static MethodReference GetArrayLoadElementAddress(ArrayType arrayType)
        {
            Contract.Requires(!arrayType.IsVector);

            MethodReference loadElementAddress;

            if (instance.arrayLoadElementAddressMethods.TryGetValue(arrayType, out loadElementAddress))
                return loadElementAddress;

            loadElementAddress = new MethodReference("Address", new ByReferenceType(arrayType.ElementType), arrayType);
            loadElementAddress.HasThis = true;

            for (int i = 0; i < arrayType.Rank; i++)
            {
                loadElementAddress.Parameters.Add(new ParameterDefinition(arrayType.Module.TypeSystem.Int32));
            }

            instance.arrayLoadElementAddressMethods.Add(arrayType, loadElementAddress);
            return loadElementAddress;
        }

        public static MethodReference GetArrayStoreElement(ArrayType arrayType)
        {
            Contract.Requires(!arrayType.IsVector);

            MethodReference storeElement;

            if (instance.arrayStoreElementMethods.TryGetValue(arrayType, out storeElement))
                return storeElement;

            storeElement = new MethodReference("Set", arrayType.Module.TypeSystem.Void, arrayType);
            storeElement.HasThis = true;

            for (int i = 0; i < arrayType.Rank; i++)
            {
                storeElement.Parameters.Add(new ParameterDefinition(arrayType.Module.TypeSystem.Int32));
            }

            storeElement.Parameters.Add(new ParameterDefinition(arrayType.ElementType));
            instance.arrayStoreElementMethods.Add(arrayType, storeElement);
            return storeElement;
        }

        internal static MethodReference GetGetEnumeratorMethod(TypeReference collectionType, TypeReference elementType)
        {
            MethodReference getEnumeratorMethod;
            var key = new DoubleTypeReferenceKey(collectionType, elementType);

            if (instance.getEnumeratorMethods.TryGetValue(key, out getEnumeratorMethod))
                return getEnumeratorMethod;

            getEnumeratorMethod = GetGetEnumeratorMethodImpl(collectionType, elementType);
            instance.getEnumeratorMethods.Add(key, getEnumeratorMethod);
            return getEnumeratorMethod;
        }

        internal static MethodReference GetEnumeratorCurrentMethod(TypeReference enumeratorType, TypeReference elementType)
        {
            MethodReference enumeratorCurrentMethod;
            var key = new DoubleTypeReferenceKey(enumeratorType, elementType);

            if (instance.enumeratorCurrentMethods.TryGetValue(key, out enumeratorCurrentMethod))
                return enumeratorCurrentMethod;

            enumeratorCurrentMethod = GetEnumeratorCurrentMethodImpl(enumeratorType, elementType);
            instance.enumeratorCurrentMethods.Add(key, enumeratorCurrentMethod);
            return enumeratorCurrentMethod;
        }

        internal static MethodReference GetEnumeratorMoveNextMethod(TypeReference enumeratorType, TypeReference elementType)
        {
            MethodReference enumeratorMoveNextMethod;
            var key = new DoubleTypeReferenceKey(enumeratorType, elementType);

            if (instance.enumeratorMoveNextMethods.TryGetValue(key, out enumeratorMoveNextMethod))
                return enumeratorMoveNextMethod;

            enumeratorMoveNextMethod = GetEnumeratorMoveNextMethodImpl(enumeratorType, elementType);
            instance.enumeratorMoveNextMethods.Add(key, enumeratorMoveNextMethod);
            return enumeratorMoveNextMethod;
        }

        public static IReadOnlyList<PropertyReference> GetProperties(TypeReference type, string propertyName)
        {
            return type.Resolve().Properties.Where(property => property.Name == propertyName).ToArray();
        }

        public static PropertyReference GetProperty(string typeName, string propertyName)
        {
            return GetProperty(FindTypeInternal(typeName), propertyName);
        }

        public static PropertyReference GetProperty(TypeReference type, string propertyName)
        {
            return GetProperties(type, propertyName).SingleOrDefault();
        }

        public static PropertyReference GetCompatibleProperty(TypeReference type, string propertyName, IReadOnlyList<TypeReference> arguments)
        {
            return GetCompatibleProperty(GetProperties(type, propertyName), arguments);
        }

        public static PropertyReference GetCompatibleProperty(IEnumerable<PropertyReference> properties, IReadOnlyList<TypeReference> arguments)
        {
            var filtered = properties.Select(property => property.Resolve()).Where(property => property.MatchesArgumentList(arguments)).ToArray();

            if (filtered.Length > 1)
                return GetBestMatch(arguments, filtered);

            if (filtered.Length == 0)
                return null;

            return filtered[0];
        }

        public static TypeReference GetPropertyType(AssemblyEmitter assembly, PropertyReference property)
        {
            return MetadataHelpers.ScopeToAssembly(assembly, property.PropertyType);
        }

        public static MethodReference GetPropertyGetter(AssemblyEmitter assembly, PropertyReference property)
        {
            var resolvedProperty = property.Resolve();

            if (resolvedProperty.GetMethod == null)
            {
                return null;
            }

            return MetadataHelpers.ScopeToAssembly(assembly, resolvedProperty.GetMethod);
        }

        public static MethodReference GetPropertySetter(AssemblyEmitter assembly, PropertyReference property)
        {
            var resolvedProperty = property.Resolve();

            if (resolvedProperty.SetMethod == null)
            {
                return null;
            }

            return MetadataHelpers.ScopeToAssembly(assembly, resolvedProperty.SetMethod);
        }

        public static FieldReference GetField(AssemblyEmitter assembly, string typeName, string fieldName)
        {
            return GetField(assembly, FindType(assembly, typeName), fieldName);
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

            return MetadataHelpers.ScopeToAssembly(assembly, field);
        }

        #endregion

        #region Privates

        private static List<MethodReference> GetMethodsImpl(TypeReference type, string methodName, bool searchBaseType)
        {
            var methods = new List<MethodReference>();
            var resolvedType = type.Resolve();
            var genericInstance = type as GenericInstanceType;

            if (!(type is ArrayType) && resolvedType != null)
            {
                for (int i = 0; i < resolvedType.Methods.Count; i++)
                {
                    var method = resolvedType.Methods[i];

                    if (method.HasGenericParameters)
                        continue;

                    if (method.Name != methodName)
                        continue;

                    var methodRef = MetadataHelpers.ScopeToAssembly(type.Module, method);

                    if (genericInstance != null)
                        methodRef.DeclaringType = genericInstance;

                    methods.Add(methodRef);
                }
            }

            if (methods.Count == 0 && searchBaseType)
            {
                foreach (var iface in type.GetInterfaces())
                    methods.AddRange(GetMethodsImpl(iface, methodName, true));

                if (methods.Count == 0)
                {
                    var baseType = type.GetBaseType();

                    if (baseType != null)
                        methods.AddRange(GetMethodsImpl(baseType, methodName, true));
                }
            }

            return methods;
        }

        private static void InflateForGenericInstanceType(MethodDefinition methodDef, GenericInstanceType declaringType)
        {

        }

        struct ParameterInfo<T>
        {
            public readonly T key;
            public readonly IList<ParameterDefinition> parameters;
            public readonly IReadOnlyList<TypeReference> parameterTypes;

            public ParameterInfo(T key, IList<ParameterDefinition> parameters, IReadOnlyList<TypeReference> parameterTypes)
            {
                this.key = key;
                this.parameters = parameters;
                this.parameterTypes = parameterTypes;
            }
        }

        private static int GetBestMatches<T>(IReadOnlyList<TypeReference> arguments, ParameterInfo<T>[] parameters)
        {
            Contract.Requires(parameters.Length > 0);
            int i = 0;

            if (parameters.Length > 1)
            {
                Array.Sort(parameters, (x, y) => CompareMatches(arguments, y, x));

                while (i < parameters.Length - 1 && CompareMatches(arguments, parameters[i], parameters[i + 1]) == 0)
                    i++;
            }

            return i;
        }

        private static MethodReference GetBestMatch(IReadOnlyList<TypeReference> arguments, IReadOnlyList<MethodReference> methods)
        {
            Contract.Requires(methods.Count > 0);

            var parameterArray = methods.Select(method => new ParameterInfo<MethodReference>(method, method.Resolve().Parameters, method.GetParameterTypes())).ToArray();
            var bestMatchIndex = GetBestMatches(arguments, parameterArray);

            if (bestMatchIndex == 0)
                return parameterArray[0].key;

            var matches = parameterArray.Take(bestMatchIndex + 1).Select(match => match.key.FullName);
            throw new Exception(string.Format("Method is ambigous. Could be: \r\n{0}", string.Join("\r\n", matches)));
        }

        private static PropertyReference GetBestMatch(IReadOnlyList<TypeReference> arguments, IReadOnlyList<PropertyDefinition> properties)
        {
            Contract.Requires(properties.Count > 0);

            var parameterArray = properties.Select(property => new ParameterInfo<PropertyDefinition>(property, property.Parameters, property.Parameters.Select(p => p.ParameterType).ToArray())).ToArray();
            var bestMatchIndex = GetBestMatches(arguments, parameterArray);

            if (bestMatchIndex == 0)
                return parameterArray[0].key;

            var matches = parameterArray.Take(bestMatchIndex + 1).Select(match => match.key.FullName);
            throw new Exception(string.Format("Property is ambigous. Could be: \r\n{0}", string.Join("\r\n", matches)));
        }

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

        private static int CompareMatches<T>(IReadOnlyList<TypeReference> arguments, ParameterInfo<T> aParameterInfo, ParameterInfo<T> bParameterInfo)
        {
            var argumentNames = arguments.Select(arg => arg.FullName);

            if (aParameterInfo.parameterTypes.Select(type => type.FullName).SequenceEqual(argumentNames))
            {
                return 1;
            }

            if (bParameterInfo.parameterTypes.Select(type => type.FullName).SequenceEqual(argumentNames))
            {
                return -1;
            }
            
            var aParameters = aParameterInfo.parameters;
            var bParameters = bParameterInfo.parameters;

            var aParameterTypes = aParameterInfo.parameterTypes;
            var bParameterTypes = bParameterInfo.parameterTypes;

            var aIsParamsMethod = aParameters[aParameters.Count - 1].IsParams();
            var bIsParamsMethod = bParameters[bParameters.Count - 1].IsParams();

            if (aIsParamsMethod)
            {
                var types = aParameterTypes.Take(aParameters.Count - 1).ToList();

                var paramsType = aParameterTypes[aParameterTypes.Count - 1].GetElementType();
                for (int i = 0; i < arguments.Count - aParameters.Count + 1; i++)
                {
                    types.Add(paramsType);
                }

                aParameterTypes = types;
            }

            if (bIsParamsMethod)
            {
                var types = bParameterTypes.Take(bParameters.Count - 1).ToList();

                var paramsType = bParameterTypes[bParameterTypes.Count - 1].GetElementType();
                for (int i = 0; i < arguments.Count - bParameters.Count + 1; i++)
                {
                    types.Add(paramsType);
                }

                bParameterTypes = types;
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                var aParameter = aParameterTypes[i];
                var bParameter = bParameterTypes[i];

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

                    argument = argument.GetBaseType();
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

        private static unsafe void CopyValueToByteArray(byte[] byteArray, int elementSize, int index, byte* valuePtr, int valueSize)
        {
            var targetIndex = index * elementSize;

            for (int i = 0; i < valueSize && i < elementSize; i++)
            {
                byteArray[targetIndex + i] = valuePtr[i];
            }

            for (int i = valueSize; i < elementSize; i++)
            {
                byteArray[targetIndex + i] = 0;
            }
        }

        private static unsafe byte[] GetArrayInitializerBytes(TypeReference elementType, IReadOnlyList<IExpressionNode> arrayInitializer)
        {
            var elementSize = MetadataHelpers.GetPrimitiveWidth(elementType);
            var initializerBytes = new byte[elementSize * arrayInitializer.Count];

            for (int i = 0; i < arrayInitializer.Count; i++)
            {
                var literalNode = ((ILiteralNode)arrayInitializer[i]);

                switch (literalNode.ExpressionReturnType.MetadataType)
                {
                    case MetadataType.Boolean:
                    case MetadataType.Byte:
                    case MetadataType.SByte:
                        {
                            var value = (byte)literalNode.Value;
                            CopyValueToByteArray(initializerBytes, elementSize, i, (byte*)&value, sizeof(byte));
                        }
                        break;

                    case MetadataType.Char:
                    case MetadataType.Int16:
                    case MetadataType.UInt16:
                        {
                            var value = (ushort)literalNode.Value;
                            CopyValueToByteArray(initializerBytes, elementSize, i, (byte*)&value, sizeof(ushort));
                        }
                        break;

                    case MetadataType.Int32:
                    case MetadataType.UInt32:
                        {
                            var value = (uint)literalNode.Value;
                            CopyValueToByteArray(initializerBytes, elementSize, i, (byte*)&value, sizeof(uint));
                        }
                        break;

                    case MetadataType.Single:
                        {
                            var value = (float)literalNode.Value;
                            CopyValueToByteArray(initializerBytes, elementSize, i, (byte*)&value, sizeof(float));
                        }
                        break;

                    case MetadataType.Int64:
                    case MetadataType.UInt64:
                        {
                            var value = (ulong)literalNode.Value;
                            CopyValueToByteArray(initializerBytes, elementSize, i, (byte*)&value, sizeof(ulong));
                        }
                        break;

                    case MetadataType.Double:
                        {
                            var value = (double)literalNode.Value;
                            CopyValueToByteArray(initializerBytes, elementSize, i, (byte*)&value, sizeof(double));
                        }
                        break;

                    default:
                        throw new ArgumentException();
                }
            }

            return initializerBytes;
        }

        private static bool IsValidEnumerator(TypeReference enumeratorType, TypeReference elementType)
        {
            return GetEnumeratorCurrentMethod(enumeratorType, elementType) != null && GetEnumeratorMoveNextMethod(enumeratorType, elementType) != null;
        }

        private static MethodReference GetEnumeratorCurrentMethodImpl(TypeReference enumeratorType, TypeReference elementType)
        {
            var methods = GetMethodsImpl(enumeratorType, "get_Current", true).Where(method =>
            {
                if (method.Parameters.Count > 0)
                    return false;
                
                var methodReturnType = method.GetReturnType();
                return methodReturnType.IsAssignableTo(elementType) || elementType.IsAssignableTo(methodReturnType);
            }).ToArray();

            if (methods.Length == 0)
                return null;

            Contract.Assert(methods.Length == 1);
            return methods[0];
        }

        private static MethodReference GetEnumeratorMoveNextMethodImpl(TypeReference enumeratorType, TypeReference elementType)
        {
            var methods = GetMethodsImpl(enumeratorType, "MoveNext", true).Where(m => m.Parameters.Count == 0 && m.GetReturnType().MetadataType == MetadataType.Boolean).ToArray();

            if (methods.Length == 0)
                return null;

            Contract.Assert(methods.Length == 1);
            return methods[0];
        }

        private static MethodReference GetGetEnumeratorMethodImpl(TypeReference collectionType, TypeReference elementType)
        {
            var methods = GetMethodsImpl(collectionType, "GetEnumerator", true).Where(m => m.Parameters.Count == 0 && IsValidEnumerator(m.GetReturnType(), elementType)).ToArray();

            if (methods.Length == 0)
                return null;

            Array.Sort(methods, (x, y) =>
            {
                var xReturnType = x.GetReturnType();
                var yReturnType = y.GetReturnType();

                if (xReturnType.IsValueType)   
                {
                    if (yReturnType.IsValueType)
                        return 0;

                    return -1;
                }

                if (yReturnType.IsValueType)
                    return 1;

                var ienumerableT = FindType(elementType.Module, "System.Collections.Generic.IEnumerable`1").MakeGenericType(elementType);
                var ienumerable = FindType(elementType.Module, "System.Collections.IEnumerable");

                var xIsIEnumerable = x.DeclaringType.IsAssignableTo(ienumerable);
                var yIsIEnumerable = y.DeclaringType.IsAssignableTo(ienumerable);

                if (!xIsIEnumerable)
                {
                    if (!yIsIEnumerable)
                        return 0;

                    return -1;
                }

                if (!yIsIEnumerable)
                    return 1;

                var xIsIEnumerableT = x.DeclaringType.IsAssignableTo(ienumerableT);
                var yIsIEnumerableT = y.DeclaringType.IsAssignableTo(ienumerableT);

                if (xIsIEnumerableT)
                {
                    if (yIsIEnumerableT)
                        return 0;

                    return -1;
                }

                if (yIsIEnumerableT)
                    return 1;

                return 0;
            });

            return methods[0];
        }

        #endregion
    }
}
