using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class LiteralNode : RValueNode, ILiteralNode
    {
        public override RValueNodeType RValueType { get { return RValueNodeType.Literal; } }
        public dynamic Value { get; private set; }
        public override TypeReference ReturnType { get; set; }
        private LiteralNode(dynamic value, TypeReference type)
        {
            ReturnType = type;
            Value = value;
        }
        public static new LiteralNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            lexerNode = lexerNode.Children[0];
            string type = lexerNode.Token.Name;
            string value = parser.GetNodeValue(lexerNode);
            try
            {
                switch (type)
                {
                    case "StringLiteral":
                        return new LiteralNode(value, parser.Primitives["string"]);
                    case "IntegerLiteral":
                        return new LiteralNode(Convert.ToInt32(value), parser.Primitives["int"]);
                    case "FloatLiteral":
                        return new LiteralNode(Convert.ToSingle(value, CultureInfo.InvariantCulture.NumberFormat), parser.Primitives["float"]);
                    default:
                        throw new ParseException("Literal expected, " + type + " received");
                }
            }
            catch(FormatException e)
            {
                throw new ParseException("Could not parse " + value + " as an " + type, e);
            }
            catch(OverflowException e)
            {
                throw new ParseException("Could not fit " + value + " in " + type, e);
            }
        }
        public override bool Equals(ParserNode obj)
        {
            if (!(obj is LiteralNode))
                return false;
            var that = (LiteralNode)obj;
            return base.Equals(obj) && Value == that.Value;
        }
    }
}
