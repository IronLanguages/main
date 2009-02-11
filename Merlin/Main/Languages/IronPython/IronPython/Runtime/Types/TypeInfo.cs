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
using System.Diagnostics;
using System.Reflection;
using System.Dynamic;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Helpers for interacting w/ .NET types.  This includes:
    /// 
    ///     Member resolution via GetMember/GetMembers.  This performs a member lookup which includes the registered
    ///         extension types in the PythonBinder.  Internally the class has many MemberResolver's which provide
    ///         the various resolution behaviors.  
    ///     
    ///     Cached member access - this is via static classes such as Object and provides various MemberInfo's so we're
    ///         not constantly looking up via reflection.
    /// </summary>
    internal static partial class TypeInfo {
        /// <summary> list of resolvers which we run to resolve items </summary>
        private static readonly MemberResolver/*!*/[]/*!*/ _resolvers = MakeResolverTable();
        [MultiRuntimeAware]
        private static DocumentationDescriptor _docDescr;
        [MultiRuntimeAware]
        internal static Dictionary<string, PythonOperationKind> _pythonOperatorTable;

        #region Public member resolution

        /// <summary>
        /// Gets the statically known member from the type with the specific name.  Searches the entire type hierarchy to find the specified member.
        /// </summary>
        public static MemberGroup/*!*/ GetMemberAll(PythonBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
            Assert.NotNull(binder, action, type, name);

            PerfTrack.NoteEvent(PerfTrack.Categories.ReflectedTypes, String.Format("ResolveMember: {0} {1}", type.Name, name));
            return GetMemberGroup(new ResolveBinder(binder), action, type, name);
        }

        /// <summary>
        /// Gets all the statically known members from the specified type.  Searches the entire type hierarchy to get all possible members.
        /// 
        /// The result may include multiple resolution.  It is the callers responsibility to only treat the 1st one by name as existing.
        /// </summary>
        public static IList<ResolvedMember/*!*/>/*!*/ GetMembersAll(PythonBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
            Assert.NotNull(binder, action, type);

            return GetResolvedMembers(new ResolveBinder(binder), action, type);
        }

        /// <summary>
        /// Gets the statically known member from the type with the specific name.  Searches only the specified type to find the member.
        /// </summary>
        public static MemberGroup/*!*/ GetMember(PythonBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
            Assert.NotNull(binder, action, type, name);

            PerfTrack.NoteEvent(PerfTrack.Categories.ReflectedTypes, String.Format("LookupMember: {0} {1}", type.Name, name));
            return GetMemberGroup(new LookupBinder(binder), action, type, name);
        }

        /// <summary>
        /// Gets all the statically known members from the specified type.  Searches only the specified type to find the members.
        /// 
        /// The result may include multiple resolution.  It is the callers responsibility to only treat the 1st one by name as existing.
        /// </summary>
        public static IList<ResolvedMember/*!*/>/*!*/ GetMembers(PythonBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
            Assert.NotNull(binder, action, type);

            return GetResolvedMembers(new LookupBinder(binder), action, type);
        }

        #endregion

        #region Cached type members

        public static class _Object {
            public new static readonly MethodInfo/*!*/ GetType = typeof(object).GetMethod("GetType");
        }

        public static class _IPythonObject {
            public static readonly PropertyInfo/*!*/ PythonType = typeof(IPythonObject).GetProperty("PythonType");
            public static readonly PropertyInfo/*!*/ Dict = typeof(IPythonObject).GetProperty("Dict");
        }

        public static class _PythonOps {
            public static readonly MethodInfo/*!*/ SlotTryGetBoundValue = typeof(PythonOps).GetMethod("SlotTryGetBoundValue");
            public static readonly MethodInfo/*!*/ GetTypeVersion = typeof(PythonOps).GetMethod("GetTypeVersion");
            public static readonly MethodInfo/*!*/ CheckTypeVersion = typeof(PythonOps).GetMethod("CheckTypeVersion");
        }

        public static class _OperationFailed {
            public static readonly FieldInfo/*!*/ Value = typeof(OperationFailed).GetField("Value");
        }

        public static class _IAttributesCollection {
            public static readonly MethodInfo/*!*/ TryGetvalue = typeof(IAttributesCollection).GetMethod("TryGetValue");
        }

        public static class _PythonGenerator {
            public static readonly ConstructorInfo Ctor = typeof(PythonGenerator).GetConstructor(new Type[] { typeof(CodeContext) });
        }


        #endregion

        #region MemberResolver implementations and infrastructure

        /// <summary>
        /// Abstract class used for resolving members.  This provides two methods of member look.  The first is looking
        /// up a single member by name.  The other is getting all of the members.
        /// 
        /// There are various subclasses of this which have different methods of resolving the members.  The primary
        /// function of the resolvers are to provide the name->value lookup.  They also need to provide a simple name
        /// enumerator.  The enumerator is kept simple because it's allowed to return duplicate names as well as return
        /// names of members that don't exist.  The base MemberResolver will then verify their existance as well as 
        /// filter duplicates.
        /// </summary>
        abstract class MemberResolver {
            /// <summary>
            /// Looks up an individual member and returns a MemberGroup with the given members.
            /// </summary>
            public abstract MemberGroup/*!*/ ResolveMember(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name);

            /// <summary>
            /// Returns a list of members that exist on the type.  The ResolvedMember structure indicates both
            /// the name and provides the MemberGroup.
            /// </summary>
            public IList<ResolvedMember/*!*/>/*!*/ ResolveMembers(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
                Dictionary<string, ResolvedMember> members = new Dictionary<string, ResolvedMember>();

                foreach (string name in GetCandidateNames(binder, action, type)) {
                    if (members.ContainsKey(name)) {
                        continue;
                    }

                    MemberGroup member = ResolveMember(binder, action, type, name);
                    if (member.Count > 0) {
                        members[name] = new ResolvedMember(name, member);
                    }
                }

                ResolvedMember[] res = new ResolvedMember[members.Count];
                members.Values.CopyTo(res, 0);
                return res;
            }

            /// <summary>
            /// Returns a list of possible members which could exist.  ResolveMember needs to be called to verify their existance. Duplicate
            /// names can also be returned.
            /// </summary>
            protected abstract IEnumerable<string/*!*/>/*!*/ GetCandidateNames(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type);
        }

        /// <summary>
        /// One off resolver for various special methods which are known by name.  A delegate is provided to provide the actual member which
        /// will be resolved.
        /// </summary>
        class OneOffResolver : MemberResolver {
            private string/*!*/ _name;
            private Func<MemberBinder/*!*/, Type/*!*/, MemberGroup/*!*/>/*!*/ _resolver;

            public OneOffResolver(string/*!*/ name, Func<MemberBinder/*!*/, Type/*!*/, MemberGroup/*!*/>/*!*/ resolver) {
                Assert.NotNull(name, resolver);

                _name = name;
                _resolver = resolver;
            }

            public override MemberGroup/*!*/ ResolveMember(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
                Assert.NotNull(binder, action, type, name);

                if (name == _name) {
                    return _resolver(binder, type);
                }

                return MemberGroup.EmptyGroup;
            }

            protected override IEnumerable<string/*!*/>/*!*/ GetCandidateNames(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
                yield return _name;
            }
        }

        /// <summary>
        /// Standard resolver for looking up .NET members.  Uses reflection to get the members by name.
        /// </summary>
        class StandardResolver : MemberResolver {
            public override MemberGroup/*!*/ ResolveMember(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
                if (name == ".ctor" || name == ".cctor") return MemberGroup.EmptyGroup;

                // normal binding
                MemberGroup res;

                foreach (Type curType in binder.GetContributingTypes(type)) {
                    res = GetBaseHelperOverloads(type, name, FilterSpecialNames(binder.GetMember(curType, name), name, action));
                    if (res.Count > 0) {
                        return res;
                    }
                }

                if (type.IsInterface) {
                    foreach (Type t in type.GetInterfaces()) {
                        res = FilterSpecialNames(binder.GetMember(t, name), name, action);
                        if (res.Count > 0) {
                            return res;
                        }
                    }
                }

                return MemberGroup.EmptyGroup;
            }

            protected override IEnumerable<string/*!*/>/*!*/ GetCandidateNames(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
                foreach (Type curType in binder.GetContributingTypes(type)) {
                    foreach (MemberInfo mi in curType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                        if (mi.MemberType == MemberTypes.Method) {
                            MethodInfo meth = (MethodInfo)mi;

                            if (meth.IsSpecialName) {
                                if (meth.IsDefined(typeof(PropertyMethodAttribute), true)) {
                                    if (meth.Name.StartsWith("Get") || meth.Name.StartsWith("Set")) {
                                        yield return meth.Name.Substring(3);
                                    } else {
                                        Debug.Assert(meth.Name.StartsWith("Delete"));
                                        yield return meth.Name.Substring(6);
                                    }
                                }

                                continue;
                            }
                        }

                        yield return mi.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Resolves methods mapped to __*__ methods automatically from the .NET operator.
        /// </summary>
        class OperatorResolver : MemberResolver {
            public override MemberGroup/*!*/ ResolveMember(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
                if (type.IsSealed && type.IsAbstract) {
                    // static types don't have PythonOperationKind
                    return MemberGroup.EmptyGroup;
                }

                // try mapping __*__ methods to .NET method names
                PythonOperationKind opMap;
                EnsureOperatorTable();

                if (_pythonOperatorTable.TryGetValue(name, out opMap)) {
                    if (IncludeOperatorMethod(type, opMap)) {
                        OperatorMapping opInfo;
                        if (IsReverseOperator(opMap)) {
                            opInfo = OperatorMapping.GetOperatorMapping(opMap & ~PythonOperationKind.Reversed);
                        } else {
                            opInfo = OperatorMapping.GetOperatorMapping(opMap);
                        }

                        if (opInfo != null) {
                            foreach (Type curType in binder.GetContributingTypes(type)) {
                                if (curType == typeof(BigInteger) &&
                                    (opInfo.Operator == PythonOperationKind.Mod ||
                                    opInfo.Operator == PythonOperationKind.RightShift ||
                                    opInfo.Operator == PythonOperationKind.LeftShift ||
                                    opInfo.Operator == PythonOperationKind.Compare ||
                                    opInfo.Operator == PythonOperationKind.Divide)) {
                                    // we override these with our own modulus/power PythonOperationKind which are different from BigInteger.
                                    continue;
                                }

                                Debug.Assert(opInfo.Name != "Equals");

                                MemberGroup res = GetBaseHelperOverloads(type, opInfo.Name, binder.GetMember(curType, opInfo.Name));
                                if (res.Count == 0 && opInfo.AlternateName != null) {
                                    res = binder.GetMember(curType, opInfo.AlternateName);
                                    if (opInfo.AlternateName == "Equals") {
                                        // "Equals" is available as an alternate method name.  Because it's also on object and Python
                                        // doesn't define it on object we need to filter it out.  
                                        res = FilterObjectEquality(res);
                                    } else {
                                        res = GetBaseHelperOverloads(type, opInfo.AlternateName, res);
                                    }
                                }

                                if (res.Count > 0) {
                                    return FilterForwardReverseMethods(name, res, type, opMap);
                                }
                            }
                        }
                    }
                }

                if (name == "__call__") {
                    MemberGroup res = binder.GetMember(type, "Call");
                    if (res.Count > 0) {
                        return res;
                    }
                }

                return MemberGroup.EmptyGroup;
            }

            /// <summary>
            /// Removes Object.Equals methods as we never return these for PythonOperationKind.
            /// </summary>
            private MemberGroup/*!*/ FilterObjectEquality(MemberGroup/*!*/ group) {
                List<MemberTracker> res = null;

                for (int i = 0; i < group.Count; i++) {
                    MemberTracker mt = group[i];
                    if (mt.MemberType == TrackerTypes.Method && (mt.DeclaringType == typeof(object) || mt.DeclaringType == typeof(double) || mt.DeclaringType == typeof(float)) && mt.Name == "Equals") {
                        if (res == null) {
                            res = new List<MemberTracker>();
                            for (int j = 0; j < i; j++) {
                                res.Add(group[j]);
                            }
                        }
                    } else if (mt.MemberType == TrackerTypes.Method && mt.DeclaringType == typeof(ValueType) && mt.Name == "Equals") {
                        // ValueType.Equals overrides object.Equals but we can't call it w/ a boxed value type therefore
                        // we return the object version and the virtual call will dispatch to ValueType.Equals.
                        if (res == null) {
                            res = new List<MemberTracker>();
                            for (int j = 0; j < i; j++) {
                                res.Add(group[j]);
                            }
                        }

                        res.Add(MemberTracker.FromMemberInfo(typeof(object).GetMethod("Equals", new Type[] { typeof(object) })));
                    } else if (res != null) {
                        res.Add(group[i]);
                    }
                }

                if (res != null) {
                    if (res.Count == 0) {
                        return MemberGroup.EmptyGroup;
                    }

                    return new MemberGroup(res.ToArray());
                }

                return group;
            }

            protected override IEnumerable<string/*!*/>/*!*/ GetCandidateNames(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
                EnsureOperatorTable();

                foreach (SymbolId si in _pythonOperatorTable.Keys) {
                    yield return SymbolTable.IdToString(si);
                }

                yield return "__call__";
            }
        }

        /// <summary>
        /// Provides bindings to private members when that global option is enabled.
        /// </summary>
        class PrivateBindingResolver : MemberResolver {
            private const BindingFlags _privateFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            public override MemberGroup/*!*/ ResolveMember(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
                if (binder.DomainManager.Configuration.PrivateBinding) {
                    // in private binding mode Python exposes private members under a mangled name.
                    string header = "_" + type.Name + "__";
                    if (name.StartsWith(header)) {
                        string memberName = name.Substring(header.Length);

                        MemberGroup res = new MemberGroup(type.GetMember(memberName, _privateFlags));
                        if (res.Count > 0) {
                            return FilterFieldAndEvent(res);
                        }

                        res = new MemberGroup(type.GetMember(memberName, BindingFlags.FlattenHierarchy | _privateFlags));
                        if (res.Count > 0) {
                            return FilterFieldAndEvent(res);
                        }
                    }
                }

                return MemberGroup.EmptyGroup;
            }

            protected override IEnumerable<string/*!*/>/*!*/ GetCandidateNames(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
                if (!binder.DomainManager.Configuration.PrivateBinding) {
                    yield break;
                }

                foreach (MemberInfo mi in type.GetMembers(_privateFlags | BindingFlags.FlattenHierarchy)) {
                    yield return String.Concat("_", mi.DeclaringType.Name, "__", mi.Name);
                }
            }
        }

        /// <summary>
        /// Provides resolutions for protected members that haven't yet been
        /// subclassed by NewTypeMaker.
        /// </summary>
        class ProtectedMemberResolver : MemberResolver {
            public override MemberGroup/*!*/ ResolveMember(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
                foreach (Type t in binder.GetContributingTypes(type)) {
                    MemberGroup res = new MemberGroup(ArrayUtils.FindAll(t.GetMember(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy), ProtectedOnly));

                    for (int i = 0; i < res.Count; i++) {
                        MethodTracker meth = res[i] as MethodTracker;                        
                        if (meth == null) {
                            continue;
                        }

                        if (meth.Name == "Finalize" && meth.Method.GetBaseDefinition() == typeof(object).GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance)) {
                            MemberTracker[] retained = new MemberTracker[res.Count - 1];
                            if (res.Count == 1) {
                                res = MemberGroup.EmptyGroup;
                            } else {
                                for (int j = 0; j < i; j++) {
                                    retained[j] = res[j];
                                }
                                for (int j = i + 1; j < res.Count; j++) {
                                    retained[j - 1] = res[j];
                                }
                                res = new MemberGroup(retained);
                            }
                            break;
                        }
                    }
                    res = FilterSpecialNames(res, name, action);

                    return GetBaseHelperOverloads(PythonTypeOps.GetFinalSystemType(type), name, res);
                }

                return MemberGroup.EmptyGroup;
            }

            protected override IEnumerable<string/*!*/>/*!*/ GetCandidateNames(MemberBinder/*!*/ binder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
                // these members are visible but only accept derived types.
                foreach (Type t in binder.GetContributingTypes(type)) {
                    MemberInfo[] mems = ArrayUtils.FindAll(t.GetMembers(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic), ProtectedOnly);
                    foreach (MemberInfo mi in mems) {
                        yield return mi.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Creates the resolver table which includes all the possible resolutions.
        /// </summary>
        /// <returns></returns>
        private static MemberResolver/*!*/[]/*!*/ MakeResolverTable() {
            return new MemberResolver[] { 
                // values that live on object need to run before the StandardResolver
                // which will pick up values off of object.
                new OneOffResolver("__str__", StringResolver),
                new OneOffResolver("__new__", NewResolver),
                new OneOffResolver("__repr__", ReprResolver),
                new OneOffResolver("__hash__", HashResolver),       
                new OneOffResolver("__iter__", IterResolver),         
#if !SILVERLIGHT
                new OneOffResolver("__reduce_ex__", SerializationResolver),
#endif
                // The standard resolver looks for types using .NET reflection by name
                new StandardResolver(), 
                
                // Runs after StandardResolver so custom __eq__ methods can be added
                // that support things like returning NotImplemented vs. IValueEquality
                // which only supports true/false.  Runs before OperatorResolver so that
                // IValueEquality takes precedence over Equals which can be provied for
                // nice .NET interop.
                new OneOffResolver("__all__", AllResolver),     
                new OneOffResolver("__contains__", ContainsResolver),
                new OneOffResolver("__dir__", DirResolver),
                new OneOffResolver("__doc__", DocResolver),
                new OneOffResolver("__enter__", EnterResolver),     
                new OneOffResolver("__eq__", EqualityResolver),     
                new OneOffResolver("__exit__", ExitResolver),  
                new OneOffResolver("__len__", LengthResolver),        
                new OneOffResolver("__ne__", InequalityResolver),
                new OneOffResolver("__format__", FormatResolver),
                new OneOffResolver("next", NextResolver),

                // non standard PythonOperationKind which are Python specific
                new OneOffResolver("__truediv__", new OneOffOperatorBinder("TrueDivide", "__truediv__", PythonOperationKind.TrueDivide).Resolver),
                new OneOffResolver("__rtruediv__", new OneOffOperatorBinder("TrueDivide", "__rtruediv__", PythonOperationKind.ReverseTrueDivide).Resolver),
                new OneOffResolver("__itruediv__", new OneOffOperatorBinder("InPlaceTrueDivide", "__itruediv__", PythonOperationKind.InPlaceTrueDivide).Resolver),
                new OneOffResolver("__floordiv__", new OneOffOperatorBinder("FloorDivide", "__floordiv__", PythonOperationKind.FloorDivide).Resolver),
                new OneOffResolver("__rfloordiv__", new OneOffOperatorBinder("FloorDivide", "__rfloordiv__", PythonOperationKind.ReverseFloorDivide).Resolver),
                new OneOffResolver("__ifloordiv__", new OneOffOperatorBinder("InPlaceFloorDivide", "__ifloordiv__", PythonOperationKind.InPlaceFloorDivide).Resolver),
                new OneOffResolver("__pow__", new OneOffPowerBinder("__pow__", PythonOperationKind.Power).Resolver),
                new OneOffResolver("__rpow__", new OneOffPowerBinder("__rpow__", PythonOperationKind.ReversePower).Resolver),
                new OneOffResolver("__ipow__", new OneOffOperatorBinder("InPlacePower", "__ipow__", PythonOperationKind.InPlacePower).Resolver),
                new OneOffResolver("__abs__", new OneOffOperatorBinder("Abs", "__abs__", PythonOperationKind.AbsoluteValue).Resolver),
                new OneOffResolver("__divmod__", new OneOffOperatorBinder("DivMod", "__divmod__", PythonOperationKind.DivMod).Resolver),
                new OneOffResolver("__rdivmod__", new OneOffOperatorBinder("DivMod", "__rdivmod__", PythonOperationKind.DivMod).Resolver),
                
                // The operator resolver maps standard .NET operator methods into Python operator
                // methods
                new OperatorResolver(),
                
                // Runs after operator resolver to map __ne__ -> !__eq__
                new OneOffResolver("__ne__", FallbackInequalityResolver),
                
                // Protected members are visible but only usable from derived types
                new ProtectedMemberResolver(),
                // Support binding to private members if the user has enabled that feature
                new PrivateBindingResolver(),
            };
        }

        #endregion

        #region One-off resolvers

        /// <summary>
        /// Provides a resolution for __str__.
        /// </summary>
        private static MemberGroup/*!*/ StringResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (type != typeof(double) && type != typeof(float)) {
                MethodInfo tostr = type.GetMethod("ToString", Type.EmptyTypes);
                if (tostr != null && tostr.DeclaringType != typeof(object)) {
                    return GetInstanceOpsMethod(type, "ToStringMethod");
                }
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Provides a resolution for __repr__
        /// </summary>
        private static MemberGroup/*!*/ ReprResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            // __repr__ for normal .NET types is special, if we're a Python type then
            // we'll use one of the built-in reprs (from object or from the type)
            if (!PythonBinder.IsPythonType(type) &&
                (!type.IsSealed || !type.IsAbstract)) {     // static types don't get __repr__
                // check and see if __repr__ has been overridden by the base type.
                foreach (Type t in binder.GetContributingTypes(type)) {
                    if (t == typeof(ObjectOps) && type != typeof(object)) {
                        break;
                    }

                    if (t.GetMember("__repr__").Length > 0) {
                        // type has a specific __repr__ overload, pick it up normally later
                        return MemberGroup.EmptyGroup;
                    }
                }

                // no override, pick up the default fancy .NET __repr__
                return binder.GetBaseInstanceMethod(type, "FancyRepr");
            }

            return MemberGroup.EmptyGroup;
        }
#if !SILVERLIGHT
        private static MemberGroup/*!*/ SerializationResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (type.IsSerializable && !PythonBinder.IsPythonType(type)) {

                string methodName = "__reduce_ex__";

                if (!TypeOverridesMethod(binder, type, methodName)) {
                    return GetInstanceOpsMethod(type, "SerializeReduce");
                }
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Helper to see if the type explicitly overrides the method.  This ignores members
        /// defined on object.
        /// </summary>
        private static bool TypeOverridesMethod(MemberBinder/*!*/ binder, Type/*!*/  type, string/*!*/  methodName) {
            // check and see if the method has been overridden by the base type.
            foreach (Type t in binder.GetContributingTypes(type)) {
                if (!PythonBinder.IsPythonType(type) && t == typeof(ObjectOps) && type != typeof(object)) {
                    break;
                }

                MemberInfo[] reduce = t.GetMember(methodName);
                if (reduce.Length > 0) {
                    // type has a specific overload
                    return true;
                }
            }
            return false;
        }
#endif

        /// <summary>
        /// Provides a resolution for IValueEquality.GetValueHashCode to __hash__.
        /// </summary>
        private static MemberGroup/*!*/ HashResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            // __repr__ for normal .NET types is special, if we're a Python type then
            // we'll use one of the built-in reprs (from object or from the type)
            if (typeof(IValueEquality).IsAssignableFrom(type) && !type.IsInterface) {
                return new MemberGroup(typeof(IValueEquality).GetMethod("GetValueHashCode"));
            }

            // otherwise we'll pick up __hash__ from ObjectOps which will call .NET's .GetHashCode therefore
            // we don't explicitly search to see if the object overrides GetHashCode here.
            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Provides a resolution for __new__.  For standard .NET types __new__ resolves to their
        /// constructor.  For Python types they inherit __new__ from their base class.
        /// 
        /// TODO: Can we just always fallback to object.__new__?  If not why not?
        /// </summary>
        private static MemberGroup/*!*/ NewResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (type.IsSealed && type.IsAbstract) {
                // static types don't have __new__
                return MemberGroup.EmptyGroup;
            }

            bool isPythonType = typeof(IPythonObject).IsAssignableFrom(type);

            // check and see if __new__ has been overridden by the base type.
            foreach (Type t in binder.GetContributingTypes(type)) {
                if (!isPythonType && t == typeof(ObjectOps) && type != typeof(object)) {
                    break;
                }

                MemberInfo[] news = t.GetMember("__new__");
                if (news.Length > 0) {
                    // type has a specific __new__ overload, return that for the constructor
                    return GetExtensionMemberGroup(type, news);
                }
            }

            // type has no Python __new__, just return the .NET constructors if they have
            // a custom new
            ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);// CompilerHelpers.GetConstructors(type, binder.DomainManager.Configuration.PrivateBinding, true);
            ctors = CompilerHelpers.FilterConstructorsToPublicAndProtected(ctors);
            if (!PythonTypeOps.IsDefaultNew(ctors)) {
                return new MemberGroup(ctors);
            }

            // if no ctor w/ parameters are defined, fall back to object.__new__ which
            // will ignore all the extra arguments allowing the user to just override
            // __init__.
            return MemberGroup.EmptyGroup;
        }

        internal static MemberGroup GetExtensionMemberGroup(Type type, MemberInfo[] news) {
            List<MemberTracker> mts = new List<MemberTracker>();
            foreach (MemberInfo mi in news) {
                if (mi.MemberType == MemberTypes.Method) {
                    if (mi.DeclaringType.IsAssignableFrom(type)) {
                        mts.Add(MethodTracker.FromMemberInfo(mi));
                    } else {
                        mts.Add(MethodTracker.FromMemberInfo(mi, type));
                    }
                }
            }

            return new MemberGroup(mts.ToArray());
        }

        /// <summary>
        /// Provides a resolution for next
        /// </summary>
        private static MemberGroup/*!*/ NextResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (typeof(IEnumerator).IsAssignableFrom(type)) {
                return GetInstanceOpsMethod(type, "NextMethod");
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Provides a resolution for __len__
        /// </summary>
        private static MemberGroup/*!*/ LengthResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (binder.GetInterfaces(type).Contains(typeof(ICollection))) {
                return GetInstanceOpsMethod(type, "LengthMethod");
            }

            foreach (Type t in binder.GetInterfaces(type)) {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)) {
                    MethodInfo genMeth = typeof(InstanceOps).GetMethod("GenericLengthMethod");
                    return new MemberGroup(
                        MethodTracker.FromMemberInfo(genMeth.MakeGenericMethod(t.GetGenericArguments()), type)
                    );
                }
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Provides a resolution for __iter__
        /// </summary>
        private static MemberGroup/*!*/ IterResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            // no __iter__ on string, just __getitem__
            if (type != typeof(string)) {
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type)) {
                    foreach (Type t in binder.GetContributingTypes(type)) {
                        MemberInfo[] news = t.GetMember("__iter__");
                        if (news.Length > 0) {
                            // type has a specific __i__ overload, we'll pick it up later
                            return MemberGroup.EmptyGroup;
                        }
                    }

                    // no special __iter__, use the default.
                    return GetInstanceOpsMethod(type, "IterMethod");
                }
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Provides a mapping of IValueEquality.ValueEquals to __eq__
        /// </summary>
        private static MemberGroup/*!*/ EqualityResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (typeof(IValueEquality).IsAssignableFrom(type)) {
                return new MemberGroup(GetEqualityMethods(type, "ValueEqualsMethod"));
            }

            return MemberGroup.EmptyGroup;
        }


        /// <summary>
        /// Provides a mapping of IValueEquality.ValueNotEquals to __ne__
        /// </summary>
        private static MemberGroup/*!*/ InequalityResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (typeof(IValueEquality).IsAssignableFrom(type)) {
                return new MemberGroup(GetEqualityMethods(type, "ValueNotEqualsMethod"));
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Looks for an Equals overload defined on the type and if one is present binds __ne__ to an
        /// InstanceOps helper.
        /// </summary>
        private static MemberGroup/*!*/ FallbackInequalityResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            // if object defines __eq__ then we can call the reverse version
            if (IncludeOperatorMethod(type, PythonOperationKind.NotEqual)) {
                foreach (Type curType in binder.GetContributingTypes(type)) {
                    MemberGroup mg = binder.GetMember(curType, "Equals");

                    foreach (MemberTracker mt in mg) {
                        if (mt.MemberType != TrackerTypes.Method || mt.DeclaringType == typeof(object)) {
                            continue;
                        }

                        MethodTracker method = (MethodTracker)mt;
                        if ((method.Method.Attributes & MethodAttributes.NewSlot) != 0) {
                            continue;
                        }

                        ParameterInfo[] pis = method.Method.GetParameters();
                        if (pis.Length == 1) {
                            if (pis[0].ParameterType == typeof(object)) {
                                return new MemberGroup(MethodTracker.FromMemberInfo(typeof(InstanceOps).GetMethod("NotEqualsMethod"), curType));
                            }
                        }
                    }
                }
            }

            return MemberGroup.EmptyGroup;
        }

        private static MemberGroup/*!*/ AllResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            // static types are like modules and define __all__.
            if (type.IsAbstract && type.IsSealed) {
                return new MemberGroup(new ExtensionPropertyTracker("__all__", typeof(InstanceOps).GetMethod("Get__all__").MakeGenericMethod(type), null, null, type));
            }

            return MemberGroup.EmptyGroup;
        }

        private static MemberGroup/*!*/ DirResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            return binder.GetMember(type, "GetMemberNames");
        }

        class DocumentationDescriptor : PythonTypeSlot {
            internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
                if (owner.IsSystemType) {
                    if (instance is IDynamicObject && !(instance is IPythonObject)) {
                        // ask the foreign IDO for it's documentation
                        value = PythonContext.GetContext(context).GetDocumentation(instance);
                    } else {
                        value = PythonTypeOps.GetDocumentation(owner.UnderlyingSystemType);
                    }
                    return true;
                }

                value = null;
                return false;
            }

            internal override bool GetAlwaysSucceeds {
                get {
                    return true;
                }
            }

            internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
                IPythonObject obj = instance as IPythonObject;
                if (obj == null || !obj.HasDictionary) {
                    string name = owner.Name;
                    if (obj != null) {
                        name = obj.PythonType.Name;
                    }
                    throw PythonOps.AttributeErrorForReadonlyAttribute(name, Symbols.Doc);
                }

                UserTypeOps.GetDictionary(obj)[Symbols.Doc] = value;
                return true;
            }
        }

        private static MemberGroup/*!*/ DocResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (_docDescr == null) {
                _docDescr = new DocumentationDescriptor();
            }

            return new MemberGroup(new CustomAttributeTracker(type, "__doc__", _docDescr));
        }

        private static MemberGroup/*!*/ EnterResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (typeof(IDisposable).IsAssignableFrom(type)) {
                return GetInstanceOpsMethod(type, "EnterMethod");
            }

            return MemberGroup.EmptyGroup;
        }

        private static MemberGroup/*!*/ ExitResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (typeof(IDisposable).IsAssignableFrom(type)) {
                return GetInstanceOpsMethod(type, "ExitMethod");
            }

            return MemberGroup.EmptyGroup;
        }

        private static MemberGroup/*!*/ FormatResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (typeof(IFormattable).IsAssignableFrom(type)) {
                return GetInstanceOpsMethod(type, "Format");
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Provides an implementation of __contains__.  We can pull contains from:
        ///     ICollection of T which defines Contains directly
        ///     IList which defines Contains directly
        ///     IDictionary which defines Contains directly
        ///     IDictionary of K,V which defines Contains directly
        ///     IEnumerable of K which we have an InstaceOps helper for
        ///     IEnumerable which we have an instance ops helper for
        ///     IEnumerator of K which we have an InstanceOps helper for
        ///     IEnumerator which we have an instance ops helper for
        ///     
        /// String is ignored here because it defines __contains__ via extension methods already.
        ///     
        /// The lookup is well ordered and not dependent upon the order of values returned by reflection.
        /// </summary>
        private static MemberGroup/*!*/ ContainsResolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
            if (type == typeof(PythonGenerator)) {
                // it's enumerable but doesn't have __contains__
                return MemberGroup.EmptyGroup;
            }

            List<MemberTracker> containsMembers = null;

            IList<Type> intf = binder.GetInterfaces(type);
            
            // if we get a __contains__ for something w/ a generic typed to object don't look for non-generic versions
            bool hasObjectContains = false;

            // search for IDictionary<K, V> first because it's ICollection<KVP<K, V>> and we want to call ContainsKey
            foreach (Type t in intf) {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>)) {
                    if (t.GetGenericArguments()[0] == typeof(object)) {
                        hasObjectContains = true;
                    }

                    if (containsMembers == null) {
                        containsMembers = new List<MemberTracker>();
                    }

                    containsMembers.Add(MethodTracker.FromMemberInfo(t.GetMethod("ContainsKey")));
                }
            }

            if (containsMembers == null) {
                // then look for ICollection<T> for generic __contains__ first if we're not an IDictionary<K, V>
                foreach (Type t in intf) {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)) {
                        if (t.GetGenericArguments()[0] == typeof(object)) {
                            hasObjectContains = true;
                        }
                        if (containsMembers == null) {
                            containsMembers = new List<MemberTracker>();
                        }

                        containsMembers.Add(MethodTracker.FromMemberInfo(t.GetMethod("Contains")));
                    }
                }
            }
            
            if (!hasObjectContains) {
                // look for non-generic contains if we didn't already find an overload which takes
                // object
                if (intf.Contains(typeof(IList))) {
                    if (containsMembers == null) {
                        containsMembers = new List<MemberTracker>();
                    }

                    containsMembers.Add(MethodTracker.FromMemberInfo(typeof(IList).GetMethod("Contains")));
                } else if (intf.Contains(typeof(IDictionary))) {
                    if (containsMembers == null) {
                        containsMembers = new List<MemberTracker>();
                    }

                    containsMembers.Add(MethodTracker.FromMemberInfo(typeof(IDictionary).GetMethod("Contains")));
                } else if (containsMembers == null) {
                    // see if we can produce a contains for IEnumerable
                    GetEnumeratorContains(type, intf, ref containsMembers, ref hasObjectContains, typeof(IEnumerable<>), typeof(IEnumerable), String.Empty);

                    if (containsMembers == null) {
                        GetEnumeratorContains(type, intf, ref containsMembers, ref hasObjectContains, typeof(IEnumerator<>), typeof(IEnumerator), "IEnumerator");
                    }
                }
            }

            if (containsMembers != null) {
                return new MemberGroup(containsMembers.ToArray());
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Helper for IEnumerable/IEnumerator __contains__ 
        /// </summary>
        private static void GetEnumeratorContains(Type type, IList<Type> intf, ref List<MemberTracker> containsMembers, ref bool hasObjectContains, Type ienumOfT, Type ienum, string name) {
            foreach (Type t in intf) {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == ienumOfT) {
                    if (t.GetGenericArguments()[0] == typeof(object)) {
                        hasObjectContains = true;
                    }

                    if (containsMembers == null) {
                        containsMembers = new List<MemberTracker>();
                    }

                    containsMembers.Add(
                        (MethodTracker)MethodTracker.FromMemberInfo(
                            typeof(InstanceOps).GetMethod("ContainsGenericMethod" + name).MakeGenericMethod(t.GetGenericArguments()[0]),
                            t
                        )
                    );
                }
            }

            if (intf.Contains(type) && !hasObjectContains) {
                if (containsMembers == null) {
                    containsMembers = new List<MemberTracker>();
                }

                containsMembers.Add(MethodTracker.FromMemberInfo(typeof(InstanceOps).GetMethod("ContainsMethod" + name), typeof(IEnumerable)));
            }
        }

        private class OneOffOperatorBinder {
            private string/*!*/ _methodName;
            private string/*!*/ _pythonName;
            private PythonOperationKind/*!*/ _op;

            public OneOffOperatorBinder(string/*!*/ methodName, string/*!*/ pythonName, PythonOperationKind opMap) {
                Assert.NotNull(methodName, pythonName, opMap);

                _methodName = methodName;
                _pythonName = pythonName;
                _op = opMap;
            }

            public MemberGroup/*!*/ Resolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
                if (type.IsSealed && type.IsAbstract) {
                    // static types don't have PythonOperationKind
                    return MemberGroup.EmptyGroup;
                }

                foreach (Type t in binder.GetContributingTypes(type)) {
                    MemberGroup res = binder.GetMember(t, _methodName);
                    if (res.Count > 0) {
                        return FilterForwardReverseMethods(_pythonName, res, type, _op);
                    }
                }
                return MemberGroup.EmptyGroup;
            }
        }

        private class OneOffPowerBinder {
            private string/*!*/ _pythonName;
            private PythonOperationKind/*!*/ _op;

            public OneOffPowerBinder(string/*!*/ pythonName, PythonOperationKind op) {
                Assert.NotNull(pythonName, op);

                _pythonName = pythonName;
                _op = op;
            }

            public MemberGroup/*!*/ Resolver(MemberBinder/*!*/ binder, Type/*!*/ type) {
                if (type.IsSealed && type.IsAbstract) {
                    // static types don't have PythonOperationKind
                    return MemberGroup.EmptyGroup;
                }

                foreach (Type t in binder.GetContributingTypes(type)) {
                    if (t == typeof(BigInteger)) continue;

                    MemberGroup res = binder.GetMember(t, "op_Power");
                    if (res.Count > 0) {
                        return FilterForwardReverseMethods(_pythonName, res, type, _op);
                    }

                    res = binder.GetMember(t, "Power");
                    if (res.Count > 0) {
                        return FilterForwardReverseMethods(_pythonName, res, type, _op);
                    }
                }
                return MemberGroup.EmptyGroup;
            }
        }

        private static MethodTracker/*!*/[]/*!*/ GetEqualityMethods(Type type, string name) {
            MethodInfo[] mis = GetMethodSet(name, 3);
            MethodTracker[] trackers = new MethodTracker[mis.Length];
            for (int i = 0; i < mis.Length; i++) {
                trackers[i] = (MethodTracker)MethodTracker.FromMemberInfo(mis[i].MakeGenericMethod(type), type);
            }
            return trackers;
        }

        #endregion

        #region Member lookup implementations

        /// <summary>
        /// Base class used for resolving a name into a member on the type.
        /// </summary>
        private abstract class MemberBinder {
            private PythonBinder/*!*/ _binder;

            public MemberBinder(PythonBinder/*!*/ binder) {
                Debug.Assert(binder != null);

                _binder = binder;
            }

            public abstract IList<Type/*!*/>/*!*/ GetContributingTypes(Type/*!*/ t);
            public abstract IList<Type/*!*/>/*!*/ GetInterfaces(Type/*!*/ t);

            /// <summary>
            /// Gets an instance op method for the given type and name.
            /// 
            /// Instance ops methods appaer on the base most class that's required to expose it.  So
            /// if we have: Array[int], Array, object we'd only add an instance op method to Array and
            /// Array[int] inherits it.  It's obviously not on object because if it was there we'd just
            /// put the method in ObjectOps.
            /// 
            /// Therefore the different binders expose this at the appropriate times.  
            /// </summary>
            public abstract MemberGroup/*!*/ GetBaseInstanceMethod(Type/*!*/ type, params string[] name);

            public abstract MemberGroup/*!*/ GetMember(Type/*!*/ type, string/*!*/ name);

            protected PythonBinder/*!*/ Binder {
                get {
                    return _binder;
                }
            }

            public ScriptDomainManager/*!*/ DomainManager {
                get {
                    return _binder.DomainManager;
                }
            }

            protected MemberGroup/*!*/ GetMember(Type/*!*/ type, string/*!*/ name, BindingFlags flags) {
                Assert.NotNull(type, name);

                MemberInfo[] foundMembers = type.GetMember(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | flags);

                if (!Binder.DomainManager.Configuration.PrivateBinding) {
                    foundMembers = CompilerHelpers.FilterNonVisibleMembers(type, foundMembers);
                }

                MemberGroup members = new MemberGroup(foundMembers);

                // check for generic types w/ arity...
                Type[] types = type.GetNestedTypes(BindingFlags.Public | flags);
                string genName = name + ReflectionUtils.GenericArityDelimiter;
                List<Type> genTypes = null;
                foreach (Type t in types) {
                    if (t.Name.StartsWith(genName)) {
                        if (genTypes == null) genTypes = new List<Type>();
                        genTypes.Add(t);
                    }
                }

                if (genTypes != null) {
                    List<MemberTracker> mt = new List<MemberTracker>(members);
                    foreach (Type t in genTypes) {
                        mt.Add(MemberTracker.FromMemberInfo(t));
                    }

                    return new MemberGroup(mt.ToArray());
                }

                if (members.Count == 0) {
                    if ((flags & BindingFlags.DeclaredOnly) == 0) {
                        members = Binder.GetAllExtensionMembers(type, name);
                    } else {
                        members = Binder.GetExtensionMembers(type, name);
                    }
                }

                return members;
            }
        }

        /// <summary>
        /// MemberBinder which searches the entire type hierarchy and their extension types to find a member.
        /// </summary>
        private class ResolveBinder : MemberBinder {
            public ResolveBinder(PythonBinder/*!*/ binder)
                : base(binder) {
            }

            public override IList<Type/*!*/>/*!*/ GetInterfaces(Type/*!*/ t) {
                return t.GetInterfaces();
            }

            public override MemberGroup/*!*/ GetBaseInstanceMethod(Type/*!*/ type, params string[] name) {
                return GetInstanceOpsMethod(type, name);
            }

            public override IList<Type/*!*/>/*!*/ GetContributingTypes(Type/*!*/ t) {
                Debug.Assert(t != null);

                List<Type> res = new List<Type>();

                IList<PythonType> mro = DynamicHelpers.GetPythonTypeFromType(t).ResolutionOrder;

                foreach (PythonType pt in mro) {
                    res.Add(pt.UnderlyingSystemType);
                }

                foreach (PythonType pt in mro) {
                    res.AddRange(Binder.GetExtensionTypesInternal(pt.UnderlyingSystemType));
                }

                if (t.IsInterface) {
                    foreach (Type iface in t.GetInterfaces()) {
                        res.Add(iface);
                    }
                }

                return res;
            }

            public override MemberGroup GetMember(Type type, string name) {
                return GetMember(type, name, 0);
            }
        }

        /// <summary>
        /// MemberBinder which searches only the current type and it's extension types to find a member.
        /// </summary>
        private class LookupBinder : MemberBinder {
            public LookupBinder(PythonBinder/*!*/ binder)
                : base(binder) {
            }

            public override IList<Type/*!*/>/*!*/ GetInterfaces(Type/*!*/ t) {
                if (t.IsInterface) {
                    return t.GetInterfaces();
                }

                Type[] allInterfaces = t.GetInterfaces();
                List<Type> res = new List<Type>();
                foreach (Type intf in allInterfaces) {
                    try {
                        InterfaceMapping imap = t.GetInterfaceMap(intf);
                        foreach (MethodInfo mi in imap.TargetMethods) {
                            if (mi.DeclaringType == t) {
                                res.Add(intf);
                                break;
                            }
                        }
                    } catch (ArgumentException) {
                        // this fails when the CLR is manufacturing an interface
                        // type for a built in type.  For example IList<string>
                        // for Array[str].  This can be reproed by doing:
                        //
                        // import System
                        // System.Array[str].__dict__['__contains__']

                        // __contains__ is actually inherited from Array's IList
                        // implementation but IList<str> interferes here.
                    }
                }

                return res;
            }

            public override MemberGroup/*!*/ GetBaseInstanceMethod(Type/*!*/ type, params string[] name) {
                if (type.BaseType == typeof(object) || type.BaseType == typeof(ValueType)) {
                    return GetInstanceOpsMethod(type, name);
                }

                return MemberGroup.EmptyGroup;
            }

            public override IList<Type/*!*/>/*!*/ GetContributingTypes(Type/*!*/ t) {
                Debug.Assert(t != null);

                List<Type> res = new List<Type>();
                res.Add(t);
                res.AddRange(Binder.GetExtensionTypesInternal(t));

                return res;
            }

            public override MemberGroup GetMember(Type type, string name) {
                return GetMember(type, name, BindingFlags.DeclaredOnly);
            }
        }

        #endregion

        #region Private implementation details

        /// <summary>
        /// Primary worker for getting the member(s) associated with a single name.  Can be called with different MemberBinder's to alter the
        /// scope of the search.
        /// </summary>
        private static MemberGroup/*!*/ GetMemberGroup(MemberBinder/*!*/ memberBinder, OldDynamicAction/*!*/ action, Type/*!*/ type, string/*!*/ name) {
            foreach (MemberResolver resolver in _resolvers) {
                MemberGroup/*!*/ group = resolver.ResolveMember(memberBinder, action, type, name);
                if (group.Count > 0) {
                    return group;
                }
            }

            return MemberGroup.EmptyGroup;
        }

        /// <summary>
        /// Primary worker for returning a list of all members in a type.  Can be called with different MemberBinder's to alter the scope
        /// of the search.
        /// </summary>
        private static IList<ResolvedMember/*!*/>/*!*/ GetResolvedMembers(MemberBinder/*!*/ memberBinder, OldDynamicAction/*!*/ action, Type/*!*/ type) {
            List<ResolvedMember> res = new List<ResolvedMember>();

            foreach (MemberResolver resolver in _resolvers) {
                res.AddRange(resolver.ResolveMembers(memberBinder, action, type));
            }

            return res;
        }

        /// <summary>
        /// Helper to get a MemberGroup for methods declared on InstanceOps
        /// </summary>
        private static MemberGroup/*!*/ GetInstanceOpsMethod(Type/*!*/ extends, params string[]/*!*/ names) {
            Assert.NotNull(extends, names);

            MethodTracker[] trackers = new MethodTracker[names.Length];
            for (int i = 0; i < names.Length; i++) {
                trackers[i] = (MethodTracker)MemberTracker.FromMemberInfo(typeof(InstanceOps).GetMethod(names[i]), extends);
            }

            return new MemberGroup(trackers);
        }

        /// <summary>
        /// Filters out methods which are present on standard .NET types but shouldn't be there in Python
        /// </summary>
        internal static bool IncludeOperatorMethod(Type/*!*/ t, PythonOperationKind op) {
            // numeric types in python don't define equality, just __cmp__
            if (t == typeof(bool) ||
                (Converter.IsNumeric(t) && t != typeof(Complex64) && t != typeof(double) && t != typeof(float))) {
                switch (op) {
                    case PythonOperationKind.Equal:
                    case PythonOperationKind.NotEqual:
                    case PythonOperationKind.GreaterThan:
                    case PythonOperationKind.LessThan:
                    case PythonOperationKind.GreaterThanOrEqual:
                    case PythonOperationKind.LessThanOrEqual:
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// When private binding is enabled we can have a collision between the private Event
        /// and private field backing the event.  We filter this out and favor the event.
        /// 
        /// This matches the v1.0 behavior of private binding.
        /// </summary>
        private static MemberGroup/*!*/ FilterFieldAndEvent(MemberGroup/*!*/ members) {
            Debug.Assert(members != null);

            TrackerTypes mt = TrackerTypes.None;
            foreach (MemberTracker mi in members) {
                mt |= mi.MemberType;
            }

            if (mt == (TrackerTypes.Event | TrackerTypes.Field)) {
                List<MemberTracker> res = new List<MemberTracker>();
                foreach (MemberTracker mi in members) {
                    if (mi.MemberType == TrackerTypes.Event) {
                        res.Add(mi);
                    }
                }
                return new MemberGroup(res.ToArray());
            }
            return members;
        }

        /// <summary>
        /// Filters down to include only protected methods
        /// </summary>
        private static bool ProtectedOnly(MemberInfo/*!*/ input) {
            Debug.Assert(input != null);

            switch (input.MemberType) {
                case MemberTypes.Method:
                    return ((MethodInfo)input).IsFamily || ((MethodInfo)input).IsFamilyOrAssembly;
                case MemberTypes.Property:
                    MethodInfo mi = ((PropertyInfo)input).GetGetMethod(true);
                    if (mi != null) return ProtectedOnly(mi);
                    return false;
                case MemberTypes.Field:
                    return ((FieldInfo)input).IsFamily || ((FieldInfo)input).IsFamilyOrAssembly;
                case MemberTypes.NestedType:
                    return ((Type)input).IsNestedFamily || ((Type)input).IsNestedFamORAssem;
                default:
                    return false;
            }
        }

        internal static bool IsReverseOperator(PythonOperationKind op) {
            return (op & PythonOperationKind.Reversed) != 0;
        }

        internal static bool IsReverseOperator(string op) {
            // TODO: Should determine reverse some other way or Python should be recognizing reverse PythonOperationKind some other way.
            return op.StartsWith("Reverse");
        }
        /// <summary>
        /// If an operator is a reverisble operator (e.g. addition) then we need to filter down to just the forward/reverse
        /// versions of the .NET method.  For example consider:
        /// 
        ///     String.op_Multiplication(int, string)
        ///     String.op_Multiplication(string, int)
        ///     
        /// If this method were defined on string it defines that you can do:
        ///     2 * 'abc'
        ///   or:
        ///     'abc' * 2
        ///     
        /// either of which will produce 'abcabc'.  The 1st form is considered the reverse form because it is declared on string
        /// but takes a non-string for the 1st argument.  The 2nd is considered the forward form because it takes a string as the
        /// 1st argument.
        /// 
        /// When dynamically dispatching for 2 * 'abc' we'll first try __mul__ on int, which will fail with a string argument.  Then we'll try
        /// __rmul__ on a string which will succeed and dispatch to the (int, string) overload.
        /// 
        /// For multiplication in this case it's not too interesting because it's commutative.  For addition this might be more interesting
        /// if, for example, we had unicode and ASCII strings.  In that case Unicode strings would define addition taking both unicode and
        /// ASCII strings in both forms.
        /// </summary>
        private static MemberGroup/*!*/ FilterForwardReverseMethods(string name, MemberGroup/*!*/ group, Type/*!*/ type, PythonOperationKind oper) {
            List<MethodTracker> res = new List<MethodTracker>(group.Count);
            PythonOperationKind reversed = Symbols.OperatorToReverseOperator(oper);
            foreach (MemberTracker mt in group) {
                if (mt.MemberType != TrackerTypes.Method) {
                    continue;
                }

                MethodTracker mTracker = (MethodTracker)mt;
                if (reversed == PythonOperationKind.None) {
                    res.Add(mTracker);
                    continue;
                }

                MethodInfo method = mTracker.Method;

                if (!method.IsStatic) {
                    if (!IsReverseOperator(oper)) {
                        res.Add(mTracker);
                    }
                    continue;
                }

                ParameterInfo[] parms = method.GetParameters();

                int ctxOffset = (parms.Length > 0 && parms[0].ParameterType == typeof(CodeContext)) ? 1 : 0;
                bool regular;

                bool reverse;

                if ((parms.Length - ctxOffset) == 2) {
                    Type param1Type = parms[0 + ctxOffset].ParameterType;
                    Type param2Type = parms[1 + ctxOffset].ParameterType;

                    // both parameters could be typed to object in which case we want to add
                    // the method as whatever we're being requested for here.  One example of this
                    // is EnumOps which can't be typed to Enum.
                    if (param1Type == typeof(object) && param2Type == typeof(object)) {
                        regular = !IsReverseOperator(oper);
                        reverse = IsReverseOperator(oper);
                    } else {
                        regular = parms.Length > 0 && AreTypesCompatible(param1Type, type);
                        reverse = ((oper & PythonOperationKind.Comparison) == 0) && parms.Length > 1 && AreTypesCompatible(param2Type, type);
                    }

                    if (IsReverseOperator(oper)) {
                        if (reverse) {
                            res.Add(mTracker);
                        }
                    } else {
                        if (regular) {
                            res.Add(mTracker);
                        }
                    }
                } else {
                    res.Add(mTracker);
                }
            }

            if (res.Count == 0) {
                return MemberGroup.EmptyGroup;
            }

            return new MemberGroup(new OperatorTracker(type, name, IsReverseOperator(oper), res.ToArray()));
        }

        /// <summary>
        /// Checks to see if the parameter type and the declaring type are compatible to determine
        /// if an operator is forward or reverse.
        /// </summary>
        private static bool AreTypesCompatible(Type paramType, Type declaringType) {
            if (paramType == typeof(object)) {
                // we may have:
                //    op_Add(object, someType)
                //    op_Add(someType, object)
                // we should recognize this as forward and reverse
                return declaringType == typeof(object);
            }

            // avoid getting/creating the PythonType if possible
            if (paramType == declaringType || declaringType.IsSubclassOf(paramType)) {
                return true;
            } else if (declaringType.IsSubclassOf(typeof(Extensible<>).MakeGenericType(paramType))) {
                return true;
            }

            return DynamicHelpers.GetPythonTypeFromType(declaringType).IsSubclassOf(DynamicHelpers.GetPythonTypeFromType(paramType));
        }

        private static void EnsureOperatorTable() {
            if (_pythonOperatorTable == null) {
                _pythonOperatorTable = InitializeOperatorTable();
            }
        }

        private static MemberGroup/*!*/ FilterSpecialNames(MemberGroup/*!*/ group, string/*!*/ name, OldDynamicAction/*!*/ action) {
            Assert.NotNull(group, name, action);

            bool filter = true;
            if (action.Kind == DynamicActionKind.Call ||
                action.Kind == DynamicActionKind.ConvertTo ||
                action.Kind == DynamicActionKind.DoOperation) {
                filter = false;
            }

            if (!IsPythonRecognizedOperator(name)) {
                filter = false;
            }

            List<MemberTracker> mts = null;
            for (int i = 0; i < group.Count; i++) {
                MemberTracker mt = group[i];
                bool skip = false;

                if (mt.MemberType == TrackerTypes.Method) {
                    MethodTracker meth = (MethodTracker)mt;
                    if (meth.Method.IsSpecialName && mt.Name != "op_Implicit" && mt.Name != "op_Explicit") {
                        if (!IsPropertyWithParameters(meth)) {
                            skip = true;
                        }
                    }

                    if (meth.Method.IsDefined(typeof(ClassMethodAttribute), true)) {
                        return new MemberGroup(new ClassMethodTracker(group));
                    }
                } else if (mt.MemberType == TrackerTypes.Property) {
                    PropertyTracker pt = (PropertyTracker)mt;
                    if (name == pt.Name && pt.GetIndexParameters().Length > 0 && IsPropertyDefaultMember(pt)) {
                        // exposed via __*item__, not the property name
                        skip = true;
                    }
                } else if (mt.MemberType == TrackerTypes.Field) {
                    FieldInfo fi = ((FieldTracker)mt).Field;

                    if (fi.IsDefined(typeof(SlotFieldAttribute), false)) {
                        if (mts == null) {
                            mts = MakeListWithPreviousMembers(group, mts, i);

                            mt = new CustomAttributeTracker(mt.DeclaringType, mt.Name, (PythonTypeSlot)fi.GetValue(null));
                        }
                    }
                }

                if (skip && filter) {
                    if (mts == null) {
                        // add any ones we skipped...
                        mts = MakeListWithPreviousMembers(group, mts, i);
                    }
                } else if (mts != null) {
                    mts.Add(mt);
                }
            }
            if (mts != null) {
                if (mts.Count == 0) {
                    return MemberGroup.EmptyGroup;
                }
                return new MemberGroup(mts.ToArray());
            }
            return group;
        }

        private static bool IsPropertyWithParameters(MethodTracker/*!*/ meth) {            
            if (meth.Method.Name.StartsWith("get_")) {
                if (!IsMethodDefaultMember(meth)) {
                    ParameterInfo[] args = meth.Method.GetParameters();
                    if (args.Length > 0) {
                        return true;
                    }
                }
            } else if (meth.Method.Name.StartsWith("set_")) {
                if (!IsMethodDefaultMember(meth)) {
                    ParameterInfo[] args = meth.Method.GetParameters();
                    if (args.Length > 1) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if this is an operator method which Python recognizes.  For example
        /// op_Comma is not recognized by Python and therefore should exposed to the user as
        /// a method that is callable by name.
        /// </summary>
        private static bool IsPythonRecognizedOperator(string name) {
            if (name.StartsWith("get_") || name.StartsWith("set_")) {
                return true;
            }

            // Python recognized operator names that aren't DLR standard names
            switch (name) {
                case "Abs":
                case "TrueDivide":
                case "FloorDivide":
                case "Power":
                case "DivMod":
                    return true;
            }

            bool isPythonRecognizedOperator = false;
            OperatorMapping oi = OperatorMapping.GetOperatorMapping(name);
            if (oi != null) {
                EnsureOperatorTable();

                if(_pythonOperatorTable.ContainsValue(oi.Operator)) {
                    isPythonRecognizedOperator = true;
                }
            }
            return isPythonRecognizedOperator;
        }

        private static bool IsPropertyDefaultMember(PropertyTracker pt) {
            foreach (MemberInfo mem in pt.DeclaringType.GetDefaultMembers()) {
                if (mem.Name == pt.Name) {
                    return true;
                }
            }
            return false;
        }

        private static bool IsMethodDefaultMember(MethodTracker pt) {
            foreach (MemberInfo mem in pt.DeclaringType.GetDefaultMembers()) {
                if (mem.MemberType == MemberTypes.Property) {
                    PropertyInfo pi = (PropertyInfo)mem;
                    if (pi.GetGetMethod() == pt.Method ||
                        pi.GetSetMethod() == pt.Method) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static List<MemberTracker> MakeListWithPreviousMembers(MemberGroup group, List<MemberTracker> mts, int i) {
            mts = new List<MemberTracker>(i);
            for (int j = 0; j < i; j++) {
                mts.Add(group[j]);
            }
            return mts;
        }

        /// <summary>
        /// Gets the base helper methods generated by NewTypeMaker for the specified method name in the given type.
        /// 
        /// These base helper methods just do a return base.Whatever(*args) so that we can issue super calls from
        /// users subclasses.
        /// </summary>
        private static MemberGroup/*!*/ GetBaseHelperOverloads(Type/*!*/ type, string/*!*/ name, MemberGroup/*!*/ res) {
            // we only get base methods when we're looking for a type from a normal .NET type.  If it's a NewTypeMaker
            // type then we don't want them as we'll bind to the NewTypeMaker methods which override the base helper
            // methods.
            if (res.Count > 0 && PythonTypeOps.GetFinalSystemType(type) == type) {
                List<MemberTracker> newMembers = null;

                IList<MethodInfo> overriddenMethods = NewTypeMaker.GetOverriddenMethods(type, name);
                
                if (overriddenMethods.Count > 0) {
                    if (newMembers == null) {
                        newMembers = new List<MemberTracker>();
                    }

                    foreach (MethodInfo mi in overriddenMethods) {
                        newMembers.Add(MemberTracker.FromMemberInfo(mi));
                    }
                }

                IList<ExtensionPropertyTracker> overriddenProperties = NewTypeMaker.GetOverriddenProperties(type, name);
                if (overriddenProperties.Count > 0) {
                    if (newMembers == null) {
                        newMembers = new List<MemberTracker>();
                    }

                    foreach (ExtensionPropertyTracker tracker in overriddenProperties) {
                        newMembers.Add(tracker);
                    }
                }

                if (newMembers != null) {
                    newMembers.InsertRange(0, res);
                    res = new MemberGroup(newMembers.ToArray());
                }
            }            
            return res;
        }

        private static MethodInfo[] GetMethodSet(string name, int expected) {
            MethodInfo[] methods = typeof(InstanceOps).GetMethods();
            MethodInfo[] filtered = new MethodInfo[expected];
            int j = 0;
            for (int i = 0; i < methods.Length; i++) {
                if (methods[i].Name == name) {
                    filtered[j++] = methods[i];
#if !DEBUG
                    if (j == expected) break;
#endif
                }
            }
            Debug.Assert(j == expected);
            return filtered;
        }

        #endregion
    }
}
