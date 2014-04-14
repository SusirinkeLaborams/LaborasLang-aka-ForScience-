using LaborasLangCompiler.ILTools;
using LaborasLangCompiler.Parser.Exceptions;
using LaborasLangCompiler.Parser.Tree;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class ClassNode : ParserNode
    {
        public override Tree.NodeType Type { get { return Tree.NodeType.ClassNode; } }
        private Dictionary<string, FieldDeclarationNode> fields;
        private List<IFunctionNode> methods;
        private ClassNode parent;
        private TypeEmitter typeEmitter;
        private ClassNode(Parser parser, ClassNode parent)
        {
            this.parent = parent;
            methods = new List<IFunctionNode>();
            fields = new Dictionary<string, FieldDeclarationNode>();
            typeEmitter = new TypeEmitter(parser.Assembly, parser.Filename);
        }
        private void AddField(string name, TypeReference type)
        {
            fields.Add(name, new FieldDeclarationNode(name, type));
        }
        public FieldNode GetField(string name)
        {
            if (fields.ContainsKey(name))
                return fields[name];

            if (parent != null)
                return parent.GetField(name);

            return null;
        }
        public static new ClassNode Parse(Parser parser, ClassNode parentClass, CodeBlockNode parentBlock, AstNode lexerNode)
        {
            var instance = new ClassNode(parser, parentClass);
            AstNode sentence;

            if (parser.Root == null)
                parser.Root = instance;

            if (parentBlock != null)
                throw new ParseException("WhatIsThisIDontEven: Class defined inside a code block");

            //declarations
            foreach (var node in lexerNode.Children)
            {
                if (node.Token.Name == "Sentence")
                {
                    sentence = node.Children[0];
                    switch (sentence.Token.Name)
                    {
                        case "NamespaceImport":
                            throw new NotImplementedException();
                        case "Declaration":
                            ParseDeclaration(parser, instance, sentence, false);
                            break;
                        case "DeclarationAndAssignment":
                            ParseDeclaration(parser, instance, sentence, true);
                            break;
                        default:
                            throw new ParseException("Import or declaration expected " + sentence.Token.Name + " received");
                    }
                }
                else
                {
                    throw new ParseException("Node Sentence expected, " + node.Token.Name + " received");
                }
            }

            //init
            FieldDeclarationNode field;
            foreach (var node in lexerNode.Children)
            {
                sentence = node.Children[0];
                switch (sentence.Token.Name)
                {
                    case "DeclarationAndAssignment":
                        ExpressionNode init = ExpressionNode.Parse(parser, instance, null, sentence.Children[2]);
                        field = instance.fields[parser.GetNodeValue(sentence.Children[1])];
                        field.Initializer = init;
                        if (field.ReturnType == null)
                        {
                            field.ReturnType = init.ReturnType;
                        }
                        else
                        {
                            if (!Parser.CompareTypes(field.ReturnType, init.ReturnType))
                                throw new TypeException("Type mismatch, field " + field.Name + " type " + field.ReturnType.FullName + " initialized with " + init.ReturnType.FullName);
                        }
                        goto case "Declaration";
                    case "Declaration":
                        field = instance.fields[parser.GetNodeValue(sentence.Children[1])];
                        field.CreateFieldDefinition(FieldAttributes.Static | FieldAttributes.Private);
                        instance.typeEmitter.AddField(field.Field);
                        //TODO: Add init
                        break;
                    default:
                        break;
                }
            }
            return instance;
        }

        private static void ParseDeclaration(Parser parser, ClassNode klass, AstNode lexerNode, bool init)
        {
            var declaredType = parser.ParseType(lexerNode.Children[0]);
            var name = parser.GetNodeValue(lexerNode.Children[1]);
            if (declaredType == null && !init)
                throw new TypeException("Type inference requires initialization");
            else
                klass.AddField(name, declaredType);
        }

        public override bool Equals(ParserNode obj)
        {
            if (!(obj is ClassNode))
                return false;

            var that = (ClassNode)obj;

            if (!(base.Equals(obj) && fields.SequenceEqual(that.fields) && methods.SequenceEqual(that.methods)))
                return false;

            if (parent != null && that.parent != null)
            {
                if (!parent.Equals(that.parent))
                    return false;
            }
            else
            {
                if (parent != null || that.parent != null)
                    return false;
            }
            return true;
        }
        public override string Print()
        {
            string delim = "";
            StringBuilder builder = new StringBuilder("(ClassNode: Fields: ");
            foreach(var field in fields)
            {
                builder.Append(String.Format("{0}{1} {2}", delim, field.Value.ReturnType.FullName, field.Key));
                if (field.Value.Initializer != null)
                    builder.Append(" = ").Append(field.Value.Initializer.Print());
                delim = ", ";
            }
            builder.Append(" Methods: ");
            delim = "";
            foreach(var method in methods)
            {
                builder.Append(String.Format("{0}{1}", delim, method.ReturnType.FullName));
                delim = ", ";
            }
            
            return builder.Append(")").ToString();
        }
    }
}
