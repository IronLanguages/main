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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class MemberInitExpression : Expression {
        private readonly NewExpression _newExpression;
        private readonly ReadOnlyCollection<MemberBinding> _bindings;

        internal MemberInitExpression(NewExpression newExpression, ReadOnlyCollection<MemberBinding> bindings) {
            _newExpression = newExpression;
            _bindings = bindings;
        }

        protected override Type GetExpressionType() {
            return _newExpression.Type;
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.MemberInit;
        }

        public NewExpression NewExpression {
            get { return _newExpression; }
        }
        public ReadOnlyCollection<MemberBinding> Bindings {
            get { return _bindings; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitMemberInit(this);
        }

        public override Expression Reduce() {
            return ReduceMemberInit(_newExpression, _bindings, true);
        }

        internal static Expression ReduceMemberInit(Expression objExpression, ReadOnlyCollection<MemberBinding> bindings, bool keepOnStack) {
            var objVar = Expression.Variable(objExpression.Type, null);
            int count = bindings.Count;
            var block = new Expression[count + 2];
            block[0] = Expression.Assign(objVar, objExpression);
            for (int i = 0; i < count; i++) {
                block[i + 1] = ReduceMemberBinding(objVar, bindings[i]);
            }
            block[count + 1] = keepOnStack ? (Expression)objVar : Expression.Empty();
            return Expression.Block(new ReadOnlyCollection<Expression>(block));
        }

        internal static Expression ReduceListInit(Expression listExpression, ReadOnlyCollection<ElementInit> initializers, bool keepOnStack) {
            var listVar = Expression.Variable(listExpression.Type, null);
            int count = initializers.Count;
            var block = new Expression[count + 2];
            block[0] = Expression.Assign(listVar, listExpression);
            for (int i = 0; i < count; i++) {
                ElementInit element = initializers[i];
                block[i + 1] = Expression.Call(listVar, element.AddMethod, element.Arguments);
            }
            block[count + 1] = keepOnStack ? (Expression)listVar : Expression.Empty();
            return Expression.Block(new ReadOnlyCollection<Expression>(block));
        }

        internal static Expression ReduceMemberBinding(ParameterExpression objVar, MemberBinding binding) {
            MemberExpression member = Expression.MakeMemberAccess(objVar, binding.Member);
            switch (binding.BindingType) {
                case MemberBindingType.Assignment:
                    return Expression.Assign(member, ((MemberAssignment)binding).Expression);
                case MemberBindingType.ListBinding:
                    return ReduceListInit(member, ((MemberListBinding)binding).Initializers, false);
                case MemberBindingType.MemberBinding:
                    return ReduceMemberInit(member, ((MemberMemberBinding)binding).Bindings, false);
                default: throw Assert.Unreachable;
            }
        }
    }

    public partial class Expression {
        //CONFORMING
        public static MemberInitExpression MemberInit(NewExpression newExpression, params MemberBinding[] bindings) {
            return MemberInit(newExpression, (IEnumerable<MemberBinding>)bindings);
        }
        //CONFORMING
        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings) {
            ContractUtils.RequiresNotNull(newExpression, "newExpression");
            ContractUtils.RequiresNotNull(bindings, "bindings");
            ReadOnlyCollection<MemberBinding> roBindings = bindings.ToReadOnly();
            ValidateMemberInitArgs(newExpression.Type, roBindings);
            return new MemberInitExpression(newExpression, roBindings);
        }
    }
}
