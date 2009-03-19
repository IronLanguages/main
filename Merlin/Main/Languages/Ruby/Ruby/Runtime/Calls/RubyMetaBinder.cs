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
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Math;
using IronRuby.Compiler.Generation;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace IronRuby.Runtime.Calls {
    public abstract class RubyMetaBinder : DynamicMetaObjectBinder {
        public abstract RubyCallSignature Signature { get; }
        protected abstract void Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args);

        protected abstract DynamicMetaObject/*!*/ InteropBind(CallArguments/*!*/ args);

        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, DynamicMetaObject/*!*/[]/*!*/ args) {
            var callArgs = new CallArguments(context, args, Signature);
            
            // TODO: COM interop
            if (IsForeignMetaObject(callArgs.MetaTarget)) {
                return InteropBind(callArgs);
            }

            var metaBuilder = new MetaObjectBuilder();
            Build(metaBuilder, callArgs);
            return metaBuilder.CreateMetaObject(this);
        }

        internal static bool IsForeignMetaObject(DynamicMetaObject/*!*/ metaObject) {
            return metaObject.Value is IDynamicMetaObjectProvider && !(metaObject is RubyMetaObject);
        }
    }
}
