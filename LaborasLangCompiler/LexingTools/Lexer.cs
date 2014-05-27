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
        public const string PrefixNode = "PrefixNode";
        public const string SuffixNode = "SuffixNode";
        public const string SuffixOperator = "SuffixOperator";
        public const string PrefixOperator = "PrefixOperator";
        public const string ReturnSentence = "ReturnSentence";
        public const string BooleanLiteral = "BooleanLiteral";
        public const string Comparison = "Comparison";
        public const string FunctionTypeArgs = "FunctionTypeArgs";
        public const string BinaryOperationNode = "BinaryOperationNode";
        public const string BinaryOperator = "BinaryOperator";
        public const string BooleanOperator = "BooleanOperator";
        public const string BooleanNode = "BooleanNode";

        private AExpression GrammarTree;
        private static string Grammar = @"
 
                Ws: [\n\r\t\s]+;       
                (?<Symbol>): [a-zA-Z_]+ [a-zA-Z0-9_]*;
                Period: [.];
                (?<FullSymbol>): Symbol (Period Symbol)*;
                (?<NamespaceImport>): 'use' Ws FullSymbol;
                
                (?<IntegerLiteral>): [0-9]+;
                (?<StringLiteral>): ('\'' [^']* '\'') / ('""' [^""]* '""'); 
                (?<FloatLiteral>): [0-9]+ Period [0-9]+;
                (?<BooleanLiteral>): 'true' / 'false';
                (?<Literal>):  FloatLiteral / IntegerLiteral / StringLiteral / BooleanLiteral;
                
                (?<PrefixOperator>): '++' / '--' / '-' / '!' / '~';
                (?<SuffixOperator>): '++' / '--';
                (?<BinaryOperator>): '^' / (!'&&' '&') / (!'||' '|') / '>>' / '<<';
                (?<MultiplicationOperator>): '/' / '*'/ '%';
                (?<BooleanOperator>): '&&' / '||';
                (?<SumOperator>): '+' / '-';
                (?<RelationOperator>): '==' / '!=' / '<=' / '>=' / '<' / '>';

                (?<AssignmentOperator>): '+=' / '-=' / '*=' / '/=' / '%=' / '&=' / '|=' / '^=' / '<<=' / '>>=' / '=';    

                (?<PrefixNode>): (PrefixOperator Ws?)* ('(' Sum ')' / FunctionCall / Literal / FullSymbol);
                (?<SuffixNode>): PrefixNode (Ws? SuffixOperator)*;
                (?<BinaryOperationNode>): SuffixNode (Ws? BinaryOperator Ws? SuffixNode)*;
                (?<Product>): BinaryOperationNode (Ws? MultiplicationOperator Ws? BinaryOperationNode)*;
                (?<Sum>): Assignment / (Product (Ws? SumOperator  Ws? Product)* );
                (?<BooleanNode>): Sum (Ws? BooleanOperator  Ws? Sum)*;
                (?<Comparison>): BooleanNode (Ws? RelationOperator Ws? BooleanNode)*;
                (?<Value>): Comparison;            

                (?<UnaryOperator>): '!' / '++' / '--';           
                                
                (?<FunctionTypeArgs>): !('(' Ws? (Type (Ws? ',' Ws? Type)* Ws?)? ')' Ws? '{') ('(' Ws? (Type (Ws? ',' Ws? Type)* Ws?)? ')');
                (?<Type>): (FullSymbol (Ws? FunctionTypeArgs)+) / FullSymbol;
                
                (?<ReturnSentence>): 'return' (Ws (Function / Value / FullSymbol))?;
                (?<FunctionType>): Type;
                (?<FunctionArgument>): Value / FunctionCall;
                (?<FunctionCall>): FullSymbol Ws? 
                    (?<Arguments>
                    '('
                        Ws?
                        (FunctionArgument Ws? (',' Ws? FunctionArgument Ws?)*)?
                    ')')+;
                
                (?<FunctionArgumentDeclaration>): Type Ws Symbol;
                (?<NamedFunctionType>): Type Ws? ('(' Ws? (FunctionArgumentDeclaration Ws? (',' Ws? FunctionArgumentDeclaration Ws?)*)? ')');

                (?<Function>): NamedFunctionType Ws? CodeBlock;
                (?<Declaration>): (FunctionType / Type) Ws Symbol;
                (?<DeclarationAndAssignment>): (FunctionType / Type) Ws Symbol Ws? '=' Ws? (Function / Value);

                (?<Assignment>): FullSymbol Ws? AssignmentOperator Ws? (Function / Value);
            
                (?<ConditionalSentence>): 'if' Ws? '(' Ws? (?<Condition> (Value / FunctionCall)) Ws? ')' (?<TrueBlock> CodeBlock) 'else' (?<FalseBlock> CodeBlock) /
                                            'if' Ws? '(' Ws? (?<Condition> (Value / FunctionCall)) Ws? ')' (?<TrueBlock> CodeBlock);
                (?<Loop>):  'while' Ws? '(' Ws? (?<Condition> Value) Ws? ')' Ws? CodeBlock;
                (?<EndOfSentence>): ';';                
                (?<CodeBlock>): Ws? '{' Ws? (Sentence Ws?)* Ws? '}'  Ws? ;
                (?<Sentence>): ((ReturnSentence / NamespaceImport / DeclarationAndAssignment / Assignment / Declaration / FunctionCall / Value) Ws? EndOfSentence) /
                                Loop /
                                ConditionalSentence /
                                CodeBlock;
                
                (?<Root>): Ws? (Sentence Ws?)* Ws?;
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
