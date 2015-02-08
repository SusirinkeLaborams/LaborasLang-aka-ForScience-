using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace LaborasLangCompiler.Codegen.MethodBodyOptimizers
{
    interface IOptimizer
    {
        bool ReleaseOnlyOpmization { get; }

        void Execute(MethodBody body);
    }

    internal class MethodBodyOptimizerBootstraper
    {
        private static IOptimizer[] optimizers;

        static MethodBodyOptimizerBootstraper()
        {
            optimizers = new IOptimizer[]
            {
                new RemoveNOPs(),
                new RemoveRedundantBranches(),
                new EliminateDeadCode(),
                new AddTailCalls()
            };
        }

        public static void Optimize(MethodBody body, bool debugBuild)
        {
            body.SimplifyMacros();

            foreach (var step in optimizers)
            {
                if (!debugBuild || !step.ReleaseOnlyOpmization)
                {
                    step.Execute(body);
                }
            }

            body.OptimizeMacros();
        }
    }
}
