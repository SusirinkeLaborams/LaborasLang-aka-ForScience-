System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 13 (0xd)
.maxstack 8
.entrypoint

IL_0000: ldstr "Test"
IL_0005: tail.
IL_0007: call void klass::TestMethod(string)
IL_000c: ret


System.String klass::get_stringProperty()
// Method begins at RVA 0x2060
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldstr "Test"
IL_0005: ret


System.Void klass::TestMethod(System.String)
// Method begins at RVA 0x2068
// Code size 10 (0xa)
.maxstack 8

IL_0000: call string klass::get_stringProperty()
IL_0005: starg arg
IL_0009: ret