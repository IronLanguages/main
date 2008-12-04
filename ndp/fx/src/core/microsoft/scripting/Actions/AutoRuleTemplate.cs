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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Dynamic.Utils;

namespace System.Dynamic {
    /// <summary>
    /// Handles auto-templating of rules.  There are three important actions this performs:
    ///     1. Detects if templating is possible between two rules
    ///     2. Re-writes a non-templated rule into templated form
    ///     3. Extracts the constants from a non-templated rule which is compatible with a 
    ///         templated rule so that they can be used by the existing generated code.
    ///         
    /// Auto-templating is currently only used for serially monomorphic call sites where we
    /// can easily avoid code gen.  It is not used for polymorphic call sites although such
    /// a feature could be enabled in the future.
    /// </summary>
    internal static class AutoRuleTemplate {
        /// <summary>
        /// The entry point into auto-rule tempating.  This consumes the monomorphic rule which is currently
        /// stored in the cache as well as the rule that was just produced by the binder.  
        /// </summary>
        /// <param name="from">The original rule that is currently stored in the cache.  This rule may
        /// or may not be a templated rule.</param>
        /// <param name="to">The new rule produced by a binder.</param>
        internal static CallSiteRule<T> CopyOrCreateTemplatedRule<T>(CallSiteRule<T> from, CallSiteRule<T> to) where T : class {
            List<ConstantExpression> newConstants;   // the constants which need to be replaced in our new rule.
            bool tooSpecific;

            from = FindCompatibleRuleForTemplate<T>(from, to, out newConstants, out tooSpecific);
            if (from == null) {
                // trees are incompatible
                return to;
            }

            // We have 2 rules which are compatible.  We should create a new template or 
            // re-use the existing one.
            object[] templateArgs = GetConstantValues(newConstants);

            Expression newBody = new TemplateRuleRewriter(newConstants).Visit(to.Binding);

            if (from.Template == null || tooSpecific) {
                // create a new one - either we are going from a non-templated rule to using a templated rule, 
                // or we are further generalizing an existing rule.  We need to re-write the incoming tree 
                // to be templated over the necessary constants and return the new rule bound to the template.

                return new CallSiteRule<T>(newBody, null, new TemplateData<T>());
            }

            // we have compatible templated rules, we can just swap out the constant pool and 
            // be on our merry way.

            // get the old target
            Delegate target = (Delegate)(object)from.RuleSet.GetTarget();

            // create a new delegate closed over our new argument array
            T dlg;
            DynamicMethod templateMethod = from.TemplateMethod as DynamicMethod;
            if (templateMethod != null) {
                dlg = (T)(object)templateMethod.CreateDelegate(typeof(T), CloneData(target.Target, templateArgs));
            } else {
                dlg = (T)(object)Delegate.CreateDelegate(typeof(T), CloneData(target.Target, templateArgs), target.Method);
            }

            // create a new rule which is bound to the new delegate w/ the expression tree from the old code.            
            return new CallSiteRule<T>(newBody, dlg, from.Template);
        }

        private static CallSiteRule<T> FindCompatibleRuleForTemplate<T>(CallSiteRule<T> from, CallSiteRule<T> to, out List<ConstantExpression> newConstants, out bool tooSpecific) where T : class {
            // no templates exist for this rule, just compare the raw trees...
            if (TreeComparer.Compare(to.Binding, from.Binding, out newConstants, out tooSpecific)) {
                // the rules are not compatbile, don't add template values...
                return from;
            }

            return null;
        }

        /// <summary>
        /// Clones the delegate target to create new delegate around it.
        /// The delegates created by the compiler are closed over the instance of Closure class.
        /// </summary>
        private static object CloneData(object data, params object[] newData) {
            Debug.Assert(data != null);

            Closure closure = data as Closure;
            if (closure != null) {
                Debug.Assert(closure.Locals == null);
                return new Closure(CopyArray(newData, closure.Constants), null);
            }

            throw Error.BadDelegateData();
        }

        private static object[] CopyArray(object[] newData, object[] oldData) {
            int copiedCount = 0;

            object[] res = new object[oldData.Length];
            for (int i = 0; i < oldData.Length; i++) {
                ITemplatedValue itv = oldData[i] as ITemplatedValue;
                if (itv == null) {
                    res[i] = oldData[i];
                    continue;
                }
                copiedCount++;

                res[i] = itv.CopyWithNewValue(newData[itv.Index]);
            }

            Debug.Assert(copiedCount == newData.Length);
            return res;
        }

        private static object[] GetConstantValues(List<ConstantExpression> newConstants) {
            object[] res = new object[newConstants.Count];
            int index = 0;
            foreach (ConstantExpression ce in newConstants) {
                res[index++] = ce.Value;
            }
            return res;
        }

        internal class TemplateRuleRewriter : ExpressionVisitor {
            private readonly List<ConstantExpression> _constants;
            private static CacheDict<Type, Func<object, int, object>> _templateCtors = new CacheDict<Type, Func<object, int, object>>(20);

            public TemplateRuleRewriter(List<ConstantExpression> constants) {
                _constants = constants;
            }

            protected internal override Expression VisitConstant(ConstantExpression node) {
                int index = _constants.IndexOf(node);
                if (index != -1) {
                    // clear the constant from the list...  This is incase the rule contains the
                    // same ConstantExpression instance multiple times and we're replacing the 
                    // multiple entries.  In that case we want each constant duplicated value
                    // to line up with a single index.
                    _constants[index] = null;

                    // this is a constant we want to re-write, replace w/ a templated constant
                    object value = node.Value;

                    Type elementType = TypeUtils.GetConstantType(node.Type);

                    Func<object, int, object> ctor;
                    lock (_templateCtors) {
                        if (!_templateCtors.TryGetValue(elementType, out ctor)) {
                            MethodInfo genMethod = typeof(TemplatedValue<>).MakeGenericType(new Type[] { elementType }).GetMethod("Make", BindingFlags.NonPublic | BindingFlags.Static);
                            _templateCtors[elementType] = ctor = (Func<object, int, object>)genMethod.CreateDelegate(typeof(Func<object, int, object>)); ;
                        }
                    }
                    
                    object constVal = ctor(value, index);                    

                    return Expression.Property(Expression.Constant(constVal), ctor.Method.ReturnType.GetProperty("Value"));
                }

                return base.VisitConstant(node);
            }
        }
    }
}

