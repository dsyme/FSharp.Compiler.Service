module internal FSharp.Compiler.Service.Tests.Common

open System
open System.IO
open System.Collections.Generic
open FSharp.Compiler
open FSharp.Compiler.SourceCodeServices

#if FX_RESHAPED_REFLECTION
open ReflectionAdapters
#endif

#if NETCOREAPP2_0
let readRefs (folder : string) (projectFile: string) =
    let runProcess (workingDir: string) (exePath: string) (args: string) =
        let psi = System.Diagnostics.ProcessStartInfo()
        psi.FileName <- exePath
        psi.WorkingDirectory <- workingDir
        psi.RedirectStandardOutput <- false
        psi.RedirectStandardError <- false
        psi.Arguments <- args
        psi.CreateNoWindow <- true
        psi.UseShellExecute <- false

        use p = new System.Diagnostics.Process()
        p.StartInfo <- psi
        p.Start() |> ignore
        p.WaitForExit()

        let exitCode = p.ExitCode
        exitCode, ()

    let projFilePath = Path.Combine(folder, projectFile)
    let runCmd exePath args = runProcess folder exePath (args |> String.concat " ")
    let msbuildExec = Dotnet.ProjInfo.Inspect.dotnetMsbuild runCmd
    let result = Dotnet.ProjInfo.Inspect.getProjectInfo ignore msbuildExec Dotnet.ProjInfo.Inspect.getFscArgs [] projFilePath
    match result with
    | Ok(Dotnet.ProjInfo.Inspect.GetResult.FscArgs x) ->
        x
        |> List.filter (fun s -> s.StartsWith("-r:", StringComparison.Ordinal))
        |> List.map (fun s -> s.Replace("-r:", ""))
    | _ -> []
#endif


// Create one global interactive checker instance
let checker = FSharpChecker.Create()

type TempFile(ext, contents) = 
    let tmpFile =  Path.ChangeExtension(System.IO.Path.GetTempFileName() , ext)
    do File.WriteAllText(tmpFile, contents)
    interface System.IDisposable with 
        member x.Dispose() = try File.Delete tmpFile with _ -> ()
    member x.Name = tmpFile

#nowarn "57"

let getBackgroundParseResultsForScriptText (input) = 
    use file =  new TempFile("fsx", input)
    let checkOptions, _diagnostics = checker.GetProjectOptionsFromScript(file.Name, input) |> Async.RunSynchronously
    checker.GetBackgroundParseResultsForFileInProject(file.Name, checkOptions)  |> Async.RunSynchronously


let getBackgroundCheckResultsForScriptText (input) = 
    use file =  new TempFile("fsx", input)
    let checkOptions, _diagnostics = checker.GetProjectOptionsFromScript(file.Name, input) |> Async.RunSynchronously
    checker.GetBackgroundCheckResultsForFileInProject(file.Name, checkOptions) |> Async.RunSynchronously


let sysLib nm = 
#if !NETCOREAPP2_0
    if System.Environment.OSVersion.Platform = System.PlatformID.Win32NT then // file references only valid on Windows 
        let programFilesx86Folder = System.Environment.GetEnvironmentVariable("PROGRAMFILES(X86)")
        programFilesx86Folder + @"\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.1\" + nm + ".dll"
    else
#endif
#if FX_NO_RUNTIMEENVIRONMENT
        let sysDir = System.AppContext.BaseDirectory
#else
        let sysDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
#endif
        let (++) a b = System.IO.Path.Combine(a,b)
        sysDir ++ nm + ".dll" 

[<AutoOpen>]
module Helpers = 
    open System
    type DummyType = A | B
    let PathRelativeToTestAssembly p = Path.Combine(Path.GetDirectoryName(Uri(typeof<FSharp.Compiler.SourceCodeServices.FSharpChecker>.Assembly.CodeBase).LocalPath), p)

let fsCoreDefaultReference() = 
    PathRelativeToTestAssembly "FSharp.Core.dll"

let mkStandardProjectReferences () = 
#if NETCOREAPP2_0
            let file = "Sample_NETCoreSDK_FSharp_Library_netstandard2_0.fsproj"
            let projDir = Path.Combine(__SOURCE_DIRECTORY__, "../projects/Sample_NETCoreSDK_FSharp_Library_netstandard2_0")
            readRefs projDir file
#else
            [ yield sysLib "mscorlib"
              yield sysLib "System"
              yield sysLib "System.Core"
              yield fsCoreDefaultReference() ]
#endif              

let mkProjectCommandLineArgsSilent (dllName, fileNames) = 
  let args = 
    [|  yield "--simpleresolution" 
        yield "--noframework" 
        yield "--debug:full" 
        yield "--define:DEBUG" 
#if NETCOREAPP
        yield "--targetprofile:netcore" 
#endif
        yield "--optimize-" 
        yield "--out:" + dllName
        yield "--doc:test.xml" 
        yield "--warn:3" 
        yield "--fullpaths" 
        yield "--flaterrors" 
        yield "--target:library" 
        for x in fileNames do 
            yield x
        let references = mkStandardProjectReferences ()
        for r in references do
            yield "-r:" + r
     |]
  args

let mkProjectCommandLineArgs (dllName, fileNames) = 
  let args = mkProjectCommandLineArgsSilent (dllName, fileNames)
  printfn "dllName = %A, args = %A" dllName args
  args

#if NETCOREAPP2_0
let mkProjectCommandLineArgsForScript (dllName, fileNames) = 
    [|  yield "--simpleresolution" 
        yield "--noframework" 
        yield "--debug:full" 
        yield "--define:DEBUG" 
        yield "--targetprofile:netcore" 
        yield "--optimize-" 
        yield "--out:" + dllName
        yield "--doc:test.xml" 
        yield "--warn:3" 
        yield "--fullpaths" 
        yield "--flaterrors" 
        yield "--target:library" 
        for x in fileNames do 
            yield x
        let references = mkStandardProjectReferences ()
        for r in references do
            yield "-r:" + r
     |]
#endif

let mkTestFileAndOptions source additionalArgs =
    let fileName = Path.ChangeExtension(Path.GetTempFileName(), ".fs")
    let project = Path.GetTempFileName()
    let dllName = Path.ChangeExtension(project, ".dll")
    let projFileName = Path.ChangeExtension(project, ".fsproj")
    File.WriteAllText(fileName, source)

    let args = Array.append (mkProjectCommandLineArgs (dllName, [fileName])) additionalArgs
    let options = checker.GetProjectOptionsFromCommandLineArgs (projFileName, args)
    fileName, options

let getProjectReferences (content, dllFiles) = 
    let fileName, options = 
      mkTestFileAndOptions content 
       [| for dllFile in dllFiles do
            yield "-r:"+dllFile |]
    let results = checker.ParseAndCheckProject(options) |> Async.RunSynchronously
    if results.HasCriticalErrors then
        let builder = new System.Text.StringBuilder()
        for err in results.Errors do
            builder.AppendLine(sprintf "**** %s: %s" (if err.Severity = FSharpErrorSeverity.Error then "error" else "warning") err.Message)
            |> ignore
        failwith (builder.ToString())
    let assemblies =
        results.ProjectContext.GetReferencedAssemblies()
        |> List.map(fun x -> x.SimpleName, x)
        |> dict
    results, assemblies

let parseAndCheckFile fileName source options =
    match checker.ParseAndCheckFileInProject(fileName, 0, source, options) |> Async.RunSynchronously with
    | parseResults, FSharpCheckFileAnswer.Succeeded(checkResults) -> parseResults, checkResults
    | _ -> failwithf "Parsing aborted unexpectedly..."

let parseAndCheckScript (file, input) = 

#if NETCOREAPP2_0
    let dllName = Path.ChangeExtension(file, ".dll")
    let projName = Path.ChangeExtension(file, ".fsproj")
    let args = mkProjectCommandLineArgsForScript (dllName, [file])
    printfn "file = %A, args = %A" file args
    let projectOptions = checker.GetProjectOptionsFromCommandLineArgs (projName, args)

#else    
    let projectOptions, _diagnostics = checker.GetProjectOptionsFromScript(file, input) |> Async.RunSynchronously
    printfn "projectOptions = %A" projectOptions
#endif

    let parseResult, typedRes = checker.ParseAndCheckFileInProject(file, 0, input, projectOptions) |> Async.RunSynchronously
    
    // if parseResult.Errors.Length > 0 then
    //     printfn "---> Parse Input = %A" input
    //     printfn "---> Parse Error = %A" parseResult.Errors

    match typedRes with
    | FSharpCheckFileAnswer.Succeeded(res) -> parseResult, res
    | res -> failwithf "Parsing did not finish... (%A)" res

let parseSource (source: string) =
    let location = Path.GetTempFileName()
    let filePath = Path.Combine(location, ".fs")
    let dllPath = Path.Combine(location, ".dll")

    let args = mkProjectCommandLineArgs(dllPath, [filePath])
    let options, errors = checker.GetParsingOptionsFromCommandLineArgs(List.ofArray args)
    let parseResults = checker.ParseFile(filePath, source, options) |> Async.RunSynchronously

    match parseResults.ParseTree with
    | Some parseTree -> parseTree
    | None -> failwithf "Expected there to be a parse tree for source:\n%s" source

/// Extract range info 
let tups (m:Range.range) = (m.StartLine, m.StartColumn), (m.EndLine, m.EndColumn)

/// Extract range info  and convert to zero-based line  - please don't use this one any more
let tupsZ (m:Range.range) = (m.StartLine-1, m.StartColumn), (m.EndLine-1, m.EndColumn)

let attribsOfSymbolUse (s:FSharpSymbolUse) = 
    [ if s.IsFromDefinition then yield "defn" 
      if s.IsFromType then yield "type"
      if s.IsFromAttribute then yield "attribute"
      if s.IsFromDispatchSlotImplementation then yield "override"
      if s.IsFromPattern then yield "pattern" 
      if s.IsFromComputationExpression then yield "compexpr" ] 

let attribsOfSymbol (s:FSharpSymbol) = 
    [ match s with 
        | :? FSharpField as v -> 
            yield "field"
            if v.IsCompilerGenerated then yield "compgen"
            if v.IsDefaultValue then yield "default"
            if v.IsMutable then yield "mutable"
            if v.IsVolatile then yield "volatile"
            if v.IsStatic then yield "static"
            if v.IsLiteral then yield sprintf "%A" v.LiteralValue.Value
            if v.IsAnonRecordField then 
                let info, tys, i = v.AnonRecordFieldDetails
                yield "anon(" + string i + ", [" + info.Assembly.QualifiedName + "/" + String.concat "+" info.EnclosingCompiledTypeNames + "/" + info.CompiledName + "]" + String.concat "," info.SortedFieldNames + ")"


        | :? FSharpEntity as v -> 
            v.TryFullName |> ignore // check there is no failure here
            if v.IsNamespace then yield "namespace"
            if v.IsFSharpModule then yield "module"
            if v.IsByRef then yield "byref"
            if v.IsClass then yield "class"
            if v.IsDelegate then yield "delegate"
            if v.IsEnum then yield "enum"
            if v.IsFSharpAbbreviation then yield "abbrev"
            if v.IsFSharpExceptionDeclaration then yield "exn"
            if v.IsFSharpRecord then yield "record"
            if v.IsFSharpUnion then yield "union"
            if v.IsInterface then yield "interface"
            if v.IsMeasure then yield "measure"
#if !NO_EXTENSIONTYPING
            if v.IsProvided then yield "provided"
            if v.IsStaticInstantiation then yield "staticinst"
            if v.IsProvidedAndErased then yield "erased"
            if v.IsProvidedAndGenerated then yield "generated"
#endif
            if v.IsUnresolved then yield "unresolved"
            if v.IsValueType then yield "valuetype"

        | :? FSharpMemberOrFunctionOrValue as v -> 
            if v.IsActivePattern then yield "apat"
            if v.IsDispatchSlot then yield "slot"
            if v.IsModuleValueOrMember && not v.IsMember then yield "val"
            if v.IsMember then yield "member"
            if v.IsProperty then yield "prop"
            if v.IsExtensionMember then yield "extmem"
            if v.IsPropertyGetterMethod then yield "getter"
            if v.IsPropertySetterMethod then yield "setter"
            if v.IsEvent then yield "event"
            if v.EventForFSharpProperty.IsSome then yield "clievent"
            if v.IsEventAddMethod then yield "add"
            if v.IsEventRemoveMethod then yield "remove"
            if v.IsTypeFunction then yield "typefun"
            if v.IsCompilerGenerated then yield "compgen"
            if v.IsImplicitConstructor then yield "ctor"
            if v.IsMutable then yield "mutable" 
            if v.IsOverrideOrExplicitInterfaceImplementation then yield "overridemem"
            if v.IsInstanceMember && not v.IsInstanceMemberInCompiledCode && not v.IsExtensionMember then yield "funky"
            if v.IsExplicitInterfaceImplementation then yield "intfmem"
//            if v.IsConstructorThisValue then yield "ctorthis"
//            if v.IsMemberThisValue then yield "this"
//            if v.LiteralValue.IsSome then yield "literal"
        | _ -> () ]

let rec allSymbolsInEntities compGen (entities: IList<FSharpEntity>) = 
    [ for e in entities do 
          yield (e :> FSharpSymbol) 
          for gp in e.GenericParameters do 
            if compGen || not gp.IsCompilerGenerated then 
             yield (gp :> FSharpSymbol)
          for x in e.MembersFunctionsAndValues do
             if compGen || not x.IsCompilerGenerated then 
               yield (x :> FSharpSymbol)
             for gp in x.GenericParameters do 
              if compGen || not gp.IsCompilerGenerated then 
               yield (gp :> FSharpSymbol)
          for x in e.UnionCases do
             yield (x :> FSharpSymbol)
             for f in x.UnionCaseFields do
                 if compGen || not f.IsCompilerGenerated then 
                     yield (f :> FSharpSymbol)
          for x in e.FSharpFields do
             if compGen || not x.IsCompilerGenerated then 
                 yield (x :> FSharpSymbol)
          yield! allSymbolsInEntities compGen e.NestedEntities ]


let coreLibAssemblyName =
#if NETCOREAPP2_0
    "System.Runtime"
#else
    "mscorlib"
#endif

