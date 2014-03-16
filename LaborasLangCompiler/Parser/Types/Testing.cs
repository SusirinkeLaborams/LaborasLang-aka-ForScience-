using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Types
{
    class Testing
    {
        public static void TestTypes()
        {
            BaseType tVoid = new PrimitiveType("void");
            BaseType tFloat = new PrimitiveType("float");
            BaseType tInt = new PrimitiveType("int");

            Debug.Print("void == void? " + tVoid.Equals(tVoid));
            Debug.Print("void == float? " + tVoid.Equals(tFloat));
            Debug.Print("float == float? " + tFloat.Equals(tFloat));
            Debug.Print("float == void? " + tFloat.Equals(tVoid));

            BaseType function = new FunctionType(tVoid, new List<BaseType> { tFloat, tFloat });
            BaseType anotherFunction = new FunctionType(tInt, new List<BaseType> { tInt, tFloat });
            BaseType moar = new FunctionType(tInt, new List<BaseType> { tFloat, tInt });

            Debug.Print("void(float, float) == void(float, float)? " + function.Equals(function));
            Debug.Print("int(int, float) == void(float, float)? " + anotherFunction.Equals(function));
            Debug.Print("int(float, int) == int(int, float)? " + moar.Equals(anotherFunction));
            Debug.Print("int(float, int) == int(float, int))? " + moar.Equals(moar));
        }
    }
}