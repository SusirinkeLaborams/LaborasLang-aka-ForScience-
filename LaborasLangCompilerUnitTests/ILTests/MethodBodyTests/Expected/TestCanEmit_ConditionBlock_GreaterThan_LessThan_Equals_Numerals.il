System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 119 (0x77)
.maxstack 3
.entrypoint
.locals (
	[0] int32,
	[1] int32
)

IL_0000: ldc.i4.5
IL_0001: stloc.0
IL_0002: ldc.i4.6
IL_0003: stloc.1
IL_0004: ldloc.0
IL_0005: ldloc.1
IL_0006: cgt
IL_0008: brfalse.s IL_0022

IL_000a: ldstr "{0} is greater than {1}."
IL_000f: ldloc.0
IL_0010: box [mscorlib]System.Int32
IL_0015: ldloc.1
IL_0016: box [mscorlib]System.Int32
IL_001b: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0020: br.s IL_0076

IL_0022: ldloc.0
IL_0023: ldloc.1
IL_0024: clt
IL_0026: brfalse.s IL_0040

IL_0028: ldstr "{0} is less than {1}."
IL_002d: ldloc.0
IL_002e: box [mscorlib]System.Int32
IL_0033: ldloc.1
IL_0034: box [mscorlib]System.Int32
IL_0039: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_003e: br.s IL_0076

IL_0040: ldloc.0
IL_0041: ldloc.1
IL_0042: ceq
IL_0044: brfalse.s IL_005e

IL_0046: ldstr "{0} and {1} are equal."
IL_004b: ldloc.0
IL_004c: box [mscorlib]System.Int32
IL_0051: ldloc.1
IL_0052: box [mscorlib]System.Int32
IL_0057: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_005c: br.s IL_0076

IL_005e: ldstr "We're screwed."
IL_0063: ldloc.0
IL_0064: box [mscorlib]System.Int32
IL_0069: ldloc.1
IL_006a: box [mscorlib]System.Int32
IL_006f: tail.
IL_0071: call void [mscorlib]System.Console::WriteLine(string, object, object)

IL_0076: ret