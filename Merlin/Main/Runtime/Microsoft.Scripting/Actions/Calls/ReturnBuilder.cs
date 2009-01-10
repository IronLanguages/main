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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions.Calls {

    class ReturnBuilder {
        private readonly Type _returnType;

        /// <summary>
        /// Creates a ReturnBuilder
        /// </summary>
        /// <param name="returnType">the type the ReturnBuilder will leave on the stack</param>
        public ReturnBuilder(Type returnType) {
            Debug.Assert(returnType != null);

            this._returnType = returnType;
        }

        internal virtual Expression ToExpression(ParameterBinder parameterBinder, IList<ArgBuilder> args, IList<Expression> parameters, Expression ret) {
            return ret;
        }

        public virtual int CountOutParams {
            get { return 0; }
        }

        public Type ReturnType {
            get {
                return _returnType;
            }
        }

        protected static object ConvertToObject(object ret) {
            if (ret is bool) {
                return ScriptingRuntimeHelpers.BooleanToObject((bool)ret);
            } else if (ret is int) {
                return ScriptingRuntimeHelpers.Int32ToObject((int)ret);
            }
            return ret;
        }
    }
}
