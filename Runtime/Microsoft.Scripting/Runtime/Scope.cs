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

#if !CLR2
using System.Linq.Expressions;
#else
using dynamic = System.Object;
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using Microsoft.Scripting.Utils;
using System.Dynamic;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Represents a host-provided variables for executable code.  The variables are
    /// typically backed by a host-provided dictionary. Languages can also associate per-language
    /// information with the context by using scope extensions.  This can be used for tracking
    /// state which is used across multiple executions, for providing custom forms of 
    /// storage (for example object keyed access), or other language specific semantics.
    /// 
    /// Scope objects are thread-safe as long as their underlying storage is thread safe.
    /// 
    /// Script hosts can choose to use thread safe or thread unsafe modules but must be sure
    /// to constrain the code they right to be single-threaded if using thread unsafe
    /// storage.
    /// </summary>
    public sealed class Scope : IDynamicMetaObjectProvider {
        private ScopeExtension[] _extensions; // resizable
        private readonly IDynamicMetaObjectProvider _storage;
        private static readonly object _getFailed = new object();

        /// <summary>
        /// Creates a new scope with a new empty thread-safe dictionary.  
        /// </summary>
        public Scope() {
            _extensions = ScopeExtension.EmptyArray;
            _storage = new ScopeStorage();
        }

        /// <summary>
        /// Creates a new scope with the provided dictionary.
        /// </summary>
        [Obsolete("IAttributesCollection is obsolete, use Scope(IDynamicMetaObjectProvider) overload instead")]
        public Scope(IAttributesCollection dictionary) {
            _extensions = ScopeExtension.EmptyArray;
            _storage = new AttributesAdapter(dictionary);
        }

        /// <summary>
        /// Creates a new scope which is backed by an arbitrary object for it's storage.
        /// </summary>
        /// <param name="storage"></param>
        public Scope(IDynamicMetaObjectProvider storage) {
            _extensions = ScopeExtension.EmptyArray;
            _storage = storage;
        }

        /// <summary>
        /// Gets the ScopeExtension associated with the provided ContextId.
        /// </summary>
        public ScopeExtension GetExtension(ContextId languageContextId) {
            return (languageContextId.Id < _extensions.Length) ? _extensions[languageContextId.Id] : null;
        }
        
        /// <summary>
        /// Sets the ScopeExtension to the provided value for the given ContextId.  
        /// 
        /// The extension can only be set once.  The returned value is either the new ScopeExtension
        /// if no value was previously set or the previous value.
        /// </summary>
        public ScopeExtension SetExtension(ContextId languageContextId, ScopeExtension extension) {
            ContractUtils.RequiresNotNull(extension, "extension");

            lock (_extensions) {
                if (languageContextId.Id >= _extensions.Length) {
                    Array.Resize(ref _extensions, languageContextId.Id + 1);
                }

                return _extensions[languageContextId.Id] ?? (_extensions[languageContextId.Id] = extension);
            }
        }

        public dynamic Storage {
            get {
                return _storage;
            }
        }

        internal sealed class MetaScope : DynamicMetaObject {
            public MetaScope(Expression parameter, Scope scope)
                : base(parameter, BindingRestrictions.Empty, scope) {
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                return Restrict(binder.Bind(StorageMetaObject, DynamicMetaObject.EmptyMetaObjects));
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                return Restrict(binder.Bind(StorageMetaObject, args));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {                
                return Restrict(binder.Bind(StorageMetaObject, new DynamicMetaObject[] { value }));
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                return Restrict(binder.Bind(StorageMetaObject, DynamicMetaObject.EmptyMetaObjects));
            }

            private DynamicMetaObject Restrict(DynamicMetaObject result) {
                if (Expression.Type == typeof(Scope)) {
                    // ideal binding, we add no new restrictions if we're binding against a strongly typed Scope
                    return result;
                }

                // Un-ideal binding: we add restrictions.
                return new DynamicMetaObject(result.Expression, BindingRestrictions.GetTypeRestriction(Expression, typeof(Scope)).Merge(result.Restrictions));
            }

            private DynamicMetaObject StorageMetaObject {
                get {
                    return DynamicMetaObject.Create(Value._storage, StorageExpression);
                }
            }

            private MemberExpression StorageExpression {
                get {
                    return Expression.Property(
                        Expression.Convert(Expression, typeof(Scope)),
                        typeof(Scope).GetProperty("Storage")
                    );
                }
            }

            public override IEnumerable<string> GetDynamicMemberNames() {
                return StorageMetaObject.GetDynamicMemberNames();
            }

            public new Scope Value {
                get {
                    return (Scope)base.Value;
                }
            }
        }

        #region IDynamicMetaObjectProvider Members

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new MetaScope(parameter, this);
        }

        #endregion

        internal sealed class AttributesAdapter : IDynamicMetaObjectProvider {
            private readonly IAttributesCollection _data;

            public AttributesAdapter(IAttributesCollection data) {
                _data = data;
            }

            private static object TryGetMember(object adapter, SymbolId name) {
                object result;
                if (((AttributesAdapter)adapter)._data.TryGetValue(name, out result)) {
                    return result;
                }
                return _getFailed;
            }

            private static void TrySetMember(object adapter, SymbolId name, object value) {
                ((AttributesAdapter)adapter)._data[name] = value;
            }

            private static bool TryDeleteMember(object adapter, SymbolId name) {
                return ((AttributesAdapter)adapter)._data.Remove(name);
            }

            #region IDynamicMetaObjectProvider Members

            DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
                return new Meta(parameter, this);
            }

            #endregion

            internal sealed class Meta : DynamicMetaObject {
                public Meta(Expression parameter, AttributesAdapter storage)
                    : base(parameter, BindingRestrictions.Empty, storage) {
                }

                public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                    return DynamicTryGetMember(binder.Name,
                        binder.FallbackGetMember(this).Expression,
                        (tmp) => tmp
                    );
                }

                public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                    return DynamicTryGetMember(binder.Name, 
                        binder.FallbackInvokeMember(this, args).Expression,
                        (tmp) => binder.FallbackInvoke(new DynamicMetaObject(tmp, BindingRestrictions.Empty), args, null).Expression
                    );
                }

                private DynamicMetaObject DynamicTryGetMember(string name, Expression fallback, Func<Expression, Expression> resultOp) {
                    var tmp = Expression.Parameter(typeof(object));
                    return new DynamicMetaObject(
                        Expression.Block(
                            new[] { tmp },
                            Expression.Condition(
                                Expression.NotEqual(
                                    Expression.Assign(
                                        tmp,
                                        Expression.Invoke(
                                            Expression.Constant(new Func<object, SymbolId, object>(AttributesAdapter.TryGetMember)),
                                            Expression,
                                            Expression.Constant(SymbolTable.StringToId(name))
                                        )
                                    ),
                                    Expression.Constant(_getFailed)
                                ),
                                ExpressionUtils.Convert(resultOp(tmp), typeof(object)),
                                ExpressionUtils.Convert(fallback, typeof(object))
                            )
                        ),
                        GetRestrictions()
                    );
                }

                private BindingRestrictions GetRestrictions() {
                    return BindingRestrictions.GetTypeRestriction(Expression, typeof(AttributesAdapter));
                }

                public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                    return new DynamicMetaObject(
                        Expression.Block(
                            Expression.Invoke(
                                Expression.Constant(new Action<object, SymbolId, object>(AttributesAdapter.TrySetMember)),
                                Expression,
                                Expression.Constant(SymbolTable.StringToId(binder.Name)),
                                Expression.Convert(
                                    value.Expression,
                                    typeof(object)
                                )
                            ),
                            value.Expression
                        ),
                        GetRestrictions()
                    );
                }

                public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                    return new DynamicMetaObject(
                        Expression.Condition(
                            Expression.Invoke(
                                Expression.Constant(new Func<object, SymbolId, bool>(AttributesAdapter.TryDeleteMember)),
                                Expression,
                                Expression.Constant(SymbolTable.StringToId(binder.Name))
                            ),
                            Expression.Default(binder.ReturnType),
                            binder.FallbackDeleteMember(this).Expression
                        ),
                        GetRestrictions()
                    );
                }

                public new AttributesAdapter Value {
                    get {
                        return (AttributesAdapter)base.Value;
                    }
                }

                public override IEnumerable<string> GetDynamicMemberNames() {
                    foreach (object o in Value._data.Keys) {
                        if (o is string) {
                            yield return (string)o;
                        }
                    }
                }
            }

        }
    }
}
