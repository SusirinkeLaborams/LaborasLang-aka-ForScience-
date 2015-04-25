using LaborasLangCompiler.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LaborasLangCompiler.Codegen.Methods
{
    struct TemporaryVariables
    {
        private struct TempVariable
        {
            public readonly VariableDefinition variable;
            public readonly bool isTaken;

            public TempVariable(VariableDefinition variable, bool isTaken) : this()
            {
                this.variable = variable;
                this.isTaken = isTaken;
            }
        }

        private readonly AssemblyEmitter assembly;
        private readonly MethodBody methodBody;
        private List<TempVariable> temporaryVariables;

        public TemporaryVariables(AssemblyEmitter assembly, MethodBody methodBody) : this()
        {
            this.assembly = assembly;
            this.methodBody = methodBody;
        }

        public VariableDefinition Acquire(TypeReference type)
        {
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<VariableDefinition>() != null);

            if (temporaryVariables == null)
                temporaryVariables = new List<TempVariable>();

            for (int i = 0; i < temporaryVariables.Count; i++)
            {
                if (!temporaryVariables[i].isTaken)
                {
                    var tempVariableType = temporaryVariables[i].variable.VariableType;

                    if (tempVariableType.FullName == type.FullName && tempVariableType.Scope == type.Scope)
                    {
                        var result = temporaryVariables[i].variable;
                        temporaryVariables[i] = new TempVariable(result, true);
                        return result;
                    }
                }
            }

            var variableName = "$Temp" + (temporaryVariables.Count + 1).ToString();
            var variable = new VariableDefinition(variableName, type);
            temporaryVariables.Add(new TempVariable(variable, true));
            methodBody.Variables.Add(variable);
            assembly.AddTypeUsage(type);
            return variable;
        }

        public void Release(VariableDefinition variable)
        {
            Contract.Requires(variable != null);

            for (int i = 0; i < temporaryVariables.Count; i++)
            {
                if (temporaryVariables[i].variable == variable)
                {
                    temporaryVariables[i] = new TempVariable(variable, false);
                    return;
                }
            }

            ContractsHelper.AssertUnreachable("Invalid variable was passed to TemporaryVairables.Release().");
        }

        public void ReleaseAll()
        {
            if (temporaryVariables == null)
                return;

            for (int i = 0; i < temporaryVariables.Count; i++)
            {
                if (temporaryVariables[i].isTaken)
                {
                    temporaryVariables[i] = new TempVariable(temporaryVariables[i].variable, false);
                }
            }
        }
    }
}
