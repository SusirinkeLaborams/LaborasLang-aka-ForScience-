using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.MethodBodyOptimizers
{
    interface IOptimizer
    {
        void Execute(MethodBody body);
    }

    internal class MethodBodyOptimizerBootstraper
    {
        private static IOptimizer[] optimizers;

        static MethodBodyOptimizerBootstraper()
        {
            optimizers = new IOptimizer[]
            {
                new AddTailCalls()
            };
        }

        public static void Optimize(MethodBody body)
        {
            body.SimplifyMacros();

            foreach (var step in optimizers)
            {
                step.Execute(body);
            }

            body.OptimizeMacros();
        }
    }
}
