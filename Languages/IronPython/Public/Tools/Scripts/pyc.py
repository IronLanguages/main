#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A
# copy of the license can be found in the License.html file at the root of this distribution. If
# you cannot locate the  Apache License, Version 2.0, please send an email to
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

"""
pyc: The Command-Line Python Compiler

Usage: ipy.exe pyc.py [options] file [file ...]

Options:
    /out:output_file                          Output file name (default is main_file.<extenstion>)
    /target:dll                               Compile only into dll.  Default
    /target:exe                               Generate console executable stub for startup in addition to dll.
    /target:winexe                            Generate windows executable stub for startup in addition to dll.
    /? /h                                     This message

EXE/WinEXE specific options:
    /main:main_file.py                        Main file of the project (module to be executed first)
    /platform:x86                             Compile for x86 only
    /platform:x64                             Compile for x64 only
    /embed                                    Embeds the generated DLL as a resource into the executable which is loaded at runtime
    /standalone                               Embeds the IronPython assemblies into the stub executable.

Example:
    ipy.exe pyc.py /main:Program.py Form.py /target:winexe
"""

import sys
import clr
clr.AddReferenceByPartialName("IronPython")

from System.Collections.Generic import List
import IronPython.Hosting as Hosting
from IronPython.Runtime.Operations import PythonOps
import System
from System.Reflection import Emit, Assembly
from System.Reflection.Emit import OpCodes, AssemblyBuilderAccess
from System.Reflection import AssemblyName, TypeAttributes, MethodAttributes, ResourceAttributes, CallingConventions

def GenerateExe(name, targetKind, platform, machine, main_module, embed, standalone):
    """generates the stub .EXE file for starting the app"""
    aName = AssemblyName(System.IO.FileInfo(name).Name)
    ab = PythonOps.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave)
    mb = ab.DefineDynamicModule(name,  aName.Name + ".exe")
    tb = mb.DefineType("PythonMain", TypeAttributes.Public)
    assemblyResolveMethod = None

    if standalone:
        print "Generating stand alone executable"
        embed = True
        
        for a in System.AppDomain.CurrentDomain.GetAssemblies():
            n = AssemblyName(a.FullName)
            if not a.IsDynamic and not a.EntryPoint and (n.Name.StartsWith("IronPython") or n.Name in ['Microsoft.Dynamic', 'Microsoft.Scripting']):                
                print "\tEmbedding %s %s" % (n.Name, str(n.Version))
                f = System.IO.FileStream(a.Location, System.IO.FileMode.Open, System.IO.FileAccess.Read)
                mb.DefineManifestResource("Dll." + n.Name, f, ResourceAttributes.Public)

        # we currently do no error checking on what is passed in to the assemblyresolve event handler
        assemblyResolveMethod = tb.DefineMethod("AssemblyResolve", MethodAttributes.Public | MethodAttributes.Static, clr.GetClrType(Assembly), (clr.GetClrType(System.Object), clr.GetClrType(System.ResolveEventArgs)))
        gen = assemblyResolveMethod.GetILGenerator()
        s = gen.DeclareLocal(clr.GetClrType(System.IO.Stream)) # resource stream
        gen.Emit(OpCodes.Ldnull)
        gen.Emit(OpCodes.Stloc, s)
        d = gen.DeclareLocal(clr.GetClrType(System.Array[System.Byte])) # data buffer
        gen.EmitCall(OpCodes.Call, clr.GetClrType(Assembly).GetMethod("GetEntryAssembly"), ())
        gen.Emit(OpCodes.Ldstr, "Dll.")
        gen.Emit(OpCodes.Ldarg_1)    # The event args
        gen.EmitCall(OpCodes.Callvirt, clr.GetClrType(System.ResolveEventArgs).GetMethod("get_Name"), ())
        gen.Emit(OpCodes.Newobj, clr.GetClrType(AssemblyName).GetConstructor((str, )))
        gen.EmitCall(OpCodes.Call, clr.GetClrType(AssemblyName).GetMethod("get_Name"), ())
        gen.EmitCall(OpCodes.Call, clr.GetClrType(str).GetMethod("Concat", (str, str)), ())
        gen.EmitCall(OpCodes.Callvirt, clr.GetClrType(Assembly).GetMethod("GetManifestResourceStream", (str, )), ())
        gen.Emit(OpCodes.Stloc, s)
        gen.Emit(OpCodes.Ldloc, s)
        gen.EmitCall(OpCodes.Callvirt, clr.GetClrType(System.IO.Stream).GetMethod("get_Length"), ())
        gen.Emit(OpCodes.Newarr, clr.GetClrType(System.Byte))
        gen.Emit(OpCodes.Stloc, d)
        gen.Emit(OpCodes.Ldloc, s)
        gen.Emit(OpCodes.Ldloc, d)
        gen.Emit(OpCodes.Ldc_I4_0)
        gen.Emit(OpCodes.Ldloc, s)
        gen.EmitCall(OpCodes.Callvirt, clr.GetClrType(System.IO.Stream).GetMethod("get_Length"), ())
        gen.Emit(OpCodes.Conv_I4)
        gen.EmitCall(OpCodes.Callvirt, clr.GetClrType(System.IO.Stream).GetMethod("Read", (clr.GetClrType(System.Array[System.Byte]), int, int)), ())
        gen.Emit(OpCodes.Pop)
        gen.Emit(OpCodes.Ldloc, d)
        gen.EmitCall(OpCodes.Call, clr.GetClrType(Assembly).GetMethod("Load", (clr.GetClrType(System.Array[System.Byte]), )), ())
        gen.Emit(OpCodes.Ret)

        # generate a static constructor to assign the AssemblyResolve handler (otherwise it tries to use IronPython before it adds the handler)
        # the other way of handling this would be to move the call to InitializeModule into a separate method.
        staticConstructor = tb.DefineConstructor(MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, System.Type.EmptyTypes)
        gen = staticConstructor.GetILGenerator()
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.AppDomain).GetMethod("get_CurrentDomain"), ())
        gen.Emit(OpCodes.Ldnull)
        gen.Emit(OpCodes.Ldftn, assemblyResolveMethod)
        gen.Emit(OpCodes.Newobj, clr.GetClrType(System.ResolveEventHandler).GetConstructor((clr.GetClrType(System.Object), clr.GetClrType(System.IntPtr))))
        gen.EmitCall(OpCodes.Callvirt, clr.GetClrType(System.AppDomain).GetMethod("add_AssemblyResolve"), ())
        gen.Emit(OpCodes.Ret)        

    mainMethod = tb.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, int, ())
    if targetKind == System.Reflection.Emit.PEFileKinds.WindowApplication:
        mainMethod.SetCustomAttribute(clr.GetClrType(System.STAThreadAttribute).GetConstructor(()), System.Array[System.Byte](()))
    gen = mainMethod.GetILGenerator()

    # get the ScriptCode assembly...
    if embed:
        # put the generated DLL into the resources for the stub exe
        w = mb.DefineResource("IPDll.resources", "Embedded IronPython Generated DLL")
        w.AddResource("IPDll." + name, System.IO.File.ReadAllBytes(name + ".dll"))
        System.IO.File.Delete(name + ".dll")

        # generate code to load the resource
        gen.Emit(OpCodes.Ldstr, "IPDll")
        gen.EmitCall(OpCodes.Call, clr.GetClrType(Assembly).GetMethod("GetEntryAssembly"), ())
        gen.Emit(OpCodes.Newobj, clr.GetClrType(System.Resources.ResourceManager).GetConstructor((str, clr.GetClrType(Assembly))))
        gen.Emit(OpCodes.Ldstr, "IPDll." + name)
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.Resources.ResourceManager).GetMethod("GetObject", (str, )), ())
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.Reflection.Assembly).GetMethod("Load", (clr.GetClrType(System.Array[System.Byte]), )), ())
    else:
        # variables for saving original working directory und return code of script
        wdSave = gen.DeclareLocal(str)

        # save current working directory
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.Environment).GetMethod("get_CurrentDirectory"), ())
        gen.Emit(OpCodes.Stloc, wdSave)
        gen.EmitCall(OpCodes.Call, clr.GetClrType(Assembly).GetMethod("GetEntryAssembly"), ())
        gen.EmitCall(OpCodes.Callvirt, clr.GetClrType(Assembly).GetMethod("get_Location"), ())
        gen.Emit(OpCodes.Newobj, clr.GetClrType(System.IO.FileInfo).GetConstructor( (str, ) ))
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.IO.FileInfo).GetMethod("get_Directory"), ())
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.IO.DirectoryInfo).GetMethod("get_FullName"), ())
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.Environment).GetMethod("set_CurrentDirectory"), ())
        gen.Emit(OpCodes.Ldstr, name + ".dll")
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.IO.Path).GetMethod("GetFullPath", (clr.GetClrType(str), )), ())
        # result of GetFullPath stays on the stack during the restore of the
        # original working directory

        # restore original working directory
        gen.Emit(OpCodes.Ldloc, wdSave)
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.Environment).GetMethod("set_CurrentDirectory"), ())

        # for the LoadFile() call, the full path of the assembly is still is on the stack
        # as the result from the call to GetFullPath()
        gen.EmitCall(OpCodes.Call, clr.GetClrType(System.Reflection.Assembly).GetMethod("LoadFile", (clr.GetClrType(str), )), ())

    # emit module name
    gen.Emit(OpCodes.Ldstr, "__main__")  # main module name
    gen.Emit(OpCodes.Ldnull)             # no references
    gen.Emit(OpCodes.Ldc_I4_0)           # don't ignore environment variables for engine startup

    # call InitializeModule
    # (this will also run the script)
    gen.EmitCall(OpCodes.Call, clr.GetClrType(PythonOps).GetMethod("InitializeModuleEx"), ())
    gen.Emit(OpCodes.Ret)
    tb.CreateType()
    ab.SetEntryPoint(mainMethod, targetKind)
    ab.Save(aName.Name + ".exe", platform, machine)

def Main(args):
    files = []
    main = None          # The main file to start the execution (passed to the PythonCompiler)
    main_name = None     # File which will drive the name of the assembly if "output" not provided
    output = None        # Output assembly name
    target = System.Reflection.Emit.PEFileKinds.Dll
    platform = System.Reflection.PortableExecutableKinds.ILOnly
    machine  = System.Reflection.ImageFileMachine.I386
    embed = False        # True to embed the generated DLL into the executable
    standalone = False   # True to embed all the IronPython and Microsoft.Scripting DLL's into the generated exe

    for arg in args:
        if arg.startswith("/main:"):
            main_name = main = arg[6:]
            # only override the target kind if its current a DLL
            if target == System.Reflection.Emit.PEFileKinds.Dll:
                target = System.Reflection.Emit.PEFileKinds.ConsoleApplication

        elif arg.startswith("/out:"):
            output = arg[5:]

        elif arg.startswith("/target:"):
            tgt = arg[8:]
            if tgt == "exe": target = System.Reflection.Emit.PEFileKinds.ConsoleApplication
            elif tgt == "winexe": target = System.Reflection.Emit.PEFileKinds.WindowApplication
            else: target = System.Reflection.Emit.PEFileKinds.Dll

        elif arg.startswith("/platform:"):
            pform = arg[10:]
            if pform == "x86":
                platform = System.Reflection.PortableExecutableKinds.ILOnly | System.Reflection.PortableExecutableKinds.Required32Bit
                machine  = System.Reflection.ImageFileMachine.I386
            elif pform == "x64":
                platform = System.Reflection.PortableExecutableKinds.ILOnly | System.Reflection.PortableExecutableKinds.PE32Plus
                machine  = System.Reflection.ImageFileMachine.AMD64
            else:
                platform = System.Reflection.PortableExecutableKinds.ILOnly
                machine  = System.Reflection.ImageFileMachine.I386

        elif arg.startswith("/embed"):
            embed = True

        elif arg.startswith("/standalone"):
            standalone = True

        elif arg in ["/?", "-?", "/h", "-h"]:
            print __doc__
            sys.exit(0)

        else:
            files.append(arg)

    if not files and not main_name:
        print __doc__
        sys.exit(0)

    if target != System.Reflection.Emit.PEFileKinds.Dll and main_name == None:
        print __doc__
        sys.exit(0)
        print "EXEs require /main:<filename> to be specified"

    if not output and main_name:
        output = System.IO.Path.GetFileNameWithoutExtension(main_name)
    elif not output and files:
        output = System.IO.Path.GetFileNameWithoutExtension(files[0])

    print "Input Files:"
    for file in files:
        print "\t%s" % file

    print "Output:\n\t%s" % output
    print "Target:\n\t%s" % target
    print "Platform:\n\t%s" % platform
    print "Machine:\n\t%s" % machine

    print "Compiling..."
    clr.CompileModules(output + ".dll", mainModule = main_name, *files)

    if target != System.Reflection.Emit.PEFileKinds.Dll:
        GenerateExe(output, target, platform, machine, main_name, embed, standalone)

    print "Saved to %s" % (output, )

if __name__ == "__main__":
    Main(sys.argv[1:])
