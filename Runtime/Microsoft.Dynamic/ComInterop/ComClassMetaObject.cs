/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT // ComObject

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Dynamic;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.ComInterop {

    internal class ComClassMetaObject : DynamicMetaObject {
        internal ComClassMetaObject(Expression expression, ComTypeClassDesc cls)
            : base(expression, BindingRestrictions.Empty, cls) {
        }

        public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
            return new DynamicMetaObject(
                Expression.Call(
                    AstUtils.Convert(Expression, typeof(ComTypeClassDesc)),
                    typeof(ComTypeClassDesc).GetMethod("CreateInstance")
                ),
                BindingRestrictions.Combine(args).Merge(
                    BindingRestrictions.GetTypeRestriction(Expression, typeof(ComTypeClassDesc))
                )
            );
        }
    }
}

#endif
