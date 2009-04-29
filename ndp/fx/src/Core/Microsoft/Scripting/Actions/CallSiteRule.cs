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
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Runtime.CompilerServices {
    /// <summary>
    /// This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.
    /// Represents a runtime binding at a call site.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    internal sealed class CallSiteRule<T> where T : class {

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

            Parameters = new TrueReadOnlyCollection<ParameterExpression>(@params);
            ReturnLabel = Expression.Label(invoke.GetReturnType());
        }

        /// <summary>
        /// The rule set that includes only this rule.
        /// </summary>
        internal readonly T Target;

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

        internal CallSiteRule(Expression binding, T target) {
            _binding = binding;
            Target = target;
        }

        internal CallSiteRule(Expression binding, T target, TemplateData<T> template) {
            _binding = binding;
            _template = template;
            Target = target;
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
        /// Gets the method which is used for templating. If the rule is
        /// not templated then this is a nop (and returns null for the getter).
        /// </summary>
        internal Func<Object[], T> TemplateFunction {
            get {
                if (_template != null) {
                    return _template.TemplateFunction;
                }
                return null;
            }
        }

        internal Set<int> TemplatedConsts {
            get {
                if (_template != null) {
                    return _template.TemplatedConstants;
                }
                return null;
            }
        }


#if MICROSOFT_SCRIPTING_CORE
        /// <summary>
        /// Returns a string representation of the <see cref="CallSiteRule{T}"/>.
        /// </summary>
        public string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter(CultureInfo.CurrentCulture)) {
                    ExpressionWriter.Dump(_binding, writer);
                    return writer.ToString();
                }
            }
        }
#endif
    }

    /// <summary>
    /// Data used for tracking templating information in a rule.
    /// </summary>
    internal class TemplateData<T> where T : class {
        private readonly Func<Object[], T> _templateFunction;
        private readonly Set<int> _templatedConstants;

        internal TemplateData(Func<Object[], T> templateFunction, Set<int> templatedConstants) {
            _templatedConstants = templatedConstants;
            _templateFunction = templateFunction;
        }

        /// <summary>
        /// Function that can produce concrete lambdas when given list of constant values.
        /// </summary>
        internal Func<Object[], T> TemplateFunction {
            get {
                return _templateFunction;
            }
        }

        /// <summary>
        /// Specifies which constants are templated by their positions.
        /// The numbering is assumed as in traversal by ExpressionVisitor.
        /// We use position as constant identity because it is stable 
        /// even in trees that contain node junctions.
        /// </summary>
        internal Set<int> TemplatedConstants {
            get {
                return _templatedConstants;
            }
        }
    }
}
