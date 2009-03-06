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
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Compiler;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = System.Linq.Expressions.Expression;

    public abstract class RubyMetaObject : DynamicMetaObject {
        public abstract RubyContext/*!*/ Context { get; }
        public abstract Expression/*!*/ ContextExpression { get; }

        internal RubyMetaObject(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, object/*!*/ value)
            : base(expression, restrictions, value) {
            ContractUtils.RequiresNotNull(value, "value");
        }
    }

    public abstract class RubyMetaObject<T> : RubyMetaObject {
        protected abstract MethodInfo/*!*/ ContextConverter { get; }

        public new T/*!*/ Value {
            get { return (T)base.Value; }
        }

        public sealed override Expression/*!*/ ContextExpression {
            get { return ContextConverter.OpCall(AstUtils.Convert(Expression, typeof(T))); }
        }

        public RubyMetaObject(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, T/*!*/ value)
            : base(expression, restrictions, value) {
        }
    }
}
