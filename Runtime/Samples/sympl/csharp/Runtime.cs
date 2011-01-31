using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Utils;
using System.Linq;
using System.Runtime.CompilerServices;
using Path = System.IO.Path;
using File = System.IO.File;
using Directory = System.IO.Directory;
using Debug = System.Diagnostics.Debug;


namespace SymplSample {

    // RuntimeHelpers is a collection of functions that perform operations at
    // runtime of Sympl code, such as performing an import or eq.
    //
    public static class RuntimeHelpers {
        // SymplImport takes the runtime and module as context for the import.
        // It takes a list of names, what, that either identify a (possibly dotted
        // sequence) of names to fetch from Globals or a file name to load.  Names
        // is a list of names to fetch from the final object that what indicates
        // and then set each name in module.  Renames is a list of names to add to
        // module instead of names.  If names is empty, then the name set in
        // module is the last name in what.  If renames is not empty, it must have
        // the same cardinality as names.
        //
        public static object SymplImport(Sympl runtime, IDynamicMetaObjectProvider module,
                                         string[] what, string[] names,
                                         string[] renames) {
            // Get object or file scope.
            object value = null;
            if (what.Length == 1) {
                string name = what[0];
                if (DynamicObjectHelpers.HasMember(runtime.Globals, name)) {
                    value = DynamicObjectHelpers.GetMember(runtime.Globals, name);
                // Since runtime.Globals has Sympl's reflection of namespaces and
                // types, we pick those up first above and don't risk hitting a
                // NamespaceTracker for assemblies added when we initialized Sympl.
                // The next check will correctly look up case-INsensitively for
                // globals the host adds to ScriptRuntime.Globals.
                } else if (DynamicObjectHelpers.HasMember(runtime.DlrGlobals, name)) {
                    value = DynamicObjectHelpers.GetMember(runtime.DlrGlobals, name);
                } else {
                    string f = (string)(DynamicObjectHelpers
                                            .GetMember(module, "__file__"));
                    f = Path.Combine(Path.GetDirectoryName(f), name + ".sympl");
                    if (File.Exists(f)) {
                        value = runtime.ExecuteFile(f);
                    } else {
                        throw new ArgumentException(
                            "Import: can't find name in globals " +
                            "or as file to load -- " + name + " " + f);
                    }
                }
            } else {
                // What has more than one name, must be Globals access.
                value = runtime.Globals;
                // For more correctness and generality, shouldn't assume all
                // globals are dynamic objects, or that a look up like foo.bar.baz
                // cascades through all dynamic objects.
                // Would need to manually create a CallSite here with Sympl's
                // GetMemberBinder, and think about a caching strategy per name.
                foreach (string name in what) {
                    value = DynamicObjectHelpers.GetMember(
                                (IDynamicMetaObjectProvider)value, name);
                }
            }
            // Assign variables in module.
            if (names.Length == 0) {
                DynamicObjectHelpers.SetMember((IDynamicMetaObjectProvider)module,
                                               what[what.Length - 1], value);
            } else {
                if (renames.Length == 0) renames = names;
                for (int i = 0; i < names.Length; i++) {
                    string name = names[i];
                    string rename = renames[i];
                    DynamicObjectHelpers.SetMember(
                        (IDynamicMetaObjectProvider)module, rename,
                        DynamicObjectHelpers.GetMember(
                             (IDynamicMetaObjectProvider)value, name));
                }
            }
            return null;
        } // SymplImport

        // Uses of the 'eq' keyword form in Sympl compile to a call to this
        // helper function.
        //
        public static bool SymplEq (object x, object y) {
            if (x == null)
                return y == null;
            else if (y == null)
                return x == null;
            else {
                var xtype = x.GetType();
                var ytype = y.GetType();
                if (xtype.IsPrimitive && xtype != typeof(string) &&
                    ytype.IsPrimitive && ytype != typeof(string))
                    return x.Equals(y);
                else
                    return object.ReferenceEquals(x, y);
            }
        }

        // Uses of the 'cons' keyword form in Sympl compile to a call to this
        // helper function.
        //
        public static Cons MakeCons (object x, object y) {
            return new Cons(x, y);
        }

        // Gets the i-th element in the Cons list.
        //
        public static object GetConsElt(Cons lst, int i) {
            return NthCdr(lst, i).First;
        }

        // Sets the i-th element in the Cons list with the specified value.
        //
        public static object SetConsElt(Cons lst, int i, object value) {
            lst = NthCdr(lst, i);
            lst.First = value;
            return value;
        }

        private static Cons NthCdr (Cons lst, int i) {
            while (i > 0 && lst != null) {
                lst = lst.Rest as Cons;
                i--;
            }
            if (i == 0 && lst != null) {
                return lst;
            } else {
                throw new ArgumentOutOfRangeException("i");
            }
        }


        //////////////////////////////////////////////////
        // Array Utilities (slicing) and some LINQ helpers
        //////////////////////////////////////////////////

        public static T[] RemoveFirstElt<T>(IList<T> list) {
            // Make array ...
            if (list.Count == 0) {
                return new T[0];
            }
            T[] res = new T[list.Count];
            list.CopyTo(res, 0);
            // Shift result
            return ShiftLeft(res, 1);
        }

        public static T[] RemoveFirstElt<T>(T[] array) {
            return ShiftLeft(array, 1);
        }

        private static T[] ShiftLeft<T>(T[] array, int count) {
            //ContractUtils.RequiresNotNull(array, "array");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            T[] result = new T[array.Length - count];
            System.Array.Copy(array, count, result, 0, result.Length);
            return result;
        }

        public static T[] RemoveLast<T>(T[] array) {
            //ContractUtils.RequiresNotNull(array, "array");
            System.Array.Resize(ref array, array.Length - 1);
            return array;
        }
      
  
        ///////////////////////////////////////
        // Utilities used by binders at runtime
        ///////////////////////////////////////

        // ParamsMatchArgs returns whether the args are assignable to the parameters.
        // We specially check for our TypeModel that wraps .NET's RuntimeType, and
        // elsewhere we detect the same situation to convert the TypeModel for calls.
        //
        // Consider checking p.IsByRef and returning false since that's not CLS.
        //
        // Could check for a.HasValue and a.Value is None and
        // ((paramtype is class or interface) or (paramtype is generic and
        // nullable<t>)) to support passing nil anywhere.
        //
        public static bool ParametersMatchArguments(ParameterInfo[] parameters,
                                                    DynamicMetaObject[] args) {
            // We only call this after filtering members by this constraint.
            Debug.Assert(args.Length == parameters.Length,
                         "Internal: args are not same len as params?!");
            for (int i = 0; i < args.Length; i++) {
                var paramType = parameters[i].ParameterType;
                // We consider arg of TypeModel and param of Type to be compatible.
                if (paramType == typeof(Type) &&
                    (args[i].LimitType == typeof(TypeModel))) {
                    continue;
                }
                if (!paramType
                    // Could check for HasValue and Value==null AND
                    // (paramtype is class or interface) or (is generic
                    // and nullable<T>) ... to bind nullables and null.
                        .IsAssignableFrom(args[i].LimitType)) {
                    return false;
                }
            }
            return true;
        }

        // Returns a DynamicMetaObject with an expression that fishes the .NET
        // RuntimeType object from the TypeModel MO.
        //
        public static DynamicMetaObject GetRuntimeTypeMoFromModel(
                                              DynamicMetaObject typeModelMO) {
            Debug.Assert((typeModelMO.LimitType == typeof(TypeModel)),
                         "Internal: MO is not a TypeModel?!");
            // Get tm.ReflType
            var pi = typeof(TypeModel).GetProperty("ReflType");
            Debug.Assert(pi != null);
            return new DynamicMetaObject(
                Expression.Property(
                    Expression.Convert(typeModelMO.Expression, typeof(TypeModel)),
                    pi),
                typeModelMO.Restrictions.Merge(
                    BindingRestrictions.GetTypeRestriction(
                        typeModelMO.Expression, typeof(TypeModel)))//,
                // Must supply a value to prevent binder FallbackXXX methods
                // from infinitely looping if they do not check this MO for
                // HasValue == false and call Defer.  After Sympl added Defer
                // checks, we could verify, say, FallbackInvokeMember by no
                // longer passing a value here.
                //((TypeModel)typeModelMO.Value).ReflType
            );
        }

        // Returns list of Convert exprs converting args to param types.  If an arg
        // is a TypeModel, then we treat it special to perform the binding.  We need
        // to map from our runtime model to .NET's RuntimeType object to match.
        //
        // To call this function, args and pinfos must be the same length, and param
        // types must be assignable from args.
        //
        // NOTE, if using this function, then need to use GetTargetArgsRestrictions
        // and make sure you're performing the same conversions as restrictions.
        //
        public static Expression[] ConvertArguments(
                                 DynamicMetaObject[] args, ParameterInfo[] ps) {
            Debug.Assert(args.Length == ps.Length,
                         "Internal: args are not same len as params?!");
            Expression[] callArgs = new Expression[args.Length];
            for (int i = 0; i < args.Length; i++) {
                Expression argExpr = args[i].Expression;
                if (args[i].LimitType == typeof(TypeModel) && 
                    ps[i].ParameterType == typeof(Type)) {
                    // Get arg.ReflType
                    argExpr = GetRuntimeTypeMoFromModel(args[i]).Expression;
                }
                argExpr = Expression.Convert(argExpr, ps[i].ParameterType);
                callArgs[i] = argExpr;
            }
            return callArgs;
        }

        // GetTargetArgsRestrictions generates the restrictions needed for the
        // MO resulting from binding an operation.  This combines all existing
        // restrictions and adds some for arg conversions.  targetInst indicates
        // whether to restrict the target to an instance (for operations on type
        // objects) or to a type (for operations on an instance of that type).
        //
        // NOTE, this function should only be used when the caller is converting
        // arguments to the same types as these restrictions.
        //
        public static BindingRestrictions GetTargetArgsRestrictions(
                DynamicMetaObject target, DynamicMetaObject[] args,
                bool instanceRestrictionOnTarget){
            // Important to add existing restriction first because the
            // DynamicMetaObjects (and possibly values) we're looking at depend
            // on the pre-existing restrictions holding true.
            var restrictions = target.Restrictions.Merge(BindingRestrictions
                                                            .Combine(args));
            if (instanceRestrictionOnTarget) {
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetInstanceRestriction(
                        target.Expression,
                        target.Value
                    ));
            } else {
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetTypeRestriction(
                        target.Expression,
                        target.LimitType
                    ));
            }
            for (int i = 0; i < args.Length; i++) {
                BindingRestrictions r;
                if (args[i].HasValue && args[i].Value == null) {
                    r = BindingRestrictions.GetInstanceRestriction(
                            args[i].Expression, null);
                } else {
                    r = BindingRestrictions.GetTypeRestriction(
                            args[i].Expression, args[i].LimitType);
                }
                restrictions = restrictions.Merge(r);
            }
            return restrictions;
        }

        // Return the expression for getting target[indexes]
        //
        // Note, callers must ensure consistent restrictions are added for
        // the conversions on args and target.
        //
        public static Expression GetIndexingExpression(
                                      DynamicMetaObject target,
                                      DynamicMetaObject[] indexes) {
            Debug.Assert(target.HasValue && target.LimitType != typeof(Array));

            var indexExpressions = indexes.Select(
                i => Expression.Convert(i.Expression, i.LimitType))
                .ToArray();

            // CONS
            if (target.LimitType == typeof(Cons)) {
                // Call RuntimeHelper.GetConsElt
                var args = new List<Expression>();
                // The first argument is the list
                args.Add(
                    Expression.Convert(
                        target.Expression, 
                        target.LimitType)
                );
                args.AddRange(indexExpressions);
                return Expression.Call(
                    typeof(RuntimeHelpers),
                    "GetConsElt",
                    null,
                    args.ToArray());
            // ARRAY
            } else if (target.LimitType.IsArray) {
                return Expression.ArrayAccess(
                    Expression.Convert(target.Expression,
                                       target.LimitType),
                    indexExpressions
                );
             // INDEXER
            } else {
                var props = target.LimitType.GetProperties();
                var indexers = props.
                    Where(p => p.GetIndexParameters().Length > 0).ToArray();
                indexers = indexers.
                    Where(idx => idx.GetIndexParameters().Length == 
                                 indexes.Length).ToArray();

                var res = new List<PropertyInfo>();
                foreach (var idxer in indexers) {
                    if (RuntimeHelpers.ParametersMatchArguments(
                                          idxer.GetIndexParameters(), indexes)) {
                        // all parameter types match
                        res.Add(idxer);
                    }
                }
                if (res.Count == 0) {
                    return Expression.Throw(
                        Expression.New(
                            typeof(MissingMemberException)
                                .GetConstructor(new Type[] { typeof(string) }),
                            Expression.Constant(
                               "Can't bind because there is no matching indexer.")
                        )
                    );
                }
                return Expression.MakeIndex(
                    Expression.Convert(target.Expression, target.LimitType),
                    res[0], indexExpressions);
            }
        }

        // CreateThrow is a convenience function for when binders cannot bind.
        // They need to return a DynamicMetaObject with appropriate restrictions
        // that throws.  Binders never just throw due to the protocol since
        // a binder or MO down the line may provide an implementation.
        //
        // It returns a DynamicMetaObject whose expr throws the exception, and 
        // ensures the expr's type is object to satisfy the CallSite return type
        // constraint.
        //
        // A couple of calls to CreateThrow already have the args and target
        // restrictions merged in, but BindingRestrictions.Merge doesn't add 
        // duplicates.
        //
        public static DynamicMetaObject CreateThrow
                (DynamicMetaObject target, DynamicMetaObject[] args,
                 BindingRestrictions moreTests,
                 Type exception, params object[] exceptionArgs) {
            Expression[] argExprs = null;
            Type[] argTypes = Type.EmptyTypes;
            int i;
            if (exceptionArgs != null) {
                i = exceptionArgs.Length;
                argExprs = new Expression[i];
                argTypes = new Type[i];
                i = 0;
                foreach (object o in exceptionArgs) {
                    Expression e = Expression.Constant(o);
                    argExprs[i] = e;
                    argTypes[i] = e.Type;
                    i += 1;
                }
            }
            ConstructorInfo constructor = exception.GetConstructor(argTypes);
            if (constructor == null) {
                throw new ArgumentException(
                    "Type doesn't have constructor with a given signature");
            }
            return new DynamicMetaObject(
                Expression.Throw(
                    Expression.New(constructor, argExprs),
                    // Force expression to be type object so that DLR CallSite
                    // code things only type object flows out of the CallSite.
                    typeof(object)),
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
                                   .Merge(moreTests));
        }

        // EnsureObjectResult wraps expr if necessary so that any binder or
        // DynamicMetaObject result expression returns object.  This is required
        // by CallSites.
        //
        public static Expression EnsureObjectResult (Expression expr) {
            if (! expr.Type.IsValueType)
                return expr;
            if (expr.Type == typeof(void))
                return Expression.Block(
                           expr, Expression.Default(typeof(object)));
            else
                return Expression.Convert(expr, typeof(object));
        }

    } // RuntimeHelpers



    //#####################################################
    //# Dynamic Helpers for HasMember, GetMember, SetMember
    //#####################################################

    // DynamicObjectHelpers provides access to IDynObj members given names as
    // data at runtime.  When the names are known at compile time (o.foo), then
    // they get baked into specific sites with specific binders that encapsulate
    // the name.  We need this in python because hasattr et al are case-sensitive.
    //
    class DynamicObjectHelpers {

        static private object _sentinel = new object();
        static internal object Sentinel { get { return _sentinel; } }

        internal static bool HasMember(IDynamicMetaObjectProvider o,
                                       string name) {
            return (DynamicObjectHelpers.GetMember(o, name) !=
                    DynamicObjectHelpers.Sentinel);
            //Alternative impl used when EOs had bug and didn't call fallback ...
            //var mo = o.GetMetaObject(Expression.Parameter(typeof(object), null));
            //foreach (string member in mo.GetDynamicMemberNames()) {
            //    if (string.Equals(member, name, StringComparison.OrdinalIgnoreCase)) {
            //        return true;
            //    }
            //}
            //return false;
        }

        static private Dictionary<string,
                                  CallSite<Func<CallSite, object, object>>>
            _getSites = new Dictionary<string,
                                       CallSite<Func<CallSite, object, object>>>();

        internal static object GetMember(IDynamicMetaObjectProvider o,
                                         string name) {
            CallSite<Func<CallSite, object, object>> site;
            if (! DynamicObjectHelpers._getSites.TryGetValue(name, out site)) {
                site = CallSite<Func<CallSite, object, object>>
                               .Create(new DoHelpersGetMemberBinder(name));
                DynamicObjectHelpers._getSites[name] = site;
            }
            return site.Target(site, o);
        }

        static private Dictionary<string,
                                  CallSite<Action<CallSite, object, object>>>
            _setSites = new Dictionary<string,
                                       CallSite<Action<CallSite, object, object>>>();

        internal static void SetMember(IDynamicMetaObjectProvider o, string name,
                                       object value) {
            CallSite<Action<CallSite, object, object>> site;
            if (! DynamicObjectHelpers._setSites.TryGetValue(name, out site)) {
                site = CallSite<Action<CallSite, object, object>>
                          .Create(new DoHelpersSetMemberBinder(name));
                DynamicObjectHelpers._setSites[name] = site;
            }
            site.Target(site, o, value);
        }

    }

    class DoHelpersGetMemberBinder : GetMemberBinder {

        internal DoHelpersGetMemberBinder(string name) : base(name, true) { }

        public override DynamicMetaObject FallbackGetMember(
                DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            return errorSuggestion ?? 
                   new DynamicMetaObject(
                           Expression.Constant(DynamicObjectHelpers.Sentinel),
                           target.Restrictions.Merge(
                               BindingRestrictions.GetTypeRestriction(
                                   target.Expression, target.LimitType)));

        }
    }

    class DoHelpersSetMemberBinder : SetMemberBinder {
        internal DoHelpersSetMemberBinder(string name) : base(name, true) { }

        public override DynamicMetaObject FallbackSetMember(
                DynamicMetaObject target, DynamicMetaObject value,
                DynamicMetaObject errorSuggestion) {
            return errorSuggestion ??
                   RuntimeHelpers.CreateThrow(
                       target, null, BindingRestrictions.Empty,
                       typeof(MissingMemberException),
                              "If IDynObj doesn't support setting members, " + 
                              "DOHelpers can't do it for the IDO.");
        }
    }


    
    //########################
    // General Runtime Binders
    //########################

    // SymplGetMemberBinder is used for general dotted expressions for fetching
    // members.
    //
    public class SymplGetMemberBinder : GetMemberBinder {
        public SymplGetMemberBinder(string name) : base(name, true) {
        }

        public override DynamicMetaObject FallbackGetMember(
                DynamicMetaObject targetMO, DynamicMetaObject errorSuggestion) {
            // First try COM binding.
            DynamicMetaObject result;
            if (ComBinder.TryBindGetMember(this, targetMO, out result, true)) {
                return result;
            }
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!targetMO.HasValue) return Defer(targetMO);
            // Find our own binding.
            var flags = BindingFlags.IgnoreCase | BindingFlags.Static | 
                        BindingFlags.Instance | BindingFlags.Public;
            var members = targetMO.LimitType.GetMember(this.Name, flags);
            if (members.Length == 1) {
                return new DynamicMetaObject(
                    RuntimeHelpers.EnsureObjectResult(
                      Expression.MakeMemberAccess(
                        Expression.Convert(targetMO.Expression,
                                           members[0].DeclaringType),
                        members[0])),
                    // Don't need restriction test for name since this
                    // rule is only used where binder is used, which is
                    // only used in sites with this binder.Name.
                    BindingRestrictions.GetTypeRestriction(targetMO.Expression,
                                                           targetMO.LimitType));
            } else {
                return errorSuggestion ??
                    RuntimeHelpers.CreateThrow(
                        targetMO, null, 
                        BindingRestrictions.GetTypeRestriction(targetMO.Expression,
                                                               targetMO.LimitType),
                        typeof(MissingMemberException),
                        "cannot bind member, " + this.Name +
                            ", on object " + targetMO.Value.ToString());
            }
        }
    }

    // SymplSetMemberBinder is used for general dotted expressions for setting
    // members.
    //
    public class SymplSetMemberBinder : SetMemberBinder {
        public SymplSetMemberBinder(string name)
            : base(name, true) {
        }

        public override DynamicMetaObject FallbackSetMember(
                DynamicMetaObject targetMO, DynamicMetaObject value,
                DynamicMetaObject errorSuggestion) {
            // First try COM binding.
            DynamicMetaObject result;
            if (ComBinder.TryBindSetMember(this, targetMO, value, out result)) {
                return result;
            }
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!targetMO.HasValue) return Defer(targetMO);
            // Find our own binding.
            var flags = BindingFlags.IgnoreCase | BindingFlags.Static | 
                        BindingFlags.Instance | BindingFlags.Public;
            var members = targetMO.LimitType.GetMember(this.Name, flags);
            if (members.Length == 1) {
                MemberInfo mem = members[0];
                Expression val;
                // Should check for member domain type being Type and value being
                // TypeModel, similar to ConvertArguments, and building an
                // expression like GetRuntimeTypeMoFromModel.
                if (mem.MemberType == MemberTypes.Property)
                    val = Expression.Convert(value.Expression,
                                             ((PropertyInfo)mem).PropertyType);
                else if (mem.MemberType == MemberTypes.Field)
                    val = Expression.Convert(value.Expression,
                                             ((FieldInfo)mem).FieldType);
                else
                    return (errorSuggestion ??
                            RuntimeHelpers.CreateThrow(
                                targetMO, null,
                                BindingRestrictions.GetTypeRestriction(
                                    targetMO.Expression,
                                    targetMO.LimitType),
                                typeof(InvalidOperationException),
                                "Sympl only supports setting Properties and " +
                                "fields at this time."));
                return new DynamicMetaObject(
                    // Assign returns the stored value, so we're good for Sympl.
                    RuntimeHelpers.EnsureObjectResult(
                      Expression.Assign(
                        Expression.MakeMemberAccess(
                            Expression.Convert(targetMO.Expression,
                                               members[0].DeclaringType),
                            members[0]),
                        val)),
                    // Don't need restriction test for name since this
                    // rule is only used where binder is used, which is
                    // only used in sites with this binder.Name.                    
                    BindingRestrictions.GetTypeRestriction(targetMO.Expression,
                                                           targetMO.LimitType));
            } else {
                return errorSuggestion ??
                    RuntimeHelpers.CreateThrow(
                        targetMO, null, 
                        BindingRestrictions.GetTypeRestriction(targetMO.Expression,
                                                               targetMO.LimitType),
                        typeof(MissingMemberException),
                         "IDynObj member name conflict.");
            }
        }
    }

	// SymplInvokeMemberBinder is used for general dotted expressions in function
	// calls for invoking members.
	//
    public class SymplInvokeMemberBinder : InvokeMemberBinder {
        public SymplInvokeMemberBinder(string name, CallInfo callinfo) 
            : base(name, true, callinfo) { // true = ignoreCase
        }

        public override DynamicMetaObject FallbackInvokeMember(
                DynamicMetaObject targetMO, DynamicMetaObject[] args,
                DynamicMetaObject errorSuggestion) {
            // First try COM binding.
            DynamicMetaObject result;
            if (ComBinder.TryBindInvokeMember(this, targetMO, args, out result)) {
                return result;
            }
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!targetMO.HasValue || args.Any((a) => !a.HasValue)) {
                var deferArgs = new DynamicMetaObject[args.Length + 1];
                for (int i = 0; i < args.Length; i++) {
                    deferArgs[i + 1] = args[i];
                }
                deferArgs[0] = targetMO;
                return Defer(deferArgs);
            }
            // Find our own binding.
            // Could consider allowing invoking static members from an instance.
            var flags = BindingFlags.IgnoreCase | BindingFlags.Instance |
                        BindingFlags.Public;
            var members = targetMO.LimitType.GetMember(this.Name, flags);
            if ((members.Length == 1) && (members[0] is PropertyInfo ||
                                          members[0] is FieldInfo)) {
                // NEED TO TEST, should check for delegate value too
                var mem = members[0];
                throw new NotImplementedException();
                //return new DynamicMetaObject(
                //    Expression.Dynamic(
                //        new SymplInvokeBinder(new CallInfo(args.Length)),
                //        typeof(object),
                //        args.Select(a => a.Expression).AddFirst(
                //               Expression.MakeMemberAccess(this.Expression, mem)));

                // Don't test for eventinfos since we do nothing with them now.
            } else {
                // Get MethodInfos with right arg counts.
                var mi_mems = members.
                    Select(m => m as MethodInfo).
                    Where(m => m is MethodInfo &&
                               ((MethodInfo)m).GetParameters().Length ==
                                   args.Length);
                // Get MethodInfos with param types that work for args.  This works
                // except for value args that need to pass to reftype params. 
                // We could detect that to be smarter and then explicitly StrongBox
                // the args.
                List<MethodInfo> res = new List<MethodInfo>();
                foreach (var mem in mi_mems) {
                    if (RuntimeHelpers.ParametersMatchArguments(
                                           mem.GetParameters(), args)) {
                        res.Add(mem);
                    }
                }
                // False below means generate a type restriction on the MO.
                // We are looking at the members targetMO's Type.
                var restrictions = RuntimeHelpers.GetTargetArgsRestrictions(
                                                      targetMO, args, false);
                if (res.Count == 0) {
                    return errorSuggestion ??
                        RuntimeHelpers.CreateThrow(
                            targetMO, args, restrictions,
                            typeof(MissingMemberException),
                            "Can't bind member invoke -- " + args.ToString());
                }
                // restrictions and conversion must be done consistently.
                var callArgs = RuntimeHelpers.ConvertArguments(
                                                 args, res[0].GetParameters());
                return new DynamicMetaObject(
                   RuntimeHelpers.EnsureObjectResult(
                     Expression.Call(
                        Expression.Convert(targetMO.Expression, 
                                           targetMO.LimitType), 
                        res[0], callArgs)),
                   restrictions);
                // Could hve tried just letting Expr.Call factory do the work,
                // but if there is more than one applicable method using just
                // assignablefrom, Expr.Call throws.  It does not pick a "most
                // applicable" method or any method.
            }
        }

        public override DynamicMetaObject FallbackInvoke(
                DynamicMetaObject targetMO, DynamicMetaObject[] args,
                DynamicMetaObject errorSuggestion) {
            var argexprs = new Expression[args.Length + 1];
            for (int i = 0; i < args.Length; i++) {
                argexprs[i + 1] = args[i].Expression;
            }
            argexprs[0] = targetMO.Expression;
            // Just "defer" since we have code in SymplInvokeBinder that knows
            // what to do, and typically this fallback is from a language like
            // Python that passes a DynamicMetaObject with HasValue == false.
            return new DynamicMetaObject(
                           Expression.Dynamic(
                               // This call site doesn't share any L2 caching
                               // since we don't call GetInvokeBinder from Sympl.
                               // We aren't plumbed to get the runtime instance here.
                               new SymplInvokeBinder(new CallInfo(args.Length)),
                               typeof(object), // ret type
                               argexprs),
                           // No new restrictions since SymplInvokeBinder will handle it.
                           targetMO.Restrictions.Merge(
                               BindingRestrictions.Combine(args)));
        }
    }

    public class SymplInvokeBinder : InvokeBinder {
        public SymplInvokeBinder(CallInfo callinfo) : base(callinfo) {
        }

        public override DynamicMetaObject FallbackInvoke(
                DynamicMetaObject targetMO, DynamicMetaObject[] argMOs,
                DynamicMetaObject errorSuggestion) {
            // First try COM binding.
            DynamicMetaObject result;
            if (ComBinder.TryBindInvoke(this, targetMO, argMOs, out result)) {
                return result;
            }
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!targetMO.HasValue || argMOs.Any((a) => !a.HasValue)) {
                var deferArgs = new DynamicMetaObject[argMOs.Length + 1];
                for (int i = 0; i < argMOs.Length; i++) {
                    deferArgs[i + 1] = argMOs[i];
                }
                deferArgs[0] = targetMO;
                return Defer(deferArgs);
            }
            // Find our own binding.
            if (targetMO.LimitType.IsSubclassOf(typeof(Delegate))) {
                var parms = targetMO.LimitType.GetMethod("Invoke").GetParameters();
                if (parms.Length == argMOs.Length) {
                    // Don't need to check if argument types match parameters.
                    // If they don't, users get an argument conversion error.
                    var callArgs = RuntimeHelpers.ConvertArguments(argMOs, parms);
                    var expression = Expression.Invoke(
                        Expression.Convert(targetMO.Expression, targetMO.LimitType),
                        callArgs);
                    return new DynamicMetaObject(
                        RuntimeHelpers.EnsureObjectResult(expression),
                        BindingRestrictions.GetTypeRestriction(targetMO.Expression,
                                                               targetMO.LimitType));
                }
            }
            return errorSuggestion ??
                RuntimeHelpers.CreateThrow(
                    targetMO, argMOs, 
                    BindingRestrictions.GetTypeRestriction(targetMO.Expression,
                                                           targetMO.LimitType),
                    typeof(InvalidOperationException),
                    "Wrong number of arguments for function -- " +
                    targetMO.LimitType.ToString() + " got " + argMOs.ToString());

        }
    }


    public class SymplCreateInstanceBinder : CreateInstanceBinder {
        public SymplCreateInstanceBinder(CallInfo callinfo)
            : base(callinfo) {
        }

        public override DynamicMetaObject FallbackCreateInstance(
                                                DynamicMetaObject target,
                                                DynamicMetaObject[] args,
                                                DynamicMetaObject errorSuggestion) {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue || args.Any((a) => !a.HasValue)) {
                var deferArgs = new DynamicMetaObject[args.Length + 1];
                for (int i = 0; i < args.Length; i++) {
                    deferArgs[i + 1] = args[i];
                }
                deferArgs[0] = target;
                return Defer(deferArgs);
            }
            // Make sure target actually contains a Type.
            if (!typeof(Type).IsAssignableFrom(target.LimitType)) {
                    return errorSuggestion ??
                        RuntimeHelpers.CreateThrow(
                           target, args, BindingRestrictions.Empty,
                           typeof(InvalidOperationException),
                                  "Type object must be used when creating instance -- " + 
                                   args.ToString());
            }
            var type = target.Value as Type;
            Debug.Assert(type != null);
            var constructors = type.GetConstructors();
            // Get constructors with right arg counts.
            var ctors = constructors.
                Where(c => c.GetParameters().Length == args.Length);
            List<ConstructorInfo> res = new List<ConstructorInfo>();
            foreach (var c in ctors) {
                if (RuntimeHelpers.ParametersMatchArguments(c.GetParameters(),
                                                            args)) {
                    res.Add(c);
                }
            }
            // We generate an instance restriction on the target since it is a
            // Type and the constructor is associate with the actual Type instance.
            var restrictions =
                RuntimeHelpers.GetTargetArgsRestrictions(
                    target, args, true);
            if (res.Count == 0) {
                return errorSuggestion ??
                    RuntimeHelpers.CreateThrow(
                       target, args, restrictions,
                       typeof(MissingMemberException),
                              "Can't bind create instance -- " + args.ToString());
            }
            var ctorArgs =
                RuntimeHelpers.ConvertArguments(
                args, res[0].GetParameters());
            return new DynamicMetaObject(
                // Creating an object, so don't need EnsureObjectResult.
                Expression.New(res[0], ctorArgs),
               restrictions);
        }
    }

    public class SymplGetIndexBinder : GetIndexBinder {
        public SymplGetIndexBinder(CallInfo callinfo)
            : base(callinfo) {
        }

        public override DynamicMetaObject FallbackGetIndex(
                     DynamicMetaObject target, DynamicMetaObject[] indexes,
                     DynamicMetaObject errorSuggestion) {
            // First try COM binding.
            DynamicMetaObject result;
            if (ComBinder.TryBindGetIndex(this, target, indexes, out result)) {
                return result;
            }
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue || indexes.Any((a) => !a.HasValue)) {
                var deferArgs = new DynamicMetaObject[indexes.Length + 1];
                for (int i = 0; i < indexes.Length; i++) {
                    deferArgs[i + 1] = indexes[i];
                }
                deferArgs[0] = target;
                return Defer(deferArgs);
            }
            // Give good error for Cons.
            if (target.LimitType == typeof(Cons)) {
                if (indexes.Length != 1)
                    return errorSuggestion ??
                        RuntimeHelpers.CreateThrow(
                             target, indexes, BindingRestrictions.Empty,
                             typeof(InvalidOperationException),
                             "Indexing list takes single index.  " + "Got " + 
                             indexes.Length.ToString());
            }
            // Find our own binding.
            //
            // Conversions created in GetIndexExpression must be consistent with
            // restrictions made in GetTargetArgsRestrictions.
            var indexingExpr = RuntimeHelpers.EnsureObjectResult(
                                  RuntimeHelpers.GetIndexingExpression(target,
                                                                       indexes));
            var restrictions = RuntimeHelpers.GetTargetArgsRestrictions(
                                                  target, indexes, false);
            return new DynamicMetaObject(indexingExpr, restrictions);
        }
    }


    public class SymplSetIndexBinder : SetIndexBinder {
        public SymplSetIndexBinder(CallInfo callinfo)
            : base(callinfo) {
        }

        public override DynamicMetaObject FallbackSetIndex(
                   DynamicMetaObject target, DynamicMetaObject[] indexes,
                   DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
            // First try COM binding.
            DynamicMetaObject result;
            if (ComBinder.TryBindSetIndex(this, target, indexes, value, out result)) {
                return result;
            }
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue || indexes.Any((a) => !a.HasValue) ||
                !value.HasValue) {
                var deferArgs = new DynamicMetaObject[indexes.Length + 2];
                for (int i = 0; i < indexes.Length; i++) {
                    deferArgs[i + 1] = indexes[i];
                }
                deferArgs[0] = target;
                deferArgs[indexes.Length + 1] = value;
                return Defer(deferArgs);
            }
            // Find our own binding.
            Expression valueExpr = value.Expression;
            //we convert a value of TypeModel to Type.
            if (value.LimitType == typeof(TypeModel)) {
                valueExpr = RuntimeHelpers.GetRuntimeTypeMoFromModel(value).Expression;
            }
            Debug.Assert(target.HasValue && target.LimitType != typeof(Array));
            Expression setIndexExpr;
            if (target.LimitType == typeof(Cons)) {
                if (indexes.Length != 1) {
                    return errorSuggestion ??
                        RuntimeHelpers.CreateThrow(
                             target, indexes, BindingRestrictions.Empty,
                             typeof(InvalidOperationException),
                             "Indexing list takes single index.  " + "Got " + indexes);
                }
                // Call RuntimeHelper.SetConsElt
                List<Expression> args = new List<Expression>();
                // The first argument is the list
                args.Add(
                    Expression.Convert(
                        target.Expression,
                        target.LimitType)
                );
                // The second argument is the index.
                args.Add(Expression.Convert(indexes[0].Expression,
                                            indexes[0].LimitType));
                // The last argument is the value
                args.Add(Expression.Convert(valueExpr, typeof(object)));
                // Sympl helper returns value stored.
                setIndexExpr = Expression.Call(
                    typeof(RuntimeHelpers),
                    "SetConsElt",
                    null,
                    args.ToArray());
            } else {
                Expression indexingExpr = RuntimeHelpers.GetIndexingExpression(
                                                            target, indexes);
                // Assign returns the stored value, so we're good for Sympl.
                setIndexExpr = Expression.Assign(indexingExpr, valueExpr);
            }

            BindingRestrictions restrictions =
                 RuntimeHelpers.GetTargetArgsRestrictions(target, indexes, false);
            return new DynamicMetaObject(
                RuntimeHelpers.EnsureObjectResult(setIndexExpr),
                restrictions);

        }
    }
    
    public class SymplBinaryOperationBinder : BinaryOperationBinder {
        public SymplBinaryOperationBinder(ExpressionType operation)
            : base(operation) {
        }

        public override DynamicMetaObject FallbackBinaryOperation(
                    DynamicMetaObject target, DynamicMetaObject arg,
                    DynamicMetaObject errorSuggestion) {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue || !arg.HasValue) {
                return Defer(target, arg);
            }
            var restrictions = target.Restrictions.Merge(arg.Restrictions)
                .Merge(BindingRestrictions.GetTypeRestriction(
                    target.Expression, target.LimitType))
                .Merge(BindingRestrictions.GetTypeRestriction(
                    arg.Expression, arg.LimitType));
            return new DynamicMetaObject(
                RuntimeHelpers.EnsureObjectResult(
                  Expression.MakeBinary(
                    this.Operation,
                    Expression.Convert(target.Expression, target.LimitType),
                    Expression.Convert(arg.Expression, arg.LimitType))),
                restrictions
            );
        }
    }

    public class SymplUnaryOperationBinder : UnaryOperationBinder {
        public SymplUnaryOperationBinder(ExpressionType operation)
            : base(operation) {
        }

        public override DynamicMetaObject FallbackUnaryOperation(
                   DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            // Defer if any object has no value so that we evaulate their
            // Expressions and nest a CallSite for the InvokeMember.
            if (!target.HasValue) {
                return Defer(target);
            }
            return new DynamicMetaObject(
                RuntimeHelpers.EnsureObjectResult(
                  Expression.MakeUnary(
                    this.Operation,
                    Expression.Convert(target.Expression, target.LimitType),
                    target.LimitType)),
                target.Restrictions.Merge(
                    BindingRestrictions.GetTypeRestriction(
                        target.Expression, target.LimitType)));
        }
    }



    //////////////////////////
    // Cons Cells and Symbols
    //////////////////////////

    public class Symbol {
        private string _name = "";
        private object _value = null;
        private Cons _plist = null;

        internal Symbol(string name) {
            _name = name;
        }


        // Need ToString when Sympl program passing Symbol to Console.WriteLine.
        // Otherwise, it prints as internal IPy constructed type.
        //
        public override string ToString()
        {
            return _name;
        }

        public string Name
        {
            get { return _name; }
            // C# forces property set and assignments to return void, 
            // so need to code gen explicit value return.
            set { _name = value; }
        }

        public object Value
        {
            get { return _value; }
            // C# forces property set and assignments to return void, 
            // so need to code gen explicit value return.
            set { _value = value; }
        }

        public Cons PList
        {
            get { return _plist; }
            // C# forces property set and assignments to return void, so
            // need to code gen explicit value return.
            set { _plist = value; }
        }
    }

    public class Cons {
        private object _first = null;
        private object _rest = null;

        public Cons(object first, object rest) {
            _first = first;
            _rest = rest;
        }

        // NOTE: does not handle circularities!
        //
        public override string ToString() {
            var head = this;
            string res = "(";
            while (head != null) {
                res = res + head._first.ToString();
                if (head._rest == null) {
                    head = null;
                } else {
                    Cons rest = head._rest as Cons;
                    if (rest != null) {
                        head = rest;
                        res = res + " ";
                    } else {
                        res = res + " . " + head._rest.ToString();
                        head = null;
                    }
                }
            }
            return res + ")";
        }

        public object First {
            get { return _first; }
            // C# forces property set and assignments to return void, 
            // so need to code gen explicit value return.
            set { _first = value; }
        }

        public object Rest {
            get { return _rest; }
            // C# forces property set and assignments to return void, 
            // so need to code gen explicit value return.
            set { _rest = value; }
        }

        // Runtime helper method.
        //
        public static Cons _List(params object[] elements) {
            if (elements.Length == 0) return null;
            Cons head = new Cons(elements[0], null);
            Cons tail = head;
            foreach (object elt in RuntimeHelpers.RemoveFirstElt(elements)) {
                tail.Rest = new Cons(elt, null);
                tail = tail.Rest as Cons;
            }
            return head;
        }
    } // Cons class

} // namespace
