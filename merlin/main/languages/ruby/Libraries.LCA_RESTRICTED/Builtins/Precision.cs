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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    [RubyModule("Precision")]
    public class Precision {
        #region prec, prec_i, prec_f

        /// <summary>
        /// Converts self into an instance of klass.
        /// </summary>
        /// <remarks>
        /// By default, prec invokes klass.induced_from(self) and returns its value.
        /// So, if <code>klass.induced_from</code> doesn't return an instance of klass, it will be necessary to reimplement prec.
        /// </remarks>
        [RubyMethod("prec")]
        public static object Prec(object self, [NotNull]RubyClass/*!*/ klass) {
            return LibrarySites.InvokeInducedFrom(klass.Context, klass, self);
        }

        /// <summary>
        /// Returns an Integer converted from self. It is equivalent to <code>prec(Integer)</code>.
        /// </summary>
        [RubyMethod("prec_i")]
        public static object PrecInteger(RubyContext/*!*/ context, object self) {
            return LibrarySites.InvokePrec(context, context.GetClass(typeof(Integer)), self);
        }

        /// <summary>
        /// Returns a Float converted from self. It is equivalent to <code>prec(Float)</code>.
        /// </summary>
        [RubyMethod("prec_f")]
        public static object PrecFloat(RubyContext/*!*/ context, object self) {
            return LibrarySites.InvokePrec(context, context.GetClass(typeof(double)), self);
        }

        #endregion

        #region included

        /// <summary>
        /// When the Precision module is mixed-in to a class, via the Module#include method, this included method is called.
        /// Here it is used to add our default induced_from implementation to the host class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="module">The module being mixed in.</param>
        /// <param name="includedIn">The host class including the module</param>
        [RubyMethod("included", RubyMethodAttributes.PublicSingleton)]
        public static object Included(RubyContext/*!*/ context, RubyModule/*!*/ self, RubyModule/*!*/ includedIn) {
            includedIn.SingletonClass.DefineLibraryMethod("induced_from", (int)RubyMethodAttributes.PublicSingleton, new Func<RubyModule, object, object>(InducedFrom));
            return self;
        }

        private static readonly SymbolId inducedFromSymbol = SymbolTable.StringToId("induced_from");

        private static object InducedFrom(RubyModule/*!*/ rubyClass, object other) {
            throw RubyExceptions.CreateTypeError(String.Format("undefined conversion from {0} into {1}",
                rubyClass.Context.GetClassOf(other).Name, rubyClass.Name));
        }

        #endregion
    }
}
