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

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using System.Globalization;

namespace IronRuby.Builtins {

    [RubyClass("Proc", Extends = typeof(Proc), Inherits = typeof(Object))]
    public static class ProcOps {

        [RubyConstructor]
        public static void Error(RubyClass/*!*/ self, params object[] args) {
            throw RubyExceptions.CreateAllocatorUndefinedError(self);
        }

        #region Public Instance Methods

        [RubyMethod("==")]
        public static bool Equal(Proc/*!*/ self, [NotNull]Proc/*!*/ other) {
            return self.Dispatcher == other.Dispatcher && self.LocalScope == other.LocalScope && self.Kind == other.Kind;
        }

        [RubyMethod("==")]
        public static bool Equal(Proc/*!*/ self, object other) {
            return false;
        }
        
        [RubyMethod("arity")]
        public static int GetArity(Proc/*!*/ self) {
            return self.Dispatcher.Arity;
        }

        [RubyMethod("binding")]
        public static Binding/*!*/ GetLocalScope(Proc/*!*/ self) {
            return new Binding(self.LocalScope);
        }

        [RubyMethod("dup")]
        [RubyMethod("clone")]
        public static Proc/*!*/ Clone(Proc/*!*/ self) {
            return self.Copy();
        }

        [RubyMethod("to_proc")]
        public static Proc/*!*/ ToProc(Proc/*!*/ self) {
            return self;
        }

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(Proc/*!*/ self) {
            var context = self.LocalScope.RubyContext;

            var str = RubyUtils.ObjectToMutableStringPrefix(context, self);
            if (self.SourcePath != null || self.SourceLine != 0) {
                str.Append('@');
                str.Append(self.SourcePath ?? "(unknown)");
                str.Append(':');
                str.Append(self.SourceLine.ToString(CultureInfo.InvariantCulture));
            }

            if (context.RubyOptions.Compatibility >= RubyCompatibility.Ruby19 && self.Kind == ProcKind.Lambda) {
                str.Append(" (lambda)"); 
            }

            str.Append('>');

            return str;
        }

        #endregion

        #region call, []

        [RubyMethod("[]"), RubyMethod("call")]
        public static object Call(Proc/*!*/ self) {
            RequireParameterCount(self, 0);
            return self.Call();
        }

        [RubyMethod("[]"), RubyMethod("call")]
        public static object Call(Proc/*!*/ self, object arg1) {
            RequireParameterCount(self, 1);
            return self.Call(arg1);
        }   

        [RubyMethod("[]"), RubyMethod("call")]
        public static object Call(Proc/*!*/ self, object arg1, object arg2) {
            RequireParameterCount(self, 2);
            return self.Call(arg1, arg2);
        }

        [RubyMethod("[]"), RubyMethod("call")]
        public static object Call(Proc/*!*/ self, object arg1, object arg2, object arg3) {
            RequireParameterCount(self, 3);
            return self.Call(arg1, arg2, arg3);
        }

        [RubyMethod("[]"), RubyMethod("call")]
        public static object Call(Proc/*!*/ self, object arg1, object arg2, object arg3, object arg4) {
            RequireParameterCount(self, 4);
            return self.Call(arg1, arg2, arg3, arg4);
        }

        [RubyMethod("[]"), RubyMethod("call")]
        public static object Call(Proc/*!*/ self, params object[]/*!*/ args) {
            RequireParameterCount(self, args.Length);
            return self.CallN(args);
        }

        private static void RequireParameterCount(Proc/*!*/ proc, int argCount) {
            int arity;
            if (proc.Kind == ProcKind.Lambda && argCount != (arity = proc.Dispatcher.Arity)) {
                if (arity >= 0) {
                    // arity 1 -> warning reported by block dispatcher
                    if (arity != 1) {
                        throw RubyOps.MakeWrongNumberOfArgumentsError(argCount, arity);
                    }
                } else if (argCount < -arity - 1) {
                    throw RubyOps.MakeWrongNumberOfArgumentsError(argCount, -arity - 1);
                }
            }
        }

        #endregion

        #region TODO: ===, eql?, hash, yield, curry, source_location, lambda? (1.9)

        #endregion

        #region Singleton Methods

        [RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
        public static Proc/*!*/ CreateNew(CallSiteStorage<Func<CallSite, object, object>>/*!*/ storage, 
            RubyScope/*!*/ scope, RubyClass/*!*/ self) {

            RubyMethodScope methodScope = scope.GetInnerMostMethodScope();
            if (methodScope == null || methodScope.BlockParameter == null) {
                throw RubyExceptions.CreateArgumentError("tried to create Proc object without a block");
            }

            var proc = methodScope.BlockParameter;

            // an instance of Proc class, the identity is preserved:
            if (self.GetUnderlyingSystemType() == typeof(Proc)) {
                return proc;
            }

            // an instance of a Proc subclass:
            var result = new Proc.Subclass(self, proc);

            var initialize = storage.GetCallSite("initialize", new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf));
            initialize.Target(initialize, result);

            return result;
        }

        [RubyMethod("new", RubyMethodAttributes.PublicSingleton)]
        public static object CreateNew(CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ storage, 
            BlockParam block, RubyClass/*!*/ self) {

            if (block == null) {
                throw RubyExceptions.CreateArgumentError("tried to create Proc object without a block");
            }

            var proc = block.Proc;

            // an instance of Proc class, the identity is preserved:
            if (self.GetUnderlyingSystemType() == typeof(Proc)) {
                return proc;
            }

            // an instance of a Proc subclass:
            var result = new Proc.Subclass(self, proc);

            // propagate retry and return control flow:
            var initialize = storage.GetCallSite("initialize", new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasBlock));
            object initResult = initialize.Target(initialize, result, block.Proc);
            if (initResult is BlockReturnResult) {
                return initResult;
            }

            return result;
        }

        #endregion
    }
}

