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
    @<file>                                   Specifies a response file to be parsed for input files and command line options (one per line)
    /? /h                                     This message    

EXE/WinEXE specific options:
    /main:main_file.py                        Main file of the project (module to be executed first)
    /platform:x86                             Compile for x86 only
    /platform:x64                             Compile for x64 only
    /embed                                    Embeds the generated DLL as a resource into the executable which is loaded at runtime
    /standalone                               Embeds the IronPython assemblies into the stub executable.
    /mta                                      Set MTAThreadAttribute on Main instead of STAThreadAttribute, only valid for /target:winexe

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

def GenerateExe(config):
    """generates the stub .EXE file for starting the app"""
    aName = AssemblyName(System.IO.FileInfo(config.output).Name)
    ab = PythonOps.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndSave)
    mb = ab.DefineDynamicModule(config.output,  aName.Name + ".exe")
    tb = mb.DefineType("PythonMain", TypeAttributes.Public)
    assemblyResolveMethod = None

    if config.standalone:
        print "Generating stand alone executable"
        config.embed = True
        
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
    if config.target == System.Reflection.Emit.PEFileKinds.WindowApplication and config.mta:
        mainMethod.SetCustomAttribute(clr.GetClrType(System.MTAThreadAttribute).GetConstructor(()), System.Array[System.Byte](()))
    elif config.target == System.Reflection.Emit.PEFileKinds.WindowApplication:
        mainMethod.SetCustomAttribute(clr.GetClrType(System.STAThreadAttribute).GetConstructor(()), System.Array[System.Byte](()))

    gen = mainMethod.GetILGenerator()

    # get the ScriptCode assembly...
    if config.embed:
        # put the generated DLL into the resources for the stub exe
        w = mb.DefineResource("IPDll.resources", "Embedded IronPython Generated DLL")
        w.AddResource("IPDll." + config.output, System.IO.File.ReadAllBytes(config.output + ".dll"))
        System.IO.File.Delete(config.output + ".dll")

        # generate code to load the resource
        gen.Emit(OpCodes.Ldstr, "IPDll")
        gen.EmitCall(OpCodes.Call, clr.GetClrType(Assembly).GetMethod("GetEntryAssembly"), ())
        gen.Emit(OpCodes.Newobj, clr.GetClrType(System.Resources.ResourceManager).GetConstructor((str, clr.GetClrType(Assembly))))
        gen.Emit(OpCodes.Ldstr, "IPDll." + config.output)
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
        gen.Emit(OpCodes.Ldstr, config.output + ".dll")
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
    ab.SetEntryPoint(mainMethod, config.target)
    ab.Save(aName.Name + ".exe", config.platform, config.machine)

class Config(object):
    def __init__(self):
        self.output = None
        self.main = None
        self.main_name = None
        self.target = System.Reflection.Emit.PEFileKinds.Dll
        self.embed = False
        self.standalone = False
        self.mta = False
        self.platform = System.Reflection.PortableExecutableKinds.ILOnly
        self.machine = System.Reflection.ImageFileMachine.I386
        self.files = []

    def ParseArgs(self, args, respFiles=[]):
        for arg in args:
            arg = arg.strip()
            if arg.startswith("#"):
                continue

            if arg.startswith("/main:"):
                self.main_name = self.main = arg[6:]
                # only override the target kind if its current a DLL
                if self.target == System.Reflection.Emit.PEFileKinds.Dll:
                    self.target = System.Reflection.Emit.PEFileKinds.ConsoleApplication

            elif arg.startswith("/out:"):
                self.output = arg[5:]

            elif arg.startswith("/target:"):
                tgt = arg[8:]
                if tgt == "exe": self.target = System.Reflection.Emit.PEFileKinds.ConsoleApplication
                elif tgt == "winexe": self.target = System.Reflection.Emit.PEFileKinds.WindowApplication
                else: self.target = System.Reflection.Emit.PEFileKinds.Dll

            elif arg.startswith("/platform:"):
                pform = arg[10:]
                if pform == "x86":
                    self.platform = System.Reflection.PortableExecutableKinds.ILOnly | System.Reflection.PortableExecutableKinds.Required32Bit
                    self.machine  = System.Reflection.ImageFileMachine.I386
                elif pform == "x64":
                    self.platform = System.Reflection.PortableExecutableKinds.ILOnly | System.Reflection.PortableExecutableKinds.PE32Plus
                    self.machine  = System.Reflection.ImageFileMachine.AMD64
                else:
                    self.platform = System.Reflection.PortableExecutableKinds.ILOnly
                    self.machine  = System.Reflection.ImageFileMachine.I386

            elif arg.startswith("/embed"):
                self.embed = True

            elif arg.startswith("/standalone"):
                self.standalone = True

            elif arg.startswith("/mta"):
                self.mta = True

            elif arg in ["/?", "-?", "/h", "-h"]:
                print __doc__
                sys.exit(0)

            else:
                if arg.startswith("@"):
                    respFile = System.IO.Path.GetFullPath(arg[1:])
                    if not respFile in respFiles:
                        respFiles.append(respFile)
                        with open(respFile, 'r') as f:
                           self.ParseArgs(f.readlines(), respFiles)
                    else:
                        print "WARNING: Already parsed response file '%s'\n" % arg[1:]
                else:
                    self.files.append(arg)

    def Validate(self):
        if not self.files and not self.main_name:
            print "No files or main defined"
            return False

        if self.target != System.Reflection.Emit.PEFileKinds.Dll and self.main_name == None:
            print "EXEs require /main:<filename> to be specified"
            return False

        if not self.output and self.main_name:
            self.output = System.IO.Path.GetFileNameWithoutExtension(self.main_name)
        elif not self.output and self.files:
            self.output = System.IO.Path.GetFileNameWithoutExtension(self.files[0])

        return True

    def __repr__(self):
        res = "Input Files:\n"
        for file in self.files:
            res += "\t%s\n" % file

        res += "Output:\n\t%s\n" % self.output
        res += "Target:\n\t%s\n" % self.target
        res += "Platform:\n\t%s\n" % self.platform
        res += "Machine:\n\t%s\n" % self.machine

        if self.target == System.Reflection.Emit.PEFileKinds.WindowApplication:
            res += "Threading:\n"
            if self.mta:
                res += "\tMTA\n"
            else:
                res += "\tSTA\n"        
        return res

def Main(args):
    files = []
    config = Config()
    
    config.ParseArgs(args)
    if not config.Validate():
        print __doc__
        sys.exit(0)

    print config
    
    print "Compiling..."
    clr.CompileModules(config.output + ".dll", mainModule = config.main_name, *config.files)

    if config.target != System.Reflection.Emit.PEFileKinds.Dll:
        GenerateExe(config)

    print "Saved to %s" % (config.output, )

if __name__ == "__main__":
    Main(sys.argv[1:])
