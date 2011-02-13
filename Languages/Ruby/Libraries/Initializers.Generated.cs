/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#pragma warning disable 169 // mcs: unused private method
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.Builtins.BuiltinsLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Threading.ThreadingLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Sockets.SocketsLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.OpenSsl.OpenSslLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Digest.DigestLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Zlib.ZlibLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.StringIO.StringIOLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.StringScanner.StringScannerLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Enumerator.EnumeratorLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.FunctionControl.FunctionControlLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.FileControl.FileControlLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.BigDecimal.BigDecimalLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Iconv.IconvLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.ParseTree.ParseTreeLibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Open3.Open3LibraryInitializer))]
[assembly: IronRuby.Runtime.RubyLibraryAttribute(typeof(IronRuby.StandardLibrary.Win32API.Win32APILibraryInitializer))]

namespace IronRuby.Builtins {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class BuiltinsLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            Context.RegisterPrimitives(
                Load__MainSingleton_Instance,
                LoadBasicObject_Instance, LoadBasicObject_Class, null,
                LoadKernel_Instance, LoadKernel_Class, null,
                LoadObject_Instance, LoadObject_Class, LoadObject_Constants,
                LoadModule_Instance, LoadModule_Class, null,
                LoadClass_Instance, LoadClass_Class, null
            );
            
            
            // Skipped primitive: __MainSingleton
            // Skipped primitive: BasicObject
            IronRuby.Builtins.RubyModule def58 = DefineGlobalModule("Comparable", typeof(IronRuby.Builtins.Comparable), 0x0000000F, LoadComparable_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def48 = DefineGlobalModule("Enumerable", typeof(IronRuby.Builtins.Enumerable), 0x0000000F, LoadEnumerable_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def2 = DefineGlobalModule("Errno", typeof(IronRuby.Builtins.Errno), 0x0000000F, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def36 = DefineModule("File::Constants", typeof(IronRuby.Builtins.RubyFileOps.Constants), 0x0000000F, null, null, LoadFile__Constants_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalModule("FileTest", typeof(IronRuby.Builtins.FileTest), 0x0000000F, LoadFileTest_Instance, LoadFileTest_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalModule("GC", typeof(IronRuby.Builtins.RubyGC), 0x0000000F, LoadGC_Instance, LoadGC_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def38 = DefineModule("IO::WaitReadable", typeof(IronRuby.Builtins.RubyIOOps.WaitReadable), 0x0000000F, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def39 = DefineModule("IO::WaitWritable", typeof(IronRuby.Builtins.RubyIOOps.WaitWritable), 0x0000000F, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def32 = DefineGlobalModule("IronRuby", typeof(IronRuby.Ruby), 0x00000004, null, LoadIronRuby_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def4 = DefineModule("IronRuby::Clr", typeof(IronRuby.Builtins.IronRubyOps.Clr), 0x00000008, null, LoadIronRuby__Clr_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def5 = DefineModule("IronRuby::Clr::BigInteger", typeof(IronRuby.Builtins.ClrBigInteger), 0x0000000F, LoadIronRuby__Clr__BigInteger_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def29 = DefineModule("IronRuby::Clr::FlagEnumeration", typeof(IronRuby.Builtins.FlagEnumeration), 0x00000008, LoadIronRuby__Clr__FlagEnumeration_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def6 = DefineModule("IronRuby::Clr::Float", typeof(IronRuby.Builtins.ClrFloat), 0x0000000F, LoadIronRuby__Clr__Float_Instance, LoadIronRuby__Clr__Float_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def7 = DefineModule("IronRuby::Clr::Integer", typeof(IronRuby.Builtins.ClrInteger), 0x0000000F, LoadIronRuby__Clr__Integer_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def33 = DefineModule("IronRuby::Clr::MultiDimensionalArray", typeof(IronRuby.Builtins.MultiDimensionalArray), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def8 = DefineModule("IronRuby::Clr::String", typeof(IronRuby.Builtins.ClrString), 0x0000000F, LoadIronRuby__Clr__String_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def34 = DefineModule("IronRuby::Print", typeof(IronRuby.Builtins.PrintOps), 0x0000000F, LoadIronRuby__Print_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            // Skipped primitive: Kernel
            DefineGlobalModule("Marshal", typeof(IronRuby.Builtins.RubyMarshal), 0x0000000F, null, LoadMarshal_Class, LoadMarshal_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalModule("Math", typeof(IronRuby.Builtins.RubyMath), 0x0000000F, LoadMath_Instance, LoadMath_Class, LoadMath_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(Microsoft.Scripting.Actions.TypeTracker), 0x00000000, null, LoadMicrosoft__Scripting__Actions__TypeTracker_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalModule("ObjectSpace", typeof(IronRuby.Builtins.ObjectSpace), 0x0000000F, null, LoadObjectSpace_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def55 = DefineGlobalModule("Precision", typeof(IronRuby.Builtins.Precision), 0x0000000F, LoadPrecision_Instance, LoadPrecision_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyModule def40 = DefineGlobalModule("Process", typeof(IronRuby.Builtins.RubyProcess), 0x0000000F, LoadProcess_Instance, LoadProcess_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT
            DefineGlobalModule("Signal", typeof(IronRuby.Builtins.Signal), 0x0000000F, null, LoadSignal_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            ExtendClass(typeof(System.Type), 0x00000000, null, LoadSystem__Type_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #if !SILVERLIGHT
            object def1 = DefineSingleton(Load__Singleton_ArgFilesSingletonOps_Instance, null, null, def48);
            #endif
            object def12 = DefineSingleton(Load__Singleton_EnvironmentSingletonOps_Instance, null, null, def48);
            ExtendClass(typeof(Microsoft.Scripting.Actions.TypeGroup), 0x00000000, null, LoadMicrosoft__Scripting__Actions__TypeGroup_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def48});
            // Skipped primitive: Object
            ExtendClass(typeof(System.Char), 0x00000000, null, LoadSystem__Char_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def8, def48, def58}, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Char>(IronRuby.Builtins.CharOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Char, System.Char>(IronRuby.Builtins.CharOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Char[], System.Char>(IronRuby.Builtins.CharOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.String, System.Char>(IronRuby.Builtins.CharOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Char>(IronRuby.Builtins.CharOps.Create)
            );
            ExtendModule(typeof(System.Collections.Generic.IDictionary<System.Object, System.Object>), 0x00000000, LoadSystem__Collections__Generic__IDictionary_Instance, null, null, def48);
            ExtendModule(typeof(System.Collections.IEnumerable), 0x00000000, LoadSystem__Collections__IEnumerable_Instance, null, null, def48);
            ExtendModule(typeof(System.Collections.IList), 0x00000000, LoadSystem__Collections__IList_Instance, null, null, def48);
            ExtendModule(typeof(System.IComparable), 0x00000000, LoadSystem__IComparable_Instance, null, null, def58);
            ExtendClass(typeof(System.String), 0x00000000, null, LoadSystem__String_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def8, def48, def58}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.String>(IronRuby.Builtins.ClrStringOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Char, System.Int32, System.String>(IronRuby.Builtins.ClrStringOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Char[], System.String>(IronRuby.Builtins.ClrStringOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Char[], System.Int32, System.Int32, System.String>(IronRuby.Builtins.ClrStringOps.Create)
            );
            DefineGlobalClass("Array", typeof(IronRuby.Builtins.RubyArray), 0x00000007, Context.ObjectClass, LoadArray_Instance, LoadArray_Class, LoadArray_Constants, new IronRuby.Builtins.RubyModule[] {def48}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.CreateArray), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Runtime.Union<System.Collections.IList, System.Int32>>, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object, System.Object>(IronRuby.Builtins.ArrayOps.CreateArray), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Int32, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.CreateArray), 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.CreateArray)
            );
            DefineGlobalClass("Binding", typeof(IronRuby.Builtins.Binding), 0x00000007, Context.ObjectClass, LoadBinding_Instance, LoadBinding_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("Dir", typeof(IronRuby.Builtins.RubyDir), 0x0000000F, Context.ObjectClass, LoadDir_Instance, LoadDir_Class, null, new IronRuby.Builtins.RubyModule[] {def48}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyDir>(IronRuby.Builtins.RubyDir.Create)
            );
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def9 = DefineGlobalClass("Encoding", typeof(IronRuby.Builtins.RubyEncoding), 0x00000007, Context.ObjectClass, LoadEncoding_Instance, LoadEncoding_Class, LoadEncoding_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            DefineGlobalClass("Enumerator", typeof(IronRuby.Builtins.Enumerator), 0x0000000F, Context.ObjectClass, LoadEnumerator_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def48}, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.String, System.Object[], IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerator.Create)
            );
            IronRuby.Builtins.RubyClass def59 = Context.ExceptionClass = DefineGlobalClass("Exception", typeof(System.Exception), 0x00000007, Context.ObjectClass, LoadException_Instance, LoadException_Class, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__Exception));
            Context.FalseClass = DefineGlobalClass("FalseClass", typeof(IronRuby.Builtins.FalseClass), 0x0000000F, Context.ObjectClass, LoadFalseClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def46 = DefineClass("File::Stat", typeof(System.IO.FileSystemInfo), 0x00000007, Context.ObjectClass, LoadFile__Stat_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def58}, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.IO.FileSystemInfo>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Create)
            );
            #endif
            DefineGlobalClass("Hash", typeof(IronRuby.Builtins.Hash), 0x00000007, Context.ObjectClass, LoadHash_Instance, LoadHash_Class, LoadHash_Constants, new IronRuby.Builtins.RubyModule[] {def48}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateHash), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateHash), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateHash)
            );
            IronRuby.Builtins.RubyClass def37 = DefineGlobalClass("IO", typeof(IronRuby.Builtins.RubyIO), 0x00000007, Context.ObjectClass, LoadIO_Instance, LoadIO_Class, LoadIO_Constants, new IronRuby.Builtins.RubyModule[] {def36, def48}, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Nullable<System.Int32>>, IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.CreateFile)
            );
            IronRuby.Builtins.RubyClass def43 = DefineClass("IronRuby::Clr::Name", typeof(IronRuby.Runtime.ClrName), 0x00000007, Context.ObjectClass, LoadIronRuby__Clr__Name_Instance, LoadIronRuby__Clr__Name_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("MatchData", typeof(IronRuby.Builtins.MatchData), 0x00000007, Context.ObjectClass, LoadMatchData_Instance, LoadMatchData_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("Method", typeof(IronRuby.Builtins.RubyMethod), 0x00000007, Context.ObjectClass, LoadMethod_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            // Skipped primitive: Module
            Context.NilClass = DefineGlobalClass("NilClass", typeof(Microsoft.Scripting.Runtime.DynamicNull), 0x00000007, Context.ObjectClass, LoadNilClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def54 = DefineGlobalClass("Numeric", typeof(IronRuby.Builtins.Numeric), 0x0000000F, Context.ObjectClass, LoadNumeric_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def58});
            DefineGlobalClass("Proc", typeof(IronRuby.Builtins.Proc), 0x00000007, Context.ObjectClass, LoadProc_Instance, LoadProc_Class, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Action<IronRuby.Builtins.RubyClass, System.Object[]>(IronRuby.Builtins.ProcOps.Error)
            );
            #if !SILVERLIGHT && !SILVERLIGHT
            IronRuby.Builtins.RubyClass def41 = DefineClass("Process::Status", typeof(IronRuby.Builtins.RubyProcess.Status), 0x0000000F, Context.ObjectClass, LoadProcess__Status_Instance, LoadProcess__Status_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            DefineGlobalClass("Range", typeof(IronRuby.Builtins.Range), 0x00000007, Context.ObjectClass, LoadRange_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def48}, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Boolean, IronRuby.Builtins.Range>(IronRuby.Builtins.RangeOps.CreateRange)
            );
            DefineGlobalClass("Regexp", typeof(IronRuby.Builtins.RubyRegex), 0x00000007, Context.ObjectClass, LoadRegexp_Instance, LoadRegexp_Class, LoadRegexp_Constants, new IronRuby.Builtins.RubyModule[] {def48}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyRegex, System.Int32, System.Object, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyRegex, System.Object, System.Object, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Boolean, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Create)
            );
            DefineGlobalClass("String", typeof(IronRuby.Builtins.MutableString), 0x00000007, Context.ObjectClass, LoadString_Instance, null, null, new IronRuby.Builtins.RubyModule[] {def58}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Byte[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Create)
            );
            DefineGlobalClass("Struct", typeof(IronRuby.Builtins.RubyStruct), 0x00000007, Context.ObjectClass, LoadStruct_Instance, LoadStruct_Class, LoadStruct_Constants, new IronRuby.Builtins.RubyModule[] {def48}, 
                new Action<IronRuby.Builtins.RubyClass, System.Object[]>(IronRuby.Builtins.RubyStructOps.AllocatorUndefined)
            );
            DefineGlobalClass("Symbol", typeof(IronRuby.Builtins.RubySymbol), 0x00000007, Context.ObjectClass, LoadSymbol_Instance, LoadSymbol_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("Thread", typeof(System.Threading.Thread), 0x00000007, Context.ObjectClass, LoadThread_Instance, LoadThread_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("ThreadGroup", typeof(IronRuby.Builtins.ThreadGroup), 0x0000000F, Context.ObjectClass, LoadThreadGroup_Instance, null, LoadThreadGroup_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("Time", typeof(IronRuby.Builtins.RubyTime), 0x00000007, Context.ObjectClass, LoadTime_Instance, LoadTime_Class, null, new IronRuby.Builtins.RubyModule[] {def58}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.Create)
            );
            Context.TrueClass = DefineGlobalClass("TrueClass", typeof(IronRuby.Builtins.TrueClass), 0x0000000F, Context.ObjectClass, LoadTrueClass_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("UnboundMethod", typeof(IronRuby.Builtins.UnboundMethod), 0x0000000F, Context.ObjectClass, LoadUnboundMethod_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            // Skipped primitive: Class
            IronRuby.Builtins.RubyClass def35 = DefineGlobalClass("File", typeof(IronRuby.Builtins.RubyFile), 0x00000007, def37, LoadFile_Instance, LoadFile_Class, LoadFile_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Nullable<System.Int32>>, IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyFile>(IronRuby.Builtins.RubyFileOps.CreateFile)
            );
            DefineGlobalClass("Float", typeof(System.Double), 0x00000007, def54, LoadFloat_Instance, LoadFloat_Class, LoadFloat_Constants, new IronRuby.Builtins.RubyModule[] {def55});
            IronRuby.Builtins.RubyClass def60 = DefineGlobalClass("Integer", typeof(IronRuby.Builtins.Integer), 0x0000000F, def54, LoadInteger_Instance, LoadInteger_Class, null, new IronRuby.Builtins.RubyModule[] {def55});
            DefineGlobalClass("NoMemoryError", typeof(IronRuby.Builtins.NoMemoryError), 0x0000000F, def59, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__NoMemoryError));
            IronRuby.Builtins.RubyClass def57 = DefineGlobalClass("ScriptError", typeof(IronRuby.Builtins.ScriptError), 0x00000007, def59, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__ScriptError));
            IronRuby.Builtins.RubyClass def56 = DefineGlobalClass("SignalException", typeof(IronRuby.Builtins.SignalException), 0x0000000F, def59, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__SignalException));
            IronRuby.Builtins.RubyClass def51 = Context.StandardErrorClass = DefineGlobalClass("StandardError", typeof(System.SystemException), 0x00000007, def59, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__StandardError));
            ExtendClass(typeof(System.Decimal), 0x00000000, def54, LoadSystem__Decimal_Instance, LoadSystem__Decimal_Class, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyModule, System.Double, System.Decimal>(IronRuby.Builtins.DecimalOps.InducedFrom)
            );
            ExtendClass(typeof(System.Single), 0x00000000, def54, LoadSystem__Single_Instance, LoadSystem__Single_Class, LoadSystem__Single_Constants, new IronRuby.Builtins.RubyModule[] {def55}, 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Single>(IronRuby.Builtins.SingleOps.Create)
            );
            DefineGlobalClass("SystemExit", typeof(IronRuby.Builtins.SystemExit), 0x00000007, def59, LoadSystemExit_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.SystemExit>(IronRuby.Builtins.SystemExitOps.Factory), 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Object, IronRuby.Builtins.SystemExit>(IronRuby.Builtins.SystemExitOps.Factory)
            );
            DefineGlobalClass("ArgumentError", typeof(System.ArgumentException), 0x00000007, def51, LoadArgumentError_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__ArgumentError));
            DefineGlobalClass("Bignum", typeof(Microsoft.Scripting.Math.BigInteger), 0x00000007, def60, LoadBignum_Instance, LoadBignum_Class, LoadBignum_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def50 = DefineGlobalClass("EncodingError", typeof(IronRuby.Builtins.EncodingError), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__EncodingError));
            DefineGlobalClass("Fixnum", typeof(System.Int32), 0x00000007, def60, LoadFixnum_Instance, LoadFixnum_Class, LoadFixnum_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("IndexError", typeof(System.IndexOutOfRangeException), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__IndexError));
            DefineGlobalClass("Interrupt", typeof(IronRuby.Builtins.Interrupt), 0x0000000F, def56, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__Interrupt));
            IronRuby.Builtins.RubyClass def52 = DefineGlobalClass("IOError", typeof(System.IO.IOException), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__IOError));
            DefineGlobalClass("LoadError", typeof(IronRuby.Builtins.LoadError), 0x00000007, def57, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__LoadError));
            DefineGlobalClass("LocalJumpError", typeof(IronRuby.Builtins.LocalJumpError), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__LocalJumpError));
            IronRuby.Builtins.RubyClass def61 = DefineGlobalClass("NameError", typeof(System.MemberAccessException), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__NameError));
            DefineGlobalClass("NotImplementedError", typeof(IronRuby.Builtins.NotImplementedError), 0x00000007, def57, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__NotImplementedError));
            IronRuby.Builtins.RubyClass def53 = DefineGlobalClass("RangeError", typeof(System.ArgumentOutOfRangeException), 0x00000007, def51, LoadRangeError_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__RangeError));
            DefineGlobalClass("RegexpError", typeof(IronRuby.Builtins.RegexpError), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__RegexpError));
            DefineGlobalClass("RuntimeError", typeof(IronRuby.Builtins.RuntimeError), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__RuntimeError));
            DefineGlobalClass("SecurityError", typeof(System.Security.SecurityException), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__SecurityError));
            DefineGlobalClass("SyntaxError", typeof(IronRuby.Builtins.SyntaxError), 0x00000007, def57, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__SyntaxError));
            ExtendClass(typeof(System.Byte), 0x00000000, def60, LoadSystem__Byte_Instance, LoadSystem__Byte_Class, LoadSystem__Byte_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Byte>(IronRuby.Builtins.ByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.Byte>(IronRuby.Builtins.ByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Byte>(IronRuby.Builtins.ByteOps.InducedFrom)
            );
            ExtendClass(typeof(System.Int16), 0x00000000, def60, LoadSystem__Int16_Instance, LoadSystem__Int16_Class, LoadSystem__Int16_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int16>(IronRuby.Builtins.Int16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.Int16>(IronRuby.Builtins.Int16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Int16>(IronRuby.Builtins.Int16Ops.InducedFrom)
            );
            ExtendClass(typeof(System.Int64), 0x00000000, def60, LoadSystem__Int64_Instance, LoadSystem__Int64_Class, LoadSystem__Int64_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int64>(IronRuby.Builtins.Int64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.Int64>(IronRuby.Builtins.Int64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Int64>(IronRuby.Builtins.Int64Ops.InducedFrom)
            );
            ExtendClass(typeof(System.SByte), 0x00000000, def60, LoadSystem__SByte_Instance, LoadSystem__SByte_Class, LoadSystem__SByte_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.SByte>(IronRuby.Builtins.SByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.SByte>(IronRuby.Builtins.SByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.SByte>(IronRuby.Builtins.SByteOps.InducedFrom)
            );
            ExtendClass(typeof(System.UInt16), 0x00000000, def60, LoadSystem__UInt16_Instance, LoadSystem__UInt16_Class, LoadSystem__UInt16_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.UInt16>(IronRuby.Builtins.UInt16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.UInt16>(IronRuby.Builtins.UInt16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.UInt16>(IronRuby.Builtins.UInt16Ops.InducedFrom)
            );
            ExtendClass(typeof(System.UInt32), 0x00000000, def60, LoadSystem__UInt32_Instance, LoadSystem__UInt32_Class, LoadSystem__UInt32_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.UInt32>(IronRuby.Builtins.UInt32Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.UInt32>(IronRuby.Builtins.UInt32Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.UInt32>(IronRuby.Builtins.UInt32Ops.InducedFrom)
            );
            ExtendClass(typeof(System.UInt64), 0x00000000, def60, LoadSystem__UInt64_Instance, LoadSystem__UInt64_Class, LoadSystem__UInt64_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.UInt64>(IronRuby.Builtins.UInt64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.UInt64>(IronRuby.Builtins.UInt64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.UInt64>(IronRuby.Builtins.UInt64Ops.InducedFrom)
            );
            IronRuby.Builtins.RubyClass def49 = DefineGlobalClass("SystemCallError", typeof(System.Runtime.InteropServices.ExternalException), 0x00000007, def51, LoadSystemCallError_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Runtime.InteropServices.ExternalException>(IronRuby.Builtins.SystemCallErrorOps.Factory), 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Runtime.InteropServices.ExternalException>(IronRuby.Builtins.SystemCallErrorOps.Factory)
            );
            DefineGlobalClass("SystemStackError", typeof(IronRuby.Builtins.SystemStackError), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__SystemStackError));
            DefineGlobalClass("ThreadError", typeof(IronRuby.Builtins.ThreadError), 0x0000000F, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__ThreadError));
            DefineGlobalClass("TypeError", typeof(System.InvalidOperationException), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__TypeError));
            DefineGlobalClass("ZeroDivisionError", typeof(System.DivideByZeroException), 0x00000007, def51, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__ZeroDivisionError));
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def11 = DefineClass("Encoding::CompatibilityError", typeof(IronRuby.Builtins.EncodingCompatibilityError), 0x00000007, def50, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__Encoding__CompatibilityError));
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def10 = DefineClass("Encoding::ConverterNotFoundError", typeof(IronRuby.Builtins.ConverterNotFoundError), 0x00000007, def50, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__Encoding__ConverterNotFoundError));
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def30 = DefineClass("Encoding::InvalidByteSequenceError", typeof(IronRuby.Builtins.InvalidByteSequenceError), 0x00000007, def50, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__Encoding__InvalidByteSequenceError));
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def42 = DefineClass("Encoding::UndefinedConversionError", typeof(IronRuby.Builtins.UndefinedConversionError), 0x00000007, def50, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__Encoding__UndefinedConversionError));
            #endif
            DefineGlobalClass("EOFError", typeof(IronRuby.Builtins.EOFError), 0x0000000F, def52, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__EOFError));
            IronRuby.Builtins.RubyClass def47 = DefineClass("Errno::EACCES", typeof(System.UnauthorizedAccessException), 0x00000007, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.UnauthorizedAccessException>(IronRuby.Builtins.Errno.UnauthorizedAccessExceptionOps.Create)
            );
            IronRuby.Builtins.RubyClass def13 = DefineClass("Errno::EADDRINUSE", typeof(IronRuby.Builtins.Errno.AddressInUseError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def25 = DefineClass("Errno::EAGAIN", typeof(IronRuby.Builtins.Errno.ResourceTemporarilyUnavailableError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def3 = DefineClass("Errno::EBADF", typeof(IronRuby.Builtins.BadFileDescriptorError), 0x00000007, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.BadFileDescriptorError>(IronRuby.Builtins.Errno.BadFileDescriptorErrorOps.Create)
            );
            IronRuby.Builtins.RubyClass def14 = DefineClass("Errno::ECHILD", typeof(IronRuby.Builtins.Errno.ChildError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def15 = DefineClass("Errno::ECONNABORTED", typeof(IronRuby.Builtins.Errno.ConnectionAbortedError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def16 = DefineClass("Errno::ECONNREFUSED", typeof(IronRuby.Builtins.Errno.ConnectionRefusedError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def17 = DefineClass("Errno::ECONNRESET", typeof(IronRuby.Builtins.Errno.ConnectionResetError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def18 = DefineClass("Errno::EDOM", typeof(IronRuby.Builtins.Errno.DomainError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def28 = DefineClass("Errno::EEXIST", typeof(IronRuby.Builtins.ExistError), 0x00000007, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.ExistError>(IronRuby.Builtins.Errno.ExistErrorOps.Create)
            );
            IronRuby.Builtins.RubyClass def19 = DefineClass("Errno::EHOSTDOWN", typeof(IronRuby.Builtins.Errno.HostDownError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def21 = DefineClass("Errno::EINTR", typeof(IronRuby.Builtins.Errno.InterruptedError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def31 = DefineClass("Errno::EINVAL", typeof(IronRuby.Builtins.InvalidError), 0x00000007, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.InvalidError>(IronRuby.Builtins.Errno.InvalidErrorOps.Create)
            );
            IronRuby.Builtins.RubyClass def45 = DefineClass("Errno::ENOENT", typeof(System.IO.FileNotFoundException), 0x00000007, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.IO.FileNotFoundException>(IronRuby.Builtins.Errno.FileNotFoundExceptionOps.Create)
            );
            IronRuby.Builtins.RubyClass def27 = DefineClass("Errno::ENOEXEC", typeof(IronRuby.Builtins.ExecFormatError), 0x00000007, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.BadFileDescriptorError>(IronRuby.Builtins.Errno.ExecFormatErrorOps.Create)
            );
            IronRuby.Builtins.RubyClass def23 = DefineClass("Errno::ENOTCONN", typeof(IronRuby.Builtins.Errno.NotConnectedError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def44 = DefineClass("Errno::ENOTDIR", typeof(System.IO.DirectoryNotFoundException), 0x00000007, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.IO.DirectoryNotFoundException>(IronRuby.Builtins.Errno.DirectoryNotFoundExceptionOps.Create)
            );
            IronRuby.Builtins.RubyClass def24 = DefineClass("Errno::EPIPE", typeof(IronRuby.Builtins.Errno.PipeError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def22 = DefineClass("Errno::ESPIPE", typeof(IronRuby.Builtins.Errno.InvalidSeekError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def26 = DefineClass("Errno::EWOULDBLOCK", typeof(IronRuby.Builtins.Errno.WouldBlockError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def20 = DefineClass("Errno::EXDEV", typeof(IronRuby.Builtins.Errno.ImproperLinkError), 0x0000000F, def49, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("FloatDomainError", typeof(IronRuby.Builtins.FloatDomainError), 0x0000000F, def53, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(BuiltinsLibraryInitializer.ExceptionFactory__FloatDomainError));
            DefineGlobalClass("NoMethodError", typeof(System.MissingMethodException), 0x00000007, def61, LoadNoMethodError_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, System.MissingMethodException>(IronRuby.Builtins.NoMethodErrorOps.Factory)
            );
            SetBuiltinConstant(def35, "Constants", def36);
            SetBuiltinConstant(def37, "WaitReadable", def38);
            SetBuiltinConstant(def37, "WaitWritable", def39);
            SetBuiltinConstant(def32, "Clr", def4);
            SetBuiltinConstant(def4, "BigInteger", def5);
            SetBuiltinConstant(def4, "FlagEnumeration", def29);
            SetBuiltinConstant(def4, "Float", def6);
            SetBuiltinConstant(def4, "Integer", def7);
            SetBuiltinConstant(def4, "MultiDimensionalArray", def33);
            SetBuiltinConstant(def4, "String", def8);
            SetBuiltinConstant(def32, "Print", def34);
            #if !SILVERLIGHT
            SetBuiltinConstant(Context.ObjectClass, "ARGF", def1);
            #endif
            SetBuiltinConstant(Context.ObjectClass, "ENV", def12);
            #if !SILVERLIGHT
            SetBuiltinConstant(def35, "Stat", def46);
            #endif
            SetBuiltinConstant(def4, "Name", def43);
            #if !SILVERLIGHT && !SILVERLIGHT
            SetBuiltinConstant(def40, "Status", def41);
            #endif
            #if !SILVERLIGHT
            SetBuiltinConstant(def9, "CompatibilityError", def11);
            #endif
            #if !SILVERLIGHT
            SetBuiltinConstant(def9, "ConverterNotFoundError", def10);
            #endif
            #if !SILVERLIGHT
            SetBuiltinConstant(def9, "InvalidByteSequenceError", def30);
            #endif
            #if !SILVERLIGHT
            SetBuiltinConstant(def9, "UndefinedConversionError", def42);
            #endif
            SetBuiltinConstant(def2, "EACCES", def47);
            SetBuiltinConstant(def2, "EADDRINUSE", def13);
            SetBuiltinConstant(def2, "EAGAIN", def25);
            SetBuiltinConstant(def2, "EBADF", def3);
            SetBuiltinConstant(def2, "ECHILD", def14);
            SetBuiltinConstant(def2, "ECONNABORTED", def15);
            SetBuiltinConstant(def2, "ECONNREFUSED", def16);
            SetBuiltinConstant(def2, "ECONNRESET", def17);
            SetBuiltinConstant(def2, "EDOM", def18);
            SetBuiltinConstant(def2, "EEXIST", def28);
            SetBuiltinConstant(def2, "EHOSTDOWN", def19);
            SetBuiltinConstant(def2, "EINTR", def21);
            SetBuiltinConstant(def2, "EINVAL", def31);
            SetBuiltinConstant(def2, "ENOENT", def45);
            SetBuiltinConstant(def2, "ENOEXEC", def27);
            SetBuiltinConstant(def2, "ENOTCONN", def23);
            SetBuiltinConstant(def2, "ENOTDIR", def44);
            SetBuiltinConstant(def2, "EPIPE", def24);
            SetBuiltinConstant(def2, "ESPIPE", def22);
            SetBuiltinConstant(def2, "EWOULDBLOCK", def26);
            SetBuiltinConstant(def2, "EXDEV", def20);
        }
        
        private static void Load__MainSingleton_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "include", 0x51, 
                0x80000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyModule[], IronRuby.Builtins.RubyClass>(IronRuby.Builtins.MainSingletonOps.Include)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.MainSingletonOps.Initialize)
            );
            
            DefineLibraryMethod(module, "private", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.MainSingletonOps.SetPrivateVisibility)
            );
            
            DefineLibraryMethod(module, "public", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.MainSingletonOps.SetPublicVisibility)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MainSingletonOps.ToS)
            );
            
        }
        
        private static void Load__MainSingleton_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
        }
        
        #if !SILVERLIGHT
        private static void Load__Singleton_ArgFilesSingletonOps_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "binmode", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.BinMode)
            );
            
            DefineLibraryMethod(module, "close", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Close)
            );
            
            DefineLibraryMethod(module, "closed?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.ArgFilesSingletonOps.Closed)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 0x00000000U, 0x00040008U, 0x000c0000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, Microsoft.Scripting.Runtime.DynamicNull, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each)
            );
            
            DefineLibraryMethod(module, "each_byte", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.EachByte)
            );
            
            DefineLibraryMethod(module, "each_line", 0x51, 
                0x00000000U, 0x00000000U, 0x00040008U, 0x000c0000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, Microsoft.Scripting.Runtime.DynamicNull, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Each)
            );
            
            DefineLibraryMethod(module, "eof", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.ArgFilesSingletonOps.EoF)
            );
            
            DefineLibraryMethod(module, "eof?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.ArgFilesSingletonOps.EoF)
            );
            
            DefineLibraryMethod(module, "file", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.ArgFilesSingletonOps.ToIO)
            );
            
            DefineLibraryMethod(module, "filename", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.GetCurrentFileName)
            );
            
            DefineLibraryMethod(module, "fileno", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.ArgFilesSingletonOps.FileNo)
            );
            
            DefineLibraryMethod(module, "getc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Getc)
            );
            
            DefineLibraryMethod(module, "gets", 0x51, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.Gets), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.Gets), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.Gets), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.Gets)
            );
            
            DefineLibraryMethod(module, "lineno", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.ArgFilesSingletonOps.GetLineNumber)
            );
            
            DefineLibraryMethod(module, "lineno=", 0x51, 
                0x00020000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.ArgFilesSingletonOps.SetLineNumber)
            );
            
            DefineLibraryMethod(module, "path", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.GetCurrentFileName)
            );
            
            DefineLibraryMethod(module, "pos", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Pos)
            );
            
            DefineLibraryMethod(module, "pos=", 0x51, 
                0x00020000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Runtime.IntegerValue>(IronRuby.Builtins.ArgFilesSingletonOps.Pos)
            );
            
            DefineLibraryMethod(module, "read", 0x51, 
                0x00000000U, 0x00020000U, 0x00030000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.Read), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.Read), 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.Read)
            );
            
            DefineLibraryMethod(module, "readchar", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.ArgFilesSingletonOps.ReadChar)
            );
            
            DefineLibraryMethod(module, "readline", 0x51, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLine)
            );
            
            DefineLibraryMethod(module, "readlines", 0x51, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArgFilesSingletonOps.ReadLines)
            );
            
            DefineLibraryMethod(module, "rewind", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Rewind)
            );
            
            DefineLibraryMethod(module, "seek", 0x51, 
                0x00060000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Runtime.IntegerValue, System.Int32, System.Int32>(IronRuby.Builtins.ArgFilesSingletonOps.Seek)
            );
            
            DefineLibraryMethod(module, "skip", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Skip)
            );
            
            DefineLibraryMethod(module, "tell", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.ArgFilesSingletonOps.Pos)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArgFilesSingletonOps.TOA)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.ArgFilesSingletonOps.FileNo)
            );
            
            DefineLibraryMethod(module, "to_io", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.ArgFilesSingletonOps.ToIO)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArgFilesSingletonOps.ToS)
            );
            
        }
        #endif
        
        private static void LoadArgumentError_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.HideMethod("message");
        }
        
        private static void LoadArray_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadArray_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadSystem__Collections__IList_Instance(module);
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 0x00000008U, 0x00000000U, 0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.Reinitialize), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Runtime.Union<System.Collections.IList, System.Int32>>, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyArray, System.Object, System.Object>(IronRuby.Builtins.ArrayOps.Reinitialize), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyArray, System.Int32, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.Reinitialize), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, System.Int32, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.ReinitializeByRepeatedValue)
            );
            
            DefineLibraryMethod(module, "pack", 0x51, 
                0x00100020U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Runtime.IntegerValue>, IronRuby.Runtime.ConversionStorage<System.Double>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyArray, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ArrayOps.Pack)
            );
            
            DefineLibraryMethod(module, "reverse!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.InPlaceReverse)
            );
            
            DefineLibraryMethod(module, "sort", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyArray, System.Object>(IronRuby.Builtins.ArrayOps.Sort)
            );
            
            DefineLibraryMethod(module, "sort!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyArray, System.Object>(IronRuby.Builtins.ArrayOps.SortInPlace)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.ToArray)
            );
            
            DefineLibraryMethod(module, "to_ary", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.ToExplicitArray)
            );
            
        }
        
        private static void LoadArray_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x61, 
                0x80000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ArrayOps.MakeArray)
            );
            
            DefineLibraryMethod(module, "try_convert", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Builtins.RubyClass, System.Object, System.Collections.IList>(IronRuby.Builtins.ArrayOps.TryConvert)
            );
            
        }
        
        private static void LoadBasicObject_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "!", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Boolean>(IronRuby.Builtins.BasicObjectOps.Not)
            );
            
            DefineLibraryMethod(module, "!=", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.BasicObjectOps.ValueNotEquals)
            );
            
            DefineLibraryMethod(module, "__send__", 0x51, 
                new[] { 0x00000000U, 0x00020004U, 0x00040008U, 0x00020004U, 0x00040008U, 0x00020004U, 0x00040008U, 0x00020004U, 0x00040008U, 0x80020004U, 0x80040008U}, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object[], System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object[], System.Object>(IronRuby.Builtins.BasicObjectOps.SendMessage)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000001U, 0x00000000U, 
                new Func<IronRuby.Runtime.IRubyObject, System.Object, System.Boolean>(IronRuby.Builtins.BasicObjectOps.ValueEquals), 
                new Func<System.Object, System.Object, System.Boolean>(IronRuby.Builtins.BasicObjectOps.ValueEquals)
            );
            
            DefineLibraryMethod(module, "equal?", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object, System.Boolean>(IronRuby.Builtins.BasicObjectOps.IsEqual)
            );
            
            DefineLibraryMethod(module, "initialize", 0x5a, 
                0x80000000U, 
                new Func<System.Object, System.Object[], System.Object>(IronRuby.Builtins.BasicObjectOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "instance_eval", 0x51, 
                0x0000000cU, 0x00000001U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.BasicObjectOps.Evaluate), 
                new Func<IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.InstanceEval)
            );
            
            DefineLibraryMethod(module, "instance_exec", 0x51, 
                0x80000001U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, System.Object[], System.Object>(IronRuby.Builtins.BasicObjectOps.InstanceExec)
            );
            
            DefineLibraryMethod(module, "method_missing", 0x52, 
                0x80000004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubySymbol, System.Object[], System.Object>(IronRuby.Builtins.BasicObjectOps.MethodMissing)
            );
            
            DefineLibraryMethod(module, "singleton_method_added", 0x5a, 
                0x00000000U, 
                new Action<System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.MethodAdded)
            );
            
            DefineLibraryMethod(module, "singleton_method_removed", 0x5a, 
                0x00000000U, 
                new Action<System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.MethodRemoved)
            );
            
            DefineLibraryMethod(module, "singleton_method_undefined", 0x5a, 
                0x00000000U, 
                new Action<System.Object, System.Object>(IronRuby.Builtins.BasicObjectOps.MethodUndefined)
            );
            
        }
        
        private static void LoadBasicObject_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
        }
        
        private static void LoadBignum_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadBignum_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__BigInteger_Instance(module);
            module.HideMethod("<");
            module.HideMethod("<=");
            module.HideMethod(">");
            module.HideMethod(">=");
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32>(IronRuby.Builtins.BignumOps.Size)
            );
            
        }
        
        private static void LoadBignum_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.UndefineMethodNoEvent("new");
        }
        
        private static void LoadBinding_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.UndefineMethodNoEvent("LocalScope");
        }
        
        private static void LoadBinding_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.UndefineMethodNoEvent("new");
        }
        
        private static void LoadClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.UndefineMethodNoEvent("append_features");
            module.UndefineMethodNoEvent("extend_object");
            module.UndefineMethodNoEvent("module_function");
            DefineRuleGenerator(module, "allocate", 0x51, IronRuby.Builtins.ClassOps.Allocate());
            
            DefineLibraryMethod(module, "clr_constructor", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.ClassOps.GetClrConstructor)
            );
            
            DefineLibraryMethod(module, "clr_ctor", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.ClassOps.GetClrConstructor)
            );
            
            DefineRuleGenerator(module, "clr_new", 0x51, IronRuby.Builtins.ClassOps.ClrNew());
            
            DefineLibraryMethod(module, "inherited", 0x5a, 
                0x00000000U, 
                new Action<System.Object, System.Object>(IronRuby.Builtins.ClassOps.Inherited)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 
                new Action<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.ClassOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000002U, 
                new Action<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.ClassOps.InitializeCopy)
            );
            
            DefineRuleGenerator(module, "new", 0x51, IronRuby.Builtins.ClassOps.New());
            
            DefineLibraryMethod(module, "superclass", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.ClassOps.GetSuperclass)
            );
            
        }
        
        private static void LoadClass_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
        }
        
        private static void LoadComparable_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.Comparable.Less)
            );
            
            DefineLibraryMethod(module, "<=", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.Comparable.LessOrEqual)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.Comparable.Equal)
            );
            
            DefineLibraryMethod(module, ">", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.Comparable.Greater)
            );
            
            DefineLibraryMethod(module, ">=", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.Comparable.GreaterOrEqual)
            );
            
            DefineLibraryMethod(module, "between?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, System.Object, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.Comparable.Between)
            );
            
        }
        
        private static void LoadDir_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "close", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyDir>(IronRuby.Builtins.RubyDir.Close)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyDir, System.Object>(IronRuby.Builtins.RubyDir.Each)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyDir, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyDir>(IronRuby.Builtins.RubyDir.Reinitialize)
            );
            
            DefineLibraryMethod(module, "path", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyDir, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyDir.GetPath)
            );
            
            DefineLibraryMethod(module, "pos", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyDir, System.Int32>(IronRuby.Builtins.RubyDir.GetCurrentPosition)
            );
            
            DefineLibraryMethod(module, "pos=", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyDir, System.Int32, System.Int32>(IronRuby.Builtins.RubyDir.SetPosition)
            );
            
            DefineLibraryMethod(module, "read", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyDir, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyDir.Read)
            );
            
            DefineLibraryMethod(module, "rewind", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyDir, IronRuby.Builtins.RubyDir>(IronRuby.Builtins.RubyDir.Rewind)
            );
            
            DefineLibraryMethod(module, "seek", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyDir, System.Int32, IronRuby.Builtins.RubyDir>(IronRuby.Builtins.RubyDir.Seek)
            );
            
            DefineLibraryMethod(module, "tell", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyDir, System.Int32>(IronRuby.Builtins.RubyDir.GetCurrentPosition)
            );
            
            DefineLibraryMethod(module, "to_path", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyDir, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyDir.GetPath)
            );
            
        }
        
        private static void LoadDir_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x61, 
                0x80010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyDir.Glob)
            );
            
            DefineLibraryMethod(module, "chdir", 0x61, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object, System.Object>(IronRuby.Builtins.RubyDir.ChangeDirectory), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object>(IronRuby.Builtins.RubyDir.ChangeDirectory)
            );
            
            DefineLibraryMethod(module, "chroot", 0x61, 
                0x00000000U, 
                new Func<System.Object, System.Int32>(IronRuby.Builtins.RubyDir.ChangeRoot)
            );
            
            DefineLibraryMethod(module, "delete", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Int32>(IronRuby.Builtins.RubyDir.RemoveDirectory)
            );
            
            DefineLibraryMethod(module, "entries", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyDir.GetEntries)
            );
            
            DefineLibraryMethod(module, "exist?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyDir.Exists)
            );
            
            DefineLibraryMethod(module, "exists?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyDir.Exists)
            );
            
            DefineLibraryMethod(module, "foreach", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object, System.Object>(IronRuby.Builtins.RubyDir.ForEach)
            );
            
            DefineLibraryMethod(module, "getwd", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyDir.GetCurrentDirectory)
            );
            
            DefineLibraryMethod(module, "glob", 0x61, 
                0x00060005U, 0x00030002U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.RubyDir.Glob), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.RubyDir.Glob)
            );
            
            DefineLibraryMethod(module, "mkdir", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Int32>(IronRuby.Builtins.RubyDir.MakeDirectory)
            );
            
            DefineLibraryMethod(module, "open", 0x61, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object, System.Object>(IronRuby.Builtins.RubyDir.Open), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.RubyDir>(IronRuby.Builtins.RubyDir.Open)
            );
            
            DefineLibraryMethod(module, "pwd", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyDir.GetCurrentDirectory)
            );
            
            DefineLibraryMethod(module, "rmdir", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Int32>(IronRuby.Builtins.RubyDir.RemoveDirectory)
            );
            
            DefineLibraryMethod(module, "unlink", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Int32>(IronRuby.Builtins.RubyDir.RemoveDirectory)
            );
            
        }
        
        #if !SILVERLIGHT
        private static void LoadEncoding_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "ANSI_X3_4_1968", IronRuby.Builtins.RubyEncodingOps.US_ASCII);
            SetBuiltinConstant(module, "ASCII", IronRuby.Builtins.RubyEncodingOps.US_ASCII);
            SetBuiltinConstant(module, "ASCII_8BIT", IronRuby.Builtins.RubyEncodingOps.ASCII_8BIT);
            SetBuiltinConstant(module, "Big5", IronRuby.Builtins.RubyEncodingOps.Big5);
            SetBuiltinConstant(module, "BIG5", IronRuby.Builtins.RubyEncodingOps.Big5);
            SetBuiltinConstant(module, "BINARY", IronRuby.Builtins.RubyEncodingOps.BINARY);
            SetBuiltinConstant(module, "EUC_JP", IronRuby.Builtins.RubyEncodingOps.EUC_JP);
            SetBuiltinConstant(module, "ISO_8859_15", IronRuby.Builtins.RubyEncodingOps.ISO_8859_15);
            SetBuiltinConstant(module, "ISO_8859_9", IronRuby.Builtins.RubyEncodingOps.ISO_8859_9);
            SetBuiltinConstant(module, "ISO8859_15", IronRuby.Builtins.RubyEncodingOps.ISO_8859_15);
            SetBuiltinConstant(module, "ISO8859_9", IronRuby.Builtins.RubyEncodingOps.ISO_8859_9);
            SetBuiltinConstant(module, "KOI8_R", IronRuby.Builtins.RubyEncodingOps.KOI8_R);
            SetBuiltinConstant(module, "Shift_JIS", IronRuby.Builtins.RubyEncodingOps.SHIFT_JIS);
            SetBuiltinConstant(module, "SHIFT_JIS", IronRuby.Builtins.RubyEncodingOps.SHIFT_JIS);
            SetBuiltinConstant(module, "TIS_620", IronRuby.Builtins.RubyEncodingOps.TIS_620);
            SetBuiltinConstant(module, "US_ASCII", IronRuby.Builtins.RubyEncodingOps.US_ASCII);
            SetBuiltinConstant(module, "UTF_16BE", IronRuby.Builtins.RubyEncodingOps.UTF_16BE);
            SetBuiltinConstant(module, "UTF_16LE", IronRuby.Builtins.RubyEncodingOps.UTF_16LE);
            SetBuiltinConstant(module, "UTF_32BE", IronRuby.Builtins.RubyEncodingOps.UTF_32BE);
            SetBuiltinConstant(module, "UTF_32LE", IronRuby.Builtins.RubyEncodingOps.UTF_32LE);
            SetBuiltinConstant(module, "UTF_7", IronRuby.Builtins.RubyEncodingOps.UTF_7);
            SetBuiltinConstant(module, "UTF_8", IronRuby.Builtins.RubyEncodingOps.UTF_8);
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadEncoding_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "ascii_compatible?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyEncoding, System.Boolean>(IronRuby.Builtins.RubyEncodingOps.IsAsciiCompatible)
            );
            
            DefineLibraryMethod(module, "based_encoding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.BasedEncoding)
            );
            
            DefineLibraryMethod(module, "dummy?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyEncoding, System.Boolean>(IronRuby.Builtins.RubyEncodingOps.IsDummy)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyEncodingOps.Inspect)
            );
            
            DefineLibraryMethod(module, "name", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyEncodingOps.ToS)
            );
            
            DefineLibraryMethod(module, "names", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyEncodingOps.GetAllNames)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyEncodingOps.ToS)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadEncoding_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "aliases", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash>(IronRuby.Builtins.RubyEncodingOps.GetAliases)
            );
            
            DefineLibraryMethod(module, "compatible?", 0x61, 
                new[] { 0x00000006U, 0x00000006U, 0x00000006U, 0x00000006U, 0x00000006U, 0x00000006U, 0x00000006U, 0x00000006U, 0x00000006U, 0x00000000U}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible), 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetCompatible)
            );
            
            DefineLibraryMethod(module, "default_external", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetDefaultExternalEncoding)
            );
            
            DefineLibraryMethod(module, "default_external=", 0x61, 
                0x00000000U, 0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.SetDefaultExternalEncoding), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.SetDefaultExternalEncoding)
            );
            
            DefineLibraryMethod(module, "default_internal", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetDefaultInternalEncoding)
            );
            
            DefineLibraryMethod(module, "default_internal=", 0x61, 
                0x00000000U, 0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.SetDefaultInternalEncoding), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.SetDefaultInternalEncoding)
            );
            
            DefineLibraryMethod(module, "find", 0x61, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyEncodingOps.GetEncoding)
            );
            
            DefineLibraryMethod(module, "list", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyEncodingOps.GetAvailableEncodings)
            );
            
            DefineLibraryMethod(module, "locale_charmap", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyEncodingOps.GetDefaultCharmap)
            );
            
            DefineLibraryMethod(module, "name_list", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyEncodingOps.GetNameList)
            );
            
        }
        #endif
        
        private static void LoadEnumerable_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "all?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.TrueForAll)
            );
            
            DefineLibraryMethod(module, "any?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.TrueForAny)
            );
            
            DefineLibraryMethod(module, "collect", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetMapEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Map)
            );
            
            DefineLibraryMethod(module, "count", 0x51, 
                0x00000000U, 0x00000000U, 0x00000004U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, System.Object, System.Int32>(IronRuby.Builtins.Enumerable.Count), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Int32>(IronRuby.Builtins.Enumerable.Count), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Count)
            );
            
            DefineLibraryMethod(module, "cycle", 0x51, 
                0x00040000U, 0x00000000U, 0x00040002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Int32, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetCycleEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, Microsoft.Scripting.Runtime.DynamicNull, System.Object>(IronRuby.Builtins.Enumerable.Cycle), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Int32, System.Object>(IronRuby.Builtins.Enumerable.Cycle)
            );
            
            DefineLibraryMethod(module, "detect", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetFindEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Find)
            );
            
            DefineLibraryMethod(module, "drop", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, System.Object, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Enumerable.Drop)
            );
            
            DefineLibraryMethod(module, "drop_while", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetDropWhileEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.DropWhile)
            );
            
            DefineLibraryMethod(module, "each_cons", 0x51, 
                0x00040000U, 0x00040002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Int32, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetEachConsEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Int32, System.Object>(IronRuby.Builtins.Enumerable.EachCons)
            );
            
            DefineLibraryMethod(module, "each_slice", 0x51, 
                0x00040000U, 0x00040002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Int32, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetEachSliceEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Int32, System.Object>(IronRuby.Builtins.Enumerable.EachSlice)
            );
            
            DefineLibraryMethod(module, "each_with_index", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetEachWithIndexEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.EachWithIndex)
            );
            
            DefineLibraryMethod(module, "entries", 0x51, 
                0x00000000U, 0x80000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Enumerable.ToArray), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Collections.IList, System.Object>>, System.Object, System.Object[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Enumerable.ToArray)
            );
            
            DefineLibraryMethod(module, "enum_cons", 0x51, 
                0x00010000U, 
                new Func<System.Object, System.Int32, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetConsEnumerator)
            );
            
            DefineLibraryMethod(module, "enum_slice", 0x51, 
                0x00010000U, 
                new Func<System.Object, System.Int32, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetSliceEnumerator)
            );
            
            DefineLibraryMethod(module, "enum_with_index", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetEnumeratorWithIndex)
            );
            
            DefineLibraryMethod(module, "find", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetFindEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Find)
            );
            
            DefineLibraryMethod(module, "find_all", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Select)
            );
            
            DefineLibraryMethod(module, "find_index", 0x51, 
                0x00000000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetFindIndexEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.FindIndex), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.FindIndex)
            );
            
            DefineLibraryMethod(module, "first", 0x51, 
                0x00000000U, 0x00020000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, System.Object, System.Object>(IronRuby.Builtins.Enumerable.First), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, System.Object, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Enumerable.Take)
            );
            
            DefineLibraryMethod(module, "grep", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Grep)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Contains)
            );
            
            DefineLibraryMethod(module, "inject", 0x51, 
                0x00000002U, 0x00080010U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Inject), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.String, System.Object>(IronRuby.Builtins.Enumerable.Inject)
            );
            
            DefineLibraryMethod(module, "map", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetMapEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Map)
            );
            
            DefineLibraryMethod(module, "max", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.GetMaximum)
            );
            
            DefineLibraryMethod(module, "member?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Contains)
            );
            
            DefineLibraryMethod(module, "min", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.GetMinimum)
            );
            
            DefineLibraryMethod(module, "minmax", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.GetExtremes)
            );
            
            DefineLibraryMethod(module, "none?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.TrueForNone)
            );
            
            DefineLibraryMethod(module, "one?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.One)
            );
            
            DefineLibraryMethod(module, "partition", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetPartitionEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Partition)
            );
            
            DefineLibraryMethod(module, "reduce", 0x51, 
                0x00000002U, 0x00080010U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Inject), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.String, System.Object>(IronRuby.Builtins.Enumerable.Inject)
            );
            
            DefineLibraryMethod(module, "reject", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Reject)
            );
            
            DefineLibraryMethod(module, "select", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Select)
            );
            
            DefineLibraryMethod(module, "sort", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.Sort)
            );
            
            DefineLibraryMethod(module, "sort_by", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.SortBy)
            );
            
            DefineLibraryMethod(module, "take", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, System.Object, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Enumerable.Take)
            );
            
            DefineLibraryMethod(module, "take_while", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerable.GetTakeWhileEnumerator), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Enumerable.TakeWhile)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 0x80000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Enumerable.ToArray), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Collections.IList, System.Object>>, System.Object, System.Object[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Enumerable.ToArray)
            );
            
            DefineLibraryMethod(module, "zip", 0x51, 
                0x80040008U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, System.Object, System.Collections.IList[], System.Object>(IronRuby.Builtins.Enumerable.Zip)
            );
            
        }
        
        private static void LoadEnumerator_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.Enumerator, System.Object>(IronRuby.Builtins.Enumerator.Each)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x80020000U, 
                new Func<IronRuby.Builtins.Enumerator, System.Object, System.String, System.Object[], IronRuby.Builtins.Enumerator>(IronRuby.Builtins.Enumerator.Reinitialize)
            );
            
        }
        
        private static void Load__Singleton_EnvironmentSingletonOps_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.EnvironmentSingletonOps.GetVariable)
            );
            
            DefineLibraryMethod(module, "[]=", 0x51, 
                0x00060004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.EnvironmentSingletonOps.SetVariable)
            );
            
            DefineLibraryMethod(module, "clear", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Clear)
            );
            
            DefineLibraryMethod(module, "delete", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Delete)
            );
            
            DefineLibraryMethod(module, "delete_if", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.DeleteIf)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Each)
            );
            
            DefineLibraryMethod(module, "each_key", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.EachKey)
            );
            
            DefineLibraryMethod(module, "each_pair", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Each)
            );
            
            DefineLibraryMethod(module, "each_value", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.EachValue)
            );
            
            DefineLibraryMethod(module, "empty?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.EnvironmentSingletonOps.IsEmpty)
            );
            
            DefineLibraryMethod(module, "fetch", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.EnvironmentSingletonOps.GetVariable)
            );
            
            DefineLibraryMethod(module, "has_key?", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.EnvironmentSingletonOps.HasKey)
            );
            
            DefineLibraryMethod(module, "has_value?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.EnvironmentSingletonOps.HasValue)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.EnvironmentSingletonOps.HasKey)
            );
            
            DefineLibraryMethod(module, "index", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.EnvironmentSingletonOps.Index)
            );
            
            DefineLibraryMethod(module, "indexes", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.EnvironmentSingletonOps.Index)
            );
            
            DefineLibraryMethod(module, "indices", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.EnvironmentSingletonOps.Indices)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.EnvironmentSingletonOps.Inspect)
            );
            
            DefineLibraryMethod(module, "invert", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.Hash>(IronRuby.Builtins.EnvironmentSingletonOps.Invert)
            );
            
            DefineLibraryMethod(module, "key?", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.EnvironmentSingletonOps.HasKey)
            );
            
            DefineLibraryMethod(module, "keys", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.EnvironmentSingletonOps.Keys)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.EnvironmentSingletonOps.Length)
            );
            
            DefineLibraryMethod(module, "rehash", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Rehash)
            );
            
            DefineLibraryMethod(module, "reject!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.DeleteIf)
            );
            
            DefineLibraryMethod(module, "replace", 0x51, 
                0x00000004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.Builtins.Hash, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Replace)
            );
            
            DefineLibraryMethod(module, "shift", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Shift)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.EnvironmentSingletonOps.Length)
            );
            
            DefineLibraryMethod(module, "store", 0x51, 
                0x00060004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.EnvironmentSingletonOps.SetVariable)
            );
            
            DefineLibraryMethod(module, "to_hash", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.Hash>(IronRuby.Builtins.EnvironmentSingletonOps.ToHash)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.EnvironmentSingletonOps.ToString)
            );
            
            DefineLibraryMethod(module, "update", 0x51, 
                0x00000004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.Builtins.Hash, System.Object>(IronRuby.Builtins.EnvironmentSingletonOps.Update)
            );
            
            DefineLibraryMethod(module, "value?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.EnvironmentSingletonOps.HasValue)
            );
            
            DefineLibraryMethod(module, "values", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.EnvironmentSingletonOps.Values)
            );
            
            DefineLibraryMethod(module, "values_at", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.EnvironmentSingletonOps.ValuesAt)
            );
            
        }
        
        private static void LoadException_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "backtrace", 0x51, 
                0x00000000U, 
                new Func<System.Exception, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ExceptionOps.GetBacktrace)
            );
            
            DefineRuleGenerator(module, "exception", 0x51, IronRuby.Builtins.ExceptionOps.GetException());
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Exception, System.Object, System.Exception>(IronRuby.Builtins.ExceptionOps.ReinitializeException)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Exception, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ExceptionOps.Inspect)
            );
            
            DefineLibraryMethod(module, "message", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Exception, System.Object>(IronRuby.Builtins.ExceptionOps.GetMessage)
            );
            
            DefineLibraryMethod(module, "set_backtrace", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<System.Exception, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ExceptionOps.SetBacktrace), 
                new Func<System.Exception, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ExceptionOps.SetBacktrace)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<System.Exception, System.Object>(IronRuby.Builtins.ExceptionOps.StringRepresentation)
            );
            
            DefineLibraryMethod(module, "to_str", 0x51, 
                0x00000000U, 
                new Func<System.Exception, System.Object>(IronRuby.Builtins.ExceptionOps.StringRepresentation)
            );
            
        }
        
        private static void LoadException_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineRuleGenerator(module, "exception", 0x61, IronRuby.Builtins.ExceptionOps.CreateException());
            
        }
        
        private static void LoadFalseClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "&", 0x51, 
                0x00000000U, 
                new Func<System.Boolean, System.Object, System.Boolean>(IronRuby.Builtins.FalseClass.And)
            );
            
            DefineLibraryMethod(module, "^", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Boolean, System.Object, System.Boolean>(IronRuby.Builtins.FalseClass.Xor), 
                new Func<System.Boolean, System.Boolean, System.Boolean>(IronRuby.Builtins.FalseClass.Xor)
            );
            
            DefineLibraryMethod(module, "|", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Boolean, System.Object, System.Boolean>(IronRuby.Builtins.FalseClass.Or), 
                new Func<System.Boolean, System.Boolean, System.Boolean>(IronRuby.Builtins.FalseClass.Or)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<System.Boolean, IronRuby.Builtins.MutableString>(IronRuby.Builtins.FalseClass.ToString)
            );
            
        }
        
        private static void LoadFile_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "ALT_SEPARATOR", IronRuby.Builtins.RubyFileOps.ALT_SEPARATOR);
            SetBuiltinConstant(module, "PATH_SEPARATOR", IronRuby.Builtins.RubyFileOps.PATH_SEPARATOR);
            SetBuiltinConstant(module, "Separator", IronRuby.Builtins.RubyFileOps.Separator);
            SetBuiltinConstant(module, "SEPARATOR", IronRuby.Builtins.RubyFileOps.SEPARATOR);
            
        }
        
        private static void LoadFile_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "atime", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyFile, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.AccessTime)
            );
            
            DefineLibraryMethod(module, "chmod", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyFile, System.Int32, System.Int32>(IronRuby.Builtins.RubyFileOps.Chmod)
            );
            
            DefineLibraryMethod(module, "chown", 0x51, 
                0x00030000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyFile, System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.RubyFileOps.ChangeOwner), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyFile, System.Object, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.ChangeOwner)
            );
            
            DefineLibraryMethod(module, "ctime", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyFile, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.CreateTime)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00800000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Nullable<System.Int32>>, IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyFile, System.Object, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyFile>(IronRuby.Builtins.RubyFileOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyFile, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.Inspect)
            );
            
            DefineLibraryMethod(module, "lstat", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyFile, System.IO.FileSystemInfo>(IronRuby.Builtins.RubyFileOps.Stat)
            );
            
            DefineLibraryMethod(module, "mtime", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyFile, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.ModifiedTime)
            );
            
            DefineLibraryMethod(module, "path", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyFile, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.GetPath)
            );
            
            DefineLibraryMethod(module, "stat", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyFile, System.IO.FileSystemInfo>(IronRuby.Builtins.RubyFileOps.Stat)
            );
            
            DefineLibraryMethod(module, "to_path", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyFile, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.GetPath)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "truncate", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyFile, System.Int32, System.Int32>(IronRuby.Builtins.RubyFileOps.Truncate)
            );
            
            #endif
        }
        
        private static void LoadFile_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "absolute_path", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.AbsolutePath)
            );
            
            DefineLibraryMethod(module, "atime", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.AccessTime)
            );
            
            DefineLibraryMethod(module, "basename", 0x61, 
                0x00040008U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.BaseName)
            );
            
            DefineLibraryMethod(module, "blockdev?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsBlockDevice)
            );
            
            DefineLibraryMethod(module, "chardev?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsCharDevice)
            );
            
            DefineLibraryMethod(module, "chmod", 0x61, 
                0x00020000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Int32, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.Chmod)
            );
            
            DefineLibraryMethod(module, "chown", 0x61, 
                0x00070008U, 0x00080010U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int32, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyFileOps.ChangeOwner), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, System.Object, System.Object, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyFileOps.ChangeOwner)
            );
            
            DefineLibraryMethod(module, "ctime", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.CreateTime)
            );
            
            DefineLibraryMethod(module, "delete", 0x61, 
                0x00000000U, 0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.Delete), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], System.Int32>(IronRuby.Builtins.RubyFileOps.Delete)
            );
            
            DefineLibraryMethod(module, "directory?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsDirectory)
            );
            
            DefineLibraryMethod(module, "dirname", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.DirName)
            );
            
            DefineLibraryMethod(module, "executable?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsExecutable)
            );
            
            DefineLibraryMethod(module, "executable_real?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsExecutable)
            );
            
            DefineLibraryMethod(module, "exist?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.Exists)
            );
            
            DefineLibraryMethod(module, "exists?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.Exists)
            );
            
            DefineLibraryMethod(module, "expand_path", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.ExpandPath)
            );
            
            DefineLibraryMethod(module, "extname", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.GetExtension)
            );
            
            DefineLibraryMethod(module, "file?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsFile)
            );
            
            DefineLibraryMethod(module, "fnmatch", 0x61, 
                0x00020004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.Builtins.MutableString, System.Object, System.Int32, System.Boolean>(IronRuby.Builtins.RubyFileOps.FnMatch)
            );
            
            DefineLibraryMethod(module, "fnmatch?", 0x61, 
                0x00020004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.Builtins.MutableString, System.Object, System.Int32, System.Boolean>(IronRuby.Builtins.RubyFileOps.FnMatch)
            );
            
            DefineLibraryMethod(module, "ftype", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.FileType)
            );
            
            DefineLibraryMethod(module, "grpowned?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsGroupOwned)
            );
            
            DefineLibraryMethod(module, "identical?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.AreIdentical)
            );
            
            DefineLibraryMethod(module, "join", 0x61, 
                0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.Join)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "link", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.Link)
            );
            
            #endif
            DefineLibraryMethod(module, "lstat", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.IO.FileSystemInfo>(IronRuby.Builtins.RubyFileOps.Stat)
            );
            
            DefineLibraryMethod(module, "mtime", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.ModifiedTime)
            );
            
            DefineRuleGenerator(module, "open", 0x61, IronRuby.Builtins.RubyFileOps.Open());
            
            DefineLibraryMethod(module, "owned?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsUserOwned)
            );
            
            DefineLibraryMethod(module, "path", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.ToPath)
            );
            
            DefineLibraryMethod(module, "pipe?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsPipe)
            );
            
            DefineLibraryMethod(module, "readable?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsReadable)
            );
            
            DefineLibraryMethod(module, "readable_real?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsReadable)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "readlink", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.Readlink)
            );
            
            #endif
            DefineLibraryMethod(module, "rename", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.Rename)
            );
            
            DefineLibraryMethod(module, "setgid?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsSetGid)
            );
            
            DefineLibraryMethod(module, "setuid?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsSetUid)
            );
            
            DefineLibraryMethod(module, "size", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.Size)
            );
            
            DefineLibraryMethod(module, "size?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.RubyFileOps.NullableSize)
            );
            
            DefineLibraryMethod(module, "socket?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsSocket)
            );
            
            DefineLibraryMethod(module, "split", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyFileOps.Split)
            );
            
            DefineLibraryMethod(module, "stat", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.IO.FileSystemInfo>(IronRuby.Builtins.RubyFileOps.Stat)
            );
            
            DefineLibraryMethod(module, "sticky?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.RubyFileOps.IsSticky)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "symlink", 0x61, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.RubyFileOps.SymLink)
            );
            
            #endif
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "symlink?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsSymLink)
            );
            
            #endif
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "truncate", 0x61, 
                0x00040000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Int32, System.Int32>(IronRuby.Builtins.RubyFileOps.Truncate)
            );
            
            #endif
            DefineLibraryMethod(module, "umask", 0x61, 
                0x00010000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int32>(IronRuby.Builtins.RubyFileOps.GetUmask), 
                new Func<IronRuby.Builtins.RubyClass, System.Int32>(IronRuby.Builtins.RubyFileOps.GetUmask)
            );
            
            DefineLibraryMethod(module, "unlink", 0x61, 
                0x00000000U, 0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.Delete), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], System.Int32>(IronRuby.Builtins.RubyFileOps.Delete)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "utime", 0x61, 
                0x0000000cU, 0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime, System.Object, System.Int32>(IronRuby.Builtins.RubyFileOps.UpdateTimes), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object[], System.Int32>(IronRuby.Builtins.RubyFileOps.UpdateTimes)
            );
            
            #endif
            DefineLibraryMethod(module, "writable?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsWritable)
            );
            
            DefineLibraryMethod(module, "writable_real?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsWritable)
            );
            
            DefineLibraryMethod(module, "zero?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.RubyFileOps.IsZeroLength)
            );
            
        }
        
        private static void LoadFile__Constants_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "APPEND", IronRuby.Builtins.RubyFileOps.Constants.APPEND);
            SetBuiltinConstant(module, "BINARY", IronRuby.Builtins.RubyFileOps.Constants.BINARY);
            SetBuiltinConstant(module, "CREAT", IronRuby.Builtins.RubyFileOps.Constants.CREAT);
            SetBuiltinConstant(module, "EXCL", IronRuby.Builtins.RubyFileOps.Constants.EXCL);
            SetBuiltinConstant(module, "FNM_CASEFOLD", IronRuby.Builtins.RubyFileOps.Constants.FNM_CASEFOLD);
            SetBuiltinConstant(module, "FNM_DOTMATCH", IronRuby.Builtins.RubyFileOps.Constants.FNM_DOTMATCH);
            SetBuiltinConstant(module, "FNM_NOESCAPE", IronRuby.Builtins.RubyFileOps.Constants.FNM_NOESCAPE);
            SetBuiltinConstant(module, "FNM_PATHNAME", IronRuby.Builtins.RubyFileOps.Constants.FNM_PATHNAME);
            SetBuiltinConstant(module, "FNM_SYSCASE", IronRuby.Builtins.RubyFileOps.Constants.FNM_SYSCASE);
            SetBuiltinConstant(module, "LOCK_EX", IronRuby.Builtins.RubyFileOps.Constants.LOCK_EX);
            SetBuiltinConstant(module, "LOCK_NB", IronRuby.Builtins.RubyFileOps.Constants.LOCK_NB);
            SetBuiltinConstant(module, "LOCK_SH", IronRuby.Builtins.RubyFileOps.Constants.LOCK_SH);
            SetBuiltinConstant(module, "LOCK_UN", IronRuby.Builtins.RubyFileOps.Constants.LOCK_UN);
            SetBuiltinConstant(module, "NONBLOCK", IronRuby.Builtins.RubyFileOps.Constants.NONBLOCK);
            SetBuiltinConstant(module, "RDONLY", IronRuby.Builtins.RubyFileOps.Constants.RDONLY);
            SetBuiltinConstant(module, "RDWR", IronRuby.Builtins.RubyFileOps.Constants.RDWR);
            SetBuiltinConstant(module, "TRUNC", IronRuby.Builtins.RubyFileOps.Constants.TRUNC);
            SetBuiltinConstant(module, "WRONLY", IronRuby.Builtins.RubyFileOps.Constants.WRONLY);
            
        }
        
        #if !SILVERLIGHT
        private static void LoadFile__Stat_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.IO.FileSystemInfo, System.Int32>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Compare), 
                new Func<System.IO.FileSystemInfo, System.Object, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Compare)
            );
            
            DefineLibraryMethod(module, "atime", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.RubyStatOps.AccessTime)
            );
            
            DefineLibraryMethod(module, "blksize", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.BlockSize)
            );
            
            DefineLibraryMethod(module, "blockdev?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsBlockDevice)
            );
            
            DefineLibraryMethod(module, "blocks", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Blocks)
            );
            
            DefineLibraryMethod(module, "chardev?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsCharDevice)
            );
            
            DefineLibraryMethod(module, "ctime", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.RubyStatOps.CreateTime)
            );
            
            DefineLibraryMethod(module, "dev", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.DeviceId)
            );
            
            DefineLibraryMethod(module, "dev_major", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.DeviceIdMajor)
            );
            
            DefineLibraryMethod(module, "dev_minor", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.DeviceIdMinor)
            );
            
            DefineLibraryMethod(module, "directory?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsDirectory)
            );
            
            DefineLibraryMethod(module, "executable?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsExecutable)
            );
            
            DefineLibraryMethod(module, "executable_real?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsExecutable)
            );
            
            DefineLibraryMethod(module, "file?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsFile)
            );
            
            DefineLibraryMethod(module, "ftype", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.RubyStatOps.FileType)
            );
            
            DefineLibraryMethod(module, "gid", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Int32>(IronRuby.Builtins.RubyFileOps.RubyStatOps.GroupId)
            );
            
            DefineLibraryMethod(module, "grpowned?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsGroupOwned)
            );
            
            DefineLibraryMethod(module, "identical?", 0x51, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, System.IO.FileSystemInfo, System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.AreIdentical)
            );
            
            DefineLibraryMethod(module, "ino", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Int32>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Inode)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.IO.FileSystemInfo, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Inspect)
            );
            
            DefineLibraryMethod(module, "mode", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Int32>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Mode)
            );
            
            DefineLibraryMethod(module, "mtime", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyFileOps.RubyStatOps.ModifiedTime)
            );
            
            DefineLibraryMethod(module, "nlink", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Int32>(IronRuby.Builtins.RubyFileOps.RubyStatOps.NumberOfLinks)
            );
            
            DefineLibraryMethod(module, "owned?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsUserOwned)
            );
            
            DefineLibraryMethod(module, "pipe?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsPipe)
            );
            
            DefineLibraryMethod(module, "rdev", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.DeviceId)
            );
            
            DefineLibraryMethod(module, "rdev_major", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.DeviceIdMajor)
            );
            
            DefineLibraryMethod(module, "rdev_minor", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.DeviceIdMinor)
            );
            
            DefineLibraryMethod(module, "readable?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsReadable)
            );
            
            DefineLibraryMethod(module, "readable_real?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsReadable)
            );
            
            DefineLibraryMethod(module, "setgid?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsSetGid)
            );
            
            DefineLibraryMethod(module, "setuid?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsSetUid)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Int32>(IronRuby.Builtins.RubyFileOps.RubyStatOps.Size)
            );
            
            DefineLibraryMethod(module, "size?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.NullableSize)
            );
            
            DefineLibraryMethod(module, "socket?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsSocket)
            );
            
            DefineLibraryMethod(module, "sticky?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Object>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsSticky)
            );
            
            DefineLibraryMethod(module, "symlink?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsSymLink)
            );
            
            DefineLibraryMethod(module, "uid", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Int32>(IronRuby.Builtins.RubyFileOps.RubyStatOps.UserId)
            );
            
            DefineLibraryMethod(module, "writable?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsWritable)
            );
            
            DefineLibraryMethod(module, "writable_real?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsWritable)
            );
            
            DefineLibraryMethod(module, "zero?", 0x51, 
                0x00000000U, 
                new Func<System.IO.FileSystemInfo, System.Boolean>(IronRuby.Builtins.RubyFileOps.RubyStatOps.IsZeroLength)
            );
            
        }
        #endif
        
        private static void LoadFileTest_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "blockdev?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsBlockDevice)
            );
            
            DefineLibraryMethod(module, "chardev?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsCharDevice)
            );
            
            DefineLibraryMethod(module, "directory?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsDirectory)
            );
            
            DefineLibraryMethod(module, "executable?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsExecutable)
            );
            
            DefineLibraryMethod(module, "executable_real?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsExecutable)
            );
            
            DefineLibraryMethod(module, "exist?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.Exists)
            );
            
            DefineLibraryMethod(module, "exists?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.Exists)
            );
            
            DefineLibraryMethod(module, "file?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsFile)
            );
            
            DefineLibraryMethod(module, "grpowned?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsGroupOwned)
            );
            
            DefineLibraryMethod(module, "identical?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.AreIdentical)
            );
            
            DefineLibraryMethod(module, "owned?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsUserOwned)
            );
            
            DefineLibraryMethod(module, "pipe?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsPipe)
            );
            
            DefineLibraryMethod(module, "readable?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsReadable)
            );
            
            DefineLibraryMethod(module, "readable_real?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsReadable)
            );
            
            DefineLibraryMethod(module, "setgid?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSetGid)
            );
            
            DefineLibraryMethod(module, "setuid?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSetUid)
            );
            
            DefineLibraryMethod(module, "size", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Int32>(IronRuby.Builtins.FileTest.Size)
            );
            
            DefineLibraryMethod(module, "size?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.FileTest.NullableSize)
            );
            
            DefineLibraryMethod(module, "socket?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSocket)
            );
            
            DefineLibraryMethod(module, "sticky?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.FileTest.IsSticky)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "symlink?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSymLink)
            );
            
            #endif
            DefineLibraryMethod(module, "writable?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsWritable)
            );
            
            DefineLibraryMethod(module, "writable_real?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsWritable)
            );
            
            DefineLibraryMethod(module, "zero?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsZeroLength)
            );
            
        }
        
        private static void LoadFileTest_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "blockdev?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsBlockDevice)
            );
            
            DefineLibraryMethod(module, "chardev?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsCharDevice)
            );
            
            DefineLibraryMethod(module, "directory?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsDirectory)
            );
            
            DefineLibraryMethod(module, "executable?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsExecutable)
            );
            
            DefineLibraryMethod(module, "executable_real?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsExecutable)
            );
            
            DefineLibraryMethod(module, "exist?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.Exists)
            );
            
            DefineLibraryMethod(module, "exists?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.Exists)
            );
            
            DefineLibraryMethod(module, "file?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsFile)
            );
            
            DefineLibraryMethod(module, "grpowned?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsGroupOwned)
            );
            
            DefineLibraryMethod(module, "identical?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.AreIdentical)
            );
            
            DefineLibraryMethod(module, "owned?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsUserOwned)
            );
            
            DefineLibraryMethod(module, "pipe?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsPipe)
            );
            
            DefineLibraryMethod(module, "readable?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsReadable)
            );
            
            DefineLibraryMethod(module, "readable_real?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsReadable)
            );
            
            DefineLibraryMethod(module, "setgid?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSetGid)
            );
            
            DefineLibraryMethod(module, "setuid?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSetUid)
            );
            
            DefineLibraryMethod(module, "size", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Int32>(IronRuby.Builtins.FileTest.Size)
            );
            
            DefineLibraryMethod(module, "size?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.FileTest.NullableSize)
            );
            
            DefineLibraryMethod(module, "socket?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSocket)
            );
            
            DefineLibraryMethod(module, "sticky?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.FileTest.IsSticky)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "symlink?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsSymLink)
            );
            
            #endif
            DefineLibraryMethod(module, "writable?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsWritable)
            );
            
            DefineLibraryMethod(module, "writable_real?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsWritable)
            );
            
            DefineLibraryMethod(module, "zero?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.FileTest.IsZeroLength)
            );
            
        }
        
        private static void LoadFixnum_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadFixnum_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Integer_Instance(module);
            DefineLibraryMethod(module, "id2name", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, System.Object>(IronRuby.Builtins.Int32Ops.Id2Name)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.Int32, System.Int32>(IronRuby.Builtins.Int32Ops.Size)
            );
            
            DefineLibraryMethod(module, "to_sym", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, System.Object>(IronRuby.Builtins.Int32Ops.ToSymbol)
            );
            
        }
        
        private static void LoadFixnum_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.UndefineMethodNoEvent("new");
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int32>(IronRuby.Builtins.Int32Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Int32>(IronRuby.Builtins.Int32Ops.InducedFrom)
            );
            
        }
        
        private static void LoadFloat_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "DIG", IronRuby.Builtins.FloatOps.DIG);
            SetBuiltinConstant(module, "EPSILON", IronRuby.Builtins.FloatOps.EPSILON);
            SetBuiltinConstant(module, "MANT_DIG", IronRuby.Builtins.FloatOps.MANT_DIG);
            SetBuiltinConstant(module, "MAX", IronRuby.Builtins.FloatOps.MAX);
            SetBuiltinConstant(module, "MAX_10_EXP", IronRuby.Builtins.FloatOps.MAX_10_EXP);
            SetBuiltinConstant(module, "MAX_EXP", IronRuby.Builtins.FloatOps.MAX_EXP);
            SetBuiltinConstant(module, "MIN", IronRuby.Builtins.FloatOps.MIN);
            SetBuiltinConstant(module, "MIN_10_EXP", IronRuby.Builtins.FloatOps.MIN_10_EXP);
            SetBuiltinConstant(module, "MIN_EXP", IronRuby.Builtins.FloatOps.MIN_EXP);
            SetBuiltinConstant(module, "RADIX", IronRuby.Builtins.FloatOps.RADIX);
            SetBuiltinConstant(module, "ROUNDS", IronRuby.Builtins.FloatOps.ROUNDS);
            
        }
        
        private static void LoadFloat_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Float_Instance(module);
        }
        
        private static void LoadFloat_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Float_Class(module);
            module.UndefineMethodNoEvent("new");
        }
        
        private static void LoadGC_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "garbage_collect", 0x51, 
                0x00000000U, 
                new Action<System.Object>(IronRuby.Builtins.RubyGC.GarbageCollect)
            );
            
        }
        
        private static void LoadGC_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "disable", 0x61, 
                0x00000000U, 
                new Func<System.Object, System.Boolean>(IronRuby.Builtins.RubyGC.Disable)
            );
            
            DefineLibraryMethod(module, "enable", 0x61, 
                0x00000000U, 
                new Func<System.Object, System.Boolean>(IronRuby.Builtins.RubyGC.Enable)
            );
            
            DefineLibraryMethod(module, "start", 0x61, 
                0x00000000U, 
                new Action<System.Object>(IronRuby.Builtins.RubyGC.GarbageCollect)
            );
            
        }
        
        private static void LoadHash_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadHash_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadSystem__Collections__Generic__IDictionary_Instance(module);
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object>(IronRuby.Builtins.HashOps.GetElement)
            );
            
            DefineLibraryMethod(module, "default", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Hash, System.Object>(IronRuby.Builtins.HashOps.GetDefaultValue), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.Proc, IronRuby.Builtins.Hash, System.Object, System.Object>>, IronRuby.Builtins.Hash, System.Object, System.Object>(IronRuby.Builtins.HashOps.GetDefaultValue)
            );
            
            DefineLibraryMethod(module, "default_proc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Hash, IronRuby.Builtins.Proc>(IronRuby.Builtins.HashOps.GetDefaultProc)
            );
            
            DefineLibraryMethod(module, "default=", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Hash, System.Object, System.Object>(IronRuby.Builtins.HashOps.SetDefaultValue)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.Hash, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.Initialize), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Hash, System.Object, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.Initialize), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Hash, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.Initialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Hash, IronRuby.Builtins.Hash, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.InitializeCopy)
            );
            
            DefineLibraryMethod(module, "replace", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Hash, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.Replace)
            );
            
            DefineLibraryMethod(module, "shift", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.Hash, System.Object, System.Object>>, IronRuby.Builtins.Hash, System.Object>(IronRuby.Builtins.HashOps.Shift)
            );
            
        }
        
        private static void LoadHash_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x61, 
                new[] { 0x00000000U, 0x00000000U, 0x00000004U, 0x00000002U, 0x80000000U}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateSubclass), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateSubclass), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Builtins.RubyClass, System.Collections.IList, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateSubclass), 
                new Func<IronRuby.Builtins.RubyClass, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateSubclass), 
                new Func<IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.Hash>(IronRuby.Builtins.HashOps.CreateSubclass)
            );
            
            DefineLibraryMethod(module, "try_convert", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Builtins.RubyClass, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>>(IronRuby.Builtins.HashOps.TryConvert)
            );
            
        }
        
        private static void LoadInteger_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "ceil", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.ToInteger)
            );
            
            DefineLibraryMethod(module, "chr", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Int32, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.Integer.ToChr)
            );
            
            DefineLibraryMethod(module, "denominator", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.Denominator)
            );
            
            DefineLibraryMethod(module, "downto", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.Integer.DownTo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Integer.DownTo)
            );
            
            DefineLibraryMethod(module, "even?", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Boolean>(IronRuby.Builtins.Integer.IsEven), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.Integer.IsEven)
            );
            
            DefineLibraryMethod(module, "floor", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.ToInteger)
            );
            
            DefineLibraryMethod(module, "gcd", 0x51, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.Integer.Gcd), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.Integer.Gcd), 
                new Func<System.Object, System.Object, System.Object>(IronRuby.Builtins.Integer.Gcd)
            );
            
            DefineLibraryMethod(module, "gcdlcm", 0x51, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Integer.GcdLcm), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Integer.GcdLcm), 
                new Func<System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Integer.GcdLcm)
            );
            
            DefineLibraryMethod(module, "integer?", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Boolean>(IronRuby.Builtins.Integer.IsInteger)
            );
            
            DefineLibraryMethod(module, "lcm", 0x51, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.Integer.Lcm), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.Integer.Lcm), 
                new Func<System.Object, System.Object, System.Object>(IronRuby.Builtins.Integer.Lcm)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Object>(IronRuby.Builtins.Integer.Next), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.Integer.Next)
            );
            
            DefineLibraryMethod(module, "numerator", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.Numerator)
            );
            
            DefineLibraryMethod(module, "odd?", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Boolean>(IronRuby.Builtins.Integer.IsOdd), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.Integer.IsOdd)
            );
            
            DefineLibraryMethod(module, "ord", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.Numerator)
            );
            
            DefineLibraryMethod(module, "pred", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Object>(IronRuby.Builtins.Integer.Pred), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.Integer.Pred)
            );
            
            DefineLibraryMethod(module, "rationalize", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object>(IronRuby.Builtins.Integer.ToRational)
            );
            
            DefineLibraryMethod(module, "round", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.ToInteger)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Object>(IronRuby.Builtins.Integer.Next), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.Integer.Next)
            );
            
            DefineLibraryMethod(module, "times", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Int32, System.Object>(IronRuby.Builtins.Integer.Times), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.Integer.Times)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.ToInteger)
            );
            
            DefineLibraryMethod(module, "to_int", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.ToInteger)
            );
            
            DefineLibraryMethod(module, "to_r", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object>(IronRuby.Builtins.Integer.ToRational)
            );
            
            DefineLibraryMethod(module, "truncate", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Integer.ToInteger)
            );
            
            DefineLibraryMethod(module, "upto", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.Integer.UpTo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Integer.UpTo)
            );
            
        }
        
        private static void LoadInteger_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00000000U, 0x00000002U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Object>(IronRuby.Builtins.Integer.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.Integer.InducedFrom), 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.RubyClass, System.Double, System.Object>(IronRuby.Builtins.Integer.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Int32>(IronRuby.Builtins.Integer.InducedFrom)
            );
            
        }
        
        private static void LoadIO_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "SEEK_CUR", IronRuby.Builtins.RubyIOOps.SEEK_CUR);
            SetBuiltinConstant(module, "SEEK_END", IronRuby.Builtins.RubyIOOps.SEEK_END);
            SetBuiltinConstant(module, "SEEK_SET", IronRuby.Builtins.RubyIOOps.SEEK_SET);
            
        }
        
        private static void LoadIO_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Print_Instance(module);
            DefineLibraryMethod(module, "binmode", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Binmode)
            );
            
            DefineLibraryMethod(module, "close", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Close)
            );
            
            DefineLibraryMethod(module, "close_read", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.CloseReader)
            );
            
            DefineLibraryMethod(module, "close_write", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.CloseWriter)
            );
            
            DefineLibraryMethod(module, "closed?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Boolean>(IronRuby.Builtins.RubyIOOps.Closed)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 0x00000000U, 0x00040008U, 0x000c0000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.Builtins.RubyIOOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, Microsoft.Scripting.Runtime.DynamicNull, System.Object>(IronRuby.Builtins.RubyIOOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, System.Object>(IronRuby.Builtins.RubyIOOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.RubyIOOps.Each)
            );
            
            DefineLibraryMethod(module, "each_byte", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.Builtins.RubyIOOps.EachByte)
            );
            
            DefineLibraryMethod(module, "each_line", 0x51, 
                0x00000000U, 0x00000000U, 0x00040008U, 0x000c0000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, System.Object>(IronRuby.Builtins.RubyIOOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, Microsoft.Scripting.Runtime.DynamicNull, System.Object>(IronRuby.Builtins.RubyIOOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, System.Object>(IronRuby.Builtins.RubyIOOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.RubyIOOps.Each)
            );
            
            DefineLibraryMethod(module, "eof", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Boolean>(IronRuby.Builtins.RubyIOOps.Eof)
            );
            
            DefineLibraryMethod(module, "eof?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Boolean>(IronRuby.Builtins.RubyIOOps.Eof)
            );
            
            DefineLibraryMethod(module, "external_encoding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyIOOps.GetExternalEncoding)
            );
            
            DefineLibraryMethod(module, "fcntl", 0x51, 
                0x00010000U, 0x00010000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyIOOps.FileControl), 
                new Func<IronRuby.Builtins.RubyIO, System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.RubyIOOps.FileControl)
            );
            
            DefineLibraryMethod(module, "fileno", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32>(IronRuby.Builtins.RubyIOOps.FileNo)
            );
            
            DefineLibraryMethod(module, "flush", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Flush)
            );
            
            DefineLibraryMethod(module, "fsync", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Flush)
            );
            
            DefineLibraryMethod(module, "getc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Object>(IronRuby.Builtins.RubyIOOps.Getc)
            );
            
            DefineLibraryMethod(module, "gets", 0x51, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Gets), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Gets), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Gets), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Gets)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00200000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Nullable<System.Int32>>, IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyIO, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.InitializeCopy)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Inspect)
            );
            
            DefineLibraryMethod(module, "internal_encoding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RubyIOOps.GetInternalEncoding)
            );
            
            DefineLibraryMethod(module, "ioctl", 0x51, 
                0x00010000U, 0x00010000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyIOOps.FileControl), 
                new Func<IronRuby.Builtins.RubyIO, System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.RubyIOOps.FileControl)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "isatty", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Boolean>(IronRuby.Builtins.RubyIOOps.IsAtty)
            );
            
            #endif
            DefineLibraryMethod(module, "lineno", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32>(IronRuby.Builtins.RubyIOOps.GetLineNumber)
            );
            
            DefineLibraryMethod(module, "lineno=", 0x51, 
                0x00020000U, 
                new Action<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO, System.Int32>(IronRuby.Builtins.RubyIOOps.SetLineNumber)
            );
            
            DefineLibraryMethod(module, "pid", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Object>(IronRuby.Builtins.RubyIOOps.Pid)
            );
            
            DefineLibraryMethod(module, "pos", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Object>(IronRuby.Builtins.RubyIOOps.Pos)
            );
            
            DefineLibraryMethod(module, "pos=", 0x51, 
                0x00010000U, 
                new Action<IronRuby.Builtins.RubyIO, IronRuby.Runtime.IntegerValue>(IronRuby.Builtins.RubyIOOps.Pos)
            );
            
            DefineLibraryMethod(module, "read", 0x51, 
                0x00000000U, 0x00020000U, 0x00030000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Read), 
                new Func<IronRuby.Builtins.RubyIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Read), 
                new Func<IronRuby.Builtins.RubyIO, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Read)
            );
            
            DefineLibraryMethod(module, "read_nonblock", 0x51, 
                0x00030000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.ReadNoBlock)
            );
            
            DefineLibraryMethod(module, "readchar", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32>(IronRuby.Builtins.RubyIOOps.ReadChar)
            );
            
            DefineLibraryMethod(module, "readline", 0x51, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.ReadLine)
            );
            
            DefineLibraryMethod(module, "readlines", 0x51, 
                0x00060000U, 0x00000000U, 0x00000000U, 0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.ReadLines)
            );
            
            DefineLibraryMethod(module, "reopen", 0x51, 
                0x00000002U, 0x00040008U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Reopen), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyIO, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Reopen), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyIO, System.Object, System.Int32, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Reopen)
            );
            
            DefineLibraryMethod(module, "rewind", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.Rewind)
            );
            
            DefineLibraryMethod(module, "seek", 0x51, 
                0x00030000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Runtime.IntegerValue, System.Int32, System.Int32>(IronRuby.Builtins.RubyIOOps.Seek)
            );
            
            DefineLibraryMethod(module, "set_encoding", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyIO, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.SetEncodings), 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.SetEncodings)
            );
            
            DefineLibraryMethod(module, "sync", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Boolean>(IronRuby.Builtins.RubyIOOps.Sync)
            );
            
            DefineLibraryMethod(module, "sync=", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Boolean, System.Boolean>(IronRuby.Builtins.RubyIOOps.Sync)
            );
            
            DefineLibraryMethod(module, "sysread", 0x51, 
                0x00030000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.SystemRead)
            );
            
            DefineLibraryMethod(module, "sysseek", 0x51, 
                0x00030000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Runtime.IntegerValue, System.Int32, System.Object>(IronRuby.Builtins.RubyIOOps.SysSeek)
            );
            
            DefineLibraryMethod(module, "syswrite", 0x51, 
                0x00000010U, 0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyIOOps.SysWrite), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyIO, System.Object, System.Int32>(IronRuby.Builtins.RubyIOOps.SysWrite)
            );
            
            DefineLibraryMethod(module, "tell", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Object>(IronRuby.Builtins.RubyIOOps.Pos)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Int32>(IronRuby.Builtins.RubyIOOps.FileNo)
            );
            
            DefineLibraryMethod(module, "to_io", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.ToIO)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "tty?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, System.Boolean>(IronRuby.Builtins.RubyIOOps.IsAtty)
            );
            
            #endif
            DefineLibraryMethod(module, "ungetc", 0x51, 
                0x00010000U, 
                new Action<IronRuby.Builtins.RubyIO, System.Int32>(IronRuby.Builtins.RubyIOOps.SetPreviousByte)
            );
            
            DefineLibraryMethod(module, "write", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyIOOps.Write), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyIO, System.Object, System.Int32>(IronRuby.Builtins.RubyIOOps.Write)
            );
            
            DefineLibraryMethod(module, "write_nonblock", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyIO, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyIOOps.WriteNoBlock), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyIO, System.Object, System.Int32>(IronRuby.Builtins.RubyIOOps.WriteNoBlock)
            );
            
        }
        
        private static void LoadIO_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "copy_stream", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.RubyIOOps.CopyStream)
            );
            
            DefineRuleGenerator(module, "for_fd", 0x61, IronRuby.Builtins.RubyIOOps.ForFileDescriptor());
            
            DefineLibraryMethod(module, "foreach", 0x61, 
                0x00060004U, 0x000e0004U, 
                new Action<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyIOOps.ForEach), 
                new Action<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.RubyIOOps.ForEach)
            );
            
            DefineRuleGenerator(module, "open", 0x61, IronRuby.Builtins.RubyIOOps.Open());
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "pipe", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.OpenPipe)
            );
            
            #endif
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "popen", 0x61, 
                0x000c0018U, 0x0006000cU, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.RubyIOOps.OpenPipe), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.RubyIOOps.OpenPipe)
            );
            
            #endif
            DefineLibraryMethod(module, "read", 0x61, 
                0x00400000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyIOOps.Read)
            );
            
            DefineLibraryMethod(module, "readlines", 0x61, 
                0x00030002U, 0x00070002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.ReadLines), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.ReadLines)
            );
            
            DefineLibraryMethod(module, "select", 0x61, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.Select), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.Select), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyIOOps.Select)
            );
            
            DefineLibraryMethod(module, "sysopen", 0x61, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Int32>(IronRuby.Builtins.RubyIOOps.SysOpen)
            );
            
        }
        
        private static void LoadIronRuby_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "configuration", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, Microsoft.Scripting.Runtime.DlrConfiguration>(IronRuby.Builtins.IronRubyOps.GetConfiguration)
            );
            
            DefineLibraryMethod(module, "globals", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, Microsoft.Scripting.Runtime.Scope>(IronRuby.Builtins.IronRubyOps.GetGlobalScope)
            );
            
            DefineLibraryMethod(module, "load", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.IronRubyOps.Load)
            );
            
            DefineLibraryMethod(module, "loaded_assemblies", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IronRubyOps.GetLoadedAssemblies)
            );
            
            DefineLibraryMethod(module, "loaded_scripts", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, System.Collections.Generic.IDictionary<System.String, Microsoft.Scripting.Runtime.Scope>>(IronRuby.Builtins.IronRubyOps.GetLoadedScripts)
            );
            
            DefineLibraryMethod(module, "require", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.IronRubyOps.Require)
            );
            
        }
        
        private static void LoadIronRuby__Clr_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "profile", 0x61, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.Hash>(IronRuby.Builtins.IronRubyOps.Clr.GetProfile), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.IronRubyOps.Clr.GetProfile)
            );
            
        }
        
        private static void LoadIronRuby__Clr__BigInteger_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "-", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Subtract), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Subtract), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Subtract), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Subtract)
            );
            
            DefineLibraryMethod(module, "%", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Modulo), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Modulo), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.ModuloOp)
            );
            
            DefineLibraryMethod(module, "&", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.And), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.And), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrBigInteger.And)
            );
            
            DefineLibraryMethod(module, "*", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Multiply), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Multiply), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Multiply), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Multiply)
            );
            
            DefineLibraryMethod(module, "**", 0x51, 
                0x00000004U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Power), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Power), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Power), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Power)
            );
            
            DefineLibraryMethod(module, "/", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Divide), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Divide), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.DivideOp), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Divide)
            );
            
            DefineLibraryMethod(module, "-@", 0x51, 
                0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Negate)
            );
            
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00010000U, 0x00000002U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Int32>(IronRuby.Builtins.ClrBigInteger.Bit), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Int32>(IronRuby.Builtins.ClrBigInteger.Bit)
            );
            
            DefineLibraryMethod(module, "^", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Xor), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Xor), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrBigInteger.Xor)
            );
            
            DefineLibraryMethod(module, "|", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.BitwiseOr), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.BitwiseOr), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrBigInteger.BitwiseOr)
            );
            
            DefineLibraryMethod(module, "~", 0x51, 
                0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Invert)
            );
            
            DefineLibraryMethod(module, "+", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Add), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Add), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Add), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Add)
            );
            
            DefineLibraryMethod(module, "<<", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.LeftShift), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.LeftShift), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrBigInteger.LeftShift)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Int32>(IronRuby.Builtins.ClrBigInteger.Compare), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Int32>(IronRuby.Builtins.ClrBigInteger.Compare), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Compare), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, Microsoft.Scripting.Math.BigInteger, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Compare)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.ClrBigInteger.Equal), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Boolean>(IronRuby.Builtins.ClrBigInteger.Equal), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, System.Double, System.Boolean>(IronRuby.Builtins.ClrBigInteger.Equal), 
                new Func<IronRuby.Runtime.BinaryOpStorage, Microsoft.Scripting.Math.BigInteger, System.Object, System.Boolean>(IronRuby.Builtins.ClrBigInteger.Equal)
            );
            
            DefineLibraryMethod(module, ">>", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.RightShift), 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.RightShift), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrBigInteger.RightShift)
            );
            
            DefineLibraryMethod(module, "abs", 0x51, 
                0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Abs)
            );
            
            DefineLibraryMethod(module, "coerce", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrBigInteger.Coerce), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrBigInteger.Coerce)
            );
            
            DefineLibraryMethod(module, "div", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Divide), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Divide), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Divide), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Div)
            );
            
            DefineLibraryMethod(module, "divmod", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrBigInteger.DivMod), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrBigInteger.DivMod), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrBigInteger.DivMod), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.DivMod)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.ClrBigInteger.Eql), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Boolean>(IronRuby.Builtins.ClrBigInteger.Eql), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Object, System.Boolean>(IronRuby.Builtins.ClrBigInteger.Eql)
            );
            
            DefineLibraryMethod(module, "fdiv", 0x51, 
                0x00000002U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrBigInteger.FDiv)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32>(IronRuby.Builtins.ClrBigInteger.Hash)
            );
            
            DefineLibraryMethod(module, "modulo", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Modulo), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Modulo), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Modulo)
            );
            
            DefineLibraryMethod(module, "quo", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Quotient), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Quotient), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Object>(IronRuby.Builtins.ClrBigInteger.Quotient), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Quotient)
            );
            
            DefineLibraryMethod(module, "remainder", 0x51, 
                0x00000002U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrBigInteger.Remainder), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrBigInteger.Remainder), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Double, System.Double>(IronRuby.Builtins.ClrBigInteger.Remainder), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrBigInteger.Remainder)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrBigInteger.ToFloat)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrBigInteger.ToString), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrBigInteger.ToString)
            );
            
        }
        
        private static void LoadIronRuby__Clr__FlagEnumeration_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "&", 0x51, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.FlagEnumerationOps.BitwiseAnd)
            );
            
            DefineLibraryMethod(module, "^", 0x51, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.FlagEnumerationOps.Xor)
            );
            
            DefineLibraryMethod(module, "|", 0x51, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.FlagEnumerationOps.BitwiseOr)
            );
            
            DefineLibraryMethod(module, "~", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.FlagEnumerationOps.OnesComplement)
            );
            
        }
        
        private static void LoadIronRuby__Clr__Float_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "-", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, System.Double>(IronRuby.Builtins.ClrFloat.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrFloat.Subtract), 
                new Func<System.Double, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Subtract), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.Subtract)
            );
            
            DefineLibraryMethod(module, "%", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, System.Double>(IronRuby.Builtins.ClrFloat.Modulo), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrFloat.Modulo), 
                new Func<System.Double, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.ModuloOp)
            );
            
            DefineLibraryMethod(module, "*", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, System.Double>(IronRuby.Builtins.ClrFloat.Multiply), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrFloat.Multiply), 
                new Func<System.Double, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Multiply), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.Multiply)
            );
            
            DefineLibraryMethod(module, "**", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, System.Double>(IronRuby.Builtins.ClrFloat.Power), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrFloat.Power), 
                new Func<System.Double, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Power), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.Power)
            );
            
            DefineLibraryMethod(module, "/", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, System.Double>(IronRuby.Builtins.ClrFloat.Divide), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrFloat.Divide), 
                new Func<System.Double, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Divide), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.Divide)
            );
            
            DefineLibraryMethod(module, "+", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, System.Double>(IronRuby.Builtins.ClrFloat.Add), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrFloat.Add), 
                new Func<System.Double, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Add), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.Add)
            );
            
            DefineLibraryMethod(module, "<", 0x51, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<System.Double, System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThan), 
                new Func<System.Double, System.Int32, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThan), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThan), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThan)
            );
            
            DefineLibraryMethod(module, "<=", 0x51, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<System.Double, System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThanOrEqual), 
                new Func<System.Double, System.Int32, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThanOrEqual), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Boolean>(IronRuby.Builtins.ClrFloat.LessThanOrEqual)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<System.Double, System.Double, System.Object>(IronRuby.Builtins.ClrFloat.Compare), 
                new Func<System.Double, System.Int32, System.Object>(IronRuby.Builtins.ClrFloat.Compare), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrFloat.Compare), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.Compare)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.Equal), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Boolean>(IronRuby.Builtins.ClrFloat.Equal)
            );
            
            DefineLibraryMethod(module, ">", 0x51, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<System.Double, System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThan), 
                new Func<System.Double, System.Int32, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThan), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThan), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThan)
            );
            
            DefineLibraryMethod(module, ">=", 0x51, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<System.Double, System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThanOrEqual), 
                new Func<System.Double, System.Int32, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThanOrEqual), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Boolean>(IronRuby.Builtins.ClrFloat.GreaterThanOrEqual)
            );
            
            DefineLibraryMethod(module, "abs", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Abs)
            );
            
            DefineLibraryMethod(module, "ceil", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.ClrFloat.Ceil)
            );
            
            DefineLibraryMethod(module, "coerce", 0x51, 
                0x00010000U, 
                new Func<System.Double, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrFloat.Coerce)
            );
            
            DefineLibraryMethod(module, "divmod", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrFloat.DivMod), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrFloat.DivMod), 
                new Func<System.Double, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrFloat.DivMod), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.DivMod)
            );
            
            DefineLibraryMethod(module, "finite?", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.IsFinite)
            );
            
            DefineLibraryMethod(module, "floor", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.ClrFloat.Floor)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Int32>(IronRuby.Builtins.ClrFloat.Hash)
            );
            
            DefineLibraryMethod(module, "infinite?", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.ClrFloat.IsInfinite)
            );
            
            DefineLibraryMethod(module, "modulo", 0x51, 
                0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 
                new Func<System.Double, System.Int32, System.Double>(IronRuby.Builtins.ClrFloat.Modulo), 
                new Func<IronRuby.Runtime.RubyContext, System.Double, Microsoft.Scripting.Math.BigInteger, System.Double>(IronRuby.Builtins.ClrFloat.Modulo), 
                new Func<System.Double, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Double, System.Object, System.Object>(IronRuby.Builtins.ClrFloat.Modulo)
            );
            
            DefineLibraryMethod(module, "nan?", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.IsNan)
            );
            
            DefineLibraryMethod(module, "round", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.ClrFloat.Round)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Double>(IronRuby.Builtins.ClrFloat.ToFloat)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.ClrFloat.ToInt)
            );
            
            DefineLibraryMethod(module, "to_int", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.ClrFloat.ToInt)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Double, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrFloat.ToS)
            );
            
            DefineLibraryMethod(module, "truncate", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.ClrFloat.ToInt)
            );
            
            DefineLibraryMethod(module, "zero?", 0x51, 
                0x00000000U, 
                new Func<System.Double, System.Boolean>(IronRuby.Builtins.ClrFloat.IsZero)
            );
            
        }
        
        private static void LoadIronRuby__Clr__Float_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Double, System.Double>(IronRuby.Builtins.ClrFloat.InducedFrom), 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.RubyModule, System.Int32, System.Object>(IronRuby.Builtins.ClrFloat.InducedFrom), 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.RubyModule, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrFloat.InducedFrom), 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Double>(IronRuby.Builtins.ClrFloat.InducedFrom)
            );
            
        }
        
        private static void LoadIronRuby__Clr__Integer_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "-", 0x51, 
                0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Subtract), 
                new Func<System.Int32, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrInteger.Subtract), 
                new Func<System.Int32, System.Double, System.Double>(IronRuby.Builtins.ClrInteger.Subtract), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Subtract)
            );
            
            DefineLibraryMethod(module, "%", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.ClrInteger.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.ModuloOp)
            );
            
            DefineLibraryMethod(module, "&", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.ClrInteger.BitwiseAnd), 
                new Func<System.Int32, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrInteger.BitwiseAnd), 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrInteger.BitwiseAnd)
            );
            
            DefineLibraryMethod(module, "*", 0x51, 
                0x00000000U, 0x00000002U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Multiply), 
                new Func<System.Int32, Microsoft.Scripting.Math.BigInteger, Microsoft.Scripting.Math.BigInteger>(IronRuby.Builtins.ClrInteger.Multiply), 
                new Func<System.Int32, System.Double, System.Double>(IronRuby.Builtins.ClrInteger.Multiply), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Multiply)
            );
            
            DefineLibraryMethod(module, "**", 0x51, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Power), 
                new Func<System.Int32, System.Double, System.Double>(IronRuby.Builtins.ClrInteger.Power), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyContext, System.Int32, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Power)
            );
            
            DefineLibraryMethod(module, "/", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Divide), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.DivideOp)
            );
            
            DefineLibraryMethod(module, "-@", 0x51, 
                0x00000000U, 
                new Func<System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Minus)
            );
            
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00010000U, 0x00000002U, 
                new Func<System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.ClrInteger.Bit), 
                new Func<System.Int32, Microsoft.Scripting.Math.BigInteger, System.Int32>(IronRuby.Builtins.ClrInteger.Bit)
            );
            
            DefineLibraryMethod(module, "^", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.BitwiseXor), 
                new Func<System.Int32, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrInteger.BitwiseXor), 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrInteger.BitwiseXor)
            );
            
            DefineLibraryMethod(module, "|", 0x51, 
                0x00000000U, 0x00000002U, 0x00020000U, 
                new Func<System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.ClrInteger.BitwiseOr), 
                new Func<System.Int32, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrInteger.BitwiseOr), 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrInteger.BitwiseOr)
            );
            
            DefineLibraryMethod(module, "~", 0x51, 
                0x00000000U, 
                new Func<System.Int32, System.Int32>(IronRuby.Builtins.ClrInteger.OnesComplement)
            );
            
            DefineLibraryMethod(module, "+", 0x51, 
                0x00000000U, 0x00000002U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Add), 
                new Func<System.Int32, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.Builtins.ClrInteger.Add), 
                new Func<System.Int32, System.Double, System.Double>(IronRuby.Builtins.ClrInteger.Add), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Add)
            );
            
            DefineLibraryMethod(module, "<", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Boolean>(IronRuby.Builtins.ClrInteger.LessThan), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.ClrInteger.LessThan)
            );
            
            DefineLibraryMethod(module, "<<", 0x51, 
                0x00000000U, 0x00020000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.LeftShift), 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrInteger.LeftShift)
            );
            
            DefineLibraryMethod(module, "<=", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Boolean>(IronRuby.Builtins.ClrInteger.LessThanOrEqual), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.ClrInteger.LessThanOrEqual)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.ClrInteger.Compare), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Compare)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Boolean>(IronRuby.Builtins.ClrInteger.Equal), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Int32, System.Object, System.Boolean>(IronRuby.Builtins.ClrInteger.Equal)
            );
            
            DefineLibraryMethod(module, ">", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Boolean>(IronRuby.Builtins.ClrInteger.GreaterThan), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.ClrInteger.GreaterThan)
            );
            
            DefineLibraryMethod(module, ">=", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Boolean>(IronRuby.Builtins.ClrInteger.GreaterThanOrEqual), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.ClrInteger.GreaterThanOrEqual)
            );
            
            DefineLibraryMethod(module, ">>", 0x51, 
                0x00000000U, 0x00020000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.RightShift), 
                new Func<IronRuby.Runtime.RubyContext, System.Int32, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.ClrInteger.RightShift)
            );
            
            DefineLibraryMethod(module, "abs", 0x51, 
                0x00000000U, 
                new Func<System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Abs)
            );
            
            DefineLibraryMethod(module, "div", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.Divide), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Divide)
            );
            
            DefineLibraryMethod(module, "divmod", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrInteger.DivMod), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Int32, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.DivMod)
            );
            
            DefineLibraryMethod(module, "fdiv", 0x51, 
                0x00010000U, 
                new Func<System.Int32, System.Int32, System.Double>(IronRuby.Builtins.ClrInteger.FDiv)
            );
            
            DefineLibraryMethod(module, "modulo", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.ClrInteger.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Modulo)
            );
            
            DefineLibraryMethod(module, "quo", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, System.Double>(IronRuby.Builtins.ClrInteger.Quotient), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Int32, System.Object, System.Object>(IronRuby.Builtins.ClrInteger.Quotient)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<System.Int32, System.Double>(IronRuby.Builtins.ClrInteger.ToFloat)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.ClrInteger.ToString), 
                new Func<Microsoft.Scripting.Math.BigInteger, System.Int32, System.Object>(IronRuby.Builtins.ClrInteger.ToString)
            );
            
            DefineLibraryMethod(module, "zero?", 0x51, 
                0x00000000U, 
                new Func<System.Int32, System.Boolean>(IronRuby.Builtins.ClrInteger.IsZero)
            );
            
        }
        
        private static void LoadIronRuby__Clr__Name_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<=>", 0x51, 
                new[] { 0x00010002U, 0x00000002U, 0x00000004U, 0x00000004U, 0x00000000U}, 
                new Func<IronRuby.Runtime.ClrName, System.String, System.Int32>(IronRuby.Builtins.ClrNameOps.Compare), 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Runtime.ClrName, System.Int32>(IronRuby.Builtins.ClrNameOps.Compare), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.ClrNameOps.Compare), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubySymbol, System.Int32>(IronRuby.Builtins.ClrNameOps.Compare), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.ClrName, System.Object, System.Object>(IronRuby.Builtins.ClrNameOps.Compare)
            );
            
            DefineLibraryMethod(module, "=~", 0x51, 
                0x00000004U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.ClrNameOps.Match), 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Runtime.ClrName, System.Object>(IronRuby.Builtins.ClrNameOps.Match), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.ClrName, System.Object, System.Object>(IronRuby.Builtins.ClrNameOps.Match)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00010002U, 0x00000002U, 0x00000004U, 0x00000002U, 
                new Func<IronRuby.Runtime.ClrName, System.String, System.Boolean>(IronRuby.Builtins.ClrNameOps.IsEqual), 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.ClrNameOps.IsEqual), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubySymbol, System.Boolean>(IronRuby.Builtins.ClrNameOps.IsEqual), 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Runtime.ClrName, System.Boolean>(IronRuby.Builtins.ClrNameOps.IsEqual)
            );
            
            DefineLibraryMethod(module, "clr_name", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.GetClrName)
            );
            
            DefineLibraryMethod(module, "dump", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.Dump)
            );
            
            DefineLibraryMethod(module, "empty?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ClrName, System.Boolean>(IronRuby.Builtins.ClrNameOps.IsEmpty)
            );
            
            DefineLibraryMethod(module, "encoding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.ClrNameOps.GetEncoding)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.Inspect)
            );
            
            DefineLibraryMethod(module, "intern", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.ClrNameOps.ToSymbol)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ClrName, System.Int32>(IronRuby.Builtins.ClrNameOps.GetLength)
            );
            
            DefineLibraryMethod(module, "match", 0x51, 
                0x00000008U, 0x00040008U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.ClrNameOps.Match), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.ClrNameOps.Match)
            );
            
            DefineLibraryMethod(module, "ruby_name", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.GetRubyName)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ClrName, System.Int32>(IronRuby.Builtins.ClrNameOps.GetLength)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.GetRubyName)
            );
            
            DefineLibraryMethod(module, "to_str", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.GetRubyName)
            );
            
            DefineLibraryMethod(module, "to_sym", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.ClrNameOps.ToSymbol)
            );
            
        }
        
        private static void LoadIronRuby__Clr__Name_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "clr_to_ruby", 0x61, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyClass, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.Mangle)
            );
            
            DefineLibraryMethod(module, "mangle", 0x61, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyClass, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.Mangle)
            );
            
            DefineLibraryMethod(module, "ruby_to_clr", 0x61, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyClass, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.Unmangle)
            );
            
            DefineLibraryMethod(module, "unmangle", 0x61, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyClass, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrNameOps.Unmangle)
            );
            
        }
        
        private static void LoadIronRuby__Clr__String_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "%", 0x51, 
                0x00000004U, 0x00000000U, 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, System.String, System.Collections.IList, System.String>(IronRuby.Builtins.ClrString.Format), 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.String, System.Object, System.String>(IronRuby.Builtins.ClrString.Format)
            );
            
            DefineLibraryMethod(module, "*", 0x51, 
                0x00010000U, 
                new Func<System.String, System.Int32, System.String>(IronRuby.Builtins.ClrString.Repeat)
            );
            
            DefineLibraryMethod(module, "[]", 0x51, 
                new[] { 0x00010000U, 0x00030000U, 0x00000004U, 0x00000002U, 0x00000004U, 0x00040004U}, 
                new Func<System.String, System.Int32, System.Object>(IronRuby.Builtins.ClrString.GetChar), 
                new Func<System.String, System.Int32, System.Int32, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, System.String, IronRuby.Builtins.Range, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<System.String, System.String, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, System.String, IronRuby.Builtins.RubyRegex, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, System.String, IronRuby.Builtins.RubyRegex, System.Int32, System.String>(IronRuby.Builtins.ClrString.GetSubstring)
            );
            
            DefineLibraryMethod(module, "+", 0x51, 
                0x00010002U, 
                new Func<System.String, IronRuby.Builtins.MutableString, System.String>(IronRuby.Builtins.ClrString.Concatenate)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<System.String, System.String, System.Int32>(IronRuby.Builtins.ClrString.Compare), 
                new Func<System.String, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.ClrString.Compare), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RespondToStorage, System.String, System.Object, System.Object>(IronRuby.Builtins.ClrString.Compare)
            );
            
            DefineLibraryMethod(module, "=~", 0x51, 
                0x00000004U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.String, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.ClrString.Match), 
                new Func<System.String, System.String, System.Object>(IronRuby.Builtins.ClrString.Match), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object>>, IronRuby.Runtime.RubyScope, System.String, System.Object, System.Object>(IronRuby.Builtins.ClrString.Match)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<System.String, System.String, System.Boolean>(IronRuby.Builtins.ClrString.StringEquals), 
                new Func<System.String, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.ClrString.StringEquals), 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, System.String, System.Object, System.Boolean>(IronRuby.Builtins.ClrString.Equals)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<System.String, System.String, System.Boolean>(IronRuby.Builtins.ClrString.StringEquals), 
                new Func<System.String, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.ClrString.StringEquals), 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, System.String, System.Object, System.Boolean>(IronRuby.Builtins.ClrString.Equals)
            );
            
            DefineLibraryMethod(module, "dump", 0x51, 
                0x00000000U, 
                new Func<System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrString.Dump)
            );
            
            DefineLibraryMethod(module, "empty?", 0x51, 
                0x00000000U, 
                new Func<System.String, System.Boolean>(IronRuby.Builtins.ClrString.IsEmpty)
            );
            
            DefineLibraryMethod(module, "encoding", 0x51, 
                0x00000000U, 
                new Func<System.String, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.ClrString.GetEncoding)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<System.String, System.String, System.Boolean>(IronRuby.Builtins.ClrString.Eql), 
                new Func<System.String, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.ClrString.Eql), 
                new Func<System.String, System.Object, System.Boolean>(IronRuby.Builtins.ClrString.Eql)
            );
            
            DefineLibraryMethod(module, "hex", 0x51, 
                0x00000000U, 
                new Func<System.String, System.Object>(IronRuby.Builtins.ClrString.ToIntegerHex)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00010002U, 
                new Func<System.String, System.String, System.Boolean>(IronRuby.Builtins.ClrString.Include)
            );
            
            DefineLibraryMethod(module, "insert", 0x51, 
                0x00030004U, 
                new Func<System.String, System.Int32, System.String, System.String>(IronRuby.Builtins.ClrString.Insert)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrString.Inspect)
            );
            
            DefineLibraryMethod(module, "intern", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.String, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.ClrString.ToSymbol)
            );
            
            DefineLibraryMethod(module, "method_missing", 0x52, 
                0x80000008U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.String, IronRuby.Builtins.RubySymbol, System.Object[], System.Object>(IronRuby.Builtins.ClrString.MethodMissing)
            );
            
            DefineLibraryMethod(module, "oct", 0x51, 
                0x00000000U, 
                new Func<System.String, System.Object>(IronRuby.Builtins.ClrString.ToIntegerOctal)
            );
            
            DefineLibraryMethod(module, "reverse", 0x51, 
                0x00000000U, 
                new Func<System.String, System.String>(IronRuby.Builtins.ClrString.GetReversed)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.String, System.Int32>(IronRuby.Builtins.ClrString.GetLength)
            );
            
            DefineLibraryMethod(module, "slice", 0x51, 
                new[] { 0x00010000U, 0x00030000U, 0x00000004U, 0x00000002U, 0x00000004U, 0x00040004U}, 
                new Func<System.String, System.Int32, System.Object>(IronRuby.Builtins.ClrString.GetChar), 
                new Func<System.String, System.Int32, System.Int32, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, System.String, IronRuby.Builtins.Range, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<System.String, System.String, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, System.String, IronRuby.Builtins.RubyRegex, System.String>(IronRuby.Builtins.ClrString.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, System.String, IronRuby.Builtins.RubyRegex, System.Int32, System.String>(IronRuby.Builtins.ClrString.GetSubstring)
            );
            
            DefineLibraryMethod(module, "split", 0x51, 
                0x00000000U, 0x00060000U, 0x00040004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.String, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrString.Split), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.String, System.String, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrString.Split), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.String, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ClrString.Split)
            );
            
            DefineLibraryMethod(module, "to_clr_string", 0x51, 
                0x00000000U, 
                new Func<System.String, System.String>(IronRuby.Builtins.ClrString.ToClrString)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<System.String, System.Double>(IronRuby.Builtins.ClrString.ToDouble)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00010000U, 
                new Func<System.String, System.Int32, System.Object>(IronRuby.Builtins.ClrString.ToInteger)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrString.ToStr)
            );
            
            DefineLibraryMethod(module, "to_str", 0x51, 
                0x00000000U, 
                new Func<System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ClrString.ToStr)
            );
            
            DefineLibraryMethod(module, "to_sym", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.String, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.ClrString.ToSymbol)
            );
            
        }
        
        private static void LoadIronRuby__Print_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<<", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.PrintOps.Output)
            );
            
            DefineLibraryMethod(module, "print", 0x51, 
                0x00000000U, 0x80000000U, 0x00000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, System.Object>(IronRuby.Builtins.PrintOps.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object[]>(IronRuby.Builtins.PrintOps.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.PrintOps.Print)
            );
            
            DefineLibraryMethod(module, "printf", 0x51, 
                0x80080010U, 
                new Action<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString, System.Object[]>(IronRuby.Builtins.PrintOps.PrintFormatted)
            );
            
            DefineLibraryMethod(module, "putc", 0x51, 
                0x00000004U, 0x00020000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.PrintOps.Putc), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Int32, System.Int32>(IronRuby.Builtins.PrintOps.Putc)
            );
            
            DefineLibraryMethod(module, "puts", 0x51, 
                0x00000000U, 0x00000004U, 0x00000010U, 0x80000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object>(IronRuby.Builtins.PrintOps.PutsEmptyLine), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.PrintOps.Puts), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object>(IronRuby.Builtins.PrintOps.Puts), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object[]>(IronRuby.Builtins.PrintOps.Puts)
            );
            
        }
        
        private static void LoadKernel_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "!~", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.NotMatch)
            );
            
            DefineLibraryMethod(module, "__id__", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.GetObjectId)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "`", 0x52, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.ExecuteCommand)
            );
            
            #endif
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Compare)
            );
            
            DefineLibraryMethod(module, "=~", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Match)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.CaseEquals)
            );
            
            DefineLibraryMethod(module, "abort", 0x52, 
                0x00000000U, 0x00020004U, 
                new Action<System.Object>(IronRuby.Builtins.KernelOps.Abort), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Abort)
            );
            
            DefineLibraryMethod(module, "Array", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object, System.Collections.IList>(IronRuby.Builtins.KernelOps.ToArray)
            );
            
            DefineLibraryMethod(module, "at_exit", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.AtExit)
            );
            
            DefineLibraryMethod(module, "autoload", 0x52, 
                0x0006000cU, 
                new Action<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.SetAutoloadedConstant)
            );
            
            DefineLibraryMethod(module, "autoload?", 0x52, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.GetAutoloadedConstantPath)
            );
            
            DefineLibraryMethod(module, "binding", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.Binding>(IronRuby.Builtins.KernelOps.GetLocalScope)
            );
            
            DefineLibraryMethod(module, "block_given?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.HasBlock)
            );
            
            DefineLibraryMethod(module, "caller", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetStackTrace)
            );
            
            DefineLibraryMethod(module, "catch", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Catch)
            );
            
            DefineLibraryMethod(module, "class", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.KernelOps.GetClass)
            );
            
            DefineLibraryMethod(module, "clone", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object>>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Clone)
            );
            
            DefineLibraryMethod(module, "clr_member", 0x51, 
                0x0004000cU, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.String, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.KernelOps.GetClrMember)
            );
            
            DefineLibraryMethod(module, "Complex", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToComplex)
            );
            
            DefineLibraryMethod(module, "define_singleton_method", 0x51, 
                new[] { 0x0000000cU, 0x0002000cU, 0x0000000cU, 0x0002000cU, 0x0000000cU, 0x0004000aU, 0x0000000aU, 0x0002000cU}, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.ClrName, IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.DefineSingletonMethod), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.KernelOps.DefineSingletonMethod), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.KernelOps.DefineSingletonMethod), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.KernelOps.DefineSingletonMethod), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.ClrName, IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.KernelOps.DefineSingletonMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.DefineSingletonMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Runtime.ClrName, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.DefineSingletonMethod), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.DefineSingletonMethod)
            );
            
            DefineLibraryMethod(module, "display", 0x51, 
                0x00000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object>(IronRuby.Builtins.KernelOps.Display)
            );
            
            DefineLibraryMethod(module, "dup", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object>>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Duplicate)
            );
            
            DefineLibraryMethod(module, "enum_for", 0x51, 
                0x80010002U, 
                new Func<System.Object, System.String, System.Object[], IronRuby.Builtins.Enumerator>(IronRuby.Builtins.KernelOps.Create)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000001U, 0x00000000U, 
                new Func<IronRuby.Runtime.IRubyObject, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.ValueEquals), 
                new Func<System.Object, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.ValueEquals)
            );
            
            DefineLibraryMethod(module, "eval", 0x52, 
                0x00000014U, 0x0000001cU, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.Binding, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Evaluate), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.Proc, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Evaluate)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "exec", 0x52, 
                0x00020004U, 0x8006000cU, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Execute), 
                new Action<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[]>(IronRuby.Builtins.KernelOps.Execute)
            );
            
            #endif
            DefineLibraryMethod(module, "exit", 0x52, 
                0x00000000U, 0x00000002U, 0x00010000U, 
                new Action<System.Object>(IronRuby.Builtins.KernelOps.Exit), 
                new Action<System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.Exit), 
                new Action<System.Object, System.Int32>(IronRuby.Builtins.KernelOps.Exit)
            );
            
            DefineLibraryMethod(module, "exit!", 0x52, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.KernelOps.TerminateExecution), 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.TerminateExecution), 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.KernelOps.TerminateExecution)
            );
            
            DefineLibraryMethod(module, "extend", 0x51, 
                0x80000018U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyModule, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyModule, System.Object, System.Object>>, System.Object, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule[], System.Object>(IronRuby.Builtins.KernelOps.Extend)
            );
            
            DefineLibraryMethod(module, "fail", 0x52, 
                0x00000000U, 0x00000002U, 0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.CallSiteStorage<Action<System.Runtime.CompilerServices.CallSite, System.Exception, IronRuby.Builtins.RubyArray>>, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.RaiseException)
            );
            
            DefineLibraryMethod(module, "Float", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.KernelOps.ToFloat)
            );
            
            DefineLibraryMethod(module, "format", 0x52, 
                0x80020004U, 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, System.Object, IronRuby.Builtins.MutableString, System.Object[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Sprintf)
            );
            
            DefineLibraryMethod(module, "freeze", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Freeze)
            );
            
            DefineLibraryMethod(module, "frozen?", 0x51, 
                0x00000001U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.KernelOps.Frozen), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.Frozen)
            );
            
            DefineLibraryMethod(module, "gets", 0x52, 
                0x00000000U, 0x00000004U, 0x00040004U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ReadInputLine), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.KernelOps.ReadInputLine), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.ReadInputLine)
            );
            
            DefineLibraryMethod(module, "global_variables", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetGlobalVariableNames)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000001U, 0x00000000U, 
                new Func<IronRuby.Runtime.IRubyObject, System.Int32>(IronRuby.Builtins.KernelOps.Hash), 
                new Func<System.Object, System.Int32>(IronRuby.Builtins.KernelOps.Hash)
            );
            
            DefineLibraryMethod(module, "id", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.GetId)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.InitializeCopy)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Inspect)
            );
            
            DefineLibraryMethod(module, "instance_of?", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.RubyModule, System.Boolean>(IronRuby.Builtins.KernelOps.IsOfClass)
            );
            
            DefineLibraryMethod(module, "instance_variable_defined?", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Boolean>(IronRuby.Builtins.KernelOps.InstanceVariableDefined)
            );
            
            DefineLibraryMethod(module, "instance_variable_get", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Object>(IronRuby.Builtins.KernelOps.InstanceVariableGet)
            );
            
            DefineLibraryMethod(module, "instance_variable_set", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Object, System.Object>(IronRuby.Builtins.KernelOps.InstanceVariableSet)
            );
            
            DefineLibraryMethod(module, "instance_variables", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetInstanceVariableNames)
            );
            
            DefineLibraryMethod(module, "Integer", 0x52, 
                0x00000002U, 0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.KernelOps.ToInteger), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Runtime.IntegerValue>, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToInteger)
            );
            
            DefineLibraryMethod(module, "is_a?", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.RubyModule, System.Boolean>(IronRuby.Builtins.KernelOps.IsKindOf)
            );
            
            DefineLibraryMethod(module, "iterator?", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.HasBlock)
            );
            
            DefineLibraryMethod(module, "kind_of?", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.RubyModule, System.Boolean>(IronRuby.Builtins.KernelOps.IsKindOf)
            );
            
            DefineLibraryMethod(module, "lambda", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.CreateLambda)
            );
            
            DefineLibraryMethod(module, "load", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Boolean, System.Boolean>(IronRuby.Builtins.KernelOps.Load)
            );
            
            DefineLibraryMethod(module, "load_assembly", 0x52, 
                0x0006000cU, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.KernelOps.LoadAssembly)
            );
            
            DefineLibraryMethod(module, "local_variables", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetLocalVariableNames)
            );
            
            DefineLibraryMethod(module, "loop", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Loop)
            );
            
            DefineLibraryMethod(module, "method", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.String, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.KernelOps.GetMethod)
            );
            
            DefineLibraryMethod(module, "methods", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetMethods)
            );
            
            DefineLibraryMethod(module, "nil?", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.IsNil)
            );
            
            DefineLibraryMethod(module, "object_id", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.GetObjectId)
            );
            
            DefineLibraryMethod(module, "open", 0x52, 
                0x000e0004U, 0x001c000aU, 0x000a0004U, 0x0014000aU, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.KernelOps.Open), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Open), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.KernelOps.Open), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Open)
            );
            
            DefineLibraryMethod(module, "p", 0x52, 
                0x80000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Object[], System.Object>(IronRuby.Builtins.KernelOps.PrintInspect)
            );
            
            DefineLibraryMethod(module, "print", 0x52, 
                0x00000000U, 0x00000000U, 0x80000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, System.Object>(IronRuby.Builtins.KernelOps.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object[]>(IronRuby.Builtins.KernelOps.Print)
            );
            
            DefineLibraryMethod(module, "printf", 0x52, 
                0x80000010U, 0x80000020U, 
                new Action<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString, System.Object[]>(IronRuby.Builtins.KernelOps.PrintFormatted), 
                new Action<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object, System.Object[]>(IronRuby.Builtins.KernelOps.PrintFormatted)
            );
            
            DefineLibraryMethod(module, "private_methods", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetPrivateMethods)
            );
            
            DefineLibraryMethod(module, "proc", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.CreateProc)
            );
            
            DefineLibraryMethod(module, "protected_methods", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetProtectedMethods)
            );
            
            DefineLibraryMethod(module, "public_methods", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetPublicMethods)
            );
            
            DefineLibraryMethod(module, "putc", 0x52, 
                0x00000004U, 0x00020000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Putc), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Int32, System.Int32>(IronRuby.Builtins.KernelOps.Putc)
            );
            
            DefineLibraryMethod(module, "puts", 0x52, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x80000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object>(IronRuby.Builtins.KernelOps.PutsEmptyLine), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.PutString), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.PutString), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object[]>(IronRuby.Builtins.KernelOps.PutString)
            );
            
            DefineLibraryMethod(module, "raise", 0x52, 
                0x00000000U, 0x00000002U, 0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.CallSiteStorage<Action<System.Runtime.CompilerServices.CallSite, System.Exception, IronRuby.Builtins.RubyArray>>, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.RaiseException)
            );
            
            DefineLibraryMethod(module, "rand", 0x52, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Double>(IronRuby.Builtins.KernelOps.Random), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Random), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Runtime.IntegerValue>, IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Random)
            );
            
            DefineLibraryMethod(module, "Rational", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToRational)
            );
            
            DefineLibraryMethod(module, "remove_instance_variable", 0x52, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Object>(IronRuby.Builtins.KernelOps.RemoveInstanceVariable)
            );
            
            DefineLibraryMethod(module, "require", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.Require)
            );
            
            DefineLibraryMethod(module, "respond_to?", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Boolean, System.Boolean>(IronRuby.Builtins.KernelOps.RespondTo)
            );
            
            DefineLibraryMethod(module, "select", 0x52, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.Select), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.Select), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.Select)
            );
            
            DefineLibraryMethod(module, "send", 0x51, 
                new[] { 0x00000000U, 0x00020004U, 0x00040008U, 0x00020004U, 0x00040008U, 0x00020004U, 0x00040008U, 0x00020004U, 0x00040008U, 0x80020004U, 0x80040008U}, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object[], System.Object>(IronRuby.Builtins.KernelOps.SendMessage), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.String, System.Object[], System.Object>(IronRuby.Builtins.KernelOps.SendMessage)
            );
            
            DefineLibraryMethod(module, "set_trace_func", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.SetTraceListener)
            );
            
            DefineLibraryMethod(module, "singleton_methods", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetSingletonMethods)
            );
            
            DefineLibraryMethod(module, "sleep", 0x52, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Action<System.Object>(IronRuby.Builtins.KernelOps.Sleep), 
                new Func<System.Object, System.Int32, System.Int32>(IronRuby.Builtins.KernelOps.Sleep), 
                new Func<System.Object, System.Double, System.Int32>(IronRuby.Builtins.KernelOps.Sleep)
            );
            
            DefineLibraryMethod(module, "sprintf", 0x52, 
                0x80020004U, 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, System.Object, IronRuby.Builtins.MutableString, System.Object[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Sprintf)
            );
            
            DefineLibraryMethod(module, "srand", 0x52, 
                0x00000000U, 0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SeedRandomNumberGenerator), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.KernelOps.SeedRandomNumberGenerator)
            );
            
            DefineLibraryMethod(module, "String", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToString)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "system", 0x52, 
                0x00020004U, 0x8006000cU, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.KernelOps.System), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], System.Boolean>(IronRuby.Builtins.KernelOps.System)
            );
            
            #endif
            DefineLibraryMethod(module, "taint", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Taint)
            );
            
            DefineLibraryMethod(module, "tainted?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.Tainted)
            );
            
            DefineLibraryMethod(module, "tap", 0x51, 
                0x00000002U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Tap)
            );
            
            DefineLibraryMethod(module, "test", 0x52, 
                0x00000004U, 0x00020000U, 0x000c0018U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.Builtins.MutableString, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Test), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Int32, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Test), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.KernelOps.Test)
            );
            
            DefineLibraryMethod(module, "throw", 0x52, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Throw)
            );
            
            DefineLibraryMethod(module, "to_enum", 0x51, 
                0x80010002U, 
                new Func<System.Object, System.String, System.Object[], IronRuby.Builtins.Enumerator>(IronRuby.Builtins.KernelOps.Create)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000001U, 0x00000000U, 
                new Func<IronRuby.Runtime.IRubyObject, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.ToS), 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.ToS)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "trap", 0x52, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.KernelOps.Trap), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Trap)
            );
            
            #endif
            DefineLibraryMethod(module, "trust", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Trust)
            );
            
            DefineLibraryMethod(module, "type", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.KernelOps.GetClassObsolete)
            );
            
            DefineLibraryMethod(module, "untaint", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Untaint)
            );
            
            DefineLibraryMethod(module, "untrust", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Untrust)
            );
            
            DefineLibraryMethod(module, "untrusted?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.Untrusted)
            );
            
            DefineLibraryMethod(module, "using_clr_extensions", 0x52, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.KernelOps.UsingClrExtensions)
            );
            
            DefineLibraryMethod(module, "warn", 0x52, 
                0x00000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ReportWarning)
            );
            
        }
        
        private static void LoadKernel_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "`", 0x61, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.ExecuteCommand)
            );
            
            #endif
            DefineLibraryMethod(module, "abort", 0x61, 
                0x00000000U, 0x00020004U, 
                new Action<System.Object>(IronRuby.Builtins.KernelOps.Abort), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Abort)
            );
            
            DefineLibraryMethod(module, "Array", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object, System.Collections.IList>(IronRuby.Builtins.KernelOps.ToArray)
            );
            
            DefineLibraryMethod(module, "at_exit", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.AtExit)
            );
            
            DefineLibraryMethod(module, "autoload", 0x61, 
                0x0006000cU, 
                new Action<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.SetAutoloadedConstant)
            );
            
            DefineLibraryMethod(module, "autoload?", 0x61, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.GetAutoloadedConstantPath)
            );
            
            DefineLibraryMethod(module, "binding", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.Binding>(IronRuby.Builtins.KernelOps.GetLocalScope)
            );
            
            DefineLibraryMethod(module, "block_given?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.HasBlock)
            );
            
            DefineLibraryMethod(module, "caller", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetStackTrace)
            );
            
            DefineLibraryMethod(module, "catch", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Catch)
            );
            
            DefineLibraryMethod(module, "Complex", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToComplex)
            );
            
            DefineLibraryMethod(module, "eval", 0x61, 
                0x00000014U, 0x0000001cU, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.Binding, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Evaluate), 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.Proc, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Evaluate)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "exec", 0x61, 
                0x00020004U, 0x8006000cU, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Execute), 
                new Action<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[]>(IronRuby.Builtins.KernelOps.Execute)
            );
            
            #endif
            DefineLibraryMethod(module, "exit", 0x61, 
                0x00000000U, 0x00000002U, 0x00010000U, 
                new Action<System.Object>(IronRuby.Builtins.KernelOps.Exit), 
                new Action<System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.Exit), 
                new Action<System.Object, System.Int32>(IronRuby.Builtins.KernelOps.Exit)
            );
            
            DefineLibraryMethod(module, "exit!", 0x61, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.KernelOps.TerminateExecution), 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.TerminateExecution), 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Int32>(IronRuby.Builtins.KernelOps.TerminateExecution)
            );
            
            DefineLibraryMethod(module, "fail", 0x61, 
                0x00000000U, 0x00000002U, 0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.CallSiteStorage<Action<System.Runtime.CompilerServices.CallSite, System.Exception, IronRuby.Builtins.RubyArray>>, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.RaiseException)
            );
            
            DefineLibraryMethod(module, "Float", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.KernelOps.ToFloat)
            );
            
            DefineLibraryMethod(module, "format", 0x61, 
                0x80020004U, 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, System.Object, IronRuby.Builtins.MutableString, System.Object[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Sprintf)
            );
            
            DefineLibraryMethod(module, "gets", 0x61, 
                0x00000000U, 0x00000004U, 0x00040004U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ReadInputLine), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.KernelOps.ReadInputLine), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.ReadInputLine)
            );
            
            DefineLibraryMethod(module, "global_variables", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetGlobalVariableNames)
            );
            
            DefineLibraryMethod(module, "Integer", 0x61, 
                0x00000002U, 0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.KernelOps.ToInteger), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Runtime.IntegerValue>, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToInteger)
            );
            
            DefineLibraryMethod(module, "iterator?", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.HasBlock)
            );
            
            DefineLibraryMethod(module, "lambda", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.CreateLambda)
            );
            
            DefineLibraryMethod(module, "load", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Boolean, System.Boolean>(IronRuby.Builtins.KernelOps.Load)
            );
            
            DefineLibraryMethod(module, "load_assembly", 0x61, 
                0x0006000cU, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.KernelOps.LoadAssembly)
            );
            
            DefineLibraryMethod(module, "local_variables", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.GetLocalVariableNames)
            );
            
            DefineLibraryMethod(module, "loop", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Loop)
            );
            
            DefineLibraryMethod(module, "open", 0x61, 
                0x000e0004U, 0x001c000aU, 0x000a0004U, 0x0014000aU, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.KernelOps.Open), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Open), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.RubyIO>(IronRuby.Builtins.KernelOps.Open), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Open)
            );
            
            DefineLibraryMethod(module, "p", 0x61, 
                0x80000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Object[], System.Object>(IronRuby.Builtins.KernelOps.PrintInspect)
            );
            
            DefineLibraryMethod(module, "print", 0x61, 
                0x00000000U, 0x00000000U, 0x80000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, System.Object>(IronRuby.Builtins.KernelOps.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object[]>(IronRuby.Builtins.KernelOps.Print)
            );
            
            DefineLibraryMethod(module, "printf", 0x61, 
                0x80000010U, 0x80000020U, 
                new Action<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString, System.Object[]>(IronRuby.Builtins.KernelOps.PrintFormatted), 
                new Action<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object, System.Object[]>(IronRuby.Builtins.KernelOps.PrintFormatted)
            );
            
            DefineLibraryMethod(module, "proc", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Object, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.CreateProc)
            );
            
            DefineLibraryMethod(module, "putc", 0x61, 
                0x00000004U, 0x00020000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Putc), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Int32, System.Int32>(IronRuby.Builtins.KernelOps.Putc)
            );
            
            DefineLibraryMethod(module, "puts", 0x61, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x80000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object>(IronRuby.Builtins.KernelOps.PutsEmptyLine), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.PutString), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.PutString), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object[]>(IronRuby.Builtins.KernelOps.PutString)
            );
            
            DefineLibraryMethod(module, "raise", 0x61, 
                0x00000000U, 0x00000002U, 0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.RaiseException), 
                new Action<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.CallSiteStorage<Action<System.Runtime.CompilerServices.CallSite, System.Exception, IronRuby.Builtins.RubyArray>>, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.RaiseException)
            );
            
            DefineLibraryMethod(module, "rand", 0x61, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Double>(IronRuby.Builtins.KernelOps.Random), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32, System.Object>(IronRuby.Builtins.KernelOps.Random), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Runtime.IntegerValue>, IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Random)
            );
            
            DefineLibraryMethod(module, "Rational", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToRational)
            );
            
            DefineLibraryMethod(module, "require", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.KernelOps.Require)
            );
            
            DefineLibraryMethod(module, "select", 0x61, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.Select), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.Select), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.KernelOps.Select)
            );
            
            DefineLibraryMethod(module, "set_trace_func", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.KernelOps.SetTraceListener)
            );
            
            DefineLibraryMethod(module, "sleep", 0x61, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Action<System.Object>(IronRuby.Builtins.KernelOps.Sleep), 
                new Func<System.Object, System.Int32, System.Int32>(IronRuby.Builtins.KernelOps.Sleep), 
                new Func<System.Object, System.Double, System.Int32>(IronRuby.Builtins.KernelOps.Sleep)
            );
            
            DefineLibraryMethod(module, "sprintf", 0x61, 
                0x80020004U, 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, System.Object, IronRuby.Builtins.MutableString, System.Object[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.KernelOps.Sprintf)
            );
            
            DefineLibraryMethod(module, "srand", 0x61, 
                0x00000000U, 0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object>(IronRuby.Builtins.KernelOps.SeedRandomNumberGenerator), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Runtime.IntegerValue, System.Object>(IronRuby.Builtins.KernelOps.SeedRandomNumberGenerator)
            );
            
            DefineLibraryMethod(module, "String", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ToString)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "system", 0x61, 
                0x00020004U, 0x8006000cU, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.KernelOps.System), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], System.Boolean>(IronRuby.Builtins.KernelOps.System)
            );
            
            #endif
            DefineLibraryMethod(module, "test", 0x61, 
                0x00000004U, 0x00020000U, 0x000c0018U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, IronRuby.Builtins.MutableString, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Test), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Int32, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Test), 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.KernelOps.Test)
            );
            
            DefineLibraryMethod(module, "throw", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Throw)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "trap", 0x61, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.KernelOps.Trap), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.KernelOps.Trap)
            );
            
            #endif
            DefineLibraryMethod(module, "using_clr_extensions", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.KernelOps.UsingClrExtensions)
            );
            
            DefineLibraryMethod(module, "warn", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Object, System.Object>(IronRuby.Builtins.KernelOps.ReportWarning)
            );
            
        }
        
        private static void LoadMarshal_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "MAJOR_VERSION", IronRuby.Builtins.RubyMarshal.MAJOR_VERSION);
            SetBuiltinConstant(module, "MINOR_VERSION", IronRuby.Builtins.RubyMarshal.MINOR_VERSION);
            
        }
        
        private static void LoadMarshal_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "dump", 0x61, 
                0x00000000U, 0x00000000U, 0x00000008U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyMarshal.WriterSites, IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyMarshal.Dump), 
                new Func<IronRuby.Builtins.RubyMarshal.WriterSites, IronRuby.Builtins.RubyModule, System.Object, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyMarshal.Dump), 
                new Func<IronRuby.Builtins.RubyMarshal.WriterSites, IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.RubyIO, System.Nullable<System.Int32>, System.Object>(IronRuby.Builtins.RubyMarshal.Dump), 
                new Func<IronRuby.Builtins.RubyMarshal.WriterSites, IronRuby.Runtime.RespondToStorage, IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Nullable<System.Int32>, System.Object>(IronRuby.Builtins.RubyMarshal.Dump)
            );
            
            DefineLibraryMethod(module, "load", 0x61, 
                0x00000008U, 0x00000008U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyMarshal.ReaderSites, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.RubyMarshal.Load), 
                new Func<IronRuby.Builtins.RubyMarshal.ReaderSites, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyIO, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.RubyMarshal.Load), 
                new Func<IronRuby.Builtins.RubyMarshal.ReaderSites, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.RubyMarshal.Load)
            );
            
            DefineLibraryMethod(module, "restore", 0x61, 
                0x00000008U, 0x00000008U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyMarshal.ReaderSites, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.RubyMarshal.Load), 
                new Func<IronRuby.Builtins.RubyMarshal.ReaderSites, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyIO, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.RubyMarshal.Load), 
                new Func<IronRuby.Builtins.RubyMarshal.ReaderSites, IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.RubyMarshal.Load)
            );
            
        }
        
        private static void LoadMatchData_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00010000U, 0x00030000U, 0x00000004U, 
                new Func<IronRuby.Builtins.MatchData, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MatchDataOps.GetGroup), 
                new Func<IronRuby.Builtins.MatchData, System.Int32, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MatchDataOps.GetGroup), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.MatchData, IronRuby.Builtins.Range, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MatchDataOps.GetGroup)
            );
            
            DefineLibraryMethod(module, "begin", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.MatchData, System.Int32, System.Object>(IronRuby.Builtins.MatchDataOps.Begin)
            );
            
            DefineLibraryMethod(module, "captures", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MatchData, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MatchDataOps.Captures)
            );
            
            DefineLibraryMethod(module, "end", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.MatchData, System.Int32, System.Object>(IronRuby.Builtins.MatchDataOps.End)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000002U, 
                new Func<IronRuby.Builtins.MatchData, IronRuby.Builtins.MatchData, IronRuby.Builtins.MatchData>(IronRuby.Builtins.MatchDataOps.InitializeCopy)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MatchData, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MatchDataOps.Inspect)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MatchData, System.Int32>(IronRuby.Builtins.MatchDataOps.Length)
            );
            
            DefineLibraryMethod(module, "offset", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.MatchData, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MatchDataOps.Offset)
            );
            
            DefineLibraryMethod(module, "post_match", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MatchData, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MatchDataOps.PostMatch)
            );
            
            DefineLibraryMethod(module, "pre_match", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MatchData, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MatchDataOps.PreMatch)
            );
            
            DefineLibraryMethod(module, "select", 0x51, 
                0x00000001U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MatchData, System.Object>(IronRuby.Builtins.MatchDataOps.Select)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MatchData, System.Int32>(IronRuby.Builtins.MatchDataOps.Length)
            );
            
            DefineLibraryMethod(module, "string", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MatchData, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MatchDataOps.ReturnFrozenString)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MatchData, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MatchDataOps.ToArray)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MatchData, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MatchDataOps.ToS)
            );
            
            DefineLibraryMethod(module, "values_at", 0x51, 
                0x80020000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.MatchData, System.Int32[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MatchDataOps.ValuesAt)
            );
            
        }
        
        private static void LoadMatchData_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.UndefineMethodNoEvent("new");
        }
        
        private static void LoadMath_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "E", IronRuby.Builtins.RubyMath.E);
            SetBuiltinConstant(module, "PI", IronRuby.Builtins.RubyMath.PI);
            
        }
        
        private static void LoadMath_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "acos", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Acos)
            );
            
            DefineLibraryMethod(module, "acosh", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Acosh)
            );
            
            DefineLibraryMethod(module, "asin", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Asin)
            );
            
            DefineLibraryMethod(module, "asinh", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Asinh)
            );
            
            DefineLibraryMethod(module, "atan", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Atan)
            );
            
            DefineLibraryMethod(module, "atan2", 0x52, 
                0x00030000U, 
                new Func<System.Object, System.Double, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Atan2)
            );
            
            DefineLibraryMethod(module, "atanh", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Atanh)
            );
            
            DefineLibraryMethod(module, "cbrt", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.CubeRoot)
            );
            
            DefineLibraryMethod(module, "cos", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Cos)
            );
            
            DefineLibraryMethod(module, "cosh", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Cosh)
            );
            
            DefineLibraryMethod(module, "erf", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Erf)
            );
            
            DefineLibraryMethod(module, "erfc", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Erfc)
            );
            
            DefineLibraryMethod(module, "exp", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Exp)
            );
            
            DefineLibraryMethod(module, "frexp", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyMath.Frexp)
            );
            
            DefineLibraryMethod(module, "gamma", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Gamma)
            );
            
            DefineLibraryMethod(module, "hypot", 0x52, 
                0x00030000U, 
                new Func<System.Object, System.Double, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Hypot)
            );
            
            DefineLibraryMethod(module, "ldexp", 0x52, 
                0x00030000U, 
                new Func<System.Object, System.Double, IronRuby.Runtime.IntegerValue, System.Double>(IronRuby.Builtins.RubyMath.Ldexp)
            );
            
            DefineLibraryMethod(module, "lgamma", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.LogGamma)
            );
            
            DefineLibraryMethod(module, "log", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Log)
            );
            
            DefineLibraryMethod(module, "log10", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Log10)
            );
            
            DefineLibraryMethod(module, "log2", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Log2)
            );
            
            DefineLibraryMethod(module, "sin", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Sin)
            );
            
            DefineLibraryMethod(module, "sinh", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Sinh)
            );
            
            DefineLibraryMethod(module, "sqrt", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Sqrt)
            );
            
            DefineLibraryMethod(module, "tan", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Tan)
            );
            
            DefineLibraryMethod(module, "tanh", 0x52, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Tanh)
            );
            
        }
        
        private static void LoadMath_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "acos", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Acos)
            );
            
            DefineLibraryMethod(module, "acosh", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Acosh)
            );
            
            DefineLibraryMethod(module, "asin", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Asin)
            );
            
            DefineLibraryMethod(module, "asinh", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Asinh)
            );
            
            DefineLibraryMethod(module, "atan", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Atan)
            );
            
            DefineLibraryMethod(module, "atan2", 0x61, 
                0x00030000U, 
                new Func<System.Object, System.Double, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Atan2)
            );
            
            DefineLibraryMethod(module, "atanh", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Atanh)
            );
            
            DefineLibraryMethod(module, "cbrt", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.CubeRoot)
            );
            
            DefineLibraryMethod(module, "cos", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Cos)
            );
            
            DefineLibraryMethod(module, "cosh", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Cosh)
            );
            
            DefineLibraryMethod(module, "erf", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Erf)
            );
            
            DefineLibraryMethod(module, "erfc", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Erfc)
            );
            
            DefineLibraryMethod(module, "exp", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Exp)
            );
            
            DefineLibraryMethod(module, "frexp", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyMath.Frexp)
            );
            
            DefineLibraryMethod(module, "gamma", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Gamma)
            );
            
            DefineLibraryMethod(module, "hypot", 0x61, 
                0x00030000U, 
                new Func<System.Object, System.Double, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Hypot)
            );
            
            DefineLibraryMethod(module, "ldexp", 0x61, 
                0x00030000U, 
                new Func<System.Object, System.Double, IronRuby.Runtime.IntegerValue, System.Double>(IronRuby.Builtins.RubyMath.Ldexp)
            );
            
            DefineLibraryMethod(module, "lgamma", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.LogGamma)
            );
            
            DefineLibraryMethod(module, "log", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Log)
            );
            
            DefineLibraryMethod(module, "log10", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Log10)
            );
            
            DefineLibraryMethod(module, "log2", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Log2)
            );
            
            DefineLibraryMethod(module, "sin", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Sin)
            );
            
            DefineLibraryMethod(module, "sinh", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Sinh)
            );
            
            DefineLibraryMethod(module, "sqrt", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Sqrt)
            );
            
            DefineLibraryMethod(module, "tan", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Tan)
            );
            
            DefineLibraryMethod(module, "tanh", 0x61, 
                0x00010000U, 
                new Func<System.Object, System.Double, System.Double>(IronRuby.Builtins.RubyMath.Tanh)
            );
            
        }
        
        private static void LoadMethod_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineRuleGenerator(module, "[]", 0x51, IronRuby.Builtins.MethodOps.Call());
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyMethod, System.Boolean>(IronRuby.Builtins.MethodOps.Equal), 
                new Func<IronRuby.Builtins.RubyMethod, System.Object, System.Boolean>(IronRuby.Builtins.MethodOps.Equal)
            );
            
            DefineLibraryMethod(module, "arity", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyMethod, System.Int32>(IronRuby.Builtins.MethodOps.GetArity)
            );
            
            DefineRuleGenerator(module, "call", 0x51, IronRuby.Builtins.MethodOps.Call());
            
            DefineLibraryMethod(module, "clone", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.MethodOps.Clone)
            );
            
            DefineLibraryMethod(module, "clr_members", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MethodOps.GetClrMembers)
            );
            
            DefineLibraryMethod(module, "of", 0x51, 
                0x80000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyMethod, System.Object[], IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.MethodOps.BindGenericParameters)
            );
            
            DefineLibraryMethod(module, "overload", 0x51, 
                0x80000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyMethod, System.Object[], IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.MethodOps.SelectOverload)
            );
            
            DefineLibraryMethod(module, "overloads", 0x51, 
                0x80000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyMethod, System.Object[], IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.MethodOps.SelectOverload_old)
            );
            
            DefineLibraryMethod(module, "parameters", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MethodOps.GetParameters)
            );
            
            DefineLibraryMethod(module, "source_location", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MethodOps.GetSourceLocation)
            );
            
            DefineLibraryMethod(module, "to_proc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyMethod, IronRuby.Builtins.Proc>(IronRuby.Builtins.MethodOps.ToProc)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyMethod, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MethodOps.ToS)
            );
            
            DefineLibraryMethod(module, "unbind", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyMethod, IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.MethodOps.Unbind)
            );
            
        }
        
        private static void LoadMicrosoft__Scripting__Actions__TypeGroup_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                0x80000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, System.Object[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.TypeGroupOps.Of), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, System.Int32, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.TypeGroupOps.Of)
            );
            
            DefineLibraryMethod(module, "clr_constructor", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.TypeGroupOps.GetClrConstructor)
            );
            
            DefineLibraryMethod(module, "clr_ctor", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.TypeGroupOps.GetClrConstructor)
            );
            
            DefineLibraryMethod(module, "clr_new", 0x51, 
                0x80000000U, 0x80000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, Microsoft.Scripting.Actions.TypeGroup, System.Object[], System.Object>(IronRuby.Builtins.TypeGroupOps.ClrNew), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.BlockParam, Microsoft.Scripting.Actions.TypeGroup, System.Object[], System.Object>(IronRuby.Builtins.TypeGroupOps.ClrNew)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, Microsoft.Scripting.Actions.TypeGroup, System.Object>(IronRuby.Builtins.TypeGroupOps.EachType)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, IronRuby.Builtins.MutableString>(IronRuby.Builtins.TypeGroupOps.Inspect)
            );
            
            DefineLibraryMethod(module, "name", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, IronRuby.Builtins.MutableString>(IronRuby.Builtins.TypeGroupOps.GetName)
            );
            
            DefineLibraryMethod(module, "new", 0x51, 
                0x80000000U, 0x80000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, Microsoft.Scripting.Actions.TypeGroup, System.Object[], System.Object>(IronRuby.Builtins.TypeGroupOps.New), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object, System.Object>>, IronRuby.Runtime.BlockParam, Microsoft.Scripting.Actions.TypeGroup, System.Object[], System.Object>(IronRuby.Builtins.TypeGroupOps.New)
            );
            
            DefineLibraryMethod(module, "of", 0x51, 
                0x80000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, System.Object[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.TypeGroupOps.Of), 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, System.Int32, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.TypeGroupOps.Of)
            );
            
            DefineLibraryMethod(module, "superclass", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.TypeGroupOps.GetSuperclass)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeGroup, IronRuby.Builtins.MutableString>(IronRuby.Builtins.TypeGroupOps.GetName)
            );
            
        }
        
        private static void LoadMicrosoft__Scripting__Actions__TypeTracker_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "to_class", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeTracker, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.TypeTrackerOps.ToClass)
            );
            
            DefineLibraryMethod(module, "to_module", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, Microsoft.Scripting.Actions.TypeTracker, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.TypeTrackerOps.ToModule)
            );
            
        }
        
        private static void LoadModule_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                0x80000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.Of), 
                new Func<IronRuby.Builtins.RubyModule, System.Int32, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.Of)
            );
            
            DefineLibraryMethod(module, "<", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.IsSubclassOrIncluded), 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.InvalidComparison)
            );
            
            DefineLibraryMethod(module, "<=", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.IsSubclassSameOrIncluded), 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.InvalidComparison)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.Comparison), 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.Comparison)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.ModuleOps.Equals)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Boolean>(IronRuby.Builtins.ModuleOps.CaseEquals)
            );
            
            DefineLibraryMethod(module, ">", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.IsNotSubclassOrIncluded), 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.InvalidComparison)
            );
            
            DefineLibraryMethod(module, ">=", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.IsNotSubclassSameOrIncluded), 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.InvalidComparison)
            );
            
            DefineLibraryMethod(module, "alias_method", 0x52, 
                0x0006000cU, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, System.String, System.String, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.AliasMethod)
            );
            
            DefineLibraryMethod(module, "ancestors", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.Ancestors)
            );
            
            DefineLibraryMethod(module, "append_features", 0x52, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.AppendFeatures)
            );
            
            DefineLibraryMethod(module, "attr", 0x52, 
                0x00020004U, 0x80020004U, 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String, System.Boolean>(IronRuby.Builtins.ModuleOps.Attr), 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[]>(IronRuby.Builtins.ModuleOps.Attr)
            );
            
            DefineLibraryMethod(module, "attr_accessor", 0x52, 
                0x00020004U, 0x80020004U, 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String>(IronRuby.Builtins.ModuleOps.AttrAccessor), 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[]>(IronRuby.Builtins.ModuleOps.AttrAccessor)
            );
            
            DefineLibraryMethod(module, "attr_reader", 0x52, 
                0x00020004U, 0x80020004U, 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String>(IronRuby.Builtins.ModuleOps.AttrReader), 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[]>(IronRuby.Builtins.ModuleOps.AttrReader)
            );
            
            DefineLibraryMethod(module, "attr_writer", 0x52, 
                0x00020004U, 0x80020004U, 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String>(IronRuby.Builtins.ModuleOps.AttrWriter), 
                new Action<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[]>(IronRuby.Builtins.ModuleOps.AttrWriter)
            );
            
            DefineLibraryMethod(module, "autoload", 0x51, 
                0x00030006U, 
                new Action<IronRuby.Builtins.RubyModule, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ModuleOps.SetAutoloadedConstant)
            );
            
            DefineLibraryMethod(module, "autoload?", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ModuleOps.GetAutoloadedConstantPath)
            );
            
            DefineLibraryMethod(module, "class_eval", 0x51, 
                0x00040018U, 0x00000001U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.ModuleOps.Evaluate), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.Evaluate)
            );
            
            DefineLibraryMethod(module, "class_exec", 0x51, 
                0x80000001U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object[], System.Object>(IronRuby.Builtins.ModuleOps.Execute)
            );
            
            DefineLibraryMethod(module, "class_variable_defined?", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Boolean>(IronRuby.Builtins.ModuleOps.IsClassVariableDefined)
            );
            
            DefineLibraryMethod(module, "class_variable_get", 0x52, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Object>(IronRuby.Builtins.ModuleOps.GetClassVariable)
            );
            
            DefineLibraryMethod(module, "class_variable_set", 0x52, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.ClassVariableSet)
            );
            
            DefineLibraryMethod(module, "class_variables", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.ClassVariables)
            );
            
            DefineLibraryMethod(module, "const_defined?", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Boolean>(IronRuby.Builtins.ModuleOps.IsConstantDefined)
            );
            
            DefineLibraryMethod(module, "const_get", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String, System.Object>(IronRuby.Builtins.ModuleOps.GetConstantValue)
            );
            
            DefineLibraryMethod(module, "const_missing", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Object>(IronRuby.Builtins.ModuleOps.ConstantMissing)
            );
            
            DefineLibraryMethod(module, "const_set", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.SetConstantValue)
            );
            
            DefineLibraryMethod(module, "constants", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetDefinedConstants)
            );
            
            DefineLibraryMethod(module, "define_method", 0x52, 
                new[] { 0x0002000cU, 0x0000000cU, 0x0002000cU, 0x0000000cU, 0x0004000aU, 0x0000000aU, 0x0002000cU, 0x0000000cU}, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String, IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.ModuleOps.DefineMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Runtime.ClrName, IronRuby.Builtins.RubyMethod, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.ModuleOps.DefineMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String, IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.ModuleOps.DefineMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Runtime.ClrName, IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.ModuleOps.DefineMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.String, IronRuby.Builtins.Proc>(IronRuby.Builtins.ModuleOps.DefineMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Runtime.ClrName, IronRuby.Builtins.Proc>(IronRuby.Builtins.ModuleOps.DefineMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String, IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.ModuleOps.DefineMethod), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Runtime.ClrName, IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.ModuleOps.DefineMethod)
            );
            
            DefineLibraryMethod(module, "extend_object", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.ModuleOps.ExtendObject)
            );
            
            DefineLibraryMethod(module, "extended", 0x52, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.ObjectExtended)
            );
            
            DefineLibraryMethod(module, "freeze", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.Freeze)
            );
            
            DefineLibraryMethod(module, "include", 0x52, 
                0x80000008U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>>, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.Include)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Boolean>(IronRuby.Builtins.ModuleOps.IncludesModule)
            );
            
            DefineLibraryMethod(module, "included", 0x52, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.Included)
            );
            
            DefineLibraryMethod(module, "included_modules", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetIncludedModules)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.InitializeCopy)
            );
            
            DefineLibraryMethod(module, "instance_method", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.ModuleOps.GetInstanceMethod)
            );
            
            DefineLibraryMethod(module, "instance_methods", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetInstanceMethods), 
                new Func<IronRuby.Builtins.RubyModule, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetInstanceMethods)
            );
            
            DefineLibraryMethod(module, "method_added", 0x5a, 
                0x00000000U, 
                new Action<System.Object, System.Object>(IronRuby.Builtins.ModuleOps.MethodAdded)
            );
            
            DefineLibraryMethod(module, "method_defined?", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Boolean>(IronRuby.Builtins.ModuleOps.MethodDefined)
            );
            
            DefineLibraryMethod(module, "method_removed", 0x5a, 
                0x00000000U, 
                new Action<System.Object, System.Object>(IronRuby.Builtins.ModuleOps.MethodRemoved)
            );
            
            DefineLibraryMethod(module, "method_undefined", 0x5a, 
                0x00000000U, 
                new Action<System.Object, System.Object>(IronRuby.Builtins.ModuleOps.MethodUndefined)
            );
            
            DefineLibraryMethod(module, "module_eval", 0x51, 
                0x00040018U, 0x00000001U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.ModuleOps.Evaluate), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.ModuleOps.Evaluate)
            );
            
            DefineLibraryMethod(module, "module_exec", 0x51, 
                0x80000001U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, System.Object[], System.Object>(IronRuby.Builtins.ModuleOps.Execute)
            );
            
            DefineLibraryMethod(module, "module_function", 0x52, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.CopyMethodsToModuleSingleton)
            );
            
            DefineLibraryMethod(module, "name", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ModuleOps.GetName)
            );
            
            DefineLibraryMethod(module, "of", 0x51, 
                0x80000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.Of), 
                new Func<IronRuby.Builtins.RubyModule, System.Int32, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.Of)
            );
            
            DefineLibraryMethod(module, "private", 0x52, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.SetPrivateVisibility)
            );
            
            DefineLibraryMethod(module, "private_class_method", 0x51, 
                0x80010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.MakeClassMethodsPrivate)
            );
            
            DefineLibraryMethod(module, "private_instance_methods", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetPrivateInstanceMethods), 
                new Func<IronRuby.Builtins.RubyModule, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetPrivateInstanceMethods)
            );
            
            DefineLibraryMethod(module, "private_method_defined?", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Boolean>(IronRuby.Builtins.ModuleOps.PrivateMethodDefined)
            );
            
            DefineLibraryMethod(module, "protected", 0x52, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.SetProtectedVisibility)
            );
            
            DefineLibraryMethod(module, "protected_instance_methods", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetProtectedInstanceMethods), 
                new Func<IronRuby.Builtins.RubyModule, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetProtectedInstanceMethods)
            );
            
            DefineLibraryMethod(module, "protected_method_defined?", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Boolean>(IronRuby.Builtins.ModuleOps.ProtectedMethodDefined)
            );
            
            DefineLibraryMethod(module, "public", 0x52, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.SetPublicVisibility)
            );
            
            DefineLibraryMethod(module, "public_class_method", 0x51, 
                0x80010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.MakeClassMethodsPublic)
            );
            
            DefineLibraryMethod(module, "public_instance_methods", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetPublicInstanceMethods), 
                new Func<IronRuby.Builtins.RubyModule, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetPublicInstanceMethods)
            );
            
            DefineLibraryMethod(module, "public_method_defined?", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Boolean>(IronRuby.Builtins.ModuleOps.PublicMethodDefined)
            );
            
            DefineLibraryMethod(module, "remove_class_variable", 0x52, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Object>(IronRuby.Builtins.ModuleOps.RemoveClassVariable)
            );
            
            DefineLibraryMethod(module, "remove_const", 0x52, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Object>(IronRuby.Builtins.ModuleOps.RemoveConstant)
            );
            
            DefineLibraryMethod(module, "remove_method", 0x52, 
                0x80010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.RemoveMethod)
            );
            
            DefineLibraryMethod(module, "to_clr_ref", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.ToClrRef)
            );
            
            DefineLibraryMethod(module, "to_clr_type", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Type>(IronRuby.Builtins.ModuleOps.ToClrType)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ModuleOps.ToS)
            );
            
            DefineLibraryMethod(module, "undef_method", 0x52, 
                0x80010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String[], IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ModuleOps.UndefineMethod)
            );
            
        }
        
        private static void LoadModule_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "constants", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetGlobalConstants)
            );
            
            DefineLibraryMethod(module, "nesting", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ModuleOps.GetLexicalModuleNesting)
            );
            
        }
        
        private static void LoadNilClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "&", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object, System.Boolean>(IronRuby.Builtins.NilClassOps.And)
            );
            
            DefineLibraryMethod(module, "^", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Object, System.Object, System.Boolean>(IronRuby.Builtins.NilClassOps.Xor), 
                new Func<System.Object, System.Boolean, System.Boolean>(IronRuby.Builtins.NilClassOps.Xor)
            );
            
            DefineLibraryMethod(module, "|", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Object, System.Object, System.Boolean>(IronRuby.Builtins.NilClassOps.Or), 
                new Func<System.Object, System.Boolean, System.Boolean>(IronRuby.Builtins.NilClassOps.Or)
            );
            
            DefineLibraryMethod(module, "GetHashCode", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Int32>(IronRuby.Builtins.NilClassOps.GetClrHashCode)
            );
            
            DefineLibraryMethod(module, "GetType", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Type>(IronRuby.Builtins.NilClassOps.GetClrType)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.NilClassOps.Inspect)
            );
            
            DefineLibraryMethod(module, "nil?", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Boolean>(IronRuby.Builtins.NilClassOps.IsNil)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.NilClassOps.ToArray)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Double>(IronRuby.Builtins.NilClassOps.ToDouble)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Int32>(IronRuby.Builtins.NilClassOps.ToInteger)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.NilClassOps.ToString)
            );
            
            DefineLibraryMethod(module, "ToString", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.String>(IronRuby.Builtins.NilClassOps.ToClrString)
            );
            
        }
        
        private static void LoadNoMethodError_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.HideMethod("message");
            DefineLibraryMethod(module, "args", 0x51, 
                0x00000000U, 
                new Func<System.MissingMethodException, System.Object>(IronRuby.Builtins.NoMethodErrorOps.GetArguments)
            );
            
        }
        
        private static void LoadNumeric_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "-@", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.Numeric.UnaryMinus)
            );
            
            DefineLibraryMethod(module, "+@", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.Numeric.UnaryPlus)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Object, System.Object>(IronRuby.Builtins.Numeric.Compare)
            );
            
            DefineLibraryMethod(module, "abs", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.UnaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.Numeric.Abs)
            );
            
            DefineLibraryMethod(module, "ceil", 0x51, 
                0x00008000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.Numeric.Ceil)
            );
            
            DefineLibraryMethod(module, "coerce", 0x51, 
                0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<System.Int32, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Numeric.Coerce), 
                new Func<System.Double, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Numeric.Coerce), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Double>, IronRuby.Runtime.ConversionStorage<System.Double>, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Numeric.Coerce)
            );
            
            DefineLibraryMethod(module, "div", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<System.Double>, System.Object, System.Object, System.Object>(IronRuby.Builtins.Numeric.Div)
            );
            
            DefineLibraryMethod(module, "divmod", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<System.Double>, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.Numeric.DivMod)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.Numeric.Eql)
            );
            
            DefineLibraryMethod(module, "floor", 0x51, 
                0x00008000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.Numeric.Floor)
            );
            
            DefineLibraryMethod(module, "integer?", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Boolean>(IronRuby.Builtins.Numeric.IsInteger)
            );
            
            DefineLibraryMethod(module, "modulo", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.Numeric.Modulo)
            );
            
            DefineLibraryMethod(module, "nonzero?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.Numeric.IsNonZero)
            );
            
            DefineLibraryMethod(module, "quo", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.Numeric.Quo)
            );
            
            DefineLibraryMethod(module, "remainder", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.Numeric.Remainder)
            );
            
            DefineLibraryMethod(module, "round", 0x51, 
                0x00008000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.Numeric.Round)
            );
            
            DefineLibraryMethod(module, "step", 0x51, 
                0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.Numeric.Step), 
                new Func<IronRuby.Runtime.BlockParam, System.Int32, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.Numeric.Step), 
                new Func<IronRuby.Runtime.BlockParam, System.Double, System.Double, System.Double, System.Object>(IronRuby.Builtins.Numeric.Step), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<System.Double>, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.Numeric.Step)
            );
            
            DefineLibraryMethod(module, "to_int", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Object, System.Object>(IronRuby.Builtins.Numeric.ToInt)
            );
            
            DefineLibraryMethod(module, "truncate", 0x51, 
                0x00008000U, 
                new Func<System.Double, System.Object>(IronRuby.Builtins.Numeric.Truncate)
            );
            
            DefineLibraryMethod(module, "zero?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Boolean>(IronRuby.Builtins.Numeric.IsZero)
            );
            
        }
        
        private static void LoadObject_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "___Numerics__", IronRuby.Builtins.ObjectOps.Numerics(module));
            SetBuiltinConstant(module, "FALSE", IronRuby.Builtins.ObjectOps.FALSE);
            SetBuiltinConstant(module, "NIL", IronRuby.Builtins.ObjectOps.NIL);
            SetBuiltinConstant(module, "TRUE", IronRuby.Builtins.ObjectOps.TRUE);
            
        }
        
        private static void LoadObject_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "initialize", 0x5a, 
                0x80000000U, 
                new Func<System.Object, System.Object[], System.Object>(IronRuby.Builtins.ObjectOps.Reinitialize)
            );
            
        }
        
        private static void LoadObject_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
        }
        
        private static void LoadObjectSpace_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "define_finalizer", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Object>(IronRuby.Builtins.ObjectSpace.DefineFinalizer)
            );
            
            DefineLibraryMethod(module, "each_object", 0x61, 
                0x00000004U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyClass, System.Object>(IronRuby.Builtins.ObjectSpace.EachObject)
            );
            
            DefineLibraryMethod(module, "garbage_collect", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyModule>(IronRuby.Builtins.ObjectSpace.GarbageCollect)
            );
            
            DefineLibraryMethod(module, "undefine_finalizer", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, System.Object, System.Object>(IronRuby.Builtins.ObjectSpace.UndefineFinalizer)
            );
            
        }
        
        private static void LoadPrecision_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "prec", 0x51, 
                0x00000004U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object, System.Object>>, System.Object, IronRuby.Builtins.RubyClass, System.Object>(IronRuby.Builtins.Precision.Prec)
            );
            
            DefineLibraryMethod(module, "prec_f", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.RubyClass, System.Object>>, System.Object, System.Object>(IronRuby.Builtins.Precision.PrecFloat)
            );
            
            DefineLibraryMethod(module, "prec_i", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.RubyClass, System.Object>>, System.Object, System.Object>(IronRuby.Builtins.Precision.PrecInteger)
            );
            
        }
        
        private static void LoadPrecision_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "included", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.Precision.Included)
            );
            
        }
        
        private static void LoadProc_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                new[] { 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x80000000U}, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object[], System.Object>(IronRuby.Builtins.ProcOps.Call)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.Proc, System.Boolean>(IronRuby.Builtins.ProcOps.Equal), 
                new Func<IronRuby.Builtins.Proc, System.Object, System.Boolean>(IronRuby.Builtins.ProcOps.Equal)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                new[] { 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x80000000U}, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object[], System.Object>(IronRuby.Builtins.ProcOps.Call)
            );
            
            DefineLibraryMethod(module, "arity", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, System.Int32>(IronRuby.Builtins.ProcOps.GetArity)
            );
            
            DefineLibraryMethod(module, "binding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.Binding>(IronRuby.Builtins.ProcOps.GetLocalScope)
            );
            
            DefineLibraryMethod(module, "call", 0x51, 
                new[] { 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x80000000U}, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object[], System.Object>(IronRuby.Builtins.ProcOps.Call)
            );
            
            DefineLibraryMethod(module, "clone", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.ProcOps.Clone)
            );
            
            DefineLibraryMethod(module, "dup", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.ProcOps.Clone)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.Proc, System.Boolean>(IronRuby.Builtins.ProcOps.Equal), 
                new Func<IronRuby.Builtins.Proc, System.Object, System.Boolean>(IronRuby.Builtins.ProcOps.Equal)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, System.Int32>(IronRuby.Builtins.ProcOps.GetHash)
            );
            
            DefineLibraryMethod(module, "lambda?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, System.Boolean>(IronRuby.Builtins.ProcOps.IsLambda)
            );
            
            DefineLibraryMethod(module, "source_location", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ProcOps.GetSourceLocation)
            );
            
            DefineLibraryMethod(module, "to_proc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(IronRuby.Builtins.ProcOps.ToProc)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.Proc, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ProcOps.ToS)
            );
            
            DefineLibraryMethod(module, "yield", 0x51, 
                new[] { 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 0x80000000U}, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object, System.Object, System.Object, System.Object, System.Object>(IronRuby.Builtins.ProcOps.Call), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Proc, System.Object[], System.Object>(IronRuby.Builtins.ProcOps.Call)
            );
            
        }
        
        private static void LoadProc_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "new", 0x61, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyClass, IronRuby.Builtins.Proc>(IronRuby.Builtins.ProcOps.CreateNew), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object>(IronRuby.Builtins.ProcOps.CreateNew)
            );
            
        }
        
        #if !SILVERLIGHT
        private static void LoadProcess_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "kill", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Object>(IronRuby.Builtins.RubyProcess.Kill)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadProcess_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "euid", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Int32>(IronRuby.Builtins.RubyProcess.EffectiveUserId)
            );
            
            DefineLibraryMethod(module, "kill", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Object, System.Object>(IronRuby.Builtins.RubyProcess.Kill)
            );
            
            DefineLibraryMethod(module, "pid", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Int32>(IronRuby.Builtins.RubyProcess.GetPid)
            );
            
            DefineLibraryMethod(module, "ppid", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Int32>(IronRuby.Builtins.RubyProcess.GetParentPid)
            );
            
            DefineLibraryMethod(module, "times", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyStruct>(IronRuby.Builtins.RubyProcess.GetTimes)
            );
            
            DefineLibraryMethod(module, "uid", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Int32>(IronRuby.Builtins.RubyProcess.UserId)
            );
            
            DefineLibraryMethod(module, "uid=", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyModule, System.Object>(IronRuby.Builtins.RubyProcess.SetUserId)
            );
            
            DefineLibraryMethod(module, "wait", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyModule>(IronRuby.Builtins.RubyProcess.Wait)
            );
            
            DefineLibraryMethod(module, "wait2", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyModule>(IronRuby.Builtins.RubyProcess.Wait2)
            );
            
            DefineLibraryMethod(module, "waitall", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyProcess.Waitall)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT && !SILVERLIGHT
        private static void LoadProcess__Status_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "coredump?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Boolean>(IronRuby.Builtins.RubyProcess.Status.CoreDump)
            );
            
            DefineLibraryMethod(module, "exited?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Boolean>(IronRuby.Builtins.RubyProcess.Status.Exited)
            );
            
            DefineLibraryMethod(module, "exitstatus", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Int32>(IronRuby.Builtins.RubyProcess.Status.ExitStatus)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyProcess.Status.Inspect)
            );
            
            DefineLibraryMethod(module, "pid", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Int32>(IronRuby.Builtins.RubyProcess.Status.Pid)
            );
            
            DefineLibraryMethod(module, "stopped?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Boolean>(IronRuby.Builtins.RubyProcess.Status.Stopped)
            );
            
            DefineLibraryMethod(module, "stopsig", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Object>(IronRuby.Builtins.RubyProcess.Status.StopSig)
            );
            
            DefineLibraryMethod(module, "success?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Boolean>(IronRuby.Builtins.RubyProcess.Status.Success)
            );
            
            DefineLibraryMethod(module, "termsig", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyProcess.Status, System.Object>(IronRuby.Builtins.RubyProcess.Status.TermSig)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT && !SILVERLIGHT
        private static void LoadProcess__Status_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.HideMethod("new");
        }
        #endif
        
        private static void LoadRange_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Builtins.Range, System.Object, System.Boolean>(IronRuby.Builtins.RangeOps.Equals), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.Range, IronRuby.Builtins.Range, System.Boolean>(IronRuby.Builtins.RangeOps.Equals)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                0x00000002U, 
                new Func<IronRuby.Runtime.ComparisonStorage, IronRuby.Builtins.Range, System.Object, System.Boolean>(IronRuby.Builtins.RangeOps.CaseEquals)
            );
            
            DefineLibraryMethod(module, "begin", 0x51, 
                0x00000001U, 
                new Func<IronRuby.Builtins.Range, System.Object>(IronRuby.Builtins.RangeOps.Begin)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Builtins.RangeOps.EachStorage, IronRuby.Builtins.Range, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.RangeOps.GetEachEnumerator), 
                new Func<IronRuby.Builtins.RangeOps.EachStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.Range, System.Object>(IronRuby.Builtins.RangeOps.Each)
            );
            
            DefineLibraryMethod(module, "end", 0x51, 
                0x00000001U, 
                new Func<IronRuby.Builtins.Range, System.Object>(IronRuby.Builtins.RangeOps.End)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Builtins.Range, System.Object, System.Boolean>(IronRuby.Builtins.RangeOps.Equals), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.Range, IronRuby.Builtins.Range, System.Boolean>(IronRuby.Builtins.RangeOps.Eql)
            );
            
            DefineLibraryMethod(module, "exclude_end?", 0x51, 
                0x00000001U, 
                new Func<IronRuby.Builtins.Range, System.Boolean>(IronRuby.Builtins.RangeOps.ExcludeEnd)
            );
            
            DefineLibraryMethod(module, "first", 0x51, 
                0x00000001U, 
                new Func<IronRuby.Builtins.Range, System.Object>(IronRuby.Builtins.RangeOps.Begin)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.Range, System.Int32>(IronRuby.Builtins.RangeOps.GetHashCode)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00000002U, 
                new Func<IronRuby.Runtime.ComparisonStorage, IronRuby.Builtins.Range, System.Object, System.Boolean>(IronRuby.Builtins.RangeOps.CaseEquals)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.Builtins.Range, System.Object, System.Object, System.Boolean, IronRuby.Builtins.Range>(IronRuby.Builtins.RangeOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RangeOps.Inspect)
            );
            
            DefineLibraryMethod(module, "last", 0x51, 
                0x00000001U, 
                new Func<IronRuby.Builtins.Range, System.Object>(IronRuby.Builtins.RangeOps.End)
            );
            
            DefineLibraryMethod(module, "member?", 0x51, 
                0x00000002U, 
                new Func<IronRuby.Runtime.ComparisonStorage, IronRuby.Builtins.Range, System.Object, System.Boolean>(IronRuby.Builtins.RangeOps.CaseEquals)
            );
            
            DefineLibraryMethod(module, "step", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Builtins.RangeOps.StepStorage, IronRuby.Builtins.Range, System.Object, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.RangeOps.GetStepEnumerator), 
                new Func<IronRuby.Builtins.RangeOps.StepStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.Range, System.Object, System.Object>(IronRuby.Builtins.RangeOps.Step)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RangeOps.ToS)
            );
            
        }
        
        private static void LoadRangeError_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.HideMethod("message");
        }
        
        private static void LoadRegexp_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "EXTENDED", IronRuby.Builtins.RegexpOps.EXTENDED);
            SetBuiltinConstant(module, "IGNORECASE", IronRuby.Builtins.RegexpOps.IGNORECASE);
            SetBuiltinConstant(module, "MULTILINE", IronRuby.Builtins.RegexpOps.MULTILINE);
            
        }
        
        private static void LoadRegexp_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "~", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.RegexpOps.ImplicitMatch)
            );
            
            DefineLibraryMethod(module, "=~", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.RegexpOps.MatchIndex)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Builtins.RubyRegex, System.Object, System.Boolean>(IronRuby.Builtins.RegexpOps.Equals), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyRegex, System.Boolean>(IronRuby.Builtins.RegexpOps.Equals)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyRegex, System.Object, System.Boolean>(IronRuby.Builtins.RegexpOps.CaseCompare)
            );
            
            DefineLibraryMethod(module, "casefold?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyRegex, System.Boolean>(IronRuby.Builtins.RegexpOps.IsCaseInsensitive)
            );
            
            DefineLibraryMethod(module, "encoding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.RegexpOps.GetEncoding)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Builtins.RubyRegex, System.Object, System.Boolean>(IronRuby.Builtins.RegexpOps.Equals), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyRegex, System.Boolean>(IronRuby.Builtins.RegexpOps.Equals)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyRegex, System.Int32>(IronRuby.Builtins.RegexpOps.GetHash)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                new[] { 0x00000002U, 0x00000004U, 0x00000004U, 0x00050002U, 0x00050002U}, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Reinitialize), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyRegex, System.Int32, System.Object, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Reinitialize), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyRegex, System.Object, System.Object, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Reinitialize), 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Reinitialize), 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, System.Boolean, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RegexpOps.Inspect)
            );
            
            DefineLibraryMethod(module, "match", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, IronRuby.Builtins.MatchData>(IronRuby.Builtins.RegexpOps.Match)
            );
            
            DefineLibraryMethod(module, "options", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyRegex, System.Int32>(IronRuby.Builtins.RegexpOps.GetOptions)
            );
            
            DefineLibraryMethod(module, "source", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RegexpOps.Source)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RegexpOps.ToS)
            );
            
        }
        
        private static void LoadRegexp_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineRuleGenerator(module, "compile", 0x61, IronRuby.Builtins.RegexpOps.Compile());
            
            DefineLibraryMethod(module, "escape", 0x61, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RegexpOps.Escape)
            );
            
            DefineLibraryMethod(module, "last_match", 0x61, 
                0x00000000U, 0x00020000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MatchData>(IronRuby.Builtins.RegexpOps.LastMatch), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyClass, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RegexpOps.LastMatch)
            );
            
            DefineLibraryMethod(module, "quote", 0x61, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RegexpOps.Escape)
            );
            
            DefineLibraryMethod(module, "union", 0x61, 
                0x00000008U, 0x00000004U, 0x80000004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Union), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Collections.IList, IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Union), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.RubyRegex>(IronRuby.Builtins.RegexpOps.Union)
            );
            
        }
        
        #if !SILVERLIGHT
        private static void LoadSignal_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "list", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyModule, IronRuby.Builtins.Hash>(IronRuby.Builtins.Signal.List)
            );
            
            DefineLibraryMethod(module, "trap", 0x61, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Object, IronRuby.Builtins.Proc, System.Object>(IronRuby.Builtins.Signal.Trap), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object, System.Object>(IronRuby.Builtins.Signal.Trap)
            );
            
        }
        #endif
        
        private static void LoadString_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "%", 0x51, 
                0x00000004U, 0x00000000U, 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Builtins.MutableString, System.Collections.IList, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Format), 
                new Func<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Builtins.MutableString, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Format)
            );
            
            DefineLibraryMethod(module, "*", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Repeat)
            );
            
            DefineLibraryMethod(module, "[]", 0x51, 
                new[] { 0x00010000U, 0x00030000U, 0x00000004U, 0x00000002U, 0x00000004U, 0x00040004U}, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetChar), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.MutableString, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring)
            );
            
            DefineLibraryMethod(module, "[]=", 0x51, 
                new[] { 0x00030004U, 0x00010000U, 0x00070008U, 0x0004000cU, 0x00020006U, 0x000c0014U}, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceCharacter), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Int32, System.Int32>(IronRuby.Builtins.MutableStringOps.SetCharacter), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceSubstring), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.MutableString, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceSubstring), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceSubstring), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceSubstring)
            );
            
            DefineLibraryMethod(module, "+", 0x51, 
                0x00010002U, 0x00000002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Concatenate), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Concatenate)
            );
            
            DefineLibraryMethod(module, "<<", 0x51, 
                0x00010002U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Append), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Append)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.MutableStringOps.Compare), 
                new Func<IronRuby.Builtins.MutableString, System.String, System.Int32>(IronRuby.Builtins.MutableStringOps.Compare), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RespondToStorage, System.Object, System.Object, System.Object>(IronRuby.Builtins.MutableStringOps.Compare)
            );
            
            DefineLibraryMethod(module, "=~", 0x51, 
                0x00000004U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.MutableStringOps.Match), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.Match), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, System.Object, System.Object>(IronRuby.Builtins.MutableStringOps.Match)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.StringEquals), 
                new Func<IronRuby.Builtins.MutableString, System.String, System.Boolean>(IronRuby.Builtins.MutableStringOps.StringEquals), 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.MutableStringOps.Equals)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.StringEquals), 
                new Func<IronRuby.Builtins.MutableString, System.String, System.Boolean>(IronRuby.Builtins.MutableStringOps.StringEquals), 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Boolean>(IronRuby.Builtins.MutableStringOps.Equals)
            );
            
            DefineLibraryMethod(module, "ascii_only?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.IsAscii)
            );
            
            DefineLibraryMethod(module, "bytes", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachByte), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachByte)
            );
            
            DefineLibraryMethod(module, "bytesize", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.MutableStringOps.GetByteCount)
            );
            
            DefineLibraryMethod(module, "capitalize", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Capitalize)
            );
            
            DefineLibraryMethod(module, "capitalize!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.CapitalizeInPlace)
            );
            
            DefineLibraryMethod(module, "casecmp", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.MutableStringOps.Casecmp)
            );
            
            DefineLibraryMethod(module, "center", 0x51, 
                0x00030000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Center)
            );
            
            DefineLibraryMethod(module, "chars", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachChar), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachChar)
            );
            
            DefineLibraryMethod(module, "chomp", 0x51, 
                0x00000000U, 0x00010000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Chomp), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Chomp)
            );
            
            DefineLibraryMethod(module, "chomp!", 0x51, 
                0x00000000U, 0x00010000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ChompInPlace), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ChompInPlace)
            );
            
            DefineLibraryMethod(module, "chop", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Chop)
            );
            
            DefineLibraryMethod(module, "chop!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ChopInPlace)
            );
            
            DefineLibraryMethod(module, "chr", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.FirstChar)
            );
            
            DefineLibraryMethod(module, "clear", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Clear)
            );
            
            DefineLibraryMethod(module, "codepoints", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachCodePoint), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachCodePoint)
            );
            
            DefineLibraryMethod(module, "concat", 0x51, 
                0x00010002U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Append), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Append)
            );
            
            DefineLibraryMethod(module, "count", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], System.Object>(IronRuby.Builtins.MutableStringOps.Count)
            );
            
            DefineLibraryMethod(module, "delete", 0x51, 
                0x80010002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Delete)
            );
            
            DefineLibraryMethod(module, "delete!", 0x51, 
                0x80010002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.DeleteInPlace)
            );
            
            DefineLibraryMethod(module, "downcase", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.DownCase)
            );
            
            DefineLibraryMethod(module, "downcase!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.DownCaseInPlace)
            );
            
            DefineLibraryMethod(module, "dump", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Dump)
            );
            
            DefineLibraryMethod(module, "each_byte", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachByte), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachByte)
            );
            
            DefineLibraryMethod(module, "each_char", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachChar), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachChar)
            );
            
            DefineLibraryMethod(module, "each_codepoint", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachCodePoint), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachCodePoint)
            );
            
            DefineLibraryMethod(module, "each_line", 0x51, 
                0x00000002U, 0x00010000U, 0x00020001U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachLine), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachLine), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachLine), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachLine)
            );
            
            DefineLibraryMethod(module, "empty?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.IsEmpty)
            );
            
            DefineLibraryMethod(module, "encode", 0x51, 
                0x00100000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Encode)
            );
            
            DefineLibraryMethod(module, "encode!", 0x51, 
                0x00100000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.Generic.IDictionary<System.Object, System.Object>>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString, System.Object, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.EncodeInPlace)
            );
            
            DefineLibraryMethod(module, "encoding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.MutableStringOps.GetEncoding)
            );
            
            DefineLibraryMethod(module, "end_with?", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.EndsWith)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.Eql), 
                new Func<IronRuby.Builtins.MutableString, System.String, System.Boolean>(IronRuby.Builtins.MutableStringOps.Eql), 
                new Func<IronRuby.Builtins.MutableString, System.Object, System.Boolean>(IronRuby.Builtins.MutableStringOps.Eql)
            );
            
            DefineLibraryMethod(module, "force_encoding", 0x51, 
                0x00000002U, 0x00020004U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ForceEncoding), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ForceEncoding)
            );
            
            DefineLibraryMethod(module, "getbyte", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.GetByte)
            );
            
            DefineLibraryMethod(module, "gsub", 0x51, 
                0x00000014U, 0x00000014U, 0x0002000cU, 0x00180030U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.MutableStringOps.BlockReplaceAll), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.BlockReplaceAll), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceAll), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Runtime.Union<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceAll)
            );
            
            DefineLibraryMethod(module, "gsub!", 0x51, 
                0x00080014U, 0x0006000cU, 0x00180030U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.MutableStringOps.BlockReplaceAllInPlace), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceAllInPlace), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Runtime.Union<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceAllInPlace)
            );
            
            DefineLibraryMethod(module, "hex", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.ToIntegerHex)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00010002U, 0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.Include), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Boolean>(IronRuby.Builtins.MutableStringOps.Include)
            );
            
            DefineLibraryMethod(module, "index", 0x51, 
                0x00030002U, 0x00040004U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.Index), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.Index)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 0x00010002U, 0x00000002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Reinitialize), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Reinitialize), 
                new Func<IronRuby.Builtins.MutableString, System.Byte[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00010002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "insert", 0x51, 
                0x00030004U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Insert)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Inspect)
            );
            
            DefineLibraryMethod(module, "intern", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.MutableStringOps.ToSymbol)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.MutableStringOps.GetCharacterCount)
            );
            
            DefineLibraryMethod(module, "lines", 0x51, 
                0x00000002U, 0x00010000U, 0x00020001U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachLine), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachLine), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.EachLine), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.EachLine)
            );
            
            DefineLibraryMethod(module, "ljust", 0x51, 
                0x00010000U, 0x00030004U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.LeftJustify), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.LeftJustify)
            );
            
            DefineLibraryMethod(module, "lstrip", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.StripLeft)
            );
            
            DefineLibraryMethod(module, "lstrip!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.StripLeftInPlace)
            );
            
            DefineLibraryMethod(module, "match", 0x51, 
                0x00000008U, 0x00040008U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.MutableStringOps.Match), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.Match)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Succ)
            );
            
            DefineLibraryMethod(module, "next!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.SuccInPlace)
            );
            
            DefineLibraryMethod(module, "oct", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.ToIntegerOctal)
            );
            
            DefineLibraryMethod(module, "ord", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.MutableStringOps.Ord)
            );
            
            DefineLibraryMethod(module, "replace", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Replace)
            );
            
            DefineLibraryMethod(module, "reverse", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetReversed)
            );
            
            DefineLibraryMethod(module, "reverse!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Reverse)
            );
            
            DefineLibraryMethod(module, "rindex", 0x51, 
                0x00010002U, 0x00030002U, 0x00040004U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.LastIndexOf), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.LastIndexOf), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.LastIndexOf)
            );
            
            DefineLibraryMethod(module, "rjust", 0x51, 
                0x00010000U, 0x00030004U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.RightJustify), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.RightJustify)
            );
            
            DefineLibraryMethod(module, "rstrip", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.StripRight)
            );
            
            DefineLibraryMethod(module, "rstrip!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.StripRightInPlace)
            );
            
            DefineLibraryMethod(module, "scan", 0x51, 
                0x00020004U, 0x0004000aU, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MutableStringOps.Scan), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.MutableStringOps.Scan)
            );
            
            DefineLibraryMethod(module, "setbyte", 0x51, 
                0x00030000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.SetByte)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.MutableStringOps.GetCharacterCount)
            );
            
            DefineLibraryMethod(module, "slice", 0x51, 
                new[] { 0x00010000U, 0x00030000U, 0x00000004U, 0x00000002U, 0x00000004U, 0x00040004U}, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetChar), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.MutableString, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetSubstring)
            );
            
            DefineLibraryMethod(module, "slice!", 0x51, 
                new[] { 0x00020000U, 0x00030000U, 0x00000004U, 0x00000004U, 0x00040004U, 0x00000002U}, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.RemoveCharInPlace), 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.RemoveSubstringInPlace), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.MutableString, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.RemoveSubstringInPlace), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.RemoveSubstringInPlace), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.RemoveSubstringInPlace), 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.RemoveSubstringInPlace)
            );
            
            DefineLibraryMethod(module, "split", 0x51, 
                0x00000000U, 0x00060000U, 0x00040004U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MutableStringOps.Split), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MutableStringOps.Split), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MutableStringOps.Split)
            );
            
            DefineLibraryMethod(module, "squeeze", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Squeeze)
            );
            
            DefineLibraryMethod(module, "squeeze!", 0x51, 
                0x80020004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.SqueezeInPlace)
            );
            
            DefineLibraryMethod(module, "start_with?", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.StartsWith)
            );
            
            DefineLibraryMethod(module, "strip", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Strip)
            );
            
            DefineLibraryMethod(module, "strip!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.StripInPlace)
            );
            
            DefineLibraryMethod(module, "sub", 0x51, 
                0x00000014U, 0x00000014U, 0x0002000cU, 0x00180030U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.MutableStringOps.BlockReplaceFirst), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.BlockReplaceFirst), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceFirst), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Runtime.Union<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceFirst)
            );
            
            DefineLibraryMethod(module, "sub!", 0x51, 
                0x00080014U, 0x0006000cU, 0x00180030U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.MutableStringOps.BlockReplaceFirstInPlace), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceFirstInPlace), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Runtime.Union<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ReplaceFirstInPlace)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Succ)
            );
            
            DefineLibraryMethod(module, "succ!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.SuccInPlace)
            );
            
            DefineLibraryMethod(module, "sum", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.GetChecksum)
            );
            
            DefineLibraryMethod(module, "swapcase", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.SwapCase)
            );
            
            DefineLibraryMethod(module, "swapcase!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.SwapCaseInPlace)
            );
            
            DefineLibraryMethod(module, "to_clr_string", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.String>(IronRuby.Builtins.MutableStringOps.ToClrString)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Double>(IronRuby.Builtins.MutableStringOps.ToDouble)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00010000U, 
                new Func<IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.Builtins.MutableStringOps.ToInteger)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ToS)
            );
            
            DefineLibraryMethod(module, "to_str", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.ToS)
            );
            
            DefineLibraryMethod(module, "to_sym", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.MutableStringOps.ToSymbol)
            );
            
            DefineLibraryMethod(module, "tr", 0x51, 
                0x00030006U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.GetTranslated)
            );
            
            DefineLibraryMethod(module, "tr!", 0x51, 
                0x00030006U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.Translate)
            );
            
            DefineLibraryMethod(module, "tr_s", 0x51, 
                0x00030006U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.TrSqueeze)
            );
            
            DefineLibraryMethod(module, "tr_s!", 0x51, 
                0x00030006U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.TrSqueezeInPlace)
            );
            
            DefineLibraryMethod(module, "unpack", 0x51, 
                0x00010002U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.MutableStringOps.Unpack)
            );
            
            DefineLibraryMethod(module, "upcase", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.UpCase)
            );
            
            DefineLibraryMethod(module, "upcase!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.MutableStringOps.UpCaseInPlace)
            );
            
            DefineLibraryMethod(module, "upto", 0x51, 
                0x00020004U, 0x0004000aU, 
                new Func<IronRuby.Builtins.RangeOps.EachStorage, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.MutableStringOps.UpTo), 
                new Func<IronRuby.Builtins.RangeOps.EachStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.MutableStringOps.UpTo)
            );
            
            DefineLibraryMethod(module, "valid_encoding?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.MutableString, System.Boolean>(IronRuby.Builtins.MutableStringOps.ValidEncoding)
            );
            
        }
        
        private static void LoadStruct_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            #if !SILVERLIGHT
            SetBuiltinConstant(module, "Tms", IronRuby.Builtins.RubyStructOps.CreateTmsClass(module));
            #endif
            
        }
        
        private static void LoadStruct_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00000000U, 0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, System.Int32, System.Object>(IronRuby.Builtins.RubyStructOps.GetValue), 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.RubySymbol, System.Object>(IronRuby.Builtins.RubyStructOps.GetValue), 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.RubyStructOps.GetValue), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyStruct, System.Object, System.Object>(IronRuby.Builtins.RubyStructOps.GetValue)
            );
            
            DefineLibraryMethod(module, "[]=", 0x51, 
                0x00000000U, 0x00000002U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, System.Int32, System.Object, System.Object>(IronRuby.Builtins.RubyStructOps.SetValue), 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.RubySymbol, System.Object, System.Object>(IronRuby.Builtins.RubyStructOps.SetValue), 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.MutableString, System.Object, System.Object>(IronRuby.Builtins.RubyStructOps.SetValue), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyStruct, System.Object, System.Object, System.Object>(IronRuby.Builtins.RubyStructOps.SetValue)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.RubyStruct, System.Object, System.Boolean>(IronRuby.Builtins.RubyStructOps.Equals)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyStruct, System.Object>(IronRuby.Builtins.RubyStructOps.Each)
            );
            
            DefineLibraryMethod(module, "each_pair", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyStruct, System.Object>(IronRuby.Builtins.RubyStructOps.EachPair)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.RubyStruct, System.Object, System.Boolean>(IronRuby.Builtins.RubyStructOps.Equal)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyStruct, System.Int32>(IronRuby.Builtins.RubyStructOps.Hash)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x80000000U, 
                new Action<IronRuby.Builtins.RubyStruct, System.Object[]>(IronRuby.Builtins.RubyStructOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.RubyStruct, IronRuby.Builtins.RubyStruct>(IronRuby.Builtins.RubyStructOps.InitializeCopy)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyStructOps.Inspect)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, System.Int32>(IronRuby.Builtins.RubyStructOps.GetSize)
            );
            
            DefineLibraryMethod(module, "members", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyStructOps.GetMembers)
            );
            
            DefineLibraryMethod(module, "select", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyStruct, System.Object>(IronRuby.Builtins.RubyStructOps.Select)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, System.Int32>(IronRuby.Builtins.RubyStructOps.GetSize)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyStructOps.Values)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyStructOps.Inspect)
            );
            
            DefineLibraryMethod(module, "values", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyStruct, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyStructOps.Values)
            );
            
            DefineLibraryMethod(module, "values_at", 0x51, 
                0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyStruct, System.Object[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyStructOps.ValuesAt)
            );
            
        }
        
        private static void LoadStruct_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "new", 0x61, 
                0x8004000cU, 0x8004000cU, 0x80060008U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubySymbol, System.String[], System.Object>(IronRuby.Builtins.RubyStructOps.NewAnonymousStruct), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.String, System.String[], System.Object>(IronRuby.Builtins.RubyStructOps.NewAnonymousStruct), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.String[], System.Object>(IronRuby.Builtins.RubyStructOps.NewStruct)
            );
            
        }
        
        private static void LoadSymbol_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.HideMethod("==");
            DefineLibraryMethod(module, "[]", 0x51, 
                new[] { 0x00010000U, 0x00030000U, 0x00000004U, 0x00000002U, 0x00000004U, 0x00040004U}, 
                new Func<IronRuby.Builtins.RubySymbol, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetChar), 
                new Func<IronRuby.Builtins.RubySymbol, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000002U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol, System.Int32>(IronRuby.Builtins.SymbolOps.Compare), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Runtime.ClrName, System.Int32>(IronRuby.Builtins.SymbolOps.Compare), 
                new Func<IronRuby.Builtins.RubySymbol, System.Object, System.Object>(IronRuby.Builtins.SymbolOps.Compare)
            );
            
            DefineLibraryMethod(module, "=~", 0x51, 
                0x00000004U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.SymbolOps.Match), 
                new Func<IronRuby.Runtime.ClrName, IronRuby.Builtins.RubySymbol, System.Object>(IronRuby.Builtins.SymbolOps.Match), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, System.Object, System.Object>(IronRuby.Builtins.SymbolOps.Match)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000002U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol, System.Boolean>(IronRuby.Builtins.SymbolOps.Equals), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Runtime.ClrName, System.Boolean>(IronRuby.Builtins.SymbolOps.Equals), 
                new Func<IronRuby.Builtins.RubySymbol, System.Object, System.Boolean>(IronRuby.Builtins.SymbolOps.Equals)
            );
            
            DefineLibraryMethod(module, "===", 0x51, 
                0x00000002U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol, System.Boolean>(IronRuby.Builtins.SymbolOps.Equals), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Runtime.ClrName, System.Boolean>(IronRuby.Builtins.SymbolOps.Equals), 
                new Func<IronRuby.Builtins.RubySymbol, System.Object, System.Boolean>(IronRuby.Builtins.SymbolOps.Equals)
            );
            
            DefineLibraryMethod(module, "capitalize", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.Capitalize)
            );
            
            DefineLibraryMethod(module, "casecmp", 0x51, 
                0x00000002U, 0x00010002U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol, System.Int32>(IronRuby.Builtins.SymbolOps.Casecmp), 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.Builtins.SymbolOps.Casecmp)
            );
            
            DefineLibraryMethod(module, "downcase", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.DownCase)
            );
            
            DefineLibraryMethod(module, "empty?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, System.Boolean>(IronRuby.Builtins.SymbolOps.IsEmpty)
            );
            
            DefineLibraryMethod(module, "encoding", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyEncoding>(IronRuby.Builtins.SymbolOps.GetEncoding)
            );
            
            DefineLibraryMethod(module, "id2name", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.ToString)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.Inspect)
            );
            
            DefineLibraryMethod(module, "intern", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.ToSymbol)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, System.Int32>(IronRuby.Builtins.SymbolOps.GetLength)
            );
            
            DefineLibraryMethod(module, "match", 0x51, 
                0x00000008U, 0x00040008U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.Builtins.SymbolOps.Match), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Runtime.RubyScope, System.Object, System.Object, System.Object>>, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.SymbolOps.Match)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.Succ)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, System.Int32>(IronRuby.Builtins.SymbolOps.GetLength)
            );
            
            DefineLibraryMethod(module, "slice", 0x51, 
                new[] { 0x00010000U, 0x00030000U, 0x00000004U, 0x00000002U, 0x00000004U, 0x00040004U}, 
                new Func<IronRuby.Builtins.RubySymbol, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetChar), 
                new Func<IronRuby.Builtins.RubySymbol, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.Range, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubyRegex, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.GetSubstring)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.Succ)
            );
            
            DefineLibraryMethod(module, "swapcase", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.SwapCase)
            );
            
            DefineLibraryMethod(module, "to_clr_string", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, System.String>(IronRuby.Builtins.SymbolOps.ToClrString)
            );
            
            DefineLibraryMethod(module, "to_proc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.Proc>(IronRuby.Builtins.SymbolOps.ToProc)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SymbolOps.ToString)
            );
            
            DefineLibraryMethod(module, "to_sym", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.ToSymbol)
            );
            
            DefineLibraryMethod(module, "upcase", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubySymbol, IronRuby.Builtins.RubySymbol>(IronRuby.Builtins.SymbolOps.UpCase)
            );
            
        }
        
        private static void LoadSymbol_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "all_symbols", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.SymbolOps.GetAllSymbols)
            );
            
        }
        
        private static void LoadSystem__Byte_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__Byte_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Integer_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ByteOps.Inspect)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<System.Byte, System.Object>(IronRuby.Builtins.ByteOps.Next)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.Byte, System.Int32>(IronRuby.Builtins.ByteOps.Size)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<System.Byte, System.Object>(IronRuby.Builtins.ByteOps.Next)
            );
            
        }
        
        private static void LoadSystem__Byte_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Byte>(IronRuby.Builtins.ByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.Byte>(IronRuby.Builtins.ByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Byte>(IronRuby.Builtins.ByteOps.InducedFrom)
            );
            
        }
        
        private static void LoadSystem__Char_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "dump", 0x51, 
                0x00000000U, 
                new Func<System.Char, IronRuby.Builtins.MutableString>(IronRuby.Builtins.CharOps.Dump)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Char, IronRuby.Builtins.MutableString>(IronRuby.Builtins.CharOps.Inspect)
            );
            
        }
        
        private static void LoadSystem__Collections__Generic__IDictionary_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.GetElement)
            );
            
            DefineLibraryMethod(module, "[]=", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Hash, System.Object, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.SetElement), 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.SetElement)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Boolean>(IronRuby.Builtins.IDictionaryOps.Equals), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Boolean>(IronRuby.Builtins.IDictionaryOps.Equals)
            );
            
            DefineLibraryMethod(module, "clear", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.Hash, System.Collections.Generic.IDictionary<System.Object, System.Object>>(IronRuby.Builtins.IDictionaryOps.Clear), 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Collections.Generic.IDictionary<System.Object, System.Object>>(IronRuby.Builtins.IDictionaryOps.Clear)
            );
            
            DefineLibraryMethod(module, "default", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.GetDefaultValue)
            );
            
            DefineLibraryMethod(module, "default_proc", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Proc>(IronRuby.Builtins.IDictionaryOps.GetDefaultProc)
            );
            
            DefineLibraryMethod(module, "delete", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Hash, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.Delete), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.Delete)
            );
            
            DefineLibraryMethod(module, "delete_if", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Hash, System.Object>(IronRuby.Builtins.IDictionaryOps.DeleteIf), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.DeleteIf)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IDictionaryOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Each)
            );
            
            DefineLibraryMethod(module, "each_key", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IDictionaryOps.EachKey), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.EachKey)
            );
            
            DefineLibraryMethod(module, "each_pair", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IDictionaryOps.Each), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Each)
            );
            
            DefineLibraryMethod(module, "each_value", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IDictionaryOps.EachValue), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.EachValue)
            );
            
            DefineLibraryMethod(module, "empty?", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Boolean>(IronRuby.Builtins.IDictionaryOps.Empty)
            );
            
            DefineLibraryMethod(module, "fetch", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.Fetch)
            );
            
            DefineLibraryMethod(module, "flatten", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Int32, System.Collections.IList>(IronRuby.Builtins.IDictionaryOps.Flatten)
            );
            
            DefineLibraryMethod(module, "has_key?", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Boolean>(IronRuby.Builtins.IDictionaryOps.HasKey)
            );
            
            DefineLibraryMethod(module, "has_value?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Boolean>(IronRuby.Builtins.IDictionaryOps.HasValue)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Boolean>(IronRuby.Builtins.IDictionaryOps.HasKey)
            );
            
            DefineLibraryMethod(module, "index", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.Index)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.IDictionaryOps.ToMutableString)
            );
            
            DefineLibraryMethod(module, "invert", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Hash>(IronRuby.Builtins.IDictionaryOps.Invert)
            );
            
            DefineLibraryMethod(module, "key?", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Boolean>(IronRuby.Builtins.IDictionaryOps.HasKey)
            );
            
            DefineLibraryMethod(module, "keys", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IDictionaryOps.GetKeys)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Int32>(IronRuby.Builtins.IDictionaryOps.Length)
            );
            
            DefineLibraryMethod(module, "member?", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Boolean>(IronRuby.Builtins.IDictionaryOps.HasKey)
            );
            
            DefineLibraryMethod(module, "merge", 0x51, 
                0x00080010U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object>>, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Merge)
            );
            
            DefineLibraryMethod(module, "merge!", 0x51, 
                0x00020004U, 0x00020004U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Hash, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Update), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Update)
            );
            
            DefineLibraryMethod(module, "rehash", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.Hash, System.Collections.Generic.IDictionary<System.Object, System.Object>>(IronRuby.Builtins.IDictionaryOps.Rehash), 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Collections.Generic.IDictionary<System.Object, System.Object>>(IronRuby.Builtins.IDictionaryOps.Rehash)
            );
            
            DefineLibraryMethod(module, "reject", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object>>, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Reject)
            );
            
            DefineLibraryMethod(module, "reject!", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Hash, System.Object>(IronRuby.Builtins.IDictionaryOps.RejectMutate), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.RejectMutate)
            );
            
            DefineLibraryMethod(module, "replace", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Hash, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Hash>(IronRuby.Builtins.IDictionaryOps.Replace)
            );
            
            DefineLibraryMethod(module, "select", 0x51, 
                0x00000000U, 0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IDictionaryOps.Select), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Select)
            );
            
            DefineLibraryMethod(module, "shift", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.Hash, System.Object>(IronRuby.Builtins.IDictionaryOps.Shift), 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Shift)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Int32>(IronRuby.Builtins.IDictionaryOps.Length)
            );
            
            DefineLibraryMethod(module, "sort", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Sort)
            );
            
            DefineLibraryMethod(module, "store", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.Hash, System.Object, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.SetElement), 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Object, System.Object>(IronRuby.Builtins.IDictionaryOps.SetElement)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IDictionaryOps.ToArray)
            );
            
            DefineLibraryMethod(module, "to_hash", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Collections.Generic.IDictionary<System.Object, System.Object>>(IronRuby.Builtins.IDictionaryOps.ToHash)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.MutableString>(IronRuby.Builtins.IDictionaryOps.ToMutableString)
            );
            
            DefineLibraryMethod(module, "update", 0x51, 
                0x00020004U, 0x00020004U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.Hash, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Update), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object>(IronRuby.Builtins.IDictionaryOps.Update)
            );
            
            DefineLibraryMethod(module, "value?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object, System.Boolean>(IronRuby.Builtins.IDictionaryOps.HasValue)
            );
            
            DefineLibraryMethod(module, "values", 0x51, 
                0x00000000U, 
                new Func<System.Collections.Generic.IDictionary<System.Object, System.Object>, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IDictionaryOps.GetValues)
            );
            
            DefineLibraryMethod(module, "values_at", 0x51, 
                0x80000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.Generic.IDictionary<System.Object, System.Object>, System.Object[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IDictionaryOps.ValuesAt)
            );
            
        }
        
        private static void LoadSystem__Collections__IEnumerable_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IEnumerable, System.Object>(IronRuby.Builtins.IEnumerableOps.Each)
            );
            
        }
        
        private static void LoadSystem__Collections__IList_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "-", 0x51, 
                0x00040008U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Collections.IList, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.Difference)
            );
            
            DefineLibraryMethod(module, "&", 0x51, 
                0x00040000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Collections.IList, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.Intersection)
            );
            
            DefineLibraryMethod(module, "*", 0x51, 
                0x00000000U, 0x00000004U, 0x00040008U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.Repeat), 
                new Func<IronRuby.Runtime.JoinConversionStorage, System.Collections.IList, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.IListOps.Repeat), 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.JoinConversionStorage, System.Collections.IList, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, System.Object>(IronRuby.Builtins.IListOps.Repeat)
            );
            
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00010000U, 0x00060000U, 0x00000008U, 
                new Func<System.Collections.IList, System.Int32, System.Object>(IronRuby.Builtins.IListOps.GetElement), 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Int32, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.GetElements), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, IronRuby.Builtins.Range, System.Collections.IList>(IronRuby.Builtins.IListOps.GetElements)
            );
            
            DefineLibraryMethod(module, "[]=", 0x51, 
                0x00010000U, 0x00010000U, 0x00060000U, 0x00000008U, 
                new Func<IronRuby.Builtins.RubyArray, System.Int32, System.Object, System.Object>(IronRuby.Builtins.IListOps.SetElement), 
                new Func<System.Collections.IList, System.Int32, System.Object, System.Object>(IronRuby.Builtins.IListOps.SetElement), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Collections.IList, System.Int32, System.Int32, System.Object, System.Object>(IronRuby.Builtins.IListOps.SetElement), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Runtime.ConversionStorage<System.Int32>, System.Collections.IList, IronRuby.Builtins.Range, System.Object, System.Object>(IronRuby.Builtins.IListOps.SetElement)
            );
            
            DefineLibraryMethod(module, "|", 0x51, 
                0x00040000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Collections.IList, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.Union)
            );
            
            DefineLibraryMethod(module, "+", 0x51, 
                0x00010002U, 
                new Func<System.Collections.IList, System.Collections.IList, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.Concatenate)
            );
            
            DefineLibraryMethod(module, "<<", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Object, System.Collections.IList>(IronRuby.Builtins.IListOps.Append)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Collections.IList, System.Object, System.Object>(IronRuby.Builtins.IListOps.Compare), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.Compare)
            );
            
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 0x00000004U, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Object, System.Boolean>(IronRuby.Builtins.IListOps.Equals), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Collections.IList, System.Boolean>(IronRuby.Builtins.IListOps.Equals)
            );
            
            DefineLibraryMethod(module, "assoc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Object, System.Collections.IList>(IronRuby.Builtins.IListOps.GetContainerOfFirstItem)
            );
            
            DefineLibraryMethod(module, "at", 0x51, 
                0x00010000U, 
                new Func<System.Collections.IList, System.Int32, System.Object>(IronRuby.Builtins.IListOps.At)
            );
            
            DefineLibraryMethod(module, "clear", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.Clear)
            );
            
            DefineLibraryMethod(module, "collect!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.CollectInPlace)
            );
            
            DefineLibraryMethod(module, "combination", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Nullable<System.Int32>, System.Object>(IronRuby.Builtins.IListOps.GetCombinations)
            );
            
            DefineLibraryMethod(module, "compact", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.Compact)
            );
            
            DefineLibraryMethod(module, "compact!", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.CompactInPlace)
            );
            
            DefineLibraryMethod(module, "concat", 0x51, 
                0x00010002U, 
                new Func<System.Collections.IList, System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.Concat)
            );
            
            DefineLibraryMethod(module, "count", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Int32>(IronRuby.Builtins.IListOps.Length)
            );
            
            DefineLibraryMethod(module, "delete", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Object, System.Object>(IronRuby.Builtins.IListOps.Delete), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object, System.Object>(IronRuby.Builtins.IListOps.Delete)
            );
            
            DefineLibraryMethod(module, "delete_at", 0x51, 
                0x00010000U, 
                new Func<System.Collections.IList, System.Int32, System.Object>(IronRuby.Builtins.IListOps.DeleteAt)
            );
            
            DefineLibraryMethod(module, "delete_if", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.DeleteIf)
            );
            
            DefineLibraryMethod(module, "each", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<System.Collections.IList, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IListOps.Each), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.Each)
            );
            
            DefineLibraryMethod(module, "each_index", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<System.Collections.IList, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IListOps.EachIndex), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.EachIndex)
            );
            
            DefineLibraryMethod(module, "empty?", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Boolean>(IronRuby.Builtins.IListOps.Empty)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Object, System.Boolean>(IronRuby.Builtins.IListOps.HashEquals)
            );
            
            DefineLibraryMethod(module, "fetch", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object, System.Object, System.Object>(IronRuby.Builtins.IListOps.Fetch)
            );
            
            DefineLibraryMethod(module, "fill", 0x51, 
                new[] { 0x00000000U, 0x00000000U, 0x00000000U, 0x00000008U, 0x00000001U, 0x00000001U, 0x00000002U, 0x0000000aU}, 
                new Func<System.Collections.IList, System.Object, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.Fill), 
                new Func<System.Collections.IList, System.Object, System.Int32, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.Fill), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, System.Collections.IList, System.Object, System.Object, System.Object, System.Collections.IList>(IronRuby.Builtins.IListOps.Fill), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, System.Collections.IList, System.Object, IronRuby.Builtins.Range, System.Collections.IList>(IronRuby.Builtins.IListOps.Fill), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Int32, System.Object>(IronRuby.Builtins.IListOps.Fill), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Int32, System.Int32, System.Object>(IronRuby.Builtins.IListOps.Fill), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object, System.Object, System.Object>(IronRuby.Builtins.IListOps.Fill), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.BlockParam, System.Collections.IList, IronRuby.Builtins.Range, System.Object>(IronRuby.Builtins.IListOps.Fill)
            );
            
            DefineLibraryMethod(module, "find_index", 0x51, 
                0x00000000U, 0x00000001U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IListOps.GetFindIndexEnumerator), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.FindIndex), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object, System.Object>(IronRuby.Builtins.IListOps.FindIndex)
            );
            
            DefineLibraryMethod(module, "first", 0x51, 
                0x00000000U, 0x00010000U, 
                new Func<System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.First), 
                new Func<System.Collections.IList, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.First)
            );
            
            DefineLibraryMethod(module, "flatten", 0x51, 
                0x00040000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Collections.IList, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.Flatten)
            );
            
            DefineLibraryMethod(module, "flatten!", 0x51, 
                0x00020000U, 0x00020000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, IronRuby.Builtins.RubyArray, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.FlattenInPlace), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Collections.IList, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.FlattenInPlace)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ConversionStorage<System.Int32>, System.Collections.IList, System.Int32>(IronRuby.Builtins.IListOps.GetHashCode)
            );
            
            DefineLibraryMethod(module, "include?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Object, System.Boolean>(IronRuby.Builtins.IListOps.Include)
            );
            
            DefineLibraryMethod(module, "index", 0x51, 
                0x00000000U, 0x00000001U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IListOps.GetFindIndexEnumerator), 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.FindIndex), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object, System.Object>(IronRuby.Builtins.IListOps.FindIndex)
            );
            
            DefineLibraryMethod(module, "indexes", 0x51, 
                0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Object[], System.Object>(IronRuby.Builtins.IListOps.Indexes)
            );
            
            DefineLibraryMethod(module, "indices", 0x51, 
                0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Object[], System.Object>(IronRuby.Builtins.IListOps.Indexes)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00010002U, 
                new Func<System.Collections.IList, System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.Replace)
            );
            
            DefineLibraryMethod(module, "insert", 0x51, 
                0x80010000U, 
                new Func<System.Collections.IList, System.Int32, System.Object[], System.Collections.IList>(IronRuby.Builtins.IListOps.Insert)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.IList, IronRuby.Builtins.MutableString>(IronRuby.Builtins.IListOps.Inspect)
            );
            
            DefineLibraryMethod(module, "join", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.JoinConversionStorage, System.Collections.IList, IronRuby.Builtins.MutableString>(IronRuby.Builtins.IListOps.Join), 
                new Func<IronRuby.Runtime.JoinConversionStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, System.Collections.IList, System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.IListOps.JoinWithLazySeparatorConversion)
            );
            
            DefineLibraryMethod(module, "last", 0x51, 
                0x00000000U, 0x00010000U, 
                new Func<System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.Last), 
                new Func<System.Collections.IList, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.Last)
            );
            
            DefineLibraryMethod(module, "length", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Int32>(IronRuby.Builtins.IListOps.Length)
            );
            
            DefineLibraryMethod(module, "map!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.CollectInPlace)
            );
            
            DefineLibraryMethod(module, "nitems", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Int32>(IronRuby.Builtins.IListOps.NumberOfNonNilItems)
            );
            
            DefineLibraryMethod(module, "none?", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Boolean>(IronRuby.Builtins.IListOps.Empty)
            );
            
            DefineLibraryMethod(module, "permutation", 0x51, 
                0x00020000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Nullable<System.Int32>, System.Object>(IronRuby.Builtins.IListOps.GetPermutations)
            );
            
            DefineLibraryMethod(module, "pop", 0x51, 
                0x00000000U, 0x00020000U, 
                new Func<System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.Pop), 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.IList, System.Int32, System.Object>(IronRuby.Builtins.IListOps.Pop)
            );
            
            DefineLibraryMethod(module, "product", 0x51, 
                0x80010002U, 
                new Func<System.Collections.IList, System.Collections.IList[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.Product)
            );
            
            DefineLibraryMethod(module, "push", 0x51, 
                0x80000000U, 
                new Func<System.Collections.IList, System.Object[], System.Collections.IList>(IronRuby.Builtins.IListOps.Push)
            );
            
            DefineLibraryMethod(module, "rassoc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Object, System.Collections.IList>(IronRuby.Builtins.IListOps.GetContainerOfSecondItem)
            );
            
            DefineLibraryMethod(module, "reject", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.Proc, System.Object>>, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.Reject)
            );
            
            DefineLibraryMethod(module, "reject!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.RejectInPlace)
            );
            
            DefineLibraryMethod(module, "replace", 0x51, 
                0x00010002U, 
                new Func<System.Collections.IList, System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.Replace)
            );
            
            DefineLibraryMethod(module, "reverse", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.Reverse)
            );
            
            DefineLibraryMethod(module, "reverse!", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.InPlaceReverse)
            );
            
            DefineLibraryMethod(module, "reverse_each", 0x51, 
                0x00000000U, 0x00000001U, 
                new Func<IronRuby.Builtins.RubyArray, IronRuby.Builtins.Enumerator>(IronRuby.Builtins.IListOps.ReverseEach), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyArray, System.Object>(IronRuby.Builtins.IListOps.ReverseEach)
            );
            
            DefineLibraryMethod(module, "rindex", 0x51, 
                0x00000001U, 0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.ReverseIndex), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object, System.Object>(IronRuby.Builtins.IListOps.ReverseIndex)
            );
            
            DefineLibraryMethod(module, "shift", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.Shift)
            );
            
            DefineLibraryMethod(module, "shuffle", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Builtins.RubyArray, System.Collections.IList>(IronRuby.Builtins.IListOps.Shuffle)
            );
            
            DefineLibraryMethod(module, "shuffle!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyArray, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.ShuffleInPlace)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, System.Int32>(IronRuby.Builtins.IListOps.Length)
            );
            
            DefineLibraryMethod(module, "slice", 0x51, 
                0x00010000U, 0x00060000U, 0x00000008U, 
                new Func<System.Collections.IList, System.Int32, System.Object>(IronRuby.Builtins.IListOps.GetElement), 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Int32, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.GetElements), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, IronRuby.Builtins.Range, System.Collections.IList>(IronRuby.Builtins.IListOps.GetElements)
            );
            
            DefineLibraryMethod(module, "slice!", 0x51, 
                0x00010000U, 0x00000008U, 0x00060000U, 
                new Func<System.Collections.IList, System.Int32, System.Object>(IronRuby.Builtins.IListOps.SliceInPlace), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, IronRuby.Builtins.Range, System.Collections.IList>(IronRuby.Builtins.IListOps.SliceInPlace), 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Int32, System.Int32, System.Collections.IList>(IronRuby.Builtins.IListOps.SliceInPlace)
            );
            
            DefineLibraryMethod(module, "sort", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.Sort)
            );
            
            DefineLibraryMethod(module, "sort!", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ComparisonStorage, IronRuby.Runtime.BlockParam, System.Collections.IList, System.Object>(IronRuby.Builtins.IListOps.SortInPlace)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.ToArray)
            );
            
            DefineLibraryMethod(module, "to_ary", 0x51, 
                0x00000000U, 
                new Func<System.Collections.IList, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.ToArray)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Collections.IList, IronRuby.Builtins.MutableString>(IronRuby.Builtins.IListOps.Inspect)
            );
            
            DefineLibraryMethod(module, "transpose", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Collections.IList, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.Transpose)
            );
            
            DefineLibraryMethod(module, "uniq", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.Unique)
            );
            
            DefineLibraryMethod(module, "uniq!", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.RubyArray, System.Collections.IList>(IronRuby.Builtins.IListOps.UniqueSelf), 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, System.Collections.IList, System.Collections.IList>(IronRuby.Builtins.IListOps.UniqueSelf)
            );
            
            DefineLibraryMethod(module, "unshift", 0x51, 
                0x00000000U, 0x80000000U, 
                new Func<System.Collections.IList, System.Object, System.Collections.IList>(IronRuby.Builtins.IListOps.Unshift), 
                new Func<System.Collections.IList, System.Object[], System.Collections.IList>(IronRuby.Builtins.IListOps.Unshift)
            );
            
            DefineLibraryMethod(module, "values_at", 0x51, 
                0x80000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.UnaryOpStorage, System.Collections.IList, System.Object[], IronRuby.Builtins.RubyArray>(IronRuby.Builtins.IListOps.ValuesAt)
            );
            
        }
        
        private static void LoadSystem__Decimal_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Decimal, System.Double, System.Boolean>(IronRuby.Builtins.DecimalOps.Equal), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Decimal, System.Object, System.Boolean>(IronRuby.Builtins.DecimalOps.Equal)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.DecimalOps.Inspect)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.Object, System.Int32>(IronRuby.Builtins.DecimalOps.Size)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<System.Decimal, System.Double>(IronRuby.Builtins.DecimalOps.ToDouble)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00000000U, 
                new Func<System.Decimal, System.Object>(IronRuby.Builtins.DecimalOps.ToInt)
            );
            
            DefineLibraryMethod(module, "to_int", 0x51, 
                0x00000000U, 
                new Func<System.Decimal, System.Object>(IronRuby.Builtins.DecimalOps.ToInt)
            );
            
        }
        
        private static void LoadSystem__Decimal_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                new[] { 0x00000000U, 0x00000000U, 0x00000000U, 0x00000002U, 0x00000000U}, 
                new Func<IronRuby.Builtins.RubyModule, System.Double, System.Decimal>(IronRuby.Builtins.DecimalOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyModule, System.Decimal, System.Decimal>(IronRuby.Builtins.DecimalOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyModule, System.Int32, System.Decimal>(IronRuby.Builtins.DecimalOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyModule, Microsoft.Scripting.Math.BigInteger, System.Decimal>(IronRuby.Builtins.DecimalOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyModule, System.Object, System.Double>(IronRuby.Builtins.DecimalOps.InducedFrom)
            );
            
        }
        
        private static void LoadSystem__IComparable_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000000U, 
                new Func<System.IComparable, System.Object, System.Int32>(IronRuby.Builtins.IComparableOps.Compare)
            );
            
        }
        
        private static void LoadSystem__Int16_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__Int16_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Integer_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.Int16Ops.Inspect)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<System.Int16, System.Object>(IronRuby.Builtins.Int16Ops.Next)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.Int16, System.Int32>(IronRuby.Builtins.Int16Ops.Size)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<System.Int16, System.Object>(IronRuby.Builtins.Int16Ops.Next)
            );
            
        }
        
        private static void LoadSystem__Int16_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int16>(IronRuby.Builtins.Int16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.Int16>(IronRuby.Builtins.Int16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Int16>(IronRuby.Builtins.Int16Ops.InducedFrom)
            );
            
        }
        
        private static void LoadSystem__Int64_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__Int64_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__BigInteger_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.Int64Ops.Inspect)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<System.Int64, System.Object>(IronRuby.Builtins.Int64Ops.Next)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.Int64, System.Int32>(IronRuby.Builtins.Int64Ops.Size)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<System.Int64, System.Object>(IronRuby.Builtins.Int64Ops.Next)
            );
            
        }
        
        private static void LoadSystem__Int64_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int64>(IronRuby.Builtins.Int64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.Int64>(IronRuby.Builtins.Int64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.Int64>(IronRuby.Builtins.Int64Ops.InducedFrom)
            );
            
        }
        
        private static void LoadSystem__SByte_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__SByte_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Integer_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SByteOps.Inspect)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<System.SByte, System.Object>(IronRuby.Builtins.SByteOps.Next)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.SByte, System.Int32>(IronRuby.Builtins.SByteOps.Size)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<System.SByte, System.Object>(IronRuby.Builtins.SByteOps.Next)
            );
            
        }
        
        private static void LoadSystem__SByte_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.SByte>(IronRuby.Builtins.SByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.SByte>(IronRuby.Builtins.SByteOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.SByte>(IronRuby.Builtins.SByteOps.InducedFrom)
            );
            
        }
        
        private static void LoadSystem__Single_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__Single_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Float_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.SingleOps.Inspect)
            );
            
        }
        
        private static void LoadSystem__Single_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Float_Class(module);
        }
        
        private static void LoadSystem__String_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.HideMethod("[]");
            module.HideMethod("==");
            module.HideMethod("clone");
            module.HideMethod("insert");
            module.HideMethod("split");
        }
        
        private static void LoadSystem__Type_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "to_class", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Type, IronRuby.Builtins.RubyClass>(IronRuby.Builtins.TypeOps.ToClass)
            );
            
            DefineLibraryMethod(module, "to_module", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Type, IronRuby.Builtins.RubyModule>(IronRuby.Builtins.TypeOps.ToModule)
            );
            
        }
        
        private static void LoadSystem__UInt16_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__UInt16_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__Integer_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.UInt16Ops.Inspect)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<System.UInt16, System.Object>(IronRuby.Builtins.UInt16Ops.Next)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.UInt16, System.Int32>(IronRuby.Builtins.UInt16Ops.Size)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<System.UInt16, System.Object>(IronRuby.Builtins.UInt16Ops.Next)
            );
            
        }
        
        private static void LoadSystem__UInt16_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.UInt16>(IronRuby.Builtins.UInt16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.UInt16>(IronRuby.Builtins.UInt16Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.UInt16>(IronRuby.Builtins.UInt16Ops.InducedFrom)
            );
            
        }
        
        private static void LoadSystem__UInt32_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__UInt32_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__BigInteger_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.UInt32Ops.Inspect)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<System.UInt32, System.Object>(IronRuby.Builtins.UInt32Ops.Next)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.UInt32, System.Int32>(IronRuby.Builtins.UInt32Ops.Size)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<System.UInt32, System.Object>(IronRuby.Builtins.UInt32Ops.Next)
            );
            
        }
        
        private static void LoadSystem__UInt32_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.UInt32>(IronRuby.Builtins.UInt32Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.UInt32>(IronRuby.Builtins.UInt32Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.UInt32>(IronRuby.Builtins.UInt32Ops.InducedFrom)
            );
            
        }
        
        private static void LoadSystem__UInt64_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            
        }
        
        private static void LoadSystem__UInt64_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadIronRuby__Clr__BigInteger_Instance(module);
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.MutableString>(IronRuby.Builtins.UInt64Ops.Inspect)
            );
            
            DefineLibraryMethod(module, "next", 0x51, 
                0x00000000U, 
                new Func<System.UInt64, System.Object>(IronRuby.Builtins.UInt64Ops.Next)
            );
            
            DefineLibraryMethod(module, "size", 0x51, 
                0x00000000U, 
                new Func<System.UInt64, System.Int32>(IronRuby.Builtins.UInt64Ops.Size)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<System.UInt64, System.Object>(IronRuby.Builtins.UInt64Ops.Next)
            );
            
        }
        
        private static void LoadSystem__UInt64_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "induced_from", 0x61, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.UInt64>(IronRuby.Builtins.UInt64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, System.UInt64>(IronRuby.Builtins.UInt64Ops.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, System.UInt64>(IronRuby.Builtins.UInt64Ops.InducedFrom)
            );
            
        }
        
        private static void LoadSystemCallError_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "errno", 0x51, 
                0x00000000U, 
                new Func<System.Runtime.InteropServices.ExternalException, System.Int32>(IronRuby.Builtins.SystemCallErrorOps.Errno)
            );
            
        }
        
        private static void LoadSystemExit_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "status", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.SystemExit, System.Int32>(IronRuby.Builtins.SystemExitOps.GetStatus)
            );
            
            DefineLibraryMethod(module, "success?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.SystemExit, System.Boolean>(IronRuby.Builtins.SystemExitOps.IsSuccessful)
            );
            
        }
        
        private static void LoadThread_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x51, 
                0x00000002U, 0x00000004U, 0x00000000U, 
                new Func<System.Threading.Thread, IronRuby.Builtins.RubySymbol, System.Object>(IronRuby.Builtins.ThreadOps.GetElement), 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.ThreadOps.GetElement), 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, System.Object, System.Object>(IronRuby.Builtins.ThreadOps.GetElement)
            );
            
            DefineLibraryMethod(module, "[]=", 0x51, 
                0x00000002U, 0x00000004U, 0x00000000U, 
                new Func<System.Threading.Thread, IronRuby.Builtins.RubySymbol, System.Object, System.Object>(IronRuby.Builtins.ThreadOps.SetElement), 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, IronRuby.Builtins.MutableString, System.Object, System.Object>(IronRuby.Builtins.ThreadOps.SetElement), 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, System.Object, System.Object, System.Object>(IronRuby.Builtins.ThreadOps.SetElement)
            );
            
            DefineLibraryMethod(module, "abort_on_exception", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Object>(IronRuby.Builtins.ThreadOps.AbortOnException)
            );
            
            DefineLibraryMethod(module, "abort_on_exception=", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Boolean, System.Object>(IronRuby.Builtins.ThreadOps.AbortOnException)
            );
            
            DefineLibraryMethod(module, "alive?", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Boolean>(IronRuby.Builtins.ThreadOps.IsAlive)
            );
            
            DefineLibraryMethod(module, "exit", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Kill)
            );
            
            DefineLibraryMethod(module, "group", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, IronRuby.Builtins.ThreadGroup>(IronRuby.Builtins.ThreadOps.Group)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ThreadOps.Inspect)
            );
            
            DefineLibraryMethod(module, "join", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Threading.Thread, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Join), 
                new Func<System.Threading.Thread, System.Double, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Join)
            );
            
            DefineLibraryMethod(module, "key?", 0x51, 
                0x00000002U, 0x00000004U, 0x00000000U, 
                new Func<System.Threading.Thread, IronRuby.Builtins.RubySymbol, System.Object>(IronRuby.Builtins.ThreadOps.HasKey), 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, IronRuby.Builtins.MutableString, System.Object>(IronRuby.Builtins.ThreadOps.HasKey), 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, System.Object, System.Object>(IronRuby.Builtins.ThreadOps.HasKey)
            );
            
            DefineLibraryMethod(module, "keys", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Threading.Thread, System.Object>(IronRuby.Builtins.ThreadOps.Keys)
            );
            
            DefineLibraryMethod(module, "kill", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Kill)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "priority", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Object>(IronRuby.Builtins.ThreadOps.Priority)
            );
            
            #endif
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "priority=", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Int32, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Priority)
            );
            
            #endif
            DefineLibraryMethod(module, "raise", 0x51, 
                0x00000000U, 0x00000002U, 0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.RaiseException), 
                new Action<System.Threading.Thread, IronRuby.Builtins.MutableString>(IronRuby.Builtins.ThreadOps.RaiseException), 
                new Action<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.CallSiteStorage<Action<System.Runtime.CompilerServices.CallSite, System.Exception, IronRuby.Builtins.RubyArray>>, System.Threading.Thread, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ThreadOps.RaiseException)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "run", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Run)
            );
            
            #endif
            DefineLibraryMethod(module, "status", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Object>(IronRuby.Builtins.ThreadOps.Status)
            );
            
            DefineLibraryMethod(module, "stop?", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Boolean>(IronRuby.Builtins.ThreadOps.IsStopped)
            );
            
            DefineLibraryMethod(module, "terminate", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Kill)
            );
            
            DefineLibraryMethod(module, "value", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Object>(IronRuby.Builtins.ThreadOps.Value)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "wakeup", 0x51, 
                0x00000000U, 
                new Func<System.Threading.Thread, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Run)
            );
            
            #endif
        }
        
        private static void LoadThread_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "abort_on_exception", 0x61, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.Builtins.ThreadOps.GlobalAbortOnException)
            );
            
            DefineLibraryMethod(module, "abort_on_exception=", 0x61, 
                0x00000000U, 
                new Func<System.Object, System.Boolean, System.Object>(IronRuby.Builtins.ThreadOps.GlobalAbortOnException)
            );
            
            DefineLibraryMethod(module, "critical", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.ThreadOps.Critical)
            );
            
            DefineLibraryMethod(module, "critical=", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object, System.Boolean>(IronRuby.Builtins.ThreadOps.Critical)
            );
            
            DefineLibraryMethod(module, "current", 0x61, 
                0x00000000U, 
                new Func<System.Object, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.Current)
            );
            
            DefineLibraryMethod(module, "list", 0x61, 
                0x00000000U, 
                new Func<System.Object, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ThreadOps.List)
            );
            
            DefineLibraryMethod(module, "main", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, System.Threading.Thread>(IronRuby.Builtins.ThreadOps.GetMainThread)
            );
            
            DefineLibraryMethod(module, "new", 0x61, 
                0x80000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object[], System.Threading.Thread>(IronRuby.Builtins.ThreadOps.CreateThread)
            );
            
            DefineLibraryMethod(module, "pass", 0x61, 
                0x00000000U, 
                new Action<System.Object>(IronRuby.Builtins.ThreadOps.Yield)
            );
            
            DefineLibraryMethod(module, "start", 0x61, 
                0x80000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object[], System.Threading.Thread>(IronRuby.Builtins.ThreadOps.CreateThread)
            );
            
            DefineLibraryMethod(module, "stop", 0x61, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, System.Object>(IronRuby.Builtins.ThreadOps.Stop)
            );
            
        }
        
        private static void LoadThreadGroup_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetBuiltinConstant(module, "Default", IronRuby.Builtins.ThreadGroup.Default);
            
        }
        
        private static void LoadThreadGroup_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "add", 0x51, 
                0x00000003U, 
                new Func<IronRuby.Builtins.ThreadGroup, System.Threading.Thread, IronRuby.Builtins.ThreadGroup>(IronRuby.Builtins.ThreadGroup.Add)
            );
            
            DefineLibraryMethod(module, "list", 0x51, 
                0x00000001U, 
                new Func<IronRuby.Builtins.ThreadGroup, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.ThreadGroup.List)
            );
            
        }
        
        private static void LoadTime_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "-", 0x51, 
                0x00010000U, 0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Double, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.SubtractSeconds), 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime, System.Double>(IronRuby.Builtins.RubyTimeOps.SubtractTime), 
                new Func<IronRuby.Builtins.RubyTime, System.DateTime, System.Double>(IronRuby.Builtins.RubyTimeOps.SubtractTime)
            );
            
            DefineLibraryMethod(module, "_dump", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyTimeOps.Dump)
            );
            
            DefineLibraryMethod(module, "+", 0x51, 
                0x00010000U, 0x00000002U, 
                new Func<IronRuby.Builtins.RubyTime, System.Double, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.AddSeconds), 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.AddSeconds)
            );
            
            DefineLibraryMethod(module, "<=>", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.CompareTo), 
                new Func<IronRuby.Builtins.RubyTime, System.Object, System.Object>(IronRuby.Builtins.RubyTimeOps.CompareSeconds)
            );
            
            DefineLibraryMethod(module, "asctime", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyTimeOps.CTime)
            );
            
            DefineLibraryMethod(module, "ctime", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyTimeOps.CTime)
            );
            
            DefineLibraryMethod(module, "day", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Day)
            );
            
            DefineLibraryMethod(module, "dst?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, System.Object>(IronRuby.Builtins.RubyTimeOps.IsDst)
            );
            
            DefineLibraryMethod(module, "eql?", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime, System.Boolean>(IronRuby.Builtins.RubyTimeOps.Eql), 
                new Func<IronRuby.Builtins.RubyTime, System.Object, System.Boolean>(IronRuby.Builtins.RubyTimeOps.Eql)
            );
            
            DefineLibraryMethod(module, "getgm", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.GetUTC)
            );
            
            DefineLibraryMethod(module, "getlocal", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.ToLocalTime)
            );
            
            DefineLibraryMethod(module, "getutc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.GetUTC)
            );
            
            DefineLibraryMethod(module, "gmt?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Boolean>(IronRuby.Builtins.RubyTimeOps.IsUts)
            );
            
            DefineLibraryMethod(module, "gmt_offset", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Object>(IronRuby.Builtins.RubyTimeOps.Offset)
            );
            
            DefineLibraryMethod(module, "gmtime", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.SwitchToUtc)
            );
            
            DefineLibraryMethod(module, "gmtoff", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Object>(IronRuby.Builtins.RubyTimeOps.Offset)
            );
            
            DefineLibraryMethod(module, "hash", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.GetHash)
            );
            
            DefineLibraryMethod(module, "hour", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Hour)
            );
            
            DefineLibraryMethod(module, "initialize", 0x52, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x52, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.InitializeCopy)
            );
            
            DefineLibraryMethod(module, "inspect", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyTimeOps.ToString)
            );
            
            DefineLibraryMethod(module, "isdst", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, System.Object>(IronRuby.Builtins.RubyTimeOps.IsDst)
            );
            
            DefineLibraryMethod(module, "localtime", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.ToLocalTime)
            );
            
            DefineLibraryMethod(module, "mday", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Day)
            );
            
            DefineLibraryMethod(module, "min", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Minute)
            );
            
            DefineLibraryMethod(module, "mon", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Month)
            );
            
            DefineLibraryMethod(module, "month", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Month)
            );
            
            DefineLibraryMethod(module, "sec", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Second)
            );
            
            DefineLibraryMethod(module, "strftime", 0x51, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyTimeOps.FormatTime)
            );
            
            DefineLibraryMethod(module, "succ", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.SuccessiveSecond)
            );
            
            DefineLibraryMethod(module, "to_a", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.RubyTimeOps.ToArray)
            );
            
            DefineLibraryMethod(module, "to_f", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Double>(IronRuby.Builtins.RubyTimeOps.ToFloatSeconds)
            );
            
            DefineLibraryMethod(module, "to_i", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Object>(IronRuby.Builtins.RubyTimeOps.ToSeconds)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyTimeOps.ToString)
            );
            
            DefineLibraryMethod(module, "tv_sec", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Object>(IronRuby.Builtins.RubyTimeOps.ToSeconds)
            );
            
            DefineLibraryMethod(module, "tv_usec", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.GetMicroSeconds)
            );
            
            DefineLibraryMethod(module, "usec", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.GetMicroSeconds)
            );
            
            DefineLibraryMethod(module, "utc", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.SwitchToUtc)
            );
            
            DefineLibraryMethod(module, "utc?", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Boolean>(IronRuby.Builtins.RubyTimeOps.IsUts)
            );
            
            DefineLibraryMethod(module, "utc_offset", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Object>(IronRuby.Builtins.RubyTimeOps.Offset)
            );
            
            DefineLibraryMethod(module, "wday", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.DayOfWeek)
            );
            
            DefineLibraryMethod(module, "yday", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.DayOfYear)
            );
            
            DefineLibraryMethod(module, "year", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyTime, System.Int32>(IronRuby.Builtins.RubyTimeOps.Year)
            );
            
            DefineLibraryMethod(module, "zone", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyTime, IronRuby.Builtins.MutableString>(IronRuby.Builtins.RubyTimeOps.GetZone)
            );
            
        }
        
        private static void LoadTime_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "_load", 0x61, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.Load)
            );
            
            DefineLibraryMethod(module, "at", 0x61, 
                0x00000002U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyTime, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Double, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.Create), 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int32, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.Create)
            );
            
            DefineLibraryMethod(module, "gm", 0x61, 
                0x00000000U, 0x80000000U, 
                new Func<System.Object, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateGmtTime), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateGmtTime)
            );
            
            DefineLibraryMethod(module, "local", 0x61, 
                0x00000000U, 0x80000000U, 
                new Func<System.Object, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateLocalTime), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateLocalTime)
            );
            
            DefineLibraryMethod(module, "mktime", 0x61, 
                0x00000000U, 0x80000000U, 
                new Func<System.Object, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateLocalTime), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateLocalTime)
            );
            
            DefineLibraryMethod(module, "now", 0x61, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.Now)
            );
            
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "times", 0xba0061, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyStruct>(IronRuby.Builtins.RubyTimeOps.Times)
            );
            
            #endif
            DefineLibraryMethod(module, "utc", 0x61, 
                0x00000000U, 0x80000000U, 
                new Func<System.Object, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, System.Int32, IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateGmtTime), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.RubyTime>(IronRuby.Builtins.RubyTimeOps.CreateGmtTime)
            );
            
        }
        
        private static void LoadTrueClass_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "&", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Boolean, System.Object, System.Boolean>(IronRuby.Builtins.TrueClass.And), 
                new Func<System.Boolean, System.Boolean, System.Boolean>(IronRuby.Builtins.TrueClass.And)
            );
            
            DefineLibraryMethod(module, "^", 0x51, 
                0x00000000U, 0x00000000U, 
                new Func<System.Boolean, System.Object, System.Boolean>(IronRuby.Builtins.TrueClass.Xor), 
                new Func<System.Boolean, System.Boolean, System.Boolean>(IronRuby.Builtins.TrueClass.Xor)
            );
            
            DefineLibraryMethod(module, "|", 0x51, 
                0x00000000U, 
                new Func<System.Boolean, System.Object, System.Boolean>(IronRuby.Builtins.TrueClass.Or)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<System.Boolean, IronRuby.Builtins.MutableString>(IronRuby.Builtins.TrueClass.ToString)
            );
            
        }
        
        private static void LoadUnboundMethod_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "==", 0x51, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.UnboundMethod, System.Boolean>(IronRuby.Builtins.UnboundMethod.Equal), 
                new Func<IronRuby.Builtins.UnboundMethod, System.Object, System.Boolean>(IronRuby.Builtins.UnboundMethod.Equal)
            );
            
            DefineLibraryMethod(module, "arity", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.UnboundMethod, System.Int32>(IronRuby.Builtins.UnboundMethod.GetArity)
            );
            
            DefineLibraryMethod(module, "bind", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.UnboundMethod, System.Object, IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.UnboundMethod.Bind)
            );
            
            DefineLibraryMethod(module, "clone", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.UnboundMethod.Clone)
            );
            
            DefineLibraryMethod(module, "clr_members", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.UnboundMethod.GetClrMembers)
            );
            
            DefineLibraryMethod(module, "of", 0x51, 
                0x80000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.UnboundMethod, System.Object[], IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.UnboundMethod.BingGenericParameters)
            );
            
            DefineLibraryMethod(module, "overload", 0x51, 
                0x80000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.UnboundMethod, System.Object[], IronRuby.Builtins.UnboundMethod>(IronRuby.Builtins.UnboundMethod.SelectOverload)
            );
            
            DefineLibraryMethod(module, "overloads", 0x51, 
                0x80000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyMethod, System.Object[], IronRuby.Builtins.RubyMethod>(IronRuby.Builtins.UnboundMethod.SelectOverload_old)
            );
            
            DefineLibraryMethod(module, "parameters", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.UnboundMethod.GetParameters)
            );
            
            DefineLibraryMethod(module, "source_location", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.RubyArray>(IronRuby.Builtins.UnboundMethod.GetSourceLocation)
            );
            
            DefineLibraryMethod(module, "to_s", 0x51, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.UnboundMethod, IronRuby.Builtins.MutableString>(IronRuby.Builtins.UnboundMethod.ToS)
            );
            
        }
        
        #if !SILVERLIGHT
        public static System.Exception/*!*/ ExceptionFactory__Encoding__ConverterNotFoundError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.ConverterNotFoundError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        #endif
        #if !SILVERLIGHT
        public static System.Exception/*!*/ ExceptionFactory__Encoding__CompatibilityError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.EncodingCompatibilityError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        #endif
        public static System.Exception/*!*/ ExceptionFactory__EncodingError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.EncodingError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__EOFError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.EOFError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__FloatDomainError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.FloatDomainError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Interrupt(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.Interrupt(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        #if !SILVERLIGHT
        public static System.Exception/*!*/ ExceptionFactory__Encoding__InvalidByteSequenceError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.InvalidByteSequenceError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        #endif
        public static System.Exception/*!*/ ExceptionFactory__LoadError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.LoadError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__LocalJumpError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.LocalJumpError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__NoMemoryError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.NoMemoryError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__NotImplementedError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.NotImplementedError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__RegexpError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.RegexpError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__RuntimeError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.RuntimeError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__ScriptError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.ScriptError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__SignalException(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.SignalException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__SyntaxError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.SyntaxError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__SystemExit(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.SystemExit(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__SystemStackError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.SystemStackError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__ThreadError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.ThreadError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        #if !SILVERLIGHT
        public static System.Exception/*!*/ ExceptionFactory__Encoding__UndefinedConversionError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.Builtins.UndefinedConversionError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        #endif
        public static System.Exception/*!*/ ExceptionFactory__ArgumentError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.ArgumentException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__RangeError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.ArgumentOutOfRangeException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__ZeroDivisionError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.DivideByZeroException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Exception(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.Exception(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__IndexError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.IndexOutOfRangeException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__TypeError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.InvalidOperationException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__IOError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.IO.IOException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__NameError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.MemberAccessException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__NoMethodError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.MissingMethodException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__SystemCallError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.Runtime.InteropServices.ExternalException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__SecurityError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.Security.SecurityException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__StandardError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new System.SystemException(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
    }
}

namespace IronRuby.StandardLibrary.Threading {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class ThreadingLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(System.Object));
            
            
            DefineGlobalClass("ConditionVariable", typeof(IronRuby.StandardLibrary.Threading.RubyConditionVariable), 0x00000008, classRef0, LoadConditionVariable_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("Mutex", typeof(IronRuby.StandardLibrary.Threading.RubyMutex), 0x00000008, classRef0, LoadMutex_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def1 = DefineGlobalClass("Queue", typeof(IronRuby.StandardLibrary.Threading.RubyQueue), 0x00000008, classRef0, LoadQueue_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            ExtendClass(typeof(System.Threading.Thread), 0x00000000, classRef0, null, LoadSystem__Threading__Thread_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            DefineGlobalClass("SizedQueue", typeof(IronRuby.StandardLibrary.Threading.SizedQueue), 0x00000008, def1, LoadSizedQueue_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
        }
        
        private static void LoadConditionVariable_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "broadcast", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyConditionVariable, IronRuby.StandardLibrary.Threading.RubyConditionVariable>(IronRuby.StandardLibrary.Threading.RubyConditionVariable.Broadcast)
            );
            
            DefineLibraryMethod(module, "signal", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyConditionVariable, IronRuby.StandardLibrary.Threading.RubyConditionVariable>(IronRuby.StandardLibrary.Threading.RubyConditionVariable.Signal)
            );
            
            DefineLibraryMethod(module, "wait", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyConditionVariable, IronRuby.StandardLibrary.Threading.RubyMutex, IronRuby.StandardLibrary.Threading.RubyConditionVariable>(IronRuby.StandardLibrary.Threading.RubyConditionVariable.Wait)
            );
            
        }
        
        private static void LoadMutex_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "exclusive_unlock", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.Threading.RubyMutex, System.Boolean>(IronRuby.StandardLibrary.Threading.RubyMutex.ExclusiveUnlock)
            );
            
            DefineLibraryMethod(module, "lock", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyMutex, IronRuby.StandardLibrary.Threading.RubyMutex>(IronRuby.StandardLibrary.Threading.RubyMutex.Lock)
            );
            
            DefineLibraryMethod(module, "locked?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyMutex, System.Boolean>(IronRuby.StandardLibrary.Threading.RubyMutex.IsLocked)
            );
            
            DefineLibraryMethod(module, "synchronize", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.Threading.RubyMutex, System.Object>(IronRuby.StandardLibrary.Threading.RubyMutex.Synchronize)
            );
            
            DefineLibraryMethod(module, "try_lock", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyMutex, System.Boolean>(IronRuby.StandardLibrary.Threading.RubyMutex.TryLock)
            );
            
            DefineLibraryMethod(module, "unlock", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyMutex, IronRuby.StandardLibrary.Threading.RubyMutex>(IronRuby.StandardLibrary.Threading.RubyMutex.Unlock)
            );
            
        }
        
        private static void LoadQueue_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<<", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Object, IronRuby.StandardLibrary.Threading.RubyQueue>(IronRuby.StandardLibrary.Threading.RubyQueue.Enqueue)
            );
            
            DefineLibraryMethod(module, "clear", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, IronRuby.StandardLibrary.Threading.RubyQueue>(IronRuby.StandardLibrary.Threading.RubyQueue.Clear)
            );
            
            DefineLibraryMethod(module, "deq", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Boolean, System.Object>(IronRuby.StandardLibrary.Threading.RubyQueue.Dequeue)
            );
            
            DefineLibraryMethod(module, "empty?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Boolean>(IronRuby.StandardLibrary.Threading.RubyQueue.IsEmpty)
            );
            
            DefineLibraryMethod(module, "enq", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Object, IronRuby.StandardLibrary.Threading.RubyQueue>(IronRuby.StandardLibrary.Threading.RubyQueue.Enqueue)
            );
            
            DefineLibraryMethod(module, "length", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Int32>(IronRuby.StandardLibrary.Threading.RubyQueue.GetCount)
            );
            
            DefineLibraryMethod(module, "num_waiting", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Int32>(IronRuby.StandardLibrary.Threading.RubyQueue.GetNumberOfWaitingThreads)
            );
            
            DefineLibraryMethod(module, "pop", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Boolean, System.Object>(IronRuby.StandardLibrary.Threading.RubyQueue.Dequeue)
            );
            
            DefineLibraryMethod(module, "push", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Object, IronRuby.StandardLibrary.Threading.RubyQueue>(IronRuby.StandardLibrary.Threading.RubyQueue.Enqueue)
            );
            
            DefineLibraryMethod(module, "shift", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Boolean, System.Object>(IronRuby.StandardLibrary.Threading.RubyQueue.Dequeue)
            );
            
            DefineLibraryMethod(module, "size", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.RubyQueue, System.Int32>(IronRuby.StandardLibrary.Threading.RubyQueue.GetCount)
            );
            
        }
        
        private static void LoadSizedQueue_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<<", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Object, IronRuby.StandardLibrary.Threading.SizedQueue>(IronRuby.StandardLibrary.Threading.SizedQueue.Enqueue)
            );
            
            DefineLibraryMethod(module, "deq", 0x11, 
                0x80000000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Object[], System.Object>(IronRuby.StandardLibrary.Threading.SizedQueue.Dequeue)
            );
            
            DefineLibraryMethod(module, "enq", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Object, IronRuby.StandardLibrary.Threading.SizedQueue>(IronRuby.StandardLibrary.Threading.SizedQueue.Enqueue)
            );
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00010000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Int32, IronRuby.StandardLibrary.Threading.SizedQueue>(IronRuby.StandardLibrary.Threading.SizedQueue.Reinitialize)
            );
            
            DefineLibraryMethod(module, "max", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Int32>(IronRuby.StandardLibrary.Threading.SizedQueue.GetLimit)
            );
            
            DefineLibraryMethod(module, "max=", 0x11, 
                0x00010000U, 
                new Action<IronRuby.StandardLibrary.Threading.SizedQueue, System.Int32>(IronRuby.StandardLibrary.Threading.SizedQueue.SetLimit)
            );
            
            DefineLibraryMethod(module, "pop", 0x11, 
                0x80000000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Object[], System.Object>(IronRuby.StandardLibrary.Threading.SizedQueue.Dequeue)
            );
            
            DefineLibraryMethod(module, "push", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Object, IronRuby.StandardLibrary.Threading.SizedQueue>(IronRuby.StandardLibrary.Threading.SizedQueue.Enqueue)
            );
            
            DefineLibraryMethod(module, "shift", 0x11, 
                0x80000000U, 
                new Func<IronRuby.StandardLibrary.Threading.SizedQueue, System.Object[], System.Object>(IronRuby.StandardLibrary.Threading.SizedQueue.Dequeue)
            );
            
        }
        
        private static void LoadSystem__Threading__Thread_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "exclusive", 0x21, 
                0x00000002U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, System.Object, System.Object>(IronRuby.StandardLibrary.Threading.ThreadOps.Exclusive)
            );
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.Sockets {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class SocketsLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(IronRuby.Builtins.RubyIO));
            IronRuby.Builtins.RubyClass classRef1 = GetClass(typeof(System.SystemException));
            
            
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def3 = DefineGlobalClass("BasicSocket", typeof(IronRuby.StandardLibrary.Sockets.RubyBasicSocket), 0x00000008, classRef0, LoadBasicSocket_Instance, LoadBasicSocket_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT && !SILVERLIGHT
            IronRuby.Builtins.RubyModule def2 = DefineModule("Socket::Constants", typeof(IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants), 0x00000008, null, null, LoadSocket__Constants_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT
            DefineGlobalClass("SocketError", typeof(System.Net.Sockets.SocketException), 0x00000000, classRef1, LoadSocketError_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(IronRuby.StandardLibrary.Sockets.SocketErrorOps.Create)
            );
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def5 = DefineGlobalClass("IPSocket", typeof(IronRuby.StandardLibrary.Sockets.IPSocket), 0x00000008, def3, LoadIPSocket_Instance, LoadIPSocket_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def1 = DefineGlobalClass("Socket", typeof(IronRuby.StandardLibrary.Sockets.RubySocket), 0x00000008, def3, LoadSocket_Instance, LoadSocket_Class, LoadSocket_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, System.Object, System.Int32, System.Int32, IronRuby.StandardLibrary.Sockets.RubySocket>(IronRuby.StandardLibrary.Sockets.RubySocket.CreateSocket)
            );
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def4 = DefineGlobalClass("TCPSocket", typeof(IronRuby.StandardLibrary.Sockets.TCPSocket), 0x00000008, def5, LoadTCPSocket_Instance, LoadTCPSocket_Class, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object, System.Int32, IronRuby.StandardLibrary.Sockets.TCPSocket>(IronRuby.StandardLibrary.Sockets.TCPSocket.CreateTCPSocket), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object, IronRuby.Builtins.MutableString, System.Object, IronRuby.StandardLibrary.Sockets.TCPSocket>(IronRuby.StandardLibrary.Sockets.TCPSocket.CreateTCPSocket)
            );
            #endif
            #if !SILVERLIGHT
            DefineGlobalClass("UDPSocket", typeof(IronRuby.StandardLibrary.Sockets.UDPSocket), 0x00000008, def5, LoadUDPSocket_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.StandardLibrary.Sockets.UDPSocket>(IronRuby.StandardLibrary.Sockets.UDPSocket.CreateUDPSocket)
            );
            #endif
            #if !SILVERLIGHT
            DefineGlobalClass("TCPServer", typeof(IronRuby.StandardLibrary.Sockets.TCPServer), 0x00000008, def4, LoadTCPServer_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object, IronRuby.StandardLibrary.Sockets.TCPServer>(IronRuby.StandardLibrary.Sockets.TCPServer.CreateTCPServer)
            );
            #endif
            #if !SILVERLIGHT && !SILVERLIGHT
            SetConstant(def1, "Constants", def2);
            #endif
        }
        
        #if !SILVERLIGHT
        private static void LoadBasicSocket_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "close_read", 0x11, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubyBasicSocket>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.CloseRead)
            );
            
            DefineLibraryMethod(module, "close_write", 0x11, 
                0x00000000U, 
                new Action<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubyBasicSocket>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.CloseWrite)
            );
            
            DefineLibraryMethod(module, "do_not_reverse_lookup", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Boolean>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.GetDoNotReverseLookup)
            );
            
            DefineLibraryMethod(module, "do_not_reverse_lookup=", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Boolean>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.SetDoNotReverseLookup)
            );
            
            DefineLibraryMethod(module, "getpeername", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Sockets.RubyBasicSocket, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.GetPeerName)
            );
            
            DefineLibraryMethod(module, "getsockname", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Sockets.RubyBasicSocket, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.GetSocketName)
            );
            
            DefineLibraryMethod(module, "getsockopt", 0x11, 
                0x000c0000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.GetSocketOption)
            );
            
            DefineLibraryMethod(module, "recv", 0x11, 
                0x00020000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Int32, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.Receive)
            );
            
            DefineLibraryMethod(module, "recv_nonblock", 0x11, 
                0x00020000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Int32, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.ReceiveNonBlocking)
            );
            
            DefineLibraryMethod(module, "send", 0x11, 
                0x00020004U, 0x000a0014U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, IronRuby.Builtins.MutableString, System.Object, System.Int32>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.Send), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, IronRuby.Builtins.MutableString, System.Object, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.Send)
            );
            
            DefineLibraryMethod(module, "setsockopt", 0x11, 
                0x000c0000U, 0x000c0000U, 0x000e0010U, 
                new Action<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Int32, System.Int32, System.Int32>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.SetSocketOption), 
                new Action<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Int32, System.Int32, System.Boolean>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.SetSocketOption), 
                new Action<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.SetSocketOption)
            );
            
            DefineLibraryMethod(module, "shutdown", 0x11, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, System.Int32, System.Int32>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.Shutdown)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadBasicSocket_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "do_not_reverse_lookup", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Boolean>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.GetDoNotReverseLookup)
            );
            
            DefineLibraryMethod(module, "do_not_reverse_lookup=", 0x21, 
                0x00000000U, 
                new Action<IronRuby.Builtins.RubyClass, System.Boolean>(IronRuby.StandardLibrary.Sockets.RubyBasicSocket.SetDoNotReverseLookup)
            );
            
            DefineRuleGenerator(module, "for_fd", 0x21, IronRuby.StandardLibrary.Sockets.RubyBasicSocket.ForFileDescriptor());
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadIPSocket_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "addr", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.IPSocket, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.IPSocket.GetLocalAddress)
            );
            
            DefineLibraryMethod(module, "peeraddr", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.IPSocket, System.Object>(IronRuby.StandardLibrary.Sockets.IPSocket.GetPeerAddress)
            );
            
            DefineLibraryMethod(module, "recvfrom", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.IPSocket, System.Int32, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.IPSocket.ReceiveFrom)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadIPSocket_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "getaddress", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.IPSocket.GetAddress)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadSocket_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            LoadSocket__Constants_Constants(module);
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadSocket_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "accept", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubySocket, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.Accept)
            );
            
            DefineLibraryMethod(module, "accept_nonblock", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubySocket, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.AcceptNonBlocking)
            );
            
            DefineLibraryMethod(module, "bind", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubySocket, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.Sockets.RubySocket.Bind)
            );
            
            DefineLibraryMethod(module, "connect", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubySocket, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.Sockets.RubySocket.Connect)
            );
            
            DefineLibraryMethod(module, "connect_nonblock", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubySocket, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.Sockets.RubySocket.ConnectNonBlocking)
            );
            
            DefineLibraryMethod(module, "listen", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubySocket, System.Int32, System.Int32>(IronRuby.StandardLibrary.Sockets.RubySocket.Listen)
            );
            
            DefineLibraryMethod(module, "recvfrom", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubySocket, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.ReceiveFrom), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubySocket, System.Int32, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.ReceiveFrom)
            );
            
            DefineLibraryMethod(module, "sysaccept", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.RubySocket, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.SysAccept)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadSocket_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "getaddrinfo", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.GetAddressInfo)
            );
            
            DefineLibraryMethod(module, "gethostbyaddr", 0x21, 
                0x00040008U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.GetHostByAddress)
            );
            
            DefineLibraryMethod(module, "gethostbyname", 0x21, 
                0x00000000U, 0x00000002U, 0x00010000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.GetHostByName), 
                new Func<IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.GetHostByName), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.GetHostByName)
            );
            
            DefineLibraryMethod(module, "gethostname", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubySocket.GetHostname)
            );
            
            DefineLibraryMethod(module, "getnameinfo", 0x21, 
                0x00000008U, 0x00010002U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyArray, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.GetNameInfo), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.GetNameInfo)
            );
            
            DefineLibraryMethod(module, "getservbyname", 0x21, 
                0x00030002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.Sockets.RubySocket.GetServiceByName)
            );
            
            DefineLibraryMethod(module, "pack_sockaddr_in", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, System.Object, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubySocket.PackInetSockAddr)
            );
            
            DefineLibraryMethod(module, "pair", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.CreateSocketPair)
            );
            
            DefineLibraryMethod(module, "sockaddr_in", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Builtins.RubyClass, System.Object, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Sockets.RubySocket.PackInetSockAddr)
            );
            
            DefineLibraryMethod(module, "socketpair", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.CreateSocketPair)
            );
            
            DefineLibraryMethod(module, "unpack_sockaddr_in", 0x21, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.RubySocket.UnPackInetSockAddr)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT && !SILVERLIGHT
        private static void LoadSocket__Constants_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetConstant(module, "AF_APPLETALK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_APPLETALK);
            SetConstant(module, "AF_ATM", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_ATM);
            SetConstant(module, "AF_CCITT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_CCITT);
            SetConstant(module, "AF_CHAOS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_CHAOS);
            SetConstant(module, "AF_DATAKIT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_DATAKIT);
            SetConstant(module, "AF_DLI", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_DLI);
            SetConstant(module, "AF_ECMA", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_ECMA);
            SetConstant(module, "AF_HYLINK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_HYLINK);
            SetConstant(module, "AF_IMPLINK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_IMPLINK);
            SetConstant(module, "AF_INET", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_INET);
            SetConstant(module, "AF_INET6", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_INET6);
            SetConstant(module, "AF_IPX", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_IPX);
            SetConstant(module, "AF_ISO", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_ISO);
            SetConstant(module, "AF_LAT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_LAT);
            SetConstant(module, "AF_MAX", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_MAX);
            SetConstant(module, "AF_NETBIOS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_NETBIOS);
            SetConstant(module, "AF_NS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_NS);
            SetConstant(module, "AF_OSI", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_OSI);
            SetConstant(module, "AF_PUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_PUP);
            SetConstant(module, "AF_SNA", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_SNA);
            SetConstant(module, "AF_UNIX", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_UNIX);
            SetConstant(module, "AF_UNSPEC", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AF_UNSPEC);
            SetConstant(module, "AI_CANONNAME", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AI_CANONNAME);
            SetConstant(module, "AI_NUMERICHOST", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AI_NUMERICHOST);
            SetConstant(module, "AI_PASSIVE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.AI_PASSIVE);
            SetConstant(module, "EAI_AGAIN", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_AGAIN);
            SetConstant(module, "EAI_BADFLAGS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_BADFLAGS);
            SetConstant(module, "EAI_FAIL", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_FAIL);
            SetConstant(module, "EAI_FAMILY", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_FAMILY);
            SetConstant(module, "EAI_MEMORY", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_MEMORY);
            SetConstant(module, "EAI_NODATA", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_NODATA);
            SetConstant(module, "EAI_NONAME", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_NONAME);
            SetConstant(module, "EAI_SERVICE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_SERVICE);
            SetConstant(module, "EAI_SOCKTYPE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.EAI_SOCKTYPE);
            SetConstant(module, "INADDR_ALLHOSTS_GROUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INADDR_ALLHOSTS_GROUP);
            SetConstant(module, "INADDR_ANY", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INADDR_ANY);
            SetConstant(module, "INADDR_BROADCAST", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INADDR_BROADCAST);
            SetConstant(module, "INADDR_LOOPBACK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INADDR_LOOPBACK);
            SetConstant(module, "INADDR_MAX_LOCAL_GROUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INADDR_MAX_LOCAL_GROUP);
            SetConstant(module, "INADDR_NONE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INADDR_NONE);
            SetConstant(module, "INADDR_UNSPEC_GROUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INADDR_UNSPEC_GROUP);
            SetConstant(module, "INET_ADDRSTRLEN", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INET_ADDRSTRLEN);
            SetConstant(module, "INET6_ADDRSTRLEN", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.INET6_ADDRSTRLEN);
            SetConstant(module, "IP_ADD_MEMBERSHIP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_ADD_MEMBERSHIP);
            SetConstant(module, "IP_ADD_SOURCE_MEMBERSHIP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_ADD_SOURCE_MEMBERSHIP);
            SetConstant(module, "IP_BLOCK_SOURCE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_BLOCK_SOURCE);
            SetConstant(module, "IP_DEFAULT_MULTICAST_LOOP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_DEFAULT_MULTICAST_LOOP);
            SetConstant(module, "IP_DEFAULT_MULTICAST_TTL", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_DEFAULT_MULTICAST_TTL);
            SetConstant(module, "IP_DROP_MEMBERSHIP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_DROP_MEMBERSHIP);
            SetConstant(module, "IP_DROP_SOURCE_MEMBERSHIP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_DROP_SOURCE_MEMBERSHIP);
            SetConstant(module, "IP_HDRINCL", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_HDRINCL);
            SetConstant(module, "IP_MAX_MEMBERSHIPS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_MAX_MEMBERSHIPS);
            SetConstant(module, "IP_MULTICAST_IF", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_MULTICAST_IF);
            SetConstant(module, "IP_MULTICAST_LOOP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_MULTICAST_LOOP);
            SetConstant(module, "IP_MULTICAST_TTL", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_MULTICAST_TTL);
            SetConstant(module, "IP_OPTIONS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_OPTIONS);
            SetConstant(module, "IP_PKTINFO", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_PKTINFO);
            SetConstant(module, "IP_TOS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_TOS);
            SetConstant(module, "IP_TTL", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_TTL);
            SetConstant(module, "IP_UNBLOCK_SOURCE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IP_UNBLOCK_SOURCE);
            SetConstant(module, "IPPORT_RESERVED", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPORT_RESERVED);
            SetConstant(module, "IPPORT_USERRESERVED", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPORT_USERRESERVED);
            SetConstant(module, "IPPROTO_AH", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_AH);
            SetConstant(module, "IPPROTO_DSTOPTS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_DSTOPTS);
            SetConstant(module, "IPPROTO_ESP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_ESP);
            SetConstant(module, "IPPROTO_FRAGMENT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_FRAGMENT);
            SetConstant(module, "IPPROTO_GGP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_GGP);
            SetConstant(module, "IPPROTO_HOPOPTS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_HOPOPTS);
            SetConstant(module, "IPPROTO_ICMP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_ICMP);
            SetConstant(module, "IPPROTO_ICMPV6", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_ICMPV6);
            SetConstant(module, "IPPROTO_IDP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_IDP);
            SetConstant(module, "IPPROTO_IGMP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_IGMP);
            SetConstant(module, "IPPROTO_IP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_IP);
            SetConstant(module, "IPPROTO_IPV6", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_IPV6);
            SetConstant(module, "IPPROTO_MAX", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_MAX);
            SetConstant(module, "IPPROTO_ND", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_ND);
            SetConstant(module, "IPPROTO_NONE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_NONE);
            SetConstant(module, "IPPROTO_PUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_PUP);
            SetConstant(module, "IPPROTO_RAW", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_RAW);
            SetConstant(module, "IPPROTO_ROUTING", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_ROUTING);
            SetConstant(module, "IPPROTO_TCP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_TCP);
            SetConstant(module, "IPPROTO_UDP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPPROTO_UDP);
            SetConstant(module, "IPV6_JOIN_GROUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPV6_JOIN_GROUP);
            SetConstant(module, "IPV6_LEAVE_GROUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPV6_LEAVE_GROUP);
            SetConstant(module, "IPV6_MULTICAST_HOPS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPV6_MULTICAST_HOPS);
            SetConstant(module, "IPV6_MULTICAST_IF", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPV6_MULTICAST_IF);
            SetConstant(module, "IPV6_MULTICAST_LOOP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPV6_MULTICAST_LOOP);
            SetConstant(module, "IPV6_PKTINFO", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPV6_PKTINFO);
            SetConstant(module, "IPV6_UNICAST_HOPS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.IPV6_UNICAST_HOPS);
            SetConstant(module, "MSG_DONTROUTE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.MSG_DONTROUTE);
            SetConstant(module, "MSG_OOB", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.MSG_OOB);
            SetConstant(module, "MSG_PEEK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.MSG_PEEK);
            SetConstant(module, "NI_DGRAM", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.NI_DGRAM);
            SetConstant(module, "NI_MAXHOST", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.NI_MAXHOST);
            SetConstant(module, "NI_MAXSERV", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.NI_MAXSERV);
            SetConstant(module, "NI_NAMEREQD", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.NI_NAMEREQD);
            SetConstant(module, "NI_NOFQDN", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.NI_NOFQDN);
            SetConstant(module, "NI_NUMERICHOST", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.NI_NUMERICHOST);
            SetConstant(module, "NI_NUMERICSERV", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.NI_NUMERICSERV);
            SetConstant(module, "PF_APPLETALK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_APPLETALK);
            SetConstant(module, "PF_ATM", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_ATM);
            SetConstant(module, "PF_CCITT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_CCITT);
            SetConstant(module, "PF_CHAOS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_CHAOS);
            SetConstant(module, "PF_DATAKIT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_DATAKIT);
            SetConstant(module, "PF_DLI", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_DLI);
            SetConstant(module, "PF_ECMA", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_ECMA);
            SetConstant(module, "PF_HYLINK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_HYLINK);
            SetConstant(module, "PF_IMPLINK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_IMPLINK);
            SetConstant(module, "PF_INET", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_INET);
            SetConstant(module, "PF_INET6", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_INET6);
            SetConstant(module, "PF_IPX", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_IPX);
            SetConstant(module, "PF_ISO", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_ISO);
            SetConstant(module, "PF_LAT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_LAT);
            SetConstant(module, "PF_MAX", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_MAX);
            SetConstant(module, "PF_NS", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_NS);
            SetConstant(module, "PF_OSI", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_OSI);
            SetConstant(module, "PF_PUP", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_PUP);
            SetConstant(module, "PF_SNA", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_SNA);
            SetConstant(module, "PF_UNIX", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_UNIX);
            SetConstant(module, "PF_UNSPEC", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.PF_UNSPEC);
            SetConstant(module, "SHUT_RD", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SHUT_RD);
            SetConstant(module, "SHUT_RDWR", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SHUT_RDWR);
            SetConstant(module, "SHUT_WR", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SHUT_WR);
            SetConstant(module, "SO_ACCEPTCONN", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_ACCEPTCONN);
            SetConstant(module, "SO_BROADCAST", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_BROADCAST);
            SetConstant(module, "SO_DEBUG", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_DEBUG);
            SetConstant(module, "SO_DONTROUTE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_DONTROUTE);
            SetConstant(module, "SO_ERROR", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_ERROR);
            SetConstant(module, "SO_KEEPALIVE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_KEEPALIVE);
            SetConstant(module, "SO_LINGER", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_LINGER);
            SetConstant(module, "SO_OOBINLINE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_OOBINLINE);
            SetConstant(module, "SO_RCVBUF", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_RCVBUF);
            SetConstant(module, "SO_RCVLOWAT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_RCVLOWAT);
            SetConstant(module, "SO_RCVTIMEO", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_RCVTIMEO);
            SetConstant(module, "SO_REUSEADDR", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_REUSEADDR);
            SetConstant(module, "SO_SNDBUF", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_SNDBUF);
            SetConstant(module, "SO_SNDLOWAT", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_SNDLOWAT);
            SetConstant(module, "SO_SNDTIMEO", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_SNDTIMEO);
            SetConstant(module, "SO_TYPE", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_TYPE);
            SetConstant(module, "SO_USELOOPBACK", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SO_USELOOPBACK);
            SetConstant(module, "SOCK_DGRAM", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SOCK_DGRAM);
            SetConstant(module, "SOCK_RAW", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SOCK_RAW);
            SetConstant(module, "SOCK_RDM", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SOCK_RDM);
            SetConstant(module, "SOCK_SEQPACKET", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SOCK_SEQPACKET);
            SetConstant(module, "SOCK_STREAM", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SOCK_STREAM);
            SetConstant(module, "SOL_SOCKET", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SOL_SOCKET);
            SetConstant(module, "SOMAXCONN", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.SOMAXCONN);
            SetConstant(module, "TCP_NODELAY", IronRuby.StandardLibrary.Sockets.RubySocket.SocketConstants.TCP_NODELAY);
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadSocketError_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            module.HideMethod("message");
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadTCPServer_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "accept", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.TCPServer, IronRuby.StandardLibrary.Sockets.TCPSocket>(IronRuby.StandardLibrary.Sockets.TCPServer.Accept)
            );
            
            DefineLibraryMethod(module, "accept_nonblock", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.TCPServer, IronRuby.StandardLibrary.Sockets.TCPSocket>(IronRuby.StandardLibrary.Sockets.TCPServer.AcceptNonBlocking)
            );
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00040000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.TCPServer, IronRuby.Builtins.MutableString, System.Object, IronRuby.StandardLibrary.Sockets.TCPServer>(IronRuby.StandardLibrary.Sockets.TCPServer.Reinitialize)
            );
            
            DefineLibraryMethod(module, "listen", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.Sockets.TCPServer, System.Int32>(IronRuby.StandardLibrary.Sockets.TCPServer.Listen)
            );
            
            DefineLibraryMethod(module, "sysaccept", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Sockets.TCPServer, System.Int32>(IronRuby.StandardLibrary.Sockets.TCPServer.SysAccept)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadTCPSocket_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00040000U, 0x00140000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.TCPServer, IronRuby.Builtins.MutableString, System.Object, System.Int32, IronRuby.StandardLibrary.Sockets.TCPServer>(IronRuby.StandardLibrary.Sockets.TCPSocket.Reinitialize), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.TCPServer, IronRuby.Builtins.MutableString, System.Object, IronRuby.Builtins.MutableString, System.Object, IronRuby.StandardLibrary.Sockets.TCPServer>(IronRuby.StandardLibrary.Sockets.TCPSocket.Reinitialize)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadTCPSocket_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "gethostbyname", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.TCPSocket.GetHostByName)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadUDPSocket_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "bind", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.UDPSocket, System.Object, System.Object, System.Int32>(IronRuby.StandardLibrary.Sockets.UDPSocket.Bind)
            );
            
            DefineLibraryMethod(module, "connect", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.UDPSocket, System.Object, System.Object, System.Int32>(IronRuby.StandardLibrary.Sockets.UDPSocket.Connect)
            );
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.UDPSocket, System.Object, IronRuby.StandardLibrary.Sockets.UDPSocket>(IronRuby.StandardLibrary.Sockets.UDPSocket.Reinitialize)
            );
            
            DefineLibraryMethod(module, "recvfrom_nonblock", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.IPSocket, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.UDPSocket.ReceiveFromNonBlocking), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.IPSocket, System.Int32, System.Object, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Sockets.UDPSocket.ReceiveFromNonBlocking)
            );
            
            DefineLibraryMethod(module, "send", 0x11, 
                0x00040008U, 0x00020004U, 0x000a0014U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, IronRuby.Builtins.MutableString, System.Object, System.Object, System.Object, System.Int32>(IronRuby.StandardLibrary.Sockets.UDPSocket.Send), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, IronRuby.Builtins.MutableString, System.Object, System.Int32>(IronRuby.StandardLibrary.Sockets.UDPSocket.Send), 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.Sockets.RubyBasicSocket, IronRuby.Builtins.MutableString, System.Object, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.Sockets.UDPSocket.Send)
            );
            
        }
        #endif
        
    }
}

namespace IronRuby.StandardLibrary.OpenSsl {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class OpenSslLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(System.Object));
            IronRuby.Builtins.RubyClass classRef1 = GetClass(typeof(System.SystemException));
            IronRuby.Builtins.RubyClass classRef2 = GetClass(typeof(System.Runtime.InteropServices.ExternalException));
            
            
            IronRuby.Builtins.RubyModule def1 = DefineGlobalModule("OpenSSL", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl), 0x00000008, null, null, LoadOpenSSL_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def2 = DefineClass("OpenSSL::BN", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.BN), 0x00000008, classRef0, null, LoadOpenSSL__BN_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def3 = DefineModule("OpenSSL::Digest", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def4 = DefineClass("OpenSSL::Digest::Digest", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest), 0x00000008, classRef0, LoadOpenSSL__Digest__Digest_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest.CreateDigest)
            );
            IronRuby.Builtins.RubyClass def5 = DefineClass("OpenSSL::HMAC", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.HMAC), 0x00000008, classRef0, null, LoadOpenSSL__HMAC_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def6 = DefineClass("OpenSSL::OpenSSLError", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.OpenSSLError), 0x00000008, classRef1, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def7 = DefineModule("OpenSSL::PKey", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.PKey), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def8 = DefineClass("OpenSSL::PKey::RSA", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.PKey.RSA), 0x00000008, classRef0, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def9 = DefineModule("OpenSSL::Random", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.RandomModule), 0x00000008, null, LoadOpenSSL__Random_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def10 = DefineModule("OpenSSL::SSL", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.SSL), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def12 = DefineModule("OpenSSL::X509", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def13 = DefineClass("OpenSSL::X509::Certificate", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate), 0x00000008, classRef0, LoadOpenSSL__X509__Certificate_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.CreateCertificate), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.CreateCertificate)
            );
            IronRuby.Builtins.RubyClass def15 = DefineClass("OpenSSL::X509::CertificateError", typeof(System.Security.Cryptography.CryptographicException), 0x00000000, classRef2, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Security.Cryptography.CryptographicException>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.CryptographicExceptionOps.Create)
            );
            IronRuby.Builtins.RubyClass def14 = DefineClass("OpenSSL::X509::Name", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Name), 0x00000008, classRef0, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def11 = DefineClass("OpenSSL::SSL::SSLError", typeof(IronRuby.StandardLibrary.OpenSsl.OpenSsl.SSL.SSLError), 0x00000008, def6, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            SetConstant(def1, "BN", def2);
            SetConstant(def1, "Digest", def3);
            SetConstant(def3, "Digest", def4);
            SetConstant(def1, "HMAC", def5);
            SetConstant(def1, "OpenSSLError", def6);
            SetConstant(def1, "PKey", def7);
            SetConstant(def7, "RSA", def8);
            SetConstant(def1, "Random", def9);
            SetConstant(def1, "SSL", def10);
            SetConstant(def1, "X509", def12);
            SetConstant(def12, "Certificate", def13);
            SetConstant(def12, "CertificateError", def15);
            SetConstant(def12, "Name", def14);
            SetConstant(def10, "SSLError", def11);
        }
        
        private static void LoadOpenSSL_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetConstant(module, "OPENSSL_VERSION", IronRuby.StandardLibrary.OpenSsl.OpenSsl.OPENSSL_VERSION);
            SetConstant(module, "OPENSSL_VERSION_NUMBER", IronRuby.StandardLibrary.OpenSsl.OpenSsl.OPENSSL_VERSION_NUMBER);
            SetConstant(module, "VERSION", IronRuby.StandardLibrary.OpenSsl.OpenSsl.VERSION);
            
        }
        
        private static void LoadOpenSSL__BN_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "rand", 0x21, 
                0x00030000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32, System.Int32, System.Boolean, Microsoft.Scripting.Math.BigInteger>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.BN.Rand)
            );
            
        }
        
        private static void LoadOpenSSL__Digest__Digest_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "digest", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest.BlankDigest)
            );
            
            DefineLibraryMethod(module, "digest_size", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, System.Int32>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest.Seed)
            );
            
            DefineLibraryMethod(module, "hexdigest", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest.BlankHexDigest)
            );
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest.Initialize)
            );
            
            DefineLibraryMethod(module, "name", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest.Name)
            );
            
            DefineLibraryMethod(module, "reset", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest.Reset)
            );
            
        }
        
        private static void LoadOpenSSL__HMAC_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "digest", 0x21, 
                0x0000000eU, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.HMAC.Digest)
            );
            
            DefineLibraryMethod(module, "hexdigest", 0x21, 
                0x0000000eU, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.StandardLibrary.OpenSsl.OpenSsl.DigestFactory.Digest, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.HMAC.HexDigest)
            );
            
        }
        
        private static void LoadOpenSSL__Random_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "pseudo_bytes", 0x21, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.RandomModule.RandomBytes)
            );
            
            DefineLibraryMethod(module, "random_bytes", 0x21, 
                0x00010000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.RandomModule.RandomBytes)
            );
            
            DefineLibraryMethod(module, "seed", 0x21, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.RandomModule.Seed)
            );
            
        }
        
        private static void LoadOpenSSL__X509__Certificate_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.Initialize)
            );
            
            DefineLibraryMethod(module, "inspect", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.ToString)
            );
            
            DefineLibraryMethod(module, "issuer", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.Issuer)
            );
            
            DefineLibraryMethod(module, "public_key", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.PublicKey)
            );
            
            DefineLibraryMethod(module, "serial", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, System.Int32>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.Serial)
            );
            
            DefineLibraryMethod(module, "subject", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.Subject)
            );
            
            DefineLibraryMethod(module, "to_s", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.ToString)
            );
            
            DefineLibraryMethod(module, "version", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate, System.Int32>(IronRuby.StandardLibrary.OpenSsl.OpenSsl.X509.Certificate.Version)
            );
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.Digest {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class DigestLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(System.Object));
            
            
            IronRuby.Builtins.RubyModule def1 = DefineGlobalModule("Digest", typeof(IronRuby.StandardLibrary.Digest.Digest), 0x00000008, null, LoadDigest_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def4 = DefineModule("Digest::Instance", typeof(IronRuby.StandardLibrary.Digest.Digest.Instance), 0x00000008, LoadDigest__Instance_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def3 = DefineClass("Digest::Class", typeof(IronRuby.StandardLibrary.Digest.Digest.Class), 0x00000008, classRef0, null, LoadDigest__Class_Class, null, new IronRuby.Builtins.RubyModule[] {def4});
            IronRuby.Builtins.RubyClass def2 = DefineClass("Digest::Base", typeof(IronRuby.StandardLibrary.Digest.Digest.Base), 0x00000008, def3, LoadDigest__Base_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def5 = DefineClass("Digest::MD5", typeof(IronRuby.StandardLibrary.Digest.Digest.MD5), 0x00000008, def2, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def6 = DefineClass("Digest::SHA1", typeof(IronRuby.StandardLibrary.Digest.Digest.SHA1), 0x00000008, def2, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def7 = DefineClass("Digest::SHA256", typeof(IronRuby.StandardLibrary.Digest.Digest.SHA256), 0x00000008, def2, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def8 = DefineClass("Digest::SHA384", typeof(IronRuby.StandardLibrary.Digest.Digest.SHA384), 0x00000008, def2, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def9 = DefineClass("Digest::SHA512", typeof(IronRuby.StandardLibrary.Digest.Digest.SHA512), 0x00000008, def2, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            SetConstant(def1, "Instance", def4);
            SetConstant(def1, "Class", def3);
            SetConstant(def1, "Base", def2);
            #if !SILVERLIGHT
            SetConstant(def1, "MD5", def5);
            #endif
            #if !SILVERLIGHT
            SetConstant(def1, "SHA1", def6);
            #endif
            #if !SILVERLIGHT
            SetConstant(def1, "SHA256", def7);
            #endif
            #if !SILVERLIGHT
            SetConstant(def1, "SHA384", def8);
            #endif
            #if !SILVERLIGHT
            SetConstant(def1, "SHA512", def9);
            #endif
        }
        
        private static void LoadDigest_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "const_missing", 0x21, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyModule, System.String, System.Object>(IronRuby.StandardLibrary.Digest.Digest.ConstantMissing)
            );
            
            DefineLibraryMethod(module, "hexencode", 0x21, 
                0x00000002U, 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.HexEncode)
            );
            
        }
        
        private static void LoadDigest__Base_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<<", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Digest.Digest.Base, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Digest.Digest.Base>(IronRuby.StandardLibrary.Digest.Digest.Base.Update)
            );
            
            DefineLibraryMethod(module, "finish", 0x12, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Digest.Digest.Base, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Base.Finish)
            );
            
            DefineLibraryMethod(module, "reset", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Digest.Digest.Base, IronRuby.StandardLibrary.Digest.Digest.Base>(IronRuby.StandardLibrary.Digest.Digest.Base.Reset)
            );
            
            DefineLibraryMethod(module, "update", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Digest.Digest.Base, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Digest.Digest.Base>(IronRuby.StandardLibrary.Digest.Digest.Base.Update)
            );
            
        }
        
        private static void LoadDigest__Class_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "digest", 0x21, 
                0x00040008U, 0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.MutableString, System.Object>>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Class.Digest), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Class.Digest)
            );
            
            DefineLibraryMethod(module, "hexdigest", 0x21, 
                0x00020004U, 0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.MutableString, System.Object>>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Class.HexDigest), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Class.HexDigest)
            );
            
        }
        
        private static void LoadDigest__Instance_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "digest", 0x11, 
                0x00000000U, 0x00080010U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Instance.Digest), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.MutableString, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Instance.Digest)
            );
            
            DefineLibraryMethod(module, "digest!", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Instance.DigestNew)
            );
            
            DefineLibraryMethod(module, "hexdigest", 0x11, 
                0x00000000U, 0x00080010U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, IronRuby.Builtins.RubyClass, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Instance.HexDigest), 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, IronRuby.Builtins.MutableString, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Instance.HexDigest)
            );
            
            DefineLibraryMethod(module, "hexdigest!", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, IronRuby.Runtime.CallSiteStorage<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object>>, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Digest.Digest.Instance.HexDigestNew)
            );
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.Zlib {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class ZlibLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(System.SystemException));
            IronRuby.Builtins.RubyClass classRef1 = GetClass(typeof(System.Object));
            IronRuby.Builtins.RubyClass classRef2 = GetClass(typeof(IronRuby.Builtins.RuntimeError));
            
            
            IronRuby.Builtins.RubyModule def1 = DefineGlobalModule("Zlib", typeof(IronRuby.StandardLibrary.Zlib.Zlib), 0x00000008, null, LoadZlib_Class, LoadZlib_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def5 = DefineClass("Zlib::Error", typeof(IronRuby.StandardLibrary.Zlib.Zlib.Error), 0x00000008, classRef0, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(ZlibLibraryInitializer.ExceptionFactory__Zlib__Error));
            IronRuby.Builtins.RubyClass def6 = DefineClass("Zlib::GzipFile", typeof(IronRuby.StandardLibrary.Zlib.Zlib.GZipFile), 0x00000008, classRef1, LoadZlib__GzipFile_Instance, LoadZlib__GzipFile_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def7 = DefineClass("Zlib::GzipFile::Error", typeof(IronRuby.StandardLibrary.Zlib.Zlib.GZipFile.Error), 0x00000008, classRef2, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def12 = DefineClass("Zlib::ZStream", typeof(IronRuby.StandardLibrary.Zlib.Zlib.ZStream), 0x00000008, classRef1, LoadZlib__ZStream_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def2 = DefineClass("Zlib::BufError", typeof(IronRuby.StandardLibrary.Zlib.Zlib.BufError), 0x00000008, def5, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(ZlibLibraryInitializer.ExceptionFactory__Zlib__BufError));
            IronRuby.Builtins.RubyClass def3 = DefineClass("Zlib::DataError", typeof(IronRuby.StandardLibrary.Zlib.Zlib.DataError), 0x00000008, def5, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(ZlibLibraryInitializer.ExceptionFactory__Zlib__DataError));
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def4 = DefineClass("Zlib::Deflate", typeof(IronRuby.StandardLibrary.Zlib.Zlib.Deflate), 0x00000008, def12, LoadZlib__Deflate_Instance, LoadZlib__Deflate_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            #endif
            IronRuby.Builtins.RubyClass def8 = DefineClass("Zlib::GzipReader", typeof(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader), 0x00000008, def6, LoadZlib__GzipReader_Instance, LoadZlib__GzipReader_Class, LoadZlib__GzipReader_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Builtins.RubyClass, System.Object, IronRuby.StandardLibrary.Zlib.Zlib.GZipReader>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.Create)
            );
            #if !SILVERLIGHT
            IronRuby.Builtins.RubyClass def9 = DefineClass("Zlib::GzipWriter", typeof(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter), 0x00000008, def6, LoadZlib__GzipWriter_Instance, LoadZlib__GzipWriter_Class, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Builtins.RubyClass, System.Object, System.Int32, System.Int32, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Create)
            );
            #endif
            IronRuby.Builtins.RubyClass def10 = DefineClass("Zlib::Inflate", typeof(IronRuby.StandardLibrary.Zlib.Zlib.Inflate), 0x00000008, def12, LoadZlib__Inflate_Instance, LoadZlib__Inflate_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def11 = DefineClass("Zlib::StreamError", typeof(IronRuby.StandardLibrary.Zlib.Zlib.StreamError), 0x00000008, def5, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
            new Func<IronRuby.Builtins.RubyClass, System.Object, System.Exception>(ZlibLibraryInitializer.ExceptionFactory__Zlib__StreamError));
            SetConstant(def1, "Error", def5);
            SetConstant(def1, "GzipFile", def6);
            SetConstant(def6, "Error", def7);
            SetConstant(def1, "ZStream", def12);
            SetConstant(def1, "BufError", def2);
            SetConstant(def1, "DataError", def3);
            #if !SILVERLIGHT
            SetConstant(def1, "Deflate", def4);
            #endif
            SetConstant(def1, "GzipReader", def8);
            #if !SILVERLIGHT
            SetConstant(def1, "GzipWriter", def9);
            #endif
            SetConstant(def1, "Inflate", def10);
            SetConstant(def1, "StreamError", def11);
        }
        
        private static void LoadZlib_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetConstant(module, "ASCII", IronRuby.StandardLibrary.Zlib.Zlib.ASCII);
            SetConstant(module, "BEST_COMPRESSION", IronRuby.StandardLibrary.Zlib.Zlib.BEST_COMPRESSION);
            SetConstant(module, "BEST_SPEED", IronRuby.StandardLibrary.Zlib.Zlib.BEST_SPEED);
            SetConstant(module, "BINARY", IronRuby.StandardLibrary.Zlib.Zlib.BINARY);
            SetConstant(module, "DEFAULT_COMPRESSION", IronRuby.StandardLibrary.Zlib.Zlib.DEFAULT_COMPRESSION);
            SetConstant(module, "DEFAULT_STRATEGY", IronRuby.StandardLibrary.Zlib.Zlib.DEFAULT_STRATEGY);
            SetConstant(module, "FILTERED", IronRuby.StandardLibrary.Zlib.Zlib.FILTERED);
            SetConstant(module, "FINISH", IronRuby.StandardLibrary.Zlib.Zlib.FINISH);
            SetConstant(module, "FIXLCODES", IronRuby.StandardLibrary.Zlib.Zlib.FIXLCODES);
            SetConstant(module, "FULL_FLUSH", IronRuby.StandardLibrary.Zlib.Zlib.FULL_FLUSH);
            SetConstant(module, "HUFFMAN_ONLY", IronRuby.StandardLibrary.Zlib.Zlib.HUFFMAN_ONLY);
            SetConstant(module, "MAX_WBITS", IronRuby.StandardLibrary.Zlib.Zlib.MAX_WBITS);
            SetConstant(module, "MAXBITS", IronRuby.StandardLibrary.Zlib.Zlib.MAXBITS);
            SetConstant(module, "MAXCODES", IronRuby.StandardLibrary.Zlib.Zlib.MAXCODES);
            SetConstant(module, "MAXDCODES", IronRuby.StandardLibrary.Zlib.Zlib.MAXDCODES);
            SetConstant(module, "MAXLCODES", IronRuby.StandardLibrary.Zlib.Zlib.MAXLCODES);
            SetConstant(module, "NO_COMPRESSION", IronRuby.StandardLibrary.Zlib.Zlib.NO_COMPRESSION);
            SetConstant(module, "NO_FLUSH", IronRuby.StandardLibrary.Zlib.Zlib.NO_FLUSH);
            SetConstant(module, "SYNC_FLUSH", IronRuby.StandardLibrary.Zlib.Zlib.SYNC_FLUSH);
            SetConstant(module, "UNKNOWN", IronRuby.StandardLibrary.Zlib.Zlib.UNKNOWN);
            SetConstant(module, "VERSION", IronRuby.StandardLibrary.Zlib.Zlib.VERSION);
            SetConstant(module, "Z_DEFLATED", IronRuby.StandardLibrary.Zlib.Zlib.Z_DEFLATED);
            SetConstant(module, "ZLIB_VERSION", IronRuby.StandardLibrary.Zlib.Zlib.ZLIB_VERSION);
            
        }
        
        private static void LoadZlib_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "crc32", 0x21, 
                0x00000000U, 0x00010000U, 
                new Func<IronRuby.Builtins.RubyModule, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.GetCrc), 
                new Func<IronRuby.Builtins.RubyModule, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GetCrc)
            );
            
            #endif
        }
        
        #if !SILVERLIGHT
        private static void LoadZlib__Deflate_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "deflate", 0x11, 
                0x00010002U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.Deflate, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.Deflate.DeflateString)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadZlib__Deflate_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "deflate", 0x21, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.Deflate.DeflateString)
            );
            
        }
        #endif
        
        private static void LoadZlib__GzipFile_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "closed?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GZipFile, System.Boolean>(IronRuby.StandardLibrary.Zlib.Zlib.GZipFile.IsClosed)
            );
            
            DefineLibraryMethod(module, "comment", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GZipFile, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.GZipFile.Comment)
            );
            
            DefineLibraryMethod(module, "orig_name", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GZipFile, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.GZipFile.OriginalName)
            );
            
            DefineLibraryMethod(module, "original_name", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GZipFile, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.GZipFile.OriginalName)
            );
            
        }
        
        private static void LoadZlib__GzipFile_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "wrap", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, System.Object, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GZipFile.Wrap)
            );
            
        }
        
        private static void LoadZlib__GzipReader_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetConstant(module, "OSES", IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.OSES);
            
        }
        
        private static void LoadZlib__GzipReader_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "close", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GZipReader, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.Close)
            );
            
            DefineLibraryMethod(module, "finish", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GZipReader, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.Finish)
            );
            
            DefineLibraryMethod(module, "open", 0x12, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GZipReader, IronRuby.StandardLibrary.Zlib.Zlib.GZipReader>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.Open)
            );
            
            DefineLibraryMethod(module, "read", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GZipReader, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.Read)
            );
            
            DefineLibraryMethod(module, "xtra_field", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GZipReader, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.ExtraField)
            );
            
        }
        
        private static void LoadZlib__GzipReader_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "open", 0x21, 
                0x00020004U, 0x0004000aU, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Zlib.Zlib.GZipReader>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.Open), 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GZipReader.Open)
            );
            
        }
        
        #if !SILVERLIGHT
        private static void LoadZlib__GzipWriter_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<<", 0x11, 
                0x00040008U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Output)
            );
            
            DefineLibraryMethod(module, "close", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Close)
            );
            
            DefineLibraryMethod(module, "comment=", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Comment)
            );
            
            DefineLibraryMethod(module, "finish", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Finish)
            );
            
            DefineLibraryMethod(module, "flush", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, System.Object, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Flush), 
                new Func<IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, System.Int32, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Flush)
            );
            
            DefineLibraryMethod(module, "orig_name=", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.OriginalName)
            );
            
            DefineLibraryMethod(module, "write", 0x11, 
                0x00040008U, 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Write)
            );
            
        }
        #endif
        
        #if !SILVERLIGHT
        private static void LoadZlib__GzipWriter_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "open", 0x21, 
                0x00000010U, 0x00000010U, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32, System.Int32, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Open), 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object, System.Object, System.Object>(IronRuby.StandardLibrary.Zlib.Zlib.GzipWriter.Open)
            );
            
        }
        #endif
        
        private static void LoadZlib__Inflate_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "close", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.Inflate, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.Inflate.Close)
            );
            
            DefineLibraryMethod(module, "inflate", 0x11, 
                0x00010002U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.Inflate, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.Inflate.InflateString)
            );
            
        }
        
        private static void LoadZlib__Inflate_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "inflate", 0x21, 
                0x00010002U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Zlib.Zlib.Inflate.InflateString)
            );
            
        }
        
        private static void LoadZlib__ZStream_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "adler", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.Adler)
            );
            
            DefineLibraryMethod(module, "avail_in", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.AvailIn)
            );
            
            DefineLibraryMethod(module, "avail_out", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.GetAvailOut)
            );
            
            DefineLibraryMethod(module, "avail_out=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Int32, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.SetAvailOut)
            );
            
            DefineLibraryMethod(module, "close", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Boolean>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.Close)
            );
            
            DefineLibraryMethod(module, "closed?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Boolean>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.IsClosed)
            );
            
            DefineLibraryMethod(module, "data_type", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.Zlib.Zlib.ZStream>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.DataType)
            );
            
            DefineLibraryMethod(module, "finish", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Boolean>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.Close)
            );
            
            DefineLibraryMethod(module, "finished?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Boolean>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.IsClosed)
            );
            
            DefineLibraryMethod(module, "flush_next_in", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Collections.Generic.List<System.Byte>>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.FlushNextIn)
            );
            
            DefineLibraryMethod(module, "flush_next_out", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Collections.Generic.List<System.Byte>>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.FlushNextOut)
            );
            
            DefineLibraryMethod(module, "reset", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.Zlib.Zlib.ZStream>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.Reset)
            );
            
            DefineLibraryMethod(module, "stream_end?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Boolean>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.IsClosed)
            );
            
            DefineLibraryMethod(module, "total_in", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.TotalIn)
            );
            
            DefineLibraryMethod(module, "total_out", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Zlib.Zlib.ZStream, System.Int32>(IronRuby.StandardLibrary.Zlib.Zlib.ZStream.TotalOut)
            );
            
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Zlib__BufError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Zlib.Zlib.BufError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Zlib__DataError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Zlib.Zlib.DataError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Zlib__Error(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Zlib.Zlib.Error(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Zlib__StreamError(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Zlib.Zlib.StreamError(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
    }
}

namespace IronRuby.StandardLibrary.StringIO {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class StringIOLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyModule moduleRef0 = GetModule(typeof(IronRuby.Builtins.Enumerable));
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(System.Object));
            
            
            DefineGlobalClass("StringIO", typeof(IronRuby.StandardLibrary.StringIO.StringIO), 0x00000008, classRef0, LoadStringIO_Instance, LoadStringIO_Class, null, new IronRuby.Builtins.RubyModule[] {moduleRef0}, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Create)
            );
        }
        
        private static void LoadStringIO_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "<<", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.Output)
            );
            
            DefineLibraryMethod(module, "binmode", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.SetBinaryMode)
            );
            
            DefineLibraryMethod(module, "close", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Close)
            );
            
            DefineLibraryMethod(module, "close_read", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.CloseRead)
            );
            
            DefineLibraryMethod(module, "close_write", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.CloseWrite)
            );
            
            DefineLibraryMethod(module, "closed?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.IsClosed)
            );
            
            DefineLibraryMethod(module, "closed_read?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.IsClosedRead)
            );
            
            DefineLibraryMethod(module, "closed_write?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.IsClosedWrite)
            );
            
            DefineLibraryMethod(module, "each", 0x11, 
                0x00000000U, 0x00000000U, 0x00040008U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine)
            );
            
            DefineLibraryMethod(module, "each_byte", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachByte)
            );
            
            DefineLibraryMethod(module, "each_line", 0x11, 
                0x00000000U, 0x00000000U, 0x00040008U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.EachLine)
            );
            
            DefineLibraryMethod(module, "eof", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.Eof)
            );
            
            DefineLibraryMethod(module, "eof?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.Eof)
            );
            
            DefineLibraryMethod(module, "fcntl", 0x11, 
                0x00000000U, 
                new Action<IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.FileControl)
            );
            
            DefineLibraryMethod(module, "fileno", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.GetDescriptor)
            );
            
            DefineLibraryMethod(module, "flush", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Flush)
            );
            
            DefineLibraryMethod(module, "fsync", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.FSync)
            );
            
            DefineLibraryMethod(module, "getc", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.GetByte)
            );
            
            DefineLibraryMethod(module, "gets", 0x11, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Gets), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Gets), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Gets), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Gets)
            );
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00000000U, 0x00030006U, 0x00010002U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reinitialize), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reinitialize), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x12, 
                0x00000008U, 0x00000006U, 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.StandardLibrary.StringIO.StringIO, System.Object, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen)
            );
            
            DefineLibraryMethod(module, "isatty", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.IsConsole)
            );
            
            DefineLibraryMethod(module, "length", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.GetLength)
            );
            
            DefineLibraryMethod(module, "lineno", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.GetLineNo)
            );
            
            DefineLibraryMethod(module, "lineno=", 0x11, 
                0x00010000U, 
                new Action<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.SetLineNo)
            );
            
            DefineLibraryMethod(module, "pid", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.GetDescriptor)
            );
            
            DefineLibraryMethod(module, "pos", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.GetPosition)
            );
            
            DefineLibraryMethod(module, "pos=", 0x11, 
                0x00010000U, 
                new Action<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Pos)
            );
            
            DefineLibraryMethod(module, "print", 0x11, 
                0x00000000U, 0x80000000U, 0x00000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object[]>(IronRuby.StandardLibrary.StringIO.StringIO.Print), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.Print)
            );
            
            DefineLibraryMethod(module, "printf", 0x11, 
                0x80080010U, 
                new Action<IronRuby.Builtins.StringFormatterSiteStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Object[]>(IronRuby.StandardLibrary.StringIO.StringIO.PrintFormatted)
            );
            
            DefineLibraryMethod(module, "putc", 0x11, 
                0x00000004U, 0x00020000U, 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Putc), 
                new Func<IronRuby.Runtime.BinaryOpStorage, System.Object, System.Int32, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Putc)
            );
            
            DefineLibraryMethod(module, "puts", 0x11, 
                0x00000000U, 0x00000004U, 0x00000010U, 0x80000000U, 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.PutsEmptyLine), 
                new Action<IronRuby.Runtime.BinaryOpStorage, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Puts), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.Puts), 
                new Action<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Runtime.ConversionStorage<System.Collections.IList>, System.Object, System.Object[]>(IronRuby.StandardLibrary.StringIO.StringIO.Puts)
            );
            
            DefineLibraryMethod(module, "read", 0x11, 
                0x00000000U, 0x00020004U, 0x00030004U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Read), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Read), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.Read)
            );
            
            DefineLibraryMethod(module, "readchar", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.ReadChar)
            );
            
            DefineLibraryMethod(module, "readline", 0x11, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00060000U, 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLine), 
                new Func<IronRuby.Runtime.RubyScope, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLine)
            );
            
            DefineLibraryMethod(module, "readlines", 0x11, 
                0x00000000U, 0x00000000U, 0x00020004U, 0x00030000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLines), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Runtime.Union<IronRuby.Builtins.MutableString, System.Int32>, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLines), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.StringIO.StringIO.ReadLines)
            );
            
            DefineLibraryMethod(module, "reopen", 0x11, 
                new[] { 0x00000000U, 0x00000008U, 0x00000006U, 0x00000002U, 0x00030006U, 0x00010002U}, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen), 
                new Func<IronRuby.Runtime.RespondToStorage, IronRuby.Runtime.UnaryOpStorage, IronRuby.StandardLibrary.StringIO.StringIO, System.Object, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32, IronRuby.StandardLibrary.StringIO.StringIO>(IronRuby.StandardLibrary.StringIO.StringIO.Reopen)
            );
            
            DefineLibraryMethod(module, "rewind", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Rewind)
            );
            
            DefineLibraryMethod(module, "seek", 0x11, 
                0x00030000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32, System.Int32, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Seek)
            );
            
            DefineLibraryMethod(module, "size", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.GetLength)
            );
            
            DefineLibraryMethod(module, "string", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.GetString)
            );
            
            DefineLibraryMethod(module, "string=", 0x11, 
                0x00010002U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.SetString)
            );
            
            DefineLibraryMethod(module, "sync", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.Sync)
            );
            
            DefineLibraryMethod(module, "sync=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.SetSync)
            );
            
            DefineLibraryMethod(module, "sysread", 0x11, 
                0x00000000U, 0x00020004U, 0x00030004U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.SystemRead), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, Microsoft.Scripting.Runtime.DynamicNull, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.SystemRead), 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringIO.StringIO.SystemRead)
            );
            
            DefineLibraryMethod(module, "syswrite", 0x11, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Write), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.StandardLibrary.StringIO.StringIO, System.Object, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Write)
            );
            
            DefineLibraryMethod(module, "tell", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.GetPosition)
            );
            
            DefineLibraryMethod(module, "truncate", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.ConversionStorage<System.Int32>, IronRuby.StandardLibrary.StringIO.StringIO, System.Object, System.Object>(IronRuby.StandardLibrary.StringIO.StringIO.SetLength)
            );
            
            DefineLibraryMethod(module, "tty?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, System.Boolean>(IronRuby.StandardLibrary.StringIO.StringIO.IsConsole)
            );
            
            DefineLibraryMethod(module, "ungetc", 0x11, 
                0x00010000U, 
                new Action<IronRuby.StandardLibrary.StringIO.StringIO, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.SetPreviousByte)
            );
            
            DefineLibraryMethod(module, "write", 0x11, 
                0x00000002U, 0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringIO.StringIO, IronRuby.Builtins.MutableString, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Write), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.StandardLibrary.StringIO.StringIO, System.Object, System.Int32>(IronRuby.StandardLibrary.StringIO.StringIO.Write)
            );
            
        }
        
        private static void LoadStringIO_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineRuleGenerator(module, "open", 0x21, IronRuby.StandardLibrary.StringIO.StringIO.Open());
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.StringScanner {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class StringScannerLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(IronRuby.Builtins.RubyObject));
            
            
            DefineGlobalClass("StringScanner", typeof(IronRuby.StandardLibrary.StringScanner.StringScanner), 0x00000008, classRef0, LoadStringScanner_Instance, LoadStringScanner_Class, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Object, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.Create)
            );
        }
        
        private static void LoadStringScanner_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "[]", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.GetMatchSubgroup)
            );
            
            DefineLibraryMethod(module, "<<", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.Concat)
            );
            
            DefineLibraryMethod(module, "beginning_of_line?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Boolean>(IronRuby.StandardLibrary.StringScanner.StringScanner.BeginningOfLine)
            );
            
            DefineLibraryMethod(module, "bol?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Boolean>(IronRuby.StandardLibrary.StringScanner.StringScanner.BeginningOfLine)
            );
            
            DefineLibraryMethod(module, "check", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.Check)
            );
            
            DefineLibraryMethod(module, "check_until", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.CheckUntil)
            );
            
            DefineLibraryMethod(module, "clear", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.Clear)
            );
            
            DefineLibraryMethod(module, "concat", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.Concat)
            );
            
            DefineLibraryMethod(module, "empty?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Boolean>(IronRuby.StandardLibrary.StringScanner.StringScanner.EndOfLine)
            );
            
            DefineLibraryMethod(module, "eos?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Boolean>(IronRuby.StandardLibrary.StringScanner.StringScanner.EndOfLine)
            );
            
            DefineLibraryMethod(module, "exist?", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Nullable<System.Int32>>(IronRuby.StandardLibrary.StringScanner.StringScanner.Exist)
            );
            
            DefineLibraryMethod(module, "get_byte", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.GetByte)
            );
            
            DefineLibraryMethod(module, "getbyte", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.GetByte)
            );
            
            DefineLibraryMethod(module, "getch", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.GetChar)
            );
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x00010002U, 
                new Action<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.StringScanner.StringScanner.Reinitialize)
            );
            
            DefineLibraryMethod(module, "initialize_copy", 0x12, 
                0x00010002U, 
                new Action<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.InitializeFrom)
            );
            
            DefineLibraryMethod(module, "inspect", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.ToString)
            );
            
            DefineLibraryMethod(module, "match?", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Nullable<System.Int32>>(IronRuby.StandardLibrary.StringScanner.StringScanner.Match)
            );
            
            DefineLibraryMethod(module, "matched", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.Matched)
            );
            
            DefineLibraryMethod(module, "matched?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Boolean>(IronRuby.StandardLibrary.StringScanner.StringScanner.WasMatched)
            );
            
            DefineLibraryMethod(module, "matched_size", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Nullable<System.Int32>>(IronRuby.StandardLibrary.StringScanner.StringScanner.MatchedSize)
            );
            
            DefineLibraryMethod(module, "matchedsize", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Nullable<System.Int32>>(IronRuby.StandardLibrary.StringScanner.StringScanner.MatchedSize)
            );
            
            DefineLibraryMethod(module, "peek", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.Peek)
            );
            
            DefineLibraryMethod(module, "peep", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.Peek)
            );
            
            DefineLibraryMethod(module, "pointer", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32>(IronRuby.StandardLibrary.StringScanner.StringScanner.GetCurrentPosition)
            );
            
            DefineLibraryMethod(module, "pointer=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32, System.Int32>(IronRuby.StandardLibrary.StringScanner.StringScanner.SetCurrentPosition)
            );
            
            DefineLibraryMethod(module, "pos", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32>(IronRuby.StandardLibrary.StringScanner.StringScanner.GetCurrentPosition)
            );
            
            DefineLibraryMethod(module, "pos=", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32, System.Int32>(IronRuby.StandardLibrary.StringScanner.StringScanner.SetCurrentPosition)
            );
            
            DefineLibraryMethod(module, "post_match", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.PostMatch)
            );
            
            DefineLibraryMethod(module, "pre_match", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.PreMatch)
            );
            
            DefineLibraryMethod(module, "reset", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.Reset)
            );
            
            DefineLibraryMethod(module, "rest", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.Rest)
            );
            
            DefineLibraryMethod(module, "rest?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Boolean>(IronRuby.StandardLibrary.StringScanner.StringScanner.IsRestLeft)
            );
            
            DefineLibraryMethod(module, "rest_size", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32>(IronRuby.StandardLibrary.StringScanner.StringScanner.RestSize)
            );
            
            DefineLibraryMethod(module, "restsize", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, System.Int32>(IronRuby.StandardLibrary.StringScanner.StringScanner.RestSize)
            );
            
            DefineLibraryMethod(module, "scan", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.StandardLibrary.StringScanner.StringScanner.Scan)
            );
            
            DefineLibraryMethod(module, "scan_full", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Boolean, System.Boolean, System.Object>(IronRuby.StandardLibrary.StringScanner.StringScanner.ScanFull)
            );
            
            DefineLibraryMethod(module, "scan_until", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Object>(IronRuby.StandardLibrary.StringScanner.StringScanner.ScanUntil)
            );
            
            DefineLibraryMethod(module, "search_full", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Boolean, System.Boolean, System.Object>(IronRuby.StandardLibrary.StringScanner.StringScanner.SearchFull)
            );
            
            DefineLibraryMethod(module, "skip", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Nullable<System.Int32>>(IronRuby.StandardLibrary.StringScanner.StringScanner.Skip)
            );
            
            DefineLibraryMethod(module, "skip_until", 0x11, 
                0x00000002U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.RubyRegex, System.Nullable<System.Int32>>(IronRuby.StandardLibrary.StringScanner.StringScanner.SkipUntil)
            );
            
            DefineLibraryMethod(module, "string", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.GetString)
            );
            
            DefineLibraryMethod(module, "string=", 0x11, 
                0x00000004U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.SetString)
            );
            
            DefineLibraryMethod(module, "terminate", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.Clear)
            );
            
            DefineLibraryMethod(module, "to_s", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.StringScanner.StringScanner.ToString)
            );
            
            DefineLibraryMethod(module, "unscan", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.StringScanner.StringScanner, IronRuby.StandardLibrary.StringScanner.StringScanner>(IronRuby.StandardLibrary.StringScanner.StringScanner.Unscan)
            );
            
        }
        
        private static void LoadStringScanner_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "must_C_version", 0x21, 
                0x00000000U, 
                new Func<System.Object, System.Object>(IronRuby.StandardLibrary.StringScanner.StringScanner.MustCVersion)
            );
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.Enumerator {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class EnumeratorLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.FunctionControl {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class FunctionControlLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.FileControl {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class FileControlLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            
            
            DefineGlobalModule("Fcntl", typeof(IronRuby.StandardLibrary.FileControl.Fcntl), 0x00000008, null, null, LoadFcntl_Constants, IronRuby.Builtins.RubyModule.EmptyArray);
        }
        
        private static void LoadFcntl_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetConstant(module, "F_SETFL", IronRuby.StandardLibrary.FileControl.Fcntl.F_SETFL);
            SetConstant(module, "O_ACCMODE", IronRuby.StandardLibrary.FileControl.Fcntl.O_ACCMODE);
            SetConstant(module, "O_APPEND", IronRuby.StandardLibrary.FileControl.Fcntl.O_APPEND);
            SetConstant(module, "O_CREAT", IronRuby.StandardLibrary.FileControl.Fcntl.O_CREAT);
            SetConstant(module, "O_EXCL", IronRuby.StandardLibrary.FileControl.Fcntl.O_EXCL);
            SetConstant(module, "O_NONBLOCK", IronRuby.StandardLibrary.FileControl.Fcntl.O_NONBLOCK);
            SetConstant(module, "O_RDONLY", IronRuby.StandardLibrary.FileControl.Fcntl.O_RDONLY);
            SetConstant(module, "O_RDWR", IronRuby.StandardLibrary.FileControl.Fcntl.O_RDWR);
            SetConstant(module, "O_TRUNC", IronRuby.StandardLibrary.FileControl.Fcntl.O_TRUNC);
            SetConstant(module, "O_WRONLY", IronRuby.StandardLibrary.FileControl.Fcntl.O_WRONLY);
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.BigDecimal {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class BigDecimalLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(IronRuby.Builtins.Numeric));
            
            
            DefineGlobalClass("BigDecimal", typeof(IronRuby.StandardLibrary.BigDecimal.BigDecimal), 0x00000000, classRef0, LoadBigDecimal_Instance, LoadBigDecimal_Class, LoadBigDecimal_Constants, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.CreateBigDecimal)
            );
            ExtendModule(typeof(IronRuby.Builtins.Kernel), 0x00000000, LoadIronRuby__Builtins__Kernel_Instance, LoadIronRuby__Builtins__Kernel_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
        }
        
        private static void LoadBigDecimal_Constants(IronRuby.Builtins.RubyModule/*!*/ module) {
            SetConstant(module, "BASE", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.BASE);
            SetConstant(module, "EXCEPTION_ALL", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.EXCEPTION_ALL);
            SetConstant(module, "EXCEPTION_INFINITY", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.EXCEPTION_INFINITY);
            SetConstant(module, "EXCEPTION_NaN", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.EXCEPTION_NaN);
            SetConstant(module, "EXCEPTION_OVERFLOW", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.EXCEPTION_OVERFLOW);
            SetConstant(module, "EXCEPTION_UNDERFLOW", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.EXCEPTION_UNDERFLOW);
            SetConstant(module, "EXCEPTION_ZERODIVIDE", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.EXCEPTION_ZERODIVIDE);
            SetConstant(module, "ROUND_CEILING", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_CEILING);
            SetConstant(module, "ROUND_DOWN", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_DOWN);
            SetConstant(module, "ROUND_FLOOR", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_FLOOR);
            SetConstant(module, "ROUND_HALF_DOWN", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_HALF_DOWN);
            SetConstant(module, "ROUND_HALF_EVEN", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_HALF_EVEN);
            SetConstant(module, "ROUND_HALF_UP", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_HALF_UP);
            SetConstant(module, "ROUND_MODE", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_MODE);
            SetConstant(module, "ROUND_UP", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ROUND_UP);
            SetConstant(module, "SIGN_NaN", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SIGN_NaN);
            SetConstant(module, "SIGN_NEGATIVE_FINITE", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SIGN_NEGATIVE_FINITE);
            SetConstant(module, "SIGN_NEGATIVE_INFINITE", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SIGN_NEGATIVE_INFINITE);
            SetConstant(module, "SIGN_NEGATIVE_ZERO", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SIGN_NEGATIVE_ZERO);
            SetConstant(module, "SIGN_POSITIVE_FINITE", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SIGN_POSITIVE_FINITE);
            SetConstant(module, "SIGN_POSITIVE_INFINITE", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SIGN_POSITIVE_INFINITE);
            SetConstant(module, "SIGN_POSITIVE_ZERO", IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SIGN_POSITIVE_ZERO);
            
        }
        
        private static void LoadBigDecimal_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "-", 0x11, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract)
            );
            
            DefineLibraryMethod(module, "%", 0x11, 
                0x00000004U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ModuloOp)
            );
            
            DefineLibraryMethod(module, "*", 0x11, 
                0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply)
            );
            
            DefineLibraryMethod(module, "**", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Power)
            );
            
            DefineLibraryMethod(module, "/", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Divide), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Divide)
            );
            
            DefineLibraryMethod(module, "-@", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Negate)
            );
            
            DefineLibraryMethod(module, "_dump", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Dump)
            );
            
            DefineLibraryMethod(module, "+", 0x11, 
                0x00000004U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add)
            );
            
            DefineLibraryMethod(module, "+@", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Identity)
            );
            
            DefineLibraryMethod(module, "<", 0x11, 
                new[] { 0x00000002U, 0x00000004U, 0x00000000U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThan), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThan), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThan), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThan), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThan)
            );
            
            DefineLibraryMethod(module, "<=", 0x11, 
                new[] { 0x00000002U, 0x00000004U, 0x00000000U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThanOrEqual), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.LessThanOrEqual)
            );
            
            DefineLibraryMethod(module, "<=>", 0x11, 
                new[] { 0x00000000U, 0x00000002U, 0x00000004U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Compare), 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Compare), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Compare), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Compare), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Compare)
            );
            
            DefineLibraryMethod(module, "==", 0x11, 
                new[] { 0x00000002U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal)
            );
            
            DefineLibraryMethod(module, "===", 0x11, 
                new[] { 0x00000002U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal)
            );
            
            DefineLibraryMethod(module, ">", 0x11, 
                new[] { 0x00000002U, 0x00000004U, 0x00000000U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThan), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThan), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThan), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThan), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThan)
            );
            
            DefineLibraryMethod(module, ">=", 0x11, 
                new[] { 0x00000002U, 0x00000004U, 0x00000000U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThanOrEqual), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThanOrEqual), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.GreaterThanOrEqual)
            );
            
            DefineLibraryMethod(module, "abs", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Abs)
            );
            
            DefineLibraryMethod(module, "add", 0x11, 
                new[] { 0x00000004U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00080000U}, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Add)
            );
            
            DefineLibraryMethod(module, "ceil", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Ceil)
            );
            
            DefineLibraryMethod(module, "coerce", 0x11, 
                0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Coerce), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Coerce), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Coerce), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Coerce)
            );
            
            DefineLibraryMethod(module, "div", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Div), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Div)
            );
            
            DefineLibraryMethod(module, "divmod", 0x11, 
                0x00000004U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.DivMod), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.DivMod), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.DivMod), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.DivMod)
            );
            
            DefineLibraryMethod(module, "eql?", 0x11, 
                new[] { 0x00000002U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Equal)
            );
            
            DefineLibraryMethod(module, "exponent", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Exponent)
            );
            
            DefineLibraryMethod(module, "finite?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.IsFinite)
            );
            
            DefineLibraryMethod(module, "fix", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Fix)
            );
            
            DefineLibraryMethod(module, "floor", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Floor)
            );
            
            DefineLibraryMethod(module, "frac", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Fraction)
            );
            
            DefineLibraryMethod(module, "hash", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Hash)
            );
            
            DefineLibraryMethod(module, "infinite?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.IsInfinite)
            );
            
            DefineLibraryMethod(module, "inspect", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Inspect)
            );
            
            DefineLibraryMethod(module, "modulo", 0x11, 
                new[] { 0x00000004U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U}, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Modulo)
            );
            
            DefineLibraryMethod(module, "mult", 0x11, 
                new[] { 0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00080000U}, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Multiply)
            );
            
            DefineLibraryMethod(module, "nan?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.IsNaN)
            );
            
            DefineLibraryMethod(module, "nonzero?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.IsNonZero)
            );
            
            DefineLibraryMethod(module, "power", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Power)
            );
            
            DefineLibraryMethod(module, "precs", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Precision)
            );
            
            DefineLibraryMethod(module, "quo", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Divide), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Quotient)
            );
            
            DefineLibraryMethod(module, "remainder", 0x11, 
                0x00000004U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Remainder), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Remainder)
            );
            
            DefineLibraryMethod(module, "round", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Round), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Round)
            );
            
            DefineLibraryMethod(module, "sign", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Sign)
            );
            
            DefineLibraryMethod(module, "split", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Split)
            );
            
            DefineLibraryMethod(module, "sqrt", 0x11, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SquareRoot), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.SquareRoot)
            );
            
            DefineLibraryMethod(module, "sub", 0x11, 
                new[] { 0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00000000U, 0x00000000U, 0x00000004U, 0x00000000U, 0x00080000U}, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, Microsoft.Scripting.Math.BigInteger, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract), 
                new Func<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.BinaryOpStorage, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Subtract)
            );
            
            DefineLibraryMethod(module, "to_f", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Double>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ToFloat)
            );
            
            DefineLibraryMethod(module, "to_i", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ToI)
            );
            
            DefineLibraryMethod(module, "to_int", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Object>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ToI)
            );
            
            DefineLibraryMethod(module, "to_s", 0x11, 
                0x00000000U, 0x00010000U, 0x00010002U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ToString), 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ToString), 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.ToString)
            );
            
            DefineLibraryMethod(module, "truncate", 0x11, 
                0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Truncate)
            );
            
            DefineLibraryMethod(module, "zero?", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.BigDecimal.BigDecimal, System.Boolean>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.IsZero)
            );
            
        }
        
        private static void LoadBigDecimal_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "_load", 0x21, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Load)
            );
            
            DefineLibraryMethod(module, "double_fig", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.DoubleFig)
            );
            
            DefineLibraryMethod(module, "induced_from", 0x21, 
                0x00000002U, 0x00000000U, 0x00000004U, 0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.StandardLibrary.BigDecimal.BigDecimal, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.InducedFrom), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, System.Int32, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.InducedFrom), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, Microsoft.Scripting.Math.BigInteger, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.InducedFrom), 
                new Func<IronRuby.Builtins.RubyClass, System.Object, IronRuby.StandardLibrary.BigDecimal.BigDecimal>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.InducedFrom)
            );
            
            DefineLibraryMethod(module, "limit", 0x21, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, System.Int32, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Limit), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, System.Object, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Limit)
            );
            
            DefineLibraryMethod(module, "mode", 0x21, 
                0x00000000U, 0x00000000U, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, System.Int32, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Mode), 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.Builtins.RubyClass, System.Int32, System.Object, System.Int32>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Mode)
            );
            
            DefineLibraryMethod(module, "ver", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.BigDecimal.BigDecimalOps.Version)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__Kernel_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "BigDecimal", 0x12, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.KernelOps.CreateBigDecimal)
            );
            
        }
        
        private static void LoadIronRuby__Builtins__Kernel_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "BigDecimal", 0x21, 
                0x00020000U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, System.Int32, System.Object>(IronRuby.StandardLibrary.BigDecimal.KernelOps.CreateBigDecimal)
            );
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.Iconv {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class IconvLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(System.Object));
            IronRuby.Builtins.RubyClass classRef1 = GetClass(typeof(IronRuby.Builtins.RuntimeError));
            IronRuby.Builtins.RubyClass classRef2 = GetClass(typeof(System.ArgumentException));
            
            
            IronRuby.Builtins.RubyClass def1 = DefineGlobalClass("Iconv", typeof(IronRuby.StandardLibrary.Iconv.Iconv), 0x00000008, classRef0, LoadIconv_Instance, LoadIconv_Class, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Iconv.Iconv>(IronRuby.StandardLibrary.Iconv.Iconv.Create)
            );
            IronRuby.Builtins.RubyModule def3 = DefineModule("Iconv::Failure", typeof(IronRuby.StandardLibrary.Iconv.Iconv.Failure), 0x00000008, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyClass def2 = DefineClass("Iconv::BrokenLibrary", typeof(IronRuby.StandardLibrary.Iconv.Iconv.BrokenLibrary), 0x00000008, classRef1, null, null, null, new IronRuby.Builtins.RubyModule[] {def3}, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, IronRuby.StandardLibrary.Iconv.Iconv.BrokenLibrary>(IronRuby.StandardLibrary.Iconv.Iconv.BrokenLibrary.Factory)
            );
            IronRuby.Builtins.RubyClass def4 = DefineClass("Iconv::IllegalSequence", typeof(IronRuby.StandardLibrary.Iconv.Iconv.IllegalSequence), 0x00000008, classRef2, null, null, null, new IronRuby.Builtins.RubyModule[] {def3}, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, IronRuby.StandardLibrary.Iconv.Iconv.IllegalSequence>(IronRuby.StandardLibrary.Iconv.Iconv.IllegalSequence.Factory)
            );
            IronRuby.Builtins.RubyClass def5 = DefineClass("Iconv::InvalidCharacter", typeof(IronRuby.StandardLibrary.Iconv.Iconv.InvalidCharacter), 0x00000008, classRef2, null, null, null, new IronRuby.Builtins.RubyModule[] {def3}, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, IronRuby.StandardLibrary.Iconv.Iconv.InvalidCharacter>(IronRuby.StandardLibrary.Iconv.Iconv.InvalidCharacter.Factory)
            );
            IronRuby.Builtins.RubyClass def6 = DefineClass("Iconv::InvalidEncoding", typeof(IronRuby.StandardLibrary.Iconv.Iconv.InvalidEncoding), 0x00000008, classRef2, null, null, null, new IronRuby.Builtins.RubyModule[] {def3}, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, IronRuby.StandardLibrary.Iconv.Iconv.InvalidEncoding>(IronRuby.StandardLibrary.Iconv.Iconv.InvalidEncoding.Factory)
            );
            IronRuby.Builtins.RubyClass def7 = DefineClass("Iconv::OutOfRange", typeof(IronRuby.StandardLibrary.Iconv.Iconv.OutOfRange), 0x00000008, classRef1, null, null, null, new IronRuby.Builtins.RubyModule[] {def3}, 
                new Func<IronRuby.Builtins.RubyClass, System.Object, System.Object, System.Object, IronRuby.StandardLibrary.Iconv.Iconv.OutOfRange>(IronRuby.StandardLibrary.Iconv.Iconv.OutOfRange.Factory)
            );
            SetConstant(def1, "Failure", def3);
            SetConstant(def1, "BrokenLibrary", def2);
            SetConstant(def1, "IllegalSequence", def4);
            SetConstant(def1, "InvalidCharacter", def5);
            SetConstant(def1, "InvalidEncoding", def6);
            SetConstant(def1, "OutOfRange", def7);
        }
        
        private static void LoadIconv_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "close", 0x11, 
                0x00000000U, 
                new Func<IronRuby.StandardLibrary.Iconv.Iconv, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Iconv.Iconv.Close)
            );
            
            DefineLibraryMethod(module, "iconv", 0x11, 
                0x00030000U, 0x00070008U, 
                new Func<IronRuby.StandardLibrary.Iconv.Iconv, IronRuby.Builtins.MutableString, System.Int32, System.Object, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Iconv.Iconv.iconv), 
                new Func<IronRuby.StandardLibrary.Iconv.Iconv, IronRuby.Builtins.MutableString, System.Int32, System.Int32, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Iconv.Iconv.iconv)
            );
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x0006000cU, 
                new Func<IronRuby.Runtime.RubyContext, IronRuby.StandardLibrary.Iconv.Iconv, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Iconv.Iconv>(IronRuby.StandardLibrary.Iconv.Iconv.Initialize)
            );
            
        }
        
        private static void LoadIconv_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "charset_map", 0x21, 
                0x00000000U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.Hash>(IronRuby.StandardLibrary.Iconv.Iconv.CharsetMap)
            );
            
            DefineLibraryMethod(module, "conv", 0x21, 
                0x00070006U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString>(IronRuby.StandardLibrary.Iconv.Iconv.Convert)
            );
            
            DefineLibraryMethod(module, "iconv", 0x21, 
                0x80030006U, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString[], IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Iconv.Iconv.iconv)
            );
            
            DefineLibraryMethod(module, "open", 0x21, 
                0x00030006U, 0x0006000dU, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Iconv.Iconv>(IronRuby.StandardLibrary.Iconv.Iconv.Create), 
                new Func<IronRuby.Runtime.BlockParam, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Object>(IronRuby.StandardLibrary.Iconv.Iconv.Open)
            );
            
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Iconv__BrokenLibrary(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Iconv.Iconv.BrokenLibrary(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Iconv__IllegalSequence(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Iconv.Iconv.IllegalSequence(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Iconv__InvalidCharacter(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Iconv.Iconv.InvalidCharacter(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Iconv__InvalidEncoding(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Iconv.Iconv.InvalidEncoding(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
        public static System.Exception/*!*/ ExceptionFactory__Iconv__OutOfRange(IronRuby.Builtins.RubyClass/*!*/ self, [DefaultParameterValueAttribute(null)]object message) {
            return IronRuby.Runtime.RubyExceptionData.InitializeException(new IronRuby.StandardLibrary.Iconv.Iconv.OutOfRange(IronRuby.Runtime.RubyExceptionData.GetClrMessage(self, message), (System.Exception)null), message);
        }
        
    }
}

namespace IronRuby.StandardLibrary.ParseTree {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class ParseTreeLibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            
            
            IronRuby.Builtins.RubyModule def1 = DefineGlobalModule("IronRuby", typeof(IronRuby.Ruby), 0x00000000, null, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            IronRuby.Builtins.RubyModule def2 = DefineModule("IronRuby::ParseTree", typeof(IronRuby.StandardLibrary.ParseTree.IronRubyOps.ParseTreeOps), 0x00000008, LoadIronRuby__ParseTree_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray);
            SetConstant(def1, "ParseTree", def2);
        }
        
        private static void LoadIronRuby__ParseTree_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineLibraryMethod(module, "parse_tree_for_meth", 0x11, 
                0x00020006U, 
                new Func<System.Object, IronRuby.Builtins.RubyModule, System.String, System.Boolean, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.ParseTree.IronRubyOps.ParseTreeOps.CreateParseTreeForMethod)
            );
            
            DefineLibraryMethod(module, "parse_tree_for_str", 0x11, 
                0x0000000cU, 
                new Func<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Int32, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.ParseTree.IronRubyOps.ParseTreeOps.CreateParseTreeForString)
            );
            
        }
        
    }
}

namespace IronRuby.StandardLibrary.Open3 {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class Open3LibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            
            
            DefineGlobalModule("Open3", typeof(IronRuby.StandardLibrary.Open3.Open3), 0x00000008, null, LoadOpen3_Class, null, IronRuby.Builtins.RubyModule.EmptyArray);
        }
        
        private static void LoadOpen3_Class(IronRuby.Builtins.RubyModule/*!*/ module) {
            #if !SILVERLIGHT
            DefineLibraryMethod(module, "popen3", 0x21, 
                0x00020004U, 
                new Func<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyArray>(IronRuby.StandardLibrary.Open3.Open3.OpenPipe)
            );
            
            #endif
        }
        
    }
}

namespace IronRuby.StandardLibrary.Win32API {
    using System;
    using Microsoft.Scripting.Utils;
    using System.Runtime.InteropServices;
    
    public sealed class Win32APILibraryInitializer : IronRuby.Builtins.LibraryInitializer {
        protected override void LoadModules() {
            IronRuby.Builtins.RubyClass classRef0 = GetClass(typeof(IronRuby.Builtins.RubyObject));
            
            
            #if !SILVERLIGHT
            DefineGlobalClass("Win32API", typeof(IronRuby.StandardLibrary.Win32API.Win32API), 0x00000008, classRef0, LoadWin32API_Instance, null, null, IronRuby.Builtins.RubyModule.EmptyArray, 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Win32API.Win32API>(IronRuby.StandardLibrary.Win32API.Win32API.Create), 
                new Func<IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubySymbol, IronRuby.StandardLibrary.Win32API.Win32API>(IronRuby.StandardLibrary.Win32API.Win32API.Create), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.Builtins.RubyClass, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Collections.IList, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Win32API.Win32API>(IronRuby.StandardLibrary.Win32API.Win32API.Create)
            );
            #endif
        }
        
        #if !SILVERLIGHT
        private static void LoadWin32API_Instance(IronRuby.Builtins.RubyModule/*!*/ module) {
            DefineRuleGenerator(module, "call", 0x11, IronRuby.StandardLibrary.Win32API.Win32API.Call());
            
            DefineRuleGenerator(module, "Call", 0x11, IronRuby.StandardLibrary.Win32API.Win32API.Call());
            
            DefineLibraryMethod(module, "initialize", 0x12, 
                0x000f001eU, 0x0016003cU, 
                new Func<IronRuby.StandardLibrary.Win32API.Win32API, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Win32API.Win32API>(IronRuby.StandardLibrary.Win32API.Win32API.Reinitialize), 
                new Func<IronRuby.Runtime.ConversionStorage<IronRuby.Builtins.MutableString>, IronRuby.StandardLibrary.Win32API.Win32API, IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, System.Collections.IList, IronRuby.Builtins.MutableString, IronRuby.StandardLibrary.Win32API.Win32API>(IronRuby.StandardLibrary.Win32API.Win32API.Reinitialize)
            );
            
        }
        #endif
        
    }
}

