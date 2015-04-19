using Lexer.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using LaborasLangCompiler.Parser.Utils;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class FieldDeclarationNode : ContextNode
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public FieldReference FieldReference { get { return FieldDefinition; } }
        public FieldDefinition FieldDefinition { get { return field.Value; } }
        public TypeReference TypeReference { get; private set; }
        public string Name { get; set; }
        public ExpressionNode Initializer { get; set; }
        public bool IsStatic { get; set; }
        public TypeReference DeclaringType { get { return GetClass().TypeReference; } }
        public MemberReference MemberReference { get { return FieldReference; } }

        private Modifiers modifiers;
        private readonly IAbstractSyntaxTree initializer;
        private readonly Lazy<FieldDefinition> field;

        public override ClassNode GetClass()
        {
            return Parent.GetClass();
        }

        public override FunctionDeclarationNode GetMethod()
        {
            return null;
        }

        public override ExpressionNode GetSymbol(string name, ContextNode scope, SequencePoint point)
        {
            return Parent.GetSymbol(name, scope, point);
        }

        public override bool IsStaticContext()
        {
            return IsStatic;
        }

        public FieldDeclarationNode(ClassNode parent, DeclarationInfo declaration, SequencePoint point)
            :base(parent.Parser, parent, point)
        {
            this.IsStatic = true;
            this.initializer = declaration.Initializer;
            this.Name = declaration.SymbolName.GetSingleSymbolOrThrow();
            this.TypeReference = TypeNode.Parse(this, declaration.Type);
            this.modifiers = declaration.Modifiers;
            this.field = new Lazy<FieldDefinition>(() => new FieldDefinition(Name, GetAttributes(), TypeReference));

            if (TypeReference.IsAuto() && declaration.Initializer != null && declaration.Initializer.IsFunctionDeclaration())
                TypeReference = FunctionDeclarationNode.ParseFunctorType(parent, declaration.Initializer);

            if (!TypeReference.IsAuto())
            {
                if (TypeReference.IsVoid())
                    ErrorCode.VoidLValue.ReportAndThrow(point, "Cannot declare a field of type void");
                parent.TypeEmitter.AddField(FieldDefinition);
            }

        }

        public void Initialize()
        {
            if(initializer == null)
            {
                if (TypeReference.IsAuto())
                    ErrorCode.MissingInit.ReportAndThrow(SequencePoint, "Type inference requires initialization");
                return;
            }

            Initializer = ExpressionNode.Parse(this, initializer, TypeReference);

            if (TypeReference.IsAuto())
            {
                if(Initializer.ExpressionReturnType.IsTypeless())
                {
                    ErrorCode.InferrenceFromTypeless.ReportAndThrow(Initializer.SequencePoint, "Cannot infer type from a typeless expression");
                }
                TypeReference = Initializer.ExpressionReturnType;
                GetClass().TypeEmitter.AddField(FieldDefinition);
            }
            else
            {
                if (!Initializer.ExpressionReturnType.IsAssignableTo(TypeReference))
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(Initializer.SequencePoint, 
                        "Field of type {0} initialized with {1}", TypeReference, Initializer.ExpressionReturnType);
                }
            }

            if(Parser.ProjectParser.ShouldEmit)
                GetClass().TypeEmitter.AddFieldInitializer(FieldDefinition, Initializer);
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
                if (TypeReference.IsFunctorType())
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
                    TooManyAccessMods(SequencePoint, modifiers);
                else
                    ret |= FieldAttributes.Public;
            }
            else if (modifiers.HasFlag(Modifiers.Protected))
            {
                if (modifiers.HasFlag(Modifiers.Private | Modifiers.Public))
                    TooManyAccessMods(SequencePoint, modifiers);
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

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("FieldDeclaration:");
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(TypeReference.FullName);
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

        private static void TooManyAccessMods(SequencePoint point, Modifiers mods)
        {
            var all = ModifierUtils.GetAccess();
            ErrorCode.InvalidFieldMods.ReportAndThrow(point, String.Format("Only one of {0} is allowed, {1} found", all, mods | all));
        }
    }
}
