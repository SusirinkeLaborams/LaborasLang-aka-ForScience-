using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LaborasLangCompiler.Codegen
{
    static class MetadataHelpers
    {
        // 0 means native
        public static int GetPrimitiveWidth(this TypeReference type)
        {
            switch (type.MetadataType)
            {
                case MetadataType.IntPtr:
                case MetadataType.UIntPtr:
                    return 0;

                case MetadataType.SByte:
                case MetadataType.Byte:
                case MetadataType.Boolean:
                    return 1;

                case MetadataType.Char:
                case MetadataType.Int16:
                case MetadataType.UInt16:
                    return 2;

                case MetadataType.Int32:
                case MetadataType.UInt32:
                case MetadataType.Single:
                    return 4;

                case MetadataType.Int64:
                case MetadataType.UInt64:
                case MetadataType.Double:
                    return 8;
            }
            
            throw new NotSupportedException(string.Format("{0} is not an integer!", type.FullName));
        }

        public static bool IsUnsignedInteger(this TypeReference type)
        {
            switch (type.MetadataType)
            {
                case MetadataType.Byte:
                case MetadataType.Char:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                case MetadataType.UInt64:
                case MetadataType.UIntPtr:
                    return true;
            }

            return false;
        }

        public static bool IsSignedInteger(this TypeReference type)
        {
            switch (type.MetadataType)
            {
                case MetadataType.SByte:
                case MetadataType.Int16:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.IntPtr:
                    return true;
            }

            return false;
        }

        public static bool IsIntegerType(this TypeReference type)
        {
            switch (type.MetadataType)
            {
                case MetadataType.SByte:
                case MetadataType.Byte:
                case MetadataType.Char:
                case MetadataType.Int16:
                case MetadataType.UInt16:
                case MetadataType.Int32:
                case MetadataType.UInt32:
                case MetadataType.Int64:
                case MetadataType.UInt64:
                case MetadataType.IntPtr:
                case MetadataType.UIntPtr:
                    return true;
            }

            return false;
        }

        public static bool IsFloatingPointType(this TypeReference type)
        {
            switch (type.MetadataType)
            {
                case MetadataType.Single:
                case MetadataType.Double:
                    return true;
            }

            return false;
        }

        public static bool IsNumericType(this TypeReference type)
        {
            return type.IsIntegerType() || type.IsFloatingPointType();
        }

        public static bool IsStringType(this TypeReference type)
        {
            return type.MetadataType == MetadataType.String;
        }

        public static bool IsBooleanType(this TypeReference type)
        {
            return type.MetadataType == MetadataType.Boolean;
        }

        public static bool IsFunctorType(this TypeReference type)
        {
            Contract.Assume(type.FullName != null);
            return type.FullName.StartsWith("$Functors.") && type.IsDefinition;
        }

        public static bool IsAssignableTo(this TypeReference right, TypeReference left)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Assume(left.FullName != null);
            Contract.Assume(right.FullName != null);

            if (left.IsByReference)
            {
                left = left.GetElementType();
                Contract.Assume(left != null);
            }

            if (right.IsByReference)
            {
                right = right.GetElementType();
                Contract.Assume(right != null);
            }
            
            if (left.FullName == right.FullName)
            {
                return true;
            }

            if (left.IsPrimitive && right.IsPrimitive)
            {
                return assignmentMap[left.MetadataType].Any(type => type == right.MetadataType);
            }

            // We support no type specs atm
            if (left is TypeSpecification || right is TypeSpecification)
                return false;

            var leftType = left.Resolve();
            var rightType = right.Resolve();

            if (leftType.HasGenericParameters || rightType.HasGenericParameters)
            {
                throw new NotSupportedException("Generic types are not supported!");
            }

            if (leftType.IsInterface)
            {
                return rightType.Interfaces.Any(interfaze => interfaze.FullName == leftType.FullName);
            }
                        
            while (rightType.BaseType != null)
            {
                rightType = rightType.BaseType.Resolve();

                if (leftType.FullName == rightType.FullName)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DerivesFrom(this TypeReference childRef, TypeReference parentRef)
        {
            if (childRef.FullName == parentRef.FullName)
            {
                return true;
            }

            var child = childRef.Resolve();
            var parent = parentRef.Resolve();
            
            if (parent.IsInterface)
            {
                if (child.Interfaces.Any(interfaze => interfaze.FullName == parent.FullName))
                {
                    return true;
                }
            }

            if (child.BaseType == null)
            {
                return false;
            }

            return child.BaseType.DerivesFrom(parent);
        }

        public static bool DeclaredBy(this TypeReference nestedType, TypeReference type)
        {
            while (nestedType != type && nestedType != null)
            {
                nestedType = nestedType.DeclaringType;
            }

            return nestedType == type;
        }

        public static bool MatchesArgumentList(this TypeReference functorType, AssemblyEmitter assemblyScope, IReadOnlyList<TypeReference> desiredParameters)
        {
            var invokeMethod = GetFunctorInvokeMethod(assemblyScope, functorType);
            return invokeMethod.MatchesArgumentList(desiredParameters);
        }

        public static bool MatchesArgumentList(this MethodReference method, IReadOnlyList<TypeReference> desiredParameters)
        {
            var methodParameters = method.Resolve().Parameters; // Resolve is needed or otherwise we will not know methods parameter attributes
            return MatchesArgumentList(methodParameters, desiredParameters);
        }

        public static bool MatchesArgumentList(IList<ParameterDefinition> methodParameters, IReadOnlyList<TypeReference> desiredParameters)
        {
            // Doesn't match if parameter count doesn't match and either method has no parameters, or last parameter is neither params, nor default one.
            var lastParameter = methodParameters.LastOrDefault();

            if (methodParameters.Count != desiredParameters.Count &&
                (methodParameters.Count == 0 || (!lastParameter.IsParams() && !lastParameter.IsDefault())))
            {
                return false;
            }

            int numberOfMatches = 0;
            while (numberOfMatches < methodParameters.Count && numberOfMatches < desiredParameters.Count)
            {
                if (desiredParameters[numberOfMatches].IsAssignableTo(methodParameters[numberOfMatches].ParameterType))
                {
                    numberOfMatches++;
                }
                else
                {
                    break;
                }
            }

            if (numberOfMatches == methodParameters.Count && numberOfMatches == desiredParameters.Count)
            {
                return true;
            }
            else if (methodParameters.Count < desiredParameters.Count)
            {
                // Check params parameters

                if (numberOfMatches != methodParameters.Count - 1)
                {
                    return false;
                }

                var paramsArgument = methodParameters.Last().ParameterType.GetElementType();

                for (int i = methodParameters.Count - 1; i < desiredParameters.Count; i++)
                {
                    if (!desiredParameters[i].IsAssignableTo(paramsArgument))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (methodParameters.Count > desiredParameters.Count)
            {
                // Check optional parameters

                if (numberOfMatches != desiredParameters.Count)
                {
                    return false;
                }

                for (int i = desiredParameters.Count; i < methodParameters.Count; i++)
                {
                    if (!methodParameters[i].IsOptional)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool IsParamsMethod(this MethodReference method)
        {
            var parameters = method.Resolve().Parameters;

            if (parameters.Count == 0)
            {
                return false;
            }

            return parameters[parameters.Count - 1].IsParams();
        }

        public static bool IsParams(this ParameterDefinition parameter)
        {
            return parameter.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == "System.ParamArrayAttribute");
        }

        public static bool IsDefault(this ParameterDefinition parameter)
        {
            return (parameter.Attributes & ParameterAttributes.HasDefault) != 0;
        }

        public static TypeReference GetFunctorReturnType(AssemblyEmitter assemblyScope, TypeReference functorType)
        {
            var invokeMethod = GetFunctorInvokeMethod(assemblyScope, functorType);
            return invokeMethod.ReturnType;
        }

        public static TypeReference GetFunctorReturnTypeAndArguments(AssemblyEmitter assemblyScope, TypeReference functorType, 
            out List<TypeReference> arguments)
        {
            var invokeMethod = GetFunctorInvokeMethod(assemblyScope, functorType);      
            arguments = invokeMethod.Parameters.Select(parameter => parameter.ParameterType).ToList();
            return invokeMethod.ReturnType;
        }

        public static List<TypeReference> GetFunctorParamTypes(AssemblyEmitter assemblyScope, TypeReference functorType)
        {
            var invokeMethod = GetFunctorInvokeMethod(assemblyScope, functorType);
            return invokeMethod.Parameters.Select(param => param.ParameterType).ToList();
        }

        private static MethodReference GetFunctorInvokeMethod(AssemblyEmitter assemblyScope, TypeReference functorType)
        {
            if (!functorType.IsFunctorType())
            {
                throw new ArgumentException("functorType isn't a functor type!");
            }

            return AssemblyRegistry.GetMethod(assemblyScope, functorType, "Invoke");
        }

        public static bool IsAccessible(FieldReference target, TypeReference scope)
        {
            var field = target.Resolve();
            var targetType = target.DeclaringType;

            switch (field.Attributes & FieldAttributes.FieldAccessMask)
            {
                case FieldAttributes.CompilerControlled:
                    return false;

                case FieldAttributes.Private:
                    return scope.DeclaredBy(targetType);

                case FieldAttributes.FamANDAssem:
                    return targetType.Module == scope.Resolve().Module && (scope.DeclaredBy(targetType) || scope.DerivesFrom(targetType));

                case FieldAttributes.Assembly:
                    return targetType.Module == scope.Resolve().Module;

                case FieldAttributes.Family:
                    return scope.DeclaredBy(targetType) || scope.DerivesFrom(targetType);

                case FieldAttributes.FamORAssem:
                    return targetType.Module == scope.Resolve().Module || scope.DeclaredBy(targetType) || scope.DerivesFrom(targetType);

                case FieldAttributes.Public:
                    return true;

                default:
                    throw new NotSupportedException(string.Format("Unknown field visibility: {0}", field.Attributes & FieldAttributes.FieldAccessMask));
            }
        }

        public static bool IsAccessible(MethodReference target, TypeReference scope)
        {
            var method = target.Resolve();
            var targetType = method.DeclaringType;

            switch (method.Attributes & MethodAttributes.MemberAccessMask)
            {
                case MethodAttributes.CompilerControlled:
                    return false;

                case MethodAttributes.Private:
                    return scope.DeclaredBy(targetType);

                case MethodAttributes.FamANDAssem:
                    return targetType.Module == scope.Resolve().Module && (scope.DeclaredBy(targetType) || scope.DerivesFrom(targetType));

                case MethodAttributes.Assembly:
                    return targetType.Module == scope.Resolve().Module;

                case MethodAttributes.Family:
                    return scope.DeclaredBy(targetType) || scope.DerivesFrom(targetType);

                case MethodAttributes.FamORAssem:
                    return targetType.Module == scope.Resolve().Module || scope.DeclaredBy(targetType) || scope.DerivesFrom(targetType);

                case MethodAttributes.Public:
                    return true;
                    
                default:
                    throw new NotSupportedException(string.Format("Unknown method visibility: {0}", method.Attributes & MethodAttributes.MemberAccessMask));
            }
        }

        public static bool IsAccessible(TypeReference target, TypeReference scope)
        {
            var type = target.Resolve();

            switch (type.Attributes & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.NotPublic:
                case TypeAttributes.NestedAssembly:
                    return type.Module == scope.Module;

                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    return true;

                case TypeAttributes.NestedPrivate:
                    return scope.DeclaredBy(type);

                case TypeAttributes.NestedFamily:
                    return scope.DerivesFrom(type);

                case TypeAttributes.NestedFamANDAssem:
                    return scope.DerivesFrom(type) && type.Module == scope.Module;

                case TypeAttributes.NestedFamORAssem:
                    return scope.DerivesFrom(type) || type.Module == scope.Module;

                default:
                    throw new NotSupportedException(string.Format("Unknown type visibility: {0}", type.Attributes & TypeAttributes.VisibilityMask));
            }
        }

        public static TypeReference GetBuiltInTypeReference(AssemblyEmitter assemblyEmitter, Type type)
        {
            switch (type.FullName)
            {
                case "System.String":
                    return assemblyEmitter.TypeSystem.String;

                case "System.Boolean":
                    return assemblyEmitter.TypeSystem.Boolean;
                    
                case "System.SByte":
                    return assemblyEmitter.TypeSystem.SByte;

                case "System.Byte":
                    return assemblyEmitter.TypeSystem.Byte;

                case "System.Char":
                    return assemblyEmitter.TypeSystem.Char;

                case "System.Int16":
                    return assemblyEmitter.TypeSystem.Int16;

                case "System.Uint16":
                    return assemblyEmitter.TypeSystem.UInt16;

                case "System.Int32":
                    return assemblyEmitter.TypeSystem.Int32;

                case "System.UInt32":
                    return assemblyEmitter.TypeSystem.UInt32;

                case "System.Int64":
                    return assemblyEmitter.TypeSystem.Int64;

                case "System.UInt64":
                    return assemblyEmitter.TypeSystem.UInt64;

                case "System.Single":
                    return assemblyEmitter.TypeSystem.Single;

                case "System.Double":
                    return assemblyEmitter.TypeSystem.Double;

                default:
                    return null;
            }
        }

        static MetadataHelpers()
        {
            assignmentMap = new Dictionary<MetadataType, MetadataType[]>();

            assignmentMap[MetadataType.Boolean] = new MetadataType[0];
            assignmentMap[MetadataType.Byte] = new MetadataType[0];
            assignmentMap[MetadataType.SByte] = new MetadataType[0];
            assignmentMap[MetadataType.UIntPtr] = new MetadataType[0];
            assignmentMap[MetadataType.IntPtr] = new MetadataType[0];

            assignmentMap[MetadataType.Char] = new MetadataType[]
            {
                MetadataType.Byte,
                MetadataType.UInt16
            };

            assignmentMap[MetadataType.UInt16] = new MetadataType[]
            {
                MetadataType.Byte,
                MetadataType.Char
            };

            assignmentMap[MetadataType.UInt32] = new MetadataType[]
            {
                MetadataType.Byte,
                MetadataType.Char,
                MetadataType.UInt16
            };

            assignmentMap[MetadataType.UInt64] = new MetadataType[]
            {
                MetadataType.Byte,
                MetadataType.Char,
                MetadataType.UInt16,
                MetadataType.UInt32
            };

            assignmentMap[MetadataType.Int16] = new MetadataType[]
            {
                MetadataType.SByte,
                MetadataType.Byte
            };

            assignmentMap[MetadataType.Int32] = new MetadataType[]
            {
                MetadataType.SByte,
                MetadataType.Byte,
                MetadataType.Char,
                MetadataType.Int16,
                MetadataType.UInt16
            };

            assignmentMap[MetadataType.Int64] = new MetadataType[]
            {
                MetadataType.SByte,
                MetadataType.Byte,
                MetadataType.Char,
                MetadataType.Int16,
                MetadataType.UInt16,
                MetadataType.Int32,
                MetadataType.UInt32
            };

            assignmentMap[MetadataType.Single] = new MetadataType[]
            {
                MetadataType.SByte,
                MetadataType.Byte,
                MetadataType.Char,
                MetadataType.Int16,
                MetadataType.UInt16,
                MetadataType.Int32,
                MetadataType.UInt32
            };

            assignmentMap[MetadataType.Double] = new MetadataType[]
            {
                MetadataType.SByte,
                MetadataType.Byte,
                MetadataType.Char,
                MetadataType.Int16,
                MetadataType.UInt16,
                MetadataType.Int32,
                MetadataType.UInt32,
                MetadataType.Int64,
                MetadataType.UInt64,
                MetadataType.Single
            };
        }

        // Doesn't include self
        private static Dictionary<MetadataType, MetadataType[]> assignmentMap;
    }
}
