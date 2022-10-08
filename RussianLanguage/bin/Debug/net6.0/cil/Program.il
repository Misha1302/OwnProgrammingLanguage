.assembly extern System.Runtime { .ver 6:0:0:0 } 
.assembly extern System.Console { .ver 6:0:0:0 } 
.assembly extern mscorlib {}
.assembly Program { }
.module Program.dll

.class public auto Program extends [System.Runtime]System.Object
{    
  .method public static void main(string[] args) cil managed
  {
    .entrypoint
    .locals init ([0] string aStr, [1] string bStr, [2] float32 a, [3] float32 b, [4] string tmpA, [5] string tmpB)
call string [mscorlib]System.Console::ReadLine()
stloc.s aStr
call string [mscorlib]System.Console::ReadLine()
stloc.s bStr
ldloc.s aStr
call float32 [mscorlib]System.Convert::ToSingle(string)
stloc.s a
ldloc.s bStr
call float32 [mscorlib]System.Convert::ToSingle(string)
stloc.s b
ldloc.s a
ldc.i4 3
div
ldc.i4 2
add
stloc.s a
ldloc.s b
ldc.i4 4
div
ldc.i4 3
sub
stloc.s b
ldloc.s a
call string [mscorlib]System.Convert::ToString(float32)
stloc.s tmpA
ldloc.s b
call string [mscorlib]System.Convert::ToString(float32)
stloc.s tmpB
ldstr "a / 3 + 2 = "
ldloc.s tmpA
call string [mscorlib]System.String::Concat(string, string)
stloc.s aStr
ldstr "b / 4 - 3 = "
ldloc.s tmpB
call string [mscorlib]System.String::Concat(string, string)
stloc.s bStr
ldloc.s aStr
call void [mscorlib]System.Console::WriteLine(string)
ldloc.s bStr
call void [mscorlib]System.Console::WriteLine(string)
    call void Program::EndOfMain()
    ret
  }  
  
  .method private static void EndOfMain() cil managed
  {
    .locals init ([0] valuetype [System.Console]System.ConsoleKeyInfo key)
    ldstr "Press enter to exit"
    call void [mscorlib]System.Console::WriteLine(string)
    
    // считываем 
    loopPoint:
    ldc.i4.1
    call         valuetype [System.Console]System.ConsoleKeyInfo [System.Console]System.Console::ReadKey(bool)
    stloc.0

    ldloca.s     key
    call         instance valuetype [System.Console]System.ConsoleKey [System.Console]System.ConsoleKeyInfo::get_Key()
    ldc.i4.s     13 // enter
    bne.un.s     loopPoint

    ret
  }
}