using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.MethodBodyOptimizers
{
    internal class AddTailCalls : IOptimizer
    {
        public bool ReleaseOnlyOpmization { get { return false; } }

        public void Execute(MethodBody body)
        {
            var instructions = body.Instructions;

            for (int i = 1; i < instructions.Count; i++)
            {
                if (instructions[i].OpCode == OpCodes.Ret && IsCall(instructions[i - 1]))
                {
                    if (i > 1 && instructions[i - 2].OpCode == OpCodes.Tail)
                    {
                        continue;
                    }

                    var tail = Instruction.Create(OpCodes.Tail);
                    var call = instructions[i - 1];

                    instructions.Insert(i - 1, tail);

                    foreach (var instruction in instructions)
                    {
                        if (instruction.Operand == call)
                        {
                            instruction.Operand = tail;
                        }
                    }
                }
            }
        }

        private static bool IsCall(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt || instruction.OpCode == OpCodes.Calli;
        }
    }
}
