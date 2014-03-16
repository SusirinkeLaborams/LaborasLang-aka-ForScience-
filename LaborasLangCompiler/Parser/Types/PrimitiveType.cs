using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Types
{
    class PrimitiveType : BaseType
    {
        public readonly string Name;
        public PrimitiveType(string name) 
        {
            Name = name;
        }

        public override bool Equals(object other)
        {
            if (!(other is PrimitiveType))
                return false;
            return Name == ((PrimitiveType)other).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}