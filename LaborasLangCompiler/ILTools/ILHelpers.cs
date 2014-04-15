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

        public static bool IsNumeralType(this TypeReference type)
        {
            return type.IsIntegerType() || type.IsFloatingPointType();
        }

        public static bool IsAssignableTo(this TypeReference left, TypeReference right)
        {
            var leftName = left.FullName;
            var rightName = right.FullName;

            if (leftName == rightName)
            {
                return true;
            }

            if (left.IsPrimitive && right.IsPrimitive)
            {
                if (left.IsIntegerType() && right.IsIntegerType())
                {
                    if (left.IsSignedInteger() != right.IsSignedInteger())
                    {
                        return false;
                    }

                    int leftWidth = GetIntegerWidth(left),
                        rightWidth = GetIntegerWidth(right);

                    if (leftWidth == rightWidth)
                    {
                        return true;
                    }

                    if (leftWidth == 0 || rightWidth == 0)
                    {
                        return false;
                    }

                    if (leftWidth >= rightWidth)
                    {
                        return true;
                    }

                    return false;
                }

                if (left.IsFloatingPointType() && right.IsFloatingPointType())
                {
                    if (leftName == "System.Float")
                    {
                        return false;
                    }

                    if (leftName == "System.Double" && rightName == "System.Decimal")
                    {
                        return false;
                    }

                    if (leftName == "System.Decimal" && rightName == "System.Double")
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }

            var leftType = left.Resolve();
            var rightType = right.Resolve();

            if (left.Resolve().IsInterface)
            {
                return right.Resolve().Interfaces.Contains(left);
            }

            if (left.Resolve().HasGenericParameters || right.Resolve().HasGenericParameters)
            {
                throw new NotSupportedException("Generic types are not supported!");
            }

            rightType = rightType.BaseType.Resolve();
            
            while (rightType != null)
            {
                if (leftName == rightType.FullName)
                {
                    return true;
                }

                rightType = rightType.BaseType.Resolve();
            }

            return false;
        }

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
