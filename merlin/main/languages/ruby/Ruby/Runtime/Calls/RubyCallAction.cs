/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic.Binders;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;

using IronRuby.Builtins;
using IronRuby.Compiler;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using IronRuby.Compiler.Generation;
   
namespace IronRuby.Runtime.Calls {

    public class RubyCallAction : MetaObjectBinder, IEquatable<RubyCallAction>, IExpressionSerializable {
        private readonly RubyCallSignature _signature;
        private readonly string/*!*/ _methodName;

        public RubyCallSignature Signature {
            get { return _signature; }
        }

        public string/*!*/ MethodName {
            get { return _methodName; }
        }

        protected RubyCallAction(string/*!*/ methodName, RubyCallSignature signature) {
            Assert.NotNull(methodName);
            _methodName = methodName;
            _signature = signature;
        }

        public static RubyCallAction/*!*/ Make(string/*!*/ methodName, int argumentCount) {
            return Make(methodName, RubyCallSignature.Simple(argumentCount));
        }

        [Emitted]
        public static RubyCallAction/*!*/ Make(string/*!*/ methodName, RubyCallSignature signature) {
            return new RubyCallAction(methodName, signature);
        }

        public override object/*!*/ CacheIdentity {
            get { return this; }
        }

        #region Object Overrides, IEquatable

        public override int GetHashCode() {
            return _methodName.GetHashCode() ^ _signature.GetHashCode() ^ GetType().GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as RubyCallAction);
        }

        public override string/*!*/ ToString() {
            return _methodName + _signature.ToString();
        }

        public bool Equals(RubyCallAction other) {
            return other != null && _methodName == other._methodName && _signature.Equals(other._signature) && other.GetType() == GetType();
        }

        #endregion

        #region IExpressionSerializable Members

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Expression.Call(
                typeof(RubyCallAction).GetMethod("Make", new Type[] { typeof(string), typeof(RubyCallSignature) }),
                Expression.Constant(_methodName),
                _signature.CreateExpression()
            );
        }

        #endregion

        public override MetaObject/*!*/ Bind(MetaObject/*!*/ context, MetaObject/*!*/[]/*!*/ args) {
            var mo = new MetaObjectBuilder();
            Bind(mo, _methodName, new CallArguments(context, args, _signature));
            return mo.CreateMetaObject(this, context, args);
        }

        /// <exception cref="MissingMethodException">The resolved method is Kernel#method_missing.</exception>
        internal static void Bind(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args) {
            metaBuilder.AddTargetTypeTest(args);

            RubyMemberInfo method = args.RubyContext.ResolveMethod(args.Target, methodName, true).InvalidateSitesOnOverride();
            if (method != null && RubyModule.IsMethodVisible(method, args.Signature.HasImplicitSelf)) {
                method.BuildCall(metaBuilder, args, methodName);
            } else {
                // insert the method name argument into the args
                object symbol = SymbolTable.StringToId(methodName);
                args.InsertSimple(0, new MetaObject(Ast.Constant(symbol), Restrictions.Empty, symbol));

                BindToMethodMissing(metaBuilder, methodName, args, method != null);
            }
        }

        internal static void BindToMethodMissing(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args, bool privateMethod) {
            // args already contain method name:
            var method = args.RubyContext.ResolveMethod(args.Target, Symbols.MethodMissing, true).InvalidateSitesOnOverride();

            // TODO: better check for builtin method
            if (method == null ||
                method.DeclaringModule == args.RubyContext.KernelModule && method is RubyMethodGroupInfo) {
                
                // throw an exception immediately, do not cache the rule:
                if (privateMethod) {
                    throw RubyExceptions.CreatePrivateMethodCalled(args.RubyContext, args.Target, methodName);
                } else {
                    throw RubyExceptions.CreateMethodMissing(args.RubyContext, args.Target, methodName);
                }
            }

            method.BuildCall(metaBuilder, args, methodName);
        }
    }
}
