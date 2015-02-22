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

        private readonly TypeReference type;

        private LiteralNode(Literal value, TypeReference type, SequencePoint point)
            : base(point)
        {
            this.type = type;
            this.Value = value;
        }

        public static LiteralNode Create(ContextNode context, IConvertible value, SequencePoint point)
        {
            TypeReference type;

            if (!MetadataHelpers.TryGetBuiltInTypeReference(context.Parser.Assembly, value.GetType(), out type))
                Errors.ReportAndThrow(ErrorCode.TypeMissmatch, point, "Cannot create literal of type {0} with value {1}", type, value);

            return new LiteralNode(new Literal(value), type, point);
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
                switch (type.MetadataType)
                {
                    case MetadataType.String:
                        return value;

                    case MetadataType.Boolean:
                        return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                        
                    case MetadataType.SByte:
                        return Convert.ToSByte(value, CultureInfo.InvariantCulture);

                    case MetadataType.Byte:
                        return Convert.ToByte(value, CultureInfo.InvariantCulture);

                    case MetadataType.Char:
                        return Convert.ToChar(value, CultureInfo.InvariantCulture);

                    case MetadataType.Int16:
                        return Convert.ToInt16(value, CultureInfo.InvariantCulture);

                    case MetadataType.UInt16:
                        return Convert.ToUInt16(value, CultureInfo.InvariantCulture);

                    case MetadataType.Int32:
                        return Convert.ToInt32(value, CultureInfo.InvariantCulture);

                    case MetadataType.UInt32:
                        return Convert.ToUInt32(value, CultureInfo.InvariantCulture);

                    case MetadataType.Int64:
                        return Convert.ToInt64(value, CultureInfo.InvariantCulture);

                    case MetadataType.UInt64:
                        return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                        
                    case MetadataType.Single:
                        return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                        
                    case MetadataType.Double:
                        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        
                    default:
                        if (type.FullName == "System.Decimal")
                            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);

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
