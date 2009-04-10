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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Binding {
    internal sealed class PythonOverloadResolverFactory : OverloadResolverFactory {
        private readonly PythonBinder/*!*/ _binder;
        private readonly Expression/*!*/ _codeContext;

        public PythonOverloadResolverFactory(PythonBinder/*!*/ binder, Expression/*!*/ codeContext) {
            Assert.NotNull(binder, codeContext);
            _binder = binder;
            _codeContext = codeContext;
        }

        public override DefaultOverloadResolver CreateOverloadResolver(IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType) {
            return new PythonOverloadResolver(_binder, args, signature, callType, _codeContext);
        }
    }

    public sealed class PythonOverloadResolver : DefaultOverloadResolver {
        private readonly Expression _context;

        public Expression ContextExpression {
            get { return _context; }
        }

        // instance method call:
        public PythonOverloadResolver(PythonBinder binder, DynamicMetaObject instance, IList<DynamicMetaObject> args, CallSignature signature,
            Expression codeContext)
            : base(binder, instance, args, signature) {
            Assert.NotNull(codeContext);
            _context = codeContext;
        }

        // method call:
        public PythonOverloadResolver(PythonBinder binder, IList<DynamicMetaObject> args, CallSignature signature, Expression codeContext)
            : this(binder, args, signature, CallTypes.None, codeContext) {
        }

        // method call:
        public PythonOverloadResolver(PythonBinder binder, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType, Expression codeContext)
            : base(binder, args, signature, callType) {
            Assert.NotNull(codeContext);
            _context = codeContext;
        }

        protected override bool BindSpecialParameter(ParameterInfo parameterInfo, List<ArgBuilder> arguments,
            List<ParameterWrapper> parameters, ref int index) {

            // CodeContext is implicitly provided at runtime, the user cannot provide it.
            if (parameterInfo.ParameterType == typeof(CodeContext) && arguments.Count == 0) {
                arguments.Add(new ContextArgBuilder(parameterInfo));
                return true;
            } else if (parameterInfo.ParameterType.IsSubclassOf(typeof(SiteLocalStorage))) {
                arguments.Add(new SiteLocalStorageBuilder(parameterInfo));
                return true;
            }

            return base.BindSpecialParameter(parameterInfo, arguments, parameters, ref index);
        }

        protected override Expression GetByRefArrayExpression(Expression argumentArrayExpression) {
            return Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), argumentArrayExpression);
        }

        protected override bool AllowKeywordArgumentSetting(MethodBase method) {
            return CompilerHelpers.IsConstructor(method) && !method.DeclaringType.IsDefined(typeof(PythonTypeAttribute), true);
        }

        public override Expression ConvertExpression(Expression expr, ParameterInfo info, Type toType) {
            return Binder.ConvertExpression(expr, toType, ConversionResultKind.ExplicitCast, _context);
        }

        public override Expression GetDynamicConversion(Expression value, Type type) {
            return Expression.Dynamic(OldConvertToAction.Make(Binder, type), type, _context, value);
        }

        public override Func<object[], object> ConvertObject(int index, DynamicMetaObject knownType, ParameterInfo info, Type toType) {
            return Binder.ConvertObject(index, knownType, toType, ConversionResultKind.ExplicitCast);
        }
    }
}
