System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 108 (0x6c)
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
IL_0016: brfalse.s IL_0029

IL_0018: ldstr "{0} is greater than {1}."
IL_001d: ldloc.0
IL_001e: ldloc.1
IL_001f: tail.
IL_0021: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0026: ret

IL_0027: br.s IL_006b

IL_0029: ldloc.0
IL_002a: ldloc.1
IL_002b: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_0030: ldc.i4.0
IL_0031: clt
IL_0033: brfalse.s IL_0046

IL_0035: ldstr "{0} is less than {1}."
IL_003a: ldloc.0
IL_003b: ldloc.1
IL_003c: tail.
IL_003e: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_0043: ret

IL_0044: br.s IL_006b

IL_0046: ldloc.0
IL_0047: ldloc.1
IL_0048: ceq
IL_004a: brfalse.s IL_005d

IL_004c: ldstr "{0} and {1} are equal."
IL_0051: ldloc.0
IL_0052: ldloc.1
IL_0053: tail.
IL_0055: call void [mscorlib]System.Console::WriteLine(string, object, object)
IL_005a: ret

IL_005b: br.s IL_006b

IL_005d: ldstr "We're screwed."
IL_0062: ldloc.0
IL_0063: ldloc.1
IL_0064: tail.
IL_0066: call void [mscorlib]System.Console::WriteLine(string, object, object)

IL_006b: ret