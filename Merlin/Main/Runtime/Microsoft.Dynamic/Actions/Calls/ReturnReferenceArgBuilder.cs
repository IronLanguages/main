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

namespace Microsoft.Scripting.Actions.Calls {
    using Ast = Expression;

    /// <summary>
    /// Builds a parameter for a reference argument when a StrongBox has not been provided.  The
    /// updated return value is returned as one of the resulting return values.
    /// </summary>
    internal sealed class ReturnReferenceArgBuilder : SimpleArgBuilder {
        private ParameterExpression _tmp;

        public ReturnReferenceArgBuilder(ParameterInfo info, int index)
            : base(info, info.ParameterType.GetElementType(), index, false, false) {
        }

        protected override SimpleArgBuilder Copy(int newIndex) {
            return new ReturnReferenceArgBuilder(ParameterInfo, newIndex);
        }

        public override ArgBuilder Clone(ParameterInfo newType) {
            return new ReturnReferenceArgBuilder(newType, Index);
        }

        internal protected override Expression ToExpression(OverloadResolver resolver, RestrictedArguments args, bool[] hasBeenUsed) {
            if (_tmp == null) {
                _tmp = resolver.GetTemporary(Type, "outParam");
            }

            return Ast.Block(Ast.Assign(_tmp, base.ToExpression(resolver, args, hasBeenUsed)), _tmp);
        }

        internal override Expression ToReturnExpression(OverloadResolver resolver) {
            return _tmp;
        }

        internal override Expression ByRefArgument {
            get { return _tmp; }
        }

        public override int Priority {
            get {
                return 5;
            }
        }
    }
}
