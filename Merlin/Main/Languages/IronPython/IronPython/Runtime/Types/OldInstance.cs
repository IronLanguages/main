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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security.Permissions;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Types {

    [PythonType("instance")]
    [Serializable]
    public sealed partial class OldInstance :
        ICodeFormattable,
        IValueEquality,
#if !SILVERLIGHT // ICustomTypeDescriptor
        ICustomTypeDescriptor,
#endif
        ISerializable,
        IWeakReferenceable,
        IDynamicMetaObjectProvider, 
        IPythonMembersList
    {

        private PythonDictionary _dict;
        internal OldClass _class;
        private WeakRefTracker _weakRef;       // initialized if user defines finalizer on class or instance

        private static PythonDictionary MakeDictionary(OldClass oldClass) {
            //if (oldClass.OptimizedInstanceNames.Length == 0) {
            //    return new CustomOldClassDictionar();
            //}
            return new PythonDictionary(new CustomOldClassDictionaryStorage(oldClass.OptimizedInstanceNames, oldClass.OptimizedInstanceNamesVersion));
        }


        public OldInstance(CodeContext/*!*/ context, OldClass @class) {
            _class = @class;
            _dict = MakeDictionary(@class);
            if (_class.HasFinalizer) {
                // class defines finalizer, we get it automatically.
                AddFinalizer(context);
            }
        }

        public OldInstance(CodeContext/*!*/ context, OldClass @class, PythonDictionary dict) {
            _class = @class;
            _dict = dict ?? PythonDictionary.MakeSymbolDictionary();
            if (_class.HasFinalizer) {
                // class defines finalizer, we get it automatically.
                AddFinalizer(context);
            }
        }

#if !SILVERLIGHT // SerializationInfo
        private OldInstance(SerializationInfo info, StreamingContext context) {
            _class = (OldClass)info.GetValue("__class__", typeof(OldClass));
            _dict = MakeDictionary(_class);

            List<object> keys = (List<object>)info.GetValue("keys", typeof(List<object>));
            List<object> values = (List<object>)info.GetValue("values", typeof(List<object>));
            for (int i = 0; i < keys.Count; i++) {
                _dict[keys[i]] = values[i];
            }
        }

#pragma warning disable 169 // unused method - called via reflection from serialization
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        private void GetObjectData(SerializationInfo info, StreamingContext context) {
            ContractUtils.RequiresNotNull(info, "info");

            info.AddValue("__class__", _class);
            List<object> keys = new List<object>();
            List<object> values = new List<object>();
            foreach (object o in _dict.keys()) {
                keys.Add(o);
                object value;
                
                bool res = ((IAttributesCollection)_dict).TryGetObjectValue(o, out value);

                Debug.Assert(res);

                values.Add(value);
            }

            info.AddValue("keys", keys);
            info.AddValue("values", values);
        }
#pragma warning restore 169
#endif

        /// <summary>
        /// Returns the dictionary used to store state for this object
        /// </summary>
        internal PythonDictionary Dictionary {
            get { return _dict; }
        }

        public static bool operator true(OldInstance self) {
            return (bool)self.__nonzero__(DefaultContext.Default);
        }

        public static bool operator false(OldInstance self) {
            return !(bool)self.__nonzero__(DefaultContext.Default);
        }

        #region Object overrides

        public override string ToString() {
            object ret = InvokeOne(this, Symbols.String);

            if (ret != NotImplementedType.Value) {
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw PythonOps.TypeError("__str__ returned non-string type ({0})", PythonTypeOps.GetName(ret));
            }

            return __repr__(DefaultContext.Default);
        }

        #endregion

        #region ICodeFormattable Members

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            object ret = InvokeOne(this, Symbols.Repr);
            if(ret != NotImplementedType.Value) {
                string strRet;
                if (Converter.TryConvertToString(ret, out strRet) && strRet != null) {
                    return strRet;
                }
                throw PythonOps.TypeError("__repr__ returned non-string type ({0})", PythonTypeOps.GetName(ret));
            }

            return string.Format("<{0} instance at {1}>", _class.FullName, PythonOps.HexId(this));
        }

        #endregion

        [return: MaybeNotImplemented]
        public object __divmod__(CodeContext context, object divmod) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.DivMod, out value)) {
                return PythonCalls.Call(context, value, divmod);
            }


            return NotImplementedType.Value;
        }

        [return: MaybeNotImplemented]
        public static object __rdivmod__(CodeContext context, object divmod, [NotNull]OldInstance self) {
            object value;

            if (self.TryGetBoundCustomMember(context, Symbols.ReverseDivMod, out value)) {
                return PythonCalls.Call(context, value, divmod);
            }

            return NotImplementedType.Value;
        }

        public object __coerce__(CodeContext context, object other) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Coerce, out value)) {
                return PythonCalls.Call(context, value, other);
            }

            return NotImplementedType.Value;
        }

        public object __len__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.Length);
        }

        public object __pos__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Positive, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.Positive);
        }

        [SpecialName]
        public object GetItem(CodeContext context, object item) {
            return PythonOps.Invoke(context, this, Symbols.GetItem, item);
        }

        [SpecialName]
        public void SetItem(CodeContext context, object item, object value) {
            PythonOps.Invoke(context, this, Symbols.SetItem, item, value);
        }

        [SpecialName]
        public object DeleteItem(CodeContext context, object item) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.DelItem, out value)) {
                return PythonCalls.Call(context, value, item);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.DelItem);
        }

        public object __getslice__(CodeContext context, int i, int j) {
            object callable;
            if (TryRawGetAttr(context, Symbols.GetSlice, out callable)) {
                return PythonCalls.Call(context, callable, i, j);
            } else if (TryRawGetAttr(context, Symbols.GetItem, out callable)) {
                return PythonCalls.Call(context, callable, new Slice(i, j));
            }

            throw PythonOps.TypeError("instance {0} does not have __getslice__ or __getitem__", _class.Name);
        }
        
        public void __setslice__(CodeContext context, int i, int j, object value) {
            object callable;
            if (TryRawGetAttr(context, Symbols.SetSlice, out callable)) {
                PythonCalls.Call(context, callable, i, j, value);
                return;
            } else if (TryRawGetAttr(context, Symbols.SetItem, out callable)) {
                PythonCalls.Call(context, callable, new Slice(i, j), value);
                return;
            }

            throw PythonOps.TypeError("instance {0} does not have __setslice__ or __setitem__", _class.Name);
        }

        public object __delslice__(CodeContext context, int i, int j) {
            object callable;
            if (TryRawGetAttr(context, Symbols.DeleteSlice, out callable)) {
                return PythonCalls.Call(context, callable, i, j);
            } else if (TryRawGetAttr(context, Symbols.DelItem, out callable)) {
                return PythonCalls.Call(context, callable, new Slice(i, j));
            }

            throw PythonOps.TypeError("instance {0} does not have __delslice__ or __delitem__", _class.Name);
        }

        public object __index__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToInt, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.TypeError("object cannot be converted to an index");
        }

        public object __neg__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.OperatorNegate, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.OperatorNegate);
        }

        public object __abs__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.AbsoluteValue, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.AbsoluteValue);
        }

        public object __invert__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.OperatorOnesComplement, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.OperatorOnesComplement);
        }

        public object __contains__(CodeContext context, object index) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.Contains, out value)) {
                return PythonCalls.Call(context, value, index);
            }

            IEnumerator ie = PythonOps.GetEnumerator(this);
            while (ie.MoveNext()) {
                if (PythonOps.EqualRetBool(context, ie.Current, index)) return ScriptingRuntimeHelpers.True;
            }

            return ScriptingRuntimeHelpers.False;
        }
        
        [SpecialName]
        public object Call(CodeContext context) {
            return Call(context, ArrayUtils.EmptyObjects);
        }

        [SpecialName]
        public object Call(CodeContext context, object args) {
            try {
                PythonOps.FunctionPushFrame(PythonContext.GetContext(context));

                object value;
                if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                    KwCallInfo kwInfo;

                    if (args is object[])
                        return PythonOps.CallWithContext(context, value, (object[])args);
                    else if ((kwInfo = args as KwCallInfo) != null)
                        return PythonOps.CallWithKeywordArgs(context, value, kwInfo.Arguments, kwInfo.Names);

                    return PythonOps.CallWithContext(context, value, args);
                }
            } finally {
                PythonOps.FunctionPopFrame();
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", _class.Name);
        }

        [SpecialName]
        public object Call(CodeContext context, params object[] args) {
            try {
                PythonOps.FunctionPushFrame(PythonContext.GetContext(context));

                object value;
                if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                    return PythonOps.CallWithContext(context, value, args);
                }
            } finally {
                PythonOps.FunctionPopFrame();
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", _class.Name);
        }

        [SpecialName]
        public object Call(CodeContext context, [ParamDictionary]IDictionary<object, object> dict, params object[] args) {
            try {
                PythonOps.FunctionPushFrame(PythonContext.GetContext(context));

                object value;
                if (TryGetBoundCustomMember(context, Symbols.Call, out value)) {
                    return PythonOps.CallWithArgsTupleAndKeywordDictAndContext(context, value, args, ArrayUtils.EmptyStrings, null, dict);
                }
            } finally {
                PythonOps.FunctionPopFrame();
            }

            throw PythonOps.AttributeError("{0} instance has no __call__ method", _class.Name);
        }

        public object __nonzero__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.NonZero, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            if (TryGetBoundCustomMember(context, Symbols.Length, out value)) {
                value = PythonOps.CallWithContext(context, value);
                // Convert resulting object to the desired type
                if (value is Int32 || value is BigInteger) {
                    return ScriptingRuntimeHelpers.BooleanToObject(Converter.ConvertToBoolean(value));
                }
                throw PythonOps.TypeError("an integer is required, got {0}", PythonTypeOps.GetName(value));
            }

            return ScriptingRuntimeHelpers.True;
        }

        public object __hex__(CodeContext context) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.ConvertToHex, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.ConvertToHex);
        }

        public object __oct__(CodeContext context) {
            object value;
            if (TryGetBoundCustomMember(context, Symbols.ConvertToOctal, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            throw PythonOps.AttributeErrorForMissingAttribute(_class.Name, Symbols.ConvertToOctal);
        }

        public object __int__(CodeContext context) {
            object value;

            if (PythonOps.TryGetBoundAttr(context, this, Symbols.ConvertToInt, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return NotImplementedType.Value;
        }

        public object __long__(CodeContext context) {
            object value;

            if (PythonOps.TryGetBoundAttr(context, this, Symbols.ConvertToLong, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return NotImplementedType.Value;
        }

        public object __float__(CodeContext context) {
            object value;

            if (PythonOps.TryGetBoundAttr(context, this, Symbols.ConvertToFloat, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return NotImplementedType.Value;
        }

        public object __complex__(CodeContext context) {
            object value;

            if (TryGetBoundCustomMember(context, Symbols.ConvertToComplex, out value)) {
                return PythonOps.CallWithContext(context, value);
            }

            return NotImplementedType.Value;
        }

        public object __getattribute__(CodeContext context, string name) {
            object res;
            if (TryGetBoundCustomMember(context, SymbolTable.StringToId(name), out res)) {
                return res;
            }

            throw PythonOps.AttributeError("{0} instance has no attribute '{1}'", _class._name, name);
        }

        internal object GetBoundMember(CodeContext context, SymbolId name) {
            object ret;
            if (TryGetBoundCustomMember(context, name, out ret)) {
                return ret;
            }
            throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'",
                PythonTypeOps.GetName(this), SymbolTable.IdToString(name));
        }

        #region ICustomMembers Members

        internal bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            int nameId = name.Id;
            if (nameId == Symbols.Dict.Id) {
                //!!! user code can modify __del__ property of __dict__ behind our back
                value = _dict;
                return true;
            } else if (nameId == Symbols.Class.Id) {
                value = _class;
                return true;
            }

            if (TryRawGetAttr(context, name, out value)) return true;

            if (nameId != Symbols.GetBoundAttr.Id) {
                object getattr;
                if (TryRawGetAttr(context, Symbols.GetBoundAttr, out getattr)) {
                    try {
                        value = PythonCalls.Call(context, getattr, SymbolTable.IdToString(name));
                        return true;
                    } catch (MissingMemberException) {
                        // __getattr__ raised AttributeError, return false.
                    }
                }
            }

            return false;
        }

        internal void SetCustomMember(CodeContext context, SymbolId name, object value) {
            object setFunc;
            int nameId = name.Id;
            if (nameId == Symbols.Class.Id) {
                SetClass(value);
            } else if (nameId == Symbols.Dict.Id) {
                SetDict(context, value);
            } else if (_class.HasSetAttr && _class.TryLookupSlot(Symbols.SetAttr, out setFunc)) {
                PythonCalls.Call(context, _class.GetOldStyleDescriptor(context, setFunc, this, _class), name.ToString(), value);
            } else if (nameId == Symbols.Unassign.Id) {
                SetFinalizer(context, name, value);
            } else {
                ((IAttributesCollection)_dict)[name] = value;
            }
        }

        private void SetFinalizer(CodeContext/*!*/ context, SymbolId name, object value) {
            if (!HasFinalizer()) {
                // user is defining __del__ late bound for the 1st time
                AddFinalizer(context);
            }

            ((IAttributesCollection)_dict)[name] = value;
        }

        private void SetDict(CodeContext/*!*/ context, object value) {
            PythonDictionary dict = value as PythonDictionary;
            if (dict == null) {
                throw PythonOps.TypeError("__dict__ must be set to a dictionary");
            }
            if (HasFinalizer() && !_class.HasFinalizer) {
                if (!((IAttributesCollection)dict).ContainsKey(Symbols.Unassign)) {
                    ClearFinalizer();
                }
            } else if (((IAttributesCollection)dict).ContainsKey(Symbols.Unassign)) {
                AddFinalizer(context);
            }

            _dict = dict;
        }

        private void SetClass(object value) {
            OldClass oc = value as OldClass;
            if (oc == null) {
                throw PythonOps.TypeError("__class__ must be set to class");
            }
            _class = oc;
        }

        internal bool DeleteCustomMember(CodeContext context, SymbolId name) {
            if (name == Symbols.Class) throw PythonOps.TypeError("__class__ must be set to class");
            if (name == Symbols.Dict) throw PythonOps.TypeError("__dict__ must be set to a dictionary");

            object delFunc;
            if (_class.HasDelAttr && _class.TryLookupSlot(Symbols.DelAttr, out delFunc)) {
                PythonCalls.Call(context, _class.GetOldStyleDescriptor(context, delFunc, this, _class), name.ToString());
                return true;
            }


            if (name == Symbols.Unassign) {
                // removing finalizer
                if (HasFinalizer() && !_class.HasFinalizer) {
                    ClearFinalizer();
                }
            }

            if (!((IAttributesCollection)_dict).Remove(name)) {
                throw PythonOps.AttributeError("{0} is not a valid attribute", SymbolTable.IdToString(name));
            }
            return true;
        }

        #endregion

        #region IMembersList Members

        IList<string> IMembersList.GetMemberNames() {
            return PythonOps.GetStringMemberList(this);
        }

        IList<object> IPythonMembersList.GetMemberNames(CodeContext/*!*/ context) {
            PythonDictionary attrs = new PythonDictionary(_dict);
            OldClass.RecurseAttrHierarchy(this._class, attrs);
            return PythonOps.MakeListFromSequence(attrs);
        }

        #endregion

        [return: MaybeNotImplemented]
        public object __cmp__(CodeContext context, object other) {
            OldInstance oiOther = other as OldInstance;
            // CPython raises this if called directly, but not via cmp(os,ns) which still calls the user __cmp__
            //if(!(oiOther is OldInstance)) 
            //    throw Ops.TypeError("instance.cmp(x,y) -> y must be an instance, got {0}", Ops.StringRepr(DynamicHelpers.GetPythonType(other)));

            object res = InternalCompare(Symbols.Cmp, other);
            if (res != NotImplementedType.Value) return res;
            if (oiOther != null) {
                res = oiOther.InternalCompare(Symbols.Cmp, this);
                if (res != NotImplementedType.Value) return ((int)res) * -1;
            }

            return NotImplementedType.Value;
        }

        private object CompareForwardReverse(object other, SymbolId forward, SymbolId reverse) {
            object res = InternalCompare(forward, other);
            if (res != NotImplementedType.Value) return res;

            OldInstance oi = other as OldInstance;
            if (oi != null) {
                // comparison operators are reflexive
                return oi.InternalCompare(reverse, this);
            }

            return NotImplementedType.Value;
        }

        [return: MaybeNotImplemented]
        public static object operator >([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorGreaterThan, Symbols.OperatorLessThan);
        }

        [return: MaybeNotImplemented]
        public static object operator <([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorLessThan, Symbols.OperatorGreaterThan);
        }

        [return: MaybeNotImplemented]
        public static object operator >=([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorGreaterThanOrEqual, Symbols.OperatorLessThanOrEqual);
        }

        [return: MaybeNotImplemented]
        public static object operator <=([NotNull]OldInstance self, object other) {
            return self.CompareForwardReverse(other, Symbols.OperatorLessThanOrEqual, Symbols.OperatorGreaterThanOrEqual);
        }

        private object InternalCompare(SymbolId cmp, object other) {
            return InvokeOne(this, other, cmp);
        }

        #region ICustomTypeDescriptor Members
#if !SILVERLIGHT // ICustomTypeDescriptor

        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return CustomTypeDescHelpers.GetAttributes(this);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return CustomTypeDescHelpers.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return CustomTypeDescHelpers.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return CustomTypeDescHelpers.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return CustomTypeDescHelpers.GetDefaultEvent(this);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return CustomTypeDescHelpers.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetEvents(this, attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return CustomTypeDescHelpers.GetEvents(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return CustomTypeDescHelpers.GetProperties(this, attributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return CustomTypeDescHelpers.GetProperties(this);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
        }

#endif
        #endregion

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return _weakRef;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            _weakRef = value;
            return true;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            ((IWeakReferenceable)this).SetWeakRef(value);
        }

        #endregion

        #region Rich Equality
        // Specific rich equality support for when the user calls directly from oldinstance type.

        public int __hash__() {
            object func;
            object ret = InvokeOne(this, Symbols.Hash);
            if(ret != NotImplementedType.Value) {
                BigInteger bi = ret as BigInteger;
                if (!Object.ReferenceEquals(bi, null)) {
                    return BigIntegerOps.__hash__(bi);
                } else if (!(ret is int))
                    throw PythonOps.TypeError("expected int from __hash__, got {0}", PythonTypeOps.GetName(ret));

                return (int)ret;
            }

            if (TryGetBoundCustomMember(DefaultContext.Default, Symbols.Cmp, out func) ||
                TryGetBoundCustomMember(DefaultContext.Default, Symbols.OperatorEquals, out func)) {
                throw PythonOps.TypeError("unhashable instance");
            }

            return GetHashCode();
        }

        [return: MaybeNotImplemented]
        public object __eq__(object other) {
            object res = InvokeBoth(other, Symbols.OperatorEquals);
            if (res != NotImplementedType.Value) {
                return res;
            }


            return NotImplementedType.Value;
        }

        private object InvokeBoth(object other, SymbolId si) {
            object res = InvokeOne(this, other, si);
            if (res != NotImplementedType.Value) {
                return res;
            }
            OldInstance oi = other as OldInstance;
            if (oi != null) {
                res = InvokeOne(oi, this, si);
                if (res != NotImplementedType.Value) {
                    return res;
                }
            }
            return NotImplementedType.Value;
        }

        private static object InvokeOne(OldInstance self, object other, SymbolId si) {
            object func;
            try {
                if (!self.TryGetBoundCustomMember(DefaultContext.Default, si, out func)) {
                    return NotImplementedType.Value;
                }
            } catch (MissingMemberException) {
                return NotImplementedType.Value;
            }

            return PythonOps.CallWithContext(DefaultContext.Default, func, other);
        }

        private static object InvokeOne(OldInstance self, object other, object other2, SymbolId si) {
            object func;
            try {
                if (!self.TryGetBoundCustomMember(DefaultContext.Default, si, out func)) {
                    return NotImplementedType.Value;
                }
            } catch (MissingMemberException) {
                return NotImplementedType.Value;
            }

            return PythonOps.CallWithContext(DefaultContext.Default, func, other, other2);
        }

        private static object InvokeOne(OldInstance self, SymbolId si) {
            object func;
            try {
                if (!self.TryGetBoundCustomMember(DefaultContext.Default, si, out func)) {
                    return NotImplementedType.Value;
                }
            } catch (MissingMemberException) {
                return NotImplementedType.Value;
            }

            return PythonOps.CallWithContext(DefaultContext.Default, func);
        }

        [return: MaybeNotImplemented]
        public object __ne__(object other) {
            object res = InvokeBoth(other, Symbols.OperatorNotEquals);
            if (res != NotImplementedType.Value) {
                return res;
            }

            return NotImplementedType.Value;
        }

        [return: MaybeNotImplemented]
        [SpecialName]
        public static object Power([NotNull]OldInstance self, object other, object mod) {
            object res = InvokeOne(self, other, mod, Symbols.OperatorPower);
            if (res != NotImplementedType.Value) return res;

            return NotImplementedType.Value;
        }

        #endregion

        #region ISerializable Members
#if !SILVERLIGHT // SerializationInfo
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("__class__", _class);
            info.AddValue("__dict__", _dict);
        }

#endif
        #endregion

        #region Private Implementation Details

        private void RecurseAttrHierarchyInt(OldClass oc, IDictionary<SymbolId, object> attrs) {
            foreach (KeyValuePair<object, object> kvp in oc._dict._storage.GetItems()) {
                string strKey = kvp.Key as string;
                if (strKey != null) {
                    SymbolId si = SymbolTable.StringToId(strKey);

                    if (!attrs.ContainsKey(si)) {
                        attrs.Add(si, si);
                    }
                }
            }
            //  recursively get attrs in parent hierarchy
            if (oc.BaseClasses.Count != 0) {
                foreach (OldClass parent in oc.BaseClasses) {
                    RecurseAttrHierarchyInt(parent, attrs);
                }
            }
        }

        private void AddFinalizer(CodeContext/*!*/ context) {
            InstanceFinalizer oif = new InstanceFinalizer(context, this);
            _weakRef = new WeakRefTracker(oif, oif);
        }

        private void ClearFinalizer() {
            if (_weakRef == null) return;

            WeakRefTracker wrt = _weakRef;
            if (wrt != null) {
                // find our handler and remove it (other users could have created weak refs to us)
                for (int i = 0; i < wrt.HandlerCount; i++) {
                    if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                        wrt.RemoveHandlerAt(i);
                        break;
                    }
                }

                // we removed the last handler
                if (wrt.HandlerCount == 0) {
                    GC.SuppressFinalize(wrt);
                    _weakRef = null;
                }
            }
        }

        private bool HasFinalizer() {
            if (_weakRef != null) {
                WeakRefTracker wrt = _weakRef;
                if (wrt != null) {
                    for (int i = 0; i < wrt.HandlerCount; i++) {
                        if (wrt.GetHandlerCallback(i) is InstanceFinalizer) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryRawGetAttr(CodeContext context, SymbolId name, out object ret) {
            if (_dict._storage.TryGetValue(name, out ret)) {
                return true;
            }

            if (_class.TryLookupSlot(name, out ret)) {
                ret = _class.GetOldStyleDescriptor(context, ret, this, _class);
                return true;
            }

            return false;
        }

        #endregion

        #region IValueEquality Members

        int IValueEquality.GetValueHashCode() {
            object res = __hash__();
            if (res is int) {
                return (int)res;
            }
            return base.GetHashCode();
        }

        bool IValueEquality.ValueEquals(object other) {
            return Equals(other);
        }

        #endregion

        #region IDynamicMetaObjectProvider Members

        DynamicMetaObject/*!*/ IDynamicMetaObjectProvider.GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaOldInstance(parameter, BindingRestrictions.Empty, this);
        }

        #endregion
    }
}
