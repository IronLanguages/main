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
using System.Dynamic.Utils;
using System.Linq.Expressions;

namespace System.Dynamic {
    /// <summary>
    /// Represents a set of binding restrictions on the <see cref="DynamicMetaObject"/>under which the dynamic binding is valid.
    /// </summary>
    public sealed class BindingRestrictions {
        private class Restriction {
            internal enum RestrictionKind {
                Type,
                Instance,
                Custom
            };

            private readonly RestrictionKind _kind;

            // Simplification ... for now just one kind of restriction rather than hierarchy.
            private readonly Expression _expression;
            private readonly Type _type;

            // We hold onto the instance here, but that's okay, because
            // we're binding. Once we generate the instance test however, we 
            // should store the instance as a WeakReference
            private readonly object _instance;

            internal RestrictionKind Kind {
                get { return _kind; }
            }

            internal Expression Expression {
                get { return _expression; }
            }

            internal Type Type {
                get { return _type; }
            }

            internal object Instance {
                get { return _instance; }
            }

            internal Restriction(Expression parameter, Type type) {
                _kind = RestrictionKind.Type;
                _expression = parameter;
                _type = type;
            }

            internal Restriction(Expression parameter, object instance) {
                _kind = RestrictionKind.Instance;
                _expression = parameter;
                _instance = instance;
            }

            internal Restriction(Expression expression) {
                _kind = RestrictionKind.Custom;
                _expression = expression;
            }

            public override bool Equals(object obj) {
                Restriction other = obj as Restriction;
                if (other == null) {
                    return false;
                }

                if (other.Kind != Kind ||
                    other.Expression != Expression) {
                    return false;
                }

                switch (other.Kind) {
                    case RestrictionKind.Instance:
                        return other.Instance == Instance;
                    case RestrictionKind.Type:
                        return other.Type == Type;
                    default:
                        return false;
                }
            }

            public override int GetHashCode() {
                // lots of collisions but we don't hash Restrictions ever
                return (int)Kind ^ Expression.GetHashCode();
            }
        }

        private readonly Restriction[] _restrictions;

        private BindingRestrictions(params Restriction[] restrictions) {
            _restrictions = restrictions;
        }

        private bool IsEmpty {
            get {
                return _restrictions.Length == 0;
            }
        }

        /// <summary>
        /// Merges the set of binding restrictions with the current binding restrictions.
        /// </summary>
        /// <param name="restrictions">The set of restrictions with which to merge the current binding restrictions.</param>
        /// <returns>The new set of binding restrictions.</returns>
        public BindingRestrictions Merge(BindingRestrictions restrictions) {
            if (restrictions.IsEmpty) {
                return this;
            } else if (IsEmpty) {
                return restrictions;
            } else {
                List<Restriction> res = new List<Restriction>(_restrictions.Length + restrictions._restrictions.Length);
                AddRestrictions(_restrictions, res);
                AddRestrictions(restrictions._restrictions, res);

                return new BindingRestrictions(res.ToArray());
            }
        }

        /// <summary>
        /// Adds unique restrictions and doesn't add restrictions which are alerady present
        /// </summary>
        private static void AddRestrictions(Restriction[] list, List<Restriction> res) {
            foreach (Restriction r in list) {
                bool found = false;
                for (int j = 0; j < res.Count; j++) {
                    if (res[j] == r) {
                        found = true;
                    }
                }

                if (!found) {
                    res.Add(r);
                }
            }
        }

        /// <summary>
        /// Represents an empty set of binding restrictions. This field is read only.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BindingRestrictions Empty = new BindingRestrictions();

        /// <summary>
        /// Creates the binding restriction that check the expression for runtime type identity.
        /// </summary>
        /// <param name="expression">The expression to test.</param>
        /// <param name="type">The exact type to test.</param>
        /// <returns>The new binding restrictions.</returns>
        public static BindingRestrictions GetTypeRestriction(Expression expression, Type type) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");

            if (expression.Type == type && type.IsSealed) {
                return BindingRestrictions.Empty;
            }

            return new BindingRestrictions(new Restriction(expression, type));
        }

        /// <summary>
        /// Creates the binding restriction that checks the expression for object instance identity.
        /// </summary>
        /// <param name="expression">The expression to test.</param>
        /// <param name="instance">The exact object instance to test.</param>
        /// <returns>The new binding restrictions.</returns>
        public static BindingRestrictions GetInstanceRestriction(Expression expression, object instance) {
            ContractUtils.RequiresNotNull(expression, "expression");

            return new BindingRestrictions(new Restriction(expression, instance));
        }

        /// <summary>
        /// Creates the binding restriction that checks the expression for arbitrary immutable properties.
        /// </summary>
        /// <param name="expression">The expression expression the restrictions.</param>
        /// <returns>The new binding restrictions.</returns>
        /// <remarks>
        /// By convention, the general restrictions created by this method must only test
        /// immutable object properties.
        /// </remarks>
        public static BindingRestrictions GetExpressionRestriction(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.Requires(expression.Type == typeof(bool), "expression");
            return new BindingRestrictions(new Restriction(expression));
        }

        /// <summary>
        /// Combines binding restrictions from the list of <see cref="DynamicMetaObject"/> instances into one set of restrictions.
        /// </summary>
        /// <param name="contributingObjects">The list of <see cref="DynamicMetaObject"/> instances from which to combine restrictions.</param>
        /// <returns>The new set of binding restrictions.</returns>
        public static BindingRestrictions Combine(IList<DynamicMetaObject> contributingObjects) {
            BindingRestrictions res = BindingRestrictions.Empty;
            if (contributingObjects != null) {
                foreach (DynamicMetaObject mo in contributingObjects) {
                    if (mo != null) {
                        res = res.Merge(mo.Restrictions);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Creates the <see cref="Expression"/> representing the binding restrictions.
        /// </summary>
        /// <returns>The expression tree representing the restrictions.</returns>
        public Expression ToExpression() {
            // We could optimize this better, e.g. common subexpression elimination
            // But for now, it's good enough.
            Expression test = null;
            foreach (Restriction r in _restrictions) {
                Expression one;
                switch (r.Kind) {
                    case Restriction.RestrictionKind.Type:
                        one = CreateTypeRestriction(r.Expression, r.Type);
                        break;
                    case Restriction.RestrictionKind.Instance:
                        one = CreateInstanceRestriction(r.Expression, r.Instance);
                        break;
                    case Restriction.RestrictionKind.Custom:
                        one = r.Expression;
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (one != null) {
                    if (test == null) {
                        test = one;
                    } else {
                        test = Expression.AndAlso(test, one);
                    }
                }
            }

            return test ?? Expression.Constant(true);
        }

        /// <summary>
        /// Creates one type identity test 
        /// </summary>
        private static Expression CreateTypeRestriction(Expression expression, Type rt) {
            // Null is special. True if expression produces null.
            if (rt == typeof(DynamicNull)) {
                return Expression.Equal(expression, Expression.Constant(null));
            }
            return Expression.TypeEqual(expression, rt);
        }

        private static Expression CreateInstanceRestriction(Expression expression, object value) {
            if (value == null) {
                return Expression.Equal(
                    Helpers.Convert(expression, typeof(object)),
                    Expression.Constant(null)
                );
            }

            // TODO: need to add special cases for valuetypes and nullables
            ParameterExpression temp = Expression.Parameter(typeof(object), null);

            Expression init = Expression.Assign(
                temp,                        
                Expression.Property(
                    Expression.Constant(new WeakReference(value)),
                    typeof(WeakReference).GetProperty("Target")
                )
            );
           
            return Expression.Block(
                new ParameterExpression[]{ temp},
                init,
                Expression.AndAlso(
                    //check that WeekReference was not collected.
                    Expression.NotEqual(
                        temp,
                        Expression.Constant(null)
                    ),
                    Expression.Equal(
                        temp,
                        Helpers.Convert(expression, typeof(object))
                    )
                )
            );
        }
    }
}
