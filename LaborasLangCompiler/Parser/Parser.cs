using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPEG;
using NPEG.GrammarInterpreter;

namespace LaborasLangCompiler.Lexer
{
    class Parser
    {
        public static AstNode Parse(ByteInputIterator bytes)
        {
            String grammar = @"
 
                (?<Whitespace>): [\n\r\t\s]+;

                (?<Type>): [a-zA-Z]+ [a-zA-Z-0-9]*; // First character must be letter
                (?<Variable>): [a-zA-Z]+ [a-zA-Z-0-9]*; // First character must be letter
                (?<FunctionCall>): Variable '()';
                (?<Value>): FunctionCall / Variable;
                (?<Declaration>): Type Whitespace Variable;
                (?<Assignment>): Variable Whitespace? '=' Whitespace? Value;
                (?<ConditionalSentence>): 'if' Whitespace? '(' Whitespace? (?<Condition> Value) Whitespace? ')' Whitespace (Sentence/CodeBlock);
                (?<Sentence>): Declaration / Assignment / FunctionCall / ConditionalSentence;
                (?<CodeBlock>): Whitespace? '{' Whitespace? ((Sentence / CodeBlock) Whitespace?)* Whitespace? '}'  Whitespace? ;
                
                (?<Root>): Whitespace? ((CodeBlock / Sentence) Whitespace?)* Whitespace?;
            ".Trim();

            AExpression rules = PEGrammar.Load(grammar);
            var visitor = new NpegParserVisitor(bytes);
            rules.Accept(visitor);
            var isMatchIsAMethod = visitor.IsMatch;
            return visitor.AST;
        }

        
        public static void ParseSample()
        {

            var bytes = Encoding.UTF8.GetBytes(@"
                   
                type variable
{
                    type2 variable2
                    if(condition)
                    {
                        do stuff
                    }

                    if(anotherCondition() ) doMoreStuff()
                    variable = funciton()
                    {
                        AnothercodeBlock()
                    }
}");

            Parse(new ByteInputIterator(bytes));
        }
    }
}
