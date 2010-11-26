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
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime {
    [ReflectionCached, CLSCompliant(false)]
    public static partial class RubyOps {
        [Emitted]
        public static readonly object DefaultArgument = new object();
        
        // Returned by a virtual site if a base call should be performed.
        [Emitted]
        public static readonly object ForwardToBase = new object();

        // an instance of a dummy type that causes any rule based on instance type check to fail
        private sealed class _NeedsUpdate {
        }

        [Emitted]
        public static readonly object NeedsUpdate = new _NeedsUpdate();

        #region Scopes

        [Emitted]
        public static MutableTuple GetLocals(RubyScope/*!*/ scope) {
            return scope.Locals;
        }

        [Emitted]
        public static MutableTuple GetParentLocals(RubyScope/*!*/ scope) {
            return scope.Parent.Locals;
        }

        [Emitted]
        public static RubyScope/*!*/ GetParentScope(RubyScope/*!*/ scope) {
            return scope.Parent;
        }

        [Emitted]
        public static Proc GetMethodBlockParameter(RubyScope/*!*/ scope) {
            var methodScope = scope.GetInnerMostMethodScope();
            return methodScope != null ? methodScope.BlockParameter : null;
        }

        [Emitted]
        public static object GetMethodBlockParameterSelf(RubyScope/*!*/ scope) {
            Proc proc = scope.GetInnerMostMethodScope().BlockParameter;
            Debug.Assert(proc != null, "CreateBfcForYield is called before this method and it checks non-nullity");
            return proc.Self;
        }

        [Emitted]
        public static object GetProcSelf(Proc/*!*/ proc) {
            return proc.Self;
        }

        [Emitted]
        public static int GetProcArity(Proc/*!*/ proc) {
            return proc.Dispatcher.Arity;
        }

        [Emitted]
        public static void InitializeScope(RubyScope/*!*/ scope, MutableTuple locals, string[] variableNames, 
            InterpretedFrame interpretedFrame) {

            if (!scope.LocalsInitialized) {
                scope.SetLocals(locals, variableNames ?? ArrayUtils.EmptyStrings);
            }
            scope.InterpretedFrame = interpretedFrame;
        }
        
        [Emitted]
        public static void InitializeScopeNoLocals(RubyScope/*!*/ scope, InterpretedFrame interpretedFrame) {
            scope.InterpretedFrame = interpretedFrame;
        }

        [Emitted]
        public static void SetDataConstant(RubyScope/*!*/ scope, string/*!*/ dataPath, int dataOffset) {
            Debug.Assert(dataOffset >= 0);
            RubyFile dataFile;
            RubyContext context = scope.RubyContext;
            if (context.DomainManager.Platform.FileExists(dataPath)) {
                dataFile = new RubyFile(context, dataPath, IOMode.ReadOnly);
                dataFile.Seek(dataOffset, SeekOrigin.Begin);
            } else {
                dataFile = null;
            }

            context.ObjectClass.SetConstant("DATA", dataFile);
        }

        [Emitted]
        public static RubyModuleScope/*!*/ CreateModuleScope(MutableTuple locals, string[] variableNames, 
            RubyScope/*!*/ parent, RubyModule/*!*/ module) {

            if (parent.RubyContext != module.Context) {
                throw RubyExceptions.CreateTypeError("Cannot open a module `{0}' defined in a foreign runtime #{1}", module.Name, module.Context.RuntimeId);
            }

            RubyModuleScope scope = new RubyModuleScope(parent, module);
            scope.SetDebugName((module.IsClass ? "class" : "module") + " " + module.Name);
            scope.SetLocals(locals, variableNames ?? ArrayUtils.EmptyStrings);
            return scope;
        }

        [Emitted]
        public static RubyMethodScope/*!*/ CreateMethodScope(MutableTuple locals, string[] variableNames, int visibleParameterCount,
            RubyScope/*!*/ parentScope, RubyModule/*!*/ declaringModule, string/*!*/ definitionName, 
            object selfObject, Proc blockParameter, InterpretedFrame interpretedFrame) {

            return new RubyMethodScope(
                locals, variableNames ?? ArrayUtils.EmptyStrings, visibleParameterCount,
                parentScope, declaringModule, definitionName, selfObject, blockParameter,
                interpretedFrame
            );            
        }

        [Emitted]
        public static RubyScope/*!*/ CreateFileInitializerScope(MutableTuple locals, string[] variableNames, RubyScope/*!*/ parent) {
            return new RubyFileInitializerScope(locals, variableNames ?? ArrayUtils.EmptyStrings, parent);
        }

        [Emitted]
        public static RubyBlockScope/*!*/ CreateBlockScope(MutableTuple locals, string[] variableNames, 
            BlockParam/*!*/ blockParam, object selfObject, InterpretedFrame interpretedFrame) {

            return new RubyBlockScope(locals, variableNames ?? ArrayUtils.EmptyStrings, blockParam, selfObject, interpretedFrame);
        }

        [Emitted]
        public static void TraceMethodCall(RubyMethodScope/*!*/ scope, string fileName, int lineNumber) {
            // MRI: 
            // Reports DeclaringModule even though an aliased method in a sub-module is called.
            // Also works for singleton module-function, which shares DeclaringModule with instance module-function.
            RubyModule module = scope.DeclaringModule;
            scope.RubyContext.ReportTraceEvent("call", scope, module, scope.DefinitionName, fileName, lineNumber);
        }

        [Emitted]
        public static void TraceMethodReturn(RubyMethodScope/*!*/ scope, string fileName, int lineNumber) {
            RubyModule module = scope.DeclaringModule;
            scope.RubyContext.ReportTraceEvent("return", scope, module, scope.DefinitionName, fileName, lineNumber);
        }

        [Emitted]
        public static void TraceBlockCall(RubyBlockScope/*!*/ scope, BlockParam/*!*/ block, string fileName, int lineNumber) {
            var method = block.Proc.Method;
            if (method != null) {
                scope.RubyContext.ReportTraceEvent("call", scope, method.DeclaringModule, method.DefinitionName, fileName, lineNumber);
            }
        }

        [Emitted]
        public static void TraceBlockReturn(RubyBlockScope/*!*/ scope, BlockParam/*!*/ block, string fileName, int lineNumber) {
            var method = block.Proc.Method;
            if (method != null) {
                scope.RubyContext.ReportTraceEvent("return", scope, method.DeclaringModule, method.DefinitionName, fileName, lineNumber);
            }
        }

        [Emitted]
        public static void PrintInteractiveResult(RubyScope/*!*/ scope, MutableString/*!*/ value) {
            var writer = scope.RubyContext.DomainManager.SharedIO.OutputStream;
            writer.WriteByte((byte)'=');
            writer.WriteByte((byte)'>');
            writer.WriteByte((byte)' ');
            var bytes = value.ToByteArray();
            writer.Write(bytes, 0, bytes.Length);
            writer.WriteByte((byte)'\r');
            writer.WriteByte((byte)'\n');
        }

        [Emitted]
        public static object GetLocalVariable(RubyScope/*!*/ scope, string/*!*/ name) {
            return scope.ResolveLocalVariable(name);
        }

        [Emitted]
        public static object SetLocalVariable(object value, RubyScope/*!*/ scope, string/*!*/ name) {
            return scope.ResolveAndSetLocalVariable(name, value);
        }

        [Emitted]
        public static VersionHandle/*!*/ GetSelfClassVersionHandle(RubyScope/*!*/ scope) {
            return scope.SelfImmediateClass.Version;
        }

        #endregion

        #region Context

        [Emitted]
        public static RubyContext/*!*/ GetContextFromModule(RubyModule/*!*/ module) {
            return module.Context;
        }
        
        [Emitted]
        public static RubyContext/*!*/ GetContextFromIRubyObject(IRubyObject/*!*/ obj) {
            return obj.ImmediateClass.Context;
        }
        
        [Emitted]
        public static RubyContext/*!*/ GetContextFromScope(RubyScope/*!*/ scope) {
            return scope.RubyContext;
        }

        [Emitted]
        public static RubyContext/*!*/ GetContextFromMethod(RubyMethod/*!*/ method) {
            return method.Info.Context;
        }

        [Emitted]
        public static RubyContext/*!*/ GetContextFromBlockParam(BlockParam/*!*/ block) {
            return block.RubyContext;
        }

        [Emitted]
        public static RubyContext/*!*/ GetContextFromProc(Proc/*!*/ proc) {
            return proc.LocalScope.RubyContext;
        }

        [Emitted]
        public static RubyScope/*!*/ GetEmptyScope(RubyContext/*!*/ context) {
            return context.EmptyScope;
        }

        [Emitted]
        public static Scope/*!*/ GetGlobalScopeFromScope(RubyScope/*!*/ scope) {
            return scope.GlobalScope.Scope;
        }

        #endregion

        #region Blocks

        [Emitted]
        public static Proc InstantiateBlock(RubyScope/*!*/ scope, object self, BlockDispatcher/*!*/ dispatcher) {
            return (dispatcher.Method != null) ? new Proc(ProcKind.Block, self, scope, dispatcher) : null;
        }
        [Emitted]
        public static Proc InstantiateLambda(RubyScope/*!*/ scope, object self, BlockDispatcher/*!*/ dispatcher) {
            return (dispatcher.Method != null) ? new Proc(ProcKind.Lambda, self, scope, dispatcher) : null;
        }

        [Emitted]
        public static Proc/*!*/ DefineBlock(RubyScope/*!*/ scope, object self, BlockDispatcher/*!*/ dispatcher, object/*!*/ clrMethod) {
            // DLR closures should not be used:
            Debug.Assert(!(((Delegate)clrMethod).Target is Closure) || ((Closure)((Delegate)clrMethod).Target).Locals == null);
            return new Proc(ProcKind.Block, self, scope, dispatcher.SetMethod(clrMethod));
        }

        [Emitted]
        public static Proc/*!*/ DefineLambda(RubyScope/*!*/ scope, object self, BlockDispatcher/*!*/ dispatcher, object/*!*/ clrMethod) {
            // DLR closures should not be used:
            Debug.Assert(!(((Delegate)clrMethod).Target is Closure) || ((Closure)((Delegate)clrMethod).Target).Locals == null);
            return new Proc(ProcKind.Lambda, self, scope, dispatcher.SetMethod(clrMethod));
        }

        /// <summary>
        /// Used in a method call with a block to reset proc-kind when the call is retried
        /// </summary>
        [Emitted]
        public static void InitializeBlock(Proc/*!*/ proc) {
            Assert.NotNull(proc);
            proc.Kind = ProcKind.Block;
        }

        /// <summary>
        /// Implements END block - like if it was a call to at_exit { ... } library method.
        /// </summary>
        [Emitted]
        public static void RegisterShutdownHandler(Proc/*!*/ proc) {
            proc.LocalScope.RubyContext.RegisterShutdownHandler(proc);
        }

        #endregion

        #region Yield: TODO: generate

        [Emitted] 
        public static object Yield0(Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.Invoke(blockParam, self, procArg);
            } catch(EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object Yield1(object arg1, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.Invoke(blockParam, self, procArg, arg1);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        // YieldNoAutoSplat1 uses InvokeNoAutoSplat instead of Invoke (used by Call1)
        internal static object YieldNoAutoSplat1(object arg1, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeNoAutoSplat(blockParam, self, procArg, arg1);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object Yield2(object arg1, object arg2, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.Invoke(blockParam, self, procArg, arg1, arg2);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object Yield3(object arg1, object arg2, object arg3, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.Invoke(blockParam, self, procArg, arg1, arg2, arg3);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object Yield4(object arg1, object arg2, object arg3, object arg4, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.Invoke(blockParam, self, procArg, arg1, arg2, arg3, arg4);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object YieldN(object[]/*!*/ args, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            Debug.Assert(args.Length > BlockDispatcher.MaxBlockArity);

            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.Invoke(blockParam, self, procArg, args);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        internal static object Yield(object[]/*!*/ args, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            switch (args.Length) {
                case 0: return RubyOps.Yield0(procArg, self, blockParam);
                case 1: return RubyOps.Yield1(args[0], procArg, self, blockParam);
                case 2: return RubyOps.Yield2(args[0], args[1], procArg, self, blockParam);
                case 3: return RubyOps.Yield3(args[0], args[1], args[2], procArg, self, blockParam);
                case 4: return RubyOps.Yield4(args[0], args[1], args[2], args[3], procArg, self, blockParam);
                default: return RubyOps.YieldN(args, procArg, self, blockParam); 
            }
        }

        [Emitted]
        public static object YieldSplat0(IList/*!*/ splattee, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeSplat(blockParam, self, procArg, splattee);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object YieldSplat1(object arg1, IList/*!*/ splattee, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, splattee);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object YieldSplat2(object arg1, object arg2, IList/*!*/ splattee, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, arg2, splattee);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object YieldSplat3(object arg1, object arg2, object arg3, IList/*!*/ splattee, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, arg2, arg3, splattee);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object YieldSplat4(object arg1, object arg2, object arg3, object arg4, IList/*!*/ splattee, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeSplat(blockParam, self, procArg, arg1, arg2, arg3, arg4, splattee);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object YieldSplatN(object[]/*!*/ args, IList/*!*/ splattee, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeSplat(blockParam, self, procArg, args, splattee);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        [Emitted]
        public static object YieldSplatNRhs(object[]/*!*/ args, IList/*!*/ splattee, object rhs, Proc procArg, object self, BlockParam/*!*/ blockParam) {
            object result;
            var proc = blockParam.Proc;
            try {
                result = proc.Dispatcher.InvokeSplatRhs(blockParam, self, procArg, args, splattee, rhs);
            } catch (EvalUnwinder evalUnwinder) {
                result = blockParam.GetUnwinderResult(evalUnwinder);
            }

            return result;
        }

        #endregion

        #region Methods

        [Emitted] // MethodDeclaration:
        public static object DefineMethod(object target, RubyScope/*!*/ scope, RubyMethodBody/*!*/ body) {
            Assert.NotNull(body, scope);

            RubyModule instanceOwner, singletonOwner;
            RubyMemberFlags instanceFlags, singletonFlags;
            bool moduleFunction = false;

            if (body.HasTarget) {
                if (!RubyUtils.CanDefineSingletonMethod(target)) {
                    throw RubyExceptions.CreateTypeError("can't define singleton method for literals");
                }

                instanceOwner = null;
                instanceFlags = RubyMemberFlags.Invalid;
                singletonOwner = scope.RubyContext.GetOrCreateSingletonClass(target);
                singletonFlags = RubyMemberFlags.Public;
            } else {
                var attributesScope = scope.GetMethodAttributesDefinitionScope();
                if ((attributesScope.MethodAttributes & RubyMethodAttributes.ModuleFunction) == RubyMethodAttributes.ModuleFunction) {
                    // Singleton module-function's scope points to the instance method's RubyMemberInfo.
                    // This affects:
                    // 1) super call
                    //    Super call is looking for Method.DeclaringModule while searching MRO, which would fail if the singleton module-function
                    //    was in MRO. Since module-function can only be used on module the singleton method could only be on module's singleton.
                    //    Module's singleton is never part of MRO so we are safe.
                    // 2) trace
                    //    Method call trace reports non-singleton module.

                    // MRI 1.8: instance method owner is self -> it is possible (via define_method) to define m.f. on a class (bug)
                    // MRI 1.9: instance method owner GetMethodDefinitionOwner
                    // MRI allows to define m.f. on classes but then doesn't work correctly with it.
                    instanceOwner = scope.GetMethodDefinitionOwner();
                    if (instanceOwner.IsClass) {
                        throw RubyExceptions.CreateTypeError("A module function cannot be defined on a class.");
                    }

                    instanceFlags = RubyMemberFlags.Private;
                    singletonOwner = instanceOwner.GetOrCreateSingletonClass();
                    singletonFlags = RubyMemberFlags.Public;
                    moduleFunction = true;
                } else {
                    instanceOwner = scope.GetMethodDefinitionOwner();
                    instanceFlags = (RubyMemberFlags)RubyUtils.GetSpecialMethodVisibility(attributesScope.Visibility, body.Name);
                    singletonOwner = null;
                    singletonFlags = RubyMemberFlags.Invalid;
                }
            }
            
            RubyMethodInfo instanceMethod = null, singletonMethod = null;

            if (instanceOwner != null) {
                SetMethod(scope.RubyContext, instanceMethod =
                    new RubyMethodInfo(body, scope, instanceOwner, instanceFlags)
                );
            }

            if (singletonOwner != null) {
                SetMethod(scope.RubyContext, singletonMethod =
                    new RubyMethodInfo(body, scope, singletonOwner, singletonFlags)
                );
            }

            // the method's scope saves the result => singleton module-function uses instance-method
            var method = instanceMethod ?? singletonMethod;

            method.DeclaringModule.MethodAdded(body.Name);

            if (moduleFunction) {
                Debug.Assert(!method.DeclaringModule.IsClass);
                method.DeclaringModule.GetOrCreateSingletonClass().MethodAdded(body.Name);
            }

            return null;
        }

        private static void SetMethod(RubyContext/*!*/ callerContext, RubyMethodInfo/*!*/ method) {
            var owner = method.DeclaringModule;

            // Do not trigger the add-method event just yet, we need to assign the result into closure before executing any user code.
            // If the method being defined is "method_added" itself, we would call that method before the info gets assigned to the closure.
            owner.SetMethodNoEvent(callerContext, method.DefinitionName, method);

            // expose RubyMethod in the scope (the method is bound to the main singleton instance):
            if (owner.GlobalScope != null) {
                RubyOps.ScopeSetMember(
                    owner.GlobalScope.Scope,
                    method.DefinitionName,
                    new RubyMethod(owner.GlobalScope.MainObject, method, method.DefinitionName)
                );
            }
        }

        [Emitted] // AliasStatement:
        public static void AliasMethod(RubyScope/*!*/ scope, string/*!*/ newName, string/*!*/ oldName) {
            scope.GetMethodDefinitionOwner().AddMethodAlias(newName, oldName);
        }

        [Emitted] // UndefineMethod:
        public static void UndefineMethod(RubyScope/*!*/ scope, string/*!*/ name) {
            RubyModule owner = scope.GetMethodDefinitionOwner();

            if (!owner.ResolveMethod(name, VisibilityContext.AllVisible).Found) {
                throw RubyExceptions.CreateUndefinedMethodError(owner, name);
            }
            owner.UndefineMethod(name);
        }

        #endregion

        #region Modules

        [Emitted]
        public static RubyModule/*!*/ DefineGlobalModule(RubyScope/*!*/ scope, string/*!*/ name) {
            return DefineModule(scope, scope.Top.TopModuleOrObject, name);
        }

        [Emitted]
        public static RubyModule/*!*/ DefineNestedModule(RubyScope/*!*/ scope, string/*!*/ name) {
            return DefineModule(scope, scope.GetInnerMostModuleForConstantLookup(), name);
        }

        [Emitted]
        public static RubyModule/*!*/ DefineModule(RubyScope/*!*/ scope, object target, string/*!*/ name) {
            return DefineModule(scope, RubyUtils.GetModuleFromObject(scope, target), name);
        }

        // thread-safe:
        private static RubyModule/*!*/ DefineModule(RubyScope/*!*/ scope, RubyModule/*!*/ owner, string/*!*/ name) {
            Assert.NotNull(scope, owner);

            ConstantStorage existing;
            if (owner.TryGetConstant(scope.GlobalScope, name, out existing)) {
                RubyModule module = existing.Value as RubyModule;
                if (module == null || module.IsClass) {
                    throw RubyExceptions.CreateTypeError(String.Format("{0} is not a module", name));
                }
                return module;
            } else {
                // create class/module object:
                return owner.Context.DefineModule(owner, name);
            }
        }

        #endregion

        #region Classes

        [Emitted]
        public static RubyClass/*!*/ DefineSingletonClass(RubyScope/*!*/ scope, object obj) {
            if (!RubyUtils.HasSingletonClass(obj)) {
                throw RubyExceptions.CreateTypeError(String.Format("no virtual class for {0}", scope.RubyContext.GetClassOf(obj).Name));
            }
            return scope.RubyContext.GetOrCreateSingletonClass(obj);
        }

        [Emitted] 
        public static RubyModule/*!*/ DefineGlobalClass(RubyScope/*!*/ scope, string/*!*/ name, object superClassObject) {
            return DefineClass(scope, scope.Top.TopModuleOrObject, name, superClassObject);
        }

        [Emitted]
        public static RubyModule/*!*/ DefineNestedClass(RubyScope/*!*/ scope, string/*!*/ name, object superClassObject) {
            return DefineClass(scope, scope.GetInnerMostModuleForConstantLookup(), name, superClassObject);
        }

        [Emitted]
        public static RubyModule/*!*/ DefineClass(RubyScope/*!*/ scope, object target, string/*!*/ name, object superClassObject) {
            return DefineClass(scope, RubyUtils.GetModuleFromObject(scope, target), name, superClassObject);
        }

        // thread-safe:
        private static RubyClass/*!*/ DefineClass(RubyScope/*!*/ scope, RubyModule/*!*/ owner, string/*!*/ name, object superClassObject) {
            Assert.NotNull(owner);
            RubyClass superClass = ToSuperClass(owner.Context, superClassObject);

            ConstantStorage existing;
            if (owner.IsObjectClass
                ? owner.TryResolveConstant(scope.GlobalScope, name, out existing)
                : owner.TryGetConstant(scope.GlobalScope, name, out existing)) {

                RubyClass cls = existing.Value as RubyClass;
                if (cls == null || !cls.IsClass) {
                    throw RubyExceptions.CreateTypeError("{0} is not a class", name);
                }

                if (superClassObject != null && !ReferenceEquals(cls.SuperClass, superClass)) {
                    throw RubyExceptions.CreateTypeError("superclass mismatch for class {0}", name);
                }
                return cls;
            } else {
                return owner.Context.DefineClass(owner, name, superClass, null);
            }
        }

        private static RubyClass/*!*/ ToSuperClass(RubyContext/*!*/ ec, object superClassObject) {
            if (superClassObject != null) {
                RubyClass superClass = superClassObject as RubyClass;
                if (superClass == null) {
                    throw RubyExceptions.CreateTypeError("superclass must be a Class ({0} given)", ec.GetClassOf(superClassObject).Name);
                }

                if (superClass.IsSingletonClass) {
                    throw RubyExceptions.CreateTypeError("can't make subclass of virtual class");
                }

                return superClass;
            } else {
                return ec.ObjectClass;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// A
        /// ::A
        /// </summary>
        [Emitted]
        public static object GetUnqualifiedConstant(RubyScope/*!*/ scope, ConstantSiteCache/*!*/ cache, string/*!*/ name, bool isGlobal) {
            object result = null;
            RubyModule missingConstantOwner;
            var context = scope.RubyContext;
            using (context.ClassHierarchyLocker()) {
                // Thread safety:
                // Another thread could have already updated the value, so the site version might be the same as CAV.
                // We do the lookup anyways since it is no-op and this only happens rarely.
                //
                // An important invariant holds here: in any time after initialized for the first time the Value field contains a valid value.
                // Threads can read an older value (the previous version) but that is still correct since we don't guarantee immediate
                // propagation of the constant write to all readers.
                // 
                // if (site.Version = CAV) {
                //   <- another thread could increment CAV here - we may return old or new value (both are ok)
                //   value = site.Value;
                // } else {
                //   <- another thread could get here as well and update the site before we get to update it.
                //   GetConstant(...)
                // }

                // Constants might be updated during constant resolution due to autoload. 
                // Any such updates need to invalidate the cache hence we need to capture the version before resolving the constant.
                int newVersion = context.ConstantAccessVersion;

                ConstantStorage storage;
                if (!isGlobal) {
                    missingConstantOwner = scope.TryResolveConstantNoLock(scope.GlobalScope, name, out storage);
                } else if (context.ObjectClass.TryResolveConstantNoLock(scope.GlobalScope, name, out storage)) {
                    missingConstantOwner = null;
                } else {
                    missingConstantOwner = context.ObjectClass;
                }

                object newCacheValue;
                if (missingConstantOwner == null) {
                    if (storage.WeakValue != null) {
                        result = storage.Value;
                        newCacheValue = storage.WeakValue;
                    } else {
                        result = newCacheValue = storage.Value;
                    }
                } else {
                    newCacheValue = ConstantSiteCache.WeakMissingConstant;
                }

                cache.Update(newCacheValue, newVersion);
            }

            if (missingConstantOwner != null) {
                result = missingConstantOwner.ConstantMissing(name);
            }

            return result;
        }

        /// <summary>
        /// A1::..::AN
        /// ::A1::..::AN
        /// </summary>
        [Emitted]
        public static object GetQualifiedConstant(RubyScope/*!*/ scope, ConstantSiteCache/*!*/ cache, string/*!*/[]/*!*/ qualifiedName, bool isGlobal) {
            var globalScope = scope.GlobalScope;
            var context = globalScope.Context;

            using (context.ClassHierarchyLocker()) {
                int newVersion = context.ConstantAccessVersion;
                
                ConstantStorage storage;
                bool anyMissing;
                RubyModule topModule = isGlobal ? context.ObjectClass : null;
                object result = ResolveQualifiedConstant(scope, qualifiedName, topModule, true, out storage, out anyMissing);

                // cache result only if no constant was missing:
                if (!anyMissing) {
                    Debug.Assert(result == storage.Value);
                    cache.Update(storage.WeakValue ?? result, newVersion);
                }

                return result;
            }
        }

        /// <summary>
        /// {expr}::A1::..::AN
        /// </summary>
        [Emitted]
        public static object GetExpressionQualifiedConstant(object target, RubyScope/*!*/ scope, ExpressionQualifiedConstantSiteCache/*!*/ cache,
            string/*!*/[]/*!*/ qualifiedName) {
            RubyModule module = target as RubyModule;
            if (module == null) {
                throw RubyUtils.CreateNotModuleException(scope, target);
            }

            var condition = cache.Condition;
            RubyContext context = module.Context;

            // Note that the module can be bound to another runtime:
            if (module.Id == condition.ModuleId && context.ConstantAccessVersion == condition.Version) {
                object value = cache.Value;
                if (value.GetType() == typeof(WeakReference)) {
                    return ((WeakReference)value).Target;
                } else {
                    return value;
                }
            }

            using (context.ClassHierarchyLocker()) {
                int newVersion = context.ConstantAccessVersion;
                
                ConstantStorage storage;
                bool anyMissing;
                object result = ResolveQualifiedConstant(scope, qualifiedName, module, true, out storage, out anyMissing);

                // cache result only if no constant was missing:
                if (!anyMissing) {
                    Debug.Assert(result == storage.Value);
                    cache.Update(storage.WeakValue ?? result, newVersion, module);
                }

                return result;
            }
        }

        /// <summary>
        /// defined? A
        /// </summary>
        [Emitted]
        public static bool IsDefinedUnqualifiedConstant(RubyScope/*!*/ scope, IsDefinedConstantSiteCache/*!*/ cache, string/*!*/ name) {
            var context = scope.RubyContext;
            using (context.ClassHierarchyLocker()) {
                int newVersion = context.ConstantAccessVersion;
                
                ConstantStorage storage;
                bool exists = scope.TryResolveConstantNoLock(null, name, out storage) == null;
                cache.Update(exists, newVersion);
                return exists;
            }
        }

        /// <summary>
        /// defined? ::A
        /// </summary>
        [Emitted]
        public static bool IsDefinedGlobalConstant(RubyScope/*!*/ scope, IsDefinedConstantSiteCache/*!*/ cache, string/*!*/ name) {
            var context = scope.RubyContext;
            using (context.ClassHierarchyLocker()) {
                int newVersion = context.ConstantAccessVersion;

                ConstantStorage storage;
                bool exists = context.ObjectClass.TryResolveConstantNoLock(null, name, out storage);
                cache.Update(exists, newVersion);
                return exists;
            }
        }

        /// <summary>
        /// defined? A1::..::AN
        /// defined? ::A1::..::AN
        /// </summary>
        [Emitted]
        public static bool IsDefinedQualifiedConstant(RubyScope/*!*/ scope, IsDefinedConstantSiteCache/*!*/ cache,
            string/*!*/[]/*!*/ qualifiedName, bool isGlobal) {

            var context = scope.RubyContext;
            using (context.ClassHierarchyLocker()) {
                int newVersion = context.ConstantAccessVersion;

                ConstantStorage storage;
                bool anyMissing;
                RubyModule topModule = isGlobal ? context.ObjectClass : null;
                RubyModule owner;
                try {
                    owner = ResolveQualifiedConstant(scope, qualifiedName, topModule, false, out storage, out anyMissing) as RubyModule;
                } catch {
                    // autoload can raise an exception
                    scope.RubyContext.SetCurrentException(null);
                    return false;
                }
                
                // Note that the owner could be another runtime's module:
                bool exists = owner != null && owner.TryResolveConstant(context, null, qualifiedName[qualifiedName.Length - 1], out storage);
                
                // cache result only if no constant was missing:
                if (!anyMissing) {
                    cache.Update(exists, newVersion);
                }

                return exists;
            }
        }

        /// <summary>
        /// defined? {expr}::A
        /// defined? {expr}::A1::..::AN
        /// </summary>
        [Emitted]
        public static bool IsDefinedExpressionQualifiedConstant(object target, RubyScope/*!*/ scope,
            ExpressionQualifiedIsDefinedConstantSiteCache/*!*/ cache, string/*!*/[]/*!*/ qualifiedName) {

            RubyModule module = target as RubyModule;
            if (module == null) {
                return false;
            }

            var condition = cache.Condition;
            RubyContext context = module.Context;

            // Note that the module can be bound to another runtime:
            if (module.Id == condition.ModuleId && context.ConstantAccessVersion == condition.Version) {
                return cache.Value;
            }

            using (context.ClassHierarchyLocker()) {
                int newVersion = context.ConstantAccessVersion;

                ConstantStorage storage;
                bool exists;
                if (qualifiedName.Length == 1) {
                    // Note that the owner could be another runtime's module:
                    exists = module.TryResolveConstant(context, null, qualifiedName[0], out storage);
                } else {
                    bool anyMissing;
                    RubyModule owner;
                    try {
                        owner = ResolveQualifiedConstant(scope, qualifiedName, module, false, out storage, out anyMissing) as RubyModule;
                    } catch {
                        // autoload can raise an exception:
                        return false;
                    }

                    // Note that the owner could be another runtime's module:
                    exists = owner != null && owner.TryResolveConstant(context, null, qualifiedName[qualifiedName.Length - 1], out storage);

                    // cache result only if no constant was missing:
                    if (anyMissing) {
                        return exists;
                    } 
                }

                cache.Update(exists, newVersion, module);
                return exists;
            }
        }

        private static object ResolveQualifiedConstant(RubyScope/*!*/ scope, string/*!*/[]/*!*/ qualifiedName, RubyModule topModule, bool isGet,
            out ConstantStorage storage, out bool anyMissing) {

            Debug.Assert(qualifiedName.Length >= 2 || qualifiedName.Length == 1 && isGet);
            RubyContext context = scope.RubyContext;
            context.RequiresClassHierarchyLock();

            RubyModule missingConstantOwner;
            RubyGlobalScope globalScope = scope.GlobalScope;
            int nameCount = (isGet) ? qualifiedName.Length : qualifiedName.Length - 1;

            string name = qualifiedName[0];
            if (topModule == null) {
                missingConstantOwner = scope.TryResolveConstantNoLock(globalScope, name, out storage);
            } else if (topModule.TryResolveConstant(context, globalScope, name, out storage)) {
                missingConstantOwner = null;
            } else {
                missingConstantOwner = topModule;
            }

            object result;
            if (missingConstantOwner == null) {
                result = storage.Value;
                anyMissing = false;
            } else {
                anyMissing = true;
                using (context.ClassHierarchyUnlocker()) {
                    result = missingConstantOwner.ConstantMissing(name);
                }
            }

            for (int i = 1; i < nameCount; i++) {
                RubyModule owner = RubyUtils.GetModuleFromObject(scope, result);
                // Note that the owner could be another runtime's module:
                name = qualifiedName[i];
                if (owner.TryResolveConstant(context, globalScope, name, out storage)) {
                    
                    // Constant write updates constant version in a single runtime only. 
                    // Therefore if the chain mixes modules from different runtimes we cannot cache the result.
                    if (owner.Context != context) {
                        anyMissing = true;
                    }
                    
                    result = storage.Value;
                } else {
                    anyMissing = true;
                    using (context.ClassHierarchyUnlocker()) {
                        result = owner.ConstantMissing(name);
                    }
                }
            }

            return result;
        }

        [Emitted]
        public static object GetMissingConstant(RubyScope/*!*/ scope, ConstantSiteCache/*!*/ cache, string/*!*/ name) {
            return scope.GetInnerMostModuleForConstantLookup().ConstantMissing(name);
        }

        [Emitted]
        public static object GetGlobalMissingConstant(RubyScope/*!*/ scope, ConstantSiteCache/*!*/ cache, string/*!*/ name) {
            return scope.RubyContext.ObjectClass.ConstantMissing(name);
        }


        [Emitted] // ConstantVariable:
        public static object SetGlobalConstant(object value, RubyScope/*!*/ scope, string/*!*/ name) {
            RubyUtils.SetConstant(scope.RubyContext.ObjectClass, name, value);
            return value;
        }

        [Emitted] // ConstantVariable:
        public static object SetUnqualifiedConstant(object value, RubyScope/*!*/ scope, string/*!*/ name) {
            RubyUtils.SetConstant(scope.GetInnerMostModuleForConstantLookup(), name, value);
            return value;
        }

        [Emitted] // ConstantVariable:
        public static object SetQualifiedConstant(object value, object target, RubyScope/*!*/ scope, string/*!*/ name) {
            RubyUtils.SetConstant(RubyUtils.GetModuleFromObject(scope, target), name, value);
            return value;
        }

        #endregion

        // MakeArray*
        public const int OptimizedOpCallParamCount = 5;
        
        #region MakeArray
        
        [Emitted]
        public static RubyArray/*!*/ MakeArray0() {
            return new RubyArray(0);
        }

        [Emitted]
        public static RubyArray/*!*/ MakeArray1(object item1) {
            RubyArray result = new RubyArray(1);
            result.Add(item1);
            return result;
        }

        [Emitted]
        public static RubyArray/*!*/ MakeArray2(object item1, object item2) {
            RubyArray result = new RubyArray(2);
            result.Add(item1);
            result.Add(item2);
            return result;
        }

        [Emitted]
        public static RubyArray/*!*/ MakeArray3(object item1, object item2, object item3) {
            RubyArray result = new RubyArray(3);
            result.Add(item1);
            result.Add(item2);
            result.Add(item3);
            return result;
        }

        [Emitted]
        public static RubyArray/*!*/ MakeArray4(object item1, object item2, object item3, object item4) {
            RubyArray result = new RubyArray(4);
            result.Add(item1);
            result.Add(item2);
            result.Add(item3);
            result.Add(item4);
            return result;
        }

        [Emitted]
        public static RubyArray/*!*/ MakeArray5(object item1, object item2, object item3, object item4, object item5) {
            RubyArray result = new RubyArray(5);
            result.Add(item1);
            result.Add(item2);
            result.Add(item3);
            result.Add(item4);
            result.Add(item5);
            return result;
        }

        [Emitted]
        public static RubyArray/*!*/ MakeArrayN(object[]/*!*/ items) {
            Debug.Assert(items != null);
            var array = new RubyArray(items.Length);
            array.AddVector(items, 0, items.Length);
            return array;
        }

        #endregion

        #region MakeHash

        [Emitted]
        public static Hash/*!*/ MakeHash0(RubyScope/*!*/ scope) {
            return new Hash(scope.RubyContext.EqualityComparer, 0);
        }
        
        [Emitted]
        public static Hash/*!*/ MakeHash(RubyScope/*!*/ scope, object[]/*!*/ items) {
            return RubyUtils.SetHashElements(scope.RubyContext, new Hash(scope.RubyContext.EqualityComparer, items.Length / 2), items);
        }

        #endregion

        #region Array

        [Emitted]
        public static RubyArray/*!*/ AddRange(RubyArray/*!*/ array, IList/*!*/ list) {
            return array.AddRange(list);
        }

        [Emitted] // method call:
        public static RubyArray/*!*/ AddSubRange(RubyArray/*!*/ result, IList/*!*/ array, int start, int count) {
            return result.AddRange(array, start, count);
        }

        [Emitted]
        public static RubyArray/*!*/ AddItem(RubyArray/*!*/ array, object item) {
            array.Add(item);
            return array;
        }

        [Emitted]
        public static IList/*!*/ SplatAppend(IList/*!*/ array, IList/*!*/ list) {
            Utils.AddRange(array, list);
            return array;
        }

        [Emitted]
        public static object Splat(IList/*!*/ list) {
            if (list.Count <= 1) {
                return (list.Count > 0) ? list[0] : null;
            }

            return list;
        }

        // 1.8 behavior
        [Emitted]
        public static object SplatPair(object value, IList/*!*/ list) {
            if (list.Count == 0) {
                return value;
            }

            RubyArray result = new RubyArray(list.Count + 1);
            result.Add(value);
            result.AddRange(list);
            return result;
        }

        [Emitted]
        public static IList/*!*/ Unsplat(object splattee) {
            var list = splattee as IList;
            if (list == null) {
                list = new RubyArray(1);
                list.Add(splattee);
            }
            return list;
        }

        // CaseExpression
        [Emitted]
        public static bool ExistsUnsplatCompare(CallSite<Func<CallSite, object, object, object>>/*!*/ comparisonSite, object splattee, object value) {
            var list = splattee as IList;
            if (list != null) {
                for (int i = 0; i < list.Count; i++) {
                    if (IsTrue(comparisonSite.Target(comparisonSite, list[i], value))) {
                        return true;
                    }
                }
                return false;
            } else {
                return IsTrue(comparisonSite.Target(comparisonSite, splattee, value)); 
            }
        }

        // CaseExpression
        [Emitted]
        public static bool ExistsUnsplat(object splattee) {
            var list = splattee as IList;
            if (list != null) {
                for (int i = 0; i < list.Count; i++) {
                    if (IsTrue(list[i])) {
                        return true;
                    }
                }
                return false;
            } else {
                return IsTrue(splattee);
            }
        }

        [Emitted] // parallel assignment:
        public static object GetArrayItem(IList/*!*/ array, int index) {
            Debug.Assert(index >= 0);
            return index < array.Count ? array[index] : null;
        }

        [Emitted] // parallel assignment:
        public static object GetTrailingArrayItem(IList/*!*/ array, int index, int explicitCount) {
            Debug.Assert(index >= 0);
            int i = Math.Max(array.Count, explicitCount) - index;
            return i >= 0 ? array[i] : null;
        }

        [Emitted] // parallel assignment:
        public static RubyArray/*!*/ GetArrayRange(IList/*!*/ array, int startIndex, int explicitCount) {
            int size = array.Count - explicitCount;
            if (size > 0) {
                RubyArray result = new RubyArray(size);
                for (int i = 0; i < size; i++) {
                    result.Add(array[startIndex + i]);
                }
                return result;
            } else {
                return new RubyArray();
            }
        }

        #endregion

        #region CLR Vectors (factories mimic Ruby Array factories)

        [Emitted, RubyConstructor]
        public static object/*!*/ CreateVector<TElement>(
            ConversionStorage<TElement>/*!*/ elementConversion, 
            ConversionStorage<Union<IList, int>>/*!*/ toAryToInt, 
            BlockParam block, RubyClass/*!*/ self, [NotNull]object/*!*/ arrayOrSize) {

            Debug.Assert(typeof(TElement) == self.GetUnderlyingSystemType().GetElementType());

            var site = toAryToInt.GetSite(CompositeConversionAction.Make(self.Context, CompositeConversion.ToAryToInt));
            var union = site.Target(site, arrayOrSize);

            if (union.First != null) {
                // block ignored
                return CreateVectorInternal(elementConversion, union.First);
            } else if (block != null) {
                return PopulateVector(elementConversion, CreateVectorInternal<TElement>(union.Second), block);
            } else {
                return CreateVectorInternal<TElement>(union.Second);
            }
        }

        [Emitted, RubyConstructor]
        public static Array/*!*/ CreateVectorWithValues<TElement>(ConversionStorage<TElement>/*!*/ elementConversion,
            RubyClass/*!*/ self, [DefaultProtocol]int size, [DefaultProtocol]TElement value) {
            Debug.Assert(typeof(TElement) == self.GetUnderlyingSystemType().GetElementType());

            TElement[] result = CreateVectorInternal<TElement>(size);
            for (int i = 0; i < result.Length; i++) {
                result[i] = value;
            }
            return result;
        }

        private static TElement[]/*!*/ CreateVectorInternal<TElement>(int size) {
            if (size < 0) {
                throw RubyExceptions.CreateArgumentError("negative array size");
            }

            return new TElement[size];
        }

        private static Array/*!*/ CreateVectorInternal<TElement>(ConversionStorage<TElement>/*!*/ elementConversion, IList/*!*/ list) {
            var site = elementConversion.GetDefaultConversionSite();

            var result = new TElement[list.Count];
            for (int i = 0; i < result.Length; i++) {
                object item = list[i];
                result[i] = (item is TElement) ? (TElement)item : site.Target(site, item);
            }

            return result;
        }

        private static object PopulateVector<TElement>(ConversionStorage<TElement>/*!*/ elementConversion, TElement[]/*!*/ array, BlockParam/*!*/ block) {
            var site = elementConversion.GetDefaultConversionSite();

            for (int i = 0; i < array.Length; i++) {
                object item;
                if (block.Yield(i, out item)) {
                    return item;
                }
                array[i] = site.Target(site, item);
            }
            return array;
        }

        #endregion

        #region Global Variables

        [Emitted]
        public static object GetGlobalVariable(RubyScope/*!*/ scope, string/*!*/ name) {
            object value;
            // no error reported if the variable doesn't exist:
            scope.RubyContext.TryGetGlobalVariable(scope, name, out value);
            return value;
        }

        [Emitted]
        public static bool IsDefinedGlobalVariable(RubyScope/*!*/ scope, string/*!*/ name) {
            GlobalVariable variable;
            return scope.RubyContext.TryGetGlobalVariable(name, out variable) && variable.IsDefined;
        }

        [Emitted]
        public static object SetGlobalVariable(object value, RubyScope/*!*/ scope, string/*!*/ name) {
            scope.RubyContext.SetGlobalVariable(scope, name, value);
            return value;
        }

        [Emitted]
        public static void AliasGlobalVariable(RubyScope/*!*/ scope, string/*!*/ newName, string/*!*/ oldName) {
            scope.RubyContext.AliasGlobalVariable(newName, oldName);
        }

        #endregion

        #region DLR Scopes

        internal static bool TryGetGlobalScopeConstant(RubyContext/*!*/ context, Scope/*!*/ scope, string/*!*/ name, out object value) {
            string mangled;
            ScopeStorage scopeStorage = ((object)scope.Storage) as ScopeStorage;
            if (scopeStorage != null) {
                return scopeStorage.TryGetValue(name, false, out value)
                    || (mangled = RubyUtils.TryMangleName(name)) != null && scopeStorage.TryGetValue(mangled, false, out value);
            } else {
                return context.Operations.TryGetMember(scope, name, out value)
                    || (mangled = RubyUtils.TryMangleName(name)) != null && context.Operations.TryGetMember(scope, mangled, out value);
            }
        }

        // TODO:
        internal static void ScopeSetMember(Scope scope, string name, object value) {
            object storage = (object)scope.Storage;

            var scopeStorage = storage as ScopeStorage;
            if (scopeStorage != null) {
                scopeStorage.SetValue(name, false, value);
                return;
            }

            var stringDict = storage as StringDictionaryExpando;
            if (stringDict != null) {
                stringDict.Dictionary[name] = value;
                return;
            }
            
            throw new NotImplementedException();
        }

        // TODO:
        internal static bool ScopeContainsMember(Scope scope, string name) {
            object storage = (object)scope.Storage;

            var scopeStorage = storage as ScopeStorage;
            if (scopeStorage != null) {
                return scopeStorage.HasValue(name, false);
            }

            var stringDict = storage as StringDictionaryExpando;
            if (stringDict != null) {
                return stringDict.Dictionary.ContainsKey(name);
            }

            throw new NotImplementedException();
        }

        // TODO:
        internal static bool ScopeDeleteMember(Scope scope, string name) {
            object storage = (object)scope.Storage;

            var scopeStorage = storage as ScopeStorage;
            if (scopeStorage != null) {
                return scopeStorage.DeleteValue(name, false);
            }

            var stringDict = storage as StringDictionaryExpando;
            if (stringDict != null) {
                return stringDict.Dictionary.Remove(name);
            }

            throw new NotImplementedException();
        }

        // TODO:
        internal static IList<KeyValuePair<string, object>> ScopeGetItems(Scope scope) {
            object storage = (object)scope.Storage;

            var scopeStorage = storage as ScopeStorage;
            if (scopeStorage != null) {
                return scopeStorage.GetItems();
            }

            var stringDict = storage as StringDictionaryExpando;
            if (stringDict != null) {
                var list = new KeyValuePair<string, object>[stringDict.Dictionary.Count];
                int i = 0;
                foreach (var entry in stringDict.Dictionary) {
                    list[i++] = entry;
                }
                return list;
            }

            throw new NotImplementedException();
        }

        #endregion

        #region Regex

        [Emitted] //RegexMatchReference:
        public static MutableString GetCurrentMatchGroup(RubyScope/*!*/ scope, int index) {
            Debug.Assert(index >= 0);
            return scope.GetInnerMostClosureScope().GetCurrentMatchGroup(index);
        }

        [Emitted] //RegexMatchReference:
        public static MatchData GetCurrentMatchData(RubyScope/*!*/ scope) {
            return scope.GetInnerMostClosureScope().CurrentMatch;
        }

        [Emitted] //RegexMatchReference:
        public static MutableString GetCurrentMatchLastGroup(RubyScope/*!*/ scope) {
            return scope.GetInnerMostClosureScope().GetCurrentMatchLastGroup();
        }

        [Emitted] //RegexMatchReference:
        public static MutableString GetCurrentPreMatch(RubyScope/*!*/ scope) {
            return scope.GetInnerMostClosureScope().GetCurrentPreMatch();
        }

        [Emitted] //RegexMatchReference:
        public static MutableString GetCurrentPostMatch(RubyScope/*!*/ scope) {
            return scope.GetInnerMostClosureScope().GetCurrentPostMatch();
        }

        [Emitted] //RegularExpression:
        public static bool MatchLastInputLine(RubyRegex/*!*/ regex, RubyScope/*!*/ scope) {
            var str = scope.GetInnerMostClosureScope().LastInputLine as MutableString;
            return (str != null) ? RubyRegex.SetCurrentMatchData(scope, regex, str) != null : false;
        }

        [Emitted] //MatchExpression:
        public static object MatchString(MutableString str, RubyRegex/*!*/ regex, RubyScope/*!*/ scope) {
            var match = RubyRegex.SetCurrentMatchData(scope, regex, str);
            return (match != null) ? ScriptingRuntimeHelpers.Int32ToObject(match.Index) : null;
        }

        #endregion

        public const char SuffixLiteral = 'L';       // Repr: literal string
        public const char SuffixMutable = 'M';       // non-literal "...#{expr}..."

        /// <summary>
        /// Specialized signatures exist for upto the following number of string parts
        /// </summary>
        public const int MakeStringParamCount = 2;

        #region CreateRegex

        private static RubyRegex/*!*/ CreateRegexWorker(
            RubyRegexOptions options, 
            StrongBox<RubyRegex> regexpCache, 
            bool isLiteralWithoutSubstitutions,
            Func<RubyRegex> createRegex) {

            try {
                bool once = ((options & RubyRegexOptions.Once) == RubyRegexOptions.Once) || isLiteralWithoutSubstitutions;
                if (once) {
                    // Note that the user is responsible for thread synchronization
                    if (regexpCache.Value == null) {
                        regexpCache.Value = createRegex();
                    }
                    return regexpCache.Value;
                } else {
                    // In the future, we can consider caching the last Regexp. For some regexp literals 
                    // with substitution, the substition will be the same most of the time
                    return createRegex();
                }
            } catch (RegexpError e) {
                if (isLiteralWithoutSubstitutions) {
                    // Ideally, this should be thrown during parsing of the source, even if the 
                    // expression happens to be unreachable at runtime.
                    throw new SyntaxError(e.Message);
                } else {
                    throw;
                }
            }
        }

        [Emitted]
        public static RubyRegex/*!*/ CreateRegexB(byte[]/*!*/ bytes, RubyEncoding/*!*/ encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache) {
            Func<RubyRegex> createRegex = delegate { return new RubyRegex(CreateMutableStringB(bytes, encoding), options); };
            return CreateRegexWorker(options, regexpCache, true, createRegex);
        }

        [Emitted]
        public static RubyRegex/*!*/ CreateRegexL(string/*!*/ str1, RubyEncoding/*!*/ encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache) {
            Func<RubyRegex> createRegex = delegate { return new RubyRegex(CreateMutableStringL(str1, encoding), options); };
            return CreateRegexWorker(options, regexpCache, true, createRegex);
        }
        
        [Emitted]
        public static RubyRegex/*!*/ CreateRegexM(MutableString str1, RubyEncoding/*!*/ encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache) {
            Func<RubyRegex> createRegex = delegate { return new RubyRegex(CreateMutableStringM(str1, encoding), options); };
            return CreateRegexWorker(options, regexpCache, false, createRegex);
        }

        [Emitted]
        public static RubyRegex/*!*/ CreateRegexLM(string/*!*/ str1, MutableString str2, RubyEncoding/*!*/ encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache) {
            Func<RubyRegex> createRegex = delegate { return new RubyRegex(CreateMutableStringLM(str1, str2, encoding), options); };
            return CreateRegexWorker(options, regexpCache, false, createRegex);
        }

        [Emitted]
        public static RubyRegex/*!*/ CreateRegexML(MutableString str1, string/*!*/ str2, RubyEncoding/*!*/ encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache) {
            Func<RubyRegex> createRegex = delegate { return new RubyRegex(CreateMutableStringML(str1, str2, encoding), options); };
            return CreateRegexWorker(options, regexpCache, false, createRegex);
        }

        [Emitted]
        public static RubyRegex/*!*/ CreateRegexMM(MutableString str1, MutableString str2, RubyEncoding/*!*/ encoding, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache) {
            Func<RubyRegex> createRegex = delegate { return new RubyRegex(CreateMutableStringMM(str1, str2, encoding), options); };
            return CreateRegexWorker(options, regexpCache, false, createRegex);
        }

        [Emitted]
        public static RubyRegex/*!*/ CreateRegexN(MutableString[]/*!*/ strings, RubyRegexOptions options, StrongBox<RubyRegex> regexpCache) {
            Func<RubyRegex> createRegex = delegate { return new RubyRegex(CreateMutableStringN(strings), options); };
            return CreateRegexWorker(options, regexpCache, false, createRegex);
        }

        #endregion

        #region CreateMutableString

        [Emitted]
        public static MutableString/*!*/ CreateMutableStringB(byte[]/*!*/ bytes, RubyEncoding/*!*/ encoding) {
            return MutableString.CreateBinary(bytes, encoding);
        }

        [Emitted]
        public static MutableString/*!*/ CreateMutableStringL(string/*!*/ str1, RubyEncoding/*!*/ encoding) {
            return MutableString.Create(str1, encoding);
        }

        [Emitted]
        public static MutableString/*!*/ CreateMutableStringM(MutableString str1, RubyEncoding/*!*/ encoding) {
            return MutableString.CreateInternal(str1, encoding);
        }

        [Emitted]
        public static MutableString/*!*/ CreateMutableStringLM(string/*!*/ str1, MutableString str2, RubyEncoding/*!*/ encoding) {
            return MutableString.CreateMutable(str1, encoding).Append(str2);
        }

        [Emitted]
        public static MutableString/*!*/ CreateMutableStringML(MutableString str1, string/*!*/ str2, RubyEncoding/*!*/ encoding) {
            return MutableString.CreateInternal(str1, encoding).Append(str2);
        }

        [Emitted]
        public static MutableString/*!*/ CreateMutableStringMM(MutableString str1, MutableString str2, RubyEncoding/*!*/ encoding) {
            return MutableString.CreateInternal(str1, encoding).Append(str2);
        }

        // TODO: we should emit Append calls directly, and not create an array first
        [Emitted]
        public static MutableString/*!*/ CreateMutableStringN(MutableString/*!*/[]/*!*/ parts) {
            Debug.Assert(parts.Length > 0);
            var result = MutableString.CreateMutable(RubyEncoding.Ascii);

            for (int i = 0; i < parts.Length; i++) {
                result.Append(parts[i]);
            }

            return result;
        }

        #endregion

        #region CreateSymbol

        [Emitted]
        public static RubySymbol/*!*/ CreateSymbolM(MutableString str1, RubyEncoding/*!*/ encoding, RubyScope/*!*/ scope) {
            return scope.RubyContext.CreateSymbol(CreateMutableStringM(str1, encoding), false);
        }

        [Emitted]
        public static RubySymbol/*!*/ CreateSymbolLM(string/*!*/ str1, MutableString str2, RubyEncoding/*!*/ encoding, RubyScope/*!*/ scope) {
            return scope.RubyContext.CreateSymbol(CreateMutableStringLM(str1, str2, encoding), false);
        }

        [Emitted]
        public static RubySymbol/*!*/ CreateSymbolML(MutableString str1, string/*!*/ str2, RubyEncoding/*!*/ encoding, RubyScope/*!*/ scope) {
            return scope.RubyContext.CreateSymbol(CreateMutableStringML(str1, str2, encoding), false);
        }
        
        [Emitted]
        public static RubySymbol/*!*/ CreateSymbolMM(MutableString str1, MutableString str2, RubyEncoding/*!*/ encoding, RubyScope/*!*/ scope) {
            return scope.RubyContext.CreateSymbol(CreateMutableStringMM(str1, str2, encoding), false);
        }

        [Emitted]
        public static RubySymbol/*!*/ CreateSymbolN(MutableString[]/*!*/ strings, RubyScope/*!*/ scope) {
            return scope.RubyContext.CreateSymbol(CreateMutableStringN(strings), false);
        }

        #endregion

        #region Strings, Encodings

        [Emitted]
        public static RubyEncoding/*!*/ CreateEncoding(int codepage) {
            return RubyEncoding.GetRubyEncoding(codepage);
        }

        [Emitted, Obsolete("Internal only")]
        public static byte[]/*!*/ GetMutableStringBytes(MutableString/*!*/ str) {

            int byteCount;
            var result = str.GetByteArray(out byteCount);
            return result;
        }

        #endregion

        #region Booleans

        [Emitted]
        public static bool IsTrue(object obj) {
            return (obj is bool) ? (bool)obj == true : obj != null;
        }

        [Emitted]
        public static bool IsFalse(object obj) {
            return (obj is bool) ? (bool)obj == false : obj == null;
        }

        [Emitted]
        public static object NullIfFalse(object obj) {
            return (obj is bool && !(bool)obj) ? null : obj;
        }

        [Emitted]
        public static object NullIfTrue(object obj) {
            return (obj is bool && !(bool)obj || obj == null) ? DefaultArgument : null;
        }

        #endregion

        #region Exceptions

        //
        // NOTE:
        // Exception Ops go directly to the current exception object. MRI ignores potential aliases.
        //

        /// <summary>
        /// Called in try-filter that wraps the entire body of a block. 
        /// We just need to capture stack trace, should not filter out any exception.
        /// </summary>
        [Emitted]
        public static bool FilterBlockException(RubyScope/*!*/ scope, Exception/*!*/ exception) {
            RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
            return false;
        }

        /// <summary>
        /// Called in try-filter that wraps the entire top-level code. 
        /// We just need to capture stack trace, should not filter out any exception.
        /// </summary>
        [Emitted]
        public static bool TraceTopLevelCodeFrame(RubyScope/*!*/ scope, Exception/*!*/ exception) {
            RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
            return false;
        }

        // Ruby method exit filter:
        [Emitted]
        public static bool IsMethodUnwinderTargetFrame(RubyScope/*!*/ scope, Exception/*!*/ exception) {
            var unwinder = exception as MethodUnwinder;
            if (unwinder == null) {
                RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
                return false;
            } else {
                return unwinder.TargetFrame == scope.FlowControlScope;
            }
        }

        [Emitted]
        public static object GetMethodUnwinderReturnValue(Exception/*!*/ exception) {
            return ((MethodUnwinder)exception).ReturnValue;
        }

        [Emitted]
        public static void LeaveMethodFrame(RuntimeFlowControl/*!*/ rfc) {
            rfc.LeaveMethod();
        }
        
        /// <summary>
        /// Filters exceptions raised from EH-body, EH-rescue and EH-else clauses.
        /// </summary>
        [Emitted]
        public static bool CanRescue(RubyScope/*!*/ scope, Exception/*!*/ exception) {
            if (exception is StackUnwinder) {
                return false;
            }

            LocalJumpError lje = exception as LocalJumpError;
            if (lje != null && lje.SkipFrame == scope.FlowControlScope) {
                return false;
            }

            // calls "new" on the exception class if it hasn't been called yet:
            exception = RubyExceptionData.HandleException(scope.RubyContext, exception);

            scope.RubyContext.CurrentException = exception;
            RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
            return true;
        }

        [Emitted]
        public static Exception/*!*/ MarkException(Exception/*!*/ exception) {
            RubyExceptionData.GetInstance(exception).Handled = true;
            return exception;
        }

        [Emitted]
        public static Exception GetCurrentException(RubyScope/*!*/ scope) {
            return scope.RubyContext.CurrentException;
        }

        /// <summary>
        /// Sets $!. Used in EH finally clauses to restore exception stored in oldExceptionVariable local.
        /// </summary>
        [Emitted] 
        public static void SetCurrentException(RubyScope/*!*/ scope, Exception exception) {
            scope.RubyContext.CurrentException = exception;
        }

        [Emitted] //RescueClause:
        public static bool CompareException(BinaryOpStorage/*!*/ comparisonStorage, RubyScope/*!*/ scope, object classObject) {            
            var context = scope.RubyContext;
            var site = comparisonStorage.GetCallSite("===");
            bool result = IsTrue(site.Target(site, classObject, context.CurrentException));
            if (result) {
                RubyExceptionData.ActiveExceptionHandled(context.CurrentException);
            }
            return result;
        }

        [Emitted] //RescueClause:
        public static bool CompareSplattedExceptions(BinaryOpStorage/*!*/ comparisonStorage, RubyScope/*!*/ scope, IList/*!*/ classObjects) {
            for (int i = 0; i < classObjects.Count; i++) {
                if (CompareException(comparisonStorage, scope, classObjects[i])) {
                    return true;
                }
            }
            return false;
        }

        [Emitted] //RescueClause:
        public static bool CompareDefaultException(RubyScope/*!*/ scope) {
            RubyContext ec = scope.RubyContext;

            // MRI doesn't call === here;
            bool result = ec.IsInstanceOf(ec.CurrentException, ec.StandardErrorClass);
            if (result) {
                RubyExceptionData.ActiveExceptionHandled(ec.CurrentException);
            }
            return result;
        }

        [Emitted]
        public static string/*!*/ GetDefaultExceptionMessage(RubyClass/*!*/ exceptionClass) {
            return exceptionClass.Name;
        }

        [Emitted]
        public static ArgumentException/*!*/ CreateArgumentsError(string message) {
            return (ArgumentException)RubyExceptions.CreateArgumentError(message);
        }

        [Emitted]
        public static ArgumentException/*!*/ CreateArgumentsErrorForMissingBlock() {
            return (ArgumentException)RubyExceptions.CreateArgumentError("block not supplied");
        }

        [Emitted]
        public static ArgumentException/*!*/ CreateArgumentsErrorForProc(string className) {
            return (ArgumentException)RubyExceptions.CreateArgumentError(String.Format("wrong type argument {0} (should be callable)", className));
        }

        [Emitted]
        public static ArgumentException/*!*/ MakeWrongNumberOfArgumentsError(int actual, int expected) {
            return new ArgumentException(String.Format("wrong number of arguments ({0} for {1})", actual, expected));
        }

        [Emitted] //SuperCall
        public static Exception/*!*/ MakeTopLevelSuperException() {
            return new MissingMethodException("super called outside of method");
        }

        [Emitted] //SuperCallAction
        public static Exception/*!*/ MakeMissingSuperException(string/*!*/ name) {
            return new MissingMethodException(String.Format("super: no superclass method `{0}'", name));
        }

        [Emitted]
        public static Exception/*!*/ MakeVirtualClassInstantiatedError() {
            return RubyExceptions.CreateTypeError("can't create instance of virtual class");
        }

        [Emitted]
        public static Exception/*!*/ MakeAbstractMethodCalledError(RuntimeMethodHandle/*!*/ method) {
            return new NotImplementedException(String.Format("Abstract method `{0}' not implemented", MethodInfo.GetMethodFromHandle(method)));
        }

        [Emitted]
        public static Exception/*!*/ MakeInvalidArgumentTypesError(string/*!*/ methodName) {
            // TODO:
            return new ArgumentException(String.Format("wrong number or type of arguments for `{0}'", methodName));
        }

        [Emitted]
        public static Exception/*!*/ MakeTypeConversionError(RubyContext/*!*/ context, object value, Type/*!*/ type) {
            return RubyExceptions.CreateTypeConversionError(context.GetClassDisplayName(value), context.GetTypeName(type, true));
        }

        [Emitted]
        public static Exception/*!*/ MakeAmbiguousMatchError(string/*!*/ message) {
            // TODO:
            return new AmbiguousMatchException(message);
        }

        [Emitted]
        public static Exception/*!*/ MakeAllocatorUndefinedError(RubyClass/*!*/ classObj) {
            return RubyExceptions.CreateAllocatorUndefinedError(classObj);
        }

        [Emitted]
        public static Exception/*!*/ MakeNotClrTypeError(RubyClass/*!*/ classObj) {
            return RubyExceptions.CreateNotClrTypeError(classObj);
        }

        [Emitted]
        public static Exception/*!*/ MakeConstructorUndefinedError(RubyClass/*!*/ classObj) {
            return RubyExceptions.CreateTypeError(String.Format("`{0}' doesn't have a visible CLR constructor", 
                classObj.Context.GetTypeName(classObj.TypeTracker.Type, true)
            ));
        }

        [Emitted]
        public static Exception/*!*/ MakeMissingDefaultConstructorError(RubyClass/*!*/ classObj, string/*!*/ initializerOwnerName) {
            return RubyExceptions.CreateMissingDefaultConstructorError(classObj, initializerOwnerName);
        }

        [Emitted]
        public static Exception/*!*/ MakePrivateMethodCalledError(RubyContext/*!*/ context, object target, string/*!*/ methodName) {
            return RubyExceptions.CreatePrivateMethodCalled(context, target, methodName);
        }

        [Emitted]
        public static Exception/*!*/ MakeProtectedMethodCalledError(RubyContext/*!*/ context, object target, string/*!*/ methodName) {
            return RubyExceptions.CreateProtectedMethodCalled(context, target, methodName);
        }

        [Emitted]
        public static Exception/*!*/ MakeClrProtectedMethodCalledError(RubyContext/*!*/ context, object target, string/*!*/ methodName) {
            return new MissingMethodException(
                RubyExceptions.FormatMethodMissingMessage(context, target, methodName, "CLR protected method `{0}' called for {1}; " +
                "CLR protected methods can only be called with a receiver whose class is a Ruby subclass of the class declaring the method")
            );
        }

        [Emitted]
        public static Exception/*!*/ MakeClrVirtualMethodCalledError(RubyContext/*!*/ context, object target, string/*!*/ methodName) {
            return new MissingMethodException(
                RubyExceptions.FormatMethodMissingMessage(context, target, methodName, "Virtual CLR method `{0}' called via super from {1}; " +
                "Super calls to virtual CLR methods can only be used in a Ruby subclass of the class declaring the method")
            );
        }

        [Emitted]
        public static Exception/*!*/ MakeImplicitSuperInBlockMethodError() {
            return RubyExceptions.CreateRuntimeError("implicit argument passing of super from method defined by define_method() is not supported. Specify all arguments explicitly.");
        }

        [Emitted]
        public static Exception/*!*/ MakeMissingMethodError(RubyContext/*!*/ context, object self, string/*!*/ methodName) {
            return RubyExceptions.CreateMethodMissing(context, self, methodName);
        }

        [Emitted]
        public static Exception/*!*/ MakeMissingMemberError(string/*!*/ memberName) {
            return new MissingMemberException(String.Format(CultureInfo.InvariantCulture, "undefined member: `{0}'", memberName));
        }

        #endregion

        #region Ranges

        [Emitted]
        public static Range/*!*/ CreateInclusiveRange(object begin, object end, RubyScope/*!*/ scope, BinaryOpStorage/*!*/ comparisonStorage) {
            return new Range(comparisonStorage, scope.RubyContext, begin, end, false);
        }

        [Emitted]
        public static Range/*!*/ CreateExclusiveRange(object begin, object end, RubyScope/*!*/ scope, BinaryOpStorage/*!*/ comparisonStorage) {
            return new Range(comparisonStorage, scope.RubyContext, begin, end, true);
        }

        [Emitted]
        public static Range/*!*/ CreateInclusiveIntegerRange(int begin, int end) {
            return new Range(begin, end, false);
        }

        [Emitted]
        public static Range/*!*/ CreateExclusiveIntegerRange(int begin, int end) {
            return new Range(begin, end, true);
        }

        #endregion

        #region Dynamic Operations

        // allocator for struct instances:
        [Emitted]
        public static RubyStruct/*!*/ AllocateStructInstance(RubyClass/*!*/ self) {
            return RubyStruct.Create(self);
        }

        // factory for struct instances:
        [Emitted]
        public static RubyStruct/*!*/ CreateStructInstance(RubyClass/*!*/ self, [NotNull]params object[]/*!*/ items) {
            var result = RubyStruct.Create(self);
            result.SetValues(items);
            return result;
        }

        [Emitted]
        public static DynamicMetaObject/*!*/ GetMetaObject(IRubyObject/*!*/ obj, MSA.Expression/*!*/ parameter) {
            return new RubyObject.Meta(parameter, BindingRestrictions.Empty, obj);
        }

        [Emitted]
        public static RubyMethod/*!*/ CreateBoundMember(object target, RubyMemberInfo/*!*/ info, string/*!*/ name) {
            return new RubyMethod(target, info, name);
        }

        [Emitted]
        public static RubyMethod/*!*/ CreateBoundMissingMember(object target, RubyMemberInfo/*!*/ info, string/*!*/ name) {
            return new RubyMethod.Curried(target, info, name);
        }

        [Emitted]
        public static bool IsClrSingletonRuleValid(RubyContext/*!*/ context, object/*!*/ target, int expectedVersion) {
            RubyInstanceData data;
            RubyClass immediate;

            // TODO: optimize this (we can have a hashtable of singletons per class: Weak(object) => Struct { ImmediateClass, InstanceVariables, Flags }):
            return context.TryGetClrTypeInstanceData(target, out data) && (immediate = data.ImmediateClass) != null && immediate.IsSingletonClass
                && immediate.Version.Method == expectedVersion;
        }

        [Emitted]
        public static bool IsClrNonSingletonRuleValid(RubyContext/*!*/ context, object/*!*/ target, VersionHandle/*!*/ versionHandle, int expectedVersion) {
            RubyInstanceData data;
            RubyClass immediate;

            return versionHandle.Method == expectedVersion
                // TODO: optimize this (we can have a hashtable of singletons per class: Weak(object) => Struct { ImmediateClass, InstanceVariables, Flags }):
                && !(context.TryGetClrTypeInstanceData(target, out data) && (immediate = data.ImmediateClass) != null && immediate.IsSingletonClass);
        }

        // super call condition
        [Emitted]
        public static object GetSuperCallTarget(RubyScope/*!*/ scope, int targetId) {
            while (true) {
                switch (scope.Kind) {
                    case ScopeKind.Method:
                        return targetId == 0 ? scope.SelfObject : NeedsUpdate;

                    case ScopeKind.BlockMethod:
                        return targetId == ((RubyBlockScope)scope).BlockFlowControl.Proc.Method.Id ? scope.SelfObject : NeedsUpdate;

                    case ScopeKind.TopLevel:
                        // This method is only called if there was method or block-method scope in lexical scope chain.
                        // Once there is it cannot be undone. It can only be shadowed by a block scope that became block-method scope, or
                        // a block-method scope's target-id can be changed.
                        throw Assert.Unreachable;
                }

                scope = scope.Parent;
            }
        }

        // super call condition
        [Emitted]
        public static bool IsSuperOutOfMethodScope(RubyScope/*!*/ scope) {
            while (true) {
                switch (scope.Kind) {
                    case ScopeKind.Method:
                    case ScopeKind.BlockMethod:
                        return false;

                    case ScopeKind.TopLevel:
                        return true;
                }

                scope = scope.Parent;
            }
        }

        #endregion

        #region Conversions

        [Emitted] // ProtocolConversionAction
        public static Proc/*!*/ ToProcValidator(string/*!*/ className, object obj) {
            Proc result = obj as Proc;
            if (result == null) {
                throw RubyExceptions.CreateReturnTypeError(className, "to_proc", "Proc");
            }
            return result;
        }

        // Used for implicit conversions from System.String to MutableString (to_str conversion like).
        [Emitted]
        public static MutableString/*!*/ StringToMutableString(string/*!*/ str) {
            return MutableString.Create(str, RubyEncoding.UTF8);
        }

        // Used for implicit conversions from System.Object to MutableString (to_s conversion like).
        [Emitted]
        public static MutableString/*!*/ ObjectToMutableString(object/*!*/ value) {
            return (value != null) ? MutableString.Create(value.ToString(), RubyEncoding.UTF8) : MutableString.FrozenEmpty;
        }

        [Emitted] // ProtocolConversionAction
        public static MutableString/*!*/ ToStringValidator(string/*!*/ className, object obj) {
            MutableString result = obj as MutableString;
            if (result == null) {
                throw RubyExceptions.CreateReturnTypeError(className, "to_str", "String");
            }
            return result;
        }

        [Emitted] // ProtocolConversionAction
        public static string/*!*/ ToSymbolValidator(string/*!*/ className, object obj) {
            var str = obj as MutableString;
            if (str == null) {
                throw RubyExceptions.CreateReturnTypeError(className, "to_str", "String"); 
            }
            return str.ConvertToString();
        }

        [Emitted] // ProtocolConversionAction
        public static string/*!*/ ConvertSymbolToClrString(RubySymbol/*!*/ value) {
            return value.ToString();
        }

        [Emitted] // ProtocolConversionAction
        public static string/*!*/ ConvertRubySymbolToClrString(RubyContext/*!*/ context, int value) {
            context.ReportWarning("do not use Fixnums as Symbols");

            RubySymbol result = context.FindSymbol(value);
            if (result != null) {
                return result.ToString();
            } else {
                throw RubyExceptions.CreateArgumentError(String.Format("{0} is not a symbol", value));
            }
        }

        [Emitted] // ProtocolConversionAction
        public static string/*!*/ ConvertMutableStringToClrString(MutableString/*!*/ value) {
            return value.ConvertToString();
        }

        [Emitted] // ProtocolConversionAction
        public static MutableString/*!*/ ConvertSymbolToMutableString(RubySymbol/*!*/ value) {
            // TODO: this is used for DefaultProtocol conversions; we might avoid clonning in some (many?) cases
            return value.String.Clone();
        }
        
        [Emitted] // ProtocolConversionAction
        public static RubyRegex/*!*/ ToRegexValidator(string/*!*/ className, object obj) {
            return new RubyRegex(RubyRegex.Escape(ToStringValidator(className, obj)), RubyRegexOptions.NONE);
        }

        [Emitted] // ProtocolConversionAction
        public static IList/*!*/ ToArrayValidator(string/*!*/ className, object obj) {
            var result = obj as IList;
            if (result == null) {
                throw RubyExceptions.CreateReturnTypeError(className, "to_ary", "Array");
            }
            return result;
        }

        [Emitted] // ProtocolConversionAction
        public static IList/*!*/ ToAValidator(string/*!*/ className, object obj) {
            var result = obj as IList;
            if (result == null) {
                throw RubyExceptions.CreateReturnTypeError(className, "to_a", "Array");
            }
            return result;
        }

        [Emitted] // ProtocolConversionAction
        public static IDictionary<object, object>/*!*/ ToHashValidator(string/*!*/ className, object obj) {
            var result = obj as IDictionary<object, object>;
            if (result == null) {
                throw RubyExceptions.CreateReturnTypeError(className, "to_hash", "Hash");
            }
            return result;
        }

        private static int ToIntValidator(string/*!*/ className, string/*!*/ targetType, object obj) {
            if (obj is int) {
                return (int)obj;
            }

            var bignum = obj as BigInteger;
            if ((object)bignum != null) {
                int fixnum;
                if (bignum.AsInt32(out fixnum)) {
                    return fixnum;
                }
                throw RubyExceptions.CreateRangeError("bignum too big to convert into {0}", targetType);
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_int", "Integer");
        }

        [Emitted] // ProtocolConversionAction
        public static int ToFixnumValidator(string/*!*/ className, object obj) {
            return ToIntValidator(className, "Fixnum", obj);
        }

        [Emitted] // ProtocolConversionAction
        public static Byte ToByteValidator(string/*!*/ className, object obj) {
            return Converter.ToByte(ToIntValidator(className, "System::Byte", obj));
        }

        [Emitted] // ProtocolConversionAction
        public static SByte ToSByteValidator(string/*!*/ className, object obj) {
            return Converter.ToSByte(ToIntValidator(className, "System::SByte", obj));
        }

        [Emitted] // ProtocolConversionAction
        public static Int16 ToInt16Validator(string/*!*/ className, object obj) {
            return Converter.ToInt16(ToIntValidator(className, "System::Int16", obj));
        }

        [Emitted] // ProtocolConversionAction
        public static UInt16 ToUInt16Validator(string/*!*/ className, object obj) {
            return Converter.ToUInt16(ToIntValidator(className, "System::UInt16", obj));
        }

        [Emitted] // ProtocolConversionAction
        public static UInt32 ToUInt32Validator(string/*!*/ className, object obj) {
            if (obj is int) {
                return Converter.ToUInt32((int)obj);
            }

            var bignum = obj as BigInteger;
            if ((object)bignum != null) {
                return Converter.ToUInt32(bignum);
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
        }

        [Emitted] // ProtocolConversionAction
        public static Int64 ToInt64Validator(string/*!*/ className, object obj) {
            if (obj is int) {
                return (int)obj;
            }

            var bignum = obj as BigInteger;
            if ((object)bignum != null) {
                return Converter.ToInt64(bignum);
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
        }

        [Emitted] // ProtocolConversionAction
        public static UInt64 ToUInt64Validator(string/*!*/ className, object obj) {
            if (obj is int) {
                return Converter.ToUInt64((int)obj);
            }

            var bignum = obj as BigInteger;
            if ((object)bignum != null) {
                return Converter.ToUInt64(bignum);
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
        }

        [Emitted] // ProtocolConversionAction
        public static BigInteger ToBignumValidator(string/*!*/ className, object obj) {
            if (obj is int) {
                return (int)obj;
            }

            var bignum = obj as BigInteger;
            if ((object)bignum != null) {
                return bignum;
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
        }

        [Emitted] // ProtocolConversionAction
        public static IntegerValue ToIntegerValidator(string/*!*/ className, object obj) {
            if (obj is int) {
                return new IntegerValue((int)obj);
            }

            var bignum = obj as BigInteger;
            if ((object)bignum != null) {
                return new IntegerValue(bignum);
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_int/to_i", "Integer");
        }

        [Emitted] // ProtocolConversionAction
        public static double ToDoubleValidator(string/*!*/ className, object obj) {
            if (obj is double) {
                return (double)obj;
            }

            if (obj is float) {
                return (double)(float)obj;
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_f", "Float");
        }

        [Emitted] // ProtocolConversionAction
        public static float ToSingleValidator(string/*!*/ className, object obj) {
            if (obj is double) {
                return (float)(double)obj;
            }

            if (obj is float) {
                return (float)obj;
            }

            throw RubyExceptions.CreateReturnTypeError(className, "to_f", "System::Single");
        }

        [Emitted]
        public static double ConvertBignumToFloat(BigInteger/*!*/ value) {
            double result;
            return value.TryToFloat64(out result) ? result : (value.IsNegative() ? Double.NegativeInfinity : Double.PositiveInfinity);
        }

        [Emitted]
        public static double ConvertMutableStringToFloat(RubyContext/*!*/ context, MutableString/*!*/ value) {
            return ConvertStringToFloat(context, value.ConvertToString());
        }

        [Emitted]
        public static double ConvertStringToFloat(RubyContext/*!*/ context, string/*!*/ value) {
            double result;
            bool complete;
            if (Tokenizer.TryParseDouble(value, out result, out complete) && complete) {
                return result;
            }

            throw RubyExceptions.InvalidValueForType(context, value, "Float");
        }

        [Emitted] // ProtocolConversionAction
        public static Exception/*!*/ CreateTypeConversionError(string/*!*/ fromType, string/*!*/ toType) {
            return RubyExceptions.CreateTypeConversionError(fromType, toType);
        }

        [Emitted] // ConvertToFixnumAction
        public static int ConvertBignumToFixnum(BigInteger/*!*/ bignum) {
            int fixnum;
            if (bignum.AsInt32(out fixnum)) {
                return fixnum;
            }
            throw RubyExceptions.CreateRangeError("bignum too big to convert into Fixnum");
        }

        [Emitted] // ConvertDoubleToFixnum
        public static int ConvertDoubleToFixnum(double value) {
            try {
                return checked((int)value);
            } catch (OverflowException) {
                throw RubyExceptions.CreateRangeError(String.Format("float {0} out of range of Fixnum", value));
            }
        }

        [Emitted] // ConvertToSAction
        public static MutableString/*!*/ ToSDefaultConversion(RubyContext/*!*/ context, object target, object converted) {
            return converted as MutableString ?? RubyUtils.ObjectToMutableString(context, target);
        }

        #endregion
        
        #region Instance variable support

        [Emitted]
        public static object GetInstanceVariable(RubyScope/*!*/ scope, object self, string/*!*/ name) {
            RubyInstanceData data = scope.RubyContext.TryGetInstanceData(self);
            return (data != null) ? data.GetInstanceVariable(name) : null;
        }

        [Emitted]
        public static bool IsDefinedInstanceVariable(RubyScope/*!*/ scope, object self, string/*!*/ name) {
            RubyInstanceData data = scope.RubyContext.TryGetInstanceData(self);
            if (data == null) return false;
            object value;
            return data.TryGetInstanceVariable(name, out value);
        }

        [Emitted]
        public static object SetInstanceVariable(object self, object value, RubyScope/*!*/ scope, string/*!*/ name) {
            scope.RubyContext.SetInstanceVariable(self, name, value);
            return value;
        }

        #endregion

        #region Class Variables

        [Emitted]
        public static object GetClassVariable(RubyScope/*!*/ scope, string/*!*/ name) {
            // owner is the first module in scope:
            RubyModule owner = scope.GetInnerMostModuleForClassVariableLookup();
            return GetClassVariableInternal(owner, name);
        }

        private static object GetClassVariableInternal(RubyModule/*!*/ module, string/*!*/ name) {
            object value;
            if (module.TryResolveClassVariable(name, out value) == null) {
                throw RubyExceptions.CreateNameError(String.Format("uninitialized class variable {0} in {1}", name, module.Name));
            }
            return value;
        }

        [Emitted]
        public static object TryGetClassVariable(RubyScope/*!*/ scope, string/*!*/ name) {
            object value;
            // owner is the first module in scope:
            scope.GetInnerMostModuleForClassVariableLookup().TryResolveClassVariable(name, out value);
            return value;
        }

        [Emitted]
        public static bool IsDefinedClassVariable(RubyScope/*!*/ scope, string/*!*/ name) {
            // owner is the first module in scope:
            RubyModule owner = scope.GetInnerMostModuleForClassVariableLookup();
            object value;
            return owner.TryResolveClassVariable(name, out value) != null;
        }

        [Emitted]
        public static object SetClassVariable(object value, RubyScope/*!*/ scope, string/*!*/ name) {
            return SetClassVariableInternal(scope.GetInnerMostModuleForClassVariableLookup(), name, value);
        }

        private static object SetClassVariableInternal(RubyModule/*!*/ lexicalOwner, string/*!*/ name, object value) {
            object oldValue;
            RubyModule owner = lexicalOwner.TryResolveClassVariable(name, out oldValue);
            (owner ?? lexicalOwner).SetClassVariable(name, value);
            return value;
        }

        #endregion

        #region Ruby Types

        [Emitted]
        public static string/*!*/ ObjectToString(IRubyObject/*!*/ obj) {
            return RubyUtils.ObjectToMutableString(obj).ToString();
        }

        [Emitted] //RubyTypeBuilder
        public static RubyInstanceData/*!*/ GetInstanceData(ref RubyInstanceData/*!*/ instanceData) {
            if (instanceData == null) {
                Interlocked.CompareExchange(ref instanceData, new RubyInstanceData(), null);
            }
            return instanceData;
        }

        [Emitted]
        public static bool IsObjectFrozen(RubyInstanceData instanceData) {
            return instanceData != null && instanceData.IsFrozen;
        }

        [Emitted]
        public static bool IsObjectTainted(RubyInstanceData instanceData) {
            return instanceData != null && instanceData.IsTainted;
        }

        [Emitted]
        public static bool IsObjectUntrusted(RubyInstanceData instanceData) {
            return instanceData != null && instanceData.IsUntrusted;
        }

        [Emitted]
        public static void FreezeObject(ref RubyInstanceData instanceData) {
            RubyOps.GetInstanceData(ref instanceData).Freeze();
        }

        [Emitted]
        public static void SetObjectTaint(ref RubyInstanceData instanceData, bool value) {
            RubyOps.GetInstanceData(ref instanceData).IsTainted = value;
        }

        [Emitted]
        public static void SetObjectTrustiness(ref RubyInstanceData instanceData, bool untrusted) {
            RubyOps.GetInstanceData(ref instanceData).IsUntrusted = untrusted;
        }

#if !SILVERLIGHT // serialization
        [Emitted(UseReflection = true)] //RubyTypeBuilder
        public static void DeserializeObject(out RubyInstanceData/*!*/ instanceData, out RubyClass/*!*/ immediateClass, SerializationInfo/*!*/ info) {
            immediateClass = (RubyClass)info.GetValue(RubyUtils.SerializationInfoClassKey, typeof(RubyClass));
            RubyInstanceData newInstanceData = null;
            foreach (SerializationEntry entry in info) {
                if (entry.Name.StartsWith("@", StringComparison.Ordinal)) {
                    if (newInstanceData == null) {
                        newInstanceData = new RubyInstanceData();
                    }
                    newInstanceData.SetInstanceVariable(entry.Name, entry.Value);
                }
            }
            instanceData = newInstanceData;
        }

        [Emitted(UseReflection = true)] //RubyTypeBuilder
        public static void SerializeObject(RubyInstanceData instanceData, RubyClass/*!*/ immediateClass, SerializationInfo/*!*/ info) {
            info.AddValue(RubyUtils.SerializationInfoClassKey, immediateClass, typeof(RubyClass));
            if (instanceData != null) {
                string[] instanceNames = instanceData.GetInstanceVariableNames();
                foreach (string name in instanceNames) {
                    object value;
                    if (!instanceData.TryGetInstanceVariable(name, out value)) {
                        value = null;
                    }
                    info.AddValue(name, value, typeof(object));
                }
            }
        }
#endif
        #endregion

        #region Delegates, Events

        /// <summary>
        /// Hooks up an event to call a proc at hand.
        /// EventInfo is passed in as object since it is an internal type.
        /// </summary>
        [Emitted]
        public static Proc/*!*/ HookupEvent(RubyEventInfo/*!*/ eventInfo, object/*!*/ target, Proc/*!*/ proc) {
            eventInfo.Tracker.AddHandler(target, proc, eventInfo.Context.DelegateCreator);
            return proc;
        }

        [Emitted]
        public static RubyEvent/*!*/ CreateEvent(RubyEventInfo/*!*/ eventInfo, object/*!*/ target, string/*!*/ name) {
            return new RubyEvent(target, eventInfo, name);
        }

        [Emitted]
        public static Delegate/*!*/ CreateDelegateFromProc(Type/*!*/ type, Proc proc) {
            if (proc == null) {
                throw RubyExceptions.NoBlockGiven();
            }
            BlockParam bp = CreateBfcForProcCall(proc);
            return proc.LocalScope.RubyContext.DelegateCreator.GetDelegate(bp, type);
        }

        [Emitted]
        public static Delegate/*!*/ CreateDelegateFromMethod(Type/*!*/ type, RubyMethod/*!*/ method) {
            return method.Info.Context.DelegateCreator.GetDelegate(method, type);
        }

        #endregion

        #region Tuples

        // Instance variable storages needs MT<n> to be a subclass of MT<m> for all n > m.
        // This property is not true if we used DynamicNull as a generic argument for arities that are not powers of 2 like MutableTuple.MakeTupleType does.
        // We make this property true for all simple tuples, thus instance variable storages can only use tuples of size <= 128.
        internal static Type/*!*/ MakeObjectTupleType(int fieldCount) {
            if (fieldCount <= MutableTuple.MaxSize) {
                if (fieldCount <= 1) {
                    return typeof(MutableTuple<object>);
                } else if (fieldCount <= 2) {
                    return typeof(MutableTuple<object, object>);
                } else if (fieldCount <= 4) {
                    return typeof(MutableTuple<object, object, object, object>);
                } else if (fieldCount <= 8) {
                    return typeof(MutableTuple<object, object, object, object, object, object, object, object>);
                } else if (fieldCount <= 16) {
                    return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                } else if (fieldCount <= 32) {
                    return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                } else if (fieldCount <= 64) {
                    return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                } else {
                    Debug.Assert(!PlatformAdaptationLayer.IsCompactFramework);
                    return MakeObjectTupleType128();
                }
            }

            Type[] types = new Type[fieldCount];
            for (int i = 0; i < types.Length; i++) {
                types[i] = typeof(object);
            }
            return MutableTuple.MakeTupleType(types);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type/*!*/ MakeObjectTupleType128() {
            return typeof(MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
        }

        internal static MutableTuple/*!*/ CreateObjectTuple(int fieldCount) {
            Debug.Assert(fieldCount <= MutableTuple.MaxSize);
            if (fieldCount <= 1) {
                return new MutableTuple<object>();
            } else if (fieldCount <= 2) {
                return new MutableTuple<object, object>();
            } else if (fieldCount <= 4) {
                return new MutableTuple<object, object, object, object>();
            } else if (fieldCount <= 8) {
                return new MutableTuple<object, object, object, object, object, object, object, object>();
            } else if (fieldCount <= 16) {
                return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
            } else if (fieldCount <= 32) {
                return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
            } else if (fieldCount <= 64) {
                return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
            } else {
                Debug.Assert(!PlatformAdaptationLayer.IsCompactFramework);
                return CreateObjectTuple128();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static MutableTuple/*!*/ CreateObjectTuple128() {
            return new MutableTuple<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>();
        }

        #endregion

        [Emitted]
        public static void X(string marker) {
        }
        
        [Emitted]
        public static object CreateDefaultInstance() {
            // nop (stub)
            return null;
        }
    }
}
