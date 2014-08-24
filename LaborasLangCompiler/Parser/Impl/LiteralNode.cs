using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.LexingTools;
using Mono.Cecil.Cil;
using LaborasLangCompiler.Parser.Impl.Wrappers;

namespace LaborasLangCompiler.Parser.Impl
{
    class LiteralNode : RValueNode, ILiteralNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Literal; } }
        public dynamic Value { get; private set; }
        public override TypeWrapper TypeWrapper { get { return returnType; } }

        private TypeWrapper returnType;
        private LiteralNode(dynamic value, TypeWrapper type, SequencePoint point)
            : base(point)
        {
            this.returnType = type;
            this.Value = value;
        }
        public static LiteralNode Parse(Parser parser, ContainerNode parentBlock, AstNode lexerNode)
        {
            lexerNode = lexerNode.Children[0];
            var point = parser.GetSequencePoint(lexerNode);
            var type = ParseLiteralType(parser, lexerNode);
            dynamic value = ParseValue(parser.ValueOf(lexerNode), type, point);
            return new LiteralNode(value, type, point);
        }

        private static TypeWrapper ParseLiteralType(Parser parser, AstNode lexerNode)
        {
            switch(lexerNode.Token.Name)
            {
                case Lexer.IntegerLiteral:
                    return parser.Int;
                case Lexer.StringLiteral:
                    return parser.String;
                case Lexer.FloatLiteral:
                    return parser.Double;
                case Lexer.BooleanLiteral:
                    return parser.Bool;
                default:
                    throw new ParseException(parser.GetSequencePoint(lexerNode), "Unknown lexer type {0}", lexerNode.Token.Name);
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
                        return Convert.ToBoolean(value);
                    case "System.Char":
                        return Convert.ToChar(value);
                    case "System.SByte":
                        return Convert.ToSByte(value);
                    case "System.Byte":
                        return Convert.ToByte(value);
                    case "System.Int16":
                        return Convert.ToInt16(value);
                    case "System.Uint16":
                        return Convert.ToUInt16(value);
                    case "System.Int32":
                        return Convert.ToInt32(value);
                    case "System.UInt32":
                        return Convert.ToUInt32(value);
                    case "System.Int64":
                        return Convert.ToInt64(value);
                    case "System.Uint64":
                        return Convert.ToUInt64(value);
                    case "System.Single":
                        return Convert.ToSingle(value);
                    case "System.Double":
                        return Convert.ToDouble(value);
                    case "System.Decimal":
                        return Convert.ToDecimal(value);
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
        public override string ToString()
        {
            return "(Literal: " + TypeWrapper.FullName + " " + Value + ")";
        }
    }
}
