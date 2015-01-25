using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Impl.Wrappers;
using Lexer.Containers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class DotOperatorNode
    {
        private ExpressionNode builtNode;
        private Parser parser;
        private ClassNode cls;
        private Context parent;

        private DotOperatorNode(Parser parser, Context parent)
        {
            this.parser = parser;
            this.parent = parent;
            this.cls = parent.GetClass();
        }

        public static ExpressionNode Parse(Parser parser, Context parent, AstNode lexerNode)
        {
            var instance = new DotOperatorNode(parser, parent);
            foreach(var node in lexerNode.Children)
            {
                if(node.Type != Lexer.TokenType.Period)
                    instance.Append(ExpressionNode.Parse(parser, parent, node));
            }
            return instance.builtNode;
        }

        private void Append(ExpressionNode node)
        {
            if(builtNode == null)
            {
                AppendFirst(node);
            }
            else
            {
                if(node.ExpressionType != ExpressionNodeType.ParserInternal)
                    throw new ParseException(node.SequencePoint, "Expressions only allowed on left of dot operator");

                var symbol = node as SymbolNode;
                if (AppendLValue(symbol))
                    return;
                if (AppendMethod(symbol))
                    return;
                if (AppendType(symbol))
                    return;
                if (AppendNamespace(symbol))
                    return;
                throw new SymbolNotFoundException(node.SequencePoint, "Symbol {0} not found", symbol.Name);
            }
        }

        private bool AppendFirst(ExpressionNode node)
        {
            if (node.ExpressionType != ExpressionNodeType.ParserInternal)
            {
                //non-symbol expressions
                builtNode = node;
                return true;
            }
            else if (node is SymbolNode)
            {
                SymbolNode symbol = node as SymbolNode;
                builtNode = parent.GetSymbol(symbol.Name, parent, node.SequencePoint);
                return builtNode != null;
            }
            else
            {
                throw new ParseException(node.SequencePoint, "Unexpected node type {0} in dot operator", node.ExpressionType);
            }
        }

        private bool AppendMethod(SymbolNode node)
        {
            if (builtNode is NamespaceNode)
                return false;

            if (builtNode is TypeNode)
            {
                //static methods
                var methods = ((TypeNode)builtNode).ParsedType.GetMethods(node.Name);
                methods = methods.Where(m => m.IsStatic);
                if (methods.Count() != 0)
                {
                    builtNode = AmbiguousMethodNode.Create(methods, parent, null, builtNode.SequencePoint);
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
                var methods = builtNode.TypeWrapper.GetMethods(node.Name);
                methods = methods.Where(m => !m.IsStatic);
                if (methods.Count() != 0)
                {
                    builtNode = AmbiguousMethodNode.Create(methods, parent, builtNode, builtNode.SequencePoint);
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
            TypeWrapper type = null;
            if (builtNode is NamespaceNode)
            {
                type = ((NamespaceNode)builtNode).Namespace.GetContainedType(node.Name);
            }
            if (builtNode is TypeNode)
            {
                type = ((TypeNode)builtNode).ParsedType.GetContainedType(node.Name);
            }

            if (type != null)
            {
                builtNode = new TypeNode(parser, type, parent, node.SequencePoint);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AppendNamespace(SymbolNode node)
        {
            NamespaceWrapper found = null;
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

        private bool AppendLValue(SymbolNode node)
        {
            FieldWrapper field = null;

            if (builtNode is TypeNode)
            {
                field = ((TypeNode)builtNode).ParsedType.GetField(node.Name);
                if (field != null && !field.IsStatic)
                    field = null;
            }
            else if (builtNode.ExpressionType != ExpressionNodeType.ParserInternal)
            {
                field = builtNode.TypeWrapper.GetField(node.Name);
                if (field != null && field.IsStatic || !builtNode.IsGettable)
                    field = null;
            }

            if (field != null)
            {
                builtNode = new FieldNode(field.IsStatic ? null : builtNode, field, parent, builtNode.SequencePoint);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
