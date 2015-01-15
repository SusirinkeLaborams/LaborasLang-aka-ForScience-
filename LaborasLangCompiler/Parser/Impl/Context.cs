using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    interface Context
    {
        FunctionDeclarationNode GetMethod();
        ClassNode GetClass();
        ExpressionNode GetSymbol(string name, TypeReference scope, SequencePoint point);
        bool IsStaticContext();
    }
}
