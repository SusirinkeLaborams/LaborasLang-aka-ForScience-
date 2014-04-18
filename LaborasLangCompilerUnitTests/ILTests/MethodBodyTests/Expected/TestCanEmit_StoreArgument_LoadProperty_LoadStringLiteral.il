System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 11 (0xb)
.maxstack 8
.entrypoint

IL_0000: ldstr "Test"
IL_0005: call void klass::TestMethod(string)
IL_000a: ret


System.String klass::get_stringProperty()
// Method begins at RVA 0x205c
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldstr "Test"
IL_0005: ret


System.Void klass::TestMethod(System.String)
// Method begins at RVA 0x2064
// Code size 8 (0x8)
.maxstack 8

IL_0000: call string klass::get_stringProperty()
IL_0005: starg.s arg
IL_0007: ret