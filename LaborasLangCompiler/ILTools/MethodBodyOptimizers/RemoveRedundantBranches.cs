using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.MethodBodyOptimizers
{
    internal class RemoveRedundantBranches : IOptimizer
    {
        public bool ReleaseOnlyOpmization { get { return true; } }

        public void Execute(MethodBody body)
        {
            var instructions = body.Instructions;

            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];

                if (instruction.OpCode != OpCodes.Ret && instruction.Next != null && 
                    instruction.Next.OpCode != OpCodes.Ret && LeadsToRet(instruction.Next))
                {
                    instructions.Insert(i + 1, Instruction.Create(OpCodes.Ret));
                    i++;
                }
            }
        }

        public bool LeadsToRet(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ret)
            {
                return true;
            }
            else if (instruction.OpCode == OpCodes.Nop ||
                instruction.OpCode.FlowControl == FlowControl.Meta)
            {
                return LeadsToRet(instruction.Next);
            }
            else if (instruction.OpCode == OpCodes.Br ||
                     instruction.OpCode == OpCodes.Br_S ||
                     instruction.OpCode == OpCodes.Leave)
            {
                return LeadsToRet((Instruction)instruction.Operand);
            }
            else
            {
                return false;
            }
        }
    }
}
