﻿using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;

namespace LaborasLangCompiler.Parser.Impl
{
    class LiteralNode : ExpressionNode, ILiteralNode, AmbiguousNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Literal; } }
        public dynamic Value { get; private set; }
        public override TypeWrapper TypeWrapper { get { return returnType; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return false; }
        }

        private TypeWrapper returnType;

        private LiteralNode(dynamic value, TypeWrapper type, SequencePoint point)
            : base(point)
        {
            this.returnType = type;
            this.Value = value;
        }

        public static LiteralNode Parse(Parser parser, Context parentBlock, AstNode lexerNode)
        {
            lexerNode = lexerNode.Children[0];
            var point = parser.GetSequencePoint(lexerNode);
            var type = ParseLiteralType(parser, lexerNode);
            dynamic value = ParseValue(lexerNode.Content.ToString(), type, point);
            return new LiteralNode(value, type, point);
        }

        private static TypeWrapper ParseLiteralType(Parser parser, AstNode lexerNode)
        {
            switch(lexerNode.Type)
            {
                case Lexer.TokenType.Integer:
                    return parser.Int32;
                case Lexer.TokenType.StringLiteral:
                    return parser.String;
                case Lexer.TokenType.Float:
                    return parser.Float;
                case Lexer.TokenType.True:
                case Lexer.TokenType.False:
                    return parser.Bool;
                case Lexer.TokenType.Double:
                    return parser.Double;
                default:
                    throw new ParseException(parser.GetSequencePoint(lexerNode), "Unknown lexer type {0}", lexerNode.Type);
            }
        }

        private static dynamic ParseValue(string value, TypeWrapper type, SequencePoint point)
        {
            try
            {
                switch (type.FullName)
                {
                    case "System.String":
                        return value.Substring(1, value.Length - 2);
                    case "System.Boolean":
                        return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                    case "System.Char":
                        return Convert.ToChar(value, CultureInfo.InvariantCulture);
                    case "System.SByte":
                        return Convert.ToSByte(value, CultureInfo.InvariantCulture);
                    case "System.Byte":
                        return Convert.ToByte(value, CultureInfo.InvariantCulture);
                    case "System.Int16":
                        return Convert.ToInt16(value, CultureInfo.InvariantCulture);
                    case "System.Uint16":
                        return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
                    case "System.Int32":
                        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    case "System.UInt32":
                        return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                    case "System.Int64":
                        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
                    case "System.UInt64":
                        return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                    case "System.Single":
                        return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                    case "System.Double":
                        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    case "System.Decimal":
                        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                    default:
                        throw new TypeException(point, "Cannot parse literal of type {0}", type.FullName);
                }
            }
            catch(OverflowException)
            {
                throw new ParseException(point, "Could not parse {0} as {1}, overflow", value, type.FullName);
            }
            catch(FormatException)
            {
                throw new ParseException(point, "Could not parse {0} as {1}, format error", value, type.FullName);
            }
        }

        public ExpressionNode RemoveAmbiguity(Parser parser, TypeWrapper expectedType)
        {
            if (expectedType == TypeWrapper)
                return this;

            if (!TypeWrapper.IsAssignableTo(expectedType))
                throw new TypeException(SequencePoint, "Cannot assign {0} which is {1} to {2}", Value, TypeWrapper, expectedType);

            return new LiteralNode(ConvertLiteral(this, expectedType), expectedType, SequencePoint);
        }

        private static dynamic ConvertLiteral(LiteralNode node, TypeWrapper type)
        {
            dynamic value = node.Value;
            if(node.TypeWrapper.FullName == "System.String")
            {
                return ParseValue((string)value, type, node.SequencePoint);
            }
            switch (type.FullName)
            {
                case "System.Boolean":
                    return (Boolean)value;
                case "System.Char":
                    return (Char)value;
                case "System.SByte":
                    return (SByte)value;
                case "System.Byte":
                    return (Byte)value;
                case "System.Int16":
                    return (Int16)value;
                case "System.Uint16":
                    return (UInt16)value;
                case "System.Int32":
                    return (Int32)value;
                case "System.UInt32":
                    return (UInt32)value;
                case "System.Int64":
                    return (Int64)value;
                case "System.UInt64":
                    return (UInt64)value;
                case "System.Single":
                    return (Single)value;
                case "System.Double":
                    return (Double)value;
                case "System.Decimal":
                    return (Decimal)value;
                default:
                    throw new TypeException(node.SequencePoint, "Expected type {0} is not a LaborasLang literal type", type);
            }
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Literal:");
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(TypeWrapper.FullName);
            builder.Indent(indent + 1).AppendLine("Value:");
            builder.Indent(indent + 2).AppendLine(Value.ToString());
            return builder.ToString();
        }
    }
}
