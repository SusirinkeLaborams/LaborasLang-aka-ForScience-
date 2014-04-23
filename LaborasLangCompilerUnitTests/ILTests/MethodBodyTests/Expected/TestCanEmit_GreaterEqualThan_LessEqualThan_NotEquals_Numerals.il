System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 153 (0x99)
.maxstack 5
.entrypoint

IL_0000: ldstr "Is {0} is greater than or equal to {1}? {2}"
IL_0005: ldc.r4 3.5
IL_000a: box [mscorlib]System.Single
IL_000f: ldc.r4 2.1
IL_0014: box [mscorlib]System.Single
IL_0019: ldc.r4 3.5
IL_001e: ldc.r4 2.1
IL_0023: clt
IL_0025: ldc.i4.0
IL_0026: ceq
IL_0028: box [mscorlib]System.Boolean
IL_002d: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0032: ldstr "Is {0} is less than or equal to {1}? {2}"
IL_0037: ldc.r4 3.5
IL_003c: box [mscorlib]System.Single
IL_0041: ldc.r4 2.1
IL_0046: box [mscorlib]System.Single
IL_004b: ldc.r4 3.5
IL_0050: ldc.r4 2.1
IL_0055: cgt
IL_0057: ldc.i4.0
IL_0058: ceq
IL_005a: box [mscorlib]System.Boolean
IL_005f: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0064: ldstr "Is {0} is not equal to {1}? {2}"
IL_0069: ldc.r4 3.5
IL_006e: box [mscorlib]System.Single
IL_0073: ldc.r4 2.1
IL_0078: box [mscorlib]System.Single
IL_007d: ldc.r4 3.5
IL_0082: ldc.r4 2.1
IL_0087: ceq
IL_0089: ldc.i4.0
IL_008a: ceq
IL_008c: box [mscorlib]System.Boolean
IL_0091: tail.
IL_0093: call void [mscorlib]System.Console::WriteLine(string, object, object, object)
IL_0098: ret