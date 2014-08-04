System.Void klass::dummy()
// Method begins at RVA 0x2050
// Code size 29 (0x1d)
.maxstack 8
.entrypoint

IL_0000: call class $Functors.$System_Void$System_Int32$System_String klass::get_MyFunction()
IL_0005: callvirt instance class $Functors.$System_Void$System_Int32$System_String/Delegate $Functors.$System_Void$System_Int32$System_String::AsDelegate()
IL_000a: ldftn instance void $Functors.$System_Void$System_Int32$System_String/Delegate::Invoke(int32, string)
IL_0010: newobj instance void klass/Delegate::.ctor(object, native int)
IL_0015: tail.
IL_0017: call void klass::set_MyDelegate(class klass/Delegate)
IL_001c: ret


System.Void klass::set_MyFunction($Functors.$System_Void$System_Int32$System_String)
// Method begins at RVA 0x2070
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: stsfld class $Functors.$System_Void$System_Int32$System_String klass::myFunction_BackingField
IL_0006: ret


$Functors.$System_Void$System_Int32$System_String klass::get_MyFunction()
// Method begins at RVA 0x2078
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldsfld class $Functors.$System_Void$System_Int32$System_String klass::myFunction_BackingField
IL_0005: ret


System.Void klass::set_MyDelegate(klass/Delegate)
// Method begins at RVA 0x2080
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: stsfld class klass/Delegate klass::myDelegate_BackingField
IL_0006: ret


klass/Delegate klass::get_MyDelegate()
// Method begins at RVA 0x2088
// Code size 6 (0x6)
.maxstack 8

IL_0000: ldsfld class klass/Delegate klass::myDelegate_BackingField
IL_0005: ret


System.Void $Functors.$System_Void$System_Int32$System_String::.ctor()
// Method begins at RVA 0x2090
// Code size 7 (0x7)
.maxstack 8

IL_0000: ldarg.0
IL_0001: call instance void [mscorlib]System.Object::.ctor()
IL_0006: ret