System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 14 (0xe)
.maxstack 8
.entrypoint

IL_0000: ldsfld bool klass::myField
IL_0005: ldc.i4.0
IL_0006: ceq
IL_0008: stsfld bool klass::myField
IL_000d: ret