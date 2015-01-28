﻿using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Parser.Impl.Wrappers;

namespace LaborasLangCompiler.Parser.Impl
{
    class LocalVariableNode : ExpressionNode, ILocalVariableNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.LocalVariable; } }
        public VariableDefinition LocalVariable { get; private set; }
        public override TypeReference ExpressionReturnType { get { return LocalVariable.VariableType; } }
        public string Name {get { return LocalVariable.Name; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return !isConst; }
        }

        private bool isConst;
        public LocalVariableNode(SequencePoint point, VariableDefinition variable, bool isConst)
            : base(point)
        {
            this.LocalVariable = variable;
            this.isConst = isConst;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Local:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(ExpressionReturnType.FullName);
            return builder.ToString();
        }
    }

    class FunctionArgumentNode : ExpressionNode, IMethodParamNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.FunctionArgument; } }
        public ParameterDefinition Param { get; private set; }
        public bool IsMethodStatic { get; set; }
        public override TypeReference ExpressionReturnType { get { return Param.ParameterType; } }
        public string Name { get { return Param.Name; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return true; }
        }


        public FunctionArgumentNode(ParameterDefinition param, bool isFunctionStatic, SequencePoint point)
            : base(point)
        {
            this.Param = param;
            IsMethodStatic = isFunctionStatic;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Param:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(ExpressionReturnType.FullName);
            return builder.ToString();
        }
    }

    class FieldNode : MemberNode, IFieldNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Field; } }
        public IExpressionNode ObjectInstance { get; private set; }
        public FieldReference Field { get; private set; }
        public override TypeReference ExpressionReturnType { get { return Field.FieldType; } }
        public override bool IsGettable
        {
            get
            {
                return true;
            }
        }
        public override bool IsSettable
        {
            get
            {
                return !Field.Resolve().Attributes.HasFlag(FieldAttributes.Literal | FieldAttributes.InitOnly);
            }
        }

        private Parser parser;
        public FieldNode(Parser parser, ExpressionNode instance, FieldReference field, Context parent, SequencePoint point)
            : base(field, parent, point)
        {
            ObjectInstance = ThisNode.GetAccessingInstance(field, instance, parent, point);
            this.Field = field;
            this.parser = parser;
        }
        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Field:");
            builder.Indent(indent + 1).AppendLine("Name:");
            builder.Indent(indent + 2).AppendLine(Field.Name);
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(ExpressionReturnType.FullName);
            return builder.ToString();
        }
    }
    /*
    class PropertyNode : SymbolNode, IPropertyNode
    {
        public override LValueNodeType LValueType { get { return LValueNodeType.Property; } }
        public IExpressionNode ObjectInstance { get; }
        public PropertyReference Property { get; }
    }*/
}
