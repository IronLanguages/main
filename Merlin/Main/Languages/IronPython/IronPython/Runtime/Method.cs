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
using System.Runtime.CompilerServices;
using System.Dynamic;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {

    [PythonType("instancemethod")]
    public sealed partial class Method : PythonTypeSlot, IWeakReferenceable, IMembersList, IDynamicMetaObjectProvider, ICodeFormattable {
        private readonly object _func;
        private readonly object _inst;
        private readonly object _declaringClass;
        private WeakRefTracker _weakref;

        public Method(object function, object instance, object @class) {
            _func = function;
            _inst = instance;
            _declaringClass = @class;
        }

        public Method(object function, object instance) {
            if (instance == null) {
                throw PythonOps.TypeError("unbound methods must have a class provided");
            }

            _func = function;
            _inst = instance;
        }

        public string __name__ {
            get { return (string)PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Name); }
        }

        public string __doc__ {
            get {
                return PythonOps.GetBoundAttr(DefaultContext.Default, _func, Symbols.Doc) as string;
            }
        }

        public object im_func {
            get {
                return _func;
            }
        }

        public object __func__ {
            get {
                return _func;
            }
        }

        public object im_self {
            get {
                return _inst;
            }
        }

        public object __self__ {
            get {
                return _inst;
            }
        }

        public object im_class {
            get {
                // we could have an OldClass (or any other object) here if the user called the ctor directly
                return PythonOps.ToPythonType(_declaringClass as PythonType) ?? _declaringClass;
            }
        }

        [SpecialName]
        public object Call(CodeContext/*!*/ context, params object[] args) {
            return PythonContext.GetContext(context).Call(this, args);
        }

        [SpecialName]
        public object Call(CodeContext/*!*/ context, [ParamDictionary]IAttributesCollection kwArgs, params object[] args) {
            return PythonContext.GetContext(context).CallWithKeywords(this, args, kwArgs);
        }

        private Exception BadSelf(object got) {
            OldClass dt = im_class as OldClass;            

            string firstArg;
            if (got == null) {
                firstArg = "nothing";
            } else {
                firstArg = PythonOps.GetPythonTypeName(got) + " instance";
            }

            return PythonOps.TypeError("unbound method {0}() must be called with {1} instance as first argument (got {2} instead)",
                __name__,
                (dt != null) ? dt.__name__ : im_class,
                firstArg);
        }

        /// <summary>
        /// Validates that the current self object is usable for this method.  
        /// </summary>
        internal object CheckSelf(object self) {
            if (!PythonOps.IsInstance(self, im_class)) throw BadSelf(self);
            return self;
        }
        
        #region Object Overrides
        private string DeclaringClassAsString() {
            if (im_class == null) return "?";
            PythonType dt = im_class as PythonType;
            if (dt != null) return dt.Name;
            OldClass oc = im_class as OldClass;
            if (oc != null) return oc.__name__;
            return im_class.ToString();
        }

        public override bool Equals(object obj) {
            Method other = obj as Method;
            if (other == null) return false;

            return
                PythonOps.EqualRetBool(_inst, other._inst) &&
                PythonOps.EqualRetBool(_func, other._func);
        }

        public override int GetHashCode() {
            if (_inst == null) return PythonOps.Hash(DefaultContext.Default, _func);

            return PythonOps.Hash(DefaultContext.Default, _inst) ^ PythonOps.Hash(DefaultContext.Default, _func);
        }
        
        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return _weakref;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            _weakref = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region Custom member access

        [SpecialName]
        public object GetCustomMember(CodeContext context, string name) {
            if (name == "__module__") {
                // Get the module name from the function and pass that out.  Note that CPython's method has
                // no __module__ attribute and this value can be gotten via a call to method.__getattribute__ 
                // there as well.
                return PythonOps.GetBoundAttr(context, _func, Symbols.Module);                
            }
            
            object value;
            SymbolId symbol = SymbolTable.StringToId(name);
            if (TypeCache.Method.TryGetBoundMember(context, this, symbol, out value) ||       // look on method
                PythonOps.TryGetBoundAttr(context, _func, symbol, out value)) {               // Forward to the func
                return value;
            }

            
            return OperationFailed.Value;
        }

        [SpecialName]
        public void SetMemberAfter(CodeContext context, string name, object value) {
            TypeCache.Method.SetMember(context, this, SymbolTable.StringToId(name), value);
        }

        [SpecialName]
        public void DeleteMember(CodeContext context, string name) {
            TypeCache.Method.DeleteMember(context, this, SymbolTable.StringToId(name));
        }

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            List ret = TypeCache.Method.GetMemberNames(context);

            ret.AddNoLockNoDups(SymbolTable.IdToString(Symbols.Module));

            PythonFunction pf = _func as PythonFunction;
            if (pf != null) {
                IAttributesCollection dict = pf.func_dict;
                
                // Check the func
                foreach (KeyValuePair<object, object> kvp in dict) {
                    ret.AddNoLockNoDups(kvp.Key);
                }                
            }

            return ret;
        }

        #endregion

        #region PythonTypeSlot Overrides

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            if (this.im_self == null) {
                if (owner == null || owner == im_class || PythonOps.IsSubClass(owner, im_class)) {
                    value = new Method(_func, instance, owner);
                    return true;
                }
            }
            value = this;
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            object name;
            if (!PythonOps.TryGetBoundAttr(context, _func, Symbols.Name, out name)) {
                name = "?";
            }

            if (_inst != null) {
                return string.Format("<bound method {0}.{1} of {2}>",
                    DeclaringClassAsString(),
                    name,
                    PythonOps.Repr(context, _inst));
            } else {
                return string.Format("<unbound method {0}.{1}>", DeclaringClassAsString(), name);
            }
        }

        #endregion

        #region IDynamicMetaObjectProvider Members

        DynamicMetaObject/*!*/ IDynamicMetaObjectProvider.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaMethod(parameter, BindingRestrictions.Empty, this);
        }

        #endregion
    }
}
