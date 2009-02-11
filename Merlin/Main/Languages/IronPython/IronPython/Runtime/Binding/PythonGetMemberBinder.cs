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
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    class PythonGetMemberBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;
        private readonly GetMemberOptions _options;
        private readonly string _name;

        public PythonGetMemberBinder(BinderState/*!*/ binder, string/*!*/ name) {
            _state = binder;
            _name = name;
        }

        public PythonGetMemberBinder(BinderState/*!*/ binder, string/*!*/ name, bool isNoThrow)
            : this(binder, name) {
            _options = isNoThrow ? GetMemberOptions.IsNoThrow : GetMemberOptions.None;
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
            Debug.Assert(args.Length == 1);
            Debug.Assert(args[0].GetLimitType() == typeof(CodeContext));

            // we don't have CodeContext if an IDO falls back to us when we ask them to produce the Call
            DynamicMetaObject cc = args[0];
            IPythonGetable icc = target as IPythonGetable;

            if (icc != null) {
                // get the member using our interface which also supports CodeContext.
                return icc.GetMember(
                    this,
                    cc.Expression
                );
            } else if (target.Value is IDynamicObject && !(target is MetaPythonObject)) {
                return GetForeignObject(target);
            }
#if !SILVERLIGHT
            else if (ComOps.IsComObject(target.Value)) {
                return GetForeignObject(target);
            }
#endif
            return Fallback(target, cc.Expression);
        }

        private DynamicMetaObject GetForeignObject(DynamicMetaObject self) {
            return new DynamicMetaObject(
                Expression.Dynamic(
                    new CompatibilityGetMember(_state, Name),
                    typeof(object),
                    self.Expression
                ),
                self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, self.GetLimitType()))
            );
        }

        #endregion

        public DynamicMetaObject/*!*/ Fallback(DynamicMetaObject/*!*/ self, Expression/*!*/ codeContext) {
            // Python always provides an extra arg to GetMember to flow the context.
            return FallbackWorker(self, codeContext, Name, _options, this);
        }

        internal static DynamicMetaObject FallbackWorker(DynamicMetaObject/*!*/ self, Expression/*!*/ codeContext, string name, GetMemberOptions options, DynamicMetaObjectBinder action) {
            if (self.NeedsDeferral()) {
                return action.Defer(self);
            }

            bool isNoThrow = ((options & GetMemberOptions.IsNoThrow) != 0) ? true : false;
            Type limitType = self.GetLimitType() ;

            if (limitType == typeof(DynamicNull) || PythonBinder.IsPythonType(limitType)) {
                // look up in the PythonType so that we can 
                // get our custom method names (e.g. string.startswith)            
                PythonType argType = DynamicHelpers.GetPythonTypeFromType(limitType);

                // if the name is defined in the CLS context but not the normal context then
                // we will hide it.                
                if (argType.IsHiddenMember(name)) {
                    DynamicMetaObject baseRes = BinderState.GetBinderState(action).Binder.GetMember(
                        name,
                        self,
                        codeContext,
                        isNoThrow
                    );
                    Expression failure = GetFailureExpression(limitType, name, isNoThrow, action);

                    return BindingHelpers.FilterShowCls(codeContext, action, baseRes, failure);
                }
            }

            if (self.GetLimitType() == typeof(OldInstance)) {
                if ((options & GetMemberOptions.IsNoThrow) != 0) {
                    return new DynamicMetaObject(
                        Ast.Field(
                            null,
                            typeof(OperationFailed).GetField("Value")
                        ),
                        self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(OldInstance)))
                    );
                } else {
                    return new DynamicMetaObject(
                        Ast.Throw(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("AttributeError"),
                                Ast.Constant("{0} instance has no attribute '{1}'"),
                                Ast.NewArrayInit(
                                    typeof(object),
                                    Ast.Constant(((OldInstance)self.Value)._class._name),
                                    Ast.Constant(name)
                                )
                            )
                        ),
                        self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(OldInstance)))
                    );
                }
            }

            var res = BinderState.GetBinderState(action).Binder.GetMember(name, self, codeContext, isNoThrow);

            // Default binder can return something typed to boolean or int.
            // If that happens, we need to apply Python's boxing rules.
            if (res.Expression.Type == typeof(bool) || res.Expression.Type == typeof(int)) {
                res = new DynamicMetaObject(
                    AstUtils.Convert(res.Expression, typeof(object)),
                    res.Restrictions
                );
            }

            return res;
        }

        private static Expression/*!*/ GetFailureExpression(Type/*!*/ limitType, string name, bool isNoThrow, DynamicMetaObjectBinder action) {
            return isNoThrow ?
                Ast.Field(null, typeof(OperationFailed).GetField("Value")) :
                DefaultBinder.MakeError(
                    BinderState.GetBinderState(action).Binder.MakeMissingMemberError(
                        limitType,
                        name
                    )
                );
        }

        public string Name {
            get {
                return _name;
            }
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public bool IsNoThrow {
            get {
                return (_options & GetMemberOptions.IsNoThrow) != 0;
            }
        }

        public override int GetHashCode() {
            return _name.GetHashCode() ^ _state.Binder.GetHashCode() ^ ((int)_options);
        }

        public override bool Equals(object obj) {
            PythonGetMemberBinder ob = obj as PythonGetMemberBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder &&
                ob._options == _options &&
                ob._name == _name;
        }

        public override string ToString() {
            return String.Format("Python GetMember {0} IsNoThrow: {1}", Name, _options);
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeGetAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Ast.Constant(Name),
                Ast.Constant(IsNoThrow)
            );
        }

        #endregion
    }

    class CompatibilityGetMember : GetMemberBinder, IPythonSite {
        private readonly BinderState/*!*/ _state;

        public CompatibilityGetMember(BinderState/*!*/ binder, string/*!*/ name)
            : this(binder, name, false) {
        }

        public CompatibilityGetMember(BinderState/*!*/ binder, string/*!*/ name, bool ignoreCase)
            : base(name, ignoreCase) {
            _state = binder;
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject onBindingError) {
#if !SILVERLIGHT
            DynamicMetaObject com;
            if (System.Dynamic.ComBinder.TryBindGetMember(this, self, out com, true)) {
                return com;
            }
#endif
            return PythonGetMemberBinder.FallbackWorker(self, BinderState.GetCodeContext(this), Name, GetMemberOptions.None, this);
        }

        #region IPythonSite Members

        public BinderState Binder {
            get { return _state; }
        }

        #endregion

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            CompatibilityGetMember ob = obj as CompatibilityGetMember;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder &&
                base.Equals(obj);
        }
    }

    [Flags]
    enum GetMemberOptions {
        None,
        IsNoThrow = 0x01,
        IsCaseInsensitive = 0x02
    }
}
