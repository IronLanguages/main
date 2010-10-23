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

        #region ==, eql?, hash, dup, clone

        [RubyMethod("=="), RubyMethod("eql?")]
        public static bool Equal(Proc/*!*/ self, [NotNull]Proc/*!*/ other) {
            return self.Dispatcher == other.Dispatcher && self.LocalScope == other.LocalScope;
        }

        [RubyMethod("=="), RubyMethod("eql?")]
        public static bool Equal(Proc/*!*/ self, object other) {
            return false;
        }

        [RubyMethod("hash")]
        public static int GetHash(Proc/*!*/ self) {
            return self.Dispatcher.GetHashCode() ^ self.LocalScope.GetHashCode();
        }

        [RubyMethod("dup"), RubyMethod("clone")]
        public static Proc/*!*/ Clone(Proc/*!*/ self) {
            return self.Copy();
        }


        #endregion

        #region arity, lambda?, binding, source_location

        [RubyMethod("arity")]
        public static int GetArity(Proc/*!*/ self) {
            return self.Dispatcher.Arity;
        }

        [RubyMethod("lambda?")]
        public static bool IsLambda(Proc/*!*/ self) {
            return self.Kind == ProcKind.Lambda;
        }

        [RubyMethod("binding")]
        public static Binding/*!*/ GetLocalScope(Proc/*!*/ self) {
            return new Binding(self.LocalScope);
        }

        [RubyMethod("source_location")]
        public static RubyArray/*!*/ GetSourceLocation(Proc/*!*/ self) {
            return new RubyArray(2) {
                self.LocalScope.RubyContext.EncodePath(self.Dispatcher.SourcePath),
                self.Dispatcher.SourceLine
            };
        }

        #endregion

        #region to_s, to_proc

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

            if (self.Kind == ProcKind.Lambda) {
                str.Append(" (lambda)"); 
            }

            str.Append('>');

            return str;
        }

        [RubyMethod("to_proc")]
        public static Proc/*!*/ ToProc(Proc/*!*/ self) {
            return self;
        }

        #endregion

        #region call, [],  ===, yield (TODO)
        
        // TODO: 1.9 yield: yield and call might have different semantics with respect to control-flow!

        [RubyMethod("==="), RubyMethod("[]"), RubyMethod("yield"), RubyMethod("call")]
        public static object Call(BlockParam block, Proc/*!*/ self) {
            RequireParameterCount(self, 0);
            return self.Call(block != null ? block.Proc : null);
        }

        [RubyMethod("==="), RubyMethod("[]"), RubyMethod("yield"), RubyMethod("call")]
        public static object Call(BlockParam block, Proc/*!*/ self, object arg1) {
            RequireParameterCount(self, 1);
            return self.Call(block != null ? block.Proc : null, arg1);
        }

        [RubyMethod("==="), RubyMethod("[]"), RubyMethod("yield"), RubyMethod("call")]
        public static object Call(BlockParam block, Proc/*!*/ self, object arg1, object arg2) {
            RequireParameterCount(self, 2);
            return self.Call(block != null ? block.Proc : null, arg1, arg2);
        }

        [RubyMethod("==="), RubyMethod("[]"), RubyMethod("yield"), RubyMethod("call")]
        public static object Call(BlockParam block, Proc/*!*/ self, object arg1, object arg2, object arg3) {
            RequireParameterCount(self, 3);
            return self.Call(block != null ? block.Proc : null, arg1, arg2, arg3);
        }

        [RubyMethod("==="), RubyMethod("[]"), RubyMethod("yield"), RubyMethod("call")]
        public static object Call(BlockParam block, Proc/*!*/ self, object arg1, object arg2, object arg3, object arg4) {
            RequireParameterCount(self, 4);
            return self.Call(block != null ? block.Proc : null, arg1, arg2, arg3, arg4);
        }

        [RubyMethod("==="), RubyMethod("[]"), RubyMethod("yield"), RubyMethod("call")]
        public static object Call(BlockParam block, Proc/*!*/ self, params object[]/*!*/ args) {
            RequireParameterCount(self, args.Length);
            return self.CallN(block != null ? block.Proc : null, args);
        }

        private static void RequireParameterCount(Proc/*!*/ proc, int argCount) {
            int arity;
            if (proc.Kind == ProcKind.Lambda && argCount != (arity = proc.Dispatcher.Arity)) {
                if (arity >= 0) {
                    throw RubyOps.MakeWrongNumberOfArgumentsError(argCount, arity);
                } else if (argCount < -arity - 1) {
                    throw RubyOps.MakeWrongNumberOfArgumentsError(argCount, -arity - 1);
                }
            }
        }

        #endregion

        #region TODO: curry


        #endregion

        #region new

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

