System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 99 (0x63)
.maxstack 3
.entrypoint
.locals (
	[0] string,
	[1] string
)

IL_0000: ldstr "hi"
IL_0005: stloc.0
IL_0006: ldstr "bye"
IL_000b: stloc.1
IL_000c: ldloc.0
IL_000d: ldloc.1
IL_000e: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_0013: ldc.i4.0
IL_0014: cgt
IL_0016: brfalse.s IL_0026

IL_0018: ldstr "{0} is greater than {1}."
IL_001d: ldloc.0
IL_001e: ldloc.1
IL_001f: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0024: br.s IL_0062

IL_0026: ldloc.0
IL_0027: ldloc.1
IL_0028: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_002d: ldc.i4.0
IL_002e: clt
IL_0030: brfalse.s IL_0040

IL_0032: ldstr "{0} is less than {1}."
IL_0037: ldloc.0
IL_0038: ldloc.1
IL_0039: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_003e: br.s IL_0062

IL_0040: ldloc.0
IL_0041: ldloc.1
IL_0042: ceq
IL_0044: brfalse.s IL_0054

IL_0046: ldstr "{0} and {1} are equal."
IL_004b: ldloc.0
IL_004c: ldloc.1
IL_004d: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0052: br.s IL_0062

IL_0054: ldstr "We're screwed."
IL_0059: ldloc.0
IL_005a: ldloc.1
IL_005b: tail.
IL_005d: call void [mscorlib]System.Console::WriteLine(string, object, object)

IL_0062: ret