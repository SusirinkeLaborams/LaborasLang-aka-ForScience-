using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.LexingTools;
using LaborasLangCompiler.Parser.Exceptions;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class DotOperatorNode
    {
        IExpressionNode builtNode;
        Parser parser;
        ClassNode cls;
        IContainerNode parent;

        private DotOperatorNode(Parser parser, IContainerNode parent)
        {
            this.parser = parser;
            this.parent = parent;
            this.cls = parent.GetClass();
        }

        public static DotOperatorNode Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new DotOperatorNode(parser, parent);
            foreach(var node in lexerNode.Children)
            {
                instance.Append(node);
            }
            return instance;
        }
        private void Append(AstNode lexerNode)
        {
            if(lexerNode.Token.Name == Lexer.FunctionCall)
            {
                AppendCall(lexerNode);
            }
            else
            {
                string name = parser.ValueOf(lexerNode);
                if (AppendLValue(name))
                    return;
                if (AppendMethod(name))
                    return;
                if (AppendType(name))
                    return;
                if (AppendNamespace(name))
                    return;
                throw new SymbolNotFoundException(String.Format("Symbol {0} not found", parser.ValueOf(lexerNode)));
            }
        }
        private bool AppendCall(AstNode lexerNode)
        {
            var args = new List<IExpressionNode>();
            for (int i = 1; i < lexerNode.Children.Count; i++)
            {
                args.Add(ExpressionNode.Parse(parser, parent, lexerNode.Children[i]));
            }
            var types = args.Select(arg => arg.ReturnType).ToList();
            if (AppendLValue(parser.ValueOf(lexerNode.Children[0])))
            {
                if (!builtNode.ReturnType.IsFunctorType())
                    return false;
                var nvm = new List<TypeReference>();
                var returnType = ILTools.ILHelpers.GetFunctorReturnTypeAndArguments(parser.Assembly, builtNode.ReturnType, out nvm);
                var method = AssemblyRegistry.GetMethods(parser.Assembly, builtNode.ReturnType, "Invoke").Single();
                if(ILHelpers.MatchesArgumentList(method, types))
                {
                    builtNode = new MethodCallNode(builtNode, returnType, args);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (AppendMethod(parser.ValueOf(lexerNode.Children[0])))
            {
                var node = (AmbiguousMethodNode)builtNode;
                var method = AmbiguousMethodNode.RemoveAmbiguity(parser, node, types);
                builtNode = new MethodCallNode(method, method.Function.ReturnType, args);
                return true;
            }
            return false;
        }
        private bool AppendMethod(string name)
        {
            if(builtNode == null)
            {
                return (builtNode = parent.GetSymbol(name)) != null;
            }
            else
            {
                if (builtNode is NamespaceNode)
                    return false;
                if(builtNode is LValueNode)
                {
                    //non-static methods
                    var methods = AssemblyRegistry.GetMethods(parser.Assembly, builtNode.ReturnType.FullName, name);
                    methods = methods.Where(m => !m.Resolve().IsStatic).ToList();
                    if (methods != null && methods.Count != 0)
                    {
                        builtNode = new AmbiguousMethodNode(methods, builtNode);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if(builtNode is TypeNode)
                {
                    //static methods
                    var methods = AssemblyRegistry.GetMethods(parser.Assembly, ((TypeNode)builtNode).ParsedType, name);
                    methods = methods.Where(m => m.Resolve().IsStatic).ToList();
                    if (methods != null && methods.Count != 0)
                    {
                        builtNode = new AmbiguousMethodNode(methods, null);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }
        private bool AppendType(string name)
        {
            if(builtNode == null)
            {
                var found = cls.FindType(name);
                return found != null;
            }
            else
            {
                if(builtNode is NamespaceNode)
                {
                    var found = cls.FindType(((NamespaceNode)builtNode).FullNamespace + "." + name);
                    if(found != null)
                    {
                        builtNode = found;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if(builtNode is TypeNode)
                {
                    var found = cls.FindType(((TypeNode)builtNode).ParsedType.FullName + "." + name);
                    if (found != null)
                    {
                        builtNode = found;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }
        private bool AppendNamespace(string name)
        {
            if (builtNode == null)
            {
                builtNode = cls.FindNamespace(name);
                return builtNode != null;
            }
            else
            {
                if(builtNode is NamespaceNode)
                {
                    var full = ((NamespaceNode)builtNode).FullNamespace + "." + name;
                    var found = cls.FindNamespace(full);
                    if(found != null)
                    {
                        builtNode = found;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        private bool AppendLValue(string name)
        {
            if(builtNode == null)
            {
                return (builtNode = parent.GetSymbol(name)) != null;
            }
            else
            {
                if(builtNode is NamespaceNode || builtNode is TypeNode)
                    return false;
                var field = AssemblyRegistry.GetField(parser.Assembly, builtNode.ReturnType.FullName, name);
                if(field != null)
                {
                    builtNode = new FieldNode(builtNode, field);
                    return true;
                }
                return false;
            }
        }
        private bool AppendExpression(AstNode lexerNode)
        {
            if (builtNode != null)
                return false;
            builtNode = ExpressionNode.Parse(parser, parent, lexerNode);
            return true;
        }
        public ExpressionNode ExtractExpression()
        {
            if (builtNode is NamespaceNode || builtNode is TypeNode)
                throw new ParseException("Expression expected");
            else
                return (ExpressionNode)builtNode;
        }
        public string ExtractNamespace()
        {
            if (!(builtNode is NamespaceNode))
                throw new ParseException("Namespace expected");
            else
                return ((NamespaceNode)builtNode).FullNamespace;
        }
        public TypeReference ExtractType()
        {
            if (!(builtNode is TypeNode))
                throw new ParseException("Type expected");
            else
                return ((TypeNode)builtNode).ParsedType;
        }
    }
}
