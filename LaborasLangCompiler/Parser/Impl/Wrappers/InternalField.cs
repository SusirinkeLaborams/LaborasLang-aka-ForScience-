using LaborasLangCompiler.Parser.Exceptions;
using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.ILTools;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class InternalField : FieldWrapper
    {
        private FieldDeclarationNode node;

        public FieldReference FieldReference { get { return node.FieldReference; } }
        public TypeReference TypeReference { get { return node.TypeReference; } }
        public string Name { get { return node.Name; } }
        public TypeReference DeclaringType { get { return node.DeclaringType; } }
        public bool IsStatic { get { return node.IsStatic; } }
        public MemberReference MemberReference { get { return FieldReference; } }

        public InternalField(FieldDeclarationNode node)
        {
            this.node = node;
        }

        public void Initialize(Parser parser)
        {
            node.Initialize(parser);
        }

        public string ToString(int indent)
        {
            return node.ToString(indent);
        }
    }
}
