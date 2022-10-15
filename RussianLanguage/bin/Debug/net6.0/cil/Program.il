.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly Program
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module Program.exe
// MVID: {304BE092-B12B-479A-A5B9-68902DA41433}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x04B30000


.class private auto ansi beforefieldinit Program
       extends [mscorlib]System.Object
{
  .method private hidebysig static void Main() cil managed
  {
    .entrypoint
    .locals init ([0] string aStr, [1] int32 a, [2] bool aLessOrEqualsZero, [3] string b, [4] string strOne)
call string [mscorlib]System.Console::ReadLine()
stloc.s aStr
ldloc.s aStr
call int32 [mscorlib]System.Convert::ToInt32(string)
stloc.s a
ldloc.s a
ldc.i4 0
clt
ldloc.s a
ldc.i4 0
ceq
or
stloc.s aLessOrEqualsZero
ldloca.s aLessOrEqualsZero
call instance string [mscorlib]System.Boolean::ToString()
stloc.s b
ldstr "a <= 0: "
ldloc.s b
call string [mscorlib]System.String::Concat(string, string)
stloc.s strOne
ldloc.s strOne
call void [mscorlib]System.Console::WriteLine(string)
    call void Program::EndOfMain()
    ret
  }  
  
  .method private static void EndOfMain() cil managed
  {
    .locals init ([0] valuetype [mscorlib]System.ConsoleKeyInfo key)
    ldstr "Press enter to exit"
    call void [mscorlib]System.Console::WriteLine(string)
    
    // считываем 
    loopPoint:
    ldc.i4.1
    call         valuetype [mscorlib]System.ConsoleKeyInfo [mscorlib]System.Console::ReadKey(bool)
    stloc.0

    ldloca.s     key
    call         instance valuetype [mscorlib]System.ConsoleKey [mscorlib]System.ConsoleKeyInfo::get_Key()
    ldc.i4.s     13 // enter
    bne.un.s     loopPoint

    ret
  }
}