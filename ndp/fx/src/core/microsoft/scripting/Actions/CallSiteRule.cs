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

using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using System.Dynamic.Utils;

namespace System.Runtime.CompilerServices {
    /// <summary>
    /// This type is only used by CallSite internally. Do not use
    /// </summary>
    public sealed class CallSiteRule<T> where T : class {

        internal static readonly ReadOnlyCollection<ParameterExpression> Parameters;
        internal static readonly LabelTarget ReturnLabel;

        // cctor will be lazily executed when a given T is first referenced
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static CallSiteRule() {
            Type target = typeof(T);
            if (!typeof(Delegate).IsAssignableFrom(target)) {
                throw Error.TypeParameterIsNotDelegate(target);
            }

            MethodInfo invoke = target.GetMethod("Invoke");
            ParameterInfo[] pis = invoke.GetParametersCached();
            if (pis[0].ParameterType != typeof(CallSite)) {
                throw Error.FirstArgumentMustBeCallSite();
            }

            var @params = new ParameterExpression[pis.Length - 1];
            for (int i = 0; i < @params.Length; i++) {
                @params[i] = Expression.Parameter(pis[i + 1].ParameterType, "$arg" + i);
            }

            Parameters = new ReadOnlyCollection<ParameterExpression>(@params);
            ReturnLabel = Expression.Label(invoke.GetReturnType());
        }

        /// <summary>
        /// The rule set that includes only this rule.
        /// </summary>
        internal readonly SmallRuleSet<T> RuleSet;

        /// <summary>
        /// The binding expression tree
        /// </summary>
        private readonly Expression _binding;

        /// <summary>
        /// Template data - null for methods which aren't templated.  Non-null for methods which
        /// have been templated.  The same template data is shared across all templated rules with
        /// the same target method.
        /// </summary>
        private readonly TemplateData<T> _template;

        internal CallSiteRule(Expression binding) {
            _binding = binding;
            RuleSet = new SmallRuleSet<T>(new[] { this });
        }

        internal CallSiteRule(Expression binding, T target, TemplateData<T> template) {
            _binding = binding;
            RuleSet = new SmallRuleSet<T>(target, new CallSiteRule<T>[] { this });
            _template = template;
        }

        /// <summary>
        /// The expression representing the bound operation
        /// </summary>
        internal Expression Binding {
            get { return _binding; }
        }

        internal TemplateData<T> Template {
            get {
                return _template;
            }
        }

        /// <summary>
        /// Gets or sets the method which is used for templating. If the rule is
        /// not templated then this is a nop (and returns null for the getter).
        /// 
        /// The method is tracked here independently from the delegate for the
        /// common case of the method being a DynamicMethod.  In order to re-bind
        /// the existing DynamicMethod to a new set of templated parameters we need
        /// to have the original method.
        /// </summary>
        internal MethodInfo TemplateMethod {
            get {
                if (_template != null) {
                    return _template.Method;
                }
                return null;
            }
            set {
                if (_template != null) {
                    _template.Method = value;
                }
            }
        }

#if MICROSOFT_SCRIPTING_CORE
        public string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter(CultureInfo.CurrentCulture)) {
                    ExpressionWriter.Dump(_binding, "Rule", writer);
                    return writer.ToString();
                }
            }
        }
#endif
    }

    /// <summary>
    /// Data used for tracking templating information in a rule.
    /// 
    /// Currently we just track the method so we can retarget to
    /// new constant pools.
    /// </summary>
    internal class TemplateData<T> where T : class {
        internal MethodInfo Method;
    }
}
