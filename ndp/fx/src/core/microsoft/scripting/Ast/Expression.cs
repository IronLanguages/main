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
using System.Globalization;
using System.Reflection;
using System.Dynamic.Utils;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace System.Linq.Expressions {
    /// <summary>
    /// Expression is the base type for all nodes in Expression Trees
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public abstract partial class Expression {
        private static CacheDict<Type, MethodInfo> _LambdaDelegateCache = new CacheDict<Type, MethodInfo>(40);
        private static CacheDict<Type, Func<Expression, string, IEnumerable<ParameterExpression>, LambdaExpression>> _exprCtors;
        private static MethodInfo _lambdaCtorMethod;

        // protected ctors are part of API surface area

#if !MICROSOFT_SCRIPTING_CORE
        /******************************************************************************************************
         * BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG BUG 
         *
         * We need to switch to using ConditionalWeakHandle whenever that becomes available in our tools drop.
         * Once that's done we can remove WeakDictionary entirely and just use mscorlib's ConditionalWeakTable.
         */
         
         [Obsolete]
         private class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
            // The one and only comparer instance.
            static readonly IEqualityComparer<object> comparer = new WeakComparer<object>();
    
            IDictionary<object, TValue> dict = new Dictionary<object, TValue>(comparer);
            int version, cleanupVersion;
    
    #if SILVERLIGHT // GC
            WeakReference cleanupGC = new WeakReference(new object());
    #else
            int cleanupGC = 0;
    #endif
    
            public WeakDictionary() {
            }
    
            #region IDictionary<TKey,TValue> Members
    
            public void Add(TKey key, TValue value) {
                CheckCleanup();
    
                // If the WeakHash already holds this value as a key, it will lead to a circular-reference and result
                // in the objects being kept alive forever. The caller needs to ensure that this cannot happen.
                Debug.Assert(!dict.ContainsKey(value));
    
                dict.Add(new WeakObject<TKey>(key), value);
            }
    
            public bool ContainsKey(TKey key) {
                // We dont have to worry about creating "new WeakObject<TKey>(key)" since the comparer
                // can compare raw objects with WeakObject<T>.
                return dict.ContainsKey(key);
            }
    
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
            public ICollection<TKey> Keys {
                get {
                    // TODO:
                    throw new NotImplementedException();
                }
            }
    
            public bool Remove(TKey key) {
                return dict.Remove(key);
            }
    
            public bool TryGetValue(TKey key, out TValue value) {
                return dict.TryGetValue(key, out value);
            }
    
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
            public ICollection<TValue> Values {
                get {
                    // TODO:
                    throw new NotImplementedException();
                }
            }
    
            public TValue this[TKey key] {
                get {
                    return dict[key];
                }
                set {
                    // If the WeakHash already holds this value as a key, it will lead to a circular-reference and result
                    // in the objects being kept alive forever. The caller needs to ensure that this cannot happen.
                    Debug.Assert(!dict.ContainsKey(value));
    
                    dict[new WeakObject<TKey>(key)] = value;
                }
            }
    
            /// <summary>
            /// Check if any of the keys have gotten collected
            /// 
            /// Currently, there is also no guarantee of how long the values will be kept alive even after the keys
            /// get collected. This could be fixed by triggerring CheckCleanup() to be called on every garbage-collection
            /// by having a dummy watch-dog object with a finalizer which calls CheckCleanup().
            /// </summary>
            void CheckCleanup() {
                version++;
    
                long change = version - cleanupVersion;
    
                // Cleanup the table if it is a while since we have done it last time.
                // Take the size of the table into account.
                if (change > 1234 + dict.Count / 2) {
                    // It makes sense to do the cleanup only if a GC has happened in the meantime.
                    // WeakReferences can become zero only during the GC.
    
                    bool garbage_collected;
    #if SILVERLIGHT // GC.CollectionCount
                    garbage_collected = !cleanupGC.IsAlive;
                    if (garbage_collected) cleanupGC = new WeakReference(new object());
    #else
                    int currentGC = GC.CollectionCount(0);
                    garbage_collected = currentGC != cleanupGC;
                    if (garbage_collected) cleanupGC = currentGC;
    #endif
                    if (garbage_collected) {
                        Cleanup();
                        cleanupVersion = version;
                    } else {
                        cleanupVersion += 1234;
                    }
                }
            }
    
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
            private void Cleanup() {
    
                int liveCount = 0;
                int emptyCount = 0;
    
                foreach (WeakObject<TKey> w in dict.Keys) {
                    if (w.Target != null)
                        liveCount++;
                    else
                        emptyCount++;
                }
    
                // Rehash the table if there is a significant number of empty slots
                if (emptyCount > liveCount / 4) {
                    Dictionary<object, TValue> newtable = new Dictionary<object, TValue>(liveCount + liveCount / 4, comparer);
    
                    foreach (WeakObject<TKey> w in dict.Keys) {
                        object target = w.Target;
    
                        if (target != null)
                            newtable[w] = dict[w];
    
                    }
    
                    dict = newtable;
                }
            }
            #endregion
    
            #region ICollection<KeyValuePair<TKey,TValue>> Members
    
            public void Add(KeyValuePair<TKey, TValue> item) {
                // TODO:
                throw new NotImplementedException();
            }
    
            public void Clear() {
                // TODO:
                throw new NotImplementedException();
            }
    
            public bool Contains(KeyValuePair<TKey, TValue> item) {
                // TODO:
                throw new NotImplementedException();
            }
    
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
                // TODO:
                throw new NotImplementedException();
            }
    
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
            public int Count {
                get {
                    // TODO:
                    throw new NotImplementedException();
                }
            }
    
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
            public bool IsReadOnly {
                get {
                    // TODO:
                    throw new NotImplementedException();
                }
            }
    
            public bool Remove(KeyValuePair<TKey, TValue> item) {
                // TODO:
                throw new NotImplementedException();
            }
    
            #endregion
    
            #region IEnumerable<KeyValuePair<TKey,TValue>> Members
    
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
                // TODO:
                throw new NotImplementedException();
            }
    
            #endregion
    
            #region IEnumerable Members
    
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                // TODO:
                throw new NotImplementedException();
            }
    
            #endregion
            
            // WeakComparer treats WeakObject as transparent envelope
            sealed class WeakComparer<T> : IEqualityComparer<T> {
                bool IEqualityComparer<T>.Equals(T x, T y) {
                    WeakObject<T> wx = x as WeakObject<T>;
                    if (wx != null)
                        x = wx.Target;
        
                    WeakObject<T> wy = y as WeakObject<T>;
                    if (wy != null)
                        y = wy.Target;
        
                    return Object.Equals(x, y);
                }
        
                int IEqualityComparer<T>.GetHashCode(T obj) {
                    WeakObject<T> wobj = obj as WeakObject<T>;
                    if (wobj != null)
                        return wobj.GetHashCode();
        
                    return (obj == null) ? 0 : obj.GetHashCode();
                }
            }
            
            internal class WeakObject<T> {
                WeakReference weakReference;
                int hashCode;
        
                public WeakObject(T obj) {
                    weakReference = new WeakReference(obj, true);
                    hashCode = (obj == null) ? 0 : obj.GetHashCode();
                }
        
                public T Target {
                    get {
                        return (T)weakReference.Target;
                    }
                }
        
                public override int GetHashCode() {
                    return hashCode;
                }
        
                public override bool Equals(object obj) {
                    object target = weakReference.Target;
                    if (target == null) {
                        return false;
                    }
        
                    return ((T)target).Equals(obj);
                }
            }
        }
        
#pragma warning disable 612
        private static WeakDictionary<Expression, ExtensionInfo> _legacyCtorSupportTable;
#pragma warning restore 612

        // LinqV1 ctor
        [Obsolete("use a different constructor that does not take ExpressionType.  Then override GetExpressionType and GetNodeKind to provide the values that would be specified to this constructor.")]
        protected Expression(ExpressionType nodeType, Type type) {
            // Can't enforce anything that V1 didn't
            if(_legacyCtorSupportTable == null) {
                Interlocked.CompareExchange(
                    ref _legacyCtorSupportTable, 
                    new WeakDictionary<Expression, ExtensionInfo>(),
                    null
                );
            }

            _legacyCtorSupportTable[this] = new ExtensionInfo(nodeType, type);
        }
#endif

        protected Expression() {
        }
        
        //CONFORMING
        public ExpressionType NodeType {
            get { return GetNodeKind(); }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type {
            get { return GetExpressionType(); }
        }
        
        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this 
        /// returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        public virtual bool CanReduce {
            get { return false; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual ExpressionType GetNodeKind() {
#if !MICROSOFT_SCRIPTING_CORE
            ExtensionInfo extInfo;
            if (_legacyCtorSupportTable.TryGetValue(this, out extInfo)) {
                return extInfo.NodeType;
            }
#endif

            // the base type failed to overload GetNodeKind
            throw new InvalidOperationException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual Type GetExpressionType() {            
#if !MICROSOFT_SCRIPTING_CORE
            ExtensionInfo extInfo;
            if (_legacyCtorSupportTable.TryGetValue(this, out extInfo)) {
                return extInfo.Type;
            }
#endif

            // the base type failed to overload GetExpressionType
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If CanReduce returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public virtual Expression Reduce() {
            ContractUtils.Requires(!CanReduce, "this", Strings.ReducibleMustOverrideReduce);
            return this;
        }

        /// <summary>
        /// Override this to provide logic to walk the node's children. A
        /// typical implementation will call visitor.Visit on each of its
        /// children, and if any of them change, should return a new copy of
        /// itself with the modified children.
        /// 
        /// The default implementation will reduce the node and then walk it
        /// This will throw an exception if the node is not reducible
        /// </summary>
        protected internal virtual Expression VisitChildren(ExpressionVisitor visitor) {
            ContractUtils.Requires(CanReduce, "this", Strings.MustBeReducible);
            return visitor.Visit(ReduceExtensions());
        }

        // Visitor pattern: this is the method that dispatches back to the visitor
        // NOTE: this is unlike the Visit method, which provides a hook for
        // derived classes to extend the visitor framework to be able to walk
        // themselves
        internal virtual Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitExtension(this);
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If CanReduce returns
        /// true, this should return a valid expression. This method is
        /// allowed to return another node which itself must be reduced.
        /// 
        /// Unlike Reduce, this method checks that the reduced node satisfies
        /// certain invaraints.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public Expression ReduceAndCheck() {
            ContractUtils.Requires(CanReduce, "this", Strings.MustBeReducible);

            var newNode = Reduce();

            // 1. Reduction must return a new, non-null node
            // 2. Reduction must return a new node whose result type can be assigned to the type of the original node
            ContractUtils.Requires(newNode != null && newNode != this, "this", Strings.MustReduceToDifferent);
            ContractUtils.Requires(TypeUtils.AreReferenceAssignable(Type, newNode.Type), "this", Strings.ReducedNotCompatible);
            return newNode;
        }

        /// <summary>
        /// Reduces the expression to a known node type (i.e. not an Extension node)
        /// or simply returns the expression if it is already a known type.
        /// </summary>
        /// <returns>the reduced expression</returns>
        public Expression ReduceExtensions() {
            var node = this;
            while (node.NodeType == ExpressionType.Extension) {
                node = node.ReduceAndCheck();
            }
            return node;
        }

        //CONFORMING
        public override string ToString() {
            return ExpressionStringBuilder.ExpressionToString(this);
        }

#if MICROSOFT_SCRIPTING_CORE
        public string Dump {
            get {
                using (System.IO.StringWriter writer = new System.IO.StringWriter(CultureInfo.CurrentCulture)) {
                    ExpressionWriter.Dump(this, GetType().Name, writer);
                    return writer.ToString();
                }
            }
        }

        public void DumpExpression(string descr, TextWriter writer) {
            ExpressionWriter.Dump(this, descr, writer);
        }
#endif

        /// <summary>
        /// Helper used for ensuring we only return 1 instance of a ReadOnlyCollection of T.
        /// 
        /// This is called from various methods where we internally hold onto an IList of T
        /// or a ROC of T.  We check to see if we've already returned a ROC of T and if so
        /// simply return the other one.  Otherwise we do a thread-safe replacement of hte
        /// list w/ a ROC which wraps it.
        /// 
        /// Ultimately this saves us from having to allocate a ReadOnlyCollection for our
        /// data types because the compiler is capable of going directly to the IList of T.
        /// </summary>
        internal static ReadOnlyCollection<T> ReturnReadOnly<T>(ref IList<T> collection) {
            IList<T> value = collection;

            // if it's already read-only just return it.
            ReadOnlyCollection<T> res = value as ReadOnlyCollection<T>;
            if (res != null) {
                return res;
            }
            
            // otherwise make sure only ROC every gets exposed
            Interlocked.CompareExchange<IList<T>>(
                ref collection,
                value.ToReadOnly(),
                value 
            );

            // and return it
            return (ReadOnlyCollection<T>)collection;
        }        

        /// <summary>
        /// Helper used for ensuring we only return 1 instance of a ReadOnlyCollection of T.
        /// 
        /// This is similar to the ReturnReadOnly of T. This version supports nodes which hold 
        /// onto multiple Expressions where one is typed to object.  That object field holds either
        /// an expression or a ReadOnlyCollection of Expressions.  When it holds a ReadOnlyCollection
        /// the IList which backs it is a ListArgumentProvider which uses the Expression which
        /// implements IArgumentProvider to get 2nd and additional values.  The ListArgumentProvider 
        /// continues to hold onto the 1st expression.  
        /// 
        /// This enables users to get the ReadOnlyCollection w/o it consuming more memory than if 
        /// it was just an array.  Meanwhile The DLR internally avoids accessing  which would force 
        /// the ROC to be created resulting in a typical memory savings.
        /// </summary>
        internal static ReadOnlyCollection<Expression> ReturnReadOnly(IArgumentProvider provider, ref object collection) {
            Expression tObj = collection as Expression;
            if (tObj != null) {
                // otherwise make sure only one ROC ever gets exposed
                Interlocked.CompareExchange(
                    ref collection,
                    new ReadOnlyCollection<Expression>(new ListArgumentProvider(provider, tObj)),
                    tObj
                );
            }

            // and return what is not guaranteed to be a ROC
            return (ReadOnlyCollection<Expression>)collection;
        }        

        /// <summary>
        /// Helper which is used for specialized subtypes which use ReturnReadOnly(ref object, ...). 
        /// This is the reverse version of ReturnReadOnly which takes an IArgumentProvider.
        /// 
        /// This is used to return the 1st argument.  The 1st argument is typed as object and either
        /// contains a ReadOnlyCollection or the Expression.  We check for the Expression and if it's
        /// present we return that, otherwise we return the 1st element of the ReadOnlyCollection.
        /// </summary>
        internal static T ReturnObject<T>(object collectionOrT) where T : class {
            T t = collectionOrT as T;
            if (t != null) {
                return t;
            }

            return ((ReadOnlyCollection<T>)collectionOrT)[0];
        }

        private static void RequiresCanRead(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }

            // validate that we can read the node
            switch (expression.NodeType) {
                case ExpressionType.Index:
                    IndexExpression index = (IndexExpression)expression;
                    if (index.Indexer != null && !index.Indexer.CanRead) {
                        throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
                    }
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression member = (MemberExpression)expression;
                    MemberInfo memberInfo = member.Member;
                    if (memberInfo.MemberType == MemberTypes.Property) {
                        PropertyInfo prop = (PropertyInfo)memberInfo;
                        if (!prop.CanRead) {
                            throw new ArgumentException(Strings.ExpressionMustBeReadable, paramName);
                        }
                    }
                    break;
            }
        }

        private static void RequiresCanRead(IEnumerable<Expression> items, string paramName) {
            if (items != null) {
                // this is called a lot, avoid allocating an enumerator if we can...
                IList<Expression> listItems = items as IList<Expression>;
                if (listItems != null) {
                    for (int i = 0; i < listItems.Count; i++) {
                        RequiresCanRead(listItems[i], paramName);
                    }
                    return;
                }

                foreach (var i in items) {
                    RequiresCanRead(i, paramName);
                }
            }
        }
        private static void RequiresCanWrite(Expression expression, string paramName) {
            if (expression == null) {
                throw new ArgumentNullException(paramName);
            }

            bool canWrite = false;
            switch (expression.NodeType) {
                case ExpressionType.Index:
                    IndexExpression index = (IndexExpression)expression;
                    if (index.Indexer != null) {
                        canWrite = index.Indexer.CanWrite;
                    } else {
                        canWrite = true;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression member = (MemberExpression)expression;
                    switch (member.Member.MemberType) {
                        case MemberTypes.Property:
                            PropertyInfo prop = (PropertyInfo)member.Member;
                            canWrite = prop.CanWrite;
                            break;
                        case MemberTypes.Field:
                            FieldInfo field = (FieldInfo)member.Member;
                            canWrite = !(field.IsInitOnly || field.IsLiteral);
                            break;
                    }
                    break;
                case ExpressionType.Parameter:
                case ExpressionType.ArrayIndex:
                    canWrite = true;
                    break;
            }

            if (!canWrite) {
                throw new ArgumentException(Strings.ExpressionMustBeWriteable, paramName);
            }
        }

#if !MICROSOFT_SCRIPTING_CORE
        struct ExtensionInfo {
            public ExtensionInfo(ExpressionType nodeType, Type type) {
                NodeType = nodeType;
                Type = type;
            }

            internal readonly ExpressionType NodeType;
            internal readonly Type Type;
        }
#endif
    }
}
