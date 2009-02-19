/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Interpretation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using System.Dynamic;
using Microsoft.Scripting.Math;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Generation;

namespace IronRuby.Runtime {

    public class CallSiteStorage<TCallSiteFunc> : SiteLocalStorage<CallSite<TCallSiteFunc>> where TCallSiteFunc : class {
        public CallSite<TCallSiteFunc>/*!*/ GetCallSite(string/*!*/ methodName, int argumentCount) {
            return RubyUtils.GetCallSite(ref Data, methodName, argumentCount);
        }

        public CallSite<TCallSiteFunc>/*!*/ GetCallSite(string/*!*/ methodName, RubyCallSignature signature) {
            return RubyUtils.GetCallSite(ref Data, methodName, signature);
        }
    }

    public class BinaryOpStorage : CallSiteStorage<Func<CallSite, RubyContext, object, object, object>> {
        public CallSite<Func<CallSite, RubyContext, object, object, object>>/*!*/ GetCallSite(string/*!*/ methodName) {
            return GetCallSite(methodName, 1);
        }
    }

    public class UnaryOpStorage : CallSiteStorage<Func<CallSite, RubyContext, object, object>> {
        public CallSite<Func<CallSite, RubyContext, object, object>>/*!*/ GetCallSite(string/*!*/ methodName) {
            return GetCallSite(methodName, 0);
        }
    }

    public class RespondToStorage : CallSiteStorage<Func<CallSite, RubyContext, object, SymbolId, object>> {
        public CallSite<Func<CallSite, RubyContext, object, SymbolId, object>>/*!*/ GetCallSite() {
            return GetCallSite("respond_to?", 1);
        }
    }

    public class ConversionStorage<TResult> : CallSiteStorage<Func<CallSite, RubyContext, object, TResult>> {
        public CallSite<Func<CallSite, RubyContext, object, TResult>>/*!*/ GetSite(RubyConversionAction/*!*/ conversion) {
            return RubyUtils.GetCallSite(ref Data, conversion);
        }
    }


    public static class RubyUtils {
        #region Objects

        public static readonly int FalseObjectId = 0;
        public static readonly int TrueObjectId = 2;
        public static readonly int NilObjectId = 4;

        // TODO: this is not correct, because it won't call singleton "eql?" methods
        public static bool ValueEquals(object self, object other) {
            return object.Equals(self, other);
        }

        // TODO: this is not correct, because it won't call singleton "hash" methods
        public static int GetHashCode(object self) {
            return self != null ? self.GetHashCode() : RubyUtils.NilObjectId;
        }

        /// <summary>
        /// Determines whether the given object is a value type in Ruby (i.e. they have value equality)
        /// 
        /// In Ruby, immediate values are Fixnums, Symbols, true, false, and nil
        /// All of those have value equality, whereas other types like Bignum & Float have reference equality
        /// 
        /// TODO: currently we treat all .NET value types (except floating point) as if they were
        /// immediate values. Is this correct?
        /// </summary>
        public static bool IsRubyValueType(object obj) {
            return obj == null || obj is ValueType && !(obj is float || obj is double);
        }

        public static bool CanCreateSingleton(object obj) {
            return !(obj is int || obj is SymbolId || obj is float || obj is double || obj is BigInteger);
        }

        public static void RequiresNotFrozen(RubyContext/*!*/ context, object/*!*/ obj) {
            if (context.IsObjectFrozen(obj)) {
                throw RubyExceptions.CreateTypeError("can't modify frozen object");
            }
        }

        public static string/*!*/ GetClassName(RubyContext/*!*/ context, object self) {
            return context.GetClassOf(self).Name;
        }

        public static MutableString/*!*/ InspectObject(UnaryOpStorage/*!*/ inspectStorage, ConversionStorage<MutableString>/*!*/ tosStorage,
            RubyContext/*!*/ context, object obj) {

            using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(obj)) {
                if (handle == null) {
                    return MutableString.Create("...");
                }

                RubyClass objClass = context.GetClassOf(obj);
                MutableString str = MutableString.CreateMutable();
                str.Append("#<");
                str.Append(objClass.GetName(context));

                // Ruby prints 2*object_id for objects
                str.Append(':');
                AppendFormatHexObjectId(str, GetObjectId(context, obj));

                RubyInstanceData data = context.TryGetInstanceData(obj);
                if (data != null) {
                    var vars = data.GetInstanceVariablePairs();
                    bool first = true;
                    foreach (KeyValuePair<string, object> var in vars) {
                        if (first) {
                            str.Append(" ");
                            first = false;
                        } else {
                            str.Append(", ");
                        }
                        str.Append(var.Key);
                        str.Append("=");

                        var inspectSite = inspectStorage.GetCallSite("inspect");
                        object inspectedValue = inspectSite.Target(inspectSite, context, var.Value);

                        var tosSite = tosStorage.GetSite(ConvertToSAction.Instance);
                        str.Append(tosSite.Target(tosSite, context, inspectedValue));

                        str.TaintBy(var.Value, context);
                    }
                }
                str.Append(">");

                str.TaintBy(obj, context);
                return str;
            }
        }

        public static MutableString/*!*/ ObjectToMutableString(RubyContext/*!*/ context, object obj) {
            RubyClass objClass = context.GetClassOf(obj);
            MutableString str = MutableString.CreateMutable();
            str.Append("#<");
            str.Append(objClass.GetName(context));

            // Ruby prints 2*object_id for objects
            str.Append(':');
            AppendFormatHexObjectId(str, GetObjectId(context, obj));

            str.Append(">");

            str.TaintBy(obj, context);
            return str;
        }

        public static MutableString/*!*/ ObjectToMutableString(UnaryOpStorage/*!*/ tosStorage, RubyContext/*!*/ context, object obj) {
            var site = tosStorage.GetCallSite("to_s");
            return site.Target(site, context, obj) as MutableString ?? ObjectToMutableString(context, obj);
        }

        public static MutableString/*!*/ AppendFormatHexObjectId(MutableString/*!*/ str, int objectId) {
            return str.AppendFormat("0x{0:x7}", 2 * objectId);
        }

        public static bool TryDuplicateObject(
            CallSiteStorage<Func<CallSite, RubyContext, object, object, object>>/*!*/ initializeCopyStorage,
            CallSiteStorage<Func<CallSite, RubyContext, RubyClass, object>>/*!*/ allocateStorage, 
            RubyContext/*!*/ context, object obj, bool cloneSemantics, out object copy) {

            // Ruby value types can't be cloned
            if (RubyUtils.IsRubyValueType(obj)) {
                copy = null;
                return false;
            }

            IDuplicable clonable = obj as IDuplicable;
            if (clonable != null) {
                copy = clonable.Duplicate(context, cloneSemantics);
            } else {
                // .NET classes and library clases that doesn't implement IDuplicable:
                var allocateSite = allocateStorage.GetCallSite("allocate", 0);
                copy = allocateSite.Target(allocateSite, context, context.GetClassOf(obj));

                context.CopyInstanceData(obj, copy, cloneSemantics);
            }

            var initializeCopySite = initializeCopyStorage.GetCallSite("initialize_copy", 1);
            initializeCopySite.Target(initializeCopySite, context, copy, obj);
            if (cloneSemantics) {
                context.FreezeObjectBy(copy, obj);
            }

            return true;
        }        

#if FALSE
        [MultiRuntimeAware]
        private static RecursionTracker/*!*/ _infiniteCopyTracker = new RecursionTracker();

        public static object DeepCopy(RubyContext/*!*/ context, object obj) {
            using (IDisposable handle = _infiniteCopyTracker.TrackObject(obj)) {
                if (handle == null) {
                    return RubyExceptions.CreateArgumentError("unable to deep copy recursive structure");
                } else {
                    RubyContext ec = RubyUtils.GetExecutionContext(context);

                    if (RubyUtils.IsRubyValueType(obj)) {
                        return obj;
                    }

                    object copy;

                    // TODO: special case class objects:
                    RubyClass classObject = obj as RubyClass;
                    if (classObject != null) {
                        copy = classObject.Duplicate();
                    } else {
                        copy = RubySites.Allocate(context, ec.GetClassOf(obj));
                    }

                    SymbolId[] names = ec.GetInstanceVariableNames(obj);
                    RubyInstanceData newVars = (names.Length > 0) ? ec.GetInstanceData(copy) : null;
                    foreach (SymbolId name in names) {
                        object value;
                        if (!ec.TryGetInstanceVariable(obj, name, out value)) {
                            value = null;
                        } else {
                            value = DeepCopy(context, value);
                        }
                        newVars.SetInstanceVariable(name, value);
                    }

                    if (classObject == null) {
                        // do any special copying needed for library types
                        // TODO: we still need to implement copy semantics for .NET types in general
                        IDuplicable duplicable = copy as IDuplicable;
                        if (duplicable != null) {
                            duplicable.InitializeFrom(obj);
                        }
                    }
                    return copy;
                }
            }
        }
#endif
        public static int GetFixnumId(int number) {
            return number * 2 + 1;
        }

        public static int GetObjectId(RubyContext/*!*/ context, object obj) {
            if (obj == null) return NilObjectId;
            if (obj is bool) return (bool)obj ? TrueObjectId : FalseObjectId;
            if (obj is int) return GetFixnumId((int)obj);

            return context.GetInstanceData(obj).ObjectId;
        }

        #endregion

        #region Names

        // Unmangles a method name. Not all names can be unmangled.
        // For a name to be unmangle-able, it must be lower_case_with_underscores.
        // If a name can't be unmangled, this function returns null
        public static string TryUnmangleName(string/*!*/ name) {
            if (name.ToUpper().Equals("INITIALIZE")) {
                // Special case for compatibility with CLR
                return name;
            }

            StringBuilder sb = new StringBuilder(name.Length);
            bool upcase = true;
            foreach (char c in name) {
                if (char.IsUpper(c)) {
                    // can't unmangle a name with uppercase letters
                    return null;
                }

                if (c == '_') {
                    if (upcase) {
                        // can't unmangle a name with consecutive or leading underscores
                        return null;
                    }
                    upcase = true;
                } else {
                    if (upcase) {
                        sb.Append(char.ToUpper(c));
                        upcase = false;
                    } else {
                        sb.Append(c);
                    }
                }
            }
            if (upcase) {
                // string was empty or ended with an underscore, can't unmangle
                return null;
            }
            return sb.ToString();
        }

        internal static string/*!*/ MangleName(string/*!*/ name) {
            Assert.NotNull(name);

            if (name.ToUpper().Equals("INITIALIZE")) {
                // Special case for compatibility with CLR
                return name;
            }

            StringBuilder result = new StringBuilder(name.Length);

            for (int i = 0; i < name.Length; i++) {
                if (Char.IsUpper(name[i])) {
                    if (!(i == 0 || i + 1 < name.Length && Char.IsUpper(name[i + 1]) || i + 1 == name.Length && Char.IsUpper(name[i - 1]))) {
                        result.Append('_');
                    }
                    result.Append(Char.ToLower(name[i]));
                } else {
                    result.Append(name[i]);
                }
            }

            return result.ToString();
        }

        public static string/*!*/ GetQualifiedName(Type/*!*/ type) {
            ContractUtils.RequiresNotNull(type, "type");

            StringBuilder sb = new StringBuilder();

            Type t = type;
            do {
                if (sb.Length > 0) {
                    sb.Insert(0, "::");
                }

                int tick = t.Name.LastIndexOf('`');
                if (tick != -1) {
                    sb.Insert(0, t.Name.Substring(0, tick));
                } else {
                    sb.Insert(0, t.Name);
                }

                t = t.DeclaringType;
            } while (t != null);

            if (type.Namespace != null) {
                sb.Insert(0, "::");
                sb.Insert(0, type.Namespace.Replace(Type.Delimiter.ToString(), "::"));
            }

            return sb.ToString();
        }

        public static string/*!*/ GetQualifiedName(NamespaceTracker/*!*/ namespaceTracker) {
            ContractUtils.RequiresNotNull(namespaceTracker, "namespaceTracker");
            if (namespaceTracker.Name == null) return String.Empty;

            return namespaceTracker.Name.Replace(Type.Delimiter.ToString(), "::");
        }

        public static void CheckConstantName(string name) {
            if (!Tokenizer.IsConstantName(name)) {
                throw RubyExceptions.CreateNameError(String.Format("`{0}' is not allowed as a constant name", name));
            }
        }

        public static void CheckClassVariableName(string name) {
            if (!Tokenizer.IsClassVariableName(name)) {
                throw RubyExceptions.CreateNameError(String.Format("`{0}' is not allowed as a class variable name", name));
            }
        }

        public static void CheckInstanceVariableName(string name) {
            if (!Tokenizer.IsInstanceVariableName(name)) {
                throw RubyExceptions.CreateNameError(String.Format("`{0}' is not allowed as an instance variable name", name));
            }
        }

        #endregion

        #region Constants, Methods

        // thread-safe:
        public static object GetConstant(RubyGlobalScope/*!*/ globalScope, RubyModule/*!*/ owner, string/*!*/ name, bool lookupObject) {
            Assert.NotNull(globalScope, owner, name);

            using (owner.Context.ClassHierarchyLocker()) {
                object result;
                if (owner.TryResolveConstantNoLock(globalScope, name, out result)) {
                    return result;
                }

                RubyClass objectClass = owner.Context.ObjectClass;
                if (owner != objectClass && lookupObject && objectClass.TryResolveConstantNoLock(globalScope, name, out result)) {
                    return result;
                }
            }

            CheckConstantName(name);
            return owner.Context.ConstantMissing(owner, name);
        }

        public static void SetConstant(RubyModule/*!*/ owner, string/*!*/ name, object value) {
            Assert.NotNull(owner, name);

            if (owner.SetConstantChecked(name, value)) {
                owner.Context.ReportWarning(String.Format("already initialized constant {0}", name));
            }

            // Initializes anonymous module's name:
            RubyModule module = value as RubyModule;
            if (module != null && module.Name == null) {
                module.Name = owner.MakeNestedModuleName(name);
            }
        }

        public static RubyMethodVisibility GetSpecialMethodVisibility(RubyMethodVisibility/*!*/ visibility, string/*!*/ methodName) {
            return (methodName == Symbols.Initialize || methodName == Symbols.InitializeCopy) ? RubyMethodVisibility.Private : visibility;
        }

        #endregion

        #region Modules, Classes

        internal static RubyModule/*!*/ DefineModule(RubyGlobalScope/*!*/ autoloadScope, RubyModule/*!*/ owner, string/*!*/ name) {
            Assert.NotNull(autoloadScope, owner);

            object existing;
            if (owner.TryGetConstant(autoloadScope, name, out existing)) {
                RubyModule module = existing as RubyModule;
                if (module == null || module.IsClass) {
                    throw RubyExceptions.CreateTypeError(String.Format("{0} is not a module", name));
                }
                return module;
            } else {
                // create class/module object:
                return owner.Context.DefineModule(owner, name);
            }
        }

        // thread-safe:
        internal static RubyClass/*!*/ DefineClass(RubyGlobalScope/*!*/ autoloadScope, RubyModule/*!*/ owner, string/*!*/ name, object superClassObject) {
            Assert.NotNull(owner);
            RubyClass superClass = ToSuperClass(owner.Context, superClassObject);

            object existing;
            if (ReferenceEquals(owner, owner.Context.ObjectClass)
                ? owner.TryResolveConstant(autoloadScope, name, out existing)
                : owner.TryGetConstant(autoloadScope, name, out existing)) {

                RubyClass cls = existing as RubyClass;
                if (cls == null || !cls.IsClass) {
                    throw RubyExceptions.CreateTypeError(String.Format("{0} is not a class", name));
                }

                if (superClassObject != null && !ReferenceEquals(cls.SuperClass, superClass)) {
                    throw RubyExceptions.CreateTypeError(String.Format("superclass mismatch for class {0}", name));
                }
                return cls;
            } else {
                return owner.Context.DefineClass(owner, name, superClass, null);
            }
        }

        private static RubyClass/*!*/ ToSuperClass(RubyContext/*!*/ ec, object superClassObject) {
            if (superClassObject != null) {
                RubyClass superClass = superClassObject as RubyClass;
                if (superClass == null) {
                    throw RubyExceptions.CreateTypeError(String.Format("superclass must be a Class ({0} given)", ec.GetClassOf(superClassObject).Name));
                }

                if (superClass.IsSingletonClass) {
                    throw RubyExceptions.CreateTypeError("can't make subclass of virtual class");
                }

                return superClass;
            } else {
                return ec.ObjectClass;
            }
        }

        internal static RubyModule/*!*/ GetModuleFromObject(RubyContext/*!*/ context, object obj) {
            Assert.NotNull(context);
            RubyModule module = obj as RubyModule;
            if (module == null) {
                throw RubyExceptions.CreateTypeError(String.Format("{0} is not a class/module", context.GetClassOf(obj)));
            }
            return module;
        }

        public static void RequireMixins(RubyModule/*!*/ target, params RubyModule[]/*!*/ modules) {
            foreach (RubyModule module in modules) {
                if (module == null) {
                    throw RubyExceptions.CreateTypeError("wrong argument type nil (expected Module)");
                }

                if (module == target) {
                    throw RubyExceptions.CreateArgumentError("cyclic include detected");
                }

                if (module.IsClass) {
                    throw RubyExceptions.CreateTypeError("wrong argument type Class (expected Module)");
                }

                if (module.Context != target.Context) {
                    throw RubyExceptions.CreateTypeError(String.Format("cannot mix a foreign module `{0}' into `{1}' (runtime mismatch)", 
                        module.GetName(target.Context), target.GetName(module.Context)
                    ));
                }
            }
        }

        #endregion

        #region Tracking operations that have the potential for infinite recursion

        public class RecursionTracker {
            [ThreadStatic]
            private Dictionary<object, bool> _infiniteTracker;

            private Dictionary<object, bool> TryPushInfinite(object obj) {
                if (_infiniteTracker == null) {
                    _infiniteTracker = new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance);
                }
                Dictionary<object, bool> infinite = _infiniteTracker;
                if (infinite.ContainsKey(obj)) {
                    return null;
                }
                infinite.Add(obj, true);
                return infinite;
            }

            public IDisposable TrackObject(object obj) {
                obj = BaseSymbolDictionary.NullToObj(obj);
                Dictionary<object, bool> tracker = TryPushInfinite(obj);
                return (tracker == null) ? null : new RecursionHandle(tracker, obj);
            }

            private class RecursionHandle : IDisposable {
                private readonly Dictionary<object, bool>/*!*/ _tracker;
                private readonly object _obj;

                internal RecursionHandle(Dictionary<object, bool>/*!*/ tracker, object obj) {
                    _tracker = tracker;
                    _obj = obj;
                }

                public void Dispose() {
                    _tracker.Remove(_obj);
                }
            }
        }

        [MultiRuntimeAware]
        private static readonly RecursionTracker _infiniteInspectTracker = new RecursionTracker();

        public static RecursionTracker InfiniteInspectTracker {
            get { return _infiniteInspectTracker; }
        }

        [MultiRuntimeAware]
        private static readonly RecursionTracker _infiniteToSTracker = new RecursionTracker();

        public static RecursionTracker InfiniteToSTracker {
            get { return _infiniteToSTracker; }
        }

        #endregion

        #region Arrays, Hashes

        // MRI checks for a subtype of RubyArray of subtypes of MutableString.
        internal static RubyArray AsArrayOfStrings(object value) {
            RubyArray array = value as RubyArray;
            if (array != null) {
                foreach (object obj in array) {
                    MutableString str = obj as MutableString;
                    if (str == null) {
                        return null;
                    }
                }
                return array;
            }
            return null;
        }

        public static object SetHashElement(RubyContext/*!*/ context, IDictionary<object, object>/*!*/ obj, object key, object value) {
            MutableString str = key as MutableString;
            if (str != null) {
                key = str.Duplicate(context, false, str.Clone()).Freeze();
            } else {
                key = BaseSymbolDictionary.NullToObj(key);
            }
            return obj[key] = value;
        }

        public static Hash/*!*/ SetHashElements(RubyContext/*!*/ context, Hash/*!*/ hash, object[]/*!*/ items) {
            Assert.NotNull(context, hash, items);
            Debug.Assert(items != null && items.Length % 2 == 0);

            for (int i = 0; i < items.Length; i += 2) {
                Debug.Assert(i + 1 < items.Length);
                SetHashElement(context, hash, items[i], items[i + 1]);
            }

            return hash;
        }

        #endregion

        #region Evals

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private static int _stringEvalCounter;

        public static RubyCompilerOptions/*!*/ CreateCompilerOptionsForEval(RubyScope/*!*/ targetScope, int line) {
            return CreateCompilerOptionsForEval(targetScope, targetScope.GetInnerMostMethodScope(), false, line);
        }

        private static RubyCompilerOptions/*!*/ CreateCompilerOptionsForEval(RubyScope/*!*/ targetScope, RubyMethodScope methodScope,
            bool isModuleEval, int line) {

            return new RubyCompilerOptions(targetScope.RubyContext.RubyOptions) {
                IsEval = true,
                FactoryKind = isModuleEval ? TopScopeFactoryKind.Module : TopScopeFactoryKind.None,
                LocalNames = targetScope.GetVisibleLocalNames(),
                TopLevelMethodName = (methodScope != null) ? methodScope.Method.DefinitionName : null,
                InitialLocation = new SourceLocation(0, line <= 0 ? 1 : line, 1),
            };
        }

        public static object Evaluate(MutableString/*!*/ code, RubyScope/*!*/ targetScope, object self, RubyModule module, MutableString file, int line) {
            Assert.NotNull(code, targetScope);

            RubyContext context = targetScope.RubyContext;
            RubyMethodScope methodScope = targetScope.GetInnerMostMethodScope();

            Utils.Log(Interlocked.Increment(ref _stringEvalCounter).ToString(), "EVAL");

            // we want to create a new top-level local scope:
            var options = CreateCompilerOptionsForEval(targetScope, methodScope, module != null, line);

            SourceUnit source = context.CreateSnippet(code.ConvertToString(), file != null ? file.ConvertToString() : "(eval)", SourceCodeKind.Statements);
            Expression<EvalEntryPointDelegate> lambda;
            try {
                lambda = context.ParseSourceCode<EvalEntryPointDelegate>(source, options, context.RuntimeErrorSink);
            } catch (SyntaxError e) {
                Utils.Log(e.Message, "EVAL_ERROR");
                Utils.Log(new String('-', 50), "EVAL_ERROR");
                Utils.Log(source.GetCode(), "EVAL_ERROR");
                Utils.Log(new String('-', 50), "EVAL_ERROR");
                throw;
            }
            Debug.Assert(lambda != null);

            Proc blockParameter;
            RubyMethodInfo methodDefinition;
            if (methodScope != null) {
                blockParameter = methodScope.BlockParameter;
                methodDefinition = methodScope.Method;
            } else {
                blockParameter = null;
                methodDefinition = null;
            }

            if (context.Options.InterpretedMode) {
                return Interpreter.TopLevelExecute(new InterpretedScriptCode(lambda, source),
                    targetScope,
                    self,
                    module,
                    blockParameter,
                    methodDefinition,
                    targetScope.RuntimeFlowControl
                );
            } else {
                return lambda.Compile(source.EmitDebugSymbols)(
                    targetScope,
                    self,
                    module,
                    blockParameter,
                    methodDefinition,
                    targetScope.RuntimeFlowControl
                );
            }
        }

        public static object EvaluateInModule(RubyModule/*!*/ self, BlockParam/*!*/ block) {
            Assert.NotNull(self, block);

            object returnValue = EvaluateInModuleNoJumpCheck(self, block);
            block.BlockJumped(returnValue);
            return returnValue;
        }

        public static object EvaluateInModule(RubyModule/*!*/ self, BlockParam/*!*/ block, object defaultReturnValue) {
            Assert.NotNull(self, block);

            object returnValue = EvaluateInModuleNoJumpCheck(self, block);

            if (block.BlockJumped(returnValue)) {
                return returnValue;
            }

            return defaultReturnValue;
        }

        private static object EvaluateInModuleNoJumpCheck(RubyModule/*!*/ self, BlockParam/*!*/ block) {
            block.ModuleDeclaration = self;
            return RubyOps.Yield1(self, self, block);
        }

        public static object EvaluateInSingleton(object self, BlockParam/*!*/ block) {
            // TODO: this is checked in method definition, if no method is defined it is ok.
            // => singleton is create in method definition also.
            if (!RubyUtils.CanCreateSingleton(self)) {
                throw RubyExceptions.CreateTypeError("can't define singleton method for literals");
            }

            block.ModuleDeclaration = block.RubyContext.CreateSingletonClass(self);

            // TODO: flows Public visibility in the block
            // Flow "Singleton" method attribute? If we change method attribute
            object returnValue = RubyOps.Yield1(self, self, block);
            block.BlockJumped(returnValue);
            return returnValue;
        }

        #endregion

        #region Object Construction

        private static readonly Type[] _ccTypes1 = new Type[] { typeof(RubyClass) };
        private static readonly Type[] _ccTypes2 = new Type[] { typeof(RubyContext) };
#if !SILVERLIGHT
        private static readonly Type[] _serializableTypeSignature = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
#endif

        public static object/*!*/ CreateObject(RubyClass/*!*/ theclass, Hash/*!*/ attributes, bool decorate) {
            Assert.NotNull(theclass, attributes);

            Type baseType = theclass.GetUnderlyingSystemType();
            object obj;
            if (typeof(ISerializable).IsAssignableFrom(baseType)) {
#if !SILVERLIGHT
                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                ConstructorInfo ci = baseType.GetConstructor(bindingFlags, null, _serializableTypeSignature, null);
                if (ci == null) {
#endif
                    string message = String.Format("Class {0} does not have a valid deserializing constructor", baseType.FullName);
                    throw new NotSupportedException(message);
#if !SILVERLIGHT
                }
                SerializationInfo info = new SerializationInfo(baseType, new FormatterConverter());
                info.AddValue("#class", theclass);
                foreach (KeyValuePair<object, object> pair in attributes) {
                    string key = pair.Key.ToString();
                    key = decorate ? "@" + key : key;
                    info.AddValue(key, pair.Value);
                }
                obj = ci.Invoke(new object[2] { info, new StreamingContext(StreamingContextStates.Other, theclass) });
#endif
            } else {
                obj = CreateObject(theclass);
                foreach (KeyValuePair<object, object> pair in attributes) {
                    string key = pair.Key.ToString();
                    key = decorate ? "@" + key : key;
                    theclass.Context.SetInstanceVariable(obj, key, pair.Value);
                }
            }
            return obj;
        }

        private static bool IsAvailable(MethodBase method) {
            return method != null && !method.IsPrivate && !method.IsFamilyAndAssembly;
        }

        public static object/*!*/ CreateObject(RubyClass/*!*/ theClass) {
            Assert.NotNull(theClass);

            Type baseType = theClass.GetUnderlyingSystemType();
            if (baseType == typeof(RubyStruct)) {
                return RubyStruct.Create(theClass);
            }

            object result;
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            ConstructorInfo ci;
            if (IsAvailable(ci = baseType.GetConstructor(bindingFlags, null, Type.EmptyTypes, null))) {
                result = ci.Invoke(new object[0] { });
            } else if (IsAvailable(ci = baseType.GetConstructor(bindingFlags, null, _ccTypes1, null))) {
                result = ci.Invoke(new object[1] { theClass });
            } else if (IsAvailable(ci = baseType.GetConstructor(bindingFlags, null, _ccTypes2, null))) {
                result = ci.Invoke(new object[1] { theClass.Context });
            } else {
                string message = String.Format("Class {0} does not have a valid constructor", theClass.Name);
                throw new NotSupportedException(message);
            }
            return result;
        }

        #endregion

        #region Call Site Storage Extensions

        public static CallSite<TCallSiteFunc>/*!*/ GetCallSite<TCallSiteFunc>(ref CallSite<TCallSiteFunc>/*!*/ site,
            string/*!*/ methodName, int argumentCount) where TCallSiteFunc : class {

            if (site == null) {
                Interlocked.CompareExchange(ref site,
                    CallSite<TCallSiteFunc>.Create(RubyCallAction.Make(methodName, RubyCallSignature.WithImplicitSelf(argumentCount))), null);
            }
            return site;
        }

        public static CallSite<TCallSiteFunc>/*!*/ GetCallSite<TCallSiteFunc>(ref CallSite<TCallSiteFunc>/*!*/ site,
            string/*!*/ methodName, RubyCallSignature signature) where TCallSiteFunc : class {

            if (site == null) {
                Interlocked.CompareExchange(ref site,
                    CallSite<TCallSiteFunc>.Create(RubyCallAction.Make(methodName, signature)), null);
            }
            return site;
        }

        public static CallSite<Func<CallSite, RubyContext, object, TResult>>/*!*/ GetCallSite<TResult>(
            ref CallSite<Func<CallSite, RubyContext, object, TResult>>/*!*/ site, RubyConversionAction/*!*/ conversion) {

            if (site == null) {
                Interlocked.CompareExchange(ref site, CallSite<Func<CallSite, RubyContext, object, TResult>>.Create(conversion), null);
            }
            return site;
        }

        public static bool MethodNotFound(object siteResult) {
            return siteResult == RubyOps.MethodNotFound;
        }

        #endregion

        #region Exceptions

#if SILVERLIGHT // Thread.ExceptionState, Thread.Abort(stateInfo)
        public static Exception GetVisibleException(Exception e) { return e; }

        public static void ExitThread(Thread/*!*/ thread) {
            thread.Abort();
        }

        public static bool IsRubyThreadExit(Exception e) {
            return e is ThreadAbortException;
        }
#else
        /// <summary>
        /// Thread#raise is implemented on top of System.Threading.Thread.ThreadAbort, and squirreling
        /// the Ruby exception expected by the use in ThreadAbortException.ExceptionState.
        /// </summary>
        private class AsyncExceptionMarker {
            internal Exception Exception { get; set; }
            internal AsyncExceptionMarker(Exception e) {
                this.Exception = e;
            }
        }

        public static void RaiseAsyncException(Thread thread, Exception e) {
            thread.Abort(new AsyncExceptionMarker(e));
        }

        // TODO: This is redundant with ThreadOps.RubyThreadInfo.ExitRequested. However, we cannot access that
        // from here as it is in a separate assembly.
        private class ThreadExitMarker {
        }

        public static void ExitThread(Thread/*!*/ thread) {
            thread.Abort(new ThreadExitMarker());
        }

        /// <summary>
        /// Thread#exit is implemented by calling Thread.Abort. However, we need to distinguish a call to Thread#exit
        /// from a raw call to Thread.Abort.
        /// 
        /// Note that if a finally block raises an exception while an Abort is pending, that exception can be propagated instead of a ThreadAbortException.
        /// </summary>
        public static bool IsRubyThreadExit(Exception e) {
            ThreadAbortException tae = e as ThreadAbortException;
            if (tae != null) {
                if (tae.ExceptionState is ThreadExitMarker) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Can return null for Thread#kill
        /// </summary>
        public static Exception GetVisibleException(Exception e) {
            ThreadAbortException tae = e as ThreadAbortException;
            if (tae != null) {
                if (IsRubyThreadExit(e)) {
                    return null;
                }
                AsyncExceptionMarker asyncExceptionMarker = tae.ExceptionState as AsyncExceptionMarker;
                if (asyncExceptionMarker != null) {
                    return asyncExceptionMarker.Exception;
                }
            }
            return e;
        }

#endif

        #endregion
    }
}
