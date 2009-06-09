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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public class MemberBinderHelper<TActionKind> : BinderHelper<TActionKind>
        where TActionKind : OldMemberAction {

        private readonly RuleBuilder _rule;              // the rule being produced
        private Type _strongBoxType;                // null or the specific instantiated type of StrongBox
        private object[] _args;                     // the arguments we're creating a rule for 
        private Expression _body = AstUtils.Empty();      // the body of the rule as it's built up
        private object _target;

        public MemberBinderHelper(CodeContext context, TActionKind action, object[] args, RuleBuilder rule)
            : base(context, action) {
            ContractUtils.RequiresNotNull(args, "args");
            if (args.Length == 0) throw new ArgumentException("args must have at least one member");

            _args = args;

            _target = args[0];
            if (CompilerHelpers.IsStrongBox(_target)) {
                _strongBoxType = _target.GetType();
                _target = ((IStrongBox)_target).Value;
            }

            _rule = rule;
        }

        protected object Target {
            get {
                return _target;
            }
        }

        /// <summary> Gets the Expression that represents the instance we're looking up </summary>
        protected Expression Instance {
            get {
                if (_strongBoxType == null) return _rule.Parameters[0];

                return Ast.Field(
                    AstUtils.Convert(_rule.Parameters[0], _strongBoxType),
                    _strongBoxType.GetField("Value")
                );
            }
        }

        protected Type StrongBoxType {
            get {
                return _strongBoxType;
            }
            set {
                _strongBoxType = value;
            }
        }

        protected RuleBuilder Rule {
            get {
                return _rule;
            }
        }

        /// <summary> helper to grab the name of the member we're looking up as a string </summary>
        protected string StringName {
            get { return SymbolTable.IdToString(Action.Name); }
        }

        protected TrackerTypes GetMemberType(MemberGroup members, out Expression error) {
            error = null;
            TrackerTypes memberType = TrackerTypes.All;
            for (int i = 0; i < members.Count; i++) {
                MemberTracker mi = members[i];
                if (mi.MemberType != memberType) {
                    if (memberType != TrackerTypes.All) {
                        error = MakeAmbiguousMatchError(members);
                        return TrackerTypes.All;
                    }
                    memberType = mi.MemberType;
                }
            }
            return memberType;
        }

        protected Expression MakeGenericPropertyExpression() {
            return Ast.New(
                typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) }),
                AstUtils.Constant(StringName)
            );
        }

        protected Expression MakeIncorrectArgumentExpression(int provided, int expected) {
            return Ast.Call(
                typeof(BinderOps).GetMethod("TypeErrorForIncorrectArgumentCount", new Type[] { typeof(string), typeof(int), typeof(int) }),
                AstUtils.Constant(StringName),
                AstUtils.Constant(provided),
                AstUtils.Constant(expected)
            );
        }

        private static Expression MakeAmbiguousMatchError(MemberGroup members) {
            StringBuilder sb = new StringBuilder();
            foreach (MethodTracker mi in members) {
                if (sb.Length != 0) sb.Append(", ");
                sb.Append(mi.MemberType);
                sb.Append(" : ");
                sb.Append(mi.ToString());
            }

            return Ast.New(typeof(AmbiguousMatchException).GetConstructor(new Type[] { typeof(string) }),
                        AstUtils.Constant(sb.ToString()));
        }

        protected void MakeMissingMemberError(Type type) {
            AddToBody(Binder.MakeMissingMemberError(type, StringName).MakeErrorForRule(Rule, Binder));
        }

        protected void MakeReadOnlyMemberError(Type type) {
            AddToBody(Binder.MakeReadOnlyMemberError(Rule, type, StringName));
        }

        protected void MakeUndeletableMemberError(Type type) {
            AddToBody(Binder.MakeUndeletableMemberError(Rule, type, StringName));
        }

        /// <summary>
        /// There is no setter on Body.  Use AddToBody to extend it instead.
        /// </summary>
        protected Expression Body {
            get {
                return _body;
            }
        }

        /// <summary>
        /// Use this method to extend the Body.  It will create BlockStatements as needed.
        /// </summary>
        /// <param name="expression"></param>
        protected void AddToBody(Expression expression) {
            if (_body is DefaultExpression) {
                _body = expression;
            } else {
                _body = Ast.Block(typeof(void), _body, expression);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // TODO: fix
        protected object[] Arguments {
            get {
                return _args;
            }
        }
    }
}
