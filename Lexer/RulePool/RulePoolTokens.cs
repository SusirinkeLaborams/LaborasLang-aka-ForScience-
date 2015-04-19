using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
    partial class RulePool
    {
        private static Condition CastOperator { get { return TokenType.CastOperator; } }
        private static Condition ForLoop { get { return TokenType.ForLoop; } }
        private static Condition For { get { return TokenType.For; } }
        private static Condition In { get { return TokenType.In; } }
        private static Condition InfixNode { get { return TokenType.InfixNode; } }
        private static Condition InfixSubnode { get { return TokenType.InfixSubnode; } }
        private static Condition InfixOperator { get { return TokenType.InfixOperator; } }
        private static Condition EndOfLine { get { return TokenType.EndOfLine; } }
        private static Condition Comma { get { return TokenType.Comma; } }
        private static Condition Period { get { return TokenType.Period; } }
        private static Condition BitwiseAnd { get { return TokenType.BitwiseAnd; } }
        private static Condition BitwiseAndEqual { get { return TokenType.BitwiseAndEqual; } }
        private static Condition Plus { get { return TokenType.Plus; } }
        private static Condition PlusPlus { get { return TokenType.PlusPlus; } }
        private static Condition Minus { get { return TokenType.Minus; } }
        private static Condition MinusMinus { get { return TokenType.MinusMinus; } }
        private static Condition MinusEqual { get { return TokenType.MinusEqual; } }
        private static Condition NotEqual { get { return TokenType.NotEqual; } }
        private static Condition Not { get { return TokenType.Not; } }
        private static Condition PlusEqual { get { return TokenType.PlusEqual; } }
        private static Condition StringLiteral { get { return TokenType.StringLiteral; } }
        private static Condition BitwiseComplement { get { return TokenType.BitwiseComplement; } }
        private static Condition BitwiseXor { get { return TokenType.BitwiseXor; } }
        private static Condition BitwiseXorEqual { get { return TokenType.BitwiseXorEqual; } }
        private static Condition BitwiseOr { get { return TokenType.BitwiseOr; } }
        private static Condition BitwiseOrEqual { get { return TokenType.BitwiseOrEqual; } }
        private static Condition LeftShiftEqual { get { return TokenType.LeftShiftEqual; } }
        private static Condition LeftShift { get { return TokenType.LeftShift; } }
        private static Condition LessOrEqual { get { return TokenType.LessOrEqual; } }
        private static Condition Less { get { return TokenType.Less; } }
        private static Condition LogicalAnd { get { return TokenType.LogicalAnd; } }
        private static Condition LogicalAndEqual { get { return TokenType.LogicalAndEqual; } }
        private static Condition LogicalOr { get { return TokenType.LogicalOr; } }
        private static Condition LogicalOrEqual { get { return TokenType.LogicalOrEqual; } }
        private static Condition More { get { return TokenType.More; } }
        private static Condition RightShift { get { return TokenType.RightShift; } }
        private static Condition RightShiftEqual { get { return TokenType.RightShiftEqual; } }
        private static Condition MoreOrEqual { get { return TokenType.MoreOrEqual; } }
        private static Condition Divide { get { return TokenType.Divide; } }
        private static Condition DivideEqual { get { return TokenType.DivideEqual; } }
        private static Condition Multiply { get { return TokenType.Multiply; } }
        private static Condition MultiplyEqual { get { return TokenType.MultiplyEqual; } }
        private static Condition Remainder { get { return TokenType.Remainder; } }
        private static Condition RemainderEqual { get { return TokenType.RemainderEqual; } }
        private static Condition Assignment { get { return TokenType.Assignment; } }
        private static Condition Equal { get { return TokenType.Equal; } }
        private static Condition LeftCurlyBrace { get { return TokenType.LeftCurlyBrace; } }
        private static Condition RightCurlyBrace { get { return TokenType.RightCurlyBrace; } }
        private static Condition LeftParenthesis { get { return TokenType.LeftParenthesis; } }
        private static Condition RightParenthesis { get { return TokenType.RightParenthesis; } }
        private static Condition Integer { get { return TokenType.Integer; } }
        private static Condition Float { get { return TokenType.Float; } }
        private static Condition Long { get { return TokenType.Long; } }
        private static Condition Double { get { return TokenType.Double; } }
        private static Condition Symbol { get { return TokenType.Symbol; } }
        private static Condition Const { get { return TokenType.Const; } }
        private static Condition Else { get { return TokenType.Else; } }
        private static Condition False { get { return TokenType.False; } }
        private static Condition If { get { return TokenType.If; } }
        private static Condition Internal { get { return TokenType.Internal; } }
        private static Condition Protected { get { return TokenType.Protected; } }
        private static Condition Return { get { return TokenType.Return; } }
        private static Condition True { get { return TokenType.True; } }
        private static Condition Use { get { return TokenType.Use; } }
        private static Condition Virtual { get { return TokenType.Virtual; } }
        private static Condition CodeConstruct { get { return TokenType.CodeConstruct; } }
        private static Condition While { get { return TokenType.While; } }
        private static Condition NoInstance { get { return TokenType.NoInstance; } }
        private static Condition Private { get { return TokenType.Private; } }
        private static Condition Public { get { return TokenType.Public; } }
        private static Condition NonTerminalToken { get { return TokenType.NonTerminalToken; } }
        private static Condition StatementWithEndOfLine { get { return TokenType.StatementWithEndOfLine; } }
        private static Condition StatementNode { get { return TokenType.StatementNode; } }
        private static Condition CodeBlockNode { get { return TokenType.CodeBlockNode; } }
        private static Condition DeclarationNode { get { return TokenType.DeclarationNode; } }
        private static Condition UseNode { get { return TokenType.UseNode; } }
        private static Condition ReturnNode { get { return TokenType.ReturnNode; } }
        private static Condition RootNode { get { return TokenType.RootNode; } }
        private static Condition FullSymbol { get { return TokenType.FullSymbol; } }
        private static Condition SubSymbol { get { return TokenType.SubSymbol; } }
        private static Condition Value { get { return TokenType.Value; } }
        private static Condition Type { get { return TokenType.Type; } }
        private static Condition ParameterList { get { return TokenType.ParameterList; } }
        private static Condition VariableModifier { get { return TokenType.VariableModifier; } }
        private static Condition WhileLoop { get { return TokenType.WhileLoop; } }
        private static Condition TypeSubnode { get { return TokenType.TypeSubnode; } }
        private static Condition TypeAndSymbolSubnode { get { return TokenType.TypeAndSymbolSubnode; } }
        private static Condition Function { get { return TokenType.Function; } }
        private static Condition ConditionalSentence { get { return TokenType.ConditionalSentence; } }
        private static Condition CommaAndValue { get { return TokenType.CommaAndValue; } }
        private static Condition ParenthesesNode { get { return TokenType.ParenthesesNode; } }
        private static Condition Operand { get { return TokenType.Operand; } }
        private static Condition PrefixNode { get { return TokenType.PrefixNode; } }
        private static Condition PostfixNode { get { return TokenType.PostfixNode; } }
        private static Condition PostfixOperator { get { return TokenType.PostfixOperator; } }
        private static Condition PrefixOperator { get { return TokenType.PrefixOperator; } }
        private static Condition FunctionArgumentsList { get { return TokenType.FunctionArgumentsList; } }
        private static Condition LiteralNode { get { return TokenType.LiteralNode; } }
        private static Condition Entry { get { return TokenType.Entry; } }
        private static Condition Mutable { get { return TokenType.Mutable; } }
        private static Condition UnknownNode { get { return TokenType.UnknownNode; } }
        private static Condition ArrayLiteral { get { return TokenType.ArrayLiteral; } }
        private static Condition IndexNode { get { return TokenType.IndexNode; } }
        private static Condition RightBracket { get { return TokenType.RightBracket; } }
        private static Condition LeftBracket { get { return TokenType.LeftBracket; } }
        private static Condition FunctorParameters { get { return TokenType.FunctorParameters; } }
        private static Condition InitializerList { get { return TokenType.InitializerList; } }
    }
}
