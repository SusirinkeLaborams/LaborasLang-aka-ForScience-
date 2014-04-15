using System;
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
        public const string Symbol = "Symbol";
        public const string FullSymbol = "FullSymbol";
        public const string NamespaceImport = "NamespaceImport";
        public const string IntegerLiteral = "IntegerLiteral";
        public const string StringLiteral = "StringLiteral";
        public const string FloatLiteral = "FloatLiteral";
        public const string Literal = "Literal";
        public const string Product = "Product";
        public const string MultiplicationOperator = "MultiplicationOperator";
        public const string Sum = "Sum";
        public const string SumOperator = "SumOperator";
        public const string AssignmentOperator = "AssignmentOperator";
        public const string RelationOperator = "RelationOperator";
        public const string ShiftOperator = "ShiftOperator";
        public const string UnaryOperator = "UnaryOperator";
        public const string Type = "Type";
        public const string Value = "Value";
        public const string FunctionType = "FunctionType";
        public const string ArgumentTypes = "ArgumentTypes";
        public const string FunctionArgument = "FunctionArgument";
        public const string FunctionCall = "FunctionCall";
        public const string FunctionArgumentDeclaration = "FunctionArgumentDeclaration";
        public const string NamedFunctionType = "NamedFunctionType";
        public const string Function = "Function";
        public const string Declaration = "Declaration";
        public const string DeclarationAndAssignment = "DeclarationAndAssignment";
        public const string Assignment = "Assignment";
        public const string ConditionalSentence = "ConditionalSentence";
        public const string Condition = "Condition";
        public const string TrueBlock = "TrueBlock";
        public const string FalseBlock = "FalseBlock";
        public const string Loop = "Loop";
        public const string EndOfSentence = "EndOfSentence";
        public const string Sentence = "Sentence";
        public const string CodeBlock = "CodeBlock";
        public const string Root = "Root";

        private AExpression GrammarTree;
        private static string Grammar = @"
 
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
                (?<FunctionArgumentDeclaration>): Type Ws Symbol;
                (?<NamedFunctionType>): (Type Ws? ('(' Ws? (FunctionArgumentDeclaration Ws? (',' Ws? FunctionArgumentDeclaration Ws?)*)? ')'));
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
