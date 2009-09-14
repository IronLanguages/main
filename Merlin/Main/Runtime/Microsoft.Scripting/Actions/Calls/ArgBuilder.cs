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
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// ArgBuilder provides an argument value used by the MethodBinder.  One ArgBuilder exists for each
    /// physical parameter defined on a method.  
    /// 
    /// Contrast this with ParameterWrapper which represents the logical argument passed to the method.
    /// </summary>
    public abstract class ArgBuilder {
        internal const int AllArguments = -1;

        // can be null, e.g. for ctor return value builder or custom arg builders
        private readonly ParameterInfo _info;

        protected ArgBuilder(ParameterInfo info) {
            _info = info;
        }

        public abstract int Priority {
            get;
        }

        public ParameterInfo ParameterInfo {
            get { return _info; }
        }

        /// <summary>
        /// The number of actual arguments consumed by this builder.
        /// </summary>
        public abstract int ConsumedArgumentCount { get; }

        /// <summary>
        /// Provides the Expression which provides the value to be passed to the argument.
        /// If <c>null</c> is returned the argument is skipped (not passed to the callee).
        /// </summary>
        internal protected abstract Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed);

        internal protected virtual Func<object[], object> ToDelegate(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            return null;
        }

        /// <summary>
        /// Function for accessing an argument from a given index.  When returned from
        /// ToDelegate closed over the value will enable delegate invocations for argument
        /// gets to be optimized away.
        /// 
        /// This function is referenced via reflection and if renamed needs CALLERS UPDATED.
        /// </summary>
        public static object ArgumentRead(object value, object[] args) {
            return args[(int)value];
        }

        /// <summary>
        /// Returns the type required for the argument or null if the ArgBuilder
        /// does not consume a type.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public virtual Type Type {
            get {
                return null;
            }
        }

        /// <summary>
        /// Provides an Expression which will update the provided value after a call to the method.  May
        /// return null if no update is required.
        /// </summary>
        internal virtual Expression UpdateFromReturn(OverloadResolver resolver, RestrictedArguments args) {
            return null;
        }

        /// <summary>
        /// If the argument produces a return value (e.g. a ref or out value) this provides
        /// the additional value to be returned.
        /// </summary>
        internal virtual Expression ToReturnExpression(OverloadResolver resolver) {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// An assignable value that is passed to a byref parameter
        /// After the call it will contain the updated value
        /// </summary>
        internal virtual Expression ByRefArgument {
            get { return null;  }
        }

        public virtual ArgBuilder Clone(ParameterInfo newType) {
            return null;
        }
    }
}
