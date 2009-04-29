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
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class TryStatementBuilder {
        private readonly List<CatchBlock> _catchBlocks = new List<CatchBlock>();
        private Expression _try;
        private Expression _finally, _fault;
        private bool _enableJumpsFromFinally;

        internal TryStatementBuilder(Expression body) {
            _try = body;
        }

        public TryStatementBuilder Catch(Type type, Expression body) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(body, "body");
            if (_finally != null) {
                throw Error.FinallyAlreadyDefined();
            }

            _catchBlocks.Add(Expression.Catch(type, body));
            return this;
        }

        public TryStatementBuilder Catch(Type type, params Expression[] body) {
            return Catch(type, Utils.BlockVoid(body));
        }

        public TryStatementBuilder Catch(ParameterExpression holder, params Expression[] body) {
            return Catch(holder, Utils.Block(body));
        }

        public TryStatementBuilder Catch(ParameterExpression holder, Expression body) {
            ContractUtils.RequiresNotNull(holder, "holder");
            ContractUtils.RequiresNotNull(body, "body");

            if (_finally != null) {
                throw Error.FinallyAlreadyDefined();
            }

            _catchBlocks.Add(Expression.Catch(holder, body));
            return this;
        }

        public TryStatementBuilder Filter(Type type, Expression condition, params Expression[] body) {
            return Filter(type, condition, Utils.Block(body));
        }

        public TryStatementBuilder Filter(Type type, Expression condition, Expression body) {
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(body, "body");

            _catchBlocks.Add(Expression.Catch(type, body, condition));
            return this;
        }

        public TryStatementBuilder Filter(ParameterExpression holder, Expression condition, params Expression[] body) {
            return Filter(holder, condition, Utils.Block(body));
        }

        public TryStatementBuilder Filter(ParameterExpression holder, Expression condition, Expression body) {
            ContractUtils.RequiresNotNull(holder, "holder");
            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(body, "body");

            _catchBlocks.Add(Expression.Catch(holder, body, condition));
            return this;
        }

        public TryStatementBuilder Finally(params Expression[] body) {
            return Finally(Utils.BlockVoid(body));
        }

        public TryStatementBuilder Finally(Expression body) {
            ContractUtils.RequiresNotNull(body, "body");
            if (_finally != null) {
                throw Error.FinallyAlreadyDefined();
            } else if (_fault != null) {
                throw Error.CannotHaveFaultAndFinally();
            }

            _finally = body;
            return this;
        }

        public TryStatementBuilder FinallyWithJumps(params Expression[] body) {
            _enableJumpsFromFinally = true;
            return Finally(body);
        }

        public TryStatementBuilder FinallyWithJumps(Expression body) {
            _enableJumpsFromFinally = true;
            return Finally(body);
        }

        public TryStatementBuilder Fault(params Expression[] body) {
            ContractUtils.RequiresNotNullItems(body, "body");

            if (_finally != null) {
                throw Error.CannotHaveFaultAndFinally();
            } else if (_fault != null) {
                throw Error.FaultAlreadyDefined();
            }

            if (body.Length == 1) {
                _fault = body[0];
            } else {
                _fault = Utils.BlockVoid(body);
            }

            return this;
        }

        public static implicit operator Expression(TryStatementBuilder builder) {
            ContractUtils.RequiresNotNull(builder, "builder");
            return builder.ToExpression();
        }

        public Expression ToExpression() {

            //
            // We can't emit a real filter or fault because they don't
            // work in DynamicMethods. Instead we do a simple transformation:
            //   fault -> catch (Exception) { ...; rethrow }
            //   filter -> catch (ExceptionType) { if (!filter) rethrow; ... }
            //
            // Note that the filter transformation is not quite equivalent to
            // real CLR semantics, but it's what IronPython and IronRuby
            // expect. If we get CLR support we'll switch over to real filter
            // and fault blocks.
            //

            var handlers = new List<CatchBlock>(_catchBlocks);
            for (int i = 0, n = handlers.Count; i < n ; i++) {
                CatchBlock handler = handlers[i];
                if (handler.Filter != null) {
                    handlers[i] = Expression.MakeCatchBlock(
                        handler.Test,
                        handler.Variable,
                        Expression.Condition(
                            handler.Filter,
                            handler.Body,
                            Expression.Rethrow(handler.Body.Type)
                        ),
                        null
                    );
                }
            }

            if (_fault != null) {
                ContractUtils.Requires(handlers.Count == 0, "fault cannot be used with catch or finally clauses");
                handlers.Add(Expression.Catch(typeof(Exception), Expression.Block(_fault, Expression.Rethrow(_try.Type))));
            }

            var result = Expression.MakeTry(null, _try, _finally, null, handlers);
            if (_enableJumpsFromFinally) {
                return Utils.FinallyFlowControl(result);
            }
            return result;
        }
    }

    public partial class Utils {
        public static TryStatementBuilder Try(params Expression[] body) {
            ContractUtils.RequiresNotNull(body, "body");
            return new TryStatementBuilder(Utils.Block(body));
        }

        public static TryStatementBuilder Try(Expression body) {
            ContractUtils.RequiresNotNull(body, "body");
            return new TryStatementBuilder(body);
        }
    }
}
