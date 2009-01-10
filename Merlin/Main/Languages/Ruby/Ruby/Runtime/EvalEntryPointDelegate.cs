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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using System.Diagnostics;
using Microsoft.Scripting.Ast;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime {
    public delegate object EvalEntryPointDelegate(
        RubyScope scope,
        object self,
        RubyModule module,
        Proc blockParameter, 
        RubyMethodInfo method,
        RuntimeFlowControl rfc
    );
}
