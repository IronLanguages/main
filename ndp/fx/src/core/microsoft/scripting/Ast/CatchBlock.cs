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

using System.Dynamic.Utils;

namespace System.Linq.Expressions {

    // TODO: probably should not have Annotations, since it's part of TryExpression
    // They can either go there on on the body
    public sealed class CatchBlock {
        private readonly Type _test;
        private readonly ParameterExpression _var;
        private readonly Expression _body;
        private readonly Expression _filter;

        internal CatchBlock(Type test, ParameterExpression variable, Expression body, Expression filter) {
            _test = test;
            _var = variable;
            _body = body;
            _filter = filter;
        }

        public ParameterExpression Variable {
            get { return _var; }
        }

        public Type Test {
            get { return _test; }
        }

        public Expression Body {
            get { return _body; }
        }

        public Expression Filter {
            get {
                return _filter;
            }
        }
    }

    public partial class Expression {
        public static CatchBlock Catch(Type type, Expression body) {
            return MakeCatchBlock(type, null, body, null);
        }

        public static CatchBlock Catch(ParameterExpression variable, Expression body) {
            ContractUtils.RequiresNotNull(variable, "variable");
            return MakeCatchBlock(variable.Type, variable, body, null);
        }

        public static CatchBlock Catch(Type type, Expression body, Expression filter) {
            return MakeCatchBlock(type, null, body, filter);
        }

        public static CatchBlock Catch(ParameterExpression variable, Expression body, Expression filter) {
            ContractUtils.RequiresNotNull(variable, "variable");
            return MakeCatchBlock(variable.Type, variable, body, filter);
        }

        public static CatchBlock MakeCatchBlock(Type type, ParameterExpression variable, Expression body, Expression filter) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(variable == null || variable.Type.Equals(type), "variable");
            Expression.RequireVariableNotByRef(variable, "variable");
            RequiresCanRead(body, "body");
            if (filter != null) {
                RequiresCanRead(filter, "filter");
                ContractUtils.Requires(filter.Type == typeof(bool), Strings.ArgumentMustBeBoolean);
            }

            return new CatchBlock(type, variable, body, filter);
        }
    }
}
