﻿using LaborasLangCompiler.Codegen;
using LaborasLangCompiler.Codegen.Methods;
using LaborasLangCompiler.Codegen.Types;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser.Emitters
{
    interface IEmitterSource
    {
        AssemblyEmitter Assembly { get; }
        IMethodEmitter CreateMethod(TypeEmitter declaringType, string name, TypeReference returnType, MethodAttributes methodAttributes);
    }
}
