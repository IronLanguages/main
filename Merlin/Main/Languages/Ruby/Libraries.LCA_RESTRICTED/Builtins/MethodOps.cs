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
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    [RubyClass("Method", Extends = typeof(RubyMethod))]
    public static class MethodOps {
        #region Public Instance Methods

        [RubyMethod("==")]
        public static bool Equal(RubyMethod/*!*/ self, [NotNull]RubyMethod/*!*/ other) {
            // TODO: method with changed visibility, define_methods, module_functions, aliases:
            return ReferenceEquals(self.Target, other.Target) && ReferenceEquals(self.Info, other.Info);
        }

        [RubyMethod("==")]
        public static bool Equal(RubyMethod/*!*/ self, object other) {
            return false;
        }

        [RubyMethod("arity")]
        public static int GetArity(RubyMethod/*!*/ self) {
            return self.Info.GetArity();            
        }

        [RubyMethod("clone")]
        public static RubyMethod/*!*/ Clone(RubyMethod/*!*/ self) {
            return new RubyMethod(self.Target, self.Info, self.Name);
        }

        [RubyMethod("[]")]
        [RubyMethod("call")]
        public static RuleGenerator/*!*/ Call() {
            return new RuleGenerator(RuleGenerators.MethodCall);
        }

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(RubyContext/*!*/ context, RubyMethod/*!*/ self) {
            return UnboundMethod.ToS(context, self.Name, self.Info.DeclaringModule, self.GetTargetClass(), "Method");
        }

        [RubyMethod("to_proc")]
        public static Proc/*!*/ ToProc(RubyContext/*!*/ context, RubyMethod/*!*/ self) {
            RubyMethodInfo mi = self.Info as RubyMethodInfo;
            if (mi != null) {
                return Proc.Create(context, mi.Method, self.Target, mi.GetArity());
            }
            // TODO: figure out what the semantics should be for a set of CLR methods returned ...
            throw new NotImplementedException();
        }

        [RubyMethod("unbind")]
        public static UnboundMethod/*!*/ Unbind(RubyMethod/*!*/ self) {
            return new UnboundMethod(self.GetTargetClass(), self.Name, self.Info);
        }

        internal static RubyMemberInfo/*!*/ BindGenericParameters(RubyContext/*!*/ context, RubyMemberInfo/*!*/ info, string/*!*/ name, object[]/*!*/ typeArgs) {
            RubyMemberInfo result = info.TryBindGenericParameters(Protocols.ToTypes(context, typeArgs));
            if (result == null) {
                throw RubyExceptions.CreateArgumentError(String.Format("wrong number of generic arguments for `{0}'", name));
            }
            return result;
        }

        internal static RubyMemberInfo/*!*/ GetOverloads(RubyContext/*!*/ context, RubyMemberInfo/*!*/ info, string/*!*/ name, object[]/*!*/ typeArgs) {
            RubyMemberInfo result = info.TrySelectOverload(Protocols.ToTypes(context, typeArgs));
            if (result == null) {
                throw RubyExceptions.CreateArgumentError(String.Format("no overload of `{0}' matches given parameter types", name));
            }
            return result;
        }

        [RubyMethod("of")]
        public static RubyMethod/*!*/ BindGenericParameters(RubyContext/*!*/ context, RubyMethod/*!*/ self, [NotNull]params object[]/*!*/ typeArgs) {
            return new RubyMethod(self.Target, BindGenericParameters(context, self.Info, self.Name, typeArgs), self.Name);
        }

        [RubyMethod("overloads")]
        public static RubyMethod/*!*/ GetOverloads(RubyContext/*!*/ context, RubyMethod/*!*/ self, [NotNull]params object[]/*!*/ parameterTypes) {
            return new RubyMethod(self.Target, GetOverloads(context, self.Info, self.Name, parameterTypes), self.Name);
        }

        [RubyMethod("clr_members")]
        public static RubyArray/*!*/ GetClrMembers(RubyMethod/*!*/ self) {
            return new RubyArray(self.Info.GetMembers());
        }

        #endregion
    }
}
