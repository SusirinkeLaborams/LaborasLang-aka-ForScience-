using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace LaborasLangCompiler.Codegen.MethodBodyOptimizers
{
    internal class RemoveNOPs : ModifierBase
    {
        public override bool ReleaseOnlyOpmization { get { return true; } }

        protected override bool MatchesPredicate(IList<Instruction> instructions, int instructionIndex)
        {
            return instructions[instructionIndex].OpCode == OpCodes.Nop;
        }

        protected override ModifierBase.InstructionOperation GetOperation()
        {
            return InstructionOperation.Remove;
        }

        protected override Instruction GetReplacementInstruction()
        {
            throw new NotSupportedException();
        }
    }
}
