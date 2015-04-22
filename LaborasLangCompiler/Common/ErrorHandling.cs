using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Common
{
    public enum ErrorCode
    {
        ParserErrors = 0000,
        InvalidStructure = 0001,
        SymbolAlreadyDeclared = 0002,
        SymbolNotFound = 0003,

        TypeMissmatch = 0004,
        IllegalCast = 0005,
        NotCallable = 0006,

        MissingInit = 0007,
        VoidDeclaration = 0008,

        NotAnRValue = 0009,
        NotAnLValue = 0010,

        DuplicateMods = 0011,

        MissingReturn = 0012,
        IllegalMethodParam = 0013,
        InvalidMethodMods = 0014,

        InvalidEntryReturn = 0015,
        InvalidEntryParams = 0016,

        InvalidFieldMods = 0017,

        InvalidVariableMods = 0018,

        InvalidCondition = 0019,
        
        TypeExpected = 0020,
        AmbiguousSymbol = 0021,
        UnreachableMember = 0022,

        MissingInstance = 0023,

        InvalidDot = 0024,

        DuplicateImport = 0025,

        CannotCreate = 0026,
        MisshapedMatrix = 0027,
        InvalidIndexCount = 0028,
        InvalidIndexType = 0029,
        CannotIndex = 0030,
        MissingArraySize = 0031,
        ArrayDimMissmatch = 0032,
        NotLiteralArrayDims = 0033,

        MultipleCharacterLiteral = 0034,

        NullInsideDot = 0035,
        InferrenceFromTypeless = 0036,

        InvalidForEachCollection = 0037,

        IntegerOverlflow = 0038,

        CompilerArgumentErrors = 1000,
        NoSourceFiles = 1001,
        UnknownCompilerSwitch = 1002,
        MoreThanOneModuleKind = 1003,
        NoOutputPaths = 1004,
        MoreThanOneOutputPath = 1005,
        OutputPathAndModuleKindMismatch = 1006,
        IllegalCharactersInPath = 1007,
        UnspecifiedNamespace = 1008,
        TypeNameClash = 1009,
        SourceFileDoesNotExist = 1010,
        UnresolvedReference = 1011,
    }

    public static class Errors
    {
        public static IReadOnlyList<Error> Reported { get { return errors; } }

        private static List<Error> errors = new List<Error>();

        public static void ReportAndThrow(this ErrorCode error, string message)
        {
            Contract.Requires(message != null);
            ReportAndThrow(error, null, message);
        }

        public static void ReportAndThrow(this ErrorCode error, SequencePoint point, string format, params object[] args)
        {
            Contract.Requires(format != null);
            Contract.Requires(args != null);

            ReportAndThrow(error, point, String.Format(format, args));
        }

        public static void ReportAndThrow(this ErrorCode error, SequencePoint point, string message)
        {
            Contract.Requires(message != null);
            Report(error, point, message);
            throw new CompilerException();
        }

        public static void Report(this ErrorCode error, string message)
        {
            Contract.Requires(message != null);
            Report(error, null, message);
        }

        public static void Report(this ErrorCode error, SequencePoint point, string message)
        {
            Contract.Requires(message != null);
            var newError = new Error(point, error, message);
            errors.Add(newError);

            Console.WriteLine(newError);
        }

        public static void Clear()
        {
            errors = new List<Error>();
        }

        public class Error
        {
            public SequencePoint Point { get; private set; }
            public ErrorCode ErrorCode { get; private set; }
            public string Message { get; private set; }

            public Error(SequencePoint point, ErrorCode error, string message)
            {
                Point = point;
                ErrorCode = error;
                Message = message;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                if(Point != null)
                {
                    builder.AppendFormat("{0}({1},{2},{3},{4}): ", Point.Document.Url, Point.StartLine, Point.StartColumn, Point.EndLine, Point.EndColumn);
                }
                builder.AppendFormat("error LL{0:0000} {1}: {2}", (int)ErrorCode, ErrorCode, Message);
                return builder.ToString();
            }
        }
    }
}
