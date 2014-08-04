System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 131 (0x83)
.maxstack 4
.entrypoint
.locals init (
	[0] int32,
	[1] int32,
	[2] int32,
	[3] int32,
	[4] int32,
	[5] int32,
	[6] int32,
	[7] object[]
)

IL_0000: ldc.i4.5
IL_0001: stloc.0
IL_0002: ldloc.0
IL_0003: not
IL_0004: stloc.1
IL_0005: ldloc.0
IL_0006: neg
IL_0007: stloc.2
IL_0008: ldloc.0
IL_0009: dup
IL_000a: ldc.i4.m1
IL_000b: add
IL_000c: stloc.0
IL_000d: stloc.3
IL_000e: ldloc.0
IL_000f: dup
IL_0010: ldc.i4.1
IL_0011: add
IL_0012: stloc.0
IL_0013: stloc.s 4
IL_0015: ldloc.0
IL_0016: ldc.i4.m1
IL_0017: add
IL_0018: dup
IL_0019: stloc.0
IL_001a: stloc.s 5
IL_001c: ldloc.0
IL_001d: ldc.i4.1
IL_001e: add
IL_001f: dup
IL_0020: stloc.0
IL_0021: stloc.s 6
IL_0023: ldstr "Results: \r\n{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}\r\n"
IL_0028: ldc.i4.7
IL_0029: newarr [mscorlib]System.Object
IL_002e: stloc.s 7
IL_0030: ldloc.s 7
IL_0032: ldc.i4.0
IL_0033: ldloc.0
IL_0034: box [mscorlib]System.Int32
IL_0039: stelem.ref
IL_003a: ldloc.s 7
IL_003c: ldc.i4.1
IL_003d: ldloc.1
IL_003e: box [mscorlib]System.Int32
IL_0043: stelem.ref
IL_0044: ldloc.s 7
IL_0046: ldc.i4.2
IL_0047: ldloc.2
IL_0048: box [mscorlib]System.Int32
IL_004d: stelem.ref
IL_004e: ldloc.s 7
IL_0050: ldc.i4.3
IL_0051: ldloc.3
IL_0052: box [mscorlib]System.Int32
IL_0057: stelem.ref
IL_0058: ldloc.s 7
IL_005a: ldc.i4.4
IL_005b: ldloc.s 4
IL_005d: box [mscorlib]System.Int32
IL_0062: stelem.ref
IL_0063: ldloc.s 7
IL_0065: ldc.i4.5
IL_0066: ldloc.s 5
IL_0068: box [mscorlib]System.Int32
IL_006d: stelem.ref
IL_006e: ldloc.s 7
IL_0070: ldc.i4.6
IL_0071: ldloc.s 6
IL_0073: box [mscorlib]System.Int32
IL_0078: stelem.ref
IL_0079: ldloc.s 7
IL_007b: tail.
IL_007d: call void [mscorlib]System.Console::WriteLine(string, object[])
IL_0082: ret