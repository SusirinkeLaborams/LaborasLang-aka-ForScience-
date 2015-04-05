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
        public unsafe ArrayFunctionResolver()
        {

        }

        public override void Transform(AbstractSyntaxTree astNode)
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
                    astNode.Type = TokenType.Function;
                    var initializerList = astNode.Children.Last();

                    Contract.Assume(initializerList.Type == TokenType.InitializerList);
                    initializerList.Type = TokenType.CodeBlockNode;
                }                
            }
        }
    }
}
