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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    /// <summary>
    /// Methods on Singleton(main).
    /// </summary>
    [RubyModule(RubyClass.MainSingletonName)]
    public static class MainSingletonOps {
        #region Private Instance Methods

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static object/*!*/ Initialize(object/*!*/ self) {
            // TODO:
            throw new NotImplementedException("TODO");
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("to_s", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ ToS(object/*!*/ self) {
            return MutableString.CreateAscii("main");
        }

        // thread-safe:
        [RubyMethod("public", RubyMethodAttributes.PublicInstance)]
        public static RubyModule/*!*/ SetPublicVisibility(RubyScope/*!*/ scope, object/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {

            return SetVisibility(scope, self, methodNames, RubyMethodAttributes.PublicInstance);
        }

        // thread-safe:
        [RubyMethod("private", RubyMethodAttributes.PublicInstance)]
        public static RubyModule/*!*/ SetPrivateVisibility(RubyScope/*!*/ scope, object/*!*/ self,
            [DefaultProtocol, NotNullItems]params string/*!*/[]/*!*/ methodNames) {

            return SetVisibility(scope, self, methodNames, RubyMethodAttributes.PrivateInstance);
        }

        private static RubyModule/*!*/ SetVisibility(RubyScope/*!*/ scope, object/*!*/ self, string/*!*/[]/*!*/ methodNames, RubyMethodAttributes attributes) {
            Assert.NotNull(scope, self, methodNames);
            RubyModule module;

            // MRI: Method is searched in the class of self (Object), not in the main singleton class.
            // IronRuby specific: If we are in a top-level scope with redirected method lookup module we use that module (hosted scopes).
            var topScope = scope.Top.GlobalScope.TopLocalScope;
            if (scope == topScope && topScope.MethodLookupModule != null) {
                module = topScope.MethodLookupModule;
            } else {
                module = scope.RubyContext.GetClassOf(self);
            }
            ModuleOps.SetMethodAttributes(scope, module, methodNames, attributes);
            return module;
        }

        // thread-safe:
        [RubyMethod("include", RubyMethodAttributes.PublicInstance)]
        public static RubyClass/*!*/ Include(RubyContext/*!*/ context, object/*!*/ self, params RubyModule[]/*!*/ modules) {
            RubyClass result = context.GetClassOf(self);
            result.IncludeModules(modules);
            return result;
        }

        #endregion
    }

    /// <summary>
    /// Methods on Singleton(class), Singleton(Singleton(object)).
    /// </summary>
    [RubyModule(RubyClass.ClassSingletonName)]
    public static class ClassSingletonOps {
        #region Private Instance Methods

        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static object/*!*/ Initialize(object/*!*/ self) {
            // TODO:
            throw new NotImplementedException("TODO");
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static object InitializeCopy(object/*!*/ self, object other) {
            // TODO:
            throw new NotImplementedException("TODO");
        }

        [RubyMethod("inherited", RubyMethodAttributes.PrivateInstance)]
        public static void Inherited(object/*!*/ self, [NotNull]RubyClass/*!*/ subclass) {
            // TODO:
            throw new NotImplementedException("TODO");
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("allocate", RubyMethodAttributes.PublicInstance)]
        public static void Allocate(RubyClass/*!*/ self) {
            throw RubyExceptions.CreateTypeError("can't create instance of virtual class");
        }

        [RubyMethod("superclass", RubyMethodAttributes.PublicInstance)]
        public static RubyClass/*!*/ GetSuperClass(RubyClass/*!*/ self) {
            RubyClass result = self.SingletonClass;
            Debug.Assert(result != null && result.IsSingletonClass);

            // do not return dummy singletons, also do not create a new singleton (MRI does):
            return result.IsDummySingletonClass ? self : result;
        }

        [RubyMethod("new", RubyMethodAttributes.PublicInstance)]
        public static void New(RubyClass/*!*/ self) {
            Allocate(self);
        }

        #endregion
    }

    /// <summary>
    /// Methods on Singleton(Singleton(class)), Singleton(Singleton(Singleton(object))).
    /// </summary>
    [RubyModule(RubyClass.ClassSingletonSingletonName), Includes(typeof(ClassSingletonOps), Copy = true)]
    public static class ClassSingletonSingletonOps {

        #region Public Instance Methods

        [RubyMethod("constants", RubyMethodAttributes.PublicInstance)]
        public static RubyArray/*!*/ GetConstants(object/*!*/ self) {
            throw new NotImplementedException("TODO");
        }

        [RubyMethod("nesting", RubyMethodAttributes.PublicInstance)]
        public static RubyModule/*!*/ GetNesting(object/*!*/ self) {
            throw new NotImplementedException("TODO");
        }

        #endregion
    }
}
