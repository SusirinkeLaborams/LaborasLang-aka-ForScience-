using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Types
{
    class FunctionType : BaseType
    {
        public readonly BaseType ReturnType;
        public readonly List<BaseType> Arguments;

        public FunctionType(BaseType returnType, List<BaseType> arguments)
        {
            ReturnType = returnType;
            Arguments = new List<BaseType>(arguments);
        }

        public override bool Equals(object other)
        {
            if (!(other is FunctionType))
                return false;
            FunctionType that = (FunctionType)other;
            return ReturnType.Equals(that.ReturnType) && Arguments.SequenceEqual(that.Arguments);
        }

        public override int GetHashCode()
        {
            return ReturnType.GetHashCode() ^ Arguments.GetHashCode();
        }
    }
}
