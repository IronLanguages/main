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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Reflection;
using System.Diagnostics;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Interpreter;

#pragma warning disable 618 // obsolete attribute

namespace IronRuby.Compiler {
    using System;
    using Microsoft.Scripting.Utils;
    
    public static partial class Methods {
        public static MethodInfo/*!*/ AddItem { get { return _AddItem ?? (_AddItem = CallInstruction.CacheFunc<IronRuby.Builtins.RubyArray, System.Object, IronRuby.Builtins.RubyArray>(RubyOps.AddItem)); } }
        private static MethodInfo _AddItem;
        public static MethodInfo/*!*/ AddRange { get { return _AddRange ?? (_AddRange = CallInstruction.CacheFunc<IronRuby.Builtins.RubyArray, System.Collections.IList, IronRuby.Builtins.RubyArray>(RubyOps.AddRange)); } }
        private static MethodInfo _AddRange;
        public static MethodInfo/*!*/ AddSubRange { get { return _AddSubRange ?? (_AddSubRange = CallInstruction.CacheFunc<IronRuby.Builtins.RubyArray, System.Collections.IList, System.Int32, System.Int32, IronRuby.Builtins.RubyArray>(RubyOps.AddSubRange)); } }
        private static MethodInfo _AddSubRange;
        public static MethodInfo/*!*/ AliasGlobalVariable { get { return _AliasGlobalVariable ?? (_AliasGlobalVariable = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.String, System.String>(RubyOps.AliasGlobalVariable)); } }
        private static MethodInfo _AliasGlobalVariable;
        public static MethodInfo/*!*/ AliasMethod { get { return _AliasMethod ?? (_AliasMethod = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.String, System.String>(RubyOps.AliasMethod)); } }
        private static MethodInfo _AliasMethod;
        public static MethodInfo/*!*/ AllocateStructInstance { get { return _AllocateStructInstance ?? (_AllocateStructInstance = CallInstruction.CacheFunc<IronRuby.Builtins.RubyClass, IronRuby.Builtins.RubyStruct>(RubyOps.AllocateStructInstance)); } }
        private static MethodInfo _AllocateStructInstance;
        public static MethodInfo/*!*/ BlockBreak { get { return _BlockBreak ?? (_BlockBreak = CallInstruction.CacheFunc<IronRuby.Runtime.BlockParam, System.Object, System.Object>(RubyOps.BlockBreak)); } }
        private static MethodInfo _BlockBreak;
        public static MethodInfo/*!*/ BlockPropagateReturn { get { return _BlockPropagateReturn ?? (_BlockPropagateReturn = CallInstruction.CacheFunc<IronRuby.Runtime.BlockParam, System.Object, System.Object>(RubyOps.BlockPropagateReturn)); } }
        private static MethodInfo _BlockPropagateReturn;
        public static MethodInfo/*!*/ BlockRetry { get { return _BlockRetry ?? (_BlockRetry = CallInstruction.CacheFunc<IronRuby.Runtime.BlockParam, System.Object>(RubyOps.BlockRetry)); } }
        private static MethodInfo _BlockRetry;
        public static MethodInfo/*!*/ BlockReturn { get { return _BlockReturn ?? (_BlockReturn = CallInstruction.CacheFunc<IronRuby.Runtime.BlockParam, System.Object, System.Object>(RubyOps.BlockReturn)); } }
        private static MethodInfo _BlockReturn;
        public static MethodInfo/*!*/ BlockYield { get { return _BlockYield ?? (_BlockYield = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, IronRuby.Runtime.BlockParam, System.Object, System.Boolean>(RubyOps.BlockYield)); } }
        private static MethodInfo _BlockYield;
        public static MethodInfo/*!*/ CanRescue { get { return _CanRescue ?? (_CanRescue = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Exception, System.Boolean>(RubyOps.CanRescue)); } }
        private static MethodInfo _CanRescue;
        public static MethodInfo/*!*/ CompareDefaultException { get { return _CompareDefaultException ?? (_CompareDefaultException = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Boolean>(RubyOps.CompareDefaultException)); } }
        private static MethodInfo _CompareDefaultException;
        public static MethodInfo/*!*/ CompareException { get { return _CompareException ?? (_CompareException = CallInstruction.CacheFunc<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, System.Object, System.Boolean>(RubyOps.CompareException)); } }
        private static MethodInfo _CompareException;
        public static MethodInfo/*!*/ CompareSplattedExceptions { get { return _CompareSplattedExceptions ?? (_CompareSplattedExceptions = CallInstruction.CacheFunc<IronRuby.Runtime.BinaryOpStorage, IronRuby.Runtime.RubyScope, System.Collections.IList, System.Boolean>(RubyOps.CompareSplattedExceptions)); } }
        private static MethodInfo _CompareSplattedExceptions;
        public static MethodInfo/*!*/ ConvertBignumToFixnum { get { return _ConvertBignumToFixnum ?? (_ConvertBignumToFixnum = CallInstruction.CacheFunc<Microsoft.Scripting.Math.BigInteger, System.Int32>(RubyOps.ConvertBignumToFixnum)); } }
        private static MethodInfo _ConvertBignumToFixnum;
        public static MethodInfo/*!*/ ConvertBignumToFloat { get { return _ConvertBignumToFloat ?? (_ConvertBignumToFloat = CallInstruction.CacheFunc<Microsoft.Scripting.Math.BigInteger, System.Double>(RubyOps.ConvertBignumToFloat)); } }
        private static MethodInfo _ConvertBignumToFloat;
        public static MethodInfo/*!*/ ConvertDoubleToFixnum { get { return _ConvertDoubleToFixnum ?? (_ConvertDoubleToFixnum = CallInstruction.CacheFunc<System.Double, System.Int32>(RubyOps.ConvertDoubleToFixnum)); } }
        private static MethodInfo _ConvertDoubleToFixnum;
        public static MethodInfo/*!*/ ConvertMutableStringToClrString { get { return _ConvertMutableStringToClrString ?? (_ConvertMutableStringToClrString = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, System.String>(RubyOps.ConvertMutableStringToClrString)); } }
        private static MethodInfo _ConvertMutableStringToClrString;
        public static MethodInfo/*!*/ ConvertMutableStringToFloat { get { return _ConvertMutableStringToFloat ?? (_ConvertMutableStringToFloat = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, IronRuby.Builtins.MutableString, System.Double>(RubyOps.ConvertMutableStringToFloat)); } }
        private static MethodInfo _ConvertMutableStringToFloat;
        public static MethodInfo/*!*/ ConvertRubySymbolToClrString { get { return _ConvertRubySymbolToClrString ?? (_ConvertRubySymbolToClrString = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Int32, System.String>(RubyOps.ConvertRubySymbolToClrString)); } }
        private static MethodInfo _ConvertRubySymbolToClrString;
        public static MethodInfo/*!*/ ConvertStringToFloat { get { return _ConvertStringToFloat ?? (_ConvertStringToFloat = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.String, System.Double>(RubyOps.ConvertStringToFloat)); } }
        private static MethodInfo _ConvertStringToFloat;
        public static MethodInfo/*!*/ ConvertSymbolToClrString { get { return _ConvertSymbolToClrString ?? (_ConvertSymbolToClrString = CallInstruction.CacheFunc<IronRuby.Builtins.RubySymbol, System.String>(RubyOps.ConvertSymbolToClrString)); } }
        private static MethodInfo _ConvertSymbolToClrString;
        public static MethodInfo/*!*/ ConvertSymbolToMutableString { get { return _ConvertSymbolToMutableString ?? (_ConvertSymbolToMutableString = CallInstruction.CacheFunc<IronRuby.Builtins.RubySymbol, IronRuby.Builtins.MutableString>(RubyOps.ConvertSymbolToMutableString)); } }
        private static MethodInfo _ConvertSymbolToMutableString;
        public static MethodInfo/*!*/ CreateArgumentsError { get { return _CreateArgumentsError ?? (_CreateArgumentsError = CallInstruction.CacheFunc<System.String, System.ArgumentException>(RubyOps.CreateArgumentsError)); } }
        private static MethodInfo _CreateArgumentsError;
        public static MethodInfo/*!*/ CreateArgumentsErrorForMissingBlock { get { return _CreateArgumentsErrorForMissingBlock ?? (_CreateArgumentsErrorForMissingBlock = CallInstruction.CacheFunc<System.ArgumentException>(RubyOps.CreateArgumentsErrorForMissingBlock)); } }
        private static MethodInfo _CreateArgumentsErrorForMissingBlock;
        public static MethodInfo/*!*/ CreateArgumentsErrorForProc { get { return _CreateArgumentsErrorForProc ?? (_CreateArgumentsErrorForProc = CallInstruction.CacheFunc<System.String, System.ArgumentException>(RubyOps.CreateArgumentsErrorForProc)); } }
        private static MethodInfo _CreateArgumentsErrorForProc;
        public static MethodInfo/*!*/ CreateBfcForLibraryMethod { get { return _CreateBfcForLibraryMethod ?? (_CreateBfcForLibraryMethod = CallInstruction.CacheFunc<IronRuby.Builtins.Proc, IronRuby.Runtime.BlockParam>(RubyOps.CreateBfcForLibraryMethod)); } }
        private static MethodInfo _CreateBfcForLibraryMethod;
        public static MethodInfo/*!*/ CreateBfcForProcCall { get { return _CreateBfcForProcCall ?? (_CreateBfcForProcCall = CallInstruction.CacheFunc<IronRuby.Builtins.Proc, IronRuby.Runtime.BlockParam>(RubyOps.CreateBfcForProcCall)); } }
        private static MethodInfo _CreateBfcForProcCall;
        public static MethodInfo/*!*/ CreateBfcForYield { get { return _CreateBfcForYield ?? (_CreateBfcForYield = CallInstruction.CacheFunc<IronRuby.Builtins.Proc, IronRuby.Runtime.BlockParam>(RubyOps.CreateBfcForYield)); } }
        private static MethodInfo _CreateBfcForYield;
        public static MethodInfo/*!*/ CreateBlockScope { get { return _CreateBlockScope ?? (_CreateBlockScope = CallInstruction.CacheFunc<Microsoft.Scripting.MutableTuple, System.String[], IronRuby.Runtime.BlockParam, System.Object, Microsoft.Scripting.Interpreter.InterpretedFrame, IronRuby.Runtime.RubyBlockScope>(RubyOps.CreateBlockScope)); } }
        private static MethodInfo _CreateBlockScope;
        public static MethodInfo/*!*/ CreateBoundMember { get { return _CreateBoundMember ?? (_CreateBoundMember = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.Calls.RubyMemberInfo, System.String, IronRuby.Builtins.RubyMethod>(RubyOps.CreateBoundMember)); } }
        private static MethodInfo _CreateBoundMember;
        public static MethodInfo/*!*/ CreateBoundMissingMember { get { return _CreateBoundMissingMember ?? (_CreateBoundMissingMember = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.Calls.RubyMemberInfo, System.String, IronRuby.Builtins.RubyMethod>(RubyOps.CreateBoundMissingMember)); } }
        private static MethodInfo _CreateBoundMissingMember;
        public static MethodInfo/*!*/ CreateDefaultInstance { get { return _CreateDefaultInstance ?? (_CreateDefaultInstance = CallInstruction.CacheFunc<System.Object>(RubyOps.CreateDefaultInstance)); } }
        private static MethodInfo _CreateDefaultInstance;
        public static MethodInfo/*!*/ CreateDelegateFromMethod { get { return _CreateDelegateFromMethod ?? (_CreateDelegateFromMethod = CallInstruction.CacheFunc<System.Type, IronRuby.Builtins.RubyMethod, System.Delegate>(RubyOps.CreateDelegateFromMethod)); } }
        private static MethodInfo _CreateDelegateFromMethod;
        public static MethodInfo/*!*/ CreateDelegateFromProc { get { return _CreateDelegateFromProc ?? (_CreateDelegateFromProc = CallInstruction.CacheFunc<System.Type, IronRuby.Builtins.Proc, System.Delegate>(RubyOps.CreateDelegateFromProc)); } }
        private static MethodInfo _CreateDelegateFromProc;
        public static MethodInfo/*!*/ CreateEncoding { get { return _CreateEncoding ?? (_CreateEncoding = CallInstruction.CacheFunc<System.Int32, IronRuby.Builtins.RubyEncoding>(RubyOps.CreateEncoding)); } }
        private static MethodInfo _CreateEncoding;
        public static MethodInfo/*!*/ CreateEvent { get { return _CreateEvent ?? (_CreateEvent = CallInstruction.CacheFunc<IronRuby.Runtime.Calls.RubyEventInfo, System.Object, System.String, IronRuby.Builtins.RubyEvent>(RubyOps.CreateEvent)); } }
        private static MethodInfo _CreateEvent;
        public static MethodInfo/*!*/ CreateExclusiveIntegerRange { get { return _CreateExclusiveIntegerRange ?? (_CreateExclusiveIntegerRange = CallInstruction.CacheFunc<System.Int32, System.Int32, IronRuby.Builtins.Range>(RubyOps.CreateExclusiveIntegerRange)); } }
        private static MethodInfo _CreateExclusiveIntegerRange;
        public static MethodInfo/*!*/ CreateExclusiveRange { get { return _CreateExclusiveRange ?? (_CreateExclusiveRange = CallInstruction.CacheFunc<System.Object, System.Object, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.Range>(RubyOps.CreateExclusiveRange)); } }
        private static MethodInfo _CreateExclusiveRange;
        public static MethodInfo/*!*/ CreateFileInitializerScope { get { return _CreateFileInitializerScope ?? (_CreateFileInitializerScope = CallInstruction.CacheFunc<Microsoft.Scripting.MutableTuple, System.String[], IronRuby.Runtime.RubyScope, IronRuby.Runtime.RubyScope>(RubyOps.CreateFileInitializerScope)); } }
        private static MethodInfo _CreateFileInitializerScope;
        public static MethodInfo/*!*/ CreateInclusiveIntegerRange { get { return _CreateInclusiveIntegerRange ?? (_CreateInclusiveIntegerRange = CallInstruction.CacheFunc<System.Int32, System.Int32, IronRuby.Builtins.Range>(RubyOps.CreateInclusiveIntegerRange)); } }
        private static MethodInfo _CreateInclusiveIntegerRange;
        public static MethodInfo/*!*/ CreateInclusiveRange { get { return _CreateInclusiveRange ?? (_CreateInclusiveRange = CallInstruction.CacheFunc<System.Object, System.Object, IronRuby.Runtime.RubyScope, IronRuby.Runtime.BinaryOpStorage, IronRuby.Builtins.Range>(RubyOps.CreateInclusiveRange)); } }
        private static MethodInfo _CreateInclusiveRange;
        public static MethodInfo/*!*/ CreateMethodScope { get { return _CreateMethodScope ?? (_CreateMethodScope = CallInstruction.CacheFunc<Microsoft.Scripting.MutableTuple, System.String[], System.Int32, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, System.String, System.Object, IronRuby.Builtins.Proc, Microsoft.Scripting.Interpreter.InterpretedFrame, IronRuby.Runtime.RubyMethodScope>(RubyOps.CreateMethodScope)); } }
        private static MethodInfo _CreateMethodScope;
        public static MethodInfo/*!*/ CreateModuleScope { get { return _CreateModuleScope ?? (_CreateModuleScope = CallInstruction.CacheFunc<Microsoft.Scripting.MutableTuple, System.String[], IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubyModule, IronRuby.Runtime.RubyModuleScope>(RubyOps.CreateModuleScope)); } }
        private static MethodInfo _CreateModuleScope;
        public static MethodInfo/*!*/ CreateMutableStringB { get { return _CreateMutableStringB ?? (_CreateMutableStringB = CallInstruction.CacheFunc<System.Byte[], IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(RubyOps.CreateMutableStringB)); } }
        private static MethodInfo _CreateMutableStringB;
        public static MethodInfo/*!*/ CreateMutableStringL { get { return _CreateMutableStringL ?? (_CreateMutableStringL = CallInstruction.CacheFunc<System.String, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(RubyOps.CreateMutableStringL)); } }
        private static MethodInfo _CreateMutableStringL;
        public static MethodInfo/*!*/ CreateMutableStringLM { get { return _CreateMutableStringLM ?? (_CreateMutableStringLM = CallInstruction.CacheFunc<System.String, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(RubyOps.CreateMutableStringLM)); } }
        private static MethodInfo _CreateMutableStringLM;
        public static MethodInfo/*!*/ CreateMutableStringM { get { return _CreateMutableStringM ?? (_CreateMutableStringM = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(RubyOps.CreateMutableStringM)); } }
        private static MethodInfo _CreateMutableStringM;
        public static MethodInfo/*!*/ CreateMutableStringML { get { return _CreateMutableStringML ?? (_CreateMutableStringML = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, System.String, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(RubyOps.CreateMutableStringML)); } }
        private static MethodInfo _CreateMutableStringML;
        public static MethodInfo/*!*/ CreateMutableStringMM { get { return _CreateMutableStringMM ?? (_CreateMutableStringMM = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.MutableString>(RubyOps.CreateMutableStringMM)); } }
        private static MethodInfo _CreateMutableStringMM;
        public static MethodInfo/*!*/ CreateMutableStringN { get { return _CreateMutableStringN ?? (_CreateMutableStringN = CallInstruction.CacheFunc<MutableString[], IronRuby.Builtins.MutableString>(RubyOps.CreateMutableStringN)); } }
        private static MethodInfo _CreateMutableStringN;
        public static MethodInfo/*!*/ CreateRegexL { get { return _CreateRegexL ?? (_CreateRegexL = CallInstruction.CacheFunc<System.String, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyRegexOptions, System.Runtime.CompilerServices.StrongBox<IronRuby.Builtins.RubyRegex>, IronRuby.Builtins.RubyRegex>(RubyOps.CreateRegexL)); } }
        public static MethodInfo/*!*/ CreateRegexB { get { return _CreateRegexL ?? (_CreateRegexL = CallInstruction.CacheFunc<System.String, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyRegexOptions, System.Runtime.CompilerServices.StrongBox<IronRuby.Builtins.RubyRegex>, IronRuby.Builtins.RubyRegex>(RubyOps.CreateRegexL)); } }
        private static MethodInfo _CreateRegexL;
        public static MethodInfo/*!*/ CreateRegexLM { get { return _CreateRegexLM ?? (_CreateRegexLM = CallInstruction.CacheFunc<System.String, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyRegexOptions, System.Runtime.CompilerServices.StrongBox<IronRuby.Builtins.RubyRegex>, IronRuby.Builtins.RubyRegex>(RubyOps.CreateRegexLM)); } }
        private static MethodInfo _CreateRegexLM;
        public static MethodInfo/*!*/ CreateRegexM { get { return _CreateRegexM ?? (_CreateRegexM = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyRegexOptions, System.Runtime.CompilerServices.StrongBox<IronRuby.Builtins.RubyRegex>, IronRuby.Builtins.RubyRegex>(RubyOps.CreateRegexM)); } }
        private static MethodInfo _CreateRegexM;
        public static MethodInfo/*!*/ CreateRegexML { get { return _CreateRegexML ?? (_CreateRegexML = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, System.String, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyRegexOptions, System.Runtime.CompilerServices.StrongBox<IronRuby.Builtins.RubyRegex>, IronRuby.Builtins.RubyRegex>(RubyOps.CreateRegexML)); } }
        private static MethodInfo _CreateRegexML;
        public static MethodInfo/*!*/ CreateRegexMM { get { return _CreateRegexMM ?? (_CreateRegexMM = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Builtins.RubyRegexOptions, System.Runtime.CompilerServices.StrongBox<IronRuby.Builtins.RubyRegex>, IronRuby.Builtins.RubyRegex>(RubyOps.CreateRegexMM)); } }
        private static MethodInfo _CreateRegexMM;
        public static MethodInfo/*!*/ CreateRegexN { get { return _CreateRegexN ?? (_CreateRegexN = CallInstruction.CacheFunc<MutableString[], IronRuby.Builtins.RubyRegexOptions, System.Runtime.CompilerServices.StrongBox<IronRuby.Builtins.RubyRegex>, IronRuby.Builtins.RubyRegex>(RubyOps.CreateRegexN)); } }
        private static MethodInfo _CreateRegexN;
        public static MethodInfo/*!*/ CreateRfcForMethod { get { return _CreateRfcForMethod ?? (_CreateRfcForMethod = CallInstruction.CacheFunc<IronRuby.Builtins.Proc, IronRuby.Runtime.RuntimeFlowControl>(RubyOps.CreateRfcForMethod)); } }
        private static MethodInfo _CreateRfcForMethod;
        public static MethodInfo/*!*/ CreateStructInstance { get { return _CreateStructInstance ?? (_CreateStructInstance = CallInstruction.CacheFunc<IronRuby.Builtins.RubyClass, System.Object[], IronRuby.Builtins.RubyStruct>(RubyOps.CreateStructInstance)); } }
        private static MethodInfo _CreateStructInstance;
        public static MethodInfo/*!*/ CreateSymbolLM { get { return _CreateSymbolLM ?? (_CreateSymbolLM = CallInstruction.CacheFunc<System.String, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol>(RubyOps.CreateSymbolLM)); } }
        private static MethodInfo _CreateSymbolLM;
        public static MethodInfo/*!*/ CreateSymbolM { get { return _CreateSymbolM ?? (_CreateSymbolM = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol>(RubyOps.CreateSymbolM)); } }
        private static MethodInfo _CreateSymbolM;
        public static MethodInfo/*!*/ CreateSymbolML { get { return _CreateSymbolML ?? (_CreateSymbolML = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, System.String, IronRuby.Builtins.RubyEncoding, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol>(RubyOps.CreateSymbolML)); } }
        private static MethodInfo _CreateSymbolML;
        public static MethodInfo/*!*/ CreateSymbolMM { get { return _CreateSymbolMM ?? (_CreateSymbolMM = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyEncoding, IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol>(RubyOps.CreateSymbolMM)); } }
        private static MethodInfo _CreateSymbolMM;
        public static MethodInfo/*!*/ CreateSymbolN { get { return _CreateSymbolN ?? (_CreateSymbolN = CallInstruction.CacheFunc<MutableString[], IronRuby.Runtime.RubyScope, IronRuby.Builtins.RubySymbol>(RubyOps.CreateSymbolN)); } }
        private static MethodInfo _CreateSymbolN;
        public static MethodInfo/*!*/ CreateTypeConversionError { get { return _CreateTypeConversionError ?? (_CreateTypeConversionError = CallInstruction.CacheFunc<System.String, System.String, System.Exception>(RubyOps.CreateTypeConversionError)); } }
        private static MethodInfo _CreateTypeConversionError;
        public static MethodInfo/*!*/ CreateVector { get { return _CreateVector ?? (_CreateVector = GetMethod(typeof(RubyOps), "CreateVector")); } }
        private static MethodInfo _CreateVector;
        public static MethodInfo/*!*/ CreateVectorWithValues { get { return _CreateVectorWithValues ?? (_CreateVectorWithValues = GetMethod(typeof(RubyOps), "CreateVectorWithValues")); } }
        private static MethodInfo _CreateVectorWithValues;
        public static MethodInfo/*!*/ DefineBlock { get { return _DefineBlock ?? (_DefineBlock = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.Calls.BlockDispatcher, System.Object, IronRuby.Builtins.Proc>(RubyOps.DefineBlock)); } }
        private static MethodInfo _DefineBlock;
        public static MethodInfo/*!*/ DefineClass { get { return _DefineClass ?? (_DefineClass = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object, IronRuby.Builtins.RubyModule>(RubyOps.DefineClass)); } }
        private static MethodInfo _DefineClass;
        public static MethodInfo/*!*/ DefineGlobalClass { get { return _DefineGlobalClass ?? (_DefineGlobalClass = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Object, IronRuby.Builtins.RubyModule>(RubyOps.DefineGlobalClass)); } }
        private static MethodInfo _DefineGlobalClass;
        public static MethodInfo/*!*/ DefineGlobalModule { get { return _DefineGlobalModule ?? (_DefineGlobalModule = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, IronRuby.Builtins.RubyModule>(RubyOps.DefineGlobalModule)); } }
        private static MethodInfo _DefineGlobalModule;
        public static MethodInfo/*!*/ DefineLambda { get { return _DefineLambda ?? (_DefineLambda = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.Calls.BlockDispatcher, System.Object, IronRuby.Builtins.Proc>(RubyOps.DefineLambda)); } }
        private static MethodInfo _DefineLambda;
        public static MethodInfo/*!*/ DefineMethod { get { return _DefineMethod ?? (_DefineMethod = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, IronRuby.Runtime.Calls.RubyMethodBody, System.Object>(RubyOps.DefineMethod)); } }
        private static MethodInfo _DefineMethod;
        public static MethodInfo/*!*/ DefineModule { get { return _DefineModule ?? (_DefineModule = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, System.String, IronRuby.Builtins.RubyModule>(RubyOps.DefineModule)); } }
        private static MethodInfo _DefineModule;
        public static MethodInfo/*!*/ DefineNestedClass { get { return _DefineNestedClass ?? (_DefineNestedClass = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Object, IronRuby.Builtins.RubyModule>(RubyOps.DefineNestedClass)); } }
        private static MethodInfo _DefineNestedClass;
        public static MethodInfo/*!*/ DefineNestedModule { get { return _DefineNestedModule ?? (_DefineNestedModule = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, IronRuby.Builtins.RubyModule>(RubyOps.DefineNestedModule)); } }
        private static MethodInfo _DefineNestedModule;
        public static MethodInfo/*!*/ DefineSingletonClass { get { return _DefineSingletonClass ?? (_DefineSingletonClass = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Builtins.RubyClass>(RubyOps.DefineSingletonClass)); } }
        private static MethodInfo _DefineSingletonClass;
        public static MethodInfo/*!*/ DeserializeObject { get { return _DeserializeObject ?? (_DeserializeObject = GetMethod(typeof(RubyOps), "DeserializeObject")); } }
        private static MethodInfo _DeserializeObject;
        public static MethodInfo/*!*/ EnterLoop { get { return _EnterLoop ?? (_EnterLoop = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope>(RubyOps.EnterLoop)); } }
        private static MethodInfo _EnterLoop;
        public static MethodInfo/*!*/ EnterRescue { get { return _EnterRescue ?? (_EnterRescue = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope>(RubyOps.EnterRescue)); } }
        private static MethodInfo _EnterRescue;
        public static MethodInfo/*!*/ EvalBreak { get { return _EvalBreak ?? (_EvalBreak = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.Object>(RubyOps.EvalBreak)); } }
        private static MethodInfo _EvalBreak;
        public static MethodInfo/*!*/ EvalNext { get { return _EvalNext ?? (_EvalNext = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.Object>(RubyOps.EvalNext)); } }
        private static MethodInfo _EvalNext;
        public static MethodInfo/*!*/ EvalPropagateReturn { get { return _EvalPropagateReturn ?? (_EvalPropagateReturn = CallInstruction.CacheFunc<System.Object, System.Object>(RubyOps.EvalPropagateReturn)); } }
        private static MethodInfo _EvalPropagateReturn;
        public static MethodInfo/*!*/ EvalRedo { get { return _EvalRedo ?? (_EvalRedo = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope>(RubyOps.EvalRedo)); } }
        private static MethodInfo _EvalRedo;
        public static MethodInfo/*!*/ EvalRetry { get { return _EvalRetry ?? (_EvalRetry = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope>(RubyOps.EvalRetry)); } }
        private static MethodInfo _EvalRetry;
        public static MethodInfo/*!*/ EvalReturn { get { return _EvalReturn ?? (_EvalReturn = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, System.Object>(RubyOps.EvalReturn)); } }
        private static MethodInfo _EvalReturn;
        public static MethodInfo/*!*/ EvalYield { get { return _EvalYield ?? (_EvalYield = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.Boolean>(RubyOps.EvalYield)); } }
        private static MethodInfo _EvalYield;
        public static MethodInfo/*!*/ ExistsUnsplat { get { return _ExistsUnsplat ?? (_ExistsUnsplat = CallInstruction.CacheFunc<System.Object, System.Boolean>(RubyOps.ExistsUnsplat)); } }
        private static MethodInfo _ExistsUnsplat;
        public static MethodInfo/*!*/ ExistsUnsplatCompare { get { return _ExistsUnsplatCompare ?? (_ExistsUnsplatCompare = CallInstruction.CacheFunc<System.Runtime.CompilerServices.CallSite<Func<System.Runtime.CompilerServices.CallSite, System.Object, System.Object, System.Object>>, System.Object, System.Object, System.Boolean>(RubyOps.ExistsUnsplatCompare)); } }
        private static MethodInfo _ExistsUnsplatCompare;
        public static MethodInfo/*!*/ FilterBlockException { get { return _FilterBlockException ?? (_FilterBlockException = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Exception, System.Boolean>(RubyOps.FilterBlockException)); } }
        private static MethodInfo _FilterBlockException;
        public static MethodInfo/*!*/ FreezeObject { get { return _FreezeObject ?? (_FreezeObject = GetMethod(typeof(RubyOps), "FreezeObject")); } }
        private static MethodInfo _FreezeObject;
        public static MethodInfo/*!*/ GetArrayItem { get { return _GetArrayItem ?? (_GetArrayItem = CallInstruction.CacheFunc<System.Collections.IList, System.Int32, System.Object>(RubyOps.GetArrayItem)); } }
        private static MethodInfo _GetArrayItem;
        public static MethodInfo/*!*/ GetArrayRange { get { return _GetArrayRange ?? (_GetArrayRange = CallInstruction.CacheFunc<System.Collections.IList, System.Int32, System.Int32, IronRuby.Builtins.RubyArray>(RubyOps.GetArrayRange)); } }
        private static MethodInfo _GetArrayRange;
        public static MethodInfo/*!*/ GetClassVariable { get { return _GetClassVariable ?? (_GetClassVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.GetClassVariable)); } }
        private static MethodInfo _GetClassVariable;
        public static MethodInfo/*!*/ GetContextFromBlockParam { get { return _GetContextFromBlockParam ?? (_GetContextFromBlockParam = CallInstruction.CacheFunc<IronRuby.Runtime.BlockParam, IronRuby.Runtime.RubyContext>(RubyOps.GetContextFromBlockParam)); } }
        private static MethodInfo _GetContextFromBlockParam;
        public static MethodInfo/*!*/ GetContextFromIRubyObject { get { return _GetContextFromIRubyObject ?? (_GetContextFromIRubyObject = CallInstruction.CacheFunc<IronRuby.Runtime.IRubyObject, IronRuby.Runtime.RubyContext>(RubyOps.GetContextFromIRubyObject)); } }
        private static MethodInfo _GetContextFromIRubyObject;
        public static MethodInfo/*!*/ GetContextFromMethod { get { return _GetContextFromMethod ?? (_GetContextFromMethod = CallInstruction.CacheFunc<IronRuby.Builtins.RubyMethod, IronRuby.Runtime.RubyContext>(RubyOps.GetContextFromMethod)); } }
        private static MethodInfo _GetContextFromMethod;
        public static MethodInfo/*!*/ GetContextFromModule { get { return _GetContextFromModule ?? (_GetContextFromModule = CallInstruction.CacheFunc<IronRuby.Builtins.RubyModule, IronRuby.Runtime.RubyContext>(RubyOps.GetContextFromModule)); } }
        private static MethodInfo _GetContextFromModule;
        public static MethodInfo/*!*/ GetContextFromProc { get { return _GetContextFromProc ?? (_GetContextFromProc = CallInstruction.CacheFunc<IronRuby.Builtins.Proc, IronRuby.Runtime.RubyContext>(RubyOps.GetContextFromProc)); } }
        private static MethodInfo _GetContextFromProc;
        public static MethodInfo/*!*/ GetContextFromScope { get { return _GetContextFromScope ?? (_GetContextFromScope = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.RubyContext>(RubyOps.GetContextFromScope)); } }
        private static MethodInfo _GetContextFromScope;
        public static MethodInfo/*!*/ GetCurrentException { get { return _GetCurrentException ?? (_GetCurrentException = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Exception>(RubyOps.GetCurrentException)); } }
        private static MethodInfo _GetCurrentException;
        public static MethodInfo/*!*/ GetCurrentMatchData { get { return _GetCurrentMatchData ?? (_GetCurrentMatchData = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MatchData>(RubyOps.GetCurrentMatchData)); } }
        private static MethodInfo _GetCurrentMatchData;
        public static MethodInfo/*!*/ GetCurrentMatchGroup { get { return _GetCurrentMatchGroup ?? (_GetCurrentMatchGroup = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Int32, IronRuby.Builtins.MutableString>(RubyOps.GetCurrentMatchGroup)); } }
        private static MethodInfo _GetCurrentMatchGroup;
        public static MethodInfo/*!*/ GetCurrentMatchLastGroup { get { return _GetCurrentMatchLastGroup ?? (_GetCurrentMatchLastGroup = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString>(RubyOps.GetCurrentMatchLastGroup)); } }
        private static MethodInfo _GetCurrentMatchLastGroup;
        public static MethodInfo/*!*/ GetCurrentPostMatch { get { return _GetCurrentPostMatch ?? (_GetCurrentPostMatch = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString>(RubyOps.GetCurrentPostMatch)); } }
        private static MethodInfo _GetCurrentPostMatch;
        public static MethodInfo/*!*/ GetCurrentPreMatch { get { return _GetCurrentPreMatch ?? (_GetCurrentPreMatch = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString>(RubyOps.GetCurrentPreMatch)); } }
        private static MethodInfo _GetCurrentPreMatch;
        public static MethodInfo/*!*/ GetDefaultExceptionMessage { get { return _GetDefaultExceptionMessage ?? (_GetDefaultExceptionMessage = CallInstruction.CacheFunc<IronRuby.Builtins.RubyClass, System.String>(RubyOps.GetDefaultExceptionMessage)); } }
        private static MethodInfo _GetDefaultExceptionMessage;
        public static MethodInfo/*!*/ GetEmptyScope { get { return _GetEmptyScope ?? (_GetEmptyScope = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, IronRuby.Runtime.RubyScope>(RubyOps.GetEmptyScope)); } }
        private static MethodInfo _GetEmptyScope;
        public static MethodInfo/*!*/ GetExpressionQualifiedConstant { get { return _GetExpressionQualifiedConstant ?? (_GetExpressionQualifiedConstant = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, IronRuby.Runtime.ExpressionQualifiedConstantSiteCache, System.String[], System.Object>(RubyOps.GetExpressionQualifiedConstant)); } }
        private static MethodInfo _GetExpressionQualifiedConstant;
        public static MethodInfo/*!*/ GetGlobalMissingConstant { get { return _GetGlobalMissingConstant ?? (_GetGlobalMissingConstant = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.ConstantSiteCache, System.String, System.Object>(RubyOps.GetGlobalMissingConstant)); } }
        private static MethodInfo _GetGlobalMissingConstant;
        public static MethodInfo/*!*/ GetGlobalScopeFromScope { get { return _GetGlobalScopeFromScope ?? (_GetGlobalScopeFromScope = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, Microsoft.Scripting.Runtime.Scope>(RubyOps.GetGlobalScopeFromScope)); } }
        private static MethodInfo _GetGlobalScopeFromScope;
        public static MethodInfo/*!*/ GetGlobalVariable { get { return _GetGlobalVariable ?? (_GetGlobalVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.GetGlobalVariable)); } }
        private static MethodInfo _GetGlobalVariable;
        public static MethodInfo/*!*/ GetInstanceData { get { return _GetInstanceData ?? (_GetInstanceData = GetMethod(typeof(RubyOps), "GetInstanceData")); } }
        private static MethodInfo _GetInstanceData;
        public static MethodInfo/*!*/ GetInstanceVariable { get { return _GetInstanceVariable ?? (_GetInstanceVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Object>(RubyOps.GetInstanceVariable)); } }
        private static MethodInfo _GetInstanceVariable;
        public static MethodInfo/*!*/ GetLocals { get { return _GetLocals ?? (_GetLocals = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, Microsoft.Scripting.MutableTuple>(RubyOps.GetLocals)); } }
        private static MethodInfo _GetLocals;
        public static MethodInfo/*!*/ GetLocalVariable { get { return _GetLocalVariable ?? (_GetLocalVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.GetLocalVariable)); } }
        private static MethodInfo _GetLocalVariable;
        public static MethodInfo/*!*/ GetMetaObject { get { return _GetMetaObject ?? (_GetMetaObject = CallInstruction.CacheFunc<IronRuby.Runtime.IRubyObject, Expression, System.Dynamic.DynamicMetaObject>(RubyOps.GetMetaObject)); } }
        private static MethodInfo _GetMetaObject;
        public static MethodInfo/*!*/ GetMethodBlockParameter { get { return _GetMethodBlockParameter ?? (_GetMethodBlockParameter = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.Proc>(RubyOps.GetMethodBlockParameter)); } }
        private static MethodInfo _GetMethodBlockParameter;
        public static MethodInfo/*!*/ GetMethodBlockParameterSelf { get { return _GetMethodBlockParameterSelf ?? (_GetMethodBlockParameterSelf = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object>(RubyOps.GetMethodBlockParameterSelf)); } }
        private static MethodInfo _GetMethodBlockParameterSelf;
        public static MethodInfo/*!*/ GetMethodUnwinderReturnValue { get { return _GetMethodUnwinderReturnValue ?? (_GetMethodUnwinderReturnValue = CallInstruction.CacheFunc<System.Exception, System.Object>(RubyOps.GetMethodUnwinderReturnValue)); } }
        private static MethodInfo _GetMethodUnwinderReturnValue;
        public static MethodInfo/*!*/ GetMissingConstant { get { return _GetMissingConstant ?? (_GetMissingConstant = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.ConstantSiteCache, System.String, System.Object>(RubyOps.GetMissingConstant)); } }
        private static MethodInfo _GetMissingConstant;
        public static MethodInfo/*!*/ GetMutableStringBytes { get { return _GetMutableStringBytes ?? (_GetMutableStringBytes = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, System.Byte[]>(RubyOps.GetMutableStringBytes)); } }
        private static MethodInfo _GetMutableStringBytes;
        public static MethodInfo/*!*/ GetParentLocals { get { return _GetParentLocals ?? (_GetParentLocals = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, Microsoft.Scripting.MutableTuple>(RubyOps.GetParentLocals)); } }
        private static MethodInfo _GetParentLocals;
        public static MethodInfo/*!*/ GetParentScope { get { return _GetParentScope ?? (_GetParentScope = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.RubyScope>(RubyOps.GetParentScope)); } }
        private static MethodInfo _GetParentScope;
        public static MethodInfo/*!*/ GetProcArity { get { return _GetProcArity ?? (_GetProcArity = CallInstruction.CacheFunc<IronRuby.Builtins.Proc, System.Int32>(RubyOps.GetProcArity)); } }
        private static MethodInfo _GetProcArity;
        public static MethodInfo/*!*/ GetProcSelf { get { return _GetProcSelf ?? (_GetProcSelf = CallInstruction.CacheFunc<IronRuby.Builtins.Proc, System.Object>(RubyOps.GetProcSelf)); } }
        private static MethodInfo _GetProcSelf;
        public static MethodInfo/*!*/ GetQualifiedConstant { get { return _GetQualifiedConstant ?? (_GetQualifiedConstant = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.ConstantSiteCache, System.String[], System.Boolean, System.Object>(RubyOps.GetQualifiedConstant)); } }
        private static MethodInfo _GetQualifiedConstant;
        public static MethodInfo/*!*/ GetSelfClassVersionHandle { get { return _GetSelfClassVersionHandle ?? (_GetSelfClassVersionHandle = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.Calls.VersionHandle>(RubyOps.GetSelfClassVersionHandle)); } }
        private static MethodInfo _GetSelfClassVersionHandle;
        public static MethodInfo/*!*/ GetSuperCallTarget { get { return _GetSuperCallTarget ?? (_GetSuperCallTarget = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Int32, System.Object>(RubyOps.GetSuperCallTarget)); } }
        private static MethodInfo _GetSuperCallTarget;
        public static MethodInfo/*!*/ GetTrailingArrayItem { get { return _GetTrailingArrayItem ?? (_GetTrailingArrayItem = CallInstruction.CacheFunc<System.Collections.IList, System.Int32, System.Int32, System.Object>(RubyOps.GetTrailingArrayItem)); } }
        private static MethodInfo _GetTrailingArrayItem;
        public static MethodInfo/*!*/ GetUnqualifiedConstant { get { return _GetUnqualifiedConstant ?? (_GetUnqualifiedConstant = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.ConstantSiteCache, System.String, System.Boolean, System.Object>(RubyOps.GetUnqualifiedConstant)); } }
        private static MethodInfo _GetUnqualifiedConstant;
        public static MethodInfo/*!*/ HookupEvent { get { return _HookupEvent ?? (_HookupEvent = CallInstruction.CacheFunc<IronRuby.Runtime.Calls.RubyEventInfo, System.Object, IronRuby.Builtins.Proc, IronRuby.Builtins.Proc>(RubyOps.HookupEvent)); } }
        private static MethodInfo _HookupEvent;
        public static MethodInfo/*!*/ InitializeBlock { get { return _InitializeBlock ?? (_InitializeBlock = CallInstruction.CacheAction<IronRuby.Builtins.Proc>(RubyOps.InitializeBlock)); } }
        private static MethodInfo _InitializeBlock;
        public static MethodInfo/*!*/ InitializeScope { get { return _InitializeScope ?? (_InitializeScope = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, Microsoft.Scripting.MutableTuple, System.String[], Microsoft.Scripting.Interpreter.InterpretedFrame>(RubyOps.InitializeScope)); } }
        private static MethodInfo _InitializeScope;
        public static MethodInfo/*!*/ InitializeScopeNoLocals { get { return _InitializeScopeNoLocals ?? (_InitializeScopeNoLocals = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, Microsoft.Scripting.Interpreter.InterpretedFrame>(RubyOps.InitializeScopeNoLocals)); } }
        private static MethodInfo _InitializeScopeNoLocals;
        public static MethodInfo/*!*/ InstantiateBlock { get { return _InstantiateBlock ?? (_InstantiateBlock = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.Calls.BlockDispatcher, IronRuby.Builtins.Proc>(RubyOps.InstantiateBlock)); } }
        private static MethodInfo _InstantiateBlock;
        public static MethodInfo/*!*/ InstantiateLambda { get { return _InstantiateLambda ?? (_InstantiateLambda = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, IronRuby.Runtime.Calls.BlockDispatcher, IronRuby.Builtins.Proc>(RubyOps.InstantiateLambda)); } }
        private static MethodInfo _InstantiateLambda;
        public static MethodInfo/*!*/ IntegerValue_ToUInt32Unchecked { get { return _IntegerValue_ToUInt32Unchecked ?? (_IntegerValue_ToUInt32Unchecked = GetMethod(typeof(IntegerValue), "ToUInt32Unchecked")); } }
        private static MethodInfo _IntegerValue_ToUInt32Unchecked;
        public static MethodInfo/*!*/ IRubyObject_BaseEquals { get { return _IRubyObject_BaseEquals ?? (_IRubyObject_BaseEquals = GetMethod(typeof(IRubyObject), "BaseEquals")); } }
        private static MethodInfo _IRubyObject_BaseEquals;
        public static MethodInfo/*!*/ IRubyObject_BaseGetHashCode { get { return _IRubyObject_BaseGetHashCode ?? (_IRubyObject_BaseGetHashCode = GetMethod(typeof(IRubyObject), "BaseGetHashCode")); } }
        private static MethodInfo _IRubyObject_BaseGetHashCode;
        public static MethodInfo/*!*/ IRubyObject_BaseToString { get { return _IRubyObject_BaseToString ?? (_IRubyObject_BaseToString = GetMethod(typeof(IRubyObject), "BaseToString")); } }
        private static MethodInfo _IRubyObject_BaseToString;
        public static MethodInfo/*!*/ IRubyObject_get_ImmediateClass { get { return _IRubyObject_get_ImmediateClass ?? (_IRubyObject_get_ImmediateClass = GetMethod(typeof(IRubyObject), "get_ImmediateClass")); } }
        private static MethodInfo _IRubyObject_get_ImmediateClass;
        public static MethodInfo/*!*/ IRubyObject_GetInstanceData { get { return _IRubyObject_GetInstanceData ?? (_IRubyObject_GetInstanceData = GetMethod(typeof(IRubyObject), "GetInstanceData")); } }
        private static MethodInfo _IRubyObject_GetInstanceData;
        public static MethodInfo/*!*/ IRubyObject_set_ImmediateClass { get { return _IRubyObject_set_ImmediateClass ?? (_IRubyObject_set_ImmediateClass = GetMethod(typeof(IRubyObject), "set_ImmediateClass")); } }
        private static MethodInfo _IRubyObject_set_ImmediateClass;
        public static MethodInfo/*!*/ IRubyObject_TryGetInstanceData { get { return _IRubyObject_TryGetInstanceData ?? (_IRubyObject_TryGetInstanceData = GetMethod(typeof(IRubyObject), "TryGetInstanceData")); } }
        private static MethodInfo _IRubyObject_TryGetInstanceData;
        public static MethodInfo/*!*/ IRubyObjectState_Freeze { get { return _IRubyObjectState_Freeze ?? (_IRubyObjectState_Freeze = GetMethod(typeof(IRubyObjectState), "Freeze")); } }
        private static MethodInfo _IRubyObjectState_Freeze;
        public static MethodInfo/*!*/ IRubyObjectState_get_IsFrozen { get { return _IRubyObjectState_get_IsFrozen ?? (_IRubyObjectState_get_IsFrozen = GetMethod(typeof(IRubyObjectState), "get_IsFrozen")); } }
        private static MethodInfo _IRubyObjectState_get_IsFrozen;
        public static MethodInfo/*!*/ IRubyObjectState_get_IsTainted { get { return _IRubyObjectState_get_IsTainted ?? (_IRubyObjectState_get_IsTainted = GetMethod(typeof(IRubyObjectState), "get_IsTainted")); } }
        private static MethodInfo _IRubyObjectState_get_IsTainted;
        public static MethodInfo/*!*/ IRubyObjectState_get_IsUntrusted { get { return _IRubyObjectState_get_IsUntrusted ?? (_IRubyObjectState_get_IsUntrusted = GetMethod(typeof(IRubyObjectState), "get_IsUntrusted")); } }
        private static MethodInfo _IRubyObjectState_get_IsUntrusted;
        public static MethodInfo/*!*/ IRubyObjectState_set_IsTainted { get { return _IRubyObjectState_set_IsTainted ?? (_IRubyObjectState_set_IsTainted = GetMethod(typeof(IRubyObjectState), "set_IsTainted")); } }
        private static MethodInfo _IRubyObjectState_set_IsTainted;
        public static MethodInfo/*!*/ IRubyObjectState_set_IsUntrusted { get { return _IRubyObjectState_set_IsUntrusted ?? (_IRubyObjectState_set_IsUntrusted = GetMethod(typeof(IRubyObjectState), "set_IsUntrusted")); } }
        private static MethodInfo _IRubyObjectState_set_IsUntrusted;
        public static MethodInfo/*!*/ IsClrNonSingletonRuleValid { get { return _IsClrNonSingletonRuleValid ?? (_IsClrNonSingletonRuleValid = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, IronRuby.Runtime.Calls.VersionHandle, System.Int32, System.Boolean>(RubyOps.IsClrNonSingletonRuleValid)); } }
        private static MethodInfo _IsClrNonSingletonRuleValid;
        public static MethodInfo/*!*/ IsClrSingletonRuleValid { get { return _IsClrSingletonRuleValid ?? (_IsClrSingletonRuleValid = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.Int32, System.Boolean>(RubyOps.IsClrSingletonRuleValid)); } }
        private static MethodInfo _IsClrSingletonRuleValid;
        public static MethodInfo/*!*/ IsDefinedClassVariable { get { return _IsDefinedClassVariable ?? (_IsDefinedClassVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Boolean>(RubyOps.IsDefinedClassVariable)); } }
        private static MethodInfo _IsDefinedClassVariable;
        public static MethodInfo/*!*/ IsDefinedExpressionQualifiedConstant { get { return _IsDefinedExpressionQualifiedConstant ?? (_IsDefinedExpressionQualifiedConstant = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, IronRuby.Runtime.ExpressionQualifiedIsDefinedConstantSiteCache, System.String[], System.Boolean>(RubyOps.IsDefinedExpressionQualifiedConstant)); } }
        private static MethodInfo _IsDefinedExpressionQualifiedConstant;
        public static MethodInfo/*!*/ IsDefinedGlobalConstant { get { return _IsDefinedGlobalConstant ?? (_IsDefinedGlobalConstant = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.IsDefinedConstantSiteCache, System.String, System.Boolean>(RubyOps.IsDefinedGlobalConstant)); } }
        private static MethodInfo _IsDefinedGlobalConstant;
        public static MethodInfo/*!*/ IsDefinedGlobalVariable { get { return _IsDefinedGlobalVariable ?? (_IsDefinedGlobalVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Boolean>(RubyOps.IsDefinedGlobalVariable)); } }
        private static MethodInfo _IsDefinedGlobalVariable;
        public static MethodInfo/*!*/ IsDefinedInstanceVariable { get { return _IsDefinedInstanceVariable ?? (_IsDefinedInstanceVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object, System.String, System.Boolean>(RubyOps.IsDefinedInstanceVariable)); } }
        private static MethodInfo _IsDefinedInstanceVariable;
        public static MethodInfo/*!*/ IsDefinedQualifiedConstant { get { return _IsDefinedQualifiedConstant ?? (_IsDefinedQualifiedConstant = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.IsDefinedConstantSiteCache, System.String[], System.Boolean, System.Boolean>(RubyOps.IsDefinedQualifiedConstant)); } }
        private static MethodInfo _IsDefinedQualifiedConstant;
        public static MethodInfo/*!*/ IsDefinedUnqualifiedConstant { get { return _IsDefinedUnqualifiedConstant ?? (_IsDefinedUnqualifiedConstant = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.IsDefinedConstantSiteCache, System.String, System.Boolean>(RubyOps.IsDefinedUnqualifiedConstant)); } }
        private static MethodInfo _IsDefinedUnqualifiedConstant;
        public static MethodInfo/*!*/ IsFalse { get { return _IsFalse ?? (_IsFalse = CallInstruction.CacheFunc<System.Object, System.Boolean>(RubyOps.IsFalse)); } }
        private static MethodInfo _IsFalse;
        public static MethodInfo/*!*/ IsMethodUnwinderTargetFrame { get { return _IsMethodUnwinderTargetFrame ?? (_IsMethodUnwinderTargetFrame = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Exception, System.Boolean>(RubyOps.IsMethodUnwinderTargetFrame)); } }
        private static MethodInfo _IsMethodUnwinderTargetFrame;
        public static MethodInfo/*!*/ IsObjectFrozen { get { return _IsObjectFrozen ?? (_IsObjectFrozen = CallInstruction.CacheFunc<IronRuby.Runtime.RubyInstanceData, System.Boolean>(RubyOps.IsObjectFrozen)); } }
        private static MethodInfo _IsObjectFrozen;
        public static MethodInfo/*!*/ IsObjectTainted { get { return _IsObjectTainted ?? (_IsObjectTainted = CallInstruction.CacheFunc<IronRuby.Runtime.RubyInstanceData, System.Boolean>(RubyOps.IsObjectTainted)); } }
        private static MethodInfo _IsObjectTainted;
        public static MethodInfo/*!*/ IsObjectUntrusted { get { return _IsObjectUntrusted ?? (_IsObjectUntrusted = CallInstruction.CacheFunc<IronRuby.Runtime.RubyInstanceData, System.Boolean>(RubyOps.IsObjectUntrusted)); } }
        private static MethodInfo _IsObjectUntrusted;
        public static MethodInfo/*!*/ IsProcConverterTarget { get { return _IsProcConverterTarget ?? (_IsProcConverterTarget = CallInstruction.CacheFunc<IronRuby.Runtime.BlockParam, IronRuby.Runtime.MethodUnwinder, System.Boolean>(RubyOps.IsProcConverterTarget)); } }
        private static MethodInfo _IsProcConverterTarget;
        public static MethodInfo/*!*/ IsRetrySingleton { get { return _IsRetrySingleton ?? (_IsRetrySingleton = CallInstruction.CacheFunc<System.Object, System.Boolean>(RubyOps.IsRetrySingleton)); } }
        private static MethodInfo _IsRetrySingleton;
        public static MethodInfo/*!*/ IsSuperOutOfMethodScope { get { return _IsSuperOutOfMethodScope ?? (_IsSuperOutOfMethodScope = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Boolean>(RubyOps.IsSuperOutOfMethodScope)); } }
        private static MethodInfo _IsSuperOutOfMethodScope;
        public static MethodInfo/*!*/ IsTrue { get { return _IsTrue ?? (_IsTrue = CallInstruction.CacheFunc<System.Object, System.Boolean>(RubyOps.IsTrue)); } }
        private static MethodInfo _IsTrue;
        public static MethodInfo/*!*/ LeaveLoop { get { return _LeaveLoop ?? (_LeaveLoop = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope>(RubyOps.LeaveLoop)); } }
        private static MethodInfo _LeaveLoop;
        public static MethodInfo/*!*/ LeaveMethodFrame { get { return _LeaveMethodFrame ?? (_LeaveMethodFrame = CallInstruction.CacheAction<IronRuby.Runtime.RuntimeFlowControl>(RubyOps.LeaveMethodFrame)); } }
        private static MethodInfo _LeaveMethodFrame;
        public static MethodInfo/*!*/ LeaveProcConverter { get { return _LeaveProcConverter ?? (_LeaveProcConverter = CallInstruction.CacheAction<IronRuby.Runtime.BlockParam>(RubyOps.LeaveProcConverter)); } }
        private static MethodInfo _LeaveProcConverter;
        public static MethodInfo/*!*/ LeaveRescue { get { return _LeaveRescue ?? (_LeaveRescue = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope>(RubyOps.LeaveRescue)); } }
        private static MethodInfo _LeaveRescue;
        public static MethodInfo/*!*/ MakeAbstractMethodCalledError { get { return _MakeAbstractMethodCalledError ?? (_MakeAbstractMethodCalledError = CallInstruction.CacheFunc<System.RuntimeMethodHandle, System.Exception>(RubyOps.MakeAbstractMethodCalledError)); } }
        private static MethodInfo _MakeAbstractMethodCalledError;
        public static MethodInfo/*!*/ MakeAllocatorUndefinedError { get { return _MakeAllocatorUndefinedError ?? (_MakeAllocatorUndefinedError = CallInstruction.CacheFunc<IronRuby.Builtins.RubyClass, System.Exception>(RubyOps.MakeAllocatorUndefinedError)); } }
        private static MethodInfo _MakeAllocatorUndefinedError;
        public static MethodInfo/*!*/ MakeAmbiguousMatchError { get { return _MakeAmbiguousMatchError ?? (_MakeAmbiguousMatchError = CallInstruction.CacheFunc<System.String, System.Exception>(RubyOps.MakeAmbiguousMatchError)); } }
        private static MethodInfo _MakeAmbiguousMatchError;
        public static MethodInfo/*!*/ MakeArray0 { get { return _MakeArray0 ?? (_MakeArray0 = CallInstruction.CacheFunc<IronRuby.Builtins.RubyArray>(RubyOps.MakeArray0)); } }
        private static MethodInfo _MakeArray0;
        public static MethodInfo/*!*/ MakeArray1 { get { return _MakeArray1 ?? (_MakeArray1 = CallInstruction.CacheFunc<System.Object, IronRuby.Builtins.RubyArray>(RubyOps.MakeArray1)); } }
        private static MethodInfo _MakeArray1;
        public static MethodInfo/*!*/ MakeArray2 { get { return _MakeArray2 ?? (_MakeArray2 = CallInstruction.CacheFunc<System.Object, System.Object, IronRuby.Builtins.RubyArray>(RubyOps.MakeArray2)); } }
        private static MethodInfo _MakeArray2;
        public static MethodInfo/*!*/ MakeArray3 { get { return _MakeArray3 ?? (_MakeArray3 = CallInstruction.CacheFunc<System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(RubyOps.MakeArray3)); } }
        private static MethodInfo _MakeArray3;
        public static MethodInfo/*!*/ MakeArray4 { get { return _MakeArray4 ?? (_MakeArray4 = CallInstruction.CacheFunc<System.Object, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(RubyOps.MakeArray4)); } }
        private static MethodInfo _MakeArray4;
        public static MethodInfo/*!*/ MakeArray5 { get { return _MakeArray5 ?? (_MakeArray5 = CallInstruction.CacheFunc<System.Object, System.Object, System.Object, System.Object, System.Object, IronRuby.Builtins.RubyArray>(RubyOps.MakeArray5)); } }
        private static MethodInfo _MakeArray5;
        public static MethodInfo/*!*/ MakeArrayN { get { return _MakeArrayN ?? (_MakeArrayN = CallInstruction.CacheFunc<System.Object[], IronRuby.Builtins.RubyArray>(RubyOps.MakeArrayN)); } }
        private static MethodInfo _MakeArrayN;
        public static MethodInfo/*!*/ MakeClrProtectedMethodCalledError { get { return _MakeClrProtectedMethodCalledError ?? (_MakeClrProtectedMethodCalledError = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Exception>(RubyOps.MakeClrProtectedMethodCalledError)); } }
        private static MethodInfo _MakeClrProtectedMethodCalledError;
        public static MethodInfo/*!*/ MakeClrVirtualMethodCalledError { get { return _MakeClrVirtualMethodCalledError ?? (_MakeClrVirtualMethodCalledError = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Exception>(RubyOps.MakeClrVirtualMethodCalledError)); } }
        private static MethodInfo _MakeClrVirtualMethodCalledError;
        public static MethodInfo/*!*/ MakeConstructorUndefinedError { get { return _MakeConstructorUndefinedError ?? (_MakeConstructorUndefinedError = CallInstruction.CacheFunc<IronRuby.Builtins.RubyClass, System.Exception>(RubyOps.MakeConstructorUndefinedError)); } }
        private static MethodInfo _MakeConstructorUndefinedError;
        public static MethodInfo/*!*/ MakeHash { get { return _MakeHash ?? (_MakeHash = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Object[], IronRuby.Builtins.Hash>(RubyOps.MakeHash)); } }
        private static MethodInfo _MakeHash;
        public static MethodInfo/*!*/ MakeHash0 { get { return _MakeHash0 ?? (_MakeHash0 = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.Hash>(RubyOps.MakeHash0)); } }
        private static MethodInfo _MakeHash0;
        public static MethodInfo/*!*/ MakeImplicitSuperInBlockMethodError { get { return _MakeImplicitSuperInBlockMethodError ?? (_MakeImplicitSuperInBlockMethodError = CallInstruction.CacheFunc<System.Exception>(RubyOps.MakeImplicitSuperInBlockMethodError)); } }
        private static MethodInfo _MakeImplicitSuperInBlockMethodError;
        public static MethodInfo/*!*/ MakeInvalidArgumentTypesError { get { return _MakeInvalidArgumentTypesError ?? (_MakeInvalidArgumentTypesError = CallInstruction.CacheFunc<System.String, System.Exception>(RubyOps.MakeInvalidArgumentTypesError)); } }
        private static MethodInfo _MakeInvalidArgumentTypesError;
        public static MethodInfo/*!*/ MakeMissingDefaultConstructorError { get { return _MakeMissingDefaultConstructorError ?? (_MakeMissingDefaultConstructorError = CallInstruction.CacheFunc<IronRuby.Builtins.RubyClass, System.String, System.Exception>(RubyOps.MakeMissingDefaultConstructorError)); } }
        private static MethodInfo _MakeMissingDefaultConstructorError;
        public static MethodInfo/*!*/ MakeMissingMemberError { get { return _MakeMissingMemberError ?? (_MakeMissingMemberError = CallInstruction.CacheFunc<System.String, System.Exception>(RubyOps.MakeMissingMemberError)); } }
        private static MethodInfo _MakeMissingMemberError;
        public static MethodInfo/*!*/ MakeMissingMethodError { get { return _MakeMissingMethodError ?? (_MakeMissingMethodError = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Exception>(RubyOps.MakeMissingMethodError)); } }
        private static MethodInfo _MakeMissingMethodError;
        public static MethodInfo/*!*/ MakeMissingSuperException { get { return _MakeMissingSuperException ?? (_MakeMissingSuperException = CallInstruction.CacheFunc<System.String, System.Exception>(RubyOps.MakeMissingSuperException)); } }
        private static MethodInfo _MakeMissingSuperException;
        public static MethodInfo/*!*/ MakeNotClrTypeError { get { return _MakeNotClrTypeError ?? (_MakeNotClrTypeError = CallInstruction.CacheFunc<IronRuby.Builtins.RubyClass, System.Exception>(RubyOps.MakeNotClrTypeError)); } }
        private static MethodInfo _MakeNotClrTypeError;
        public static MethodInfo/*!*/ MakePrivateMethodCalledError { get { return _MakePrivateMethodCalledError ?? (_MakePrivateMethodCalledError = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Exception>(RubyOps.MakePrivateMethodCalledError)); } }
        private static MethodInfo _MakePrivateMethodCalledError;
        public static MethodInfo/*!*/ MakeProtectedMethodCalledError { get { return _MakeProtectedMethodCalledError ?? (_MakeProtectedMethodCalledError = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.String, System.Exception>(RubyOps.MakeProtectedMethodCalledError)); } }
        private static MethodInfo _MakeProtectedMethodCalledError;
        public static MethodInfo/*!*/ MakeTopLevelSuperException { get { return _MakeTopLevelSuperException ?? (_MakeTopLevelSuperException = CallInstruction.CacheFunc<System.Exception>(RubyOps.MakeTopLevelSuperException)); } }
        private static MethodInfo _MakeTopLevelSuperException;
        public static MethodInfo/*!*/ MakeTypeConversionError { get { return _MakeTypeConversionError ?? (_MakeTypeConversionError = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.Type, System.Exception>(RubyOps.MakeTypeConversionError)); } }
        private static MethodInfo _MakeTypeConversionError;
        public static MethodInfo/*!*/ MakeVirtualClassInstantiatedError { get { return _MakeVirtualClassInstantiatedError ?? (_MakeVirtualClassInstantiatedError = CallInstruction.CacheFunc<System.Exception>(RubyOps.MakeVirtualClassInstantiatedError)); } }
        private static MethodInfo _MakeVirtualClassInstantiatedError;
        public static MethodInfo/*!*/ MakeWrongNumberOfArgumentsError { get { return _MakeWrongNumberOfArgumentsError ?? (_MakeWrongNumberOfArgumentsError = CallInstruction.CacheFunc<System.Int32, System.Int32, System.ArgumentException>(RubyOps.MakeWrongNumberOfArgumentsError)); } }
        private static MethodInfo _MakeWrongNumberOfArgumentsError;
        public static MethodInfo/*!*/ MarkException { get { return _MarkException ?? (_MarkException = CallInstruction.CacheFunc<System.Exception, System.Exception>(RubyOps.MarkException)); } }
        private static MethodInfo _MarkException;
        public static MethodInfo/*!*/ MatchLastInputLine { get { return _MatchLastInputLine ?? (_MatchLastInputLine = CallInstruction.CacheFunc<IronRuby.Builtins.RubyRegex, IronRuby.Runtime.RubyScope, System.Boolean>(RubyOps.MatchLastInputLine)); } }
        private static MethodInfo _MatchLastInputLine;
        public static MethodInfo/*!*/ MatchString { get { return _MatchString ?? (_MatchString = CallInstruction.CacheFunc<IronRuby.Builtins.MutableString, IronRuby.Builtins.RubyRegex, IronRuby.Runtime.RubyScope, System.Object>(RubyOps.MatchString)); } }
        private static MethodInfo _MatchString;
        public static MethodInfo/*!*/ MethodBreak { get { return _MethodBreak ?? (_MethodBreak = CallInstruction.CacheAction<System.Object>(RubyOps.MethodBreak)); } }
        private static MethodInfo _MethodBreak;
        public static MethodInfo/*!*/ MethodNext { get { return _MethodNext ?? (_MethodNext = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.Object>(RubyOps.MethodNext)); } }
        private static MethodInfo _MethodNext;
        public static MethodInfo/*!*/ MethodProcCall { get { return _MethodProcCall ?? (_MethodProcCall = CallInstruction.CacheFunc<IronRuby.Runtime.BlockParam, System.Object, System.Object>(RubyOps.MethodProcCall)); } }
        private static MethodInfo _MethodProcCall;
        public static MethodInfo/*!*/ MethodPropagateReturn { get { return _MethodPropagateReturn ?? (_MethodPropagateReturn = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.Proc, IronRuby.Runtime.BlockReturnResult, System.Object>(RubyOps.MethodPropagateReturn)); } }
        private static MethodInfo _MethodPropagateReturn;
        public static MethodInfo/*!*/ MethodRedo { get { return _MethodRedo ?? (_MethodRedo = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope>(RubyOps.MethodRedo)); } }
        private static MethodInfo _MethodRedo;
        public static MethodInfo/*!*/ MethodRetry { get { return _MethodRetry ?? (_MethodRetry = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Builtins.Proc, System.Object>(RubyOps.MethodRetry)); } }
        private static MethodInfo _MethodRetry;
        public static MethodInfo/*!*/ MethodYield { get { return _MethodYield ?? (_MethodYield = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, IronRuby.Runtime.BlockParam, System.Object, System.Boolean>(RubyOps.MethodYield)); } }
        private static MethodInfo _MethodYield;
        public static MethodInfo/*!*/ NullIfFalse { get { return _NullIfFalse ?? (_NullIfFalse = CallInstruction.CacheFunc<System.Object, System.Object>(RubyOps.NullIfFalse)); } }
        private static MethodInfo _NullIfFalse;
        public static MethodInfo/*!*/ NullIfTrue { get { return _NullIfTrue ?? (_NullIfTrue = CallInstruction.CacheFunc<System.Object, System.Object>(RubyOps.NullIfTrue)); } }
        private static MethodInfo _NullIfTrue;
        public static MethodInfo/*!*/ ObjectToMutableString { get { return _ObjectToMutableString ?? (_ObjectToMutableString = CallInstruction.CacheFunc<System.Object, IronRuby.Builtins.MutableString>(RubyOps.ObjectToMutableString)); } }
        private static MethodInfo _ObjectToMutableString;
        public static MethodInfo/*!*/ ObjectToString { get { return _ObjectToString ?? (_ObjectToString = CallInstruction.CacheFunc<IronRuby.Runtime.IRubyObject, System.String>(RubyOps.ObjectToString)); } }
        private static MethodInfo _ObjectToString;
        public static MethodInfo/*!*/ PrintInteractiveResult { get { return _PrintInteractiveResult ?? (_PrintInteractiveResult = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, IronRuby.Builtins.MutableString>(RubyOps.PrintInteractiveResult)); } }
        private static MethodInfo _PrintInteractiveResult;
        public static MethodInfo/*!*/ PropagateRetrySingleton { get { return _PropagateRetrySingleton ?? (_PropagateRetrySingleton = CallInstruction.CacheFunc<System.Object, System.Object, System.Object>(RubyOps.PropagateRetrySingleton)); } }
        private static MethodInfo _PropagateRetrySingleton;
        public static MethodInfo/*!*/ RegisterShutdownHandler { get { return _RegisterShutdownHandler ?? (_RegisterShutdownHandler = CallInstruction.CacheAction<IronRuby.Builtins.Proc>(RubyOps.RegisterShutdownHandler)); } }
        private static MethodInfo _RegisterShutdownHandler;
        public static MethodInfo/*!*/ RubyStruct_GetValue { get { return _RubyStruct_GetValue ?? (_RubyStruct_GetValue = CallInstruction.CacheFunc<IronRuby.Builtins.RubyStruct, System.Int32, System.Object>(RubyStruct.GetValue)); } }
        private static MethodInfo _RubyStruct_GetValue;
        public static MethodInfo/*!*/ RubyStruct_SetValue { get { return _RubyStruct_SetValue ?? (_RubyStruct_SetValue = CallInstruction.CacheFunc<IronRuby.Builtins.RubyStruct, System.Int32, System.Object, System.Object>(RubyStruct.SetValue)); } }
        private static MethodInfo _RubyStruct_SetValue;
        public static MethodInfo/*!*/ SerializeObject { get { return _SerializeObject ?? (_SerializeObject = GetMethod(typeof(RubyOps), "SerializeObject")); } }
        private static MethodInfo _SerializeObject;
        public static MethodInfo/*!*/ SetClassVariable { get { return _SetClassVariable ?? (_SetClassVariable = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.SetClassVariable)); } }
        private static MethodInfo _SetClassVariable;
        public static MethodInfo/*!*/ SetCurrentException { get { return _SetCurrentException ?? (_SetCurrentException = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.Exception>(RubyOps.SetCurrentException)); } }
        private static MethodInfo _SetCurrentException;
        public static MethodInfo/*!*/ SetDataConstant { get { return _SetDataConstant ?? (_SetDataConstant = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.String, System.Int32>(RubyOps.SetDataConstant)); } }
        private static MethodInfo _SetDataConstant;
        public static MethodInfo/*!*/ SetGlobalConstant { get { return _SetGlobalConstant ?? (_SetGlobalConstant = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.SetGlobalConstant)); } }
        private static MethodInfo _SetGlobalConstant;
        public static MethodInfo/*!*/ SetGlobalVariable { get { return _SetGlobalVariable ?? (_SetGlobalVariable = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.SetGlobalVariable)); } }
        private static MethodInfo _SetGlobalVariable;
        public static MethodInfo/*!*/ SetInstanceVariable { get { return _SetInstanceVariable ?? (_SetInstanceVariable = CallInstruction.CacheFunc<System.Object, System.Object, IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.SetInstanceVariable)); } }
        private static MethodInfo _SetInstanceVariable;
        public static MethodInfo/*!*/ SetLocalVariable { get { return _SetLocalVariable ?? (_SetLocalVariable = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.SetLocalVariable)); } }
        private static MethodInfo _SetLocalVariable;
        public static MethodInfo/*!*/ SetObjectTaint { get { return _SetObjectTaint ?? (_SetObjectTaint = GetMethod(typeof(RubyOps), "SetObjectTaint")); } }
        private static MethodInfo _SetObjectTaint;
        public static MethodInfo/*!*/ SetObjectTrustiness { get { return _SetObjectTrustiness ?? (_SetObjectTrustiness = GetMethod(typeof(RubyOps), "SetObjectTrustiness")); } }
        private static MethodInfo _SetObjectTrustiness;
        public static MethodInfo/*!*/ SetQualifiedConstant { get { return _SetQualifiedConstant ?? (_SetQualifiedConstant = CallInstruction.CacheFunc<System.Object, System.Object, IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.SetQualifiedConstant)); } }
        private static MethodInfo _SetQualifiedConstant;
        public static MethodInfo/*!*/ SetUnqualifiedConstant { get { return _SetUnqualifiedConstant ?? (_SetUnqualifiedConstant = CallInstruction.CacheFunc<System.Object, IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.SetUnqualifiedConstant)); } }
        private static MethodInfo _SetUnqualifiedConstant;
        public static MethodInfo/*!*/ Splat { get { return _Splat ?? (_Splat = CallInstruction.CacheFunc<System.Collections.IList, System.Object>(RubyOps.Splat)); } }
        private static MethodInfo _Splat;
        public static MethodInfo/*!*/ SplatAppend { get { return _SplatAppend ?? (_SplatAppend = CallInstruction.CacheFunc<System.Collections.IList, System.Collections.IList, System.Collections.IList>(RubyOps.SplatAppend)); } }
        private static MethodInfo _SplatAppend;
        public static MethodInfo/*!*/ SplatPair { get { return _SplatPair ?? (_SplatPair = CallInstruction.CacheFunc<System.Object, System.Collections.IList, System.Object>(RubyOps.SplatPair)); } }
        private static MethodInfo _SplatPair;
        public static MethodInfo/*!*/ StringToMutableString { get { return _StringToMutableString ?? (_StringToMutableString = CallInstruction.CacheFunc<System.String, IronRuby.Builtins.MutableString>(RubyOps.StringToMutableString)); } }
        private static MethodInfo _StringToMutableString;
        public static MethodInfo/*!*/ ToArrayValidator { get { return _ToArrayValidator ?? (_ToArrayValidator = CallInstruction.CacheFunc<System.String, System.Object, System.Collections.IList>(RubyOps.ToArrayValidator)); } }
        private static MethodInfo _ToArrayValidator;
        public static MethodInfo/*!*/ ToAValidator { get { return _ToAValidator ?? (_ToAValidator = CallInstruction.CacheFunc<System.String, System.Object, System.Collections.IList>(RubyOps.ToAValidator)); } }
        private static MethodInfo _ToAValidator;
        public static MethodInfo/*!*/ ToBignumValidator { get { return _ToBignumValidator ?? (_ToBignumValidator = CallInstruction.CacheFunc<System.String, System.Object, Microsoft.Scripting.Math.BigInteger>(RubyOps.ToBignumValidator)); } }
        private static MethodInfo _ToBignumValidator;
        public static MethodInfo/*!*/ ToByteValidator { get { return _ToByteValidator ?? (_ToByteValidator = CallInstruction.CacheFunc<System.String, System.Object, System.Byte>(RubyOps.ToByteValidator)); } }
        private static MethodInfo _ToByteValidator;
        public static MethodInfo/*!*/ ToDoubleValidator { get { return _ToDoubleValidator ?? (_ToDoubleValidator = CallInstruction.CacheFunc<System.String, System.Object, System.Double>(RubyOps.ToDoubleValidator)); } }
        private static MethodInfo _ToDoubleValidator;
        public static MethodInfo/*!*/ ToFixnumValidator { get { return _ToFixnumValidator ?? (_ToFixnumValidator = CallInstruction.CacheFunc<System.String, System.Object, System.Int32>(RubyOps.ToFixnumValidator)); } }
        private static MethodInfo _ToFixnumValidator;
        public static MethodInfo/*!*/ ToHashValidator { get { return _ToHashValidator ?? (_ToHashValidator = CallInstruction.CacheFunc<System.String, System.Object, System.Collections.Generic.IDictionary<System.Object, System.Object>>(RubyOps.ToHashValidator)); } }
        private static MethodInfo _ToHashValidator;
        public static MethodInfo/*!*/ ToInt16Validator { get { return _ToInt16Validator ?? (_ToInt16Validator = CallInstruction.CacheFunc<System.String, System.Object, System.Int16>(RubyOps.ToInt16Validator)); } }
        private static MethodInfo _ToInt16Validator;
        public static MethodInfo/*!*/ ToInt64Validator { get { return _ToInt64Validator ?? (_ToInt64Validator = CallInstruction.CacheFunc<System.String, System.Object, System.Int64>(RubyOps.ToInt64Validator)); } }
        private static MethodInfo _ToInt64Validator;
        public static MethodInfo/*!*/ ToIntegerValidator { get { return _ToIntegerValidator ?? (_ToIntegerValidator = CallInstruction.CacheFunc<System.String, System.Object, IronRuby.Runtime.IntegerValue>(RubyOps.ToIntegerValidator)); } }
        private static MethodInfo _ToIntegerValidator;
        public static MethodInfo/*!*/ ToProcValidator { get { return _ToProcValidator ?? (_ToProcValidator = CallInstruction.CacheFunc<System.String, System.Object, IronRuby.Builtins.Proc>(RubyOps.ToProcValidator)); } }
        private static MethodInfo _ToProcValidator;
        public static MethodInfo/*!*/ ToRegexValidator { get { return _ToRegexValidator ?? (_ToRegexValidator = CallInstruction.CacheFunc<System.String, System.Object, IronRuby.Builtins.RubyRegex>(RubyOps.ToRegexValidator)); } }
        private static MethodInfo _ToRegexValidator;
        public static MethodInfo/*!*/ ToSByteValidator { get { return _ToSByteValidator ?? (_ToSByteValidator = CallInstruction.CacheFunc<System.String, System.Object, System.SByte>(RubyOps.ToSByteValidator)); } }
        private static MethodInfo _ToSByteValidator;
        public static MethodInfo/*!*/ ToSDefaultConversion { get { return _ToSDefaultConversion ?? (_ToSDefaultConversion = CallInstruction.CacheFunc<IronRuby.Runtime.RubyContext, System.Object, System.Object, IronRuby.Builtins.MutableString>(RubyOps.ToSDefaultConversion)); } }
        private static MethodInfo _ToSDefaultConversion;
        public static MethodInfo/*!*/ ToSingleValidator { get { return _ToSingleValidator ?? (_ToSingleValidator = CallInstruction.CacheFunc<System.String, System.Object, System.Single>(RubyOps.ToSingleValidator)); } }
        private static MethodInfo _ToSingleValidator;
        public static MethodInfo/*!*/ ToStringValidator { get { return _ToStringValidator ?? (_ToStringValidator = CallInstruction.CacheFunc<System.String, System.Object, IronRuby.Builtins.MutableString>(RubyOps.ToStringValidator)); } }
        private static MethodInfo _ToStringValidator;
        public static MethodInfo/*!*/ ToSymbolValidator { get { return _ToSymbolValidator ?? (_ToSymbolValidator = CallInstruction.CacheFunc<System.String, System.Object, System.String>(RubyOps.ToSymbolValidator)); } }
        private static MethodInfo _ToSymbolValidator;
        public static MethodInfo/*!*/ ToUInt16Validator { get { return _ToUInt16Validator ?? (_ToUInt16Validator = CallInstruction.CacheFunc<System.String, System.Object, System.UInt16>(RubyOps.ToUInt16Validator)); } }
        private static MethodInfo _ToUInt16Validator;
        public static MethodInfo/*!*/ ToUInt32Validator { get { return _ToUInt32Validator ?? (_ToUInt32Validator = CallInstruction.CacheFunc<System.String, System.Object, System.UInt32>(RubyOps.ToUInt32Validator)); } }
        private static MethodInfo _ToUInt32Validator;
        public static MethodInfo/*!*/ ToUInt64Validator { get { return _ToUInt64Validator ?? (_ToUInt64Validator = CallInstruction.CacheFunc<System.String, System.Object, System.UInt64>(RubyOps.ToUInt64Validator)); } }
        private static MethodInfo _ToUInt64Validator;
        public static MethodInfo/*!*/ TraceBlockCall { get { return _TraceBlockCall ?? (_TraceBlockCall = CallInstruction.CacheAction<IronRuby.Runtime.RubyBlockScope, IronRuby.Runtime.BlockParam, System.String, System.Int32>(RubyOps.TraceBlockCall)); } }
        private static MethodInfo _TraceBlockCall;
        public static MethodInfo/*!*/ TraceBlockReturn { get { return _TraceBlockReturn ?? (_TraceBlockReturn = CallInstruction.CacheAction<IronRuby.Runtime.RubyBlockScope, IronRuby.Runtime.BlockParam, System.String, System.Int32>(RubyOps.TraceBlockReturn)); } }
        private static MethodInfo _TraceBlockReturn;
        public static MethodInfo/*!*/ TraceMethodCall { get { return _TraceMethodCall ?? (_TraceMethodCall = CallInstruction.CacheAction<IronRuby.Runtime.RubyMethodScope, System.String, System.Int32>(RubyOps.TraceMethodCall)); } }
        private static MethodInfo _TraceMethodCall;
        public static MethodInfo/*!*/ TraceMethodReturn { get { return _TraceMethodReturn ?? (_TraceMethodReturn = CallInstruction.CacheAction<IronRuby.Runtime.RubyMethodScope, System.String, System.Int32>(RubyOps.TraceMethodReturn)); } }
        private static MethodInfo _TraceMethodReturn;
        public static MethodInfo/*!*/ TraceTopLevelCodeFrame { get { return _TraceTopLevelCodeFrame ?? (_TraceTopLevelCodeFrame = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.Exception, System.Boolean>(RubyOps.TraceTopLevelCodeFrame)); } }
        private static MethodInfo _TraceTopLevelCodeFrame;
        public static MethodInfo/*!*/ TryGetClassVariable { get { return _TryGetClassVariable ?? (_TryGetClassVariable = CallInstruction.CacheFunc<IronRuby.Runtime.RubyScope, System.String, System.Object>(RubyOps.TryGetClassVariable)); } }
        private static MethodInfo _TryGetClassVariable;
        public static MethodInfo/*!*/ UndefineMethod { get { return _UndefineMethod ?? (_UndefineMethod = CallInstruction.CacheAction<IronRuby.Runtime.RubyScope, System.String>(RubyOps.UndefineMethod)); } }
        private static MethodInfo _UndefineMethod;
        public static MethodInfo/*!*/ Unsplat { get { return _Unsplat ?? (_Unsplat = CallInstruction.CacheFunc<System.Object, System.Collections.IList>(RubyOps.Unsplat)); } }
        private static MethodInfo _Unsplat;
        public static MethodInfo/*!*/ UpdateProfileTicks { get { return _UpdateProfileTicks ?? (_UpdateProfileTicks = CallInstruction.CacheAction<System.Int32, System.Int64>(RubyOps.UpdateProfileTicks)); } }
        private static MethodInfo _UpdateProfileTicks;
        public static MethodInfo/*!*/ X { get { return _X ?? (_X = CallInstruction.CacheAction<System.String>(RubyOps.X)); } }
        private static MethodInfo _X;
        public static MethodInfo/*!*/ Yield0 { get { return _Yield0 ?? (_Yield0 = CallInstruction.CacheFunc<Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.Yield0)); } }
        private static MethodInfo _Yield0;
        public static MethodInfo/*!*/ Yield1 { get { return _Yield1 ?? (_Yield1 = CallInstruction.CacheFunc<System.Object, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.Yield1)); } }
        private static MethodInfo _Yield1;
        public static MethodInfo/*!*/ Yield2 { get { return _Yield2 ?? (_Yield2 = CallInstruction.CacheFunc<System.Object, System.Object, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.Yield2)); } }
        private static MethodInfo _Yield2;
        public static MethodInfo/*!*/ Yield3 { get { return _Yield3 ?? (_Yield3 = CallInstruction.CacheFunc<System.Object, System.Object, System.Object, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.Yield3)); } }
        private static MethodInfo _Yield3;
        public static MethodInfo/*!*/ Yield4 { get { return _Yield4 ?? (_Yield4 = CallInstruction.CacheFunc<System.Object, System.Object, System.Object, System.Object, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.Yield4)); } }
        private static MethodInfo _Yield4;
        public static MethodInfo/*!*/ YieldN { get { return _YieldN ?? (_YieldN = CallInstruction.CacheFunc<System.Object[], Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldN)); } }
        private static MethodInfo _YieldN;
        public static MethodInfo/*!*/ YieldSplat0 { get { return _YieldSplat0 ?? (_YieldSplat0 = CallInstruction.CacheFunc<System.Collections.IList, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldSplat0)); } }
        private static MethodInfo _YieldSplat0;
        public static MethodInfo/*!*/ YieldSplat1 { get { return _YieldSplat1 ?? (_YieldSplat1 = CallInstruction.CacheFunc<System.Object, System.Collections.IList, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldSplat1)); } }
        private static MethodInfo _YieldSplat1;
        public static MethodInfo/*!*/ YieldSplat2 { get { return _YieldSplat2 ?? (_YieldSplat2 = CallInstruction.CacheFunc<System.Object, System.Object, System.Collections.IList, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldSplat2)); } }
        private static MethodInfo _YieldSplat2;
        public static MethodInfo/*!*/ YieldSplat3 { get { return _YieldSplat3 ?? (_YieldSplat3 = CallInstruction.CacheFunc<System.Object, System.Object, System.Object, System.Collections.IList, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldSplat3)); } }
        private static MethodInfo _YieldSplat3;
        public static MethodInfo/*!*/ YieldSplat4 { get { return _YieldSplat4 ?? (_YieldSplat4 = CallInstruction.CacheFunc<System.Object, System.Object, System.Object, System.Object, System.Collections.IList, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldSplat4)); } }
        private static MethodInfo _YieldSplat4;
        public static MethodInfo/*!*/ YieldSplatN { get { return _YieldSplatN ?? (_YieldSplatN = CallInstruction.CacheFunc<System.Object[], System.Collections.IList, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldSplatN)); } }
        private static MethodInfo _YieldSplatN;
        public static MethodInfo/*!*/ YieldSplatNRhs { get { return _YieldSplatNRhs ?? (_YieldSplatNRhs = CallInstruction.CacheFunc<System.Object[], System.Collections.IList, System.Object, Proc, System.Object, IronRuby.Runtime.BlockParam, System.Object>(RubyOps.YieldSplatNRhs)); } }
        private static MethodInfo _YieldSplatNRhs;
        
        public static MethodInfo/*!*/ CreateRegex(string/*!*/ suffix) {
            Debug.Assert(suffix.Length <= RubyOps.MakeStringParamCount);
            switch (suffix) {
                case "N": return CreateRegexN;
                case "M": return CreateRegexM;
                case "LM": return CreateRegexLM;
                case "ML": return CreateRegexML;
                case "MM": return CreateRegexMM;
            }
            throw Assert.Unreachable;
        }
        
        public static MethodInfo/*!*/ CreateMutableString(string/*!*/ suffix) {
            Debug.Assert(suffix.Length <= RubyOps.MakeStringParamCount);
            switch (suffix) {
                case "N": return CreateMutableStringN;
                case "M": return CreateMutableStringM;
                case "LM": return CreateMutableStringLM;
                case "ML": return CreateMutableStringML;
                case "MM": return CreateMutableStringMM;
            }
            throw Assert.Unreachable;
        }
        
        public static MethodInfo/*!*/ CreateSymbol(string/*!*/ suffix) {
            Debug.Assert(suffix.Length <= RubyOps.MakeStringParamCount);
            switch (suffix) {
                case "N": return CreateSymbolN;
                case "M": return CreateSymbolM;
                case "LM": return CreateSymbolLM;
                case "ML": return CreateSymbolML;
                case "MM": return CreateSymbolMM;
            }
            throw Assert.Unreachable;
        }
        
        public static MethodInfo/*!*/ Yield(int parameterCount) {
            switch (parameterCount) {
                case 0: return Yield0;
                case 1: return Yield1;
                case 2: return Yield2;
                case 3: return Yield3;
                case 4: return Yield4;
            }
            return YieldN;
        }
        
        public static MethodInfo/*!*/ YieldSplat(int parameterCount) {
            switch (parameterCount) {
                case 0: return YieldSplat0;
                case 1: return YieldSplat1;
                case 2: return YieldSplat2;
                case 3: return YieldSplat3;
                case 4: return YieldSplat4;
            }
            return YieldSplatN;
        }
        
    }
    public static partial class Fields {
        public static FieldInfo/*!*/ ConstantSiteCache_Value { get { return _ConstantSiteCache_Value ?? (_ConstantSiteCache_Value = GetField(typeof(ConstantSiteCache), "Value")); } }
        private static FieldInfo _ConstantSiteCache_Value;
        public static FieldInfo/*!*/ ConstantSiteCache_Version { get { return _ConstantSiteCache_Version ?? (_ConstantSiteCache_Version = GetField(typeof(ConstantSiteCache), "Version")); } }
        private static FieldInfo _ConstantSiteCache_Version;
        public static FieldInfo/*!*/ ConstantSiteCache_WeakMissingConstant { get { return _ConstantSiteCache_WeakMissingConstant ?? (_ConstantSiteCache_WeakMissingConstant = GetField(typeof(ConstantSiteCache), "WeakMissingConstant")); } }
        private static FieldInfo _ConstantSiteCache_WeakMissingConstant;
        public static FieldInfo/*!*/ ConstantSiteCache_WeakNull { get { return _ConstantSiteCache_WeakNull ?? (_ConstantSiteCache_WeakNull = GetField(typeof(ConstantSiteCache), "WeakNull")); } }
        private static FieldInfo _ConstantSiteCache_WeakNull;
        public static FieldInfo/*!*/ DefaultArgument { get { return _DefaultArgument ?? (_DefaultArgument = GetField(typeof(RubyOps), "DefaultArgument")); } }
        private static FieldInfo _DefaultArgument;
        public static FieldInfo/*!*/ ForwardToBase { get { return _ForwardToBase ?? (_ForwardToBase = GetField(typeof(RubyOps), "ForwardToBase")); } }
        private static FieldInfo _ForwardToBase;
        public static FieldInfo/*!*/ IsDefinedConstantSiteCache_Value { get { return _IsDefinedConstantSiteCache_Value ?? (_IsDefinedConstantSiteCache_Value = GetField(typeof(IsDefinedConstantSiteCache), "Value")); } }
        private static FieldInfo _IsDefinedConstantSiteCache_Value;
        public static FieldInfo/*!*/ IsDefinedConstantSiteCache_Version { get { return _IsDefinedConstantSiteCache_Version ?? (_IsDefinedConstantSiteCache_Version = GetField(typeof(IsDefinedConstantSiteCache), "Version")); } }
        private static FieldInfo _IsDefinedConstantSiteCache_Version;
        public static FieldInfo/*!*/ NeedsUpdate { get { return _NeedsUpdate ?? (_NeedsUpdate = GetField(typeof(RubyOps), "NeedsUpdate")); } }
        private static FieldInfo _NeedsUpdate;
        public static FieldInfo/*!*/ RubyContext_ConstantAccessVersion { get { return _RubyContext_ConstantAccessVersion ?? (_RubyContext_ConstantAccessVersion = GetField(typeof(RubyContext), "ConstantAccessVersion")); } }
        private static FieldInfo _RubyContext_ConstantAccessVersion;
        public static FieldInfo/*!*/ RubyModule_Version { get { return _RubyModule_Version ?? (_RubyModule_Version = GetField(typeof(RubyModule), "Version")); } }
        private static FieldInfo _RubyModule_Version;
        public static FieldInfo/*!*/ VersionHandle_Method { get { return _VersionHandle_Method ?? (_VersionHandle_Method = GetField(typeof(VersionHandle), "Method")); } }
        private static FieldInfo _VersionHandle_Method;
        
    }
}
