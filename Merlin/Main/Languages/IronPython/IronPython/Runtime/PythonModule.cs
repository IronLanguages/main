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
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime {
    /// <summary>
    /// Python module.  Stores classes, functions, and data.  Usually a module
    /// is created by importing a file or package from disk.  But a module can also
    /// be directly created by calling the module type and providing a name or
    /// optionally a documentation string.
    /// </summary>
    [PythonType("module")]
    public class PythonModule : IDynamicMetaObjectProvider, IPythonMembersList {
        private readonly PythonDictionary _dict;
        private Scope _scope;
        
        public PythonModule() {
            _dict = new PythonDictionary();
        }

        /// <summary>
        /// Creates a new module backed by a Scope.  Used for creating modules for foreign Scope's.
        /// </summary>
        internal PythonModule(Scope scope) {
            _dict = new PythonDictionary(new ScopeDictionaryStorage(scope));
            _scope = scope;
        }

        /// <summary>
        /// Creates a new module backed by a Scope.  Used for creating modules for Python code.
        /// </summary>
        internal PythonModule(PythonDictionary dict, Scope scope) {
            _dict = dict;
            _scope = scope;
        }

        /// <summary>
        /// Creates a new PythonModule with the specified dictionary.
        /// 
        /// Used for creating modules for builtin modules which don't have any code associated with them.
        /// </summary>
        internal PythonModule(PythonDictionary dict) {
            _dict = dict;
        }

        public static PythonModule/*!*/ __new__(CodeContext/*!*/ context, PythonType/*!*/ cls, params object[]/*!*/ args\u00F8) {
            PythonModule res;
            if (cls == TypeCache.Module) {
                res = new PythonModule();
            } else if (cls.IsSubclassOf(TypeCache.Module)) {
                res = (PythonModule)cls.CreateInstance(context);
            } else {
                throw PythonOps.TypeError("{0} is not a subtype of module", cls.Name);
            }

            return res;
        }

        [StaticExtensionMethod]
        public static PythonModule/*!*/ __new__(CodeContext/*!*/ context, PythonType/*!*/ cls, [ParamDictionary]PythonDictionary kwDict\u00F8, params object[]/*!*/ args\u00F8) {
            return __new__(context, cls, args\u00F8);
        }

        public void __init__(string name) {
            __init__(name, null);
        }

        public void __init__(string name, string documentation) {
            _dict["__name__"] = name;

            if (documentation != null) {
                _dict["__doc__"] = documentation;
            }
        }

        public object __getattribute__(CodeContext/*!*/ context, string name) {
            switch (name) {
                // never look in the dict for these...
                case "__dict__": return __dict__;
                case "__class__": return DynamicHelpers.GetPythonType(this);
            }

            object res;
            if (_dict.TryGetValue(name, out res)) {
                return res;
            }

            // fall back to object to provide all of our other attributes (e.g. __setattr__, etc...)
            return ObjectOps.__getattribute__(context, this, name);
        }

        internal object GetAttributeNoThrow(CodeContext/*!*/ context, string name) {
            switch (name) {
                // never look in the dict for these...
                case "__dict__": return __dict__;
                case "__class__": return DynamicHelpers.GetPythonType(this);
            }

            object res;
            if (_dict.TryGetValue(name, out res)) {
                return res;
            }

            // fall back to object to provide all of our other attributes (e.g. __setattr__, etc...)
            try {
                return ObjectOps.__getattribute__(context, this, name);
            } catch (MissingMemberException) {
                return OperationFailed.Value;
            }
        }

        public void __setattr__(string name, object value) {
            Debug.Assert(value != Uninitialized.Instance);

            _dict[name] = value;
        }

        public void __delattr__(string name) {
            switch (name) {
                case "__dict__": throw PythonOps.TypeError("readonly attribute");
                case "__class__": throw PythonOps.TypeError("can't delete __class__ attribute");
            }

            object value;
            if (!_dict.TryRemoveValue(name, out value)) {
                throw PythonOps.AttributeErrorForMissingAttribute("module", SymbolTable.StringToId(name));
            }
        }

        public string/*!*/ __repr__() {
            return __str__();
        }

        public string/*!*/ __str__() {
            object fileObj, nameObj;
            if (!_dict.TryGetValue("__file__", out fileObj)) {
                fileObj = null;
            }
            if (!_dict._storage.TryGetName(out nameObj)) {
                nameObj = null;
            }
            
            string file = fileObj as string;
            string name = nameObj as string ?? "?";

            if (file == null) {
                return String.Format("<module '{0}' (built-in)>", name);
            }
            return String.Format("<module '{0}' from '{1}'>", name, file);
        }

        public PythonDictionary __dict__ {
            get {
                return _dict;
            }
        }

        internal Scope Scope {
            get {
                if (_scope == null) {
                    Interlocked.CompareExchange(ref _scope, new Scope(_dict), null);
                }

                return _scope;
            }
        }
        
        #region IDynamicMetaObjectProvider Members

        [PythonHidden] // needs to be public so that we can override it.
        public DynamicMetaObject GetMetaObject(Expression parameter) {
            return new MetaModule(this, parameter);
        }

        #endregion

        class MetaModule : DynamicMetaObject {
            public MetaModule(PythonModule module, Expression self)
                : base(self, BindingRestrictions.Empty, module) {
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                if (binder.Name == "__dict__") {
                    return new DynamicMetaObject(
                        Expression.Property(
                            Utils.Convert(Expression, typeof(PythonModule)),
                            typeof(PythonModule).GetProperty("__dict__")
                        ),
                        BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                    );

                }

                var tmp = Expression.Variable(typeof(object), "res");
                
                return new DynamicMetaObject(
                    Expression.Block(
                        new[] { tmp },
                        Expression.Condition(
                            Expression.Call(
                                typeof(PythonOps).GetMethod("ModuleGetMember"),
                                Utils.Convert(Expression, typeof(PythonModule)),
                                Expression.Constant(binder.Name),
                                tmp
                            ),
                            tmp,
                            binder.FallbackGetMember(this).Expression
                        )
                    ),
                    BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                );
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                Debug.Assert(value.Value != Uninitialized.Instance);

                switch (binder.Name) {
                    case "__dict__":
                        return new DynamicMetaObject(
                            Expression.Throw(
                                Expression.Call(
                                    typeof(PythonOps).GetMethod("TypeError"),
                                    Expression.Constant("readonly attribute"),
                                    Expression.NewArrayInit(typeof(object))
                                ),
                                typeof(object)
                            ),
                            BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                        );
                    case "__class__":
                        return new DynamicMetaObject(
                            Expression.Throw(
                                Expression.Call(
                                    typeof(PythonOps).GetMethod("TypeError"),
                                    Expression.Constant("__class__ assignment: only for heap types"),
                                    Expression.NewArrayInit(typeof(object))
                                ),
                                typeof(object)
                            ),
                            BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                        );
                }

                return new DynamicMetaObject(
                    Expression.Call(
                        typeof(PythonOps).GetMethod("ModuleSetMember"),
                        Utils.Convert(Expression, typeof(PythonModule)),
                        Expression.Constant(binder.Name),
                        value.Expression
                    ),
                    BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                );
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                switch (binder.Name) {
                    case "__dict__":
                        return new DynamicMetaObject(
                            Expression.Throw(
                                Expression.Call(
                                    typeof(PythonOps).GetMethod("TypeError"),
                                    Expression.Constant("can't set attributes of built-in/extension type 'module'"),
                                    Expression.NewArrayInit(typeof(object))
                                )
                            ),
                            BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                        );
                    case "__class__":
                        return new DynamicMetaObject(
                            Expression.Throw(
                                Expression.Call(
                                    typeof(PythonOps).GetMethod("TypeError"),
                                    Expression.Constant("can't delete __class__ attribute"),
                                    Expression.NewArrayInit(typeof(object))
                                )
                            ),
                            BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                        );
                }

                return new DynamicMetaObject(
                    Expression.Condition(
                        Expression.Call(
                            typeof(PythonOps).GetMethod("ModuleDeleteMember"),
                            Utils.Convert(Expression, typeof(PythonModule)),
                            Expression.Constant(binder.Name)
                        ),
                        Expression.Default(typeof(object)),
                        binder.FallbackDeleteMember(this).Expression
                    ),
                    BindingRestrictions.GetTypeRestriction(Expression, Value.GetType())
                );
            }

            public override IEnumerable<string> GetDynamicMemberNames() {
                foreach (object o in ((PythonModule)Value).__dict__.Keys) {
                    string str = o as string;
                    if (str != null) {
                        yield return str;
                    }
                }
            }
        }

        internal string GetFile() {
            object res;
            if (_dict.TryGetValue("__file__", out res)) {
                return res as string;
            }
            return null;
        }

        internal string GetName() {
            object res;
            if (_dict._storage.TryGetName(out res)) {
                return res as string;
            }
            return null;
        }

        #region IPythonMembersList Members

        IList<object> IPythonMembersList.GetMemberNames(CodeContext context) {
            return new List<object>(__dict__.Keys);
        }

        #endregion

        #region IMembersList Members

        IList<string> IMembersList.GetMemberNames() {
            List<string> res = new List<string>(__dict__.Keys.Count);
            foreach (object o in __dict__.Keys) {
                string strKey = o as string;
                if (strKey != null) {
                    res.Add(strKey);
                }
            }

            return res;
        }

        #endregion
    }
}
