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
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    /// <summary>
    /// The Action used for Python call sites.  This supports both splatting of position and keyword arguments.
    /// 
    /// When a foreign object is encountered the arguments are expanded into normal position/keyword arguments.
    /// </summary>
    class PythonInvokeBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;
        private readonly CallSignature _signature;

        public PythonInvokeBinder(BinderState/*!*/ binder, CallSignature signature) {
            _state = binder;
            _signature = signature;
        }

        #region MetaAction overrides

        /// <summary>
        /// Python's Invoke is a non-standard action.  Here we first try to bind through a Python
        /// internal interface (IPythonInvokable) which supports CallSigantures.  If that fails
        /// and we have an IDO then we translate to the DLR protocol through a nested dynamic site -
        /// this includes unsplatting any keyword / position arguments.  Finally if it's just a plain
        /// old .NET type we use the default binder which supports CallSignatures.
        /// </summary>
        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            Debug.Assert(args.Length > 0);

            DynamicMetaObject cc = target;
            DynamicMetaObject actualTarget = args[0];
            args = ArrayUtils.RemoveFirst(args);

            Debug.Assert(cc.GetLimitType() == typeof(CodeContext));

            return BindWorker(cc, actualTarget, args);
        }

        private DynamicMetaObject BindWorker(DynamicMetaObject/*!*/ context, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            // we don't have CodeContext if an IDO falls back to us when we ask them to produce the Call
            IPythonInvokable icc = target as IPythonInvokable;

            if (icc != null) {
                // call it and provide the context
                return icc.Invoke(
                    this,
                    context.Expression,
                    target,
                    args
                );
            } else if (target.Value is IDynamicMetaObjectProvider) {
                return InvokeForeignObject(target, args);
            }
#if !SILVERLIGHT
            else if (ComOps.IsComObject(target.Value)) {
                return InvokeForeignObject(target, args);
            }
#endif

            return Fallback(context.Expression, target, args);
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args) {
            IFastInvokable ifi = args[1] as IFastInvokable;
            if (ifi != null) {
                FastBindResult<T> res = ifi.MakeInvokeBinding(site, this, (CodeContext)args[0], ArrayUtils.ShiftLeft(args, 2));
                if (res.Target != null) {
                    if (res.ShouldCache) {
                        base.CacheTarget(res.Target);
                    }

                    return res.Target;
                }
            }

            return base.BindDelegate(site, args);
        }

        public T Optimize<T>(CallSite<T> site, object[] args) where T : class {
            return base.BindDelegate<T>(site, args);
        }

        /// <summary>
        /// Fallback - performs the default binding operation if the object isn't recognized
        /// as being invokable.
        /// </summary>
        internal DynamicMetaObject/*!*/ Fallback(Expression codeContext, DynamicMetaObject target, DynamicMetaObject/*!*/[]/*!*/ args) {
            if (target.NeedsDeferral()) {
                return Defer(args);
            }

            return PythonProtocol.Call(this, target, args) ??
                Binder.Binder.Create(Signature, new ParameterBinderWithCodeContext(Binder.Binder, codeContext), target, args) ??
                Binder.Binder.Call(Signature, new ParameterBinderWithCodeContext(Binder.Binder, codeContext), target, args);
        }

        #endregion

        #region Object Overrides

        public override int GetHashCode() {
            return _signature.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonInvokeBinder ob = obj as PythonInvokeBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder &&
                _signature == ob._signature;
        }

        public override string ToString() {
            return "Python Invoke " + Signature.ToString();
        }

        #endregion

        #region Public API Surface

        /// <summary>
        /// Gets the CallSignature for this invocation which describes how the MetaObject array
        /// is to be mapped.
        /// </summary>
        public CallSignature Signature {
            get {
                return _signature;
            }
        }

        #endregion

        #region Implementation Details

        /// <summary>
        /// Creates a nested dynamic site which uses the unpacked arguments.
        /// </summary>
        protected DynamicMetaObject InvokeForeignObject(DynamicMetaObject target, DynamicMetaObject[] args) {
            // need to unpack any dict / list arguments...
            CallInfo callInfo;
            List<Expression> metaArgs;
            Expression test;
            BindingRestrictions restrictions;
            TranslateArguments(target, args, out callInfo, out metaArgs, out test, out restrictions);

            Debug.Assert(metaArgs.Count > 0);

            return BindingHelpers.AddDynamicTestAndDefer(
                this,
                new DynamicMetaObject(
                    Expression.Dynamic(
                        _state.CompatInvoke(callInfo),
                        typeof(object),
                        metaArgs.ToArray()
                    ),
                    restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target.Expression, target.GetLimitType()))
                ),
                args,
                new ValidationInfo(test)
            );
        }

        /// <summary>
        /// Translates our CallSignature into a DLR Argument list and gives the simple MetaObject's which are extracted
        /// from the tuple or dictionary parameters being splatted.
        /// </summary>
        private void TranslateArguments(DynamicMetaObject target, DynamicMetaObject/*!*/[]/*!*/ args, out CallInfo /*!*/ callInfo, out List<Expression/*!*/>/*!*/ metaArgs, out Expression test, out BindingRestrictions restrictions) {
            Argument[] argInfo = _signature.GetArgumentInfos();

            List<string> namedArgNames = new List<string>();
            metaArgs = new List<Expression>();
            metaArgs.Add(target.Expression);
            Expression splatArgTest = null;
            Expression splatKwArgTest = null;
            restrictions = BindingRestrictions.Empty;

            for (int i = 0; i < argInfo.Length; i++) {
                Argument ai = argInfo[i];

                switch (ai.Kind) {
                    case ArgumentType.Dictionary:
                        IAttributesCollection iac = (IAttributesCollection)args[i].Value;
                        List<string> argNames = new List<string>();

                        foreach (KeyValuePair<object, object> kvp in iac) {
                            string key = (string)kvp.Key;
                            namedArgNames.Add(key);
                            argNames.Add(key);

                            metaArgs.Add(
                                Expression.Call(
                                    AstUtils.Convert(args[i].Expression, typeof(IAttributesCollection)),
                                    typeof(IAttributesCollection).GetMethod("get_Item"),
                                    AstUtils.Constant(SymbolTable.StringToId(key))
                                )
                            );
                        }

                        restrictions = restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(args[i].Expression, args[i].GetLimitType()));
                        splatKwArgTest = Expression.Call(
                            typeof(PythonOps).GetMethod("CheckDictionaryMembers"),
                            AstUtils.Convert(args[i].Expression, typeof(IAttributesCollection)),
                            AstUtils.Constant(argNames.ToArray())
                        );
                        break;
                    case ArgumentType.List:
                        IList<object> splattedArgs = (IList<object>)args[i].Value;
                        splatArgTest = Expression.Equal(
                            Expression.Property(AstUtils.Convert(args[i].Expression, args[i].GetLimitType()), typeof(ICollection<object>).GetProperty("Count")),
                            AstUtils.Constant(splattedArgs.Count)
                        );

                        for (int splattedArg = 0; splattedArg < splattedArgs.Count; splattedArg++) {
                            metaArgs.Add(
                                Expression.Call(
                                    AstUtils.Convert(args[i].Expression, typeof(IList<object>)),
                                    typeof(IList<object>).GetMethod("get_Item"),
                                    AstUtils.Constant(splattedArg)
                                )
                            );
                        }

                        restrictions = restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(args[i].Expression, args[i].GetLimitType()));
                        break;
                    case ArgumentType.Named:
                        namedArgNames.Add(ai.Name);
                        metaArgs.Add(args[i].Expression);
                        break;
                    case ArgumentType.Simple:
                        metaArgs.Add(args[i].Expression);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            
            callInfo = new CallInfo(metaArgs.Count, namedArgNames.ToArray());

            test = splatArgTest;
            if (splatKwArgTest != null) {
                if (test != null) {
                    test = Expression.AndAlso(test, splatKwArgTest);
                } else {
                    test = splatKwArgTest;
                }
            }
        }

        #endregion

        #region IPythonSite Members

        public BinderState Binder {
            get { return _state; }
        }

        #endregion

        #region IExpressionSerializable Members

        public virtual Expression CreateExpression() {
            return Expression.Call(
                typeof(PythonOps).GetMethod("MakeInvokeAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Signature.CreateExpression()
            );
        }

        #endregion
    }
}
