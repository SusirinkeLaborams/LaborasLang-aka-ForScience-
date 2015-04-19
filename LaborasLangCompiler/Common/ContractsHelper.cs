using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Common
{
    static class ContractsHelper
    {
        [ContractVerification(false)]
        public static void AssumeUnreachable(string message)
        {
            Contract.Assume(false);
            throw new Exception(string.Format("Code that was assumed to be unreachable was reached: {0}", message));
        }

        [ContractVerification(false)]
        public static void AssertUnreachable(string message)
        {
            Contract.Requires(false);
            throw new Exception(string.Format("Code that was asserted to be unreachable was reached: {0}", message));
        }

        [ContractVerification(false)]
        public static void AssertUnreachable(string format, params object[] args)
        {
            AssertUnreachable(String.Format(format, args));
        }

        [ContractVerification(false)]
        public static void AssumeUnreachable(string format, params object[] args)
        {
            AssumeUnreachable(String.Format(format, args));
        }
    }
}
