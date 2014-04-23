System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 105 (0x69)
.maxstack 5
.entrypoint
.locals (
	[0] uint32,
	[1] uint32
)

IL_0000: ldc.i4 342
IL_0005: stloc.0
IL_0006: ldc.i4 2113
IL_000b: stloc.1
IL_000c: ldstr "{0} & {1} == {2}."
IL_0011: ldloc.0
IL_0012: box [mscorlib]System.UInt32
IL_0017: ldloc.1
IL_0018: box [mscorlib]System.UInt32
IL_001d: ldloc.0
IL_001e: ldloc.1
IL_001f: and
IL_0020: box [mscorlib]System.UInt32
IL_0025: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_002a: ldstr "{0} | {1} == {2}."
IL_002f: ldloc.0
IL_0030: box [mscorlib]System.UInt32
IL_0035: ldloc.1
IL_0036: box [mscorlib]System.UInt32
IL_003b: ldloc.0
IL_003c: ldloc.1
IL_003d: or
IL_003e: box [mscorlib]System.UInt32
IL_0043: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0048: ldstr "{0} ^ {1} == {2}."
IL_004d: ldloc.0
IL_004e: box [mscorlib]System.UInt32
IL_0053: ldloc.1
IL_0054: box [mscorlib]System.UInt32
IL_0059: ldloc.0
IL_005a: ldloc.1
IL_005b: xor
IL_005c: box [mscorlib]System.UInt32
IL_0061: tail.
IL_0063: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0068: ret