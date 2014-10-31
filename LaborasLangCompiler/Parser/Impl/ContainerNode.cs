using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    interface ContainerNode
    {
        FunctionDeclarationNode GetFunction();
        ClassNode GetClass();
        ExpressionNode GetSymbol(string name, SequencePoint point);
    }
}
