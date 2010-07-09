/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
#if FEATURE_CALL_SITE_TRACER
#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;

using Microsoft.Scripting;

namespace IronRuby.Runtime.Calls {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public sealed class CallSiteTracer {
        /// <summary>
        /// Registers a call site tracer associated with the current thread.
        /// Traces is called for each rule created for sites with arguments:
        /// - context meta-object (RubyScope or RubyContext)
        /// - argument meta-objects
        /// - resulting meta-object
        /// - sourceId passed to Transform method
        /// - site offset within the source code
        /// </summary>
        public static void Register(Action<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject, int, int>/*!*/ tracer) {
            ContractUtils.RequiresNotNull(tracer, "tracer");
            TracingRubyCallAction.Tracer = tracer;
        }

        public static MSA.Expression<T>/*!*/ Transform<T>(SourceUnitTree/*!*/ ast, SourceUnit/*!*/ sourceUnit,
            RubyCompilerOptions/*!*/ options, int sourceId) {

            var siteNodes = new Dictionary<MSA.DynamicExpression, SourceSpan>();
            var context = (RubyContext)sourceUnit.LanguageContext;
            context.CallSiteCreated = (expression, callSite) => siteNodes.Add(callSite, expression.Location);

            var generator = new AstGenerator(context, options, sourceUnit.Document, ast.Encoding, false);
            var lambda = ast.Transform<T>(generator);

            return (MSA.Expression<T>)new CallSiteTraceInjector(siteNodes, sourceId).Visit(lambda);
        }

        public sealed class TracingRubyCallAction : RubyCallAction, IExpressionSerializable {
            [ThreadStatic]
            private static int _Id;

            [ThreadStatic]
            private static int _Location;

            [ThreadStatic]
            internal static Action<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject, int, int> Tracer;

            [Emitted]
            public static T EnterCallSite<T>(T result, int id, int location) {
                _Id = id;
                _Location = location;
                return result;
            }

            internal TracingRubyCallAction(string/*!*/ methodName, RubyCallSignature signature)
                : base(null, methodName, signature) {
            }

            public override string/*!*/ ToString() {
                return base.ToString() + "!";
            }

            public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, DynamicMetaObject/*!*/[]/*!*/ args) {
                var result = base.Bind(context, args);
                var tracer = Tracer;
                if (tracer != null) {
                    tracer(context, args, result, _Id, _Location);
                }
                return result;
            }

            MSA.Expression/*!*/ IExpressionSerializable.CreateExpression() {
                throw new NotSupportedException();
            }
        }

        private sealed class CallSiteTraceInjector : MSA.ExpressionVisitor {
            private readonly Dictionary<MSA.DynamicExpression, SourceSpan>/*!*/ _sites;
            private readonly int _sourceId;

            public CallSiteTraceInjector(Dictionary<MSA.DynamicExpression, SourceSpan>/*!*/ sites, int sourceId) {
                _sites = sites;
                _sourceId = sourceId;
            }

            protected override MSA.Expression/*!*/ VisitDynamic(MSA.DynamicExpression/*!*/ node) {
                var callAction = node.Binder as RubyCallAction;
                if (callAction != null) {
                    var args = new MSA.Expression[node.Arguments.Count];

                    for (int i = 0; i < args.Length; i++) {
                        args[i] = node.Arguments[i];
                    }

                    Debug.Assert(args.Length > 0);
                    int last = args.Length - 1;

                    args[last] = typeof(TracingRubyCallAction).GetMethod("EnterCallSite").MakeGenericMethod(args[last].Type).OpCall(
                        args[last],
                        AstUtils.Constant(_sourceId), 
                        AstUtils.Constant(_sites[node].Start.Index)
                    );

                    return Ast.Dynamic(
                        new TracingRubyCallAction(callAction.MethodName, callAction.Signature),
                        node.Type,
                        args
                    );
                } else {
                    return base.VisitDynamic(node);
                }
            }
        }
    }
}
#endif