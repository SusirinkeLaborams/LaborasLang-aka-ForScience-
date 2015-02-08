using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace LaborasLangCompiler.Codegen.MethodBodyOptimizers
{
    internal class RemoveRedundantBranches : ModifierBase
    {
        public override bool ReleaseOnlyOpmization { get { return false; } }

        protected override bool MatchesPredicate(IList<Instruction> instructions, int instructionIndex)
        {
            var instruction = instructions[instructionIndex];

            if (instruction.OpCode != OpCodes.Ret && instruction.Next != null &&
                instruction.Next.OpCode != OpCodes.Ret && LeadsToRet(instruction.Next))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override ModifierBase.InstructionOperation GetOperation()
        {
            return InstructionOperation.InsertAfter;
        }

        protected override Instruction GetReplacementInstruction()
        {
            return Instruction.Create(OpCodes.Ret);
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
