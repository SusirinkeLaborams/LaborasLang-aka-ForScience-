using LaborasLangCompiler.Common;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Parser.Utils;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class DotOperatorNode
    {
        private ExpressionNode builtNode;
        private readonly ContextNode context;
        private readonly Parser parser;

        private DotOperatorNode(ContextNode context)
        {
            this.context = context;
            this.parser = context.Parser;
        }

        public static ExpressionNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            var instance = new DotOperatorNode(context);
            foreach(var node in lexerNode.Children)
            {
                if(node.Type != Lexer.TokenType.Period)
                    instance.Append(ExpressionNode.Parse(context, node));
            }
            return instance.builtNode;
        }

        public static DotOperatorNode Create(ContextNode parent)
        {
            return new DotOperatorNode(parent);
        }

        public DotOperatorNode Append(ExpressionNode node)
        {
            if(builtNode == null)
            {
                AppendFirst(node);
                return this;
            }
            else
            {
                if (node.ExpressionType != ExpressionNodeType.ParserInternal)
                    ErrorCode.InvalidDot.Report(node.SequencePoint, "Expressions only allowed on left of dot operator");

                var symbol = node as SymbolNode;
                if (AppendField(symbol))
                    return this;
                if (AppendMethod(symbol))
                    return this;
                if (AppendType(symbol))
                    return this;
                if (AppendNamespace(symbol))
                    return this;
                if (AppendProperty(symbol))
                    return this;

                ErrorCode.SymbolNotFound.ReportAndThrow(node.SequencePoint, "Symbol {0} not found", symbol.Name);
                return null;//unreachable
            }
        }

        private void AppendFirst(ExpressionNode node)
        {
            if (node is SymbolNode)
            {
                SymbolNode symbol = node as SymbolNode;
                builtNode = context.GetSymbol(symbol.Name, context, node.SequencePoint);
                if(builtNode == null)
                    ErrorCode.SymbolNotFound.ReportAndThrow(symbol.SequencePoint, "Symbol {0} not found", symbol.Name);
            }
            else
            {
                builtNode = node;
            }
        }

        private bool AppendMethod(SymbolNode node)
        {
            if (builtNode is NamespaceNode)
                return false;

            if (builtNode is TypeNode)
            {
                //static methods

                IEnumerable<MethodReference> methods = AssemblyRegistry.GetMethods(parser.Assembly, ((TypeNode)builtNode).ParsedType, node.Name);
                methods = methods.Where(m => m.IsStatic());
                if (methods.Count() != 0)
                {
                    builtNode = AmbiguousMethodNode.Create(methods, context, null, builtNode.SequencePoint);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //non-static methods
                if (!builtNode.IsGettable)
                    return false;
                IEnumerable<MethodReference> methods = AssemblyRegistry.GetMethods(parser.Assembly, builtNode.ExpressionReturnType, node.Name);
                methods = methods.Where(m => !m.IsStatic());
                if (methods.Count() != 0)
                {
                    builtNode = AmbiguousMethodNode.Create(methods, context, builtNode, builtNode.SequencePoint);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool AppendType(SymbolNode node)
        {
            TypeReference type = null;
            if (builtNode is NamespaceNode)
            {
                type = ((NamespaceNode)builtNode).Namespace.GetContainedType(node.Name);
            }
            if (builtNode is TypeNode)
            {

                type = ((TypeNode)builtNode).ParsedType.GetNestedType(parser.Assembly, node.Name);
            }

            if (type != null)
            {
                builtNode = TypeNode.Create(type, context, node.SequencePoint);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AppendNamespace(SymbolNode node)
        {
            Namespace found = null;
            if (builtNode is NamespaceNode)
            {
                found = ((NamespaceNode)builtNode).Namespace.GetContainedNamespace(node.Name);
            }

            if (found != null)
            {
                builtNode = new NamespaceNode(found, node.SequencePoint);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AppendField(SymbolNode node)
        {
            FieldReference field = null;

            if (builtNode is TypeNode)
            {
                field = AssemblyRegistry.GetField(parser.Assembly, ((TypeNode)builtNode).ParsedType, node.Name);
                if (field != null && !field.IsStatic())
                    field = null;
            }
            else if (builtNode.ExpressionType != ExpressionNodeType.ParserInternal)
            {

                field = AssemblyRegistry.GetField(parser.Assembly, builtNode.ExpressionReturnType, node.Name);
                if (field != null && field.IsStatic() || !builtNode.IsGettable)
                    field = null;
            }

            if (field != null)
            {
                builtNode = new FieldNode(field.IsStatic() ? null : builtNode, field, context, builtNode.SequencePoint);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AppendProperty(SymbolNode node)
        {
            PropertyReference property = null;

            if(builtNode is TypeNode)
            {
                property = AssemblyRegistry.GetProperty(((TypeNode)builtNode).ParsedType, node.Name);
                if (property != null && !property.IsStatic())
                    property = null;
            }
            else if(builtNode.ExpressionType != ExpressionNodeType.ParserInternal)
            {
                property = AssemblyRegistry.GetProperty(builtNode.ExpressionReturnType, node.Name);
                if (property != null && property.IsStatic() || !builtNode.IsGettable)
                    property = null;
            }

            if (property != null)
            {
                builtNode = new PropertyNode(property.IsStatic() ? null : builtNode, property, context, builtNode.SequencePoint);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
