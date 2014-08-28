using LaborasLangCompiler.Parser.Impl.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    interface AmbiguousNode
    {
        ExpressionNode RemoveAmbiguity(Parser parser, TypeWrapper expectedType = null);
    }
}
