using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPEG;
using NPEG.GrammarInterpreter;

namespace LaborasLangCompiler.LexingTools
{
    static class Lexer
    {
        public static AstNode MakeTree(ByteInputIterator bytes)
        {
            String grammar = @"
 
                (?<Ws>): [\n\r\t\s]+;       
                (?<Symbol>): [a-zA-Z_]+ [a-zA-Z-0-9_]*;
                (?<Type>): Symbol;
                (?<Literal>): [0-9];
                (?<Value>): FunctionCall / Symbol / Literal;

                (?<FunctionArgument>): Value;
                (?<FunctionCall>): (?<FunctionName> Symbol) Ws? 
                    '('
                        Ws?
                        (FunctionArgument Ws? (',' Ws? FunctionArgument Ws?)*)?
                    ')';
                
                (?<Declaration>): Type Ws Symbol;
                (?<Assignment>): Symbol Ws? '=' Ws? Value;
                
                (?<ConditionalSentence>): 'if' Ws? '(' Ws? (?<Condition> Value) Ws? ')' Ws? CodeBlock;
                
                (?<EndOfSentence>): ';';                
                (?<Sentence>): (Declaration / Assignment / FunctionCall) EndOfSentence;
                
                (?<CodeBlock>): Ws? '{' Ws? ((Sentence / CodeBlock / ConditionalSentence) Ws?)* Ws? '}'  Ws? ;
                
                (?<Root>): Ws? ((CodeBlock / Sentence) Ws?)* Ws?;
            ".Trim();

            AExpression rules = PEGrammar.Load(grammar);
            var visitor = new NpegParserVisitor(bytes);
            rules.Accept(visitor);
            var isMatchIsAMethod = visitor.IsMatch;
            return visitor.AST;
        }
    }
}
