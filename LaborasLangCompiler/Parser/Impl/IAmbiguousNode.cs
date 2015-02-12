using LaborasLangCompiler.Parser.Impl.Wrappers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    interface IAmbiguousNode : IExpressionNode
    {
        ExpressionNode RemoveAmbiguity(Parser parser, TypeReference expectedType);
    }
}
