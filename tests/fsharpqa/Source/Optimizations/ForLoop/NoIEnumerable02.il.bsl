
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.0.30319.1
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern FSharp.Core
{
  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )                         // .?_....:
  .ver 4:0:0:0
}
.assembly NoIEnumerable02
{
  .custom instance void [FSharp.Core]Microsoft.FSharp.Core.FSharpInterfaceDataVersionAttribute::.ctor(int32,
                                                                                                      int32,
                                                                                                      int32) = ( 01 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 ) 

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 03 00 00 00 00 00 ) 

  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.mresource public FSharpSignatureData.NoIEnumerable02
{
  // Offset: 0x00000000 Length: 0x000001E5
}
.mresource public FSharpOptimizationData.NoIEnumerable02
{
  // Offset: 0x000001F0 Length: 0x0000006C
}
.module NoIEnumerable02.dll
// MVID: {4BEB29BD-5066-4012-A745-0383BD29EB4B}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x00450000


// =============== CLASS MEMBERS DECLARATION ===================

.class public abstract auto ansi sealed M
       extends [mscorlib]System.Object
{
  .custom instance void [FSharp.Core]Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype [FSharp.Core]Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 ) 
  .method public static void  loop2(int32 N) cil managed
  {
    // Code size       44 (0x2c)
    .maxstack  5
    .locals init ([0] int32 V_0,
             [1] int32 i,
             [2] class [FSharp.Core]Microsoft.FSharp.Core.PrintfFormat`4<class [FSharp.Core]Microsoft.FSharp.Core.Unit,class [mscorlib]System.IO.TextWriter,class [FSharp.Core]Microsoft.FSharp.Core.Unit,class [FSharp.Core]Microsoft.FSharp.Core.Unit> V_2)
    .language '{AB4F38C9-B6E6-43BA-BE3B-58080B2CCCE3}', '{994B45C4-E6E9-11D2-903F-00C04FA302A1}', '{5A869D0B-6611-11D3-BD2A-0000F80849BD}'
    .line 7,7 : 4,24 
    IL_0000:  nop
    IL_0001:  ldc.i4.s   100
    IL_0003:  stloc.1
    IL_0004:  ldarg.0
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldloc.1
    IL_0008:  blt.s      IL_002b

    .line 8,8 : 7,20 
    IL_000a:  ldstr      "aaa"
    IL_000f:  newobj     instance void class [FSharp.Core]Microsoft.FSharp.Core.PrintfFormat`5<class [FSharp.Core]Microsoft.FSharp.Core.Unit,class [mscorlib]System.IO.TextWriter,class [FSharp.Core]Microsoft.FSharp.Core.Unit,class [FSharp.Core]Microsoft.FSharp.Core.Unit,class [FSharp.Core]Microsoft.FSharp.Core.Unit>::.ctor(string)
    IL_0014:  stloc.2
    IL_0015:  call       class [mscorlib]System.IO.TextWriter [mscorlib]System.Console::get_Out()
    IL_001a:  ldloc.2
    IL_001b:  call       !!0 [FSharp.Core]Microsoft.FSharp.Core.PrintfModule::PrintFormatLineToTextWriter<class [FSharp.Core]Microsoft.FSharp.Core.Unit>(class [mscorlib]System.IO.TextWriter,
                                                                                                                                                         class [FSharp.Core]Microsoft.FSharp.Core.PrintfFormat`4<!!0,class [mscorlib]System.IO.TextWriter,class [FSharp.Core]Microsoft.FSharp.Core.Unit,class [FSharp.Core]Microsoft.FSharp.Core.Unit>)
    IL_0020:  pop
    IL_0021:  ldloc.1
    IL_0022:  ldc.i4.1
    IL_0023:  add
    IL_0024:  stloc.1
    .line 7,7 : 20,21 
    IL_0025:  ldloc.1
    IL_0026:  ldloc.0
    IL_0027:  ldc.i4.1
    IL_0028:  add
    IL_0029:  bne.un.s   IL_000a

    IL_002b:  ret
  } // end of method M::loop2

} // end of class M

.class private abstract auto ansi sealed '<StartupCode$NoIEnumerable02>'.$M$fsx
       extends [mscorlib]System.Object
{
} // end of class '<StartupCode$NoIEnumerable02>'.$M$fsx


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************