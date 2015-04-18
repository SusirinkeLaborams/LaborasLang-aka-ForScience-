using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Common;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class ForLoopNode : ParserNode, IForLoopNode
    {
        public override NodeType Type
        {
            get { throw new NotImplementedException(); }
        }
        public ICodeBlockNode InitializationBlock { get { return initializer; } }
        public IExpressionNode ConditionBlock { get { return condition; } }
        public ICodeBlockNode IncrementBlock { get { return increment; } }
        public ICodeBlockNode Body { get { return body; } }

        private CodeBlockNode initializer;
        private ExpressionNode condition;
        private CodeBlockNode increment;
        private CodeBlockNode body;

        private ForLoopNode(CodeBlockNode init, ExpressionNode condition, CodeBlockNode increment, CodeBlockNode body, SequencePoint point)
            :base(point)
        {
            this.initializer = init;
            this.condition = condition;
            this.increment = increment;
            this.body = body;
        }

        public static ParserNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            if(lexerNode.Children.Any(node => node.Type == TokenType.In))
            {
                throw new NotImplementedException("foreach not implemented yet");
            }

            CodeBlockNode init = null;
            ExpressionNode condition = null;
            CodeBlockNode increment = null;
            CodeBlockNode body = null;

            foreach(var node in lexerNode.Children)
            {
                switch(node.Type)
                {

                }
            }
        }

        private class ForBuilder
        {
            private bool initializerDone = false;
            private bool conditionDone = false;
            private bool incrementDone = false;

            private IAbstractSyntaxTree init;
            private IAbstractSyntaxTree condition;
            private IAbstractSyntaxTree increment;
            private IAbstractSyntaxTree body;

            private ForBuilder(IAbstractSyntaxTree lexerNode)
            {

                foreach(var node in lexerNode.Children)
                {
                    switch(node.Type)
                    {
                        case TokenType.For:
                        case TokenType.LeftParenthesis:
                            break;
                        case TokenType.DeclarationSubnode:
                            if (initializerDone)
                            {
                                ContractsHelper.AssumeUnreachable("Found declaration after for initializer was parsed");
                            }
                            init = node;
                            break;
                        case TokenType.Value:
                            if(!initializerDone)
                            {
                                init = node;
                            }
                            else if(!conditionDone)
                            {
                                condition = node;
                            }
                            else if(!incrementDone)
                            {
                                increment = node;
                            }
                            else if(body == null)
                            {
                                body = node;
                            }
                            else
                            {
                                ContractsHelper.AssumeUnreachable("Found node Value after all for nodes were parsed");
                            }
                            break;
                        case TokenType.RightParenthesis:
                            if(!initializerDone || !conditionDone)
                            {
                                ContractsHelper.AssertUnreachable("Right parenthesis found in for before condition and initializer were parsed");
                            }
                            if(incrementDone)
                            {
                                ContractsHelper.AssumeUnreachable("Right parenthesis found in for after increment was parsed");
                            }
                            incrementDone = true;
                            break;
                        case TokenType.EndOfLine:
                            if(!initializerDone)
                            {
                                initializerDone = true;
                            }
                            else if(!conditionDone)
                            {
                                conditionDone = true;
                            }
                            else if(!incrementDone)
                            {
                                ContractsHelper.AssumeUnreachable
                            }
                    }
                }
            }
        }

        //context should be init block if init exists
        public static ForLoopNode Create(ContextNode context, CodeBlockNode init, ExpressionNode condition, CodeBlockNode increment, CodeBlockNode body, SequencePoint point)
        {
            if (init == null)
            {
                init = CodeBlockNode.Create(context, point);
            }

            if (condition == null)
            {
                condition = LiteralNode.Create(context, true, point);
            }

            if (increment == null)
            {
                increment = CodeBlockNode.Create(context, point);
            }

            if(!(condition.IsGettable && condition.ExpressionReturnType.IsAssignableTo(context.Parser.Bool)))
            {
                ErrorCode.InvalidCondition.ReportAndThrow(point, "Condition must be a gettable boolean expression");
            }

            return new ForLoopNode(init, condition, increment, body, point);
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}
