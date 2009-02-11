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
using System.Dynamic.Binders;
using System.Linq.Expressions;

namespace IronPython.Runtime.Binding {
    /// <summary>
    /// Interface used to mark objects as being invokable from Python.  These objects support
    /// calling with splatted positional and keyword arguments.
    /// </summary>
    interface IPythonInvokable {
        MetaObject/*!*/ Invoke(InvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, MetaObject/*!*/[]/*!*/ args);
    }
}
