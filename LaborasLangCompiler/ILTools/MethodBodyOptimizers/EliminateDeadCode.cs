using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.ILTools.MethodBodyOptimizers
{
    internal class EliminateDeadCode : ModifierBase
    {
        public override bool ReleaseOnlyOpmization { get { return true; } }

        private Dictionary<Instruction, bool> isVisitedMap;

        private void MakeInstructionIndexMap(IList<Instruction> instructions)
        {
            isVisitedMap = new Dictionary<Instruction, bool>();

            for (int i = 0; i < instructions.Count; i++)
            {
                isVisitedMap[instructions[i]] = false;
            }
        }

        private void AddToStackIfNotVisited(Instruction[] stack, ref int stackSize, Instruction instructionToAdd)
        {
            if (!isVisitedMap[instructionToAdd])
            {
                stack[stackSize] = instructionToAdd;
                isVisitedMap[instructionToAdd] = true;

                stackSize++;
            }
        }

        // Basically, do a Depth first search on instructions to find 
        // which ones are not gonna be visited and remember that for later
        protected override void OnBegin(IList<Instruction> instructions)
        {
            MakeInstructionIndexMap(instructions);
            var dfsStack = new Instruction[instructions.Count];

            dfsStack[0] = instructions[0];
            isVisitedMap[instructions[0]] = true;
            var stackSize = 1;

            while (stackSize > 0)
            {
                var instruction = dfsStack[stackSize - 1];
                stackSize--;

                if (instruction.OpCode == OpCodes.Ret)
                {
                    continue;
                }
                else if (instruction.OpCode.FlowControl == FlowControl.Cond_Branch)
                {
                    AddToStackIfNotVisited(dfsStack, ref stackSize, instruction.Next);
                    AddToStackIfNotVisited(dfsStack, ref stackSize, (Instruction)instruction.Operand);
                }
                else if (instruction.OpCode.FlowControl == FlowControl.Branch)
                {
                    AddToStackIfNotVisited(dfsStack, ref stackSize, (Instruction)instruction.Operand);
                }
                else
                {
                    AddToStackIfNotVisited(dfsStack, ref stackSize, instruction.Next);
                }
            }
        }

        protected override bool MatchesPredicate(IList<Mono.Cecil.Cil.Instruction> instructions, int instructionIndex)
        {
            return !isVisitedMap[instructions[instructionIndex]];
        }

        protected override InstructionOperation GetOperation()
        {
            return InstructionOperation.Remove;
        }

        protected override Instruction GetReplacementInstruction()
        {
            throw new NotSupportedException();
        }
    }
}
