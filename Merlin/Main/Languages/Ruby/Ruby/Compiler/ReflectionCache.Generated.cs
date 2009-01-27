/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Reflection;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler {
    internal static partial class Methods {
        private static MethodInfo _MakeAmbiguousMatchError, _IsSuperCallTarget, _CreateInclusiveRange, _CreateExclusiveRange, _CreateInclusiveIntegerRange, _CreateExclusiveIntegerRange, _AllocateStructInstance, _CreateStructInstance, _GetMetaObject, _ToProcValidator, _ToStringValidator, _ToSymbolValidator, _ConvertSymbolIdToSymbol, _ConvertFixnumToSymbol, _ConvertMutableStringToSymbol, _ToRegexValidator, _ToArrayValidator, _ToHashValidator, _ToFixnumValidator, 
        _ToIntegerValidator, _ToFloatValidator, _ConvertBignumToFloat, _ConvertStringToFloat, _CreateTypeConversionError, _ConvertBignumToFixnum, _ToSDefaultConversion, _GetInstanceVariable, _IsDefinedInstanceVariable, _SetInstanceVariable, _GetObjectClassVariable, _GetClassVariable, _TryGetObjectClassVariable, _TryGetClassVariable, _IsDefinedObjectClassVariable, _IsDefinedClassVariable, _SetObjectClassVariable, _SetClassVariable, _GetInstanceData, _DeserializeObject, 
        _SerializeObject, _HookupEvent, _CreateDelegateFromProc, _CreateDelegateFromMethod, _X, _IsDefinedQualifiedConstant, _SetGlobalConstant, _SetUnqualifiedConstant, _SetQualifiedConstant, _MakeArray0, _MakeArray1, _MakeArray2, _MakeArray3, _MakeArray4, _MakeArray5, _MakeArrayN, _MakeHash0, _MakeHash, _SplatAppend, _Splat, 
        _SplatPair, _Unsplat, _GetArrayItem, _GetArraySuffix, _GetGlobalVariable, _IsDefinedGlobalVariable, _SetGlobalVariable, _AliasGlobalVariable, _GetCurrentMatchGroup, _GetCurrentMatchData, _GetCurrentMatchLastGroup, _GetCurrentMatchPrefix, _GetCurrentMatchSuffix, _MatchLastInputLine, _MatchString, _CreateRegexB, _CreateRegexU, _CreateRegexE, _CreateRegexM, _CreateRegexBM, 
        _CreateRegexUM, _CreateRegexEM, _CreateRegexMB, _CreateRegexMU, _CreateRegexME, _CreateRegexMM, _CreateRegexN, _CreateMutableStringB, _CreateMutableStringU, _CreateMutableStringE, _CreateMutableStringM, _CreateMutableStringBM, _CreateMutableStringUM, _CreateMutableStringEM, _CreateMutableStringMB, _CreateMutableStringMU, _CreateMutableStringME, _CreateMutableStringMM, _CreateMutableStringN, _CreateSymbolB, 
        _CreateSymbolU, _CreateSymbolE, _CreateSymbolM, _CreateSymbolBM, _CreateSymbolUM, _CreateSymbolEM, _CreateSymbolMB, _CreateSymbolMU, _CreateSymbolME, _CreateSymbolMM, _CreateSymbolN, _CreateEncoding, _IsTrue, _IsFalse, _CheckForAsyncRaiseViaThreadAbort, _GetCurrentException, _SetCurrentExceptionAndStackTrace, _SetCurrentException, _CompareException, _CompareSplattedExceptions, 
        _CompareDefaultException, _GetDefaultExceptionMessage, _CreateArgumentsError, _CreateArgumentsErrorForMissingBlock, _CreateArgumentsErrorForProc, _MakeWrongNumberOfArgumentsError, _MakeTopLevelSuperException, _MakeMissingSuperException, _MakeInvalidArgumentTypesError, _UpdateProfileTicks, _IsProcConverterTarget, _CreateBfcForYield, _CreateBfcForMethodProcCall, _CreateBfcForProcCall, _CreateBfcForLibraryMethod, _LeaveProcConverter, _CreateRfcForMethod, _BlockRetry, _MethodRetry, _EvalRetry, 
        _BlockBreak, _MethodBreak, _EvalBreak, _MethodNext, _EvalNext, _MethodRedo, _EvalRedo, _BlockReturn, _EvalReturn, _BlockYield, _MethodYield, _EvalYield, _MethodProcCall, _CanRescue, _IsRetrySingleton, _PropagateRetrySingleton, _GetRetrySingleton, _CreateMainTopLevelScope, _CreateTopLevelHostedScope, _CreateTopLevelScope, 
        _CreateWrappedTopLevelScope, _CreateModuleEvalScope, _CreateModuleScope, _CreateMethodScope, _CreateBlockScope, _TraceMethodCall, _TraceMethodReturn, _TraceBlockCall, _TraceBlockReturn, _PrintInteractiveResult, _GetLocalVariable, _SetLocalVariable, _GetContextFromScope, _GetContextFromMethod, _GetContextFromBlockParam, _GetContextFromProc, _GetEmptyScope, _DefineBlock, _InitializeBlock, _Yield0, 
        _Yield1, _Yield2, _Yield3, _Yield4, _YieldN, _YieldSplat0, _YieldSplat1, _YieldSplat2, _YieldSplat3, _YieldSplat4, _YieldSplatN, _YieldSplatNRhs, _DefineMethod, _MethodDefined, _AliasMethod, _UndefineMethod, _IsDefinedMethod, _DefineGlobalModule, _DefineNestedModule, _DefineModule, 
        _DefineSingletonClass, _DefineGlobalClass, _DefineNestedClass, _DefineClass, _GetGlobalConstant, _GetUnqualifiedConstant, _GetQualifiedConstant, _IsDefinedGlobalConstant, _IsDefinedUnqualifiedConstant;
        
        public static MethodInfo/*!*/ MakeAmbiguousMatchError { get { return _MakeAmbiguousMatchError ?? (_MakeAmbiguousMatchError = GetMethod(typeof(RubyOps), "MakeAmbiguousMatchError")); } }
        public static MethodInfo/*!*/ IsSuperCallTarget { get { return _IsSuperCallTarget ?? (_IsSuperCallTarget = GetMethod(typeof(RubyOps), "IsSuperCallTarget")); } }
        public static MethodInfo/*!*/ CreateInclusiveRange { get { return _CreateInclusiveRange ?? (_CreateInclusiveRange = GetMethod(typeof(RubyOps), "CreateInclusiveRange")); } }
        public static MethodInfo/*!*/ CreateExclusiveRange { get { return _CreateExclusiveRange ?? (_CreateExclusiveRange = GetMethod(typeof(RubyOps), "CreateExclusiveRange")); } }
        public static MethodInfo/*!*/ CreateInclusiveIntegerRange { get { return _CreateInclusiveIntegerRange ?? (_CreateInclusiveIntegerRange = GetMethod(typeof(RubyOps), "CreateInclusiveIntegerRange")); } }
        public static MethodInfo/*!*/ CreateExclusiveIntegerRange { get { return _CreateExclusiveIntegerRange ?? (_CreateExclusiveIntegerRange = GetMethod(typeof(RubyOps), "CreateExclusiveIntegerRange")); } }
        public static MethodInfo/*!*/ AllocateStructInstance { get { return _AllocateStructInstance ?? (_AllocateStructInstance = GetMethod(typeof(RubyOps), "AllocateStructInstance")); } }
        public static MethodInfo/*!*/ CreateStructInstance { get { return _CreateStructInstance ?? (_CreateStructInstance = GetMethod(typeof(RubyOps), "CreateStructInstance")); } }
        public static MethodInfo/*!*/ GetMetaObject { get { return _GetMetaObject ?? (_GetMetaObject = GetMethod(typeof(RubyOps), "GetMetaObject")); } }
        public static MethodInfo/*!*/ ToProcValidator { get { return _ToProcValidator ?? (_ToProcValidator = GetMethod(typeof(RubyOps), "ToProcValidator")); } }
        public static MethodInfo/*!*/ ToStringValidator { get { return _ToStringValidator ?? (_ToStringValidator = GetMethod(typeof(RubyOps), "ToStringValidator")); } }
        public static MethodInfo/*!*/ ToSymbolValidator { get { return _ToSymbolValidator ?? (_ToSymbolValidator = GetMethod(typeof(RubyOps), "ToSymbolValidator")); } }
        public static MethodInfo/*!*/ ConvertSymbolIdToSymbol { get { return _ConvertSymbolIdToSymbol ?? (_ConvertSymbolIdToSymbol = GetMethod(typeof(RubyOps), "ConvertSymbolIdToSymbol")); } }
        public static MethodInfo/*!*/ ConvertFixnumToSymbol { get { return _ConvertFixnumToSymbol ?? (_ConvertFixnumToSymbol = GetMethod(typeof(RubyOps), "ConvertFixnumToSymbol")); } }
        public static MethodInfo/*!*/ ConvertMutableStringToSymbol { get { return _ConvertMutableStringToSymbol ?? (_ConvertMutableStringToSymbol = GetMethod(typeof(RubyOps), "ConvertMutableStringToSymbol")); } }
        public static MethodInfo/*!*/ ToRegexValidator { get { return _ToRegexValidator ?? (_ToRegexValidator = GetMethod(typeof(RubyOps), "ToRegexValidator")); } }
        public static MethodInfo/*!*/ ToArrayValidator { get { return _ToArrayValidator ?? (_ToArrayValidator = GetMethod(typeof(RubyOps), "ToArrayValidator")); } }
        public static MethodInfo/*!*/ ToHashValidator { get { return _ToHashValidator ?? (_ToHashValidator = GetMethod(typeof(RubyOps), "ToHashValidator")); } }
        public static MethodInfo/*!*/ ToFixnumValidator { get { return _ToFixnumValidator ?? (_ToFixnumValidator = GetMethod(typeof(RubyOps), "ToFixnumValidator")); } }
        public static MethodInfo/*!*/ ToIntegerValidator { get { return _ToIntegerValidator ?? (_ToIntegerValidator = GetMethod(typeof(RubyOps), "ToIntegerValidator")); } }
        public static MethodInfo/*!*/ ToFloatValidator { get { return _ToFloatValidator ?? (_ToFloatValidator = GetMethod(typeof(RubyOps), "ToFloatValidator")); } }
        public static MethodInfo/*!*/ ConvertBignumToFloat { get { return _ConvertBignumToFloat ?? (_ConvertBignumToFloat = GetMethod(typeof(RubyOps), "ConvertBignumToFloat")); } }
        public static MethodInfo/*!*/ ConvertStringToFloat { get { return _ConvertStringToFloat ?? (_ConvertStringToFloat = GetMethod(typeof(RubyOps), "ConvertStringToFloat")); } }
        public static MethodInfo/*!*/ CreateTypeConversionError { get { return _CreateTypeConversionError ?? (_CreateTypeConversionError = GetMethod(typeof(RubyOps), "CreateTypeConversionError")); } }
        public static MethodInfo/*!*/ ConvertBignumToFixnum { get { return _ConvertBignumToFixnum ?? (_ConvertBignumToFixnum = GetMethod(typeof(RubyOps), "ConvertBignumToFixnum")); } }
        public static MethodInfo/*!*/ ToSDefaultConversion { get { return _ToSDefaultConversion ?? (_ToSDefaultConversion = GetMethod(typeof(RubyOps), "ToSDefaultConversion")); } }
        public static MethodInfo/*!*/ GetInstanceVariable { get { return _GetInstanceVariable ?? (_GetInstanceVariable = GetMethod(typeof(RubyOps), "GetInstanceVariable")); } }
        public static MethodInfo/*!*/ IsDefinedInstanceVariable { get { return _IsDefinedInstanceVariable ?? (_IsDefinedInstanceVariable = GetMethod(typeof(RubyOps), "IsDefinedInstanceVariable")); } }
        public static MethodInfo/*!*/ SetInstanceVariable { get { return _SetInstanceVariable ?? (_SetInstanceVariable = GetMethod(typeof(RubyOps), "SetInstanceVariable")); } }
        public static MethodInfo/*!*/ GetObjectClassVariable { get { return _GetObjectClassVariable ?? (_GetObjectClassVariable = GetMethod(typeof(RubyOps), "GetObjectClassVariable")); } }
        public static MethodInfo/*!*/ GetClassVariable { get { return _GetClassVariable ?? (_GetClassVariable = GetMethod(typeof(RubyOps), "GetClassVariable")); } }
        public static MethodInfo/*!*/ TryGetObjectClassVariable { get { return _TryGetObjectClassVariable ?? (_TryGetObjectClassVariable = GetMethod(typeof(RubyOps), "TryGetObjectClassVariable")); } }
        public static MethodInfo/*!*/ TryGetClassVariable { get { return _TryGetClassVariable ?? (_TryGetClassVariable = GetMethod(typeof(RubyOps), "TryGetClassVariable")); } }
        public static MethodInfo/*!*/ IsDefinedObjectClassVariable { get { return _IsDefinedObjectClassVariable ?? (_IsDefinedObjectClassVariable = GetMethod(typeof(RubyOps), "IsDefinedObjectClassVariable")); } }
        public static MethodInfo/*!*/ IsDefinedClassVariable { get { return _IsDefinedClassVariable ?? (_IsDefinedClassVariable = GetMethod(typeof(RubyOps), "IsDefinedClassVariable")); } }
        public static MethodInfo/*!*/ SetObjectClassVariable { get { return _SetObjectClassVariable ?? (_SetObjectClassVariable = GetMethod(typeof(RubyOps), "SetObjectClassVariable")); } }
        public static MethodInfo/*!*/ SetClassVariable { get { return _SetClassVariable ?? (_SetClassVariable = GetMethod(typeof(RubyOps), "SetClassVariable")); } }
        public static MethodInfo/*!*/ GetInstanceData { get { return _GetInstanceData ?? (_GetInstanceData = GetMethod(typeof(RubyOps), "GetInstanceData")); } }
        public static MethodInfo/*!*/ DeserializeObject { get { return _DeserializeObject ?? (_DeserializeObject = GetMethod(typeof(RubyOps), "DeserializeObject")); } }
        public static MethodInfo/*!*/ SerializeObject { get { return _SerializeObject ?? (_SerializeObject = GetMethod(typeof(RubyOps), "SerializeObject")); } }
        public static MethodInfo/*!*/ HookupEvent { get { return _HookupEvent ?? (_HookupEvent = GetMethod(typeof(RubyOps), "HookupEvent")); } }
        public static MethodInfo/*!*/ CreateDelegateFromProc { get { return _CreateDelegateFromProc ?? (_CreateDelegateFromProc = GetMethod(typeof(RubyOps), "CreateDelegateFromProc")); } }
        public static MethodInfo/*!*/ CreateDelegateFromMethod { get { return _CreateDelegateFromMethod ?? (_CreateDelegateFromMethod = GetMethod(typeof(RubyOps), "CreateDelegateFromMethod")); } }
        public static MethodInfo/*!*/ X { get { return _X ?? (_X = GetMethod(typeof(RubyOps), "X")); } }
        public static MethodInfo/*!*/ IsDefinedQualifiedConstant { get { return _IsDefinedQualifiedConstant ?? (_IsDefinedQualifiedConstant = GetMethod(typeof(RubyOps), "IsDefinedQualifiedConstant")); } }
        public static MethodInfo/*!*/ SetGlobalConstant { get { return _SetGlobalConstant ?? (_SetGlobalConstant = GetMethod(typeof(RubyOps), "SetGlobalConstant")); } }
        public static MethodInfo/*!*/ SetUnqualifiedConstant { get { return _SetUnqualifiedConstant ?? (_SetUnqualifiedConstant = GetMethod(typeof(RubyOps), "SetUnqualifiedConstant")); } }
        public static MethodInfo/*!*/ SetQualifiedConstant { get { return _SetQualifiedConstant ?? (_SetQualifiedConstant = GetMethod(typeof(RubyOps), "SetQualifiedConstant")); } }
        public static MethodInfo/*!*/ MakeArray0 { get { return _MakeArray0 ?? (_MakeArray0 = GetMethod(typeof(RubyOps), "MakeArray0")); } }
        public static MethodInfo/*!*/ MakeArray1 { get { return _MakeArray1 ?? (_MakeArray1 = GetMethod(typeof(RubyOps), "MakeArray1")); } }
        public static MethodInfo/*!*/ MakeArray2 { get { return _MakeArray2 ?? (_MakeArray2 = GetMethod(typeof(RubyOps), "MakeArray2")); } }
        public static MethodInfo/*!*/ MakeArray3 { get { return _MakeArray3 ?? (_MakeArray3 = GetMethod(typeof(RubyOps), "MakeArray3")); } }
        public static MethodInfo/*!*/ MakeArray4 { get { return _MakeArray4 ?? (_MakeArray4 = GetMethod(typeof(RubyOps), "MakeArray4")); } }
        public static MethodInfo/*!*/ MakeArray5 { get { return _MakeArray5 ?? (_MakeArray5 = GetMethod(typeof(RubyOps), "MakeArray5")); } }
        public static MethodInfo/*!*/ MakeArrayN { get { return _MakeArrayN ?? (_MakeArrayN = GetMethod(typeof(RubyOps), "MakeArrayN")); } }
        public static MethodInfo/*!*/ MakeHash0 { get { return _MakeHash0 ?? (_MakeHash0 = GetMethod(typeof(RubyOps), "MakeHash0")); } }
        public static MethodInfo/*!*/ MakeHash { get { return _MakeHash ?? (_MakeHash = GetMethod(typeof(RubyOps), "MakeHash")); } }
        public static MethodInfo/*!*/ SplatAppend { get { return _SplatAppend ?? (_SplatAppend = GetMethod(typeof(RubyOps), "SplatAppend")); } }
        public static MethodInfo/*!*/ Splat { get { return _Splat ?? (_Splat = GetMethod(typeof(RubyOps), "Splat")); } }
        public static MethodInfo/*!*/ SplatPair { get { return _SplatPair ?? (_SplatPair = GetMethod(typeof(RubyOps), "SplatPair")); } }
        public static MethodInfo/*!*/ Unsplat { get { return _Unsplat ?? (_Unsplat = GetMethod(typeof(RubyOps), "Unsplat")); } }
        public static MethodInfo/*!*/ GetArrayItem { get { return _GetArrayItem ?? (_GetArrayItem = GetMethod(typeof(RubyOps), "GetArrayItem")); } }
        public static MethodInfo/*!*/ GetArraySuffix { get { return _GetArraySuffix ?? (_GetArraySuffix = GetMethod(typeof(RubyOps), "GetArraySuffix")); } }
        public static MethodInfo/*!*/ GetGlobalVariable { get { return _GetGlobalVariable ?? (_GetGlobalVariable = GetMethod(typeof(RubyOps), "GetGlobalVariable")); } }
        public static MethodInfo/*!*/ IsDefinedGlobalVariable { get { return _IsDefinedGlobalVariable ?? (_IsDefinedGlobalVariable = GetMethod(typeof(RubyOps), "IsDefinedGlobalVariable")); } }
        public static MethodInfo/*!*/ SetGlobalVariable { get { return _SetGlobalVariable ?? (_SetGlobalVariable = GetMethod(typeof(RubyOps), "SetGlobalVariable")); } }
        public static MethodInfo/*!*/ AliasGlobalVariable { get { return _AliasGlobalVariable ?? (_AliasGlobalVariable = GetMethod(typeof(RubyOps), "AliasGlobalVariable")); } }
        public static MethodInfo/*!*/ GetCurrentMatchGroup { get { return _GetCurrentMatchGroup ?? (_GetCurrentMatchGroup = GetMethod(typeof(RubyOps), "GetCurrentMatchGroup")); } }
        public static MethodInfo/*!*/ GetCurrentMatchData { get { return _GetCurrentMatchData ?? (_GetCurrentMatchData = GetMethod(typeof(RubyOps), "GetCurrentMatchData")); } }
        public static MethodInfo/*!*/ GetCurrentMatchLastGroup { get { return _GetCurrentMatchLastGroup ?? (_GetCurrentMatchLastGroup = GetMethod(typeof(RubyOps), "GetCurrentMatchLastGroup")); } }
        public static MethodInfo/*!*/ GetCurrentMatchPrefix { get { return _GetCurrentMatchPrefix ?? (_GetCurrentMatchPrefix = GetMethod(typeof(RubyOps), "GetCurrentMatchPrefix")); } }
        public static MethodInfo/*!*/ GetCurrentMatchSuffix { get { return _GetCurrentMatchSuffix ?? (_GetCurrentMatchSuffix = GetMethod(typeof(RubyOps), "GetCurrentMatchSuffix")); } }
        public static MethodInfo/*!*/ MatchLastInputLine { get { return _MatchLastInputLine ?? (_MatchLastInputLine = GetMethod(typeof(RubyOps), "MatchLastInputLine")); } }
        public static MethodInfo/*!*/ MatchString { get { return _MatchString ?? (_MatchString = GetMethod(typeof(RubyOps), "MatchString")); } }
        public static MethodInfo/*!*/ CreateRegexB { get { return _CreateRegexB ?? (_CreateRegexB = GetMethod(typeof(RubyOps), "CreateRegexB")); } }
        public static MethodInfo/*!*/ CreateRegexU { get { return _CreateRegexU ?? (_CreateRegexU = GetMethod(typeof(RubyOps), "CreateRegexU")); } }
        public static MethodInfo/*!*/ CreateRegexE { get { return _CreateRegexE ?? (_CreateRegexE = GetMethod(typeof(RubyOps), "CreateRegexE")); } }
        public static MethodInfo/*!*/ CreateRegexM { get { return _CreateRegexM ?? (_CreateRegexM = GetMethod(typeof(RubyOps), "CreateRegexM")); } }
        public static MethodInfo/*!*/ CreateRegexBM { get { return _CreateRegexBM ?? (_CreateRegexBM = GetMethod(typeof(RubyOps), "CreateRegexBM")); } }
        public static MethodInfo/*!*/ CreateRegexUM { get { return _CreateRegexUM ?? (_CreateRegexUM = GetMethod(typeof(RubyOps), "CreateRegexUM")); } }
        public static MethodInfo/*!*/ CreateRegexEM { get { return _CreateRegexEM ?? (_CreateRegexEM = GetMethod(typeof(RubyOps), "CreateRegexEM")); } }
        public static MethodInfo/*!*/ CreateRegexMB { get { return _CreateRegexMB ?? (_CreateRegexMB = GetMethod(typeof(RubyOps), "CreateRegexMB")); } }
        public static MethodInfo/*!*/ CreateRegexMU { get { return _CreateRegexMU ?? (_CreateRegexMU = GetMethod(typeof(RubyOps), "CreateRegexMU")); } }
        public static MethodInfo/*!*/ CreateRegexME { get { return _CreateRegexME ?? (_CreateRegexME = GetMethod(typeof(RubyOps), "CreateRegexME")); } }
        public static MethodInfo/*!*/ CreateRegexMM { get { return _CreateRegexMM ?? (_CreateRegexMM = GetMethod(typeof(RubyOps), "CreateRegexMM")); } }
        public static MethodInfo/*!*/ CreateRegexN { get { return _CreateRegexN ?? (_CreateRegexN = GetMethod(typeof(RubyOps), "CreateRegexN")); } }
        public static MethodInfo/*!*/ CreateMutableStringB { get { return _CreateMutableStringB ?? (_CreateMutableStringB = GetMethod(typeof(RubyOps), "CreateMutableStringB")); } }
        public static MethodInfo/*!*/ CreateMutableStringU { get { return _CreateMutableStringU ?? (_CreateMutableStringU = GetMethod(typeof(RubyOps), "CreateMutableStringU")); } }
        public static MethodInfo/*!*/ CreateMutableStringE { get { return _CreateMutableStringE ?? (_CreateMutableStringE = GetMethod(typeof(RubyOps), "CreateMutableStringE")); } }
        public static MethodInfo/*!*/ CreateMutableStringM { get { return _CreateMutableStringM ?? (_CreateMutableStringM = GetMethod(typeof(RubyOps), "CreateMutableStringM")); } }
        public static MethodInfo/*!*/ CreateMutableStringBM { get { return _CreateMutableStringBM ?? (_CreateMutableStringBM = GetMethod(typeof(RubyOps), "CreateMutableStringBM")); } }
        public static MethodInfo/*!*/ CreateMutableStringUM { get { return _CreateMutableStringUM ?? (_CreateMutableStringUM = GetMethod(typeof(RubyOps), "CreateMutableStringUM")); } }
        public static MethodInfo/*!*/ CreateMutableStringEM { get { return _CreateMutableStringEM ?? (_CreateMutableStringEM = GetMethod(typeof(RubyOps), "CreateMutableStringEM")); } }
        public static MethodInfo/*!*/ CreateMutableStringMB { get { return _CreateMutableStringMB ?? (_CreateMutableStringMB = GetMethod(typeof(RubyOps), "CreateMutableStringMB")); } }
        public static MethodInfo/*!*/ CreateMutableStringMU { get { return _CreateMutableStringMU ?? (_CreateMutableStringMU = GetMethod(typeof(RubyOps), "CreateMutableStringMU")); } }
        public static MethodInfo/*!*/ CreateMutableStringME { get { return _CreateMutableStringME ?? (_CreateMutableStringME = GetMethod(typeof(RubyOps), "CreateMutableStringME")); } }
        public static MethodInfo/*!*/ CreateMutableStringMM { get { return _CreateMutableStringMM ?? (_CreateMutableStringMM = GetMethod(typeof(RubyOps), "CreateMutableStringMM")); } }
        public static MethodInfo/*!*/ CreateMutableStringN { get { return _CreateMutableStringN ?? (_CreateMutableStringN = GetMethod(typeof(RubyOps), "CreateMutableStringN")); } }
        public static MethodInfo/*!*/ CreateSymbolB { get { return _CreateSymbolB ?? (_CreateSymbolB = GetMethod(typeof(RubyOps), "CreateSymbolB")); } }
        public static MethodInfo/*!*/ CreateSymbolU { get { return _CreateSymbolU ?? (_CreateSymbolU = GetMethod(typeof(RubyOps), "CreateSymbolU")); } }
        public static MethodInfo/*!*/ CreateSymbolE { get { return _CreateSymbolE ?? (_CreateSymbolE = GetMethod(typeof(RubyOps), "CreateSymbolE")); } }
        public static MethodInfo/*!*/ CreateSymbolM { get { return _CreateSymbolM ?? (_CreateSymbolM = GetMethod(typeof(RubyOps), "CreateSymbolM")); } }
        public static MethodInfo/*!*/ CreateSymbolBM { get { return _CreateSymbolBM ?? (_CreateSymbolBM = GetMethod(typeof(RubyOps), "CreateSymbolBM")); } }
        public static MethodInfo/*!*/ CreateSymbolUM { get { return _CreateSymbolUM ?? (_CreateSymbolUM = GetMethod(typeof(RubyOps), "CreateSymbolUM")); } }
        public static MethodInfo/*!*/ CreateSymbolEM { get { return _CreateSymbolEM ?? (_CreateSymbolEM = GetMethod(typeof(RubyOps), "CreateSymbolEM")); } }
        public static MethodInfo/*!*/ CreateSymbolMB { get { return _CreateSymbolMB ?? (_CreateSymbolMB = GetMethod(typeof(RubyOps), "CreateSymbolMB")); } }
        public static MethodInfo/*!*/ CreateSymbolMU { get { return _CreateSymbolMU ?? (_CreateSymbolMU = GetMethod(typeof(RubyOps), "CreateSymbolMU")); } }
        public static MethodInfo/*!*/ CreateSymbolME { get { return _CreateSymbolME ?? (_CreateSymbolME = GetMethod(typeof(RubyOps), "CreateSymbolME")); } }
        public static MethodInfo/*!*/ CreateSymbolMM { get { return _CreateSymbolMM ?? (_CreateSymbolMM = GetMethod(typeof(RubyOps), "CreateSymbolMM")); } }
        public static MethodInfo/*!*/ CreateSymbolN { get { return _CreateSymbolN ?? (_CreateSymbolN = GetMethod(typeof(RubyOps), "CreateSymbolN")); } }
        public static MethodInfo/*!*/ CreateEncoding { get { return _CreateEncoding ?? (_CreateEncoding = GetMethod(typeof(RubyOps), "CreateEncoding")); } }
        public static MethodInfo/*!*/ IsTrue { get { return _IsTrue ?? (_IsTrue = GetMethod(typeof(RubyOps), "IsTrue")); } }
        public static MethodInfo/*!*/ IsFalse { get { return _IsFalse ?? (_IsFalse = GetMethod(typeof(RubyOps), "IsFalse")); } }
        public static MethodInfo/*!*/ CheckForAsyncRaiseViaThreadAbort { get { return _CheckForAsyncRaiseViaThreadAbort ?? (_CheckForAsyncRaiseViaThreadAbort = GetMethod(typeof(RubyOps), "CheckForAsyncRaiseViaThreadAbort")); } }
        public static MethodInfo/*!*/ GetCurrentException { get { return _GetCurrentException ?? (_GetCurrentException = GetMethod(typeof(RubyOps), "GetCurrentException")); } }
        public static MethodInfo/*!*/ SetCurrentExceptionAndStackTrace { get { return _SetCurrentExceptionAndStackTrace ?? (_SetCurrentExceptionAndStackTrace = GetMethod(typeof(RubyOps), "SetCurrentExceptionAndStackTrace")); } }
        public static MethodInfo/*!*/ SetCurrentException { get { return _SetCurrentException ?? (_SetCurrentException = GetMethod(typeof(RubyOps), "SetCurrentException")); } }
        public static MethodInfo/*!*/ CompareException { get { return _CompareException ?? (_CompareException = GetMethod(typeof(RubyOps), "CompareException")); } }
        public static MethodInfo/*!*/ CompareSplattedExceptions { get { return _CompareSplattedExceptions ?? (_CompareSplattedExceptions = GetMethod(typeof(RubyOps), "CompareSplattedExceptions")); } }
        public static MethodInfo/*!*/ CompareDefaultException { get { return _CompareDefaultException ?? (_CompareDefaultException = GetMethod(typeof(RubyOps), "CompareDefaultException")); } }
        public static MethodInfo/*!*/ GetDefaultExceptionMessage { get { return _GetDefaultExceptionMessage ?? (_GetDefaultExceptionMessage = GetMethod(typeof(RubyOps), "GetDefaultExceptionMessage")); } }
        public static MethodInfo/*!*/ CreateArgumentsError { get { return _CreateArgumentsError ?? (_CreateArgumentsError = GetMethod(typeof(RubyOps), "CreateArgumentsError")); } }
        public static MethodInfo/*!*/ CreateArgumentsErrorForMissingBlock { get { return _CreateArgumentsErrorForMissingBlock ?? (_CreateArgumentsErrorForMissingBlock = GetMethod(typeof(RubyOps), "CreateArgumentsErrorForMissingBlock")); } }
        public static MethodInfo/*!*/ CreateArgumentsErrorForProc { get { return _CreateArgumentsErrorForProc ?? (_CreateArgumentsErrorForProc = GetMethod(typeof(RubyOps), "CreateArgumentsErrorForProc")); } }
        public static MethodInfo/*!*/ MakeWrongNumberOfArgumentsError { get { return _MakeWrongNumberOfArgumentsError ?? (_MakeWrongNumberOfArgumentsError = GetMethod(typeof(RubyOps), "MakeWrongNumberOfArgumentsError")); } }
        public static MethodInfo/*!*/ MakeTopLevelSuperException { get { return _MakeTopLevelSuperException ?? (_MakeTopLevelSuperException = GetMethod(typeof(RubyOps), "MakeTopLevelSuperException")); } }
        public static MethodInfo/*!*/ MakeMissingSuperException { get { return _MakeMissingSuperException ?? (_MakeMissingSuperException = GetMethod(typeof(RubyOps), "MakeMissingSuperException")); } }
        public static MethodInfo/*!*/ MakeInvalidArgumentTypesError { get { return _MakeInvalidArgumentTypesError ?? (_MakeInvalidArgumentTypesError = GetMethod(typeof(RubyOps), "MakeInvalidArgumentTypesError")); } }
        public static MethodInfo/*!*/ UpdateProfileTicks { get { return _UpdateProfileTicks ?? (_UpdateProfileTicks = GetMethod(typeof(RubyOps), "UpdateProfileTicks")); } }
        public static MethodInfo/*!*/ IsProcConverterTarget { get { return _IsProcConverterTarget ?? (_IsProcConverterTarget = GetMethod(typeof(RubyOps), "IsProcConverterTarget")); } }
        public static MethodInfo/*!*/ CreateBfcForYield { get { return _CreateBfcForYield ?? (_CreateBfcForYield = GetMethod(typeof(RubyOps), "CreateBfcForYield")); } }
        public static MethodInfo/*!*/ CreateBfcForMethodProcCall { get { return _CreateBfcForMethodProcCall ?? (_CreateBfcForMethodProcCall = GetMethod(typeof(RubyOps), "CreateBfcForMethodProcCall")); } }
        public static MethodInfo/*!*/ CreateBfcForProcCall { get { return _CreateBfcForProcCall ?? (_CreateBfcForProcCall = GetMethod(typeof(RubyOps), "CreateBfcForProcCall")); } }
        public static MethodInfo/*!*/ CreateBfcForLibraryMethod { get { return _CreateBfcForLibraryMethod ?? (_CreateBfcForLibraryMethod = GetMethod(typeof(RubyOps), "CreateBfcForLibraryMethod")); } }
        public static MethodInfo/*!*/ LeaveProcConverter { get { return _LeaveProcConverter ?? (_LeaveProcConverter = GetMethod(typeof(RubyOps), "LeaveProcConverter")); } }
        public static MethodInfo/*!*/ CreateRfcForMethod { get { return _CreateRfcForMethod ?? (_CreateRfcForMethod = GetMethod(typeof(RubyOps), "CreateRfcForMethod")); } }
        public static MethodInfo/*!*/ BlockRetry { get { return _BlockRetry ?? (_BlockRetry = GetMethod(typeof(RubyOps), "BlockRetry")); } }
        public static MethodInfo/*!*/ MethodRetry { get { return _MethodRetry ?? (_MethodRetry = GetMethod(typeof(RubyOps), "MethodRetry")); } }
        public static MethodInfo/*!*/ EvalRetry { get { return _EvalRetry ?? (_EvalRetry = GetMethod(typeof(RubyOps), "EvalRetry")); } }
        public static MethodInfo/*!*/ BlockBreak { get { return _BlockBreak ?? (_BlockBreak = GetMethod(typeof(RubyOps), "BlockBreak")); } }
        public static MethodInfo/*!*/ MethodBreak { get { return _MethodBreak ?? (_MethodBreak = GetMethod(typeof(RubyOps), "MethodBreak")); } }
        public static MethodInfo/*!*/ EvalBreak { get { return _EvalBreak ?? (_EvalBreak = GetMethod(typeof(RubyOps), "EvalBreak")); } }
        public static MethodInfo/*!*/ MethodNext { get { return _MethodNext ?? (_MethodNext = GetMethod(typeof(RubyOps), "MethodNext")); } }
        public static MethodInfo/*!*/ EvalNext { get { return _EvalNext ?? (_EvalNext = GetMethod(typeof(RubyOps), "EvalNext")); } }
        public static MethodInfo/*!*/ MethodRedo { get { return _MethodRedo ?? (_MethodRedo = GetMethod(typeof(RubyOps), "MethodRedo")); } }
        public static MethodInfo/*!*/ EvalRedo { get { return _EvalRedo ?? (_EvalRedo = GetMethod(typeof(RubyOps), "EvalRedo")); } }
        public static MethodInfo/*!*/ BlockReturn { get { return _BlockReturn ?? (_BlockReturn = GetMethod(typeof(RubyOps), "BlockReturn")); } }
        public static MethodInfo/*!*/ EvalReturn { get { return _EvalReturn ?? (_EvalReturn = GetMethod(typeof(RubyOps), "EvalReturn")); } }
        public static MethodInfo/*!*/ BlockYield { get { return _BlockYield ?? (_BlockYield = GetMethod(typeof(RubyOps), "BlockYield")); } }
        public static MethodInfo/*!*/ MethodYield { get { return _MethodYield ?? (_MethodYield = GetMethod(typeof(RubyOps), "MethodYield")); } }
        public static MethodInfo/*!*/ EvalYield { get { return _EvalYield ?? (_EvalYield = GetMethod(typeof(RubyOps), "EvalYield")); } }
        public static MethodInfo/*!*/ MethodProcCall { get { return _MethodProcCall ?? (_MethodProcCall = GetMethod(typeof(RubyOps), "MethodProcCall")); } }
        public static MethodInfo/*!*/ CanRescue { get { return _CanRescue ?? (_CanRescue = GetMethod(typeof(RubyOps), "CanRescue")); } }
        public static MethodInfo/*!*/ IsRetrySingleton { get { return _IsRetrySingleton ?? (_IsRetrySingleton = GetMethod(typeof(RubyOps), "IsRetrySingleton")); } }
        public static MethodInfo/*!*/ PropagateRetrySingleton { get { return _PropagateRetrySingleton ?? (_PropagateRetrySingleton = GetMethod(typeof(RubyOps), "PropagateRetrySingleton")); } }
        public static MethodInfo/*!*/ GetRetrySingleton { get { return _GetRetrySingleton ?? (_GetRetrySingleton = GetMethod(typeof(RubyOps), "GetRetrySingleton")); } }
        public static MethodInfo/*!*/ CreateMainTopLevelScope { get { return _CreateMainTopLevelScope ?? (_CreateMainTopLevelScope = GetMethod(typeof(RubyOps), "CreateMainTopLevelScope")); } }
        public static MethodInfo/*!*/ CreateTopLevelHostedScope { get { return _CreateTopLevelHostedScope ?? (_CreateTopLevelHostedScope = GetMethod(typeof(RubyOps), "CreateTopLevelHostedScope")); } }
        public static MethodInfo/*!*/ CreateTopLevelScope { get { return _CreateTopLevelScope ?? (_CreateTopLevelScope = GetMethod(typeof(RubyOps), "CreateTopLevelScope")); } }
        public static MethodInfo/*!*/ CreateWrappedTopLevelScope { get { return _CreateWrappedTopLevelScope ?? (_CreateWrappedTopLevelScope = GetMethod(typeof(RubyOps), "CreateWrappedTopLevelScope")); } }
        public static MethodInfo/*!*/ CreateModuleEvalScope { get { return _CreateModuleEvalScope ?? (_CreateModuleEvalScope = GetMethod(typeof(RubyOps), "CreateModuleEvalScope")); } }
        public static MethodInfo/*!*/ CreateModuleScope { get { return _CreateModuleScope ?? (_CreateModuleScope = GetMethod(typeof(RubyOps), "CreateModuleScope")); } }
        public static MethodInfo/*!*/ CreateMethodScope { get { return _CreateMethodScope ?? (_CreateMethodScope = GetMethod(typeof(RubyOps), "CreateMethodScope")); } }
        public static MethodInfo/*!*/ CreateBlockScope { get { return _CreateBlockScope ?? (_CreateBlockScope = GetMethod(typeof(RubyOps), "CreateBlockScope")); } }
        public static MethodInfo/*!*/ TraceMethodCall { get { return _TraceMethodCall ?? (_TraceMethodCall = GetMethod(typeof(RubyOps), "TraceMethodCall")); } }
        public static MethodInfo/*!*/ TraceMethodReturn { get { return _TraceMethodReturn ?? (_TraceMethodReturn = GetMethod(typeof(RubyOps), "TraceMethodReturn")); } }
        public static MethodInfo/*!*/ TraceBlockCall { get { return _TraceBlockCall ?? (_TraceBlockCall = GetMethod(typeof(RubyOps), "TraceBlockCall")); } }
        public static MethodInfo/*!*/ TraceBlockReturn { get { return _TraceBlockReturn ?? (_TraceBlockReturn = GetMethod(typeof(RubyOps), "TraceBlockReturn")); } }
        public static MethodInfo/*!*/ PrintInteractiveResult { get { return _PrintInteractiveResult ?? (_PrintInteractiveResult = GetMethod(typeof(RubyOps), "PrintInteractiveResult")); } }
        public static MethodInfo/*!*/ GetLocalVariable { get { return _GetLocalVariable ?? (_GetLocalVariable = GetMethod(typeof(RubyOps), "GetLocalVariable")); } }
        public static MethodInfo/*!*/ SetLocalVariable { get { return _SetLocalVariable ?? (_SetLocalVariable = GetMethod(typeof(RubyOps), "SetLocalVariable")); } }
        public static MethodInfo/*!*/ GetContextFromScope { get { return _GetContextFromScope ?? (_GetContextFromScope = GetMethod(typeof(RubyOps), "GetContextFromScope")); } }
        public static MethodInfo/*!*/ GetContextFromMethod { get { return _GetContextFromMethod ?? (_GetContextFromMethod = GetMethod(typeof(RubyOps), "GetContextFromMethod")); } }
        public static MethodInfo/*!*/ GetContextFromBlockParam { get { return _GetContextFromBlockParam ?? (_GetContextFromBlockParam = GetMethod(typeof(RubyOps), "GetContextFromBlockParam")); } }
        public static MethodInfo/*!*/ GetContextFromProc { get { return _GetContextFromProc ?? (_GetContextFromProc = GetMethod(typeof(RubyOps), "GetContextFromProc")); } }
        public static MethodInfo/*!*/ GetEmptyScope { get { return _GetEmptyScope ?? (_GetEmptyScope = GetMethod(typeof(RubyOps), "GetEmptyScope")); } }
        public static MethodInfo/*!*/ DefineBlock { get { return _DefineBlock ?? (_DefineBlock = GetMethod(typeof(RubyOps), "DefineBlock")); } }
        public static MethodInfo/*!*/ InitializeBlock { get { return _InitializeBlock ?? (_InitializeBlock = GetMethod(typeof(RubyOps), "InitializeBlock")); } }
        public static MethodInfo/*!*/ Yield0 { get { return _Yield0 ?? (_Yield0 = GetMethod(typeof(RubyOps), "Yield0")); } }
        public static MethodInfo/*!*/ Yield1 { get { return _Yield1 ?? (_Yield1 = GetMethod(typeof(RubyOps), "Yield1")); } }
        public static MethodInfo/*!*/ Yield2 { get { return _Yield2 ?? (_Yield2 = GetMethod(typeof(RubyOps), "Yield2")); } }
        public static MethodInfo/*!*/ Yield3 { get { return _Yield3 ?? (_Yield3 = GetMethod(typeof(RubyOps), "Yield3")); } }
        public static MethodInfo/*!*/ Yield4 { get { return _Yield4 ?? (_Yield4 = GetMethod(typeof(RubyOps), "Yield4")); } }
        public static MethodInfo/*!*/ YieldN { get { return _YieldN ?? (_YieldN = GetMethod(typeof(RubyOps), "YieldN")); } }
        public static MethodInfo/*!*/ YieldSplat0 { get { return _YieldSplat0 ?? (_YieldSplat0 = GetMethod(typeof(RubyOps), "YieldSplat0")); } }
        public static MethodInfo/*!*/ YieldSplat1 { get { return _YieldSplat1 ?? (_YieldSplat1 = GetMethod(typeof(RubyOps), "YieldSplat1")); } }
        public static MethodInfo/*!*/ YieldSplat2 { get { return _YieldSplat2 ?? (_YieldSplat2 = GetMethod(typeof(RubyOps), "YieldSplat2")); } }
        public static MethodInfo/*!*/ YieldSplat3 { get { return _YieldSplat3 ?? (_YieldSplat3 = GetMethod(typeof(RubyOps), "YieldSplat3")); } }
        public static MethodInfo/*!*/ YieldSplat4 { get { return _YieldSplat4 ?? (_YieldSplat4 = GetMethod(typeof(RubyOps), "YieldSplat4")); } }
        public static MethodInfo/*!*/ YieldSplatN { get { return _YieldSplatN ?? (_YieldSplatN = GetMethod(typeof(RubyOps), "YieldSplatN")); } }
        public static MethodInfo/*!*/ YieldSplatNRhs { get { return _YieldSplatNRhs ?? (_YieldSplatNRhs = GetMethod(typeof(RubyOps), "YieldSplatNRhs")); } }
        public static MethodInfo/*!*/ DefineMethod { get { return _DefineMethod ?? (_DefineMethod = GetMethod(typeof(RubyOps), "DefineMethod")); } }
        public static MethodInfo/*!*/ MethodDefined { get { return _MethodDefined ?? (_MethodDefined = GetMethod(typeof(RubyOps), "MethodDefined")); } }
        public static MethodInfo/*!*/ AliasMethod { get { return _AliasMethod ?? (_AliasMethod = GetMethod(typeof(RubyOps), "AliasMethod")); } }
        public static MethodInfo/*!*/ UndefineMethod { get { return _UndefineMethod ?? (_UndefineMethod = GetMethod(typeof(RubyOps), "UndefineMethod")); } }
        public static MethodInfo/*!*/ IsDefinedMethod { get { return _IsDefinedMethod ?? (_IsDefinedMethod = GetMethod(typeof(RubyOps), "IsDefinedMethod")); } }
        public static MethodInfo/*!*/ DefineGlobalModule { get { return _DefineGlobalModule ?? (_DefineGlobalModule = GetMethod(typeof(RubyOps), "DefineGlobalModule")); } }
        public static MethodInfo/*!*/ DefineNestedModule { get { return _DefineNestedModule ?? (_DefineNestedModule = GetMethod(typeof(RubyOps), "DefineNestedModule")); } }
        public static MethodInfo/*!*/ DefineModule { get { return _DefineModule ?? (_DefineModule = GetMethod(typeof(RubyOps), "DefineModule")); } }
        public static MethodInfo/*!*/ DefineSingletonClass { get { return _DefineSingletonClass ?? (_DefineSingletonClass = GetMethod(typeof(RubyOps), "DefineSingletonClass")); } }
        public static MethodInfo/*!*/ DefineGlobalClass { get { return _DefineGlobalClass ?? (_DefineGlobalClass = GetMethod(typeof(RubyOps), "DefineGlobalClass")); } }
        public static MethodInfo/*!*/ DefineNestedClass { get { return _DefineNestedClass ?? (_DefineNestedClass = GetMethod(typeof(RubyOps), "DefineNestedClass")); } }
        public static MethodInfo/*!*/ DefineClass { get { return _DefineClass ?? (_DefineClass = GetMethod(typeof(RubyOps), "DefineClass")); } }
        public static MethodInfo/*!*/ GetGlobalConstant { get { return _GetGlobalConstant ?? (_GetGlobalConstant = GetMethod(typeof(RubyOps), "GetGlobalConstant")); } }
        public static MethodInfo/*!*/ GetUnqualifiedConstant { get { return _GetUnqualifiedConstant ?? (_GetUnqualifiedConstant = GetMethod(typeof(RubyOps), "GetUnqualifiedConstant")); } }
        public static MethodInfo/*!*/ GetQualifiedConstant { get { return _GetQualifiedConstant ?? (_GetQualifiedConstant = GetMethod(typeof(RubyOps), "GetQualifiedConstant")); } }
        public static MethodInfo/*!*/ IsDefinedGlobalConstant { get { return _IsDefinedGlobalConstant ?? (_IsDefinedGlobalConstant = GetMethod(typeof(RubyOps), "IsDefinedGlobalConstant")); } }
        public static MethodInfo/*!*/ IsDefinedUnqualifiedConstant { get { return _IsDefinedUnqualifiedConstant ?? (_IsDefinedUnqualifiedConstant = GetMethod(typeof(RubyOps), "IsDefinedUnqualifiedConstant")); } }
        
        public static MethodInfo/*!*/ CreateRegex(string/*!*/ suffix) {
            switch (suffix) {
                case "N": return CreateRegexN;
                case "B": return CreateRegexB;
                case "E": return CreateRegexE;
                case "U": return CreateRegexU;
                case "M": return CreateRegexM;
                case "BM": return CreateRegexBM;
                case "UM": return CreateRegexUM;
                case "EM": return CreateRegexEM;
                case "MB": return CreateRegexMB;
                case "MU": return CreateRegexMU;
                case "ME": return CreateRegexME;
                case "MM": return CreateRegexMM;
            }
            throw Assert.Unreachable;
        }
        
        public static MethodInfo/*!*/ CreateMutableString(string/*!*/ suffix) {
            switch (suffix) {
                case "N": return CreateMutableStringN;
                case "B": return CreateMutableStringB;
                case "E": return CreateMutableStringE;
                case "U": return CreateMutableStringU;
                case "M": return CreateMutableStringM;
                case "BM": return CreateMutableStringBM;
                case "UM": return CreateMutableStringUM;
                case "EM": return CreateMutableStringEM;
                case "MB": return CreateMutableStringMB;
                case "MU": return CreateMutableStringMU;
                case "ME": return CreateMutableStringME;
                case "MM": return CreateMutableStringMM;
            }
            throw Assert.Unreachable;
        }
        
        public static MethodInfo/*!*/ CreateSymbol(string/*!*/ suffix) {
            switch (suffix) {
                case "N": return CreateSymbolN;
                case "B": return CreateSymbolB;
                case "E": return CreateSymbolE;
                case "U": return CreateSymbolU;
                case "M": return CreateSymbolM;
                case "BM": return CreateSymbolBM;
                case "UM": return CreateSymbolUM;
                case "EM": return CreateSymbolEM;
                case "MB": return CreateSymbolMB;
                case "MU": return CreateSymbolMU;
                case "ME": return CreateSymbolME;
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
}
