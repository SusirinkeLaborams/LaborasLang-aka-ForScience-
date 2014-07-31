using LaborasLangCompiler.ILTools;
using Mono.Cecil;
using NPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Impl
{
    class FunctionHeader
    {
        public TypeReference ReturnType { get; private set; }
        public IReadOnlyList<FunctionArgumentNode> Args { get; private set; }
        public TypeReference FunctorReturnType { get; private set; }
        public static FunctionHeader Parse(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var instance = new FunctionHeader();
            instance.FunctorReturnType = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            List<FunctionArgumentNode> args = new List<FunctionArgumentNode>();
            List<TypeReference> types = new List<TypeReference>();
            for(int i = 1; i < lexerNode.Children.Count; i++)
            {
                var arg = ParseArgument(parser, parent, lexerNode.Children[i]); 
                args.Add(arg);
                types.Add(arg.ReturnType);
            }
            instance.Args = args;
            instance.ReturnType = AssemblyRegistry.GetFunctorType(parser.Assembly, instance.FunctorReturnType, types);
            return instance;
        }
        public static FunctionArgumentNode ParseArgument(Parser parser, IContainerNode parent, AstNode lexerNode)
        {
            var type = TypeNode.Parse(parser, parent, lexerNode.Children[0]);
            var name = parser.ValueOf(lexerNode.Children[1]);
            return new FunctionArgumentNode(new ParameterDefinition(name, ParameterAttributes.None, type), true, parser.GetSequencePoint(lexerNode));
        }
    }
}
