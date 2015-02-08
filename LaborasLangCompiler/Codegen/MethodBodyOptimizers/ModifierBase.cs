using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace LaborasLangCompiler.Codegen.MethodBodyOptimizers
{
    internal abstract class ModifierBase : IOptimizer
    {
        public abstract bool ReleaseOnlyOpmization { get; }

        public void Execute(MethodBody body)
        {
            var instructions = body.Instructions;
            var replacementMap = new Dictionary<Instruction, Instruction>();

            OnBegin(instructions);

            for (int i = 0; i < instructions.Count; i++)
            {
                if (MatchesPredicate(instructions, i))
                {
                    var operation = GetOperation();

                    if (operation == InstructionOperation.Remove)
                    {
                        var next = instructions[i].Next;
                        if (next != null)
                        {
                            replacementMap[instructions[i]] = next;
                        }

                        instructions.RemoveAt(i);
                        i--;

                        continue;
                    }

                    var targetInstruction = GetReplacementInstruction();

                    switch (operation)
                    {
                        case InstructionOperation.Replace:
                            replacementMap[instructions[i]] = targetInstruction;
                            instructions[i] = targetInstruction;
                            break;

                        case InstructionOperation.InsertAfter:
                            instructions.Insert(i + 1, targetInstruction);
                            i++;
                            continue;

                        case InstructionOperation.InsertBefore:
                            replacementMap[instructions[i]] = targetInstruction;
                            instructions.Insert(i, targetInstruction);
                            i++;
                            continue;
                    }
                }
            }

            var keys = new List<Instruction>(replacementMap.Keys);

            foreach (var key in keys)
            {
                var value = replacementMap[key];

                while (replacementMap.ContainsKey(value))
                {
                    replacementMap[key] = value = replacementMap[value];
                }
            }

            foreach (var instruction in instructions)
            {
                var operand = instruction.Operand as Instruction;

                if (operand != null && replacementMap.ContainsKey(operand))
                {
                    instruction.Operand = replacementMap[operand];
                }
            }
        }

        protected enum InstructionOperation
        {
            Remove,
            InsertBefore,
            InsertAfter,
            Replace
        }

        protected abstract bool MatchesPredicate(IList<Instruction> instructions, int instructionIndex);
        protected abstract InstructionOperation GetOperation();
        protected abstract Instruction GetReplacementInstruction();

        protected virtual void OnBegin(IList<Instruction> instructions)
        {
        }
    }
}
