using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexer.Containers;
using System.Diagnostics.Contracts;

namespace Lexer.PostProcessors
{
    class ArrayFunctionResolver : PostProcessor
    {
        private unsafe RootNode root;

        public unsafe ArrayFunctionResolver(RootNode root) : base(root)
        {
            this.root = root;
        }

        public override void Transform(RootNode RootNode, AstNode astNode)
        {
            if (astNode.Type == TokenType.ArrayLiteral)
            {
                var type = astNode.Children.First();
                if(type.Type == TokenType.Type) 
                {
                    if (type.Children.Last().Type == TokenType.ParameterList)
                    {
                        if(type.Children.Last().Children.Last().Type == TokenType.IndexNode)
                        {
                            return;
                        }
                    }

                    //Transform it to a function
                    astNode.Type = TokenType.FunctionType;
                    var initializerList = astNode.Children.Last();

                    Contract.Assume(initializerList.Type == TokenType.InitializerList);
                    initializerList.Type = TokenType.CodeBlockNode;
                }                
            }
        }
    }
}
