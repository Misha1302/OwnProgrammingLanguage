.assembly extern System.Private.CoreLib {}
.assembly extern System.Console {}
.assembly extern mscorlib {}
.assembly extern System.Runtime {}
.assembly extern System.Console {}

.assembly Program
{
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}
.module Program.dll


.class public abstract auto ansi sealed beforefieldinit Program extends [mscorlib]System.Object
{
  .method public hidebysig static void Main(string[] args) cil managed
  {
    .entrypoint
    .locals ([0] string aStr, [1] float32 q, [2] float32 abs, [3] float32 cos, [4] float32 sin, [5] string a, [6] string b, [7] string c, [8] string absStr, [9] string cosStr, [10] string sinStr)
call string [System.Console]System.Console::ReadLine()
stloc.s aStr
ldloc.s aStr
call float32 [System.Private.CoreLib]System.Convert::ToSingle(string)
stloc.s q
ldloc.s q
call float32 [System.Private.CoreLib]System.MathF::Abs(float32)
stloc.s abs
ldloc.s q
call float32 [System.Private.CoreLib]System.MathF::Cos(float32)
stloc.s cos
ldloc.s q
call float32 [System.Private.CoreLib]System.MathF::Sin(float32)
stloc.s sin
ldloca.s abs
call instance string [System.Private.CoreLib]System.Single::ToString()
stloc.s a
ldloca.s cos
call instance string [System.Private.CoreLib]System.Single::ToString()
stloc.s b
ldloca.s sin
call instance string [System.Private.CoreLib]System.Single::ToString()
stloc.s c
ldstr "Abs: "
ldloc.s a
call string [System.Private.CoreLib]System.String::Concat(string, string)
stloc.s absStr
ldstr "Cos: "
ldloc.s b
call string [System.Private.CoreLib]System.String::Concat(string, string)
stloc.s cosStr
ldstr "Sin: "
ldloc.s c
call string [System.Private.CoreLib]System.String::Concat(string, string)
stloc.s sinStr
ldloc.s absStr
call void [System.Console]System.Console::WriteLine(string)
ldloc.s cosStr
call void [System.Console]System.Console::WriteLine(string)
ldloc.s sinStr
call void [System.Console]System.Console::WriteLine(string)
ldstr "Конец программы :>"
call void [System.Console]System.Console::WriteLine(string)
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
    ldc.i4.s     13 // enter key
    bne.un.s     loopPoint

    ret
  }
}