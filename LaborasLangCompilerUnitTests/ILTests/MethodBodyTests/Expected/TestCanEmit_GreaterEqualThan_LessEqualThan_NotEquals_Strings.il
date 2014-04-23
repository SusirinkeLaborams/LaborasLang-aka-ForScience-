System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 135 (0x87)
.maxstack 5
.entrypoint

IL_0000: ldstr "Is {0} is greater than or equal to {1}? {2}"
IL_0005: ldstr "hi"
IL_000a: ldstr "bye"
IL_000f: ldstr "hi"
IL_0014: ldstr "bye"
IL_0019: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_001e: ldc.i4.0
IL_001f: clt
IL_0021: ldc.i4.0
IL_0022: ceq
IL_0024: box [mscorlib]System.Boolean
IL_0029: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_002e: ldstr "Is {0} is less than or equal to {1}? {2}"
IL_0033: ldstr "hi"
IL_0038: ldstr "bye"
IL_003d: ldstr "hi"
IL_0042: ldstr "bye"
IL_0047: call int32 [mscorlib]System.String::CompareOrdinal(string, string)
IL_004c: ldc.i4.0
IL_004d: cgt
IL_004f: ldc.i4.0
IL_0050: ceq
IL_0052: box [mscorlib]System.Boolean
IL_0057: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_005c: ldstr "Is {0} is not equal to {1}? {2}"
IL_0061: ldstr "hi"
IL_0066: ldstr "bye"
IL_006b: ldstr "hi"
IL_0070: ldstr "bye"
IL_0075: ceq
IL_0077: ldc.i4.0
IL_0078: ceq
IL_007a: box [mscorlib]System.Boolean
IL_007f: tail.
IL_0081: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0086: ret