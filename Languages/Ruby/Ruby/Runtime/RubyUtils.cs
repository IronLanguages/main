/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
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
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime {
    using EvalEntryPointDelegate = Func<RubyScope, object, RubyModule, Proc, object>;
    using Microsoft.Scripting.Actions.Calls;

    public static class RubyUtils {
        #region Objects

        public static readonly int FalseObjectId = 0;
        public static readonly int TrueObjectId = 2;
        public static readonly int NilObjectId = 4;

        /// <summary>
        /// Ruby value types:
        /// 
        /// NilClass
        /// TrueClass
        /// FalseClass
        /// Fixnum
        /// Symbol
        /// + CLR structs
        /// </summary>
        public static bool IsRubyValueType(object obj) {
            return (obj == null || obj is ValueType || obj is RubySymbol) && !(obj is float || obj is double);
        }

        /// <summary>
        /// The object maintains a state (frozen, tainted flags and instance variables).
        /// <code>
        /// obj.instance_variable_set(:@foo, 'bar')
        /// puts obj.instance_variable_get(:@foo)     # => 'bar'
        /// puts obj.taint.tainted?                   # => true
        /// </code>
        /// </summary>
        public static bool HasObjectState(object obj) {
            return !IsRubyValueType(obj);
        }

        /// <summary>
        /// Can the object be cloned?
        /// </summary>
        public static bool CanClone(object obj) {
            return !IsRubyValueType(obj);
        }

        /// <summary>
        /// Is it allowed to define a sigleton method for the object?
        /// </summary>
        public static bool CanDefineSingletonMethod(object obj) {
            return !(obj is ValueType || obj is RubySymbol || obj is BigInteger) || obj is bool;
        }

        /// <summary>
        /// Does the object have a sigleton class?
        /// </summary>
        public static bool HasSingletonClass(object obj) {
            return !(obj is int || obj is RubySymbol);
        }
        
        public static MutableString/*!*/ InspectObject(UnaryOpStorage/*!*/ inspectStorage, ConversionStorage<MutableString>/*!*/ tosConversion, 
            object obj) {

            var context = tosConversion.Context;
            using (IDisposable handle = RubyUtils.InfiniteInspectTracker.TrackObject(obj)) {
                if (handle == null) {
                    return MutableString.CreateAscii("...");
                }

                MutableString str = MutableString.CreateMutable(context.GetIdentifierEncoding());
                str.Append("#<");
                str.Append(context.GetClassDisplayName(obj));

                // Ruby prints 2*object_id for objects
                str.Append(':');
                AppendFormatHexObjectId(str, GetObjectId(context, obj));

                RubyInstanceData data = context.TryGetInstanceData(obj);
                if (data != null) {
                    var vars = data.GetInstanceVariablePairs();
                    bool first = true;
                    foreach (KeyValuePair<string, object> var in vars) {
                        if (first) {
                            str.Append(' ');
                            first = false;
                        } else {
                            str.Append(", ");
                        }
                        // TODO (encoding):
                        str.Append(var.Key);
                        str.Append('=');

                        var inspectSite = inspectStorage.GetCallSite("inspect");
                        object inspectedValue = inspectSite.Target(inspectSite, var.Value);

                        var tosSite = tosConversion.GetSite(ConvertToSAction.Make(context));
                        str.Append(tosSite.Target(tosSite, inspectedValue));

                        str.TaintBy(var.Value, context);
                    }
                }
                str.Append(">");

                str.TaintBy(obj, context);
                return str;
            }
        }

        public static MutableString/*!*/ FormatObjectPrefix(RubyContext/*!*/ context, string/*!*/ className, long objectId, bool isTainted, bool isUntrusted) {
            MutableString str = MutableString.CreateMutable(context.GetIdentifierEncoding());
            str.Append("#<");
            str.Append(className);

            // Ruby prints 2*object_id for objects
            str.Append(':');
            AppendFormatHexObjectId(str, objectId);

            str.IsTainted |= isTainted;
            str.IsUntrusted |= isUntrusted;
            return str;
        }

        public static MutableString/*!*/ FormatObject(RubyContext/*!*/ context, string/*!*/ className, long objectId, bool isTainted, bool isUntrusted) {
            return FormatObjectPrefix(context, className, objectId, isTainted, isUntrusted).Append(">");
        }

        public static MutableString/*!*/ ObjectToMutableString(RubyContext/*!*/ context, object obj) {
            bool tainted, untrusted;
            context.GetObjectTrust(obj, out tainted, out untrusted);
            return FormatObject(context, context.GetClassDisplayName(obj), GetObjectId(context, obj), tainted, untrusted);
        }

        public static MutableString/*!*/ ObjectToMutableStringPrefix(RubyContext/*!*/ context, object obj) {
            bool tainted, untrusted;
            context.GetObjectTrust(obj, out tainted, out untrusted);
            return FormatObjectPrefix(context, context.GetClassDisplayName(obj), GetObjectId(context, obj), tainted, untrusted);
        }

        public static MutableString/*!*/ AppendFormatHexObjectId(MutableString/*!*/ str, long objectId) {
            return str.AppendFormat("0x{0:x7}", 2 * objectId);
        }

        public static MutableString/*!*/ ObjectToMutableString(IRubyObject/*!*/ self) {
            return RubyUtils.FormatObject(
                self.ImmediateClass.Context, 
                self.ImmediateClass.GetNonSingletonClass().Name, 
                self.GetInstanceData().ObjectId, 
                self.IsTainted,
                self.IsUntrusted
            );
        }

        public static MutableString/*!*/ ObjectBaseToMutableString(IRubyObject/*!*/ self) {
            if (self is RubyObject) {
                return RubyUtils.ObjectToMutableString(self);
            } else {
                return MutableString.CreateMutable(self.BaseToString(), RubyEncoding.UTF8);
            }
        }

        public static bool TryDuplicateObject(
            CallSiteStorage<Func<CallSite, object, object, object>>/*!*/ initializeCopyStorage,
            CallSiteStorage<Func<CallSite, RubyClass, object>>/*!*/ allocateStorage, 
            object obj, bool cloneSemantics, out object copy) {

            // Ruby value types can't be cloned
            if (!RubyUtils.CanClone(obj)) {
                copy = null;
                return false;
            }

            var context = allocateStorage.Context;

            IDuplicable clonable = obj as IDuplicable;
            if (clonable != null) {
                copy = clonable.Duplicate(context, cloneSemantics);
            } else {
                // .NET and library classes that don't implement IDuplicable:
                var allocateSite = allocateStorage.GetCallSite("allocate", 0);
                copy = allocateSite.Target(allocateSite, context.GetClassOf(obj));

                context.CopyInstanceData(obj, copy, cloneSemantics);
            }

            var initializeCopySite = initializeCopyStorage.GetCallSite("initialize_copy", 1);
            initializeCopySite.Target(initializeCopySite, copy, obj);
            if (cloneSemantics) {
                context.FreezeObjectBy(copy, obj);
            }

            return true;
        }        

        public static long GetFixnumId(int number) {
            return ((long)number << 1) + 1;
        }

        public static long GetObjectId(RubyContext/*!*/ context, object obj) {
            if (obj == null) return NilObjectId;
            if (obj is bool) return (bool)obj ? TrueObjectId : FalseObjectId;
            if (obj is int) return GetFixnumId((int)obj);

            return context.GetInstanceData(obj).ObjectId;
        }

        private const int CachedCharCount = 256;
        private static object[] _charCache = new object[CachedCharCount];

        public static object/*!*/ CharToObject(char ch) {
            return (ch < CachedCharCount) ? (_charCache[(int)ch] ?? (_charCache[(int)ch] = (object)ch)) : (object)ch;
        }

        

        #endregion

        #region Names

        public static bool HasUnmangledName(string/*!*/ name) {
            return !NotUnmangledObject.Contains(name);
        }
        
        public static string TryUnmangleMethodName(string/*!*/ name) {
            return HasUnmangledName(name) ? TryUnmangleName(name) : null;
        }

        /// <summary>
        /// Converts a Ruby name to PascalCase name (foo_bar -> FooBar).
        /// Returns null if the name is not a well-formed Ruby name (it contains upper-case latter or subsequent underscores).
        /// Characters that are not upper case letters are treated as lower-case letters.
        /// </summary>
        public static string TryUnmangleName(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");
            if (name.Length == 0) {
                return null;
            }

            StringBuilder mangled = new StringBuilder();

            bool lastWasSpecial = false;
            int i = 0, j = 0;
            while (i < name.Length) {
                char c;
                while (j < name.Length && (c = name[j]) != '_') {
                    if (Char.IsUpper(c)) {
                        return null;
                    }
                    j++;
                }

                if (j == i || j == name.Length - 1) {
                    return null;
                }

                if (j - i == 1) {
                    // "ip_f_xxx" -/-> "IPFXxx"
                    if (lastWasSpecial) {
                        return null;
                    }
                    mangled.Append(name[i].ToUpperInvariant());
                    lastWasSpecial = false;
                } else {
                    string special = MapSpecialWord(name, i, j - i);
                    if (special != null) {
                        // "ip_ip" -/-> "IPIP"
                        if (lastWasSpecial) {
                            return null;
                        }
                        mangled.Append(special.ToUpperInvariant());
                        lastWasSpecial = true;
                    } else {
                        mangled.Append(name[i].ToUpperInvariant());
                        mangled.Append(name, i + 1, j - i - 1);
                        lastWasSpecial = false;
                    }
                }

                i = ++j;
            }

            return mangled.ToString();
        }

        public static bool HasMangledName(string/*!*/ name) {
            return !NotMangledObject.Contains(name);
        }

        public static string TryMangleMethodName(string/*!*/ name) {
            return HasMangledName(name) ? TryMangleName(name) : null;
        }

        /// <summary>
        /// Converts a camelCase or PascalCase name to a Ruby name (FooBar -> foo_bar).
        /// Returns null if the name is not in camelCase or PascalCase (FooBAR, foo, etc.).
        /// Characters that are not upper case letters are treated as lower-case letters.
        /// </summary>
        public static string TryMangleName(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");

            StringBuilder mangled = null;
            int i = 0;
            while (i < name.Length) {
                char c = name[i];
                if (Char.IsUpper(c)) {
                    int j = i + 1;
                    while (j < name.Length && Char.IsUpper(name, j)) {
                        j++;
                    }

                    if (j < name.Length) {
                        j--;
                    }

                    if (mangled == null) {
                        mangled = new StringBuilder();
                        mangled.Append(name, 0, i);
                    } 

                    if (i > 0) {
                        mangled.Append('_');
                    }

                    int count = j - i;
                    if (count == 0) {
                        // NaN{end}, NaNXxx
                        if (i + 2 < name.Length && 
                            Char.IsUpper(name[i + 2]) && 
                            (i + 3 == name.Length || Char.IsUpper(name[i + 3]) && 
                            (i + 4 < name.Length && !Char.IsUpper(name[i + 4])))) {
                            return null;
                        } else {
                            // X{end}, In, NaN, Xml, Html, ...
                            mangled.Append(c.ToLowerInvariant());
                            i++;
                        }
                    } else if (count == 1) {
                        // FXx
                        mangled.Append(c.ToLowerInvariant());
                        i++;
                    } else {
                        // FOXxx, FOOXxx, FOOOXxx, ...
                        string special = MapSpecialWord(name, i, count);
                        if (special != null) {
                            mangled.Append(special.ToLowerInvariant());
                            i = j;
                        } else {
                            return null;
                        }
                    }
                } else if (c == '_') {
                    return null;
                } else {
                    if (mangled != null) {
                        mangled.Append(c);
                    }
                    i++;
                }
            }

            return mangled != null ? mangled.ToString() : null;
        }

        private static string MapSpecialWord(string/*!*/ name, int start, int count) {
            if (count == 2) {
                return IsTwoLetterWord(name, start) ? null : name.Substring(start, count);
            }

            return null;
        }

        private static bool IsTwoLetterWord(string/*!*/ str, int index) {
            int c = LetterPair(str, index);
            switch (c) {
                case ('a' << 8) | 't':
                case ('a' << 8) | 's':
                case ('b' << 8) | 'y':
                case ('d' << 8) | 'o':
                case ('i' << 8) | 'd':
                case ('i' << 8) | 't':
                case ('i' << 8) | 'f':
                case ('i' << 8) | 'n':
                case ('i' << 8) | 's':
                case ('g' << 8) | 'o':
                case ('m' << 8) | 'e':
                case ('m' << 8) | 'y':
                case ('n' << 8) | 'o':
                case ('o' << 8) | 'f':
                case ('o' << 8) | 'k':
                case ('o' << 8) | 'n':
                case ('o' << 8) | 'r':
                case ('t' << 8) | 'o':
                case ('u' << 8) | 'p':
                    return true;
            }
            return false;
        }

        private static int LetterPair(string/*!*/ str, int index) {
            return (str[index + 1] & 0xff00) == 0 ? (str[index].ToLowerInvariant() << 8) | str[index + 1].ToLowerInvariant() : -1;
        }

        /// <summary>
        /// A list of Kernel/Object methods that are expected by common Ruby libraries to work on all objects.
        /// We don't unmangled them to allow Ruby programs work with .NET objects that define e.g. Class property. 
        /// </summary>
        internal static readonly HashSet<string> NotUnmangledObject = new HashSet<string>() {
            // Kernel
            "class",
            "clone",
            "display",
            "dup",
            "extend",
            "freeze",
            "hash",
            // "id", Kernel#id is deprecated
            "initialize",
            "inspect",
            "instance_eval",
            "instance_exec",
            "instance_variable_get",
            "instance_variable_set",
            "instance_variables",
            "method",
            "methods",
            "object_id",
            "private_methods",
            "protected_methods",
            "public_methods",
            "send",
            "singleton_methods",
            "taint",
            // "type", Kernel#type is deprecated
            "untaint",
        };

        internal static readonly HashSet<string> NotMangledObject = new HashSet<string>() {
            "Class",
            "Clone",
            "Display",
            "Dup",
            "Extend",
            "Freeze",
            "Hash",
            // "Id", Kernel#id is deprecated
            "Initialize",
            "Inspect",
            "InstanceEval",
            "InstanceExec",
            "InstanceVariableGet",
            "InstanceVariableSet",
            "InstanceVariables",
            "Method",
            "Methods",
            "ObjectId",
            "PrivateMethods",
            "ProtectedMethods",
            "PublicMethods",
            "Send",
            "SingletonMethods",
            "Taint",
            // "Type", Kernel#type is deprecated
            "Untaint",
        };

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

        #region Constants

        // thread-safe:
        public static object GetConstant(RubyGlobalScope/*!*/ globalScope, RubyModule/*!*/ owner, string/*!*/ name, bool lookupObject) {
            Assert.NotNull(globalScope, owner, name);

            using (owner.Context.ClassHierarchyLocker()) {
                ConstantStorage storage;
                if (owner.TryResolveConstantNoLock(globalScope, name, out storage)) {
                    return storage.Value;
                }

                RubyClass objectClass = owner.Context.ObjectClass;
                if (owner != objectClass && lookupObject && objectClass.TryResolveConstantNoLock(globalScope, name, out storage)) {
                    return storage.Value;
                }
            }

            RubyUtils.CheckConstantName(name);
            return owner.ConstantMissing(name);
        }

        public static void SetConstant(RubyModule/*!*/ owner, string/*!*/ name, object value) {
            Assert.NotNull(owner, name);

            if (owner.SetConstantChecked(name, value)) {
                owner.Context.ReportWarning(String.Format("already initialized constant {0}", name));
            }

            // Initializes anonymous module's name, publishes the module:
            RubyModule module = value as RubyModule;
            if (module != null) {
                if (module.Name == null) {
                    module.Name = owner.MakeNestedModuleName(name);
                }
                if (owner.IsObjectClass) {
                    module.Publish(name);
                }
            }
        }

        #endregion

        #region Methods

        public static RubyMethodVisibility GetSpecialMethodVisibility(RubyMethodVisibility/*!*/ visibility, string/*!*/ methodName) {
            return (methodName == Symbols.Initialize || methodName == Symbols.InitializeCopy) ? RubyMethodVisibility.Private : visibility;
        }

        internal static string ToClrOperatorName(string/*!*/ rubyName) {
            switch (rubyName) {
                case "+": return "op_Addition";
                case "-": return "op_Subtraction";
                case "/": return "op_Division";
                case "*": return "op_Multiply";
                case "%": return "op_Modulus";
                case "==": return "op_Equality";
                case "!=": return "op_Inequality";
                case ">": return "op_GreaterThan";
                case ">=": return "op_GreaterThanOrEqual";
                case "<": return "op_LessThan";
                case "<=": return "op_LessThanOrEqual";
                case "-@": return "op_UnaryNegation";
                case "+@": return "op_UnaryPlus";
                case "<<": return "op_LeftShift";
                case ">>": return "op_RightShift";
                case "^": return "op_ExclusiveOr";
                case "~": return "op_OnesComplement";
                case "&": return "op_BitwiseAnd";
                case "|": return "op_BitwiseOr";
                
                case "**": return "Power";
                case "<=>": return "Compare";

                default:
                    return null;
            }
        }

        internal static string ToRubyOperatorName(string/*!*/ clrName) {
            switch (clrName) {
                case "op_Addition": return "+";
                case "op_Subtraction": return "-";
                case "op_Division": return "/";
                case "op_Multiply": return "*";
                case "op_Modulus": return "%";
                case "op_Equality": return "==";
                case "op_Inequality": return "!=";
                case "op_GreaterThan": return ">";
                case "op_GreaterThanOrEqual": return ">=";
                case "op_LessThan": return "<";
                case "op_LessThanOrEqual": return "<=";
                case "op_UnaryNegation": return "-@";
                case "op_UnaryPlus": return "+@";
                case "op_LeftShift": return "<<";
                case "op_RightShift": return ">>";
                case "op_BitwiseAnd": return "&";
                case "op_BitwiseOr": return "|";
                case "op_ExclusiveOr": return "^";
                case "op_OnesComplement": return "~";

                case "Power": return "**";
                case "Compare": return "<=>";
                
                default:
                    return null;
            }
        }

        internal static string MapOperator(OverloadInfo/*!*/ method) {
            return method.IsStatic && method.IsSpecialName ? ToRubyOperatorName(method.Name) : null;
        }

        internal static string MapOperator(MethodInfo/*!*/ method) {
            return method.IsStatic && method.IsSpecialName ? ToRubyOperatorName(method.Name) : null;
        }

        internal static bool IsOperator(OverloadInfo/*!*/ method) {
            return MapOperator(method) != null;
        }

        internal static string MapOperator(ExpressionType op) {
            string methodName;
            TryMapOperator(op, out methodName);
            return methodName;
        }

        internal static int TryMapOperator(ExpressionType op, out string methodName) {
            switch (op) {
                case ExpressionType.Add: methodName = "+"; return 2;
                case ExpressionType.Subtract: methodName = "-"; return 2;
                case ExpressionType.Divide: methodName = "/"; return 2;
                case ExpressionType.Multiply: methodName = "*"; return 2;
                case ExpressionType.Modulo: methodName = "%"; return 2;
                case ExpressionType.Equal: methodName = "=="; return 2;
                case ExpressionType.NotEqual: methodName = "!="; return 2;
                case ExpressionType.GreaterThan: methodName = ">"; return 2;
                case ExpressionType.GreaterThanOrEqual: methodName = ">="; return 2;
                case ExpressionType.LessThan: methodName = "<"; return 2;
                case ExpressionType.LessThanOrEqual: methodName = "<="; return 2;
                case ExpressionType.LeftShift: methodName = "<<"; return 2;
                case ExpressionType.RightShift: methodName = ">>"; return 2;
                case ExpressionType.And: methodName = "&"; return 2;
                case ExpressionType.Or: methodName = "|"; return 2;
                case ExpressionType.ExclusiveOr: methodName = "^"; return 2;
                case ExpressionType.Power: methodName = "**"; return 2;

                case ExpressionType.Negate: methodName = "-@"; return 1;
                case ExpressionType.UnaryPlus: methodName = "+@"; return 1;
                case ExpressionType.OnesComplement: methodName = "~"; return 1;
                case ExpressionType.Not: methodName = "!"; return 1;
            }

            methodName = null;
            return 0;
        }

        internal static int TryMapOperator(string/*!*/ methodName, out ExpressionType op) {
            switch (methodName) {
                case "+": op = ExpressionType.Add; return 2;
                case "-": op = ExpressionType.Subtract; return 2;
                case "/": op = ExpressionType.Divide; return 2;
                case "*": op = ExpressionType.Multiply; return 2;
                case "%": op = ExpressionType.Modulo; return 2;
                case "==": op = ExpressionType.Equal; return 2;
                case "!=": op = ExpressionType.NotEqual; return 2;
                case ">": op = ExpressionType.GreaterThan; return 2;
                case ">=": op = ExpressionType.GreaterThanOrEqual; return 2;
                case "<": op = ExpressionType.LessThan; return 2;
                case "<=": op = ExpressionType.LessThanOrEqual; return 2;
                case "<<": op = ExpressionType.LeftShift; return 2;
                case ">>": op = ExpressionType.RightShift; return 2;
                case "&": op = ExpressionType.And; return 2;
                case "|": op = ExpressionType.Or; return 2;
                case "^": op = ExpressionType.ExclusiveOr; return 2;
                case "**": op = ExpressionType.Power; return 2;

                case "-@": op = ExpressionType.Negate; return 1;
                case "+@": op = ExpressionType.UnaryPlus; return 1;
                case "~": op = ExpressionType.OnesComplement; return 1;
                case "!": op = ExpressionType.Not; return 1;
            }

            op = default(ExpressionType);
            return 0;
        }

        #endregion

        #region Modules, Classes
        
        internal static RubyModule/*!*/ GetModuleFromObject(RubyScope/*!*/ scope, object obj) {
            RubyModule module = obj as RubyModule;
            if (module == null) {
                throw CreateNotModuleException(scope, obj);
            }
            return module;
        }

        internal static Exception/*!*/ CreateNotModuleException(RubyScope/*!*/ scope, object obj) {
            return RubyExceptions.CreateTypeError(String.Format("{0} is not a class/module", scope.RubyContext.GetClassOf(obj)));
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

        public static readonly MutableString InfiniteRecursionMarker = MutableString.CreateAscii("[...]").Freeze();

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
                obj = CustomStringDictionary.NullToObj(obj);
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
                key = CustomStringDictionary.NullToObj(key);
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

        //                         scope                   parent scope            self
        // M.module_eval {}        block                   current                 M 
        // x.instance_eval {}      block                   current                 x
        // M.module_eval ""        ModEval(module: M)      current                 M
        // x.instance_eval ""      ModEval(module: S(x))   current                 x 
        // eval "", binding        binding.scope           binding.scope.parent    binding.scope.self
        //

#if DEBUG
        private static int _stringEvalCounter;
#endif

        public static RubyCompilerOptions/*!*/ CreateCompilerOptionsForEval(RubyScope/*!*/ targetScope, int line) {
            return CreateCompilerOptionsForEval(targetScope, targetScope.GetInnerMostMethodScope(), false, line);
        }

        private static RubyCompilerOptions/*!*/ CreateCompilerOptionsForEval(RubyScope/*!*/ targetScope, RubyMethodScope methodScope,
            bool isModuleEval, int line) {

            return new RubyCompilerOptions(targetScope.RubyContext.RubyOptions) {
                IsEval = true,
                FactoryKind = isModuleEval ? TopScopeFactoryKind.ModuleEval : TopScopeFactoryKind.None,
                LocalNames = targetScope.GetVisibleLocalNames(),
                TopLevelMethodName = (methodScope != null) ? methodScope.DefinitionName : null,
                TopLevelParameterNames = (methodScope != null) ? methodScope.GetVisibleParameterNames() : null,
                TopLevelHasUnsplatParameter = (methodScope != null) ? methodScope.HasUnsplatParameter : false,
                InitialLocation = new SourceLocation(0, line <= 0 ? 1 : line, 1),
            };
        }

        private static SourceUnit/*!*/ CreateRubySourceUnit(RubyContext/*!*/ context, MutableString/*!*/ code, string path) {
            return context.CreateSourceUnit(new BinaryContentProvider(code.ToByteArray()), path, code.Encoding.Encoding, SourceCodeKind.File);
        }

        public static object Evaluate(MutableString/*!*/ code, RubyScope/*!*/ targetScope, object self, RubyModule module, MutableString file, int line) {
            Assert.NotNull(code, targetScope);

            RubyContext context = targetScope.RubyContext;
            RubyMethodScope methodScope = targetScope.GetInnerMostMethodScope();

#if DEBUG
            Utils.Log(Interlocked.Increment(ref _stringEvalCounter).ToString() + ": " + (file != null ? file.ToString() : "?") + " : " + line, "EVAL");
#endif

            // we want to create a new top-level local scope:
            var options = CreateCompilerOptionsForEval(targetScope, methodScope, module != null, line);
            var source = CreateRubySourceUnit(context, code, file != null ? file.ConvertToString() : "(eval)");

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

            // module-eval:
            if (module != null) {
                targetScope = CreateModuleEvalScope(targetScope, self, module);
            }

            return ((EvalEntryPointDelegate)RubyScriptCode.CompileLambda(lambda, context))(
                targetScope,
                self,
                module,
                (methodScope != null) ? methodScope.BlockParameter : null
            );
        }

        private static RubyScope/*!*/ CreateModuleEvalScope(RubyScope/*!*/ parent, object self, RubyModule/*!*/ module) {
            var scope = new RubyModuleEvalScope(parent, module, self);
            scope.SetDebugName("instance/module-eval");
            return scope;
        }

        public static object EvaluateInModule(RubyModule/*!*/ self, BlockParam/*!*/ block, object[] args) {
            object result;
            EvaluateBlock(block, self, self, args, out result);
            return result;
        }

        public static object EvaluateInModule(RubyModule/*!*/ self, BlockParam/*!*/ block, object[] args, object defaultReturnValue) {
            object result;
            return EvaluateBlock(block, self, self, args, out result) ? result : defaultReturnValue;
        }

        public static object EvaluateInSingleton(object self, BlockParam/*!*/ block, object[] args) {
            // TODO: this is checked in method definition, if no method is defined it is ok.
            // => singleton is created in method definition also.
            if (!RubyUtils.CanDefineSingletonMethod(self)) {
                throw RubyExceptions.CreateTypeError("can't define singleton method for literals");
            }

            object result;
            EvaluateBlock(block, block.RubyContext.GetOrCreateSingletonClass(self), self, args, out result);
            return result;
        }

        private static bool EvaluateBlock(BlockParam/*!*/ block, RubyModule/*!*/ module, object self, object[] args, out object result) {
            Assert.NotNull(block, module);
            block.MethodLookupModule = module;

            if (args != null) {
                result = RubyOps.Yield(args, null, self, block);
            } else {
                result = RubyOps.Yield0(null, self, block);
            }

            return block.BlockJumped(result);
        }

        #endregion

        #region Object Construction

        private static readonly Type[] _ccTypes1 = new Type[] { typeof(RubyClass) };
        private static readonly Type[] _ccTypes2 = new Type[] { typeof(RubyContext) };
#if !SILVERLIGHT // serialization
        private static readonly Type[] _serializableTypeSignature = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
#endif

        public static readonly string SerializationInfoClassKey = "#immediateClass";

        public static object/*!*/ CreateObject(RubyClass/*!*/ theclass, IEnumerable<KeyValuePair<string, object>>/*!*/ attributes) {
            Assert.NotNull(theclass, attributes);

            Type baseType = theclass.GetUnderlyingSystemType();
            object obj;
            if (typeof(ISerializable).IsAssignableFrom(baseType)) {
#if !SILVERLIGHT // serialization
                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                ConstructorInfo ci = baseType.GetConstructor(bindingFlags, null, _serializableTypeSignature, null);
                if (ci == null) {
#endif
                string message = String.Format("Class {0} does not have a valid deserializing constructor", baseType.FullName);
                    throw new NotSupportedException(message);
#if !SILVERLIGHT // serialization
                }
                SerializationInfo info = new SerializationInfo(baseType, new FormatterConverter());
                info.AddValue(SerializationInfoClassKey, theclass);
                foreach (var pair in attributes) {
                    info.AddValue(pair.Key, pair.Value);
                }
                obj = ci.Invoke(new object[2] { info, new StreamingContext(StreamingContextStates.Other, theclass) });
#endif
            } else {
                obj = CreateObject(theclass);
                foreach (var pair in attributes) {
                    theclass.Context.SetInstanceVariable(obj, pair.Key, pair.Value);
                }
            }
            return obj;
        }

        private static bool IsAvailable(MethodBase method) {
            return method != null && !method.IsPrivate && !method.IsAssembly && !method.IsFamilyAndAssembly;
        }

        // TODO: remove
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

        public static CallSite<TCallSiteFunc>/*!*/ GetCallSite<TCallSiteFunc>(ref CallSite<TCallSiteFunc>/*!*/ site, RubyContext/*!*/ context,
            string/*!*/ methodName, int argumentCount) where TCallSiteFunc : class {

            if (site == null) {
                Interlocked.CompareExchange(ref site,
                    CallSite<TCallSiteFunc>.Create(RubyCallAction.Make(context, methodName, RubyCallSignature.WithImplicitSelf(argumentCount))), null);
            }
            return site;
        }

        public static CallSite<TCallSiteFunc>/*!*/ GetCallSite<TCallSiteFunc>(ref CallSite<TCallSiteFunc>/*!*/ site, RubyContext/*!*/ context,
            string/*!*/ methodName, RubyCallSignature signature) where TCallSiteFunc : class {

            if (site == null) {
                Interlocked.CompareExchange(ref site,
                    CallSite<TCallSiteFunc>.Create(RubyCallAction.Make(context, methodName, signature)), null);
            }
            return site;
        }

        public static CallSite<Func<CallSite, object, TResult>>/*!*/ GetCallSite<TResult>(
            ref CallSite<Func<CallSite, object, TResult>>/*!*/ site, RubyConversionAction/*!*/ conversion) {

            if (site == null) {
                Interlocked.CompareExchange(ref site, CallSite<Func<CallSite, object, TResult>>.Create(conversion), null);
            }
            return site;
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

        #region Paths

        public static MutableString CanonicalizePath(MutableString path) {
            for (int i = 0; i < path.Length; i++) {
                if (path.GetChar(i) == '\\')
                    path.SetChar(i, '/');
            }
            return path;
        }

        public static String CanonicalizePath(string path) {
            return path.Replace('\\', '/');
        }

        public static String CombinePaths(string basePath, string path) {
            return basePath.Length == 0 || basePath.EndsWith("\\", StringComparison.Ordinal) || basePath.EndsWith("/", StringComparison.Ordinal) ? 
                basePath + path :
                basePath + "/" + path;
        }

        // TODO: virtualize via PAL
        public static bool FileSystemUsesDriveLetters { get { return Path.DirectorySeparatorChar == '\\'; } }

        // Is path something like "/foo/bar" (or "c:/foo/bar" on Windows)
        // We need this instead of Path.IsPathRooted since we need to be able to deal with Unix-style path names even on Windows
        public static bool IsAbsolutePath(string path) {
            if (IsAbsoluteDriveLetterPath(path)) {
                return true;
            }

            if (String.IsNullOrEmpty(path)) {
                return false;
            }

            return path[0] == '/';
        }

        // Is path something like "c:/foo/bar" (on Windows)
        public static bool IsAbsoluteDriveLetterPath(string path) {
            if (String.IsNullOrEmpty(path)) {
                return false;
            }

            if (!FileSystemUsesDriveLetters) {
                return false;
            }

            return Tokenizer.IsLetter(path[0]) && path.Length >= 2 && path[1] == ':' && path[2] == '/';
        }

        // returns "//", "///", ... or something like "c:/"
        public static string GetPathRoot(PlatformAdaptationLayer/*!*/ platform, string path, out string pathAfterRoot) {
            Debug.Assert(IsAbsolutePath(path));
            if (IsAbsoluteDriveLetterPath(path)) {
                pathAfterRoot = path.Substring(3);
                return path.Substring(0, 3);
            } else {
                Debug.Assert(path[0] == '/');

                // The root for "////foo" is "/////"
                string withoutInitialSlashes = path.TrimStart('/');
                int initialSlashesCount = path.Length - withoutInitialSlashes.Length;
                string initialSlashes = path.Substring(0, initialSlashesCount);
                pathAfterRoot = path.Substring(initialSlashesCount);

                if (!FileSystemUsesDriveLetters || initialSlashesCount > 1) {
                    return initialSlashes;
                } else {
                    string currentDirectory = RubyUtils.CanonicalizePath(platform.CurrentDirectory);
                    Debug.Assert(IsAbsoluteDriveLetterPath(currentDirectory));
                    string temp;
                    return GetPathRoot(platform, currentDirectory, out temp);
                }
            }
        }

        // Is path something like "c:foo" (note that this is not "c:/foo")
        public static bool HasPartialDriveLetter(string path, out char partialDriveLetter, out string relativePath) {
            partialDriveLetter = '\0';
            relativePath = null;

            if (String.IsNullOrEmpty(path)) {
                return false;
            }

            if (!FileSystemUsesDriveLetters) {
                return false;
            }

            if (Tokenizer.IsLetter(path[0]) && path.Length >= 2 && path[1] == ':' && (path.Length == 2 || path[2] != '/')) {
                partialDriveLetter = path[0];
                relativePath = path.Substring(2);
                return true;
            } else {
                return false;
            }
        }

        #region expand_path

        // Algorithm to find HOME equivalents under Windows. This is equivalent to Ruby 1.9 behavior:
        // 
        // 1. Try get HOME
        // 2. Try to generate HOME equivalent using HOMEDRIVE + HOMEPATH
        // 3. Try to generate HOME equivalent from USERPROFILE
        // 4. Try to generate HOME equivalent from Personal special folder 

        public static string/*!*/ GetHomeDirectory(PlatformAdaptationLayer/*!*/ pal) {
            string result = pal.GetEnvironmentVariable("HOME");
            if (result == null) {
                string homeDrive = pal.GetEnvironmentVariable("HOMEDRIVE");
                string homePath = pal.GetEnvironmentVariable("HOMEPATH");
                if (homeDrive == null && homePath == null) {
                    string userEnvironment = pal.GetEnvironmentVariable("USERPROFILE");
                    if (userEnvironment == null) {
                        // This will always succeed with a non-null string, but it can fail
                        // if the Personal folder was renamed or deleted. In this case it returns
                        // an empty string.
                        result = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    } else {
                        result = userEnvironment;
                    }
                } else if (homeDrive == null) {
                    result = homePath;
                } else if (homePath == null) {
                    result = homeDrive + Path.DirectorySeparatorChar;
                } else {
                    result = homeDrive + homePath;
                }

                if (result != null) {
                    result = ExpandPath(pal, result);
                }
            }

            return result;
        }

        class PathExpander {
            List<string> _pathComponents = new List<string>(); // does not include the root
            string _root; // Typically "c:/" on Windows, and "/" on Unix

            internal PathExpander(PlatformAdaptationLayer/*!*/ platform, string absoluteBasePath) {
                Debug.Assert(RubyUtils.IsAbsolutePath(absoluteBasePath));

                string basePathAfterRoot = null;
                _root = RubyUtils.GetPathRoot(platform, absoluteBasePath, out basePathAfterRoot);

                // Normally, basePathAfterRoot[0] will not be '/', but here we deal with cases like "c:////foo"
                basePathAfterRoot = basePathAfterRoot.TrimStart('/');

                AddRelativePath(basePathAfterRoot);
            }

            internal void AddRelativePath(string relPath) {
                Debug.Assert(!RubyUtils.IsAbsolutePath(relPath));

                string[] relPathComponents = relPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string pathComponent in relPathComponents) {
                    if (pathComponent == "..") {
                        if (_pathComponents.Count == 0) {
                            // MRI allows more pops than the base path components
                            continue;
                        }
                        _pathComponents.RemoveAt(_pathComponents.Count - 1);
                    } else if (pathComponent == ".") {
                        continue;
                    } else {
                        _pathComponents.Add(pathComponent);
                    }
                }
            }

            internal string/*!*/ GetResult() {
                StringBuilder result = new StringBuilder(_root);

                if (_pathComponents.Count >= 1) {
                    // Here we make this work:
                    //   File.expand_path("c:/..a..") -> "c:/..a"
                    string lastComponent = _pathComponents[_pathComponents.Count - 1];
                    if (RubyUtils.FileSystemUsesDriveLetters && !String.IsNullOrEmpty(lastComponent.TrimEnd('.'))) {
                        _pathComponents[_pathComponents.Count - 1] = lastComponent.TrimEnd('.');
                    }
                }

                for (int i = 0; i < _pathComponents.Count; i++) {
                    result.Append(_pathComponents[i]);
                    if (i < (_pathComponents.Count - 1)) {
                        result.Append('/');
                    }
                }
#if DEBUG
                _pathComponents = null;
                _root = null;
#endif
                return result.ToString();
            }
        }

        // Expand directory path - these cases exist:
        //
        // 1. Empty string or nil means return current directory
        // 2. ~ with non-existent HOME directory throws exception
        // 3. ~, ~/ or ~\ which expands to HOME
        // 4. ~foo is left unexpanded
        // 5. Expand to full path if path is a relative path
        // 
        // No attempt is made to determine whether the path is valid or not
        // Returned path is always canonicalized to forward slashes

        public static string/*!*/ ExpandPath(PlatformAdaptationLayer/*!*/ platform, string/*!*/ path) {
            return ExpandPath(platform, path, platform.CurrentDirectory, true);
        }

        public static string/*!*/ ExpandPath(PlatformAdaptationLayer/*!*/ platform, string/*!*/ path, string/*!*/ basePath, bool expandHome) {
            return ExpandPath(platform, path, basePath, expandHome, true);
        }

        private static string/*!*/ ExpandPath(PlatformAdaptationLayer/*!*/ platform, string/*!*/ path, string/*!*/ basePath, bool expandHome, bool expandBase) {
            Assert.NotNull(platform, path, basePath);

            if (expandHome) {
                int length = path.Length;
                if (length > 0 && path[0] == '~') {
                    if (length == 1 || (path[1] == '/' || path[1] == '\\')) {
                        string homeDirectory = platform.GetEnvironmentVariable("HOME");
                        if (homeDirectory == null) {
                            throw RubyExceptions.CreateArgumentError("couldn't find HOME environment -- expanding `~'");
                        }

                        if (length <= 2) {
                            path = homeDirectory;
                        } else {
                            path = Path.Combine(homeDirectory, path.Substring(2));
                        }
                    } else {
                        // MRI doesn't expand path that starts with ~, seems like a bug
                        // http://redmine.ruby-lang.org/issues/show/3629
                        return path;
                    }
                }
                // MRI deosn't expand content of HOME variable (could be a relative path). We do.
                // See http://redmine.ruby-lang.org/issues/show/3630
            }

            path = RubyUtils.CanonicalizePath(path);
            basePath = RubyUtils.CanonicalizePath(basePath);

            if (RubyUtils.IsAbsolutePath(path)) {
                // "basePath" can be ignored if "path" is an absolute path
                return new PathExpander(platform, path).GetResult();
            }

            // expand base path:
            string expandedBasePath = expandBase ? ExpandPath(platform, basePath, platform.CurrentDirectory, expandHome, false) : basePath;

            char drive;
            string relativePath;
            if (RubyUtils.HasPartialDriveLetter(path, out drive, out relativePath)) {
                string _;
                string root = RubyUtils.GetPathRoot(platform, expandedBasePath, out _);
                if (root[0].ToLowerInvariant() != drive.ToLowerInvariant()) {
                    expandedBasePath = RubyUtils.CanonicalizePath(platform.CurrentDirectory);
                    root = RubyUtils.GetPathRoot(platform, expandedBasePath, out _);
                    if (root[0].ToLowerInvariant() != drive.ToLowerInvariant()) {
                        // MRI: Converts "X:path" to "X:/path" if X != base drive or current drive:
                        return new PathExpander(platform, drive.ToString() + ":/" + relativePath).GetResult();
                    }
                }
                path = relativePath;
            }

            PathExpander pathExpander = new PathExpander(platform, expandedBasePath);
            pathExpander.AddRelativePath(path);
            return pathExpander.GetResult();
        }

        /// <summary>
        /// Returns the extension of given path.
        /// Equivalent to <see cref="Path.GetExtension"/> but it doesn't check for an invalid path and 
        /// it considers ".foo" to be a file name with no extension rather than an extension with an empty file name.
        /// </summary>
        /// <returns>Null iff path is null. Empty string if the path doesn't have any extension.</returns>
        public static string GetExtension(string path) {
            if (path == null) {
                return null;
            }

            int length = path.Length;
            int i = length;
            while (--i >= 0) {
                char c = path[i];
                if (c == '.') {
                    if (i != length - 1 && i > 0) {
                        return path.Substring(i, length - i);
                    }
                    return String.Empty;
                }
                if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar) {
                    break;
                }
            }
            return String.Empty;
        }

        #endregion

        #endregion

        #region Streams

        /// <summary>
        /// Writes binary content of <see cref="MutableString"/> into the given buffer.
        /// </summary>
        public static void Write(this Stream/*!*/ stream, MutableString/*!*/ str, int start, int count) {
            int byteCount;
            byte[] bytes = str.GetByteArray(out byteCount);
            if (start < byteCount) {
                stream.Write(bytes, start, Math.Min(byteCount - start, count));
            }
        }

        #endregion
    }
}
