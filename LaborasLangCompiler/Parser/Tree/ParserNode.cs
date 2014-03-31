using LaborasLangCompiler.Parser.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Tree
{
    abstract class ParserNode
    {
        public readonly NodeType NodeType;
        private readonly ParserNode parent;
    }

    enum NodeType
    {
        TypeLiteral,
        TypeVariable,
        TypeBinaryOperator,
        TypeUnaryOperator,
        TypeAssignmentOperator,
        TypeFunctionCall,
        TypeSymbolDeclaration,
        TypeFunction,
        TypeFunctionArg//deklaracijos arg, ne call
    }
}
