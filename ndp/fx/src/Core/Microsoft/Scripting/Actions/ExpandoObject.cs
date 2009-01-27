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
using System.Diagnostics;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic {
    /// <summary>
    /// Represents an object with members that can be dynamically added and removed at runtime.
    /// </summary>
    public sealed class ExpandoObject : IDynamicObject {
        private ExpandoData _data;                                  // the data currently being held by the Expando object

        internal static object Uninitialized = new object();        // A marker object used to identify that a value is uninitialized.

        /// <summary>
        /// Creates a new ExpandoObject with no members.
        /// </summary>
        public ExpandoObject() {
            _data = ExpandoData.Empty;
        }

        #region Get/Set/Delete Helpers

        /// <summary>
        /// Gets the data stored for the specified class at the specified index.  If the
        /// class has changed a full lookup for the slot will be performed and the correct
        /// value will be retrieved.
        /// </summary>
        internal object GetValue(ExpandoClass klass, int index, bool caseInsensitive, string name) {
            Debug.Assert(index != -1);

            // read the data now.  The data is immutable so we get a consistent view.
            // If there's a concurrent writer they will replace data and it just appears
            // that we won the race
            ExpandoData data = _data;
            object res = Uninitialized;
            if (data.Class != klass || caseInsensitive) {
                /* Re-search for the index matching the name here if
                 *  1) the class has changed, we need to get the correct index and return
                 *  the value there.
                 *  2) the search is case insensitive:
                 *      a. the member specified by index may be deleted, but there might be other
                 *      members matching the name if the binder is case insensitive.
                 *      b. the member that exactly matches the name didn't exist before and exists now,
                 *      need to find the exact match.
                 */
                index = data.Class.GetValueIndex(name, caseInsensitive, this);
            }

            // index is now known to be correct
            res = data.Data[index];

            if (res == Uninitialized) {
                throw new MissingMemberException(klass.GetIndexName(index));
            }

            return res;
        }
        
        /// <summary>
        /// Sets the data for the specified class at the specified index.  If the class has
        /// changed then a full look for the slot will be performed.  If the new class does
        /// not have the provided slot then the Expando's class will change. Only case sensitive
        /// setter is supported in ExpandoObject.
        /// </summary>
        internal void SetValue(ExpandoClass klass, int index, object value) {
            Debug.Assert(index != -1);

            lock (this) {
                ExpandoData data = _data;

                if (data.Class != klass) {
                    // the class has changed, we need to get the correct index and set
                    // the value there.  If we don't have the value then we need to
                    // promote the class - that should only happen when we have multiple
                    // concurrent writers.
                    string name = klass.GetIndexName(index);
                    index = data.Class.GetValueIndexCaseSensitive(name);
                    if (index == -1) {
                        ExpandoClass newClass = data.Class.FindNewClass(name);

                        data = PromoteClassWorker(data.Class, newClass);
                        index = data.Class.GetValueIndexCaseSensitive(name);

                        Debug.Assert(index != -1);
                    }
                }

                data.Data[index] = value;
            }           
        }              

        /// <summary>
        /// Deletes the data stored for the specified class at the specified index.
        /// </summary>
        internal bool DeleteValue(ExpandoClass klass, int index) {
            Debug.Assert(index != -1);

            lock (this) {
                ExpandoData data = _data;

                if (data.Class != klass) {
                    // the class has changed, we need to get the correct index.  If there is
                    // no associated index we simply can't have the value and we return
                    // false.
                    index = data.Class.GetValueIndexCaseSensitive(klass.GetIndexName(index));
                    if (index == -1) {
                        return false;
                    }
                }

                object curValue = data.Data[index];
                data.Data[index] = Uninitialized;

                return curValue != Uninitialized;
            }
        }

        /// <summary>
        /// Returns true if the member at the specified index has been deleted,
        /// otherwise false.
        /// </summary>
        internal bool IsDeletedMember(int index) {
            ExpandoData data = _data;
            Debug.Assert(index >= 0 && index <= data.Data.Length);

            if (index == data.Data.Length) {
                //the member is a newly added by SetMemberBinder and not in data yet
                return false;
            }

            return _data.Data[index] == ExpandoObject.Uninitialized;
        }

        /// <summary>
        /// Exposes the ExpandoClass which we've associated with this 
        /// Expando object.  Used for type checks in rules.
        /// </summary>
        internal ExpandoClass Class {
            get {
                return _data.Class;
            }
        }

        /// <summary>
        /// Promotes the class from the old type to the new type and returns the new
        /// ExpandoData object.
        /// </summary>
        private ExpandoData PromoteClassWorker(ExpandoClass oldClass, ExpandoClass newClass) {
            Debug.Assert(oldClass != newClass);

            lock (this) {
                if (_data.Class == oldClass) {
                    _data = new ExpandoData(newClass, newClass.GetNewKeys(_data.Data));
                }
                return _data;
            }
        }

        /// <summary>
        /// Internal helper to promote a class.  Called from our RuntimeOps helper.  This
        /// version simply doesn't expose the ExpandoData object which is a private
        /// data structure.
        /// </summary>
        internal void PromoteClass(ExpandoClass oldClass, ExpandoClass newClass) {
            PromoteClassWorker(oldClass, newClass);
        }

        #endregion

        #region IDynamicObject Members

        DynamicMetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new MetaExpando(parameter, this);
        }

        #endregion

        #region MetaExpando

        private class MetaExpando : DynamicMetaObject {
            public MetaExpando(Expression expression, ExpandoObject value)
                : base(expression, BindingRestrictions.Empty, value) {
            }

            //Get the DynamicMetaObject corresponding the the value for the
            //specified member.
            private DynamicMetaObject GetDynamicMetaObjectForMember(string name, bool ignoreCase, DynamicMetaObjectBinder binder) {
                ExpandoClass klass = Value.Class;

                int index = klass.GetValueIndex(name, ignoreCase, Value);
                string methodName = ignoreCase ? "ExpandoGetValueIgnoreCase" : "ExpandoGetValue";

                Expression target;
                if (index == -1) {
                    // the key does not exist, report a MissingMemberException
                    target = Helpers.Convert(
                        Expression.Throw(
                            Expression.New(
                                typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                                Expression.Constant(name)
                            )
                        ),
                        typeof(object)
                    );
                } else {
                    target = Expression.Call(
                        typeof(RuntimeOps).GetMethod(methodName),
                        GetLimitedSelf(),
                        Expression.Constant(klass),
                        Expression.Constant(index),
                        Expression.Constant(name)
                    );
                }

                // add the dynamic test for the target
                return new DynamicMetaObject(
                    AddDynamicTestAndDefer(
                        binder,
                        new DynamicMetaObject[] { this },
                        klass,
                        null,
                        target
                    ),
                    GetRestrictions()
                );
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                ContractUtils.RequiresNotNull(binder, "binder");
                return GetDynamicMetaObjectForMember(binder.Name, binder.IgnoreCase, binder);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                ContractUtils.RequiresNotNull(binder, "binder");
                DynamicMetaObject memberValue = GetDynamicMetaObjectForMember(binder.Name, binder.IgnoreCase, binder);
                
                //invoke the member value using the language's binder
                return binder.FallbackInvoke(memberValue, args, null);
            }

            public override DynamicMetaObject BindUnaryOperationOnMember(UnaryOperationOnMemberBinder binder) {
                ContractUtils.RequiresNotNull(binder, "binder");
                DynamicMetaObject memberValue = GetDynamicMetaObjectForMember(binder.Name, binder.IgnoreCase, binder);

                //apply the unary operation to the member value using the language's binder
                return binder.FallbackUnaryOperationOnMember(memberValue);
            }

            public override DynamicMetaObject BindBinaryOperationOnMember(BinaryOperationOnMemberBinder binder, DynamicMetaObject value) {
                ContractUtils.RequiresNotNull(binder, "binder");
                DynamicMetaObject memberValue = GetDynamicMetaObjectForMember(binder.Name, binder.IgnoreCase, binder);

                //apply the binary operation to the member value using the language's binder
                return binder.FallbackBinaryOperationOnMember(memberValue, value);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                ContractUtils.RequiresNotNull(binder, "binder");
                ContractUtils.RequiresNotNull(value, "value");

                ExpandoClass klass;
                int index;

                ExpandoClass originalClass = GetClassEnsureIndex(binder.Name, out klass, out index);

                //SetMember is always case sensitive
                return new DynamicMetaObject(
                    AddDynamicTestAndDefer(
                        binder,
                        new DynamicMetaObject[] { this, value },
                        klass,
                        originalClass,
                        Helpers.Convert(
                            Expression.Call(
                                typeof(RuntimeOps).GetMethod("ExpandoSetValue"),
                                GetLimitedSelf(),
                                Expression.Constant(klass),
                                Expression.Constant(index),
                                Helpers.Convert(
                                    value.Expression,
                                    typeof(object)
                                )
                            ),
                            typeof(object)
                        )
                    ),
                    GetRestrictions()
                );
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                ContractUtils.RequiresNotNull(binder, "binder");

                ExpandoClass klass;
                int index;

                ExpandoClass originalClass = GetClassEnsureIndex(binder.Name, out klass, out index);

                //DeleteMember is always case sensitive
                return new DynamicMetaObject(
                    AddDynamicTestAndDefer(
                        binder,
                        new DynamicMetaObject[] { this },
                        klass,
                        originalClass,
                        Helpers.Convert(
                            Expression.Call(
                                typeof(RuntimeOps).GetMethod("ExpandoDeleteValue"),
                                GetLimitedSelf(),
                                Expression.Constant(klass),
                                Expression.Constant(index)
                            ),
                            typeof(object)
                        )
                    ),
                    GetRestrictions()
                );
            }

            public override IEnumerable<KeyValuePair<string,object>> GetDynamicDataMembers()
            {
                var expandoData = Value._data;
                var klass = expandoData.Class;
                var data = expandoData.Data;
                for (int i = 0; i < klass.Keys.Length; i++) {
                    object val = data[i];
                    // all members are data members in a class
                    if (val != ExpandoObject.Uninitialized) {
                        yield return new KeyValuePair<string, object>(klass.Keys[i], val);
                    }
                }
            } 

            public override IEnumerable<string> GetDynamicMemberNames() {
                var expandoData = Value._data;
                var klass = expandoData.Class;
                var data = expandoData.Data;
                for (int i = 0; i < klass.Keys.Length; i++) {
                    object val = data[i];
                    if (val != ExpandoObject.Uninitialized) {
                        yield return klass.Keys[i];
                    }
                }
            }

            /// <summary>
            /// Adds a dynamic test which checks if the version has changed.  The test is only necessary for
            /// performance as the methods will do the correct thing if called with an incorrect version.
            /// </summary>
            private Expression AddDynamicTestAndDefer(DynamicMetaObjectBinder binder, DynamicMetaObject[] args, ExpandoClass klass, ExpandoClass originalClass, Expression ifTestSucceeds) {
                if (originalClass != null) {
                    // we are accessing a member which has not yet been defined on this class.
                    // We force a class promotion after the type check.  If the class changes the 
                    // promotion will fail and the set/delete will do a full lookup using the new
                    // class to discover the name.
                    Debug.Assert(originalClass != klass);

                    ifTestSucceeds = Expression.Block(
                        Expression.Call(
                            null,
                            typeof(RuntimeOps).GetMethod("ExpandoPromoteClass"),
                            GetLimitedSelf(),
                            Expression.Constant(originalClass),
                            Expression.Constant(klass)
                        ),
                        ifTestSucceeds
                    );
                }

                return Expression.Condition(
                    Expression.Call(
                        null,
                        typeof(RuntimeOps).GetMethod("ExpandoCheckVersion"),
                        GetLimitedSelf(),
                        Expression.Constant(originalClass ?? klass)
                    ),
                    ifTestSucceeds,
                    binder.Defer(args).Expression
                );
            }

            /// <summary>
            /// Gets the class and the index associated with the given name.  Does not update the expando object.  Instead
            /// this returns both the original and desired new class.  A rule is created which includes the test for the
            /// original class, the promotion to the new class, and the set/delete based on the class post-promotion.
            /// </summary>
            private ExpandoClass GetClassEnsureIndex(string name, out ExpandoClass klass, out int index) {
                ExpandoClass originalClass = Value.Class;

                index = originalClass.GetValueIndexCaseSensitive(name);
                if (index == -1) {
                    // go ahead and find a new class now...
                    ExpandoClass newClass = originalClass.FindNewClass(name);

                    klass = newClass;
                    index = newClass.GetValueIndexCaseSensitive(name);

                    Debug.Assert(index != -1);
                    return originalClass;
                } else {
                    klass = originalClass;
                    return null;
                }                
            }

            /// <summary>
            /// Returns our Expression converted to our known LimitType
            /// </summary>
            private Expression GetLimitedSelf() {
                return Helpers.Convert(
                    Expression,
                    LimitType
                );
            }

            /// <summary>
            /// Returns a Restrictions object which includes our current restrictions merged
            /// with a restriction limiting our type
            /// </summary>
            private BindingRestrictions GetRestrictions() {
                Debug.Assert(Restrictions == BindingRestrictions.Empty, "We don't merge, restrictions are always empty");

                return BindingRestrictions.GetTypeRestriction(Expression, LimitType);
            }

            public new ExpandoObject Value {
                get {
                    return (ExpandoObject)base.Value;
                }
            }
        }

        #endregion

        #region ExpandoData
        
        /// <summary>
        /// Stores the class and the data associated with the class as one atomic
        /// pair.  This enables us to do a class check in a thread safe manner w/o
        /// requiring locks.
        /// </summary>
        private class ExpandoData {
            internal static ExpandoData Empty = new ExpandoData();

            /// <summary>
            /// the dynamically assigned class associated with the Expando object
            /// </summary>
            internal readonly ExpandoClass Class;
            /// <summary>
            /// data stored in the expando object, key names are stored in the class.
            /// 
            /// Expando._data must be locked when mutating the value.  Otherwise a copy of it 
            /// could be made and lose values.
            /// </summary>
            internal readonly object[] Data;

            /// <summary>
            /// Constructs an empty ExpandoData object with the empty class and no data.
            /// </summary>
            private ExpandoData() {
                Class = ExpandoClass.Empty;
                Data = new object[0];
            }

            /// <summary>
            /// Constructs a new ExpandoData object with the specified class and data.
            /// </summary>
            internal ExpandoData(ExpandoClass klass, object[] data) {
                Class = klass;
                Data = data;
            }
        }

        #endregion            
    }
}

namespace System.Runtime.CompilerServices {
    public static partial class RuntimeOps {
        /// <summary>
        /// Gets the value of an item in an expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>The value of the member.</returns>
        [Obsolete("used by generated code", true)]
        public static object ExpandoGetValue(ExpandoObject expando, object indexClass, int index, string name) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.GetValue((ExpandoClass)indexClass, index, false, name);
        }

        /// <summary>
        /// Gets the value of an item in an expando object, ignoring the case of the member name.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>The value of the member.</returns>
        [Obsolete("used by generated code", true)]
        public static object ExpandoGetValueIgnoreCase(ExpandoObject expando, object indexClass, int index, string name) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.GetValue((ExpandoClass)indexClass, index, true, name);
        }

        /// <summary>
        /// Sets the value of an item in an expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <param name="value">The value of the member.</param>
        [Obsolete("used by generated code", true)]
        public static void ExpandoSetValue(ExpandoObject expando, object indexClass, int index, object value) {
            ContractUtils.RequiresNotNull(expando, "expando");
            expando.SetValue((ExpandoClass)indexClass, index, value);
        }

        /// <summary>
        /// Deletes the value of an item in an expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="indexClass">The class of the expando object.</param>
        /// <param name="index">The index of the member.</param>
        /// <returns>true if the item was successfully removed; otherwise, false.</returns>
        [Obsolete("used by generated code", true)]
        public static bool ExpandoDeleteValue(ExpandoObject expando, object indexClass, int index) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.DeleteValue((ExpandoClass)indexClass, index);
        }

        /// <summary>
        /// Checks the version of the expando object.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="version">the version to check.</param>
        /// <returns>true if the version is equal; otherwise, false.</returns>
        [Obsolete("used by generated code", true)]
        public static bool ExpandoCheckVersion(ExpandoObject expando, object version) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.Class == version;
        }

        /// <summary>
        /// Promotes an expando object from one class to a new class.
        /// </summary>
        /// <param name="expando">The expando object.</param>
        /// <param name="oldClass">The old class of the expando object.</param>
        /// <param name="newClass">The new class of the expando object.</param>
        [Obsolete("used by generated code", true)]
        public static void ExpandoPromoteClass(ExpandoObject expando, object oldClass, object newClass) {
            ContractUtils.RequiresNotNull(expando, "expando");
            expando.PromoteClass((ExpandoClass)oldClass, (ExpandoClass)newClass);
        }
    }
}

