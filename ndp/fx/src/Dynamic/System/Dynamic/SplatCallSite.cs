/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Diagnostics;
using System.Linq.Expressions;

namespace System.Dynamic {

    internal sealed class SplatCallSite {
        // stored callable. also used as identity of the handler.
        internal readonly object _callable;

        // lambda that contains callsite expr.
        private Func<object, object[], object> _caller = null;

        internal SplatCallSite(object callable) {
            Debug.Assert(callable != null);
            _callable = callable;
        }

        internal object Invoke(object[] args) {
            Debug.Assert(args != null);
            
            if (_caller == null) {
                if (_callable as Delegate != null) {
                    _caller = MakeDelegateCaller();
                } else {
                    _caller = MakeSplatCaller();
                }
            }

            return _caller(_callable, args);
        }

        private static Func<object, object[], object> MakeDelegateCaller() {
            return (object del, object[] args) => ((Delegate)del).DynamicInvoke(args);
        }


        /// <summary>
        /// creates a lambda that represent dynamic operation bound by SplatInvokeBinder
        ///   (target, args) => SplatInvoke(target, args) 
        /// </summary>
        /// <returns></returns>
        private static Func<object, object[], object> MakeSplatCaller() {
            ParameterExpression target = Expression.Parameter(typeof(object), "target");
            ParameterExpression args = Expression.Parameter(typeof(object[]), "args");

            DynamicExpression de = Expression.Dynamic(
                SplatInvokeBinder.Instance,
                typeof(object),
                target,
                args
            );

            var caller = Expression.Lambda<Func<object, object[], object>>(
                de,
                target,
                args
            );

            return caller.Compile();
        }
    }
}
