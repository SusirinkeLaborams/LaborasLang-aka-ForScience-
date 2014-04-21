System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 22 (0x16)
.maxstack 8
.entrypoint

IL_0000: ldc.r8 3.4
IL_0009: ldc.r4 5.5
IL_000e: conv.r8
IL_000f: sub
IL_0010: stsfld float64 klass::myField
IL_0015: ret