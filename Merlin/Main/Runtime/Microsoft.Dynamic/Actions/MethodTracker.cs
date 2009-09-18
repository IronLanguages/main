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
using System.Dynamic;
using System.Reflection;

using Microsoft.Contracts;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Expression;
    
    public class MethodTracker : MemberTracker {
        private readonly MethodInfo _method;
        private readonly bool _isStatic;

        internal MethodTracker(MethodInfo method) {
            ContractUtils.RequiresNotNull(method, "method");
            _method = method;
            _isStatic = method.IsStatic;
        }

        internal MethodTracker(MethodInfo method, bool isStatic) {
            ContractUtils.RequiresNotNull(method, "method");
            _method = method;
            _isStatic = isStatic;
        }

        public override Type DeclaringType {
            get { return _method.DeclaringType; }
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Method; }
        }

        public override string Name {
            get { return _method.Name; }
        }

        public MethodInfo Method {
            get {
                return _method;
            }
        }

        public bool IsPublic {
            get {
                return _method.IsPublic;
            }
        }

        public bool IsStatic {
            get {
                return _isStatic;
            }
        }

        [Confined]
        public override string ToString() {
            return _method.ToString();
        }

        public override MemberTracker BindToInstance(DynamicMetaObject instance) {
            if (IsStatic) {
                return this;
            }

            return new BoundMemberTracker(this, instance);
        }

        protected internal override DynamicMetaObject GetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject instance) {
            return binder.ReturnMemberTracker(type, BindToInstance(instance));
        }

        internal override DynamicMetaObject Call(OverloadResolverFactory resolverFactory, ActionBinder binder, params DynamicMetaObject[] arguments) {
            if (Method.IsPublic && Method.DeclaringType.IsVisible) {
                return binder.MakeCallExpression(resolverFactory, Method, arguments);
            }

            //methodInfo.Invoke(obj, object[] params)
            if (Method.IsStatic) {
                return new DynamicMetaObject(
                        Ast.Convert(
                            Ast.Call(
                                AstUtils.Constant(Method),
                                typeof(MethodInfo).GetMethod("Invoke", new Type[] { typeof(object), typeof(object[]) }),
                                AstUtils.Constant(null),
                                AstUtils.NewArrayHelper(typeof(object), ArrayUtils.ConvertAll(arguments, x => x.Expression))
                            ),
                            Method.ReturnType
                        ),
                        BindingRestrictions.Empty
                    )
                ;
            }

            if (arguments.Length == 0) throw Error.NoInstanceForCall();

            return new DynamicMetaObject(
                Ast.Convert(
                    Ast.Call(
                        AstUtils.Constant(Method),
                        typeof(MethodInfo).GetMethod("Invoke", new Type[] { typeof(object), typeof(object[]) }),
                        arguments[0].Expression,
                        AstUtils.NewArrayHelper(typeof(object), ArrayUtils.ConvertAll(ArrayUtils.RemoveFirst(arguments), x => x.Expression))
                    ),
                    Method.ReturnType
                ),
                BindingRestrictions.Empty
            );
        }
    }
}
