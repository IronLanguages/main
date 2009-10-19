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
using System.Diagnostics;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler {
    public static partial class Methods {
        public static MethodInfo/*!*/ AliasGlobalVariable { get { return _AliasGlobalVariable ?? (_AliasGlobalVariable = GetMethod(typeof(RubyOps), "AliasGlobalVariable")); } }
        private static MethodInfo _AliasGlobalVariable;
        public static MethodInfo/*!*/ AliasMethod { get { return _AliasMethod ?? (_AliasMethod = GetMethod(typeof(RubyOps), "AliasMethod")); } }
        private static MethodInfo _AliasMethod;
        public static MethodInfo/*!*/ AllocateStructInstance { get { return _AllocateStructInstance ?? (_AllocateStructInstance = GetMethod(typeof(RubyOps), "AllocateStructInstance")); } }
        private static MethodInfo _AllocateStructInstance;
        public static MethodInfo/*!*/ BlockBreak { get { return _BlockBreak ?? (_BlockBreak = GetMethod(typeof(RubyOps), "BlockBreak")); } }
        private static MethodInfo _BlockBreak;
        public static MethodInfo/*!*/ BlockRetry { get { return _BlockRetry ?? (_BlockRetry = GetMethod(typeof(RubyOps), "BlockRetry")); } }
        private static MethodInfo _BlockRetry;
        public static MethodInfo/*!*/ BlockReturn { get { return _BlockReturn ?? (_BlockReturn = GetMethod(typeof(RubyOps), "BlockReturn")); } }
        private static MethodInfo _BlockReturn;
        public static MethodInfo/*!*/ BlockYield { get { return _BlockYield ?? (_BlockYield = GetMethod(typeof(RubyOps), "BlockYield")); } }
        private static MethodInfo _BlockYield;
        public static MethodInfo/*!*/ CanRescue { get { return _CanRescue ?? (_CanRescue = GetMethod(typeof(RubyOps), "CanRescue")); } }
        private static MethodInfo _CanRescue;
        public static MethodInfo/*!*/ CompareDefaultException { get { return _CompareDefaultException ?? (_CompareDefaultException = GetMethod(typeof(RubyOps), "CompareDefaultException")); } }
        private static MethodInfo _CompareDefaultException;
        public static MethodInfo/*!*/ CompareException { get { return _CompareException ?? (_CompareException = GetMethod(typeof(RubyOps), "CompareException")); } }
        private static MethodInfo _CompareException;
        public static MethodInfo/*!*/ CompareSplattedExceptions { get { return _CompareSplattedExceptions ?? (_CompareSplattedExceptions = GetMethod(typeof(RubyOps), "CompareSplattedExceptions")); } }
        private static MethodInfo _CompareSplattedExceptions;
        public static MethodInfo/*!*/ ConvertBignumToFixnum { get { return _ConvertBignumToFixnum ?? (_ConvertBignumToFixnum = GetMethod(typeof(RubyOps), "ConvertBignumToFixnum")); } }
        private static MethodInfo _ConvertBignumToFixnum;
        public static MethodInfo/*!*/ ConvertBignumToFloat { get { return _ConvertBignumToFloat ?? (_ConvertBignumToFloat = GetMethod(typeof(RubyOps), "ConvertBignumToFloat")); } }
        private static MethodInfo _ConvertBignumToFloat;
        public static MethodInfo/*!*/ ConvertDoubleToFixnum { get { return _ConvertDoubleToFixnum ?? (_ConvertDoubleToFixnum = GetMethod(typeof(RubyOps), "ConvertDoubleToFixnum")); } }
        private static MethodInfo _ConvertDoubleToFixnum;
        public static MethodInfo/*!*/ ConvertFixnumToSymbol { get { return _ConvertFixnumToSymbol ?? (_ConvertFixnumToSymbol = GetMethod(typeof(RubyOps), "ConvertFixnumToSymbol")); } }
        private static MethodInfo _ConvertFixnumToSymbol;
        public static MethodInfo/*!*/ ConvertMutableStringToFloat { get { return _ConvertMutableStringToFloat ?? (_ConvertMutableStringToFloat = GetMethod(typeof(RubyOps), "ConvertMutableStringToFloat")); } }
        private static MethodInfo _ConvertMutableStringToFloat;
        public static MethodInfo/*!*/ ConvertMutableStringToSymbol { get { return _ConvertMutableStringToSymbol ?? (_ConvertMutableStringToSymbol = GetMethod(typeof(RubyOps), "ConvertMutableStringToSymbol")); } }
        private static MethodInfo _ConvertMutableStringToSymbol;
        public static MethodInfo/*!*/ ConvertStringToFloat { get { return _ConvertStringToFloat ?? (_ConvertStringToFloat = GetMethod(typeof(RubyOps), "ConvertStringToFloat")); } }
        private static MethodInfo _ConvertStringToFloat;
        public static MethodInfo/*!*/ ConvertSymbolIdToSymbol { get { return _ConvertSymbolIdToSymbol ?? (_ConvertSymbolIdToSymbol = GetMethod(typeof(RubyOps), "ConvertSymbolIdToSymbol")); } }
        private static MethodInfo _ConvertSymbolIdToSymbol;
        public static MethodInfo/*!*/ CreateArgumentsError { get { return _CreateArgumentsError ?? (_CreateArgumentsError = GetMethod(typeof(RubyOps), "CreateArgumentsError")); } }
        private static MethodInfo _CreateArgumentsError;
        public static MethodInfo/*!*/ CreateArgumentsErrorForMissingBlock { get { return _CreateArgumentsErrorForMissingBlock ?? (_CreateArgumentsErrorForMissingBlock = GetMethod(typeof(RubyOps), "CreateArgumentsErrorForMissingBlock")); } }
        private static MethodInfo _CreateArgumentsErrorForMissingBlock;
        public static MethodInfo/*!*/ CreateArgumentsErrorForProc { get { return _CreateArgumentsErrorForProc ?? (_CreateArgumentsErrorForProc = GetMethod(typeof(RubyOps), "CreateArgumentsErrorForProc")); } }
        private static MethodInfo _CreateArgumentsErrorForProc;
        public static MethodInfo/*!*/ CreateBfcForLibraryMethod { get { return _CreateBfcForLibraryMethod ?? (_CreateBfcForLibraryMethod = GetMethod(typeof(RubyOps), "CreateBfcForLibraryMethod")); } }
        private static MethodInfo _CreateBfcForLibraryMethod;
        public static MethodInfo/*!*/ CreateBfcForMethodProcCall { get { return _CreateBfcForMethodProcCall ?? (_CreateBfcForMethodProcCall = GetMethod(typeof(RubyOps), "CreateBfcForMethodProcCall")); } }
        private static MethodInfo _CreateBfcForMethodProcCall;
        public static MethodInfo/*!*/ CreateBfcForProcCall { get { return _CreateBfcForProcCall ?? (_CreateBfcForProcCall = GetMethod(typeof(RubyOps), "CreateBfcForProcCall")); } }
        private static MethodInfo _CreateBfcForProcCall;
        public static MethodInfo/*!*/ CreateBfcForYield { get { return _CreateBfcForYield ?? (_CreateBfcForYield = GetMethod(typeof(RubyOps), "CreateBfcForYield")); } }
        private static MethodInfo _CreateBfcForYield;
        public static MethodInfo/*!*/ CreateBlockScope { get { return _CreateBlockScope ?? (_CreateBlockScope = GetMethod(typeof(RubyOps), "CreateBlockScope")); } }
        private static MethodInfo _CreateBlockScope;
        public static MethodInfo/*!*/ CreateBoundMember { get { return _CreateBoundMember ?? (_CreateBoundMember = GetMethod(typeof(RubyOps), "CreateBoundMember")); } }
        private static MethodInfo _CreateBoundMember;
        public static MethodInfo/*!*/ CreateBoundMissingMember { get { return _CreateBoundMissingMember ?? (_CreateBoundMissingMember = GetMethod(typeof(RubyOps), "CreateBoundMissingMember")); } }
        private static MethodInfo _CreateBoundMissingMember;
        public static MethodInfo/*!*/ CreateDefaultInstance { get { return _CreateDefaultInstance ?? (_CreateDefaultInstance = GetMethod(typeof(RubyOps), "CreateDefaultInstance")); } }
        private static MethodInfo _CreateDefaultInstance;
        public static MethodInfo/*!*/ CreateDelegateFromMethod { get { return _CreateDelegateFromMethod ?? (_CreateDelegateFromMethod = GetMethod(typeof(RubyOps), "CreateDelegateFromMethod")); } }
        private static MethodInfo _CreateDelegateFromMethod;
        public static MethodInfo/*!*/ CreateDelegateFromProc { get { return _CreateDelegateFromProc ?? (_CreateDelegateFromProc = GetMethod(typeof(RubyOps), "CreateDelegateFromProc")); } }
        private static MethodInfo _CreateDelegateFromProc;
        public static MethodInfo/*!*/ CreateEncoding { get { return _CreateEncoding ?? (_CreateEncoding = GetMethod(typeof(RubyOps), "CreateEncoding")); } }
        private static MethodInfo _CreateEncoding;
        public static MethodInfo/*!*/ CreateEvent { get { return _CreateEvent ?? (_CreateEvent = GetMethod(typeof(RubyOps), "CreateEvent")); } }
        private static MethodInfo _CreateEvent;
        public static MethodInfo/*!*/ CreateExclusiveIntegerRange { get { return _CreateExclusiveIntegerRange ?? (_CreateExclusiveIntegerRange = GetMethod(typeof(RubyOps), "CreateExclusiveIntegerRange")); } }
        private static MethodInfo _CreateExclusiveIntegerRange;
        public static MethodInfo/*!*/ CreateExclusiveRange { get { return _CreateExclusiveRange ?? (_CreateExclusiveRange = GetMethod(typeof(RubyOps), "CreateExclusiveRange")); } }
        private static MethodInfo _CreateExclusiveRange;
        public static MethodInfo/*!*/ CreateInclusiveIntegerRange { get { return _CreateInclusiveIntegerRange ?? (_CreateInclusiveIntegerRange = GetMethod(typeof(RubyOps), "CreateInclusiveIntegerRange")); } }
        private static MethodInfo _CreateInclusiveIntegerRange;
        public static MethodInfo/*!*/ CreateInclusiveRange { get { return _CreateInclusiveRange ?? (_CreateInclusiveRange = GetMethod(typeof(RubyOps), "CreateInclusiveRange")); } }
        private static MethodInfo _CreateInclusiveRange;
        public static MethodInfo/*!*/ CreateMethodScope { get { return _CreateMethodScope ?? (_CreateMethodScope = GetMethod(typeof(RubyOps), "CreateMethodScope")); } }
        private static MethodInfo _CreateMethodScope;
        public static MethodInfo/*!*/ CreateModuleScope { get { return _CreateModuleScope ?? (_CreateModuleScope = GetMethod(typeof(RubyOps), "CreateModuleScope")); } }
        private static MethodInfo _CreateModuleScope;
        public static MethodInfo/*!*/ CreateMutableStringL { get { return _CreateMutableStringL ?? (_CreateMutableStringL = GetMethod(typeof(RubyOps), "CreateMutableStringL")); } }
        private static MethodInfo _CreateMutableStringL;
        public static MethodInfo/*!*/ CreateMutableStringLM { get { return _CreateMutableStringLM ?? (_CreateMutableStringLM = GetMethod(typeof(RubyOps), "CreateMutableStringLM")); } }
        private static MethodInfo _CreateMutableStringLM;
        public static MethodInfo/*!*/ CreateMutableStringM { get { return _CreateMutableStringM ?? (_CreateMutableStringM = GetMethod(typeof(RubyOps), "CreateMutableStringM")); } }
        private static MethodInfo _CreateMutableStringM;
        public static MethodInfo/*!*/ CreateMutableStringML { get { return _CreateMutableStringML ?? (_CreateMutableStringML = GetMethod(typeof(RubyOps), "CreateMutableStringML")); } }
        private static MethodInfo _CreateMutableStringML;
        public static MethodInfo/*!*/ CreateMutableStringMM { get { return _CreateMutableStringMM ?? (_CreateMutableStringMM = GetMethod(typeof(RubyOps), "CreateMutableStringMM")); } }
        private static MethodInfo _CreateMutableStringMM;
        public static MethodInfo/*!*/ CreateMutableStringN { get { return _CreateMutableStringN ?? (_CreateMutableStringN = GetMethod(typeof(RubyOps), "CreateMutableStringN")); } }
        private static MethodInfo _CreateMutableStringN;
        public static MethodInfo/*!*/ CreateRegexL { get { return _CreateRegexL ?? (_CreateRegexL = GetMethod(typeof(RubyOps), "CreateRegexL")); } }
        private static MethodInfo _CreateRegexL;
        public static MethodInfo/*!*/ CreateRegexLM { get { return _CreateRegexLM ?? (_CreateRegexLM = GetMethod(typeof(RubyOps), "CreateRegexLM")); } }
        private static MethodInfo _CreateRegexLM;
        public static MethodInfo/*!*/ CreateRegexM { get { return _CreateRegexM ?? (_CreateRegexM = GetMethod(typeof(RubyOps), "CreateRegexM")); } }
        private static MethodInfo _CreateRegexM;
        public static MethodInfo/*!*/ CreateRegexML { get { return _CreateRegexML ?? (_CreateRegexML = GetMethod(typeof(RubyOps), "CreateRegexML")); } }
        private static MethodInfo _CreateRegexML;
        public static MethodInfo/*!*/ CreateRegexMM { get { return _CreateRegexMM ?? (_CreateRegexMM = GetMethod(typeof(RubyOps), "CreateRegexMM")); } }
        private static MethodInfo _CreateRegexMM;
        public static MethodInfo/*!*/ CreateRegexN { get { return _CreateRegexN ?? (_CreateRegexN = GetMethod(typeof(RubyOps), "CreateRegexN")); } }
        private static MethodInfo _CreateRegexN;
        public static MethodInfo/*!*/ CreateRfcForMethod { get { return _CreateRfcForMethod ?? (_CreateRfcForMethod = GetMethod(typeof(RubyOps), "CreateRfcForMethod")); } }
        private static MethodInfo _CreateRfcForMethod;
        public static MethodInfo/*!*/ CreateStructInstance { get { return _CreateStructInstance ?? (_CreateStructInstance = GetMethod(typeof(RubyOps), "CreateStructInstance")); } }
        private static MethodInfo _CreateStructInstance;
        public static MethodInfo/*!*/ CreateSymbolL { get { return _CreateSymbolL ?? (_CreateSymbolL = GetMethod(typeof(RubyOps), "CreateSymbolL")); } }
        private static MethodInfo _CreateSymbolL;
        public static MethodInfo/*!*/ CreateSymbolLM { get { return _CreateSymbolLM ?? (_CreateSymbolLM = GetMethod(typeof(RubyOps), "CreateSymbolLM")); } }
        private static MethodInfo _CreateSymbolLM;
        public static MethodInfo/*!*/ CreateSymbolM { get { return _CreateSymbolM ?? (_CreateSymbolM = GetMethod(typeof(RubyOps), "CreateSymbolM")); } }
        private static MethodInfo _CreateSymbolM;
        public static MethodInfo/*!*/ CreateSymbolML { get { return _CreateSymbolML ?? (_CreateSymbolML = GetMethod(typeof(RubyOps), "CreateSymbolML")); } }
        private static MethodInfo _CreateSymbolML;
        public static MethodInfo/*!*/ CreateSymbolMM { get { return _CreateSymbolMM ?? (_CreateSymbolMM = GetMethod(typeof(RubyOps), "CreateSymbolMM")); } }
        private static MethodInfo _CreateSymbolMM;
        public static MethodInfo/*!*/ CreateSymbolN { get { return _CreateSymbolN ?? (_CreateSymbolN = GetMethod(typeof(RubyOps), "CreateSymbolN")); } }
        private static MethodInfo _CreateSymbolN;
        public static MethodInfo/*!*/ CreateTypeConversionError { get { return _CreateTypeConversionError ?? (_CreateTypeConversionError = GetMethod(typeof(RubyOps), "CreateTypeConversionError")); } }
        private static MethodInfo _CreateTypeConversionError;
        public static MethodInfo/*!*/ CreateVector { get { return _CreateVector ?? (_CreateVector = GetMethod(typeof(RubyOps), "CreateVector")); } }
        private static MethodInfo _CreateVector;
        public static MethodInfo/*!*/ CreateVectorWithValues { get { return _CreateVectorWithValues ?? (_CreateVectorWithValues = GetMethod(typeof(RubyOps), "CreateVectorWithValues")); } }
        private static MethodInfo _CreateVectorWithValues;
        public static MethodInfo/*!*/ DefineBlock { get { return _DefineBlock ?? (_DefineBlock = GetMethod(typeof(RubyOps), "DefineBlock")); } }
        private static MethodInfo _DefineBlock;
        public static MethodInfo/*!*/ DefineClass { get { return _DefineClass ?? (_DefineClass = GetMethod(typeof(RubyOps), "DefineClass")); } }
        private static MethodInfo _DefineClass;
        public static MethodInfo/*!*/ DefineGlobalClass { get { return _DefineGlobalClass ?? (_DefineGlobalClass = GetMethod(typeof(RubyOps), "DefineGlobalClass")); } }
        private static MethodInfo _DefineGlobalClass;
        public static MethodInfo/*!*/ DefineGlobalModule { get { return _DefineGlobalModule ?? (_DefineGlobalModule = GetMethod(typeof(RubyOps), "DefineGlobalModule")); } }
        private static MethodInfo _DefineGlobalModule;
        public static MethodInfo/*!*/ DefineMethod { get { return _DefineMethod ?? (_DefineMethod = GetMethod(typeof(RubyOps), "DefineMethod")); } }
        private static MethodInfo _DefineMethod;
        public static MethodInfo/*!*/ DefineModule { get { return _DefineModule ?? (_DefineModule = GetMethod(typeof(RubyOps), "DefineModule")); } }
        private static MethodInfo _DefineModule;
        public static MethodInfo/*!*/ DefineNestedClass { get { return _DefineNestedClass ?? (_DefineNestedClass = GetMethod(typeof(RubyOps), "DefineNestedClass")); } }
        private static MethodInfo _DefineNestedClass;
        public static MethodInfo/*!*/ DefineNestedModule { get { return _DefineNestedModule ?? (_DefineNestedModule = GetMethod(typeof(RubyOps), "DefineNestedModule")); } }
        private static MethodInfo _DefineNestedModule;
        public static MethodInfo/*!*/ DefineSingletonClass { get { return _DefineSingletonClass ?? (_DefineSingletonClass = GetMethod(typeof(RubyOps), "DefineSingletonClass")); } }
        private static MethodInfo _DefineSingletonClass;
        public static MethodInfo/*!*/ DeserializeObject { get { return _DeserializeObject ?? (_DeserializeObject = GetMethod(typeof(RubyOps), "DeserializeObject")); } }
        private static MethodInfo _DeserializeObject;
        public static MethodInfo/*!*/ EnterLoop { get { return _EnterLoop ?? (_EnterLoop = GetMethod(typeof(RubyOps), "EnterLoop")); } }
        private static MethodInfo _EnterLoop;
        public static MethodInfo/*!*/ EnterRescue { get { return _EnterRescue ?? (_EnterRescue = GetMethod(typeof(RubyOps), "EnterRescue")); } }
        private static MethodInfo _EnterRescue;
        public static MethodInfo/*!*/ EvalBreak { get { return _EvalBreak ?? (_EvalBreak = GetMethod(typeof(RubyOps), "EvalBreak")); } }
        private static MethodInfo _EvalBreak;
        public static MethodInfo/*!*/ EvalNext { get { return _EvalNext ?? (_EvalNext = GetMethod(typeof(RubyOps), "EvalNext")); } }
        private static MethodInfo _EvalNext;
        public static MethodInfo/*!*/ EvalRedo { get { return _EvalRedo ?? (_EvalRedo = GetMethod(typeof(RubyOps), "EvalRedo")); } }
        private static MethodInfo _EvalRedo;
        public static MethodInfo/*!*/ EvalRetry { get { return _EvalRetry ?? (_EvalRetry = GetMethod(typeof(RubyOps), "EvalRetry")); } }
        private static MethodInfo _EvalRetry;
        public static MethodInfo/*!*/ EvalReturn { get { return _EvalReturn ?? (_EvalReturn = GetMethod(typeof(RubyOps), "EvalReturn")); } }
        private static MethodInfo _EvalReturn;
        public static MethodInfo/*!*/ EvalYield { get { return _EvalYield ?? (_EvalYield = GetMethod(typeof(RubyOps), "EvalYield")); } }
        private static MethodInfo _EvalYield;
        public static MethodInfo/*!*/ ExistsUnsplat { get { return _ExistsUnsplat ?? (_ExistsUnsplat = GetMethod(typeof(RubyOps), "ExistsUnsplat")); } }
        private static MethodInfo _ExistsUnsplat;
        public static MethodInfo/*!*/ FilterBlockException { get { return _FilterBlockException ?? (_FilterBlockException = GetMethod(typeof(RubyOps), "FilterBlockException")); } }
        private static MethodInfo _FilterBlockException;
        public static MethodInfo/*!*/ FreezeObject { get { return _FreezeObject ?? (_FreezeObject = GetMethod(typeof(RubyOps), "FreezeObject")); } }
        private static MethodInfo _FreezeObject;
        public static MethodInfo/*!*/ GetArrayItem { get { return _GetArrayItem ?? (_GetArrayItem = GetMethod(typeof(RubyOps), "GetArrayItem")); } }
        private static MethodInfo _GetArrayItem;
        public static MethodInfo/*!*/ GetArraySuffix { get { return _GetArraySuffix ?? (_GetArraySuffix = GetMethod(typeof(RubyOps), "GetArraySuffix")); } }
        private static MethodInfo _GetArraySuffix;
        public static MethodInfo/*!*/ GetClassVariable { get { return _GetClassVariable ?? (_GetClassVariable = GetMethod(typeof(RubyOps), "GetClassVariable")); } }
        private static MethodInfo _GetClassVariable;
        public static MethodInfo/*!*/ GetContextFromBlockParam { get { return _GetContextFromBlockParam ?? (_GetContextFromBlockParam = GetMethod(typeof(RubyOps), "GetContextFromBlockParam")); } }
        private static MethodInfo _GetContextFromBlockParam;
        public static MethodInfo/*!*/ GetContextFromIRubyObject { get { return _GetContextFromIRubyObject ?? (_GetContextFromIRubyObject = GetMethod(typeof(RubyOps), "GetContextFromIRubyObject")); } }
        private static MethodInfo _GetContextFromIRubyObject;
        public static MethodInfo/*!*/ GetContextFromMethod { get { return _GetContextFromMethod ?? (_GetContextFromMethod = GetMethod(typeof(RubyOps), "GetContextFromMethod")); } }
        private static MethodInfo _GetContextFromMethod;
        public static MethodInfo/*!*/ GetContextFromModule { get { return _GetContextFromModule ?? (_GetContextFromModule = GetMethod(typeof(RubyOps), "GetContextFromModule")); } }
        private static MethodInfo _GetContextFromModule;
        public static MethodInfo/*!*/ GetContextFromProc { get { return _GetContextFromProc ?? (_GetContextFromProc = GetMethod(typeof(RubyOps), "GetContextFromProc")); } }
        private static MethodInfo _GetContextFromProc;
        public static MethodInfo/*!*/ GetContextFromScope { get { return _GetContextFromScope ?? (_GetContextFromScope = GetMethod(typeof(RubyOps), "GetContextFromScope")); } }
        private static MethodInfo _GetContextFromScope;
        public static MethodInfo/*!*/ GetCurrentException { get { return _GetCurrentException ?? (_GetCurrentException = GetMethod(typeof(RubyOps), "GetCurrentException")); } }
        private static MethodInfo _GetCurrentException;
        public static MethodInfo/*!*/ GetCurrentMatchData { get { return _GetCurrentMatchData ?? (_GetCurrentMatchData = GetMethod(typeof(RubyOps), "GetCurrentMatchData")); } }
        private static MethodInfo _GetCurrentMatchData;
        public static MethodInfo/*!*/ GetCurrentMatchGroup { get { return _GetCurrentMatchGroup ?? (_GetCurrentMatchGroup = GetMethod(typeof(RubyOps), "GetCurrentMatchGroup")); } }
        private static MethodInfo _GetCurrentMatchGroup;
        public static MethodInfo/*!*/ GetCurrentMatchLastGroup { get { return _GetCurrentMatchLastGroup ?? (_GetCurrentMatchLastGroup = GetMethod(typeof(RubyOps), "GetCurrentMatchLastGroup")); } }
        private static MethodInfo _GetCurrentMatchLastGroup;
        public static MethodInfo/*!*/ GetCurrentMatchPrefix { get { return _GetCurrentMatchPrefix ?? (_GetCurrentMatchPrefix = GetMethod(typeof(RubyOps), "GetCurrentMatchPrefix")); } }
        private static MethodInfo _GetCurrentMatchPrefix;
        public static MethodInfo/*!*/ GetCurrentMatchSuffix { get { return _GetCurrentMatchSuffix ?? (_GetCurrentMatchSuffix = GetMethod(typeof(RubyOps), "GetCurrentMatchSuffix")); } }
        private static MethodInfo _GetCurrentMatchSuffix;
        public static MethodInfo/*!*/ GetDefaultExceptionMessage { get { return _GetDefaultExceptionMessage ?? (_GetDefaultExceptionMessage = GetMethod(typeof(RubyOps), "GetDefaultExceptionMessage")); } }
        private static MethodInfo _GetDefaultExceptionMessage;
        public static MethodInfo/*!*/ GetEmptyScope { get { return _GetEmptyScope ?? (_GetEmptyScope = GetMethod(typeof(RubyOps), "GetEmptyScope")); } }
        private static MethodInfo _GetEmptyScope;
        public static MethodInfo/*!*/ GetExpressionQualifiedConstant { get { return _GetExpressionQualifiedConstant ?? (_GetExpressionQualifiedConstant = GetMethod(typeof(RubyOps), "GetExpressionQualifiedConstant")); } }
        private static MethodInfo _GetExpressionQualifiedConstant;
        public static MethodInfo/*!*/ GetGlobalMissingConstant { get { return _GetGlobalMissingConstant ?? (_GetGlobalMissingConstant = GetMethod(typeof(RubyOps), "GetGlobalMissingConstant")); } }
        private static MethodInfo _GetGlobalMissingConstant;
        public static MethodInfo/*!*/ GetGlobalVariable { get { return _GetGlobalVariable ?? (_GetGlobalVariable = GetMethod(typeof(RubyOps), "GetGlobalVariable")); } }
        private static MethodInfo _GetGlobalVariable;
        public static MethodInfo/*!*/ GetInstanceData { get { return _GetInstanceData ?? (_GetInstanceData = GetMethod(typeof(RubyOps), "GetInstanceData")); } }
        private static MethodInfo _GetInstanceData;
        public static MethodInfo/*!*/ GetInstanceVariable { get { return _GetInstanceVariable ?? (_GetInstanceVariable = GetMethod(typeof(RubyOps), "GetInstanceVariable")); } }
        private static MethodInfo _GetInstanceVariable;
        public static MethodInfo/*!*/ GetLocals { get { return _GetLocals ?? (_GetLocals = GetMethod(typeof(RubyOps), "GetLocals")); } }
        private static MethodInfo _GetLocals;
        public static MethodInfo/*!*/ GetLocalVariable { get { return _GetLocalVariable ?? (_GetLocalVariable = GetMethod(typeof(RubyOps), "GetLocalVariable")); } }
        private static MethodInfo _GetLocalVariable;
        public static MethodInfo/*!*/ GetMetaObject { get { return _GetMetaObject ?? (_GetMetaObject = GetMethod(typeof(RubyOps), "GetMetaObject")); } }
        private static MethodInfo _GetMetaObject;
        public static MethodInfo/*!*/ GetMethodBlockParameter { get { return _GetMethodBlockParameter ?? (_GetMethodBlockParameter = GetMethod(typeof(RubyOps), "GetMethodBlockParameter")); } }
        private static MethodInfo _GetMethodBlockParameter;
        public static MethodInfo/*!*/ GetMethodBlockParameterSelf { get { return _GetMethodBlockParameterSelf ?? (_GetMethodBlockParameterSelf = GetMethod(typeof(RubyOps), "GetMethodBlockParameterSelf")); } }
        private static MethodInfo _GetMethodBlockParameterSelf;
        public static MethodInfo/*!*/ GetMethodUnwinderReturnValue { get { return _GetMethodUnwinderReturnValue ?? (_GetMethodUnwinderReturnValue = GetMethod(typeof(RubyOps), "GetMethodUnwinderReturnValue")); } }
        private static MethodInfo _GetMethodUnwinderReturnValue;
        public static MethodInfo/*!*/ GetMissingConstant { get { return _GetMissingConstant ?? (_GetMissingConstant = GetMethod(typeof(RubyOps), "GetMissingConstant")); } }
        private static MethodInfo _GetMissingConstant;
        public static MethodInfo/*!*/ GetMutableStringBytes { get { return _GetMutableStringBytes ?? (_GetMutableStringBytes = GetMethod(typeof(RubyOps), "GetMutableStringBytes")); } }
        private static MethodInfo _GetMutableStringBytes;
        public static MethodInfo/*!*/ GetParentLocals { get { return _GetParentLocals ?? (_GetParentLocals = GetMethod(typeof(RubyOps), "GetParentLocals")); } }
        private static MethodInfo _GetParentLocals;
        public static MethodInfo/*!*/ GetParentScope { get { return _GetParentScope ?? (_GetParentScope = GetMethod(typeof(RubyOps), "GetParentScope")); } }
        private static MethodInfo _GetParentScope;
        public static MethodInfo/*!*/ GetProcSelf { get { return _GetProcSelf ?? (_GetProcSelf = GetMethod(typeof(RubyOps), "GetProcSelf")); } }
        private static MethodInfo _GetProcSelf;
        public static MethodInfo/*!*/ GetQualifiedConstant { get { return _GetQualifiedConstant ?? (_GetQualifiedConstant = GetMethod(typeof(RubyOps), "GetQualifiedConstant")); } }
        private static MethodInfo _GetQualifiedConstant;
        public static MethodInfo/*!*/ GetRetrySingleton { get { return _GetRetrySingleton ?? (_GetRetrySingleton = GetMethod(typeof(RubyOps), "GetRetrySingleton")); } }
        private static MethodInfo _GetRetrySingleton;
        public static MethodInfo/*!*/ GetSelfClassVersionHandle { get { return _GetSelfClassVersionHandle ?? (_GetSelfClassVersionHandle = GetMethod(typeof(RubyOps), "GetSelfClassVersionHandle")); } }
        private static MethodInfo _GetSelfClassVersionHandle;
        public static MethodInfo/*!*/ GetUnqualifiedConstant { get { return _GetUnqualifiedConstant ?? (_GetUnqualifiedConstant = GetMethod(typeof(RubyOps), "GetUnqualifiedConstant")); } }
        private static MethodInfo _GetUnqualifiedConstant;
        public static MethodInfo/*!*/ HookupEvent { get { return _HookupEvent ?? (_HookupEvent = GetMethod(typeof(RubyOps), "HookupEvent")); } }
        private static MethodInfo _HookupEvent;
        public static MethodInfo/*!*/ InitializeBlock { get { return _InitializeBlock ?? (_InitializeBlock = GetMethod(typeof(RubyOps), "InitializeBlock")); } }
        private static MethodInfo _InitializeBlock;
        public static MethodInfo/*!*/ InitializeScope { get { return _InitializeScope ?? (_InitializeScope = GetMethod(typeof(RubyOps), "InitializeScope")); } }
        private static MethodInfo _InitializeScope;
        public static MethodInfo/*!*/ InitializeScopeNoLocals { get { return _InitializeScopeNoLocals ?? (_InitializeScopeNoLocals = GetMethod(typeof(RubyOps), "InitializeScopeNoLocals")); } }
        private static MethodInfo _InitializeScopeNoLocals;
        public static MethodInfo/*!*/ InstantiateBlock { get { return _InstantiateBlock ?? (_InstantiateBlock = GetMethod(typeof(RubyOps), "InstantiateBlock")); } }
        private static MethodInfo _InstantiateBlock;
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
        public static MethodInfo/*!*/ IRubyObjectState_set_IsTainted { get { return _IRubyObjectState_set_IsTainted ?? (_IRubyObjectState_set_IsTainted = GetMethod(typeof(IRubyObjectState), "set_IsTainted")); } }
        private static MethodInfo _IRubyObjectState_set_IsTainted;
        public static MethodInfo/*!*/ IsClrNonSingletonRuleValid { get { return _IsClrNonSingletonRuleValid ?? (_IsClrNonSingletonRuleValid = GetMethod(typeof(RubyOps), "IsClrNonSingletonRuleValid")); } }
        private static MethodInfo _IsClrNonSingletonRuleValid;
        public static MethodInfo/*!*/ IsClrSingletonRuleValid { get { return _IsClrSingletonRuleValid ?? (_IsClrSingletonRuleValid = GetMethod(typeof(RubyOps), "IsClrSingletonRuleValid")); } }
        private static MethodInfo _IsClrSingletonRuleValid;
        public static MethodInfo/*!*/ IsDefinedClassVariable { get { return _IsDefinedClassVariable ?? (_IsDefinedClassVariable = GetMethod(typeof(RubyOps), "IsDefinedClassVariable")); } }
        private static MethodInfo _IsDefinedClassVariable;
        public static MethodInfo/*!*/ IsDefinedExpressionQualifiedConstant { get { return _IsDefinedExpressionQualifiedConstant ?? (_IsDefinedExpressionQualifiedConstant = GetMethod(typeof(RubyOps), "IsDefinedExpressionQualifiedConstant")); } }
        private static MethodInfo _IsDefinedExpressionQualifiedConstant;
        public static MethodInfo/*!*/ IsDefinedGlobalConstant { get { return _IsDefinedGlobalConstant ?? (_IsDefinedGlobalConstant = GetMethod(typeof(RubyOps), "IsDefinedGlobalConstant")); } }
        private static MethodInfo _IsDefinedGlobalConstant;
        public static MethodInfo/*!*/ IsDefinedGlobalVariable { get { return _IsDefinedGlobalVariable ?? (_IsDefinedGlobalVariable = GetMethod(typeof(RubyOps), "IsDefinedGlobalVariable")); } }
        private static MethodInfo _IsDefinedGlobalVariable;
        public static MethodInfo/*!*/ IsDefinedInstanceVariable { get { return _IsDefinedInstanceVariable ?? (_IsDefinedInstanceVariable = GetMethod(typeof(RubyOps), "IsDefinedInstanceVariable")); } }
        private static MethodInfo _IsDefinedInstanceVariable;
        public static MethodInfo/*!*/ IsDefinedQualifiedConstant { get { return _IsDefinedQualifiedConstant ?? (_IsDefinedQualifiedConstant = GetMethod(typeof(RubyOps), "IsDefinedQualifiedConstant")); } }
        private static MethodInfo _IsDefinedQualifiedConstant;
        public static MethodInfo/*!*/ IsDefinedUnqualifiedConstant { get { return _IsDefinedUnqualifiedConstant ?? (_IsDefinedUnqualifiedConstant = GetMethod(typeof(RubyOps), "IsDefinedUnqualifiedConstant")); } }
        private static MethodInfo _IsDefinedUnqualifiedConstant;
        public static MethodInfo/*!*/ IsFalse { get { return _IsFalse ?? (_IsFalse = GetMethod(typeof(RubyOps), "IsFalse")); } }
        private static MethodInfo _IsFalse;
        public static MethodInfo/*!*/ IsMethodUnwinderTargetFrame { get { return _IsMethodUnwinderTargetFrame ?? (_IsMethodUnwinderTargetFrame = GetMethod(typeof(RubyOps), "IsMethodUnwinderTargetFrame")); } }
        private static MethodInfo _IsMethodUnwinderTargetFrame;
        public static MethodInfo/*!*/ IsObjectFrozen { get { return _IsObjectFrozen ?? (_IsObjectFrozen = GetMethod(typeof(RubyOps), "IsObjectFrozen")); } }
        private static MethodInfo _IsObjectFrozen;
        public static MethodInfo/*!*/ IsObjectTainted { get { return _IsObjectTainted ?? (_IsObjectTainted = GetMethod(typeof(RubyOps), "IsObjectTainted")); } }
        private static MethodInfo _IsObjectTainted;
        public static MethodInfo/*!*/ IsProcConverterTarget { get { return _IsProcConverterTarget ?? (_IsProcConverterTarget = GetMethod(typeof(RubyOps), "IsProcConverterTarget")); } }
        private static MethodInfo _IsProcConverterTarget;
        public static MethodInfo/*!*/ IsRetrySingleton { get { return _IsRetrySingleton ?? (_IsRetrySingleton = GetMethod(typeof(RubyOps), "IsRetrySingleton")); } }
        private static MethodInfo _IsRetrySingleton;
        public static MethodInfo/*!*/ IsSuperCallTarget { get { return _IsSuperCallTarget ?? (_IsSuperCallTarget = GetMethod(typeof(RubyOps), "IsSuperCallTarget")); } }
        private static MethodInfo _IsSuperCallTarget;
        public static MethodInfo/*!*/ IsTrue { get { return _IsTrue ?? (_IsTrue = GetMethod(typeof(RubyOps), "IsTrue")); } }
        private static MethodInfo _IsTrue;
        public static MethodInfo/*!*/ LeaveLoop { get { return _LeaveLoop ?? (_LeaveLoop = GetMethod(typeof(RubyOps), "LeaveLoop")); } }
        private static MethodInfo _LeaveLoop;
        public static MethodInfo/*!*/ LeaveMethodFrame { get { return _LeaveMethodFrame ?? (_LeaveMethodFrame = GetMethod(typeof(RubyOps), "LeaveMethodFrame")); } }
        private static MethodInfo _LeaveMethodFrame;
        public static MethodInfo/*!*/ LeaveProcConverter { get { return _LeaveProcConverter ?? (_LeaveProcConverter = GetMethod(typeof(RubyOps), "LeaveProcConverter")); } }
        private static MethodInfo _LeaveProcConverter;
        public static MethodInfo/*!*/ LeaveRescue { get { return _LeaveRescue ?? (_LeaveRescue = GetMethod(typeof(RubyOps), "LeaveRescue")); } }
        private static MethodInfo _LeaveRescue;
        public static MethodInfo/*!*/ MakeAbstractMethodCalledError { get { return _MakeAbstractMethodCalledError ?? (_MakeAbstractMethodCalledError = GetMethod(typeof(RubyOps), "MakeAbstractMethodCalledError")); } }
        private static MethodInfo _MakeAbstractMethodCalledError;
        public static MethodInfo/*!*/ MakeAllocatorUndefinedError { get { return _MakeAllocatorUndefinedError ?? (_MakeAllocatorUndefinedError = GetMethod(typeof(RubyOps), "MakeAllocatorUndefinedError")); } }
        private static MethodInfo _MakeAllocatorUndefinedError;
        public static MethodInfo/*!*/ MakeAmbiguousMatchError { get { return _MakeAmbiguousMatchError ?? (_MakeAmbiguousMatchError = GetMethod(typeof(RubyOps), "MakeAmbiguousMatchError")); } }
        private static MethodInfo _MakeAmbiguousMatchError;
        public static MethodInfo/*!*/ MakeArray0 { get { return _MakeArray0 ?? (_MakeArray0 = GetMethod(typeof(RubyOps), "MakeArray0")); } }
        private static MethodInfo _MakeArray0;
        public static MethodInfo/*!*/ MakeArray1 { get { return _MakeArray1 ?? (_MakeArray1 = GetMethod(typeof(RubyOps), "MakeArray1")); } }
        private static MethodInfo _MakeArray1;
        public static MethodInfo/*!*/ MakeArray2 { get { return _MakeArray2 ?? (_MakeArray2 = GetMethod(typeof(RubyOps), "MakeArray2")); } }
        private static MethodInfo _MakeArray2;
        public static MethodInfo/*!*/ MakeArray3 { get { return _MakeArray3 ?? (_MakeArray3 = GetMethod(typeof(RubyOps), "MakeArray3")); } }
        private static MethodInfo _MakeArray3;
        public static MethodInfo/*!*/ MakeArray4 { get { return _MakeArray4 ?? (_MakeArray4 = GetMethod(typeof(RubyOps), "MakeArray4")); } }
        private static MethodInfo _MakeArray4;
        public static MethodInfo/*!*/ MakeArray5 { get { return _MakeArray5 ?? (_MakeArray5 = GetMethod(typeof(RubyOps), "MakeArray5")); } }
        private static MethodInfo _MakeArray5;
        public static MethodInfo/*!*/ MakeArrayN { get { return _MakeArrayN ?? (_MakeArrayN = GetMethod(typeof(RubyOps), "MakeArrayN")); } }
        private static MethodInfo _MakeArrayN;
        public static MethodInfo/*!*/ MakeClrProtectedMethodCalledError { get { return _MakeClrProtectedMethodCalledError ?? (_MakeClrProtectedMethodCalledError = GetMethod(typeof(RubyOps), "MakeClrProtectedMethodCalledError")); } }
        private static MethodInfo _MakeClrProtectedMethodCalledError;
        public static MethodInfo/*!*/ MakeConstructorUndefinedError { get { return _MakeConstructorUndefinedError ?? (_MakeConstructorUndefinedError = GetMethod(typeof(RubyOps), "MakeConstructorUndefinedError")); } }
        private static MethodInfo _MakeConstructorUndefinedError;
        public static MethodInfo/*!*/ MakeHash { get { return _MakeHash ?? (_MakeHash = GetMethod(typeof(RubyOps), "MakeHash")); } }
        private static MethodInfo _MakeHash;
        public static MethodInfo/*!*/ MakeHash0 { get { return _MakeHash0 ?? (_MakeHash0 = GetMethod(typeof(RubyOps), "MakeHash0")); } }
        private static MethodInfo _MakeHash0;
        public static MethodInfo/*!*/ MakeInvalidArgumentTypesError { get { return _MakeInvalidArgumentTypesError ?? (_MakeInvalidArgumentTypesError = GetMethod(typeof(RubyOps), "MakeInvalidArgumentTypesError")); } }
        private static MethodInfo _MakeInvalidArgumentTypesError;
        public static MethodInfo/*!*/ MakeMissingDefaultConstructorError { get { return _MakeMissingDefaultConstructorError ?? (_MakeMissingDefaultConstructorError = GetMethod(typeof(RubyOps), "MakeMissingDefaultConstructorError")); } }
        private static MethodInfo _MakeMissingDefaultConstructorError;
        public static MethodInfo/*!*/ MakeMissingSuperException { get { return _MakeMissingSuperException ?? (_MakeMissingSuperException = GetMethod(typeof(RubyOps), "MakeMissingSuperException")); } }
        private static MethodInfo _MakeMissingSuperException;
        public static MethodInfo/*!*/ MakeNotClrTypeError { get { return _MakeNotClrTypeError ?? (_MakeNotClrTypeError = GetMethod(typeof(RubyOps), "MakeNotClrTypeError")); } }
        private static MethodInfo _MakeNotClrTypeError;
        public static MethodInfo/*!*/ MakePrivateMethodCalledError { get { return _MakePrivateMethodCalledError ?? (_MakePrivateMethodCalledError = GetMethod(typeof(RubyOps), "MakePrivateMethodCalledError")); } }
        private static MethodInfo _MakePrivateMethodCalledError;
        public static MethodInfo/*!*/ MakeProtectedMethodCalledError { get { return _MakeProtectedMethodCalledError ?? (_MakeProtectedMethodCalledError = GetMethod(typeof(RubyOps), "MakeProtectedMethodCalledError")); } }
        private static MethodInfo _MakeProtectedMethodCalledError;
        public static MethodInfo/*!*/ MakeTopLevelSuperException { get { return _MakeTopLevelSuperException ?? (_MakeTopLevelSuperException = GetMethod(typeof(RubyOps), "MakeTopLevelSuperException")); } }
        private static MethodInfo _MakeTopLevelSuperException;
        public static MethodInfo/*!*/ MakeTypeConversionError { get { return _MakeTypeConversionError ?? (_MakeTypeConversionError = GetMethod(typeof(RubyOps), "MakeTypeConversionError")); } }
        private static MethodInfo _MakeTypeConversionError;
        public static MethodInfo/*!*/ MakeWrongNumberOfArgumentsError { get { return _MakeWrongNumberOfArgumentsError ?? (_MakeWrongNumberOfArgumentsError = GetMethod(typeof(RubyOps), "MakeWrongNumberOfArgumentsError")); } }
        private static MethodInfo _MakeWrongNumberOfArgumentsError;
        public static MethodInfo/*!*/ MarkException { get { return _MarkException ?? (_MarkException = GetMethod(typeof(RubyOps), "MarkException")); } }
        private static MethodInfo _MarkException;
        public static MethodInfo/*!*/ MatchLastInputLine { get { return _MatchLastInputLine ?? (_MatchLastInputLine = GetMethod(typeof(RubyOps), "MatchLastInputLine")); } }
        private static MethodInfo _MatchLastInputLine;
        public static MethodInfo/*!*/ MatchString { get { return _MatchString ?? (_MatchString = GetMethod(typeof(RubyOps), "MatchString")); } }
        private static MethodInfo _MatchString;
        public static MethodInfo/*!*/ MethodBreak { get { return _MethodBreak ?? (_MethodBreak = GetMethod(typeof(RubyOps), "MethodBreak")); } }
        private static MethodInfo _MethodBreak;
        public static MethodInfo/*!*/ MethodNext { get { return _MethodNext ?? (_MethodNext = GetMethod(typeof(RubyOps), "MethodNext")); } }
        private static MethodInfo _MethodNext;
        public static MethodInfo/*!*/ MethodProcCall { get { return _MethodProcCall ?? (_MethodProcCall = GetMethod(typeof(RubyOps), "MethodProcCall")); } }
        private static MethodInfo _MethodProcCall;
        public static MethodInfo/*!*/ MethodRedo { get { return _MethodRedo ?? (_MethodRedo = GetMethod(typeof(RubyOps), "MethodRedo")); } }
        private static MethodInfo _MethodRedo;
        public static MethodInfo/*!*/ MethodRetry { get { return _MethodRetry ?? (_MethodRetry = GetMethod(typeof(RubyOps), "MethodRetry")); } }
        private static MethodInfo _MethodRetry;
        public static MethodInfo/*!*/ MethodYield { get { return _MethodYield ?? (_MethodYield = GetMethod(typeof(RubyOps), "MethodYield")); } }
        private static MethodInfo _MethodYield;
        public static MethodInfo/*!*/ NullIfFalse { get { return _NullIfFalse ?? (_NullIfFalse = GetMethod(typeof(RubyOps), "NullIfFalse")); } }
        private static MethodInfo _NullIfFalse;
        public static MethodInfo/*!*/ NullIfTrue { get { return _NullIfTrue ?? (_NullIfTrue = GetMethod(typeof(RubyOps), "NullIfTrue")); } }
        private static MethodInfo _NullIfTrue;
        public static MethodInfo/*!*/ ObjectToMutableString { get { return _ObjectToMutableString ?? (_ObjectToMutableString = GetMethod(typeof(RubyOps), "ObjectToMutableString")); } }
        private static MethodInfo _ObjectToMutableString;
        public static MethodInfo/*!*/ ObjectToString { get { return _ObjectToString ?? (_ObjectToString = GetMethod(typeof(RubyOps), "ObjectToString")); } }
        private static MethodInfo _ObjectToString;
        public static MethodInfo/*!*/ PrintInteractiveResult { get { return _PrintInteractiveResult ?? (_PrintInteractiveResult = GetMethod(typeof(RubyOps), "PrintInteractiveResult")); } }
        private static MethodInfo _PrintInteractiveResult;
        public static MethodInfo/*!*/ PropagateRetrySingleton { get { return _PropagateRetrySingleton ?? (_PropagateRetrySingleton = GetMethod(typeof(RubyOps), "PropagateRetrySingleton")); } }
        private static MethodInfo _PropagateRetrySingleton;
        public static MethodInfo/*!*/ RubyModule_get_Context { get { return _RubyModule_get_Context ?? (_RubyModule_get_Context = GetMethod(typeof(RubyModule), "get_Context")); } }
        private static MethodInfo _RubyModule_get_Context;
        public static MethodInfo/*!*/ RubyStruct_GetValue { get { return _RubyStruct_GetValue ?? (_RubyStruct_GetValue = GetMethod(typeof(RubyStruct), "GetValue")); } }
        private static MethodInfo _RubyStruct_GetValue;
        public static MethodInfo/*!*/ RubyStruct_SetValue { get { return _RubyStruct_SetValue ?? (_RubyStruct_SetValue = GetMethod(typeof(RubyStruct), "SetValue")); } }
        private static MethodInfo _RubyStruct_SetValue;
        public static MethodInfo/*!*/ SerializeObject { get { return _SerializeObject ?? (_SerializeObject = GetMethod(typeof(RubyOps), "SerializeObject")); } }
        private static MethodInfo _SerializeObject;
        public static MethodInfo/*!*/ SetClassVariable { get { return _SetClassVariable ?? (_SetClassVariable = GetMethod(typeof(RubyOps), "SetClassVariable")); } }
        private static MethodInfo _SetClassVariable;
        public static MethodInfo/*!*/ SetCurrentException { get { return _SetCurrentException ?? (_SetCurrentException = GetMethod(typeof(RubyOps), "SetCurrentException")); } }
        private static MethodInfo _SetCurrentException;
        public static MethodInfo/*!*/ SetDataConstant { get { return _SetDataConstant ?? (_SetDataConstant = GetMethod(typeof(RubyOps), "SetDataConstant")); } }
        private static MethodInfo _SetDataConstant;
        public static MethodInfo/*!*/ SetGlobalConstant { get { return _SetGlobalConstant ?? (_SetGlobalConstant = GetMethod(typeof(RubyOps), "SetGlobalConstant")); } }
        private static MethodInfo _SetGlobalConstant;
        public static MethodInfo/*!*/ SetGlobalVariable { get { return _SetGlobalVariable ?? (_SetGlobalVariable = GetMethod(typeof(RubyOps), "SetGlobalVariable")); } }
        private static MethodInfo _SetGlobalVariable;
        public static MethodInfo/*!*/ SetInstanceVariable { get { return _SetInstanceVariable ?? (_SetInstanceVariable = GetMethod(typeof(RubyOps), "SetInstanceVariable")); } }
        private static MethodInfo _SetInstanceVariable;
        public static MethodInfo/*!*/ SetLocalVariable { get { return _SetLocalVariable ?? (_SetLocalVariable = GetMethod(typeof(RubyOps), "SetLocalVariable")); } }
        private static MethodInfo _SetLocalVariable;
        public static MethodInfo/*!*/ SetObjectTaint { get { return _SetObjectTaint ?? (_SetObjectTaint = GetMethod(typeof(RubyOps), "SetObjectTaint")); } }
        private static MethodInfo _SetObjectTaint;
        public static MethodInfo/*!*/ SetQualifiedConstant { get { return _SetQualifiedConstant ?? (_SetQualifiedConstant = GetMethod(typeof(RubyOps), "SetQualifiedConstant")); } }
        private static MethodInfo _SetQualifiedConstant;
        public static MethodInfo/*!*/ SetUnqualifiedConstant { get { return _SetUnqualifiedConstant ?? (_SetUnqualifiedConstant = GetMethod(typeof(RubyOps), "SetUnqualifiedConstant")); } }
        private static MethodInfo _SetUnqualifiedConstant;
        public static MethodInfo/*!*/ Splat { get { return _Splat ?? (_Splat = GetMethod(typeof(RubyOps), "Splat")); } }
        private static MethodInfo _Splat;
        public static MethodInfo/*!*/ SplatAppend { get { return _SplatAppend ?? (_SplatAppend = GetMethod(typeof(RubyOps), "SplatAppend")); } }
        private static MethodInfo _SplatAppend;
        public static MethodInfo/*!*/ SplatPair { get { return _SplatPair ?? (_SplatPair = GetMethod(typeof(RubyOps), "SplatPair")); } }
        private static MethodInfo _SplatPair;
        public static MethodInfo/*!*/ StringToMutableString { get { return _StringToMutableString ?? (_StringToMutableString = GetMethod(typeof(RubyOps), "StringToMutableString")); } }
        private static MethodInfo _StringToMutableString;
        public static MethodInfo/*!*/ ToArrayValidator { get { return _ToArrayValidator ?? (_ToArrayValidator = GetMethod(typeof(RubyOps), "ToArrayValidator")); } }
        private static MethodInfo _ToArrayValidator;
        public static MethodInfo/*!*/ ToBignumValidator { get { return _ToBignumValidator ?? (_ToBignumValidator = GetMethod(typeof(RubyOps), "ToBignumValidator")); } }
        private static MethodInfo _ToBignumValidator;
        public static MethodInfo/*!*/ ToByteValidator { get { return _ToByteValidator ?? (_ToByteValidator = GetMethod(typeof(RubyOps), "ToByteValidator")); } }
        private static MethodInfo _ToByteValidator;
        public static MethodInfo/*!*/ ToDoubleValidator { get { return _ToDoubleValidator ?? (_ToDoubleValidator = GetMethod(typeof(RubyOps), "ToDoubleValidator")); } }
        private static MethodInfo _ToDoubleValidator;
        public static MethodInfo/*!*/ ToFixnumValidator { get { return _ToFixnumValidator ?? (_ToFixnumValidator = GetMethod(typeof(RubyOps), "ToFixnumValidator")); } }
        private static MethodInfo _ToFixnumValidator;
        public static MethodInfo/*!*/ ToHashValidator { get { return _ToHashValidator ?? (_ToHashValidator = GetMethod(typeof(RubyOps), "ToHashValidator")); } }
        private static MethodInfo _ToHashValidator;
        public static MethodInfo/*!*/ ToInt16Validator { get { return _ToInt16Validator ?? (_ToInt16Validator = GetMethod(typeof(RubyOps), "ToInt16Validator")); } }
        private static MethodInfo _ToInt16Validator;
        public static MethodInfo/*!*/ ToInt64Validator { get { return _ToInt64Validator ?? (_ToInt64Validator = GetMethod(typeof(RubyOps), "ToInt64Validator")); } }
        private static MethodInfo _ToInt64Validator;
        public static MethodInfo/*!*/ ToIntegerValidator { get { return _ToIntegerValidator ?? (_ToIntegerValidator = GetMethod(typeof(RubyOps), "ToIntegerValidator")); } }
        private static MethodInfo _ToIntegerValidator;
        public static MethodInfo/*!*/ ToProcValidator { get { return _ToProcValidator ?? (_ToProcValidator = GetMethod(typeof(RubyOps), "ToProcValidator")); } }
        private static MethodInfo _ToProcValidator;
        public static MethodInfo/*!*/ ToRegexValidator { get { return _ToRegexValidator ?? (_ToRegexValidator = GetMethod(typeof(RubyOps), "ToRegexValidator")); } }
        private static MethodInfo _ToRegexValidator;
        public static MethodInfo/*!*/ ToSByteValidator { get { return _ToSByteValidator ?? (_ToSByteValidator = GetMethod(typeof(RubyOps), "ToSByteValidator")); } }
        private static MethodInfo _ToSByteValidator;
        public static MethodInfo/*!*/ ToSDefaultConversion { get { return _ToSDefaultConversion ?? (_ToSDefaultConversion = GetMethod(typeof(RubyOps), "ToSDefaultConversion")); } }
        private static MethodInfo _ToSDefaultConversion;
        public static MethodInfo/*!*/ ToSingleValidator { get { return _ToSingleValidator ?? (_ToSingleValidator = GetMethod(typeof(RubyOps), "ToSingleValidator")); } }
        private static MethodInfo _ToSingleValidator;
        public static MethodInfo/*!*/ ToStringValidator { get { return _ToStringValidator ?? (_ToStringValidator = GetMethod(typeof(RubyOps), "ToStringValidator")); } }
        private static MethodInfo _ToStringValidator;
        public static MethodInfo/*!*/ ToSymbolValidator { get { return _ToSymbolValidator ?? (_ToSymbolValidator = GetMethod(typeof(RubyOps), "ToSymbolValidator")); } }
        private static MethodInfo _ToSymbolValidator;
        public static MethodInfo/*!*/ ToUInt16Validator { get { return _ToUInt16Validator ?? (_ToUInt16Validator = GetMethod(typeof(RubyOps), "ToUInt16Validator")); } }
        private static MethodInfo _ToUInt16Validator;
        public static MethodInfo/*!*/ ToUInt32Validator { get { return _ToUInt32Validator ?? (_ToUInt32Validator = GetMethod(typeof(RubyOps), "ToUInt32Validator")); } }
        private static MethodInfo _ToUInt32Validator;
        public static MethodInfo/*!*/ ToUInt64Validator { get { return _ToUInt64Validator ?? (_ToUInt64Validator = GetMethod(typeof(RubyOps), "ToUInt64Validator")); } }
        private static MethodInfo _ToUInt64Validator;
        public static MethodInfo/*!*/ TraceBlockCall { get { return _TraceBlockCall ?? (_TraceBlockCall = GetMethod(typeof(RubyOps), "TraceBlockCall")); } }
        private static MethodInfo _TraceBlockCall;
        public static MethodInfo/*!*/ TraceBlockReturn { get { return _TraceBlockReturn ?? (_TraceBlockReturn = GetMethod(typeof(RubyOps), "TraceBlockReturn")); } }
        private static MethodInfo _TraceBlockReturn;
        public static MethodInfo/*!*/ TraceMethodCall { get { return _TraceMethodCall ?? (_TraceMethodCall = GetMethod(typeof(RubyOps), "TraceMethodCall")); } }
        private static MethodInfo _TraceMethodCall;
        public static MethodInfo/*!*/ TraceMethodReturn { get { return _TraceMethodReturn ?? (_TraceMethodReturn = GetMethod(typeof(RubyOps), "TraceMethodReturn")); } }
        private static MethodInfo _TraceMethodReturn;
        public static MethodInfo/*!*/ TraceTopLevelCodeFrame { get { return _TraceTopLevelCodeFrame ?? (_TraceTopLevelCodeFrame = GetMethod(typeof(RubyOps), "TraceTopLevelCodeFrame")); } }
        private static MethodInfo _TraceTopLevelCodeFrame;
        public static MethodInfo/*!*/ TryGetClassVariable { get { return _TryGetClassVariable ?? (_TryGetClassVariable = GetMethod(typeof(RubyOps), "TryGetClassVariable")); } }
        private static MethodInfo _TryGetClassVariable;
        public static MethodInfo/*!*/ UndefineMethod { get { return _UndefineMethod ?? (_UndefineMethod = GetMethod(typeof(RubyOps), "UndefineMethod")); } }
        private static MethodInfo _UndefineMethod;
        public static MethodInfo/*!*/ Unsplat { get { return _Unsplat ?? (_Unsplat = GetMethod(typeof(RubyOps), "Unsplat")); } }
        private static MethodInfo _Unsplat;
        public static MethodInfo/*!*/ UpdateProfileTicks { get { return _UpdateProfileTicks ?? (_UpdateProfileTicks = GetMethod(typeof(RubyOps), "UpdateProfileTicks")); } }
        private static MethodInfo _UpdateProfileTicks;
        public static MethodInfo/*!*/ X { get { return _X ?? (_X = GetMethod(typeof(RubyOps), "X")); } }
        private static MethodInfo _X;
        public static MethodInfo/*!*/ Yield0 { get { return _Yield0 ?? (_Yield0 = GetMethod(typeof(RubyOps), "Yield0")); } }
        private static MethodInfo _Yield0;
        public static MethodInfo/*!*/ Yield1 { get { return _Yield1 ?? (_Yield1 = GetMethod(typeof(RubyOps), "Yield1")); } }
        private static MethodInfo _Yield1;
        public static MethodInfo/*!*/ Yield2 { get { return _Yield2 ?? (_Yield2 = GetMethod(typeof(RubyOps), "Yield2")); } }
        private static MethodInfo _Yield2;
        public static MethodInfo/*!*/ Yield3 { get { return _Yield3 ?? (_Yield3 = GetMethod(typeof(RubyOps), "Yield3")); } }
        private static MethodInfo _Yield3;
        public static MethodInfo/*!*/ Yield4 { get { return _Yield4 ?? (_Yield4 = GetMethod(typeof(RubyOps), "Yield4")); } }
        private static MethodInfo _Yield4;
        public static MethodInfo/*!*/ YieldN { get { return _YieldN ?? (_YieldN = GetMethod(typeof(RubyOps), "YieldN")); } }
        private static MethodInfo _YieldN;
        public static MethodInfo/*!*/ YieldSplat0 { get { return _YieldSplat0 ?? (_YieldSplat0 = GetMethod(typeof(RubyOps), "YieldSplat0")); } }
        private static MethodInfo _YieldSplat0;
        public static MethodInfo/*!*/ YieldSplat1 { get { return _YieldSplat1 ?? (_YieldSplat1 = GetMethod(typeof(RubyOps), "YieldSplat1")); } }
        private static MethodInfo _YieldSplat1;
        public static MethodInfo/*!*/ YieldSplat2 { get { return _YieldSplat2 ?? (_YieldSplat2 = GetMethod(typeof(RubyOps), "YieldSplat2")); } }
        private static MethodInfo _YieldSplat2;
        public static MethodInfo/*!*/ YieldSplat3 { get { return _YieldSplat3 ?? (_YieldSplat3 = GetMethod(typeof(RubyOps), "YieldSplat3")); } }
        private static MethodInfo _YieldSplat3;
        public static MethodInfo/*!*/ YieldSplat4 { get { return _YieldSplat4 ?? (_YieldSplat4 = GetMethod(typeof(RubyOps), "YieldSplat4")); } }
        private static MethodInfo _YieldSplat4;
        public static MethodInfo/*!*/ YieldSplatN { get { return _YieldSplatN ?? (_YieldSplatN = GetMethod(typeof(RubyOps), "YieldSplatN")); } }
        private static MethodInfo _YieldSplatN;
        public static MethodInfo/*!*/ YieldSplatNRhs { get { return _YieldSplatNRhs ?? (_YieldSplatNRhs = GetMethod(typeof(RubyOps), "YieldSplatNRhs")); } }
        private static MethodInfo _YieldSplatNRhs;
        
        public static MethodInfo/*!*/ CreateRegex(string/*!*/ suffix) {
            Debug.Assert(suffix.Length <= RubyOps.MakeStringParamCount);
            switch (suffix) {
                case "N": return CreateRegexN;
                case "L": return CreateRegexL;
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
                case "L": return CreateMutableStringL;
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
                case "L": return CreateSymbolL;
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
        public static FieldInfo/*!*/ RubyContext_ConstantAccessVersion { get { return _RubyContext_ConstantAccessVersion ?? (_RubyContext_ConstantAccessVersion = GetField(typeof(RubyContext), "ConstantAccessVersion")); } }
        private static FieldInfo _RubyContext_ConstantAccessVersion;
        public static FieldInfo/*!*/ RubyModule_Version { get { return _RubyModule_Version ?? (_RubyModule_Version = GetField(typeof(RubyModule), "Version")); } }
        private static FieldInfo _RubyModule_Version;
        public static FieldInfo/*!*/ VersionHandle_Method { get { return _VersionHandle_Method ?? (_VersionHandle_Method = GetField(typeof(VersionHandle), "Method")); } }
        private static FieldInfo _VersionHandle_Method;
        
    }
}
