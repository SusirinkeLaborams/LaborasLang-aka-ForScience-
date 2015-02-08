using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Common
{
    public enum ErrorCode
    {
        SymbolAlreadyDeclared = 0001,
        IllegalCast = 0002,
        IllegalMethodParam = 0003,
        MissingReturn = 0004,
        InvalidEntryReturn = 0005,
        InvalidEntryParams = 0006,
        InvalidMethodMods = 0007,
        InvalidStructure = 0008,
        VoidLValue = 0009,
        MissingInit = 0010,
        TypeMissmatch = 0011,
        InvalidFieldMods = 0012,
        DuplicateMods = 0013,
        InvalidCondition = 0014,
        SymbolNotFound = 0015,
        NotAnRValue = 0016,
        NotCallable = 0017,
        InvalidDot = 0018,
        NotAnLValue = 0019,
        InvalidVariableMods = 0020,
        MissingInstance = 0021,
        UnreachableMember = 0022,
        AmbiguousSymbol = 0023,
        DuplicateImport = 0024,
        TypeExpected = 0025
    }

    public static class Errors
    {
        public static IReadOnlyList<Error> Reported { get { return errors; } }

        private static List<Error> errors = new List<Error>();

        public static void ReportAndThrow(this ErrorCode error, string message)
        {
            ReportAndThrow(error, null, message);
        }

        public static void ReportAndThrow(this ErrorCode error, SequencePoint point, string format, params object[] args)
        {
            ReportAndThrow(error, point, String.Format(format, args));
        }

        public static void ReportAndThrow(this ErrorCode error, SequencePoint point, string message)
        {
            Report(error, point, message);
            throw new CompilerException();
        }

        public static void Report(this ErrorCode error, string message)
        {
            Report(error, null, message);
        }

        public static void Report(this ErrorCode error, SequencePoint point, string message)
        {
            if (message == null)
                throw new ArgumentNullException("Message must not be null");
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
                builder.AppendFormat("error LL{0:0000}: {1}", (int)ErrorCode, Message);
                return builder.ToString();
            }
        }
    }
}
