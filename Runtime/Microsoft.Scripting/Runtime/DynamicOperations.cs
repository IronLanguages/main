/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {

    /// <summary>
    /// ObjectOperations provide a large catalogue of object operations such as member access, conversions, 
    /// indexing, and things like addition.  There are several introspection and tool support services available
    /// for more advanced hosts.  
    /// 
    /// You get ObjectOperation instances from ScriptEngine, and they are bound to their engines for the semantics 
    /// of the operations.  There is a default instance of ObjectOperations you can share across all uses of the 
    /// engine.  However, very advanced hosts can create new instances.
    /// </summary>
    public sealed class DynamicOperations {
        private readonly LanguageContext _lc;

        /// <summary> a dictionary of SiteKey's which are used to cache frequently used operations, logically a set </summary>
        private Dictionary<SiteKey, SiteKey> _sites = new Dictionary<SiteKey, SiteKey>();

        /// <summary> the # of sites we had created at the last cleanup </summary>
        private int LastCleanup;

        /// <summary> the total number of sites we've ever created </summary>
        private int SitesCreated;

        /// <summary> the number of sites required before we'll try cleaning up the cache... </summary>
        private const int CleanupThreshold = 20;

        /// <summary> the minimum difference between the average that is required to remove </summary>
        private const int RemoveThreshold = 2;

        /// <summary> the maximum number we'll remove on a single cache cleanup </summary>
        private const int StopCleanupThreshold = CleanupThreshold / 2;

        /// <summary> the number of sites we should clear after if we can't make progress cleaning up otherwise </summary>
        private const int ClearThreshold = 50;

        public DynamicOperations(LanguageContext lc) {
            ContractUtils.RequiresNotNull(lc, "lc");
            _lc = lc;
        }

        #region Basic Operations

        private Dictionary<int, Func<DynamicOperations, CallSiteBinder, object, object[], object>> _invokers = new Dictionary<int, Func<DynamicOperations, CallSiteBinder, object, object[], object>>();

        /// <summary>
        /// Calls the provided object with the given parameters and returns the result.
        /// 
        /// The prefered way of calling objects is to convert the object to a strongly typed delegate 
        /// using the ConvertTo methods and then invoking that delegate.
        /// </summary>
        public object Invoke(object obj, params object[] parameters) {
            return GetInvoker(parameters.Length)(this, _lc.CreateInvokeBinder(new CallInfo(parameters.Length)), obj, parameters);
        }
        
        /// <summary>
        /// Invokes a member on the provided object with the given parameters and returns the result.
        /// </summary>
        public object InvokeMember(object obj, string memberName, params object[] parameters) {
            return InvokeMember(obj, memberName, false, parameters);
        }

        /// <summary>
        /// Invokes a member on the provided object with the given parameters and returns the result.
        /// </summary>
        public object InvokeMember(object obj, string memberName, bool ignoreCase, params object[] parameters) {
            return GetInvoker(parameters.Length)(this, _lc.CreateCallBinder(memberName, ignoreCase, new CallInfo(parameters.Length)), obj, parameters);
        }

        /// <summary>
        /// Creates a new instance from the provided object using the given parameters, and returns the result.
        /// </summary>
        public object CreateInstance(object obj, params object[] parameters) {
            return GetInvoker(parameters.Length)(this, _lc.CreateCreateBinder(new CallInfo(parameters.Length)), obj, parameters);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Throws an exception if the member does not exist or is write-only.
        /// </summary>
        public object GetMember(object obj, string name) {
            return GetMember(obj, name, false);
        }

        /// <summary>
        /// Gets the member name from the object obj and converts it to the type T.  Throws an exception if the
        /// member does not exist, is write-only, or cannot be converted.
        /// </summary>
        public T GetMember<T>(object obj, string name) {
            return GetMember<T>(obj, name, false);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Returns true if the member is successfully retrieved and 
        /// stores the value in the value out param.
        /// </summary>
        public bool TryGetMember(object obj, string name, out object value) {
            return TryGetMember(obj, name, false, out value);
        }

        /// <summary>
        /// Returns true if the object has a member named name, false if the member does not exist.
        /// </summary>
        public bool ContainsMember(object obj, string name) {
            return ContainsMember(obj, name, false);
        }

        /// <summary>
        /// Removes the member name from the object obj.
        /// </summary>
        public void RemoveMember(object obj, string name) {
            RemoveMember(obj, name, false);
        }

        /// <summary>
        /// Sets the member name on object obj to value.
        /// </summary>
        public void SetMember(object obj, string name, object value) {
            SetMember(obj, name, value, false);
        }

        /// <summary>
        /// Sets the member name on object obj to value.  This overload can be used to avoid
        /// boxing and casting of strongly typed members.
        /// </summary>
        public void SetMember<T>(object obj, string name, T value) {
            SetMember<T>(obj, name, value, false);
        }

        /// <summary>
        /// Gets the member name from the object obj.  Throws an exception if the member does not exist or is write-only.
        /// </summary>
        public object GetMember(object obj, string name, bool ignoreCase) {
            CallSite<Func<CallSite, object, object>> site;
            site = GetOrCreateSite<object, object>(_lc.CreateGetMemberBinder(name, ignoreCase));
            return site.Target(site, obj);
        }

        /// <summary>
        /// Gets the member name from the object obj and converts it to the type T. The conversion will be explicit or implicit
        /// depending on what the langauge prefers. Throws an exception if the member does not exist, is write-only, or cannot be converted.
        /// </summary>
        public T GetMember<T>(object obj, string name, bool ignoreCase) {
            CallSite<Func<CallSite, object, T>> convertSite = GetOrCreateSite<object, T>(_lc.CreateConvertBinder(typeof(T), null));
            CallSite<Func<CallSite, object, object>> site = GetOrCreateSite<object, object>(_lc.CreateGetMemberBinder(name, ignoreCase));
            return convertSite.Target(convertSite, site.Target(site, obj));
        }

        /// <summary>
        /// Gets the member name from the object obj.  Returns true if the member is successfully retrieved and 
        /// stores the value in the value out param.
        /// </summary>
        public bool TryGetMember(object obj, string name, bool ignoreCase, out object value) {
            try {
                value = GetMember(obj, name, ignoreCase);
                return true;
            } catch (MissingMemberException) {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Returns true if the object has a member named name, false if the member does not exist.
        /// </summary>
        public bool ContainsMember(object obj, string name, bool ignoreCase) {
            object dummy;
            return TryGetMember(obj, name, ignoreCase, out dummy);
        }

        /// <summary>
        /// Removes the member name from the object obj.  Returns true if the member was successfully removed
        /// or false if the member does not exist.
        /// </summary>
        public void RemoveMember(object obj, string name, bool ignoreCase) {
            CallSite<Action<CallSite, object>> site;
            site = GetOrCreateActionSite<object>(_lc.CreateDeleteMemberBinder(name, ignoreCase));
            site.Target(site, obj);
        }

        /// <summary>
        /// Sets the member name on object obj to value.
        /// </summary>
        public void SetMember(object obj, string name, object value, bool ignoreCase) {
            CallSite<Func<CallSite, object, object, object>> site;
            site = GetOrCreateSite<object, object, object>(_lc.CreateSetMemberBinder(name, ignoreCase));
            site.Target(site, obj, value);
        }

        /// <summary>
        /// Sets the member name on object obj to value.  This overload can be used to avoid
        /// boxing and casting of strongly typed members.
        /// </summary>
        public void SetMember<T>(object obj, string name, T value, bool ignoreCase) {
            CallSite<Func<CallSite, object, T, object>> site;
            site = GetOrCreateSite<object, T, object>(_lc.CreateSetMemberBinder(name, ignoreCase));
            site.Target(site, obj, value);
        }

        /// <summary>
        /// Converts the object obj to the type T.  The conversion will be explicit or implicit
        /// depending on what the langauge prefers.
        /// </summary>
        public T ConvertTo<T>(object obj) {
            CallSite<Func<CallSite, object, T>> site;
            site = GetOrCreateSite<object, T>(_lc.CreateConvertBinder(typeof(T), null));
            return site.Target(site, obj);
        }

        /// <summary> 
        /// Converts the object obj to the type type.  The conversion will be explicit or implicit
        /// depending on what the langauge prefers.
        /// </summary>
        public object ConvertTo(object obj, Type type) {
            if (type.IsInterface || type.IsClass) {
                CallSite<Func<CallSite, object, object>> site;
                site = GetOrCreateSite<object, object>(_lc.CreateConvertBinder(type, null));
                return site.Target(site, obj);
            }

            // TODO: We should probably cache these instead of using reflection all the time.
            foreach (MethodInfo mi in typeof(DynamicOperations).GetMember("ConvertTo")) {
                if (mi.IsGenericMethod) {
                    try {
                        return mi.MakeGenericMethod(type).Invoke(this, new object[] { obj });
                    } catch(TargetInvocationException tie) {
                        throw tie.InnerException;
                    }
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Converts the object obj to the type T.  Returns true if the value can be converted, false if it cannot.
        /// 
        /// The conversion will be explicit or implicit depending on what the langauge prefers.
        /// </summary>
        public bool TryConvertTo<T>(object obj, out T result) {
            try {
                result = ConvertTo<T>(obj);
                return true;
            } catch (ArgumentTypeException) {
                result = default(T);
                return false;
            } catch (InvalidCastException) {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Converts the object obj to the type type.  Returns true if the value can be converted, false if it cannot.
        /// 
        /// The conversion will be explicit or implicit depending on what the langauge prefers.
        /// </summary>
        public bool TryConvertTo(object obj, Type type, out object result) {
            try {
                result = ConvertTo(obj, type);
                return true;
            } catch (ArgumentTypeException) {
                result = null;
                return false;
            } catch (InvalidCastException) {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Convers the object obj to the type T including explicit conversions which may lose information.
        /// </summary>
        public T ExplicitConvertTo<T>(object obj) {
            CallSite<Func<CallSite, object, T>> site;
            site = GetOrCreateSite<object, T>(_lc.CreateConvertBinder(typeof(T), true));
            return site.Target(site, obj);
        }

        /// <summary>
        /// Converts the object obj to the type type including explicit conversions which may lose information.
        /// </summary>
        public object ExplicitConvertTo(object obj, Type type) {
            CallSite<Func<CallSite, object, object>> site;
            site = GetOrCreateSite<object, object>(_lc.CreateConvertBinder(type, true));
            return site.Target(site, obj);
        }

        /// <summary>
        /// Converts the object obj to the type type including explicit conversions which may lose information.  
        /// 
        /// Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryExplicitConvertTo(object obj, Type type, out object result) {
            try {
                result = ExplicitConvertTo(obj, type);
                return true;
            } catch (ArgumentTypeException) {
                result = null;
                return false;
            } catch (InvalidCastException) {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Converts the object obj to the type T.  Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryExplicitConvertTo<T>(object obj, out T result) {
            try {
                result = ExplicitConvertTo<T>(obj);
                return true;
            } catch (ArgumentTypeException) {
                result = default(T);
                return false;
            } catch (InvalidCastException) {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Convers the object obj to the type T including implicit conversions.
        /// </summary>
        public T ImplicitConvertTo<T>(object obj) {
            CallSite<Func<CallSite, object, T>> site;
            site = GetOrCreateSite<object, T>(_lc.CreateConvertBinder(typeof(T), false));
            return site.Target(site, obj);
        }

        /// <summary>
        /// Converts the object obj to the type type including implicit conversions.
        /// </summary>
        public object ImplicitConvertTo(object obj, Type type) {
            CallSite<Func<CallSite, object, object>> site;
            site = GetOrCreateSite<object, object>(_lc.CreateConvertBinder(type, false));
            return site.Target(site, obj);
        }

        /// <summary>
        /// Converts the object obj to the type type including implicit conversions. 
        /// 
        /// Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryImplicitConvertTo(object obj, Type type, out object result) {
            try {
                result = ImplicitConvertTo(obj, type);
                return true;
            } catch (ArgumentTypeException) {
                result = null;
                return false;
            } catch (InvalidCastException) {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Converts the object obj to the type T.  Returns true if the value can be converted, false if it cannot.
        /// </summary>
        public bool TryImplicitConvertTo<T>(object obj, out T result) {
            try {
                result = ImplicitConvertTo<T>(obj);
                return true;
            } catch (ArgumentTypeException) {
                result = default(T);
                return false;
            } catch (InvalidCastException) {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Performs a generic unary operation on the strongly typed target and returns the value as the specified type
        /// </summary>
        public TResult DoOperation<TTarget, TResult>(ExpressionType operation, TTarget target) {
            var site = GetOrCreateSite<TTarget, TResult>(_lc.CreateUnaryOperationBinder(operation));
            return site.Target(site, target);
        }

        /// <summary>
        /// Peforms the generic binary operation on the specified strongly typed targets and returns
        /// the strongly typed result.
        /// </summary>
        public TResult DoOperation<TTarget, TOther, TResult>(ExpressionType operation, TTarget target, TOther other) {
            var site = GetOrCreateSite<TTarget, TOther, TResult>(_lc.CreateBinaryOperationBinder(operation));
            return site.Target(site, target, other);
        }
        
        public string GetDocumentation(object o) {
            return _lc.GetDocumentation(o);
        }

        public IList<string> GetCallSignatures(object o) {
            return _lc.GetCallSignatures(o);
        }

        public bool IsCallable(object o) {
            return _lc.IsCallable(o);
        }
        
        /// <summary>
        /// Returns a list of strings which contain the known members of the object.
        /// </summary>
        public IList<string> GetMemberNames(object obj) {
            return _lc.GetMemberNames(obj);
        }

        /// <summary>
        /// Returns a string representation of the object in a language specific object display format.
        /// </summary>
        public string Format(object obj) {
            return _lc.FormatObject(this, obj);
        }

        #endregion

        #region Private implementation details

        /// <summary>
        /// Gets or creates a dynamic site w/ the specified type parameters for the provided binder.
        /// </summary>
        /// <remarks>
        /// This will either get the site from the cache or create a new site and return it. The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </remarks>
        public CallSite<Func<CallSite, T1, TResult>> GetOrCreateSite<T1, TResult>(CallSiteBinder siteBinder) {
            return GetOrCreateSite<CallSite<Func<CallSite, T1, TResult>>>(siteBinder, CallSite<Func<CallSite, T1, TResult>>.Create);
        }

        /// <summary>
        /// Gets or creates a dynamic site w/ the specified type parameters for the provided binder.
        /// </summary>
        /// <remarks>
        /// This will either get the site from the cache or create a new site and return it. The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </remarks>
        public CallSite<Action<CallSite, T1>> GetOrCreateActionSite<T1>(CallSiteBinder siteBinder) {
            return GetOrCreateSite<CallSite<Action<CallSite, T1>>>(siteBinder, CallSite<Action<CallSite, T1>>.Create);
        }

        /// <summary>
        /// Gets or creates a dynamic site w/ the specified type parameters for the provided binder.
        /// </summary>
        /// <remarks>
        /// This will either get the site from the cache or create a new site and return it. The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </remarks>
        public CallSite<Func<CallSite, T1, T2, TResult>> GetOrCreateSite<T1, T2, TResult>(CallSiteBinder siteBinder) {
            return GetOrCreateSite<CallSite<Func<CallSite, T1, T2, TResult>>>(siteBinder, CallSite<Func<CallSite, T1, T2, TResult>>.Create);
        }

        /// <summary>
        /// Gets or creates a dynamic site w/ the specified type parameters for the provided binder.
        /// </summary>
        /// <remarks>
        /// This will either get the site from the cache or create a new site and return it. The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </remarks>
        public CallSite<Func<CallSite, T1, T2, T3, TResult>> GetOrCreateSite<T1, T2, T3, TResult>(CallSiteBinder siteBinder) {
            return GetOrCreateSite<CallSite<Func<CallSite, T1, T2, T3, TResult>>>(siteBinder, CallSite<Func<CallSite, T1, T2, T3, TResult>>.Create);
        }

        /// <summary>
        /// Gets or creates a dynamic site w/ the specified type parameters for the provided binder.
        /// </summary>
        /// <remarks>
        /// This will either get the site from the cache or create a new site and return it. The cache
        /// may be cleaned if it's gotten too big since the last usage.
        /// </remarks>
        public CallSite<TSiteFunc> GetOrCreateSite<TSiteFunc>(CallSiteBinder siteBinder) where TSiteFunc : class {
            return GetOrCreateSite<CallSite<TSiteFunc>>(siteBinder, CallSite<TSiteFunc>.Create);
        }

        /// <summary>
        /// Helper to create to get or create the dynamic site - called by the GetSite methods.
        /// </summary>
        private T GetOrCreateSite<T>(CallSiteBinder siteBinder, Func<CallSiteBinder, T> factory) where T : CallSite {
            SiteKey sk = new SiteKey(typeof(T), siteBinder);

            lock (_sites) {
                SiteKey old;
                if (!_sites.TryGetValue(sk, out old)) {
                    SitesCreated++;
                    if (SitesCreated < 0) {
                        // overflow, just reset back to zero...
                        SitesCreated = 0;
                        LastCleanup = 0;
                    }
                    sk.Site = factory(sk.SiteBinder);
                    _sites[sk] = sk;
                } else {
                    sk = old;
                }

                sk.HitCount++;

                CleanupNoLock();
            }

            return (T)sk.Site;
        }

        /// <summary>
        /// Removes items from the cache that have the lowest usage...
        /// </summary>
        private void CleanupNoLock() {
            // cleanup only if we have too many sites and we've created a bunch since our last cleanup
            if (_sites.Count > CleanupThreshold && (LastCleanup < SitesCreated - CleanupThreshold)) {
                LastCleanup = SitesCreated;

                // calculate the average use, remove up to StopCleanupThreshold that are below average.
                int totalUse = 0;
                foreach (SiteKey sk in _sites.Keys) {
                    totalUse += sk.HitCount;
                }

                int avgUse = totalUse / _sites.Count;
                if (avgUse == 1 && _sites.Count > ClearThreshold) {
                    // we only have a bunch of one-off requests
                    _sites.Clear();
                    return;
                }

                List<SiteKey> toRemove = null;
                foreach (SiteKey sk in _sites.Keys) {
                    if (sk.HitCount < (avgUse - RemoveThreshold)) {
                        if (toRemove == null) {
                            toRemove = new List<SiteKey>();
                        }

                        toRemove.Add(sk);
                        if (toRemove.Count > StopCleanupThreshold) {
                            // if we have a setup like weight(100), weight(1), weight(1), weight(1), ... we don't want
                            // to just run through and remove all of the weight(1)'s. 
                            break;
                        }
                    }

                }

                if (toRemove != null) {
                    foreach (SiteKey sk in toRemove) {
                        _sites.Remove(sk);
                    }

                    // reset all hit counts so the next time through is fair 
                    // to newly added members which may take precedence.
                    foreach (SiteKey sk in _sites.Keys) {
                        sk.HitCount = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Helper class for tracking all of our unique dynamic sites and their
        /// usage patterns.  We hash on the combination of the binder and site type.
        /// 
        /// We also track the hit count and the key holds the site associated w/ the 
        /// key.  Logically this is a set based upon the binder and site-type but we
        /// store it in a dictionary.
        /// </summary>
        private class SiteKey : IEquatable<SiteKey> {
            // the key portion of the data
            internal readonly CallSiteBinder SiteBinder;
            private readonly Type _siteType;

            // not used for equality, used for caching strategy
            public int HitCount;
            public CallSite Site;

            public SiteKey(Type siteType, CallSiteBinder siteBinder) {
                Debug.Assert(siteType != null);
                Debug.Assert(siteBinder != null);

                SiteBinder = siteBinder;
                _siteType = siteType;
            }

            [Confined]
            public override bool Equals(object obj) {
                return Equals(obj as SiteKey);
            }

            [Confined]
            public override int GetHashCode() {
                return SiteBinder.GetHashCode() ^ _siteType.GetHashCode();
            }

            #region IEquatable<SiteKey> Members

            [StateIndependent]
            public bool Equals(SiteKey other) {
                if (other == null) return false;

                return other.SiteBinder.Equals(SiteBinder) &&
                    other._siteType == _siteType;
            }

            #endregion
#if DEBUG
            [Confined]
            public override string ToString() {
                return String.Format("{0} {1}", SiteBinder.ToString(), HitCount);
            }
#endif
        }

        private Func<DynamicOperations, CallSiteBinder, object, object[], object> GetInvoker(int paramCount) {
            Func<DynamicOperations, CallSiteBinder, object, object[], object> invoker;
            lock (_invokers) {
                if (!_invokers.TryGetValue(paramCount, out invoker)) {
                    ParameterExpression dynOps = Expression.Parameter(typeof(DynamicOperations));
                    ParameterExpression callInfo = Expression.Parameter(typeof(CallSiteBinder));
                    ParameterExpression target = Expression.Parameter(typeof(object));
                    ParameterExpression args = Expression.Parameter(typeof(object[]));
                    Type funcType = DelegateUtils.GetObjectCallSiteDelegateType(paramCount);
                    ParameterExpression site = Expression.Parameter(typeof(CallSite<>).MakeGenericType(funcType));
                    Expression[] siteArgs = new Expression[paramCount + 2];
                    siteArgs[0] = site;
                    siteArgs[1] = target;
                    for (int i = 0; i < paramCount; i++) {
                        siteArgs[i + 2] = Expression.ArrayIndex(args, Expression.Constant(i));
                    }

                    var getOrCreateSiteFunc = new Func<CallSiteBinder, CallSite<Func<object>>>(GetOrCreateSite<Func<object>>).Method.GetGenericMethodDefinition();
                    _invokers[paramCount] = invoker = Expression.Lambda<Func<DynamicOperations, CallSiteBinder, object, object[], object>>(
                        Expression.Block(
                            new[] { site },
                            Expression.Assign(
                                site,
                                Expression.Call(dynOps, getOrCreateSiteFunc.MakeGenericMethod(funcType), callInfo)
                            ),
                            Expression.Invoke(
                                Expression.Field(
                                    site,
                                    site.Type.GetField("Target")
                                ),
                                siteArgs
                            )
                        ),
                        new[] { dynOps, callInfo, target, args }
                    ).Compile();
                }
            }
            return invoker;
        }
       
        #endregion
    }
}
