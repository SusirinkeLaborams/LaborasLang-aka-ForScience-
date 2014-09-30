using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl.Wrappers
{
    class InternalField : FieldWrapper
    {
        public FieldReference FieldReference { get { return FieldDefinition; } }
        public FieldDefinition FieldDefinition { get; set; }
        public TypeWrapper TypeWrapper { get; set; }
        public string Name { get; set; }
        public ExpressionNode Initializer { get; set; }
        public bool IsStatic { get; set; }
        public DeclarationInfo Declaration { get; private set; }

        private Modifiers modifiers;
        private SequencePoint point;
        public InternalField(DeclarationInfo declaration, SequencePoint point)
        {
            this.Declaration = declaration;
            this.IsStatic = true;
            this.point = point;
            this.modifiers = declaration.Modifiers;
        }

        public FieldAttributes GetAttributes()
        {
            FieldAttributes ret = 0;
            if(!modifiers.HasAccess())
            {
                modifiers |= Modifiers.Private;
            }
            if(!modifiers.HasStorage())
            {
                modifiers |= Modifiers.NoInstance;
            }
            if(!modifiers.HasMutability())
            {
                if (TypeWrapper.IsFunctorType())
                    modifiers |= Modifiers.Const;
                else
                    modifiers |= Modifiers.Mutable;
            }

            if (modifiers.HasFlag(Modifiers.Private))
            {
                ret |= FieldAttributes.Private;
            }
            else if (modifiers.HasFlag(Modifiers.Public))
            {
                if (modifiers.HasFlag(Modifiers.Private))
                    throw new ParseException(point, "Illegal method declaration, only one access modifier allowed");
                else
                    ret |= FieldAttributes.Public;
            }
            else if (modifiers.HasFlag(Modifiers.Protected))
            {
                if (modifiers.HasFlag(Modifiers.Private | Modifiers.Public))
                    throw new ParseException(point, "Illegal method declaration, only one access modifier allowed");
                else
                    ret |= FieldAttributes.Family;
            }

            if(modifiers.HasFlag(Modifiers.Const))
            {
                ret |= FieldAttributes.InitOnly;
            }

            if (modifiers.HasFlag(Modifiers.NoInstance))
            {
                ret |= FieldAttributes.Static;
            }
            else
            {
                throw new NotImplementedException("Only static methods allowed");
            }

            return ret;
        }

        public string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Field:");
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(TypeWrapper.FullName);
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name); 
            builder.Indent(indent + 1).AppendLine("Modifiers:");
            builder.Indent(indent + 2).AppendLine(modifiers.ToString());
            if(Initializer != null)
            {
                builder.Indent(indent + 1).AppendLine("Initializer:");
                builder.AppendLine(Initializer.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}
