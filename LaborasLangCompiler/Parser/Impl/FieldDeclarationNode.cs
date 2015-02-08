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
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    class FieldDeclarationNode : ParserNode, Context
    {
        public override NodeType Type { get { return NodeType.ParserInternal; } }
        public FieldReference FieldReference { get { return FieldDefinition; } }
        public FieldDefinition FieldDefinition { get { return field.Value; } }
        public TypeReference TypeReference { get; private set; }
        public string Name { get; set; }
        public ExpressionNode Initializer { get; set; }
        public bool IsStatic { get; set; }
        public TypeReference DeclaringType { get { return parent.TypeReference; } }
        public MemberReference MemberReference { get { return FieldReference; } }

        private Modifiers modifiers;
        private SequencePoint point;
        private AstNode initializer;
        private Lazy<FieldDefinition> field;
        private ClassNode parent;

        public FunctionDeclarationNode GetMethod()
        {
            return null;
        }

        public ClassNode GetClass()
        {
            return parent.GetClass();
        }

        public ExpressionNode GetSymbol(string name, Context scope, SequencePoint point)
        {
            return parent.GetSymbol(name, scope, point);
        }

        public bool IsStaticContext()
        {
            return IsStatic;
        }

        public FieldDeclarationNode(Parser parser, ClassNode parent, DeclarationInfo declaration, SequencePoint point)
            :base(point)
        {
            this.IsStatic = true;
            this.point = point;
            this.initializer = declaration.Initializer;
            this.Name = declaration.SymbolName.GetSingleSymbolOrThrow();
            this.parent = parent;
            this.TypeReference = TypeNode.Parse(parser, this, declaration.Type);
            this.modifiers = declaration.Modifiers;
            this.field = new Lazy<FieldDefinition>(() => new FieldDefinition(Name, GetAttributes(), TypeReference));

            if (TypeReference.IsAuto() && !declaration.Initializer.IsNull && declaration.Initializer.IsFunctionDeclaration())
                TypeReference = FunctionDeclarationNode.ParseFunctorType(parser, parent, declaration.Initializer);

            if (!TypeReference.IsAuto())
            {
                if (TypeReference.IsVoid())
                    Errors.ReportAndThrow(ErrorCode.VoidLValue, point, "Cannot declare a field of type void");
                parent.TypeEmitter.AddField(FieldDefinition);
            }

        }

        public void Initialize(Parser parser)
        {
            if(initializer.IsNull)
            {
                if (TypeReference.IsAuto())
                    Errors.ReportAndThrow(ErrorCode.MissingInit, point, "Type inference requires initialization");
                return;
            }

            Initializer = ExpressionNode.Parse(parser, this, initializer, TypeReference);

            if (TypeReference.IsAuto())
            {
                TypeReference = Initializer.ExpressionReturnType;
                parent.TypeEmitter.AddField(FieldDefinition);
            }
            else
            {
                if (!Initializer.ExpressionReturnType.IsAssignableTo(TypeReference))
                {
                    Utils.Report(ErrorCode.TypeMissmatch, Initializer.SequencePoint, 
                        "Field of type {0} initialized with {1}", TypeReference, Initializer.ExpressionReturnType);
                }
            }

            if(parser.ProjectParser.ShouldEmit)
                parent.TypeEmitter.AddFieldInitializer(FieldDefinition, Initializer);
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
                    TooManyAccessMods(point, modifiers);
                else
                    ret |= FieldAttributes.Public;
            }
            else if (modifiers.HasFlag(Modifiers.Protected))
            {
                if (modifiers.HasFlag(Modifiers.Private | Modifiers.Public))
                    TooManyAccessMods(point, modifiers);
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
            Errors.ReportAndThrow(ErrorCode.InvalidFieldMods, point, String.Format("Only one of {0} is allowed, {1} found", all, mods | all));
        }
    }
}
