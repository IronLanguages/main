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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Dynamic.Utils;

namespace System.Dynamic {
    /// <summary>
    /// Simple type which implements IDynamicObject to support getting/setting/deleting members
    /// at runtime.
    /// </summary>
    public sealed class ExpandoObject : IDynamicObject {
        private ExpandoData _data;                                  // the data currently being held by the Expando object

        internal static object Uninitialized = new object();        // A marker object used to identify that a value is uninitialized.

        /// <summary>
        /// Creates a new Expando object with no members.
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
        internal object GetValue(ExpandoClass klass, int index, bool caseInsensitive) {
            Debug.Assert(index != -1);

            // read the data now.  The data is immutable so we get a consistent view.
            // If there's a concurrent writer they will replace data and it just appears
            // that we won the race
            ExpandoData data = _data;
            object res = Uninitialized;
            if (data.Class != klass) {
                // the class has changed, we need to get the correct index and return
                // the value there.
                index = data.Class.GetValueIndex(klass.GetIndexName(index), caseInsensitive);
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
        /// not have the provided slot then the Expando's class will change.
        /// </summary>
        internal void SetValue(ExpandoClass klass, int index, bool caseInsensitive, object value) {
            Debug.Assert(index != -1);

            lock (this) {
                ExpandoData data = _data;

                if (data.Class != klass) {
                    // the class has changed, we need to get the correct index and set
                    // the value there.  If we don't have the value then we need to
                    // promote the class - that should only happen when we have multiple
                    // concurrent writers.
                    string name = klass.GetIndexName(index);
                    index = data.Class.GetValueIndex(name, caseInsensitive);
                    if (index == -1) {
                        ExpandoClass newClass = data.Class.FindNewClass(name, caseInsensitive);

                        data = PromoteClassWorker(data.Class, newClass);
                        index = data.Class.GetValueIndex(name, caseInsensitive);

                        Debug.Assert(index != -1);
                    }
                }

                data.Data[index] = value;
            }           
        }              

        /// <summary>
        /// Gets the data stored for the specified class at the specified index.  If the
        /// class has changed a full lookup for the slot will be performed and the correct
        /// value will be retrieved.
        /// </summary>
        internal bool DeleteValue(ExpandoClass klass, int index, bool caseInsensitive) {
            Debug.Assert(index != -1);

            lock (this) {
                ExpandoData data = _data;

                if (data.Class != klass) {
                    // the class has changed, we need to get the correct index.  If there is
                    // no associated index we simply can't have the value and we return
                    // false.
                    index = data.Class.GetValueIndex(klass.GetIndexName(index), caseInsensitive);
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

        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new MetaExpando(parameter, this);
        }

        #endregion

        #region MetaExpando

        private class MetaExpando : MetaObject {
            public MetaExpando(Expression expression, ExpandoObject value)
                : base(expression, Restrictions.Empty, value) {
            }

            public override MetaObject BindGetMember(GetMemberBinder binder) {
                ContractUtils.RequiresNotNull(binder, "binder");

                ExpandoClass klass = Value.Class;

                int index = klass.GetValueIndex(binder.Name, binder.IgnoreCase);
                string methodName = binder.IgnoreCase ? "ExpandoGetValueIgnoreCase" : "ExpandoGetValue";

                Expression target;
                if (index == -1) {
                    // the key does not exist, report a MissingMemberException
                    target = Helpers.Convert(
                        Expression.Throw(
                            Expression.New(
                                typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                                Expression.Constant(binder.Name)
                            )
                        ),
                        typeof(object)                        
                    );
                } else {
                    target = Expression.Call(
                        typeof(RuntimeOps).GetMethod(methodName),
                        GetLimitedSelf(),
                        Expression.Constant(klass),
                        Expression.Constant(index)
                    );
                }

                // add the dynamic test for the target
                return new MetaObject(
                    AddDynamicTestAndDefer(
                        binder,
                        new MetaObject[] { this },
                        klass,
                        null,
                        target
                    ),
                    GetRestrictions()
                );
            }

            public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
                ContractUtils.RequiresNotNull(binder, "binder");
                ContractUtils.RequiresNotNull(value, "value");

                ExpandoClass klass;
                int index;

                ExpandoClass originalClass = GetClassEnsureIndex(binder.Name, binder.IgnoreCase, out klass, out index);

                string methodName = binder.IgnoreCase ? "ExpandoSetValueIgnoreCase" : "ExpandoSetValue";

                return new MetaObject(
                    AddDynamicTestAndDefer(
                        binder,
                        new MetaObject[] { this, value },
                        klass,
                        originalClass,
                        Helpers.Convert(
                            Expression.Call(
                                typeof(RuntimeOps).GetMethod(methodName),
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

            public override MetaObject BindDeleteMember(DeleteMemberBinder binder) {
                ContractUtils.RequiresNotNull(binder, "binder");

                ExpandoClass klass;
                int index;

                ExpandoClass originalClass = GetClassEnsureIndex(binder.Name, binder.IgnoreCase, out klass, out index);

                string methodName = binder.IgnoreCase ? "ExpandoDeleteValueIgnoreCase" : "ExpandoDeleteValue";

                return new MetaObject(
                    AddDynamicTestAndDefer(
                        binder, 
                        new MetaObject[] { this }, 
                        klass, 
                        originalClass,
                        Helpers.Convert(
                            Expression.Call(
                                typeof(RuntimeOps).GetMethod(methodName),
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

            public override IEnumerable<string> GetDynamicMemberNames() {
                return new ReadOnlyCollection<string>(Value._data.Class.Keys);
            }

            /// <summary>
            /// Adds a dynamic test which checks if the version has changed.  The test is only necessary for
            /// performance as the methods will do the correct thing if called with an incorrect version.
            /// </summary>
            private Expression AddDynamicTestAndDefer(MetaObjectBinder binder, MetaObject[] args, ExpandoClass klass, ExpandoClass originalClass, Expression ifTestSucceeds) {
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
            private ExpandoClass GetClassEnsureIndex(string name, bool ignoreCase, out ExpandoClass klass, out int index) {
                ExpandoClass originalClass = Value.Class;

                index = originalClass.GetValueIndex(name, ignoreCase);
                if (index == -1) {
                    // go ahead and find a new class now...
                    ExpandoClass newClass = originalClass.FindNewClass(name, ignoreCase);

                    klass = newClass;
                    index = newClass.GetValueIndex(name, ignoreCase);

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
            private Restrictions GetRestrictions() {
                Debug.Assert(Restrictions == Restrictions.Empty, "We don't merge, restrictions are always empty");

                return Restrictions.GetTypeRestriction(Expression, LimitType);
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
        [Obsolete("used by generated code", true)]
        public static object ExpandoGetValue(ExpandoObject expando, object indexClass, int index) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.GetValue((ExpandoClass)indexClass, index, false);
        }

        [Obsolete("used by generated code", true)]
        public static object ExpandoGetValueIgnoreCase(ExpandoObject expando, object indexClass, int index) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.GetValue((ExpandoClass)indexClass, index, true);
        }

        [Obsolete("used by generated code", true)]
        public static void ExpandoSetValue(ExpandoObject expando, object indexClass, int index, object value) {
            ContractUtils.RequiresNotNull(expando, "expando");
            expando.SetValue((ExpandoClass)indexClass, index, false, value);
        }

        [Obsolete("used by generated code", true)]
        public static void ExpandoSetValueIgnoreCase(ExpandoObject expando, object indexClass, int index, object value) {
            ContractUtils.RequiresNotNull(expando, "expando");
            expando.SetValue((ExpandoClass)indexClass, index, true, value);
        }

        [Obsolete("used by generated code", true)]
        public static bool ExpandoDeleteValue(ExpandoObject expando, object indexClass, int index) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.DeleteValue((ExpandoClass)indexClass, index, false);
        }

        [Obsolete("used by generated code", true)]
        public static bool ExpandoDeleteValueIgnoreCase(ExpandoObject expando, object indexClass, int index) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.DeleteValue((ExpandoClass)indexClass, index, true);
        }

        [Obsolete("used by generated code", true)]
        public static bool ExpandoCheckVersion(ExpandoObject expando, object version) {
            ContractUtils.RequiresNotNull(expando, "expando");
            return expando.Class == version;
        }

        public static void ExpandoPromoteClass(ExpandoObject expando, object oldClass, object newClass) {
            ContractUtils.RequiresNotNull(expando, "expando");
            expando.PromoteClass((ExpandoClass)oldClass, (ExpandoClass)newClass);
        }
    }
}

