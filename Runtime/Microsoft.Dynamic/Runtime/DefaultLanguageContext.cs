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
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Actions.Calls;

namespace Microsoft.Scripting.Runtime {
    // TODO: invariant context
    internal class DefaultActionBinder : DefaultBinder {
        private Type[] _extensionTypes;

        public DefaultActionBinder(ScriptDomainManager manager, Type[] extensionTypes)
            : base(manager) {
            this._extensionTypes = extensionTypes;
        }

        public override IList<Type> GetExtensionTypes(Type t) {
            return _extensionTypes;
        }

        // A bunch of conversion code
        public override object Convert(object obj, Type toType) {
            throw new NotImplementedException();
        }

        public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level) {
            // TODO: None -> nullable reference types?
            return toType.IsAssignableFrom(fromType);
        }

        public override Candidate PreferConvert(Type t1, Type t2) {
            throw new NotImplementedException();
        }

        public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, OverloadResolverFactory factory) {
            if (toType.IsAssignableFrom(expr.Type)) {
                return expr;
            }
            return Ast.Utils.Convert(expr, toType);
        }
    }
}
