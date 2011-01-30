using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Linq;

using Scope = Microsoft.Scripting.Runtime.Scope;

using System.Reflection;
using System.IO;



namespace SymplSample {

    public class Sympl {

        private IList<Assembly> _assemblies;
        private ExpandoObject _globals = new ExpandoObject();
        private Scope _dlrGlobals;
        private Dictionary<string, Symbol> Symbols = 
            new Dictionary<string, Symbol>();

        public Sympl(IList<Assembly> assms, Scope dlrGlobals) {
            _assemblies = assms;
            _dlrGlobals = dlrGlobals;
            AddAssemblyNamesAndTypes();
        }

        // _addNamespacesAndTypes builds a tree of ExpandoObjects representing
        // .NET namespaces, with TypeModel objects at the leaves.  Though Sympl is
        // case-insensitive, we store the names as they appear in .NET reflection
        // in case our globals object or a namespace object gets passed as an IDO
        // to another language or library, where they may be looking for names
        // case-sensitively using EO's default lookup.
        //
        public void AddAssemblyNamesAndTypes() {
            foreach (var assm in _assemblies) {
                foreach (var typ in assm.GetExportedTypes()) {
                    string[] names = typ.FullName.Split('.');
                    var table = _globals;
                    for (int i = 0; i < names.Length - 1; i++) {
                        string name = names[i].ToLower();
                        if (DynamicObjectHelpers.HasMember(
                               (IDynamicMetaObjectProvider)table, name)) {
                            // Must be Expando since only we have put objs in
                            // the tables so far.
                            table = (ExpandoObject)(DynamicObjectHelpers
                                                       .GetMember(table, name));
                        } else {
                            var tmp = new ExpandoObject();
                            DynamicObjectHelpers.SetMember(table, name, tmp);
                            table = tmp;
                        }
                    }
                    DynamicObjectHelpers.SetMember(table, names[names.Length - 1],
                                                   new TypeModel(typ));
                }
            }
        }

        // ExecuteFile executes the file in a new module scope and stores the
        // scope on Globals, using either the provided name, globalVar, or the
        // file's base name.  This function returns the module scope.
        //
        public IDynamicMetaObjectProvider ExecuteFile(string filename) {
            return ExecuteFile(filename, null);
        }

        public IDynamicMetaObjectProvider ExecuteFile(string filename,
                                                      string globalVar) {
            var moduleEO = CreateScope();
            ExecuteFileInScope(filename, moduleEO);

            globalVar = globalVar ?? Path.GetFileNameWithoutExtension(filename);
            DynamicObjectHelpers.SetMember(this._globals, globalVar, moduleEO);

            return moduleEO;
        }

        // ExecuteFileInScope executes the file in the given module scope.  This
        // does NOT store the module scope on Globals.  This function returns
        // nothing.
        //
        public void ExecuteFileInScope(string filename,
                                       IDynamicMetaObjectProvider moduleEO) {
            var f = new StreamReader(filename);
            // Simple way to convey script rundir for RuntimeHelpes.SymplImport
            // to load .sympl files.
            DynamicObjectHelpers.SetMember(moduleEO, "__file__", 
                                           Path.GetFullPath(filename));
            try {
                var moduleFun = ParseFileToLambda(filename, f);
                var d = moduleFun.Compile();
                d(this, moduleEO);
            } finally {
                f.Close();
            }
        }

        internal Expression<Func<Sympl, IDynamicMetaObjectProvider, object>>
                 ParseFileToLambda(string filename, TextReader reader) {
            var asts = new Parser().ParseFile(reader);
            var scope = new AnalysisScope(
                null,
                filename,
                this,
                Expression.Parameter(typeof(Sympl), "symplRuntime"),
                Expression.Parameter(typeof(IDynamicMetaObjectProvider),
                                     "fileModule"));
            List<Expression> body = new List<Expression>();
            foreach (var e in asts) {
                body.Add(ETGen.AnalyzeExpr(e, scope));
            }
            body.Add(Expression.Constant(null));
            var moduleFun = Expression.Lambda<Func<Sympl, IDynamicMetaObjectProvider,
                                                   object>>(
                Expression.Block(body),
                scope.RuntimeExpr,
                scope.ModuleExpr);
            return moduleFun;
        }

        // Execute a single expression parsed from string in the provided module
        // scope and returns the resulting value.
        //
        public object ExecuteExpr(string expr_str,
                                  IDynamicMetaObjectProvider moduleEO) {
            var moduleFun = ParseExprToLambda(new StringReader(expr_str));
            var d = moduleFun.Compile();
            return d(this, moduleEO);
        }

        internal Expression<Func<Sympl, IDynamicMetaObjectProvider, object>>
                 ParseExprToLambda(TextReader reader) {
            var ast = new Parser().ParseExpr(reader);
            var scope = new AnalysisScope(
                null,
                "__snippet__",
                this,
                Expression.Parameter(typeof(Sympl), "symplRuntime"),
                Expression.Parameter(typeof(IDynamicMetaObjectProvider),
                                     "fileModule"));
            List<Expression> body = new List<Expression>();
            body.Add(Expression.Convert(ETGen.AnalyzeExpr(ast, scope),
                                        typeof(object)));
            var moduleFun = Expression.Lambda<Func<Sympl, IDynamicMetaObjectProvider,
                                                   object>>(
                Expression.Block(body),
                scope.RuntimeExpr,
                scope.ModuleExpr
            );
            return moduleFun;
        }


        public IDynamicMetaObjectProvider Globals { get { return _globals; } }
        public IDynamicMetaObjectProvider DlrGlobals { get { return _dlrGlobals; } }

        public static ExpandoObject CreateScope() {
            return new ExpandoObject();
        }
        
        // Symbol returns the Symbol interned in this runtime if it is already
        // there.  If not, this makes the Symbol and interns it.
        //
        public Symbol MakeSymbol(string name) {
            string downname = name.ToLower();
            if (Symbols.ContainsKey(downname)) {
                return Symbols[downname];
            } else {
                Symbol s = new Symbol(name);
                Symbols[downname] = s;
                return s;
            }
        }


        /////////////////////////
        // Canonicalizing Binders
        /////////////////////////

        // We need to canonicalize binders so that we can share L2 dynamic
        // dispatch caching across common call sites.  Every call site with the
        // same operation and same metadata on their binders should return the
        // same rules whenever presented with the same kinds of inputs.  The
        // DLR saves the L2 cache on the binder instance.  If one site somewhere
        // produces a rule, another call site performing the same operation with
        // the same metadata could get the L2 cached rule rather than computing
        // it again.  For this to work, we need to place the same binder instance
        // on those functionally equivalent call sites.

        private Dictionary<string, SymplGetMemberBinder> _getMemberBinders =
            new Dictionary<string, SymplGetMemberBinder>();
        public SymplGetMemberBinder GetGetMemberBinder (string name) {
            lock (_getMemberBinders) {
                // Don't lower the name.  Sympl is case-preserving in the metadata
                // in case some DynamicMetaObject ignores ignoreCase.  This makes
                // some interop cases work, but the cost is that if a Sympl program
                // spells ".foo" and ".Foo" at different sites, they won't share rules.
                if (_getMemberBinders.ContainsKey(name))
                    return _getMemberBinders[name];
                var b = new SymplGetMemberBinder(name);
                _getMemberBinders[name] = b;
                return b;
            }
        }

        private Dictionary<string, SymplSetMemberBinder> _setMemberBinders =
            new Dictionary<string, SymplSetMemberBinder>();
        public SymplSetMemberBinder GetSetMemberBinder (string name) {
            lock (_setMemberBinders) {
                // Don't lower the name.  Sympl is case-preserving in the metadata
                // in case some DynamicMetaObject ignores ignoreCase.  This makes
                // some interop cases work, but the cost is that if a Sympl program
                // spells ".foo" and ".Foo" at different sites, they won't share rules.
                if (_setMemberBinders.ContainsKey(name))
                    return _setMemberBinders[name];
                var b = new SymplSetMemberBinder(name);
                _setMemberBinders[name] = b;
                return b;
            }
        }

        private Dictionary<CallInfo, SymplInvokeBinder> _invokeBinders =
            new Dictionary<CallInfo, SymplInvokeBinder>();
        public SymplInvokeBinder GetInvokeBinder (CallInfo info) {
            lock (_invokeBinders) {
                if (_invokeBinders.ContainsKey(info))
                    return _invokeBinders[info];
                var b = new SymplInvokeBinder(info);
                _invokeBinders[info] = b;
                return b;
            }
        }

        private Dictionary<InvokeMemberBinderKey, SymplInvokeMemberBinder>
            _invokeMemberBinders =
                new Dictionary<InvokeMemberBinderKey, SymplInvokeMemberBinder>();
        public SymplInvokeMemberBinder GetInvokeMemberBinder
                (InvokeMemberBinderKey info) {
            lock (_invokeMemberBinders) {
                if (_invokeMemberBinders.ContainsKey(info))
                    return _invokeMemberBinders[info];
                var b = new SymplInvokeMemberBinder(info.Name, info.Info);
                _invokeMemberBinders[info] = b;
                return b;
            }
        }

        private Dictionary<CallInfo, SymplCreateInstanceBinder> 
            _createInstanceBinders =
                new Dictionary<CallInfo, SymplCreateInstanceBinder>();
        public SymplCreateInstanceBinder GetCreateInstanceBinder(CallInfo info) {
            lock (_createInstanceBinders) {
                if (_createInstanceBinders.ContainsKey(info))
                    return _createInstanceBinders[info];
                var b = new SymplCreateInstanceBinder(info);
                _createInstanceBinders[info] = b;
                return b;
            }
        }

        private Dictionary<CallInfo, SymplGetIndexBinder> _getIndexBinders =
            new Dictionary<CallInfo, SymplGetIndexBinder>();
        public SymplGetIndexBinder GetGetIndexBinder(CallInfo info) {
            lock (_getIndexBinders) {
                if (_getIndexBinders.ContainsKey(info))
                    return _getIndexBinders[info];
                var b = new SymplGetIndexBinder(info);
                _getIndexBinders[info] = b;
                return b;
            }
        }

        private Dictionary<CallInfo, SymplSetIndexBinder> _setIndexBinders =
            new Dictionary<CallInfo, SymplSetIndexBinder>();
        public SymplSetIndexBinder GetSetIndexBinder(CallInfo info) {
            lock (_setIndexBinders) {
                if (_setIndexBinders.ContainsKey(info))
                    return _setIndexBinders[info];
                var b = new SymplSetIndexBinder(info);
                _setIndexBinders[info] = b;
                return b;
            }
        }

        private Dictionary<ExpressionType, SymplBinaryOperationBinder>
            _binaryOperationBinders =
                new Dictionary<ExpressionType, SymplBinaryOperationBinder>();
        public SymplBinaryOperationBinder GetBinaryOperationBinder
                (ExpressionType op) {
            lock (_binaryOperationBinders) {
                if (_binaryOperationBinders.ContainsKey(op))
                    return _binaryOperationBinders[op];
                var b = new SymplBinaryOperationBinder(op);
                _binaryOperationBinders[op] = b;
                return b;
            }
        }

        private Dictionary<ExpressionType, SymplUnaryOperationBinder>
            _unaryOperationBinders =
                new Dictionary<ExpressionType, SymplUnaryOperationBinder>();
        public SymplUnaryOperationBinder GetUnaryOperationBinder
                (ExpressionType op) {
            lock (_unaryOperationBinders) {
                if (_unaryOperationBinders.ContainsKey(op))
                    return _unaryOperationBinders[op];
                var b = new SymplUnaryOperationBinder(op);
                _unaryOperationBinders[op] = b;
                return b;
            }
        }

    } // Sympl


    // This class is needed to canonicalize InvokeMemberBinders in Sympl.  See
    // the comment above the GetXXXBinder methods at the end of the Sympl class.
    //
    public class InvokeMemberBinderKey {
        string _name;
        CallInfo _info;
        
        public InvokeMemberBinderKey(string name, CallInfo info) {
            _name = name;
            _info = info;
        }

        public string Name { get { return _name; } }
        public CallInfo Info { get { return _info; } }

        public override bool Equals(object obj) {
            InvokeMemberBinderKey key = obj as InvokeMemberBinderKey;
            // Don't lower the name.  Sympl is case-preserving in the metadata
            // in case some DynamicMetaObject ignores ignoreCase.  This makes
            // some interop cases work, but the cost is that if a Sympl program
            // spells ".foo" and ".Foo" at different sites, they won't share rules.
            return key != null && key._name == _name && key._info.Equals(_info);
        }

        public override int GetHashCode() {
            // Stolen from DLR sources when it overrode GetHashCode on binders.
            return 0x28000000 ^ _name.GetHashCode() ^ _info.GetHashCode();
        }

    } //InvokeMemberBinderKey



    ////////////////////////////////////
    // TypeModel and TypeModelMetaObject
    ////////////////////////////////////

    // TypeModel wraps System.Runtimetypes. When Sympl code encounters
    // a type leaf node in Sympl.Globals and tries to invoke a member, wrapping
    // the ReflectionTypes in TypeModels allows member access to get the type's
    // members and not ReflectionType's members.
    //
    public class TypeModel : IDynamicMetaObjectProvider {
        private Type _reflType;

        public TypeModel(Type type) {
            _reflType = type;
        }

        public Type ReflType { get { return _reflType; } }

        DynamicMetaObject IDynamicMetaObjectProvider
                              .GetMetaObject(Expression parameter) {
            return new TypeModelMetaObject(parameter, this);
        }
    }

    public class TypeModelMetaObject : DynamicMetaObject {
        private TypeModel _typeModel;

        public TypeModel TypeModel { get { return _typeModel; } }
        public Type ReflType { get { return _typeModel.ReflType; } }

        // Constructor takes ParameterExpr to reference CallSite, and a TypeModel
        // that the new TypeModelMetaObject represents.
        //
        public TypeModelMetaObject(Expression objParam, TypeModel typeModel)
            : base(objParam, BindingRestrictions.Empty, typeModel) {
                _typeModel = typeModel;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
            var flags = BindingFlags.IgnoreCase | BindingFlags.Static | 
                        BindingFlags.Public;
            // consider BindingFlags.Instance if want to return wrapper for
            // inst members that is callable.
            var members = ReflType.GetMember(binder.Name, flags);
            if (members.Length == 1) {
                return new DynamicMetaObject(
                    // We always access static members for type model objects, so the
                    // first argument in MakeMemberAccess should be null.
                    RuntimeHelpers.EnsureObjectResult(
                        Expression.MakeMemberAccess(
                            null,
                            members[0])),
                    // Don't need restriction test for name since this
                    // rule is only used where binder is used, which is
                    // only used in sites with this binder.Name.
                    this.Restrictions.Merge(
                        BindingRestrictions.GetInstanceRestriction(
                            this.Expression,
                            this.Value)
                    )
                );
            } else {
                return binder.FallbackGetMember(this);
            }
        }

        // Because we don't ComboBind over several MOs and operations, and no one
        // is falling back to this function with MOs that have no values, we
        // don't need to check HasValue.  If we did check, and HasValue == False,
        // then would defer to new InvokeMemberBinder.Defer().
        //
        public override DynamicMetaObject BindInvokeMember(
                InvokeMemberBinder binder, DynamicMetaObject[] args) {
            var flags = BindingFlags.IgnoreCase | BindingFlags.Static |
                        BindingFlags.Public;
            var members = ReflType.GetMember(binder.Name, flags);
            if ((members.Length == 1) && (members[0] is PropertyInfo || 
                                          members[0] is FieldInfo)){
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
                // for except for value args that need to pass to reftype params. 
                // We could detect that to be smarter and then explicitly StrongBox
                // the args.
                List<MethodInfo> res = new List<MethodInfo>();
                foreach (var mem in mi_mems) {
                    if (RuntimeHelpers.ParametersMatchArguments(
                                           mem.GetParameters(), args)) {
                        res.Add(mem);
                    }
                }
                if (res.Count == 0) {
                    // Sometimes when binding members on TypeModels the member
                    // is an instance member since the Type is an instance of Type.
                    // We fallback to the binder with the Type instance to see if
                    // it binds.  The SymplInvokeMemberBinder does handle this.
                    var typeMO = RuntimeHelpers.GetRuntimeTypeMoFromModel(this);
                    var result = binder.FallbackInvokeMember(typeMO, args, null);
                    return result;
                }
                // True below means generate an instance restriction on the MO.
                // We are only looking at the members defined in this Type instance.
                var restrictions = RuntimeHelpers.GetTargetArgsRestrictions(
                    this, args, true);
                // restrictions and conversion must be done consistently.
                var callArgs = 
                    RuntimeHelpers.ConvertArguments(
                    args, res[0].GetParameters());
                return new DynamicMetaObject(
                   RuntimeHelpers.EnsureObjectResult(
                       Expression.Call(res[0], callArgs)),
                   restrictions);
                // Could hve tried just letting Expr.Call factory do the work,
                // but if there is more than one applicable method using just
                // assignablefrom, Expr.Call throws.  It does not pick a "most
                // applicable" method or any method.

            }
        }

        public override DynamicMetaObject BindCreateInstance(
                   CreateInstanceBinder binder, DynamicMetaObject[] args) {
            var constructors = ReflType.GetConstructors();
            var ctors = constructors.
                Where(c => c.GetParameters().Length == args.Length);
            List<ConstructorInfo> res = new List<ConstructorInfo>();
            foreach (var c in ctors) {
                if (RuntimeHelpers.ParametersMatchArguments(c.GetParameters(),
                                                            args)) {
                    res.Add(c);
                }
            }
            if (res.Count == 0) {
                // Binders won't know what to do with TypeModels, so pass the
                // RuntimeType they represent.  The binder might not be Sympl's.
                return binder.FallbackCreateInstance(
                                  RuntimeHelpers.GetRuntimeTypeMoFromModel(this),
                                  args);
            }
            // For create instance of a TypeModel, we can create a instance 
            // restriction on the MO, hence the true arg.
            var restrictions = RuntimeHelpers.GetTargetArgsRestrictions(
                                this, args, true);
            var ctorArgs =
                RuntimeHelpers.ConvertArguments(
                args, res[0].GetParameters());
            return new DynamicMetaObject(
                // Creating an object, so don't need EnsureObjectResult.
                Expression.New(res[0], ctorArgs),
               restrictions);
        }
    }//TypeModelMetaObject
}