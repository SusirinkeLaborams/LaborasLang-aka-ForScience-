﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPEG;
using NPEG.GrammarInterpreter;

namespace LaborasLangCompiler.LexingTools
{
    public class Lexer
    {
        private AExpression GrammarTree;
        private static String Grammar = @"
 
                Ws: [\n\r\t\s]+;       
                (?<Symbol>): [a-zA-Z_]+ [a-zA-Z0-9_]*;
                Period: [.];
                (?<FullSymbol>): Symbol (Period Symbol)*;
                (?<NamespaceImport>): 'use' Ws FullSymbol;
                
                (?<IntegerLiteral>): [0-9]+;
                (?<StringLiteral>): '\'' [^']* '\''; 
                (?<FloatLiteral>): [0-9]+ Period [0-9]+;
                (?<Literal>):  FloatLiteral / IntegerLiteral / StringLiteral;
    
                Factor: '(' Sum ')' / FunctionCall / Symbol / Literal;
                (?<Product>): Factor (Ws? (?<MultiplicationOperator> '/' / '*')  Ws? Factor)*;
                (?<Sum>): Product (Ws? (?<SumOperator> '+' / '-')  Ws? Product)*;

                (?<AssignmentOperator>): '+=' / '-=' / '*=' / '/=' / '%=' / '&=' / '|=' / '^=' / '<<=' / '>>=' / '=';                
                (?<RelationOperator>): '==' / '!=' / '<=' / '>=' / '<' / '>';
                (?<ShiftOperator>): '>>' / '<<';     
                (?<UnaryOperator>): '!' / '++' / '--';           
                
                (?<Type>): Symbol;
                
                (?<Value>):  Sum;
                
                (?<FunctionType>): Type Ws? (?<ArgumentTypes> '(' Ws? (Type Ws? (',' Ws? Type Ws?)*)? ')');
                (?<FunctionArgument>): Value;
                (?<FunctionCall>): (FullSymbol) Ws? 
                    '('
                        Ws?
                        (FunctionArgument Ws? (',' Ws? FunctionArgument Ws?)*)?
                    ')';
                
                (?<NamedFunctionType>): (Type Ws? ('(' Ws? ((Type Ws Symbol) Ws? (',' Ws? (Type Ws Symbol) Ws?)*)? ')'));
                (?<Function>): NamedFunctionType Ws? CodeBlock;
                (?<Declaration>): (FunctionType / Type) Ws Symbol;
                (?<DeclarationAndAssignment>): (FunctionType / Type) Ws Symbol Ws? '=' Ws? (Function / Value);

                (?<Assignment>): Symbol Ws? '=' Ws? Function /
                                 Symbol Ws? '=' Ws? Value /
                                 Symbol Ws? AssignmentOperator Ws? Value;
            
                (?<ConditionalSentence>): 'if' Ws? '(' Ws? (?<Condition> Value) Ws? ')' (?<TrueBlock> CodeBlock) 'else' (?<FalseBlock> CodeBlock) /
                                            'if' Ws? '(' Ws? (?<Condition> Value) Ws? ')' (?<TrueBlock> CodeBlock);
                (?<Loop>):  'while' Ws? '(' Ws? (?<Condition> Value) Ws? ')' Ws? CodeBlock;
                (?<EndOfSentence>): ';';                
                (?<Sentence>): ((NamespaceImport / DeclarationAndAssignment / Assignment / Declaration / FunctionCall) Ws? EndOfSentence) /
                                Loop /
                                ConditionalSentence;
                
                (?<CodeBlock>): Ws? '{' Ws? ((Sentence / CodeBlock) Ws?)* Ws? '}'  Ws? ;
                
                (?<Root>): Ws? (( CodeBlock / Sentence) Ws?)* Ws?;
            ".Trim();

        public Lexer()
        {
            GrammarTree = PEGrammar.Load(Grammar);
        }

        public AstNode MakeTree(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            ByteInputIterator inputIterator = new ByteInputIterator(bytes);
            return MakeTree(inputIterator);
        }

        public AstNode MakeTree(ByteInputIterator inputIterator)
        {
            var visitor = new NpegParserVisitor(inputIterator);
            GrammarTree.Accept(visitor);
            var isMatch = visitor.IsMatch;
            return visitor.AST;
        }

    }
}
