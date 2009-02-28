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
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Types {
    [PythonType("getset_descriptor")]
    public class ReflectedProperty : ReflectedGetterSetter, ICodeFormattable {
        private readonly PropertyInfo/*!*/ _info;

        public ReflectedProperty(PropertyInfo info, MethodInfo getter, MethodInfo setter, NameType nt)
            : base(new MethodInfo[] { getter }, new MethodInfo[] { setter }, nt) {
            Debug.Assert(info != null);

            _info = info;
        }

        public ReflectedProperty(PropertyInfo info, MethodInfo[] getters, MethodInfo[] setters, NameType nt)
            : base(getters, setters, nt) {
            Debug.Assert(info != null);

            _info = info;
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            if (Setter.Length == 0) {
                return false;
            }

            if (instance == null) {
                foreach (MethodInfo mi in Setter) {
                    if(mi.IsStatic && DeclaringType != owner.UnderlyingSystemType) {
                        return false;
                    } else if (mi.IsFamily || mi.IsFamilyAndAssembly) {
                        throw PythonOps.TypeErrorForProtectedMember(owner.UnderlyingSystemType, _info.Name);
                    }
                }
            } else if (instance != null) {
                foreach (MethodInfo mi in Setter) {
                    if (mi.IsStatic) {
                        return false;
                    }
                }
            }

            return CallSetter(context, PythonContext.GetContext(context).GetGenericCallSiteStorage(), instance, ArrayUtils.EmptyObjects, value);
        }

        internal override Type DeclaringType {
            get { return _info.DeclaringType; }
        }

        public override string __name__ {
            get { return _info.Name; }
        }

        public PropertyInfo Info {
            [PythonHidden]
            get {
                return _info;
            }
        }

        public PythonType PropertyType {
            [PythonHidden]
            get {
                return DynamicHelpers.GetPythonTypeFromType(_info.PropertyType);
            }
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Properties, this);

            value = CallGetter(context, PythonContext.GetContext(context).GetGenericCallSiteStorage(), instance, ArrayUtils.EmptyObjects);
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            __delete__(instance);
            return true;
        }

        internal override Expression/*!*/ MakeGetExpression(PythonBinder/*!*/ binder, Expression/*!*/ codeContext, Expression instance, Expression/*!*/ owner, Expression/*!*/ error) {
            if (Getter.Length != 0 && !Getter[0].IsPublic) {
                // fallback to runtime call
                return base.MakeGetExpression(binder, codeContext, instance, owner, error);
            } else if (NeedToReturnProperty(instance, Getter)) {
                return AstUtils.Constant(this);
            } else if (Getter[0].ContainsGenericParameters) {
                return DefaultBinder.MakeError(
                    binder.MakeContainsGenericParametersError(
                        MemberTracker.FromMemberInfo(_info)
                    )
                );
            }

            Expression res;
            if (instance != null) {
                res = binder.MakeCallExpression(
                    codeContext,
                    Getter[0],
                    instance
                );
            } else {
                res = binder.MakeCallExpression(
                    codeContext,
                    Getter[0]
                );
            }
            Debug.Assert(res != null);
            return res;
        }

        internal override bool IsAlwaysVisible {
            get {
                return NameType == NameType.PythonProperty;
            }
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            return Setter.Length != 0;
        }

        #region Public Python APIs

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        [PythonHidden]
        public object GetValue(CodeContext context, object instance) {
            object value;
            if (TryGetValue(context, instance, DynamicHelpers.GetPythonType(instance), out value)) {
                return value;
            }
            throw new InvalidOperationException("cannot get property");
        }

        /// <summary>
        /// Convenience function for users to call directly
        /// </summary>
        [PythonHidden]
        public void SetValue(CodeContext context, object instance, object value) {
            if (!TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value)) {
                throw new InvalidOperationException("cannot set property");
            }
        }

        public void __set__(CodeContext context, object instance, object value) {
            // TODO: Throw?  currently we have a test that verifies we never throw when this is called directly.
            TrySetValue(context, instance, DynamicHelpers.GetPythonType(instance), value);
        }

        public void __delete__(object instance) {
            if (Setter.Length != 0)
                throw PythonOps.AttributeErrorForReadonlyAttribute(
                    DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(__name__));
            else
                throw PythonOps.AttributeErrorForBuiltinAttributeDeletion(
                    DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name,
                    SymbolTable.StringToId(__name__));
        }

        public string __doc__ {
            get {
                return DocBuilder.DocOneInfo(Info);
            }
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            return string.Format("<property# {0} on {1}>",
                __name__,
                DynamicHelpers.GetPythonTypeFromType(DeclaringType).Name);
        }

        #endregion
    }
}
