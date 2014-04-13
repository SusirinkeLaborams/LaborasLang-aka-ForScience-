System.Void klass::Test(System.Boolean)
// Method begins at RVA 0x2050
// Code size 1 (0x1)
.maxstack 8

IL_0000: ret


System.Void klass::dummy()
// Method begins at RVA 0x2054
// Code size 7 (0x7)
.maxstack 8
.entrypoint

IL_0000: ldc.i4.1
IL_0001: call void klass::Test(bool)
IL_0006: ret