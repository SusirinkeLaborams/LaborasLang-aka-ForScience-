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
        SymbolAlreadyDeclared = 0001
    }

    public static class ErrorHandling
    {
        public static IReadOnlyList<Error> Errors { get { return errors; } }

        private static List<Error> errors = new List<Error>();

        public static void Report(ErrorCode error, SequencePoint point, string message)
        {
            if (message == null)
                throw new ArgumentNullException("Message must not be null");
            var newError = new Error(point, error, message);
            errors.Add(newError);

            Console.WriteLine(newError);
            throw new CompilerException();
        }

        public static void Report(ErrorCode error, string message)
        {
            Report(error, null, message);
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
                builder.AppendFormat("error LL{0:4}: {2}", ErrorCode, Message);
                return builder.ToString();
            }
        }
    }
}
