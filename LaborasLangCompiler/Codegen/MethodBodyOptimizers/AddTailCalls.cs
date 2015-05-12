using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace LaborasLangCompiler.Codegen.MethodBodyOptimizers
{
    internal class AddTailCalls : ModifierBase
    {
        public override bool ReleaseOnlyOpmization { get { return false; } }

        protected override bool MatchesPredicate(MethodBody body, int instructionIndex)
        {
            var instructions = body.Instructions;
            var i = instructionIndex;

            if (IsWithinExceptionHandler(body.ExceptionHandlers, instructions[i]))
                return false;

            if (i < instructions.Count - 1 && instructions[i + 1].OpCode == OpCodes.Ret && IsTailCallable(instructions[i]))
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

        private static bool IsTailCallable(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Calli)
                return true;

            if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt)
                return false;

            // Can't pass by reference to a tail call
            var target = (MethodReference)instruction.Operand;

            for (int i = 0; i < target.Parameters.Count; i++)
            {
                if (target.Parameters[i].ParameterType.IsByReference)
                    return false;
            }

            if (target.HasThis && target.DeclaringType.IsValueType)
                return false;

            return true;
        }
    }
}
