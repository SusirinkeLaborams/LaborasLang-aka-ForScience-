System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8
.entrypoint

IL_0000: ret


System.Void klass::set_testProperty(System.Boolean)
// Method begins at RVA 0x2054
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: stsfld bool klass::testProperty_backingField
IL_0006: ret


System.Void klass::.cctor()
// Method begins at RVA 0x205c
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldc.i4.1
IL_0001: call void klass::set_testProperty(bool)
IL_0006: ret