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

            OnBegin(body);

            for (int i = 0; i < instructions.Count; i++)
            {
                if (MatchesPredicate(body, i))
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

            foreach (var exceptionHandler in body.ExceptionHandlers)
            {
                if (exceptionHandler.TryStart != null && replacementMap.ContainsKey(exceptionHandler.TryStart))
                    exceptionHandler.TryStart = replacementMap[exceptionHandler.TryStart];

                if (exceptionHandler.TryEnd != null && replacementMap.ContainsKey(exceptionHandler.TryEnd))
                    exceptionHandler.TryEnd = replacementMap[exceptionHandler.TryEnd];

                if (exceptionHandler.FilterStart != null && replacementMap.ContainsKey(exceptionHandler.FilterStart))
                    exceptionHandler.FilterStart = replacementMap[exceptionHandler.FilterStart];

                if (exceptionHandler.HandlerStart != null && replacementMap.ContainsKey(exceptionHandler.HandlerStart))
                    exceptionHandler.HandlerStart = replacementMap[exceptionHandler.HandlerStart];

                if (exceptionHandler.HandlerEnd != null && replacementMap.ContainsKey(exceptionHandler.HandlerEnd))
                    exceptionHandler.HandlerEnd = replacementMap[exceptionHandler.HandlerEnd];
            }
        }

        protected enum InstructionOperation
        {
            Remove,
            InsertBefore,
            InsertAfter,
            Replace
        }

        protected abstract bool MatchesPredicate(MethodBody body, int instructionIndex);
        protected abstract InstructionOperation GetOperation();
        protected abstract Instruction GetReplacementInstruction();

        protected virtual void OnBegin(MethodBody body)
        {
        }

        protected static bool IsWithinExceptionHandler(IList<ExceptionHandler> exceptionHandlers, Instruction instruction)
        {
            for (int i = 0; i < exceptionHandlers.Count; i++)
            {
                var handler = exceptionHandlers[i];

                var tryBegin = handler.TryStart;
                var tryEnd = handler.TryEnd;

                while (tryBegin != tryEnd && tryBegin != null)
                {
                    if (tryBegin == instruction)
                        return true;

                    tryBegin = tryBegin.Next;
                }

                var handlerBegin = handler.HandlerStart;
                var handlerEnd = handler.HandlerEnd;

                while (handlerBegin != handlerEnd && handlerBegin != null)
                {
                    if (handlerBegin == instruction)
                        return true;

                    handlerBegin = handlerBegin.Next;
                }
            }

            return false;
        }
    }
}
