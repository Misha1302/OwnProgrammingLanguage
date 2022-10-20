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
    .locals (