using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.MethodBodyOptimizers
{
    internal class AddTailCalls : ModifierBase
    {
        public override bool ReleaseOnlyOpmization { get { return false; } }

        protected override bool MatchesPredicate(IList<Instruction> instructions, int instructionIndex)
        {
            var i = instructionIndex;

            if (i < instructions.Count - 1 && instructions[i + 1].OpCode == OpCodes.Ret && IsCall(instructions[i]))
            {
                if (i > 0 && instructions[i - 1].OpCode == OpCodes.Tail)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        protected override InstructionOperation GetOperation()
        {
            return InstructionOperation.InsertBefore;
        }

        protected override Instruction GetReplacementInstruction()
        {
            return Instruction.Create(OpCodes.Tail);
        }

        private static bool IsCall(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt || instruction.OpCode == OpCodes.Calli;
        }
    }
}
