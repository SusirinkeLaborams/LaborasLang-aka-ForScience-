System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Void klass::TestNestedInstanceFieldAssignment()
// Method begins at RVA 0x2054
// Code size 120 (0x78)
.maxstack 11

IL_0000: ldarg.0
IL_0001: ldarg.0
IL_0002: ldarg.0
IL_0003: ldarg.0
IL_0004: ldarg.0
IL_0005: ldarg.0
IL_0006: ldarg.0
IL_0007: ldarg.0
IL_0008: ldarg.0
IL_0009: ldarg.0
IL_000a: ldc.r4 110
IL_000f: stfld float32 klass::intField0
IL_0014: ldarg.0
IL_0015: ldfld float32 klass::intField0
IL_001a: stfld float32 klass::intField1
IL_001f: ldarg.0
IL_0020: ldfld float32 klass::intField1
IL_0025: stfld float32 klass::intField2
IL_002a: ldarg.0
IL_002b: ldfld float32 klass::intField2
IL_0030: stfld float32 klass::intField3
IL_0035: ldarg.0
IL_0036: ldfld float32 klass::intField3
IL_003b: stfld float32 klass::intField4
IL_0040: ldarg.0
IL_0041: ldfld float32 klass::intField4
IL_0046: stfld float32 klass::intField5
IL_004b: ldarg.0
IL_004c: ldfld float32 klass::intField5
IL_0051: stfld float32 klass::intField6
IL_0056: ldarg.0
IL_0057: ldfld float32 klass::intField6
IL_005c: stfld float32 klass::intField7
IL_0061: ldarg.0
IL_0062: ldfld float32 klass::intField7
IL_0067: stfld float32 klass::intField8
IL_006c: ldarg.0
IL_006d: ldfld float32 klass::intField8
IL_0072: stfld float32 klass::intField9
IL_0077: ret