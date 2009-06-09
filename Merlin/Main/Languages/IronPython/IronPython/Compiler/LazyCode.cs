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
using System.Diagnostics;
using System.Reflection;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using System.Linq.Expressions;
using System.Threading;

namespace IronPython.Compiler {
    /// <summary>
    /// Represents code which can be lazily compiled.
    /// 
    /// The code is created in an AST which provides the Expression of T and 
    /// whether or not the code should be interpreted.  For non-pre compiled
    /// scenarios the code will not be compiled until the 1st time it is run.
    /// 
    /// For pre-compiled scenarios the code is IExpressionSerializable and will
    /// turn into a normal pre-compiled method.
    /// </summary>
    sealed class LazyCode<T> : IExpressionSerializable where T : class {
        public Expression<T> Code;
        private T Delegate;
        private bool _shouldInterpret;

        public LazyCode(Expression<T> code, bool shouldInterpret) {
            Code = code;
            _shouldInterpret = shouldInterpret;
        }

        public T EnsureDelegate() {
            if (Delegate == null) {
                lock (this) {
                    if (Delegate == null) {
                        Delegate = Compile();
                        Code = null;
                    }
                }
            }

            return Delegate;
        }

        private T Compile() {
            if (_shouldInterpret) {
                return (T)(object)Microsoft.Scripting.Generation.CompilerHelpers.LightCompile(Code);
            }

            return Code.Compile();
        }

        #region IExpressionSerializable Members

        public System.Linq.Expressions.Expression CreateExpression() {
            return Code;
        }

        #endregion
    }
}
