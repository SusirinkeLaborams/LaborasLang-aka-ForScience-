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
            string grammar = @"
 
                (?<Ws>): [\n\r\t\s]+;       
                (?<Symbol>): [a-zA-Z_]+ [a-zA-Z0-9_]*;
                (?<Period>): [.];
                (?<FullSymbol>): Symbol (Period Symbol)*;
                (?<NamespaceImport>): 'use' Ws FullSymbol;
                
                (?<Literal>): [0-9];
                (?<AssignmentOperator>): '+=' / '-=' / '*=' / '/=' / '%=' / '&=' / '|=' / '^=' / '<<=' / '>>=' / '=';                
                (?<RelationOperator>): '==' / '!=' / '<=' / '>=' / '<' / '>';
                (?<ShiftOperator>): '>>' / '<<';     
                (?<UnaryOperator>): '!' / '++' / '--';           
                
                (?<Type>): Symbol;
                (?<Value>): FunctionCall / Symbol / Literal;
                
                (?<FunctionType>): Type Ws? (?<ArgumentTypes> '(' Ws? (Type Ws? (',' Ws? Type Ws?)*)? ')');
                (?<FunctionArgument>): Value;
                (?<FunctionCall>): (Function) Ws? 
                    '('
                        Ws?
                        (FunctionArgument Ws? (',' Ws? FunctionArgument Ws?)*)?
                    ')';
                (?<NamedFunctionType>): Type Ws? ('(' Ws? ((Type Ws Symbol) Ws? (',' Ws? (Type Ws Symbol) Ws?)*)? ')');
                (?<Function>): NamedFunctionType Ws? CodeBlock / Symbol;
                (?<Declaration>): (FunctionType / Type) Ws Symbol;


                (?<Assignment>): (Declaration / Symbol) Ws? '=' Ws? Value /
                                 Symbol Ws? AssignmentOperator Ws? Value;
                (?<FunctionAssignment>): Declaration Ws? '=' Ws? Function;
                (?<ConditionalSentence>): 'if' Ws? '(' Ws? (?<Condition> Value) Ws? ')' (?<TrueBlock> CodeBlock) 'else' (?<FalseBlock> CodeBlock) /
                                            'if' Ws? '(' Ws? (?<Condition> Value) Ws? ')' (?<TrueBlock> CodeBlock);
                (?<Loop>):  'while' Ws? '(' Ws? (?<Condition> Value) Ws? ')' Ws? CodeBlock;
                (?<EndOfSentence>): ';';                
                (?<Sentence>): ((NamespaceImport / Assignment / Declaration / FunctionCall) EndOfSentence) /
                                FunctionAssignment;
                
                (?<CodeBlock>): Ws? '{' Ws? ((Sentence / CodeBlock / ConditionalSentence / Loop) Ws?)* Ws? '}'  Ws? ;
                
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
