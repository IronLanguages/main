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
using Microsoft.Scripting;

namespace IronRuby.Runtime {
    public abstract class GlobalVariable {
        protected GlobalVariable() {
        }

        /// <summary>
        /// Whether the variable is listed in the globals_variable result.
        /// For hosts and libraries global variables are good way how to associate arbitrary data with execution context.
        /// They can use this property to hide such data from user.
        /// </summary>
        public virtual bool IsEnumerated { get { return true; } }

        /// <summary>
        /// Implements semantics if the defined? keyword.
        /// </summary>
        public virtual bool IsDefined { get { return true; } }

        public abstract object GetValue(RubyContext/*!*/ context, RubyScope scope);
        public abstract void SetValue(RubyContext/*!*/ context, RubyScope scope, string/*!*/ name, object value);

        internal Exception/*!*/ ReadOnlyError(string/*!*/ name) {
            return RubyExceptions.CreateNameError(String.Format("${0} is a read-only variable", name));
        }

        internal T RequireType<T>(object value, string/*!*/ variableName, string/*!*/ typeName) {
            if (!(value is T)) {
                throw RubyExceptions.CreateTypeError(String.Format("Value of ${0} must be {1}", variableName, typeName));
            }
            return (T)value;
        }
    }
}
