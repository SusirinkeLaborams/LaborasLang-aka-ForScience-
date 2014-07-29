using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools
{
    static class ILHelpers
    {
        // 0 means native
        public static int GetIntegerWidth(this TypeReference type)
        {
            var typeName = type.FullName;
            
            if (typeName == "System.IntPtr" || typeName == "System.UIntPtr")
            {
                return 0;
            }

            if (typeName == "System.SByte" || typeName == "System.Byte")
            {
                return 1;
            }

            if (typeName == "System.Int16" || typeName == "System.UInt16" || typeName == "System.Char")
            {
                return 2;
            }

            if (typeName == "System.Int32" || typeName == "System.UInt32")
            {
                return 4;
            }

            if (typeName == "System.Int64" || typeName == "System.UInt64")
            {
                return 8;
            }

            throw new NotSupportedException("Type is not an integer!");
        }

        public static float GetFloatWidth(TypeReference type)
        {
            var typeName = type.FullName;

            if (typeName == "System.Single")
            {
                return 4;
            }

            if (typeName == "System.Double")
            {
                return 8;
            }

            if (typeName == "System.Decimal")
            {
                return 16;
            }

            throw new NotSupportedException("Type is not a float!");
        }

        public static bool IsUnsignedInteger(this TypeReference type)
        {
            return type.IsIntegerType() && !type.IsSignedInteger();
        }

        public static bool IsSignedInteger(this TypeReference type)
        {
            return SignedIntegerTypes.Any(signedIntType => signedIntType == type.FullName);
        }

        public static bool IsIntegerType(this TypeReference type)
        {
            return IntegerTypes.Any(intType => intType == type.FullName);
        }

        public static bool IsFloatingPointType(this TypeReference type)
        {
            return FloatingPointTypes.Any(floatType => floatType == type.FullName);
        }

        public static bool IsNumericType(this TypeReference type)
        {
            return type.IsIntegerType() || type.IsFloatingPointType();
        }

        public static bool IsStringType(this TypeReference type)
        {
            return type.FullName == "System.String";
        }

        public static bool IsBooleanType(this TypeReference type)
        {
            return type.FullName == "System.Boolean";
        }

        public static bool IsFunctorType(this TypeReference type)
        {
            return type.FullName.StartsWith("$Functors.");
        }

        public static bool IsAssignableTo(this TypeReference right, TypeReference left)
        {
            if (left.IsByReference)
            {
                left = left.GetElementType();
            }

            if (right.IsByReference)
            {
                right = right.GetElementType();
            }

            var leftName = left.FullName;
            var rightName = right.FullName;

            if (leftName == rightName)
            {
                return true;
            }

            if (left.IsPrimitive && right.IsPrimitive)
            {
                return assignmentMap[leftName].Any(assignableType => assignableType == rightName);
            }

            var leftType = left.Resolve();
            var rightType = right.Resolve();

            if (leftType.HasGenericParameters || rightType.HasGenericParameters)
            {
                throw new NotSupportedException("Generic types are not supported!");
            }

            if (leftType.IsInterface)
            {
                return rightType.Interfaces.Any(interfaze => interfaze.FullName == leftName);
            }
                        
            while (rightType.BaseType != null)
            {
                rightType = rightType.BaseType.Resolve();

                if (leftName == rightType.FullName)
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

        public static bool MatchesArgumentList(this MethodReference method, IReadOnlyList<TypeReference> desiredParameters)
        {
            var methodParameters = method.Resolve().Parameters; // Resolve is needed or otherwise we will not know methods parameter attributes

            // Doesn't match if parameter count doesn't match and either method has no parameters, or last parameter is neither params, not default one.
            if (methodParameters.Count != desiredParameters.Count &&
                (!method.HasParameters || (!method.IsParamsMethod() && (methodParameters.Last().Attributes & ParameterAttributes.HasDefault) == 0)))
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

            var lastParameterAttributes = parameters[parameters.Count - 1].CustomAttributes;
            return lastParameterAttributes.Any(attribute => attribute.AttributeType.FullName == "System.ParamArrayAttribute");
        }

        public static TypeReference GetFunctorReturnTypeAndArguments(AssemblyEmitter assemblyScope, TypeReference functorType, 
            out List<TypeReference> arguments)
        {
            if (!functorType.IsFunctorType())
            {
                throw new ArgumentException("functorType isn't a functor type!");
            }
            
            var invokeMethod = AssemblyRegistry.GetMethods(assemblyScope, functorType, "Invoke").Single();
            
            arguments = invokeMethod.Parameters.Select(parameter => parameter.ParameterType).ToList();
            return invokeMethod.ReturnType;
        }

        static ILHelpers()
        {
            assignmentMap = new Dictionary<string, string[]>();

            assignmentMap["System.Boolean"] = new string[0];
            assignmentMap["System.Byte"] = new string[0];
            assignmentMap["System.SByte"] = new string[0];
            assignmentMap["System.UIntPtr"] = new string[0];
            assignmentMap["System.IntPtr"] = new string[0];

            assignmentMap["System.Char"] = new string[]
            {
                "System.Byte", 
                "System.UInt16"
            };

            assignmentMap["System.UInt16"] = new string[]
            {
                "System.Byte", 
                "System.Char"
            };
            
            assignmentMap["System.UInt32"] = new string[]
            {
                "System.Byte", 
                "System.Char",
                "System.UInt16"
            };

            assignmentMap["System.UInt64"] = new string[]
            {
                "System.Byte", 
                "System.Char",
                "System.UInt16",
                "System.UInt32"
            };

            assignmentMap["System.Int16"] = new string[]
            {
                "System.SByte",
            };

            assignmentMap["System.Int32"] = new string[]
            {
                "System.Char",
                "System.SByte", 
                "System.Byte", 
                "System.Int16",
                "System.UInt16",
            };

            assignmentMap["System.Int64"] = new string[]
            {
                "System.Char",
                "System.SByte", 
                "System.Byte",
                "System.Int16",
                "System.UInt16",
                "System.Int32",
                "System.UInt32",
            };

            assignmentMap["System.Single"] = new string[]
            {
                "System.Char",
                "System.Byte",
                "System.SByte",
                "System.Char",
                "System.Int16",
                "System.UInt16",
                "System.Int32",
                "System.UInt32",
            };

            assignmentMap["System.Double"] = new string[]
            {
                "System.Single",
                "System.Char",
                "System.Byte",
                "System.SByte",
                "System.Char",
                "System.Int16",
                "System.UInt16",
                "System.Int32",
                "System.UInt32",
                "System.Int64",
                "System.UInt64",
            };

            assignmentMap["System.Decimal"] = new string[]
            {
                "System.Single"
            };
        }

        // Doesn't include self
        private static Dictionary<string, string[]> assignmentMap;

        private static readonly string[] IntegerTypes = new string[]
        {
            "System.Char",
            "System.Byte",
            "System.SByte",
            "System.Int16",
            "System.UInt16",
            "System.Int32",
            "System.UInt32",
            "System.Int64",
            "System.UInt64",
            "System.IntPtr",
            "System.UIntPtr"
        };

        private static readonly string[] SignedIntegerTypes = new string[]
        {
            "System.SByte",
            "System.Int16",
            "System.Int32",
            "System.Int64",
            "System.IntPtr"
        };

        private static readonly string[] FloatingPointTypes = new string[]
        {
            "System.Single",
            "System.Double",
            "System.Decimal"
        };
    }
}
