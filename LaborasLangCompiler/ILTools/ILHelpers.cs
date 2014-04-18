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
        public static int GetIntegerWidth(TypeReference type)
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

        public static bool IsSignedInteger(this TypeReference type)
        {
            return SignedIntegerTypes.Any(x => x == type.FullName);
        }

        public static bool IsIntegerType(this TypeReference type)
        {
            return IntegerTypes.Any(x => x == type.FullName);
        }

        public static bool IsFloatingPointType(this TypeReference type)
        {
            return FloatingPointTypes.Any(x => x == type.FullName);
        }

        public static bool IsNumericType(this TypeReference type)
        {
            return type.IsIntegerType() || type.IsFloatingPointType();
        }
        public static bool isStringType(this TypeReference type)
        {
            return type.FullName == "System.String";
        }

        public static bool IsAssignableTo(this TypeReference right, TypeReference left)
        {
            var leftName = left.FullName;
            var rightName = right.FullName;

            if (leftName == rightName)
            {
                return true;
            }

            if (left.IsPrimitive && right.IsPrimitive)
            {
                return assignmentMap[leftName].Any(x => x == rightName);
            }

            var leftType = left.Resolve();
            var rightType = right.Resolve();

            if (leftType.HasGenericParameters || rightType.HasGenericParameters)
            {
                throw new NotSupportedException("Generic types are not supported!");
            }

            if (leftType.IsInterface)
            {
                return rightType.Interfaces.Any(x => x.FullName == leftName);
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
