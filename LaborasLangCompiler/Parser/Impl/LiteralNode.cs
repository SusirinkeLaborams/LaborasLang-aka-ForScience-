using LaborasLangCompiler.Parser.Utils;
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
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;

namespace LaborasLangCompiler.Parser.Impl
{
    class LiteralNode : ExpressionNode, ILiteralNode, IAmbiguousNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.Literal; } }
        public Literal Value { get; private set; }
        public override TypeReference ExpressionReturnType { get { return type; } }
        public override bool IsGettable
        {
            get { return true; }
        }
        public override bool IsSettable
        {
            get { return false; }
        }

        private TypeReference type;

        private LiteralNode(Literal value, TypeReference type, SequencePoint point)
            : base(point)
        {
            this.type = type;
            this.Value = value;
        }

        public static LiteralNode Parse(ContextNode context, AstNode lexerNode)
        {
            lexerNode = lexerNode.Children[0];
            var point = context.Parser.GetSequencePoint(lexerNode);
            var type = ParseLiteralType(context.Parser, lexerNode);
            Literal value = new Literal(ParseValue(lexerNode.Content.ToString(), type, point));
            return new LiteralNode(value, type, point);
        }

        private static TypeReference ParseLiteralType(Parser parser, AstNode lexerNode)
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
                    ErrorCode.InvalidStructure.ReportAndThrow(parser.GetSequencePoint(lexerNode), "Unexpected literal type {0}", lexerNode.Type);
                    return null;//unreachable
            }
        }

        private static IConvertible ParseValue(string value, TypeReference type, SequencePoint point)
        {
            try
            {
                switch (type.FullName)
                {
                    case "System.String":
                        return value;
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
                        ErrorCode.InvalidStructure.ReportAndThrow(point, "Unexpected literal type {0}", type.FullName);
                        return null;//unreachable
                }
            }
            catch(OverflowException)
            {
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Could not parse {0} as {1}, overflow", value, type.FullName);
                return null;//unreachable
            }
            catch(FormatException)
            {
                ErrorCode.InvalidStructure.ReportAndThrow(point, "Could not parse {0} as {1}, format error", value, type.FullName);
                return null;//unreachable
            }
        }

        public ExpressionNode RemoveAmbiguity(ContextNode context, TypeReference expectedType)
        {
            if (expectedType.FullName == type.FullName)
                return this;

            var conversions = GetImplicitConversions(context.Parser, this);
            if(conversions.Any(c => c.TypeEquals(expectedType)))
            {
                return ConvertLiteral(this, expectedType);
            }
            else
            {
                //cannot convert
                return this;
            }
        }

        private static LiteralNode ConvertLiteral(LiteralNode node, TypeReference type)
        {
            return new LiteralNode(node.Value, type, node.SequencePoint);
        }

        private static IEnumerable<TypeReference> GetImplicitConversions(Parser parser, LiteralNode node)
        {
            var type = node.ExpressionReturnType;
            if(type.IsIntegerType())
            {
                if(type.IsSignedInteger())
                {
                    var value = (long)node.Value;
                    if (value > 0)
                    {
                        return parser.ProjectParser.MaxValues.Where(kv => kv.Key >= (ulong)value).Select(kv => kv.Value);
                    }
                    else
                    {
                        return parser.ProjectParser.MinValues.Where(kv => kv.Key >= value).Select(kv => kv.Value);
                    }
                }
                else
                {
                    var value = (ulong)node.Value;
                    return parser.ProjectParser.MaxValues.Where(kv => kv.Key >= value).Select(kv => kv.Value);
                }
            }
            else
            {
                return Enumerable.Empty<TypeReference>();
            }
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("Literal:");
            builder.Indent(indent + 1).AppendLine("Type:");
            builder.Indent(indent + 2).AppendLine(type.FullName);
            builder.Indent(indent + 1).AppendLine("Value:");
            builder.Indent(indent + 2).AppendLine(Value.ToString());
            return builder.ToString();
        }
    }
}
