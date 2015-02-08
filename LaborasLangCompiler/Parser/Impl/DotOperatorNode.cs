﻿using LaborasLangCompiler.Common;
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
        private Context parent;

        private DotOperatorNode(Parser parser, Context parent)
        {
            this.parser = parser;
            this.parent = parent;
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
                if (node.ExpressionType != ExpressionNodeType.ParserInternal)
                    Utils.Report(ErrorCode.InvalidDot, node.SequencePoint, "Expressions only allowed on left of dot operator");

                var symbol = node as SymbolNode;
                if (AppendField(symbol))
                    return;
                if (AppendMethod(symbol))
                    return;
                if (AppendType(symbol))
                    return;
                if (AppendNamespace(symbol))
                    return;
                if (AppendProperty(symbol))
                    return;
                Utils.Report(ErrorCode.SymbolNotFound, node.SequencePoint, "Symbol {0} not found", symbol.Name);
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
                Utils.Report(ErrorCode.InvalidDot, node.SequencePoint, "Unexpected node type {0}", node.ExpressionType);
                return false;//unreachable
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
                    builtNode = AmbiguousMethodNode.Create(parser, methods, parent, null, builtNode.SequencePoint);
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
                    builtNode = AmbiguousMethodNode.Create(parser, methods, parent, builtNode, builtNode.SequencePoint);
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
                builtNode = new TypeNode(type, parent, node.SequencePoint);
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
                builtNode = new FieldNode(field.IsStatic() ? null : builtNode, field, parent, builtNode.SequencePoint);
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
                property = AssemblyRegistry.GetProperty(parser.Assembly, ((TypeNode)builtNode).ParsedType, node.Name);
                if (property != null && !property.IsStatic())
                    property = null;
            }
            else if(builtNode.ExpressionType != ExpressionNodeType.ParserInternal)
            {
                property = AssemblyRegistry.GetProperty(parser.Assembly, builtNode.ExpressionReturnType, node.Name);
                if (property != null && property.IsStatic() || !builtNode.IsGettable)
                    property = null;
            }

            if (property != null)
            {
                builtNode = new PropertyNode(property.IsStatic() ? null : builtNode, property, parent, builtNode.SequencePoint);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
