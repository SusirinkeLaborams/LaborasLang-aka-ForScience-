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
            Contract.Requires(methods.Any());
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

        public static TypeReference GetPropertyType(AssemblyEmitter assembly, PropertyReference property)
        {
            return ScopeToAssembly(assembly, property.PropertyType);
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

            if (reference.DeclaringType.Scope == null)
                return reference;

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

        #endregion
    }
}
