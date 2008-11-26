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
using System.Collections.ObjectModel;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents a try/catch/finally/fault block.
    /// 
    /// The body is protected by the try block.
    /// The handlers consist of a set of CatchBlocks that can either be catch or filters.
    /// The fault runs if an exception is thrown.
    /// The finally runs regardless of how control exits the body.
    /// Only fault or finally can be supplied
    /// </summary>
    public sealed class TryExpression : Expression {
        private readonly Expression _body;
        private readonly ReadOnlyCollection<CatchBlock> _handlers;
        private readonly Expression _finally;
        private readonly Expression _fault;

        internal TryExpression(Expression body, Expression @finally, Expression fault, ReadOnlyCollection<CatchBlock> handlers) {
            _body = body;
            _handlers = handlers;
            _finally = @finally;
            _fault = fault;
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Try;
        }

        public Expression Body {
            get { return _body; }
        }

        public ReadOnlyCollection<CatchBlock> Handlers {
            get { return _handlers; }
        }

        public Expression Finally {
            get { return _finally; }
        }

        public Expression Fault {
            get { return _fault; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitTry(this);
        }
    }

    // TODO: CatchBlock handlers come last because they're params--is this
    // confusing? The alternative is to put them after the body but remove
    // params. Fortunately, they're strongly typed and not Expressions which
    // mitigates this concern somewhat.
    public partial class Expression {

        // TryFault
        public static TryExpression TryFault(Expression body, Expression fault) {
            return MakeTry(body, null, fault, null);
        }

        // TryFinally
        public static TryExpression TryFinally(Expression body, Expression @finally) {
            return MakeTry(body, @finally, null, null);
        }

        // TryCatch
        public static TryExpression TryCatch(Expression body, params CatchBlock[] handlers) {
            return MakeTry(body, null, null, handlers);
        }

        // TryCatchFinally
        public static TryExpression TryCatchFinally(Expression body, Expression @finally, params CatchBlock[] handlers) {
            return MakeTry(body, @finally, null, handlers);
        }

        // MakeTry: the one factory that creates TryStatement
        public static TryExpression MakeTry(Expression body, Expression @finally, Expression fault, IEnumerable<CatchBlock> handlers) {
            RequiresCanRead(body, "body");

            var @catch = handlers.ToReadOnly();
            ContractUtils.RequiresNotNullItems(@catch, "handlers");

            if (fault != null) {
                if (@finally != null || @catch.Count > 0) {
                    throw Error.FaultCannotHaveCatchOrFinally();
                }
                RequiresCanRead(fault, "fault");
            } else if (@finally != null) {
                RequiresCanRead(@finally, "finally");
            } else if (@catch.Count == 0) {
                throw Error.TryMustHaveCatchFinallyOrFault();
            }

            return new TryExpression(body, @finally, fault, @catch);
        }
    }

}
