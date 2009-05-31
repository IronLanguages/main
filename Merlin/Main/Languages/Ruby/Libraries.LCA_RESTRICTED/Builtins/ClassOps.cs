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
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Builtins {

    [RubyClass("Class", Extends = typeof(RubyClass), Inherits = typeof(RubyModule))]
    [UndefineMethod("extend_object")]
    [UndefineMethod("append_features")]
    [UndefineMethod("module_function")]
    public sealed class ClassOps {

        #region Construction

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

        
        #endregion

        #region Private Instance Methods

        [RubyMethod("inherited", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void Inherited(object/*!*/ self, object subclass) {
            // nop
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("allocate")]
        public static RuleGenerator/*!*/ GetInstanceAllocator() {
            return new RuleGenerator(RuleGenerators.InstanceAllocator);
        }

        [RubyMethod("new")]
        public static RuleGenerator/*!*/ GetInstanceConstructor() {
            return new RuleGenerator(RuleGenerators.InstanceConstructor);
        }

        [RubyMethod("superclass")]
        public static RubyClass GetSuperclass(RubyClass/*!*/ self) {
            return self.SuperClass;
        }

        #endregion
    }
}
