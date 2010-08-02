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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Dynamic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Compiler.Generation;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    [RubyClass(Extends = typeof(TypeGroup), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(Enumerable))]
    public static class TypeGroupOps {
        [RubyMethod("of")]
        [RubyMethod("[]")]
        public static RubyModule/*!*/ Of(RubyContext/*!*/ context, TypeGroup/*!*/ self, [NotNullItems]params object/*!*/[]/*!*/ typeArgs) {
            TypeTracker tracker = self.GetTypeForArity(typeArgs.Length);

            if (tracker == null) {
                throw RubyExceptions.CreateArgumentError("Invalid number of type arguments for `{0}'", self.Name);
            }

            Type concreteType;
            if (typeArgs.Length > 0) {
                concreteType = tracker.Type.MakeGenericType(Protocols.ToTypes(context, typeArgs));
            } else {
                concreteType = tracker.Type;
            }

            return context.GetModule(concreteType);
        }

        [RubyMethod("of")]
        [RubyMethod("[]")]
        public static RubyModule/*!*/ Of(RubyContext/*!*/ context, TypeGroup/*!*/ self, int genericArity) {
            TypeTracker tracker = self.GetTypeForArity(genericArity);
            if (tracker == null) {
                throw RubyExceptions.CreateArgumentError("Type group `{0}' does not contain a type of generic arity {1}", self.Name, genericArity);
            }

            return context.GetModule(tracker.Type);
        }

        [RubyMethod("each")]
        public static object EachType(RubyContext/*!*/ context, BlockParam/*!*/ block, TypeGroup/*!*/ self) {
            if (block == null) {
                throw RubyExceptions.NoBlockGiven();
            }

            foreach (Type type in self.Types) {
                RubyModule module = context.GetModule(type);
                object result;
                if (block.Yield(module, out result)) {
                    return result;
                }
            }

            return self;
        }

        [RubyMethod("name")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ GetName(RubyContext/*!*/ context, TypeGroup/*!*/ self) {
            return MutableString.Create(self.Name, context.GetIdentifierEncoding());
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, TypeGroup/*!*/ self) {
            var result = MutableString.CreateMutable(context.GetIdentifierEncoding());
            result.Append("#<TypeGroup: ");

            bool isFirst = true;
            foreach (var entry in self.TypesByArity.ToSortedList((x, y) => x.Key - y.Key)) {
                Type type = entry.Value;

                if (!isFirst) {
                    result.Append(", ");
                } else {
                    isFirst = false;
                }

                result.Append(context.GetTypeName(type, true));
            }
            result.Append('>');

            return result;
        }

        private static Type/*!*/ GetNonGenericType(TypeGroup/*!*/ self) {
            TypeTracker type = self.GetTypeForArity(0);
            if (type == null) {
                throw RubyExceptions.CreateTypeError("type group doesn't include non-generic type");
            }

            return type.Type;
        }

        [Emitted]
        public static RubyClass/*!*/ GetNonGenericClass(RubyContext/*!*/ context, TypeGroup/*!*/ typeGroup) {
            Type type = GetNonGenericType(typeGroup);
            if (type.IsInterface) {
                throw RubyExceptions.CreateTypeError("cannot instantiate an interface");
            }
            return context.GetClass(type);
        }

        private static object New(string/*!*/ methodName, CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ storage,
            TypeGroup/*!*/ self, params object[]/*!*/ args) {

            var cls = GetNonGenericClass(storage.Context, self);
            var site = storage.GetCallSite(methodName,
                new RubyCallSignature(1, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasSplattedArgument)
            );

            return site.Target(site, cls, RubyOps.MakeArrayN(args));
        }

        private static object New(string/*!*/ methodName, CallSiteStorage<Func<CallSite, object, object, object, object>>/*!*/ storage, BlockParam block, 
            TypeGroup/*!*/ self, params object[]/*!*/ args) {

            var cls = GetNonGenericClass(storage.Context, self);
            var site = storage.GetCallSite(methodName,
                new RubyCallSignature(1, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasSplattedArgument | RubyCallFlags.HasBlock)
            );

            return site.Target(site, cls, block != null ? block.Proc : null, RubyOps.MakeArrayN(args));
        }

        // ARGS: N
        [RubyMethod("new")]
        public static object/*!*/ New(CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ storage, 
            TypeGroup/*!*/ self, params object[]/*!*/ args) {
            return New("new", storage, self, args);
        }

        // ARGS: N&
        [RubyMethod("new")]
        public static object New(CallSiteStorage<Func<CallSite, object, object, object, object>>/*!*/ storage, BlockParam block, 
            TypeGroup/*!*/ self, params object[]/*!*/ args) {
            return New("new", storage, block, self, args);
        }

        [RubyMethod("superclass")]
        public static RubyClass GetSuperclass(RubyContext/*!*/ context, TypeGroup/*!*/ self) {
            Type type = GetNonGenericType(self);
            return type.IsInterface ? null : context.GetClass(type).SuperClass;
        }

        // ARGS: N
        [RubyMethod("clr_new")]
        public static object/*!*/ ClrNew(CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ storage,
            TypeGroup/*!*/ self, params object[]/*!*/ args) {
            return New("clr_new", storage, self, args);
        }

        // ARGS: N&
        [RubyMethod("clr_new")]
        public static object ClrNew(CallSiteStorage<Func<CallSite, object, object, object, object>>/*!*/ storage, BlockParam block,
            TypeGroup/*!*/ self, params object[]/*!*/ args) {
            return New("clr_new", storage, block, self, args);
        }

        [RubyMethod("clr_ctor")]
        [RubyMethod("clr_constructor")]
        public static RubyMethod/*!*/ GetClrConstructor(RubyContext/*!*/ context, TypeGroup/*!*/ self) {
            return ClassOps.GetClrConstructor(GetNonGenericClass(context, self));
        }
    }
}
