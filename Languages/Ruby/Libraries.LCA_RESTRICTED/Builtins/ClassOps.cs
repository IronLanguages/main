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

using System.Diagnostics;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    [RubyClass("Class", Extends = typeof(RubyClass), Inherits = typeof(RubyModule), Restrictions = ModuleRestrictions.Builtin | ModuleRestrictions.NoUnderlyingType)]
    [UndefineMethod("extend_object")]
    [UndefineMethod("append_features")]
    [UndefineMethod("module_function")]
    public sealed class ClassOps {
        #region initialize, initialize_copy, allocate, new, superclass, inherited

        // factory defined in on RubyClass

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static void Reinitialize(BlockParam body, RubyClass/*!*/ self, [Optional]RubyClass superClass) {
            // Class cannot be subclassed, so this can only be called directly on an already initialized class:
            throw RubyExceptions.CreateTypeError("already initialized class");
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static void InitializeCopy(RubyClass/*!*/ self, [NotNull]RubyClass/*!*/ other) {
            self.InitializeClassCopy(other);
        }
        
        [RubyMethod("allocate")]
        public static RuleGenerator/*!*/ Allocate() {
            return new RuleGenerator(RuleGenerators.InstanceAllocator);
        }

        [RubyMethod("new")]
        public static RuleGenerator/*!*/ New() {
            return new RuleGenerator(RuleGenerators.InstanceConstructor);
        }

        [RubyMethod("superclass")]
        public static RubyClass GetSuperclass(RubyClass/*!*/ self) {
            if (self.IsSingletonClass) {
                RubyClass result = self.ImmediateClass;
                Debug.Assert(result.IsSingletonClass);

                // do not return dummy singletons, also do not create a new singleton (MRI does):
                return result.IsDummySingletonClass ? self : result;
            } else {
                return self.SuperClass;
            }
        }

        [RubyMethod("inherited", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void Inherited(object/*!*/ self, object subclass) {
            // nop
        }

        #endregion

        #region IronRuby: clr_new, clr_ctor

        [RubyMethod("clr_new")]
        public static RuleGenerator/*!*/ ClrNew() {
            return (metaBuilder, args, name) => ((RubyClass)args.Target).BuildClrObjectConstruction(metaBuilder, args, name);
        }

        [RubyMethod("clr_ctor")]
        [RubyMethod("clr_constructor")]
        public static RubyMethod/*!*/ GetClrConstructor(RubyClass/*!*/ self) {
            RubyMemberInfo info;

            if (self.TypeTracker == null) {
                throw RubyExceptions.CreateNotClrTypeError(self);
            }

            if (!self.TryGetClrConstructor(out info)) {
                throw RubyOps.MakeConstructorUndefinedError(self);
            }

            return new RubyMethod(self, info, ".ctor");
        }

        #endregion
    }
}
