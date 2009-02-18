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
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// Python class hierarchy is represented using the __class__ field in the object. It does not 
    /// use the CLI type system for pure Python types. However, Python types which inherit from a 
    /// CLI type, or from a builtin Python type which is implemented in the engine by a CLI type,
    /// do have to use the CLI type system to interoperate with the CLI world. This means that 
    /// objects of different Python types, but with the same CLI base type, can use the same CLI type - 
    /// they will just have different values for the __class__ field.
    /// 
    /// The easiest way to inspect the functionality implemented by NewTypeMaker is to persist the
    /// generated IL using "ipy.exe -X:SaveAssemblies", and then inspect the
    /// persisted IL using ildasm.
    /// </summary>
    class NewTypeMaker {
        public const string VtableNamesField = "#VTableNames#";
        public const string TypePrefix = "IronPython.NewTypes.";
        public const string BaseMethodPrefix = "#base#";
        public const string FieldGetterPrefix = "#field_get#", FieldSetterPrefix = "#field_set#";

        private static readonly Publisher<NewTypeInfo, Type> _newTypes = new Publisher<NewTypeInfo, Type>();

        [MultiRuntimeAware]
        private static int _typeCount;

        protected Type _baseType;
        protected IList<string> _slots;
        protected TypeBuilder _tg;
        protected FieldInfo _typeField;
        protected FieldInfo _dictField;
        protected FieldInfo _weakrefField;
        protected FieldInfo _slotsField;
        private FieldInfo _explicitMO;
        protected IEnumerable<Type> _interfaceTypes;
        protected PythonTuple _baseClasses;

        private int _site;

        private static readonly Dictionary<Type, Dictionary<string, List<MethodInfo>>> _overriddenMethods = new Dictionary<Type, Dictionary<string, List<MethodInfo>>>();
        private static readonly Dictionary<Type, Dictionary<string, List<ExtensionPropertyTracker>>> _overriddenProperties = new Dictionary<Type, Dictionary<string, List<ExtensionPropertyTracker>>>();
        
        public static Type GetNewType(string typeName, PythonTuple bases, IAttributesCollection dict) {
            if (bases == null) bases = PythonTuple.EMPTY;
            // we're really only interested in the "correct" base type pulled out of bases
            // and any slot information contained in dict
            // other info might be used for future optimizations

            NewTypeInfo typeInfo = GetTypeInfo(typeName, bases, GetSlots(dict));

            if (typeInfo.BaseType.IsValueType)
                throw PythonOps.TypeError("cannot derive from {0} because it is a value type", typeInfo.BaseType.FullName);
            if (typeInfo.BaseType.IsSealed)
                throw PythonOps.TypeError("cannot derive from {0} because it is sealed", typeInfo.BaseType.FullName);

            Type ret = _newTypes.GetOrCreateValue(typeInfo,
                delegate() {
                    if (typeInfo.InterfaceTypes.Count == 0 && typeInfo.Slots == null) {
                        // types that the have DynamicBaseType attribute can be used as NewType's directly, no 
                        // need to create a new type unless we're adding interfaces or slots...
                        object[] attrs = typeInfo.BaseType.GetCustomAttributes(typeof(DynamicBaseTypeAttribute), false);
                        if (attrs.Length > 0) {
                            return typeInfo.BaseType;
                        }
                    }

                    // creation code                    
                    return GetTypeMaker(bases, typeInfo).CreateNewType();
                });
            
            OptimizedScriptCode.InitializeFields(ret, true);

            return ret;
        }

        private static NewTypeMaker GetTypeMaker(PythonTuple bases, NewTypeInfo ti) {
            if (IsInstanceType(ti.BaseType)) {
                return new NewSubtypeMaker(bases, ti);
            }

            return new NewTypeMaker(bases, ti);
        }

        internal static List<string> GetSlots(IAttributesCollection dict) {
            List<string> res = null;
            object slots;
            if (dict != null && dict.TryGetValue(Symbols.Slots, out slots)) {
                res = SlotsToList(slots);
            }

            return res;
        }

        internal static List<string> SlotsToList(object slots) {
            List<string> res = new List<string>();
            ISequence seq = slots as ISequence;
            if (seq != null && !(seq is ExtensibleString)) {
                res = new List<string>(seq.__len__());
                for (int i = 0; i < seq.__len__(); i++) {
                    res.Add(GetSlotName(seq[i]));
                }

                res.Sort();
            } else {
                res = new List<string>(1);
                res.Add(GetSlotName(slots));
            }
            return res;
        }


        private static string GetSlotName(object o) {
            string value;
            if (!Converter.TryConvertToString(o, out value) || String.IsNullOrEmpty(value))
                throw PythonOps.TypeError("slots must be one string or a list of strings");

            for (int i = 0; i < value.Length; i++) {
                if ((value[i] >= 'a' && value[i] <= 'z') ||
                    (value[i] >= 'A' && value[i] <= 'Z') ||
                    (i != 0 && value[i] >= '0' && value[i] <= '9') ||
                    value[i] == '_') {
                    continue;
                }
                throw PythonOps.TypeError("__slots__ must be valid identifiers");
            }

            return value;
        }

        /// <summary>
        /// Is this a type used for instances Python types (and not for the types themselves)?
        /// </summary>
        internal static bool IsInstanceType(Type type) {
            return type.FullName.IndexOf(NewTypeMaker.TypePrefix) == 0;
        }

        /// <summary>
        /// "bases" contains a set of PythonTypes. These can include types defined in Python (say cpy1, cpy2),
        /// CLI types (say cCLI1, cCLI2), and CLI interfaces (say iCLI1, iCLI2). Here are some
        /// examples of how this works:
        /// 
        /// (bases)                      => baseType,        {interfaceTypes}
        /// 
        /// (cpy1)                       => System.Object,   {}
        /// (cpy1, cpy2)                 => System.Object,   {}
        /// (cpy1, cCLI1, iCLI1, iCLI2)  => cCLI1,           {iCLI1, iCLI2}
        /// [some type that satisfies the line above] => 
        ///                                 cCLI1,           {iCLI1, iCLI2}
        /// (cCLI1, cCLI2)               => error
        /// </summary>
        private static NewTypeInfo GetTypeInfo(string typeName, PythonTuple bases, List<string> slots) {
            List<Type> interfaceTypes = new List<Type>();
            Type baseCLIType = typeof(object); // Pure Python object instances inherit from System.Object
            PythonType basePythonType = null;

            foreach (PythonType curBasePythonType in GetPythonTypes(typeName, bases)) {
                // discover the initial base/interfaces
                IList<Type> baseInterfaces = Type.EmptyTypes;
                Type curTypeToExtend = curBasePythonType.ExtensionType;

                if (curBasePythonType.ExtensionType.IsInterface) {
                    baseInterfaces = new Type[] { curTypeToExtend };
                    curTypeToExtend = typeof(object);
                } else if (IsInstanceType(curTypeToExtend)) {
                    PythonTypeSlot dummy;
                    baseInterfaces = new List<Type>();
                    if (!curBasePythonType.TryLookupSlot(DefaultContext.Default, Symbols.Slots, out dummy) &&
                        (slots == null || slots.Count == 0)) {
                        // user did:
                        // class foo(object): __slots__ = 'abc'  (creates object_x)
                        // class bar(foo): pass                  
                        // rather than creating a new object_x_y type we re-use the object_x type.
                        curTypeToExtend = GetBaseTypeFromUserType(curBasePythonType, baseInterfaces, curTypeToExtend.BaseType);
                   }                    
                }

                if (curTypeToExtend == null || typeof(BuiltinFunction).IsAssignableFrom(curTypeToExtend) || typeof(PythonFunction).IsAssignableFrom(curTypeToExtend))
                    throw PythonOps.TypeError(typeName + ": {0} is not an acceptable base type", curBasePythonType.Name);
                if (curTypeToExtend.ContainsGenericParameters)
                    throw PythonOps.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", curBasePythonType);

                foreach (Type interfaceType in baseInterfaces) {
                    if (interfaceType.ContainsGenericParameters)
                        throw PythonOps.TypeError(typeName + ": cannot inhert from open generic instantiation {0}. Only closed instantiations are supported.", interfaceType);

                    // collecting all the interfaces because we override them all.
                    interfaceTypes.Add(interfaceType);
                }

                // if we're not extending something already in our existing base classes type hierarchy
                // then we better be in some esoteric __slots__ situation
                if (!baseCLIType.IsSubclassOf(curTypeToExtend)) {
                    if (baseCLIType != typeof(object) && baseCLIType != curTypeToExtend) {
                        bool isOkConflit = false;
                        if (IsInstanceType(baseCLIType) && IsInstanceType(curTypeToExtend)) {
                            List<string> slots1 = SlotsToList(curBasePythonType.GetBoundMember(DefaultContext.Default, null, Symbols.Slots));
                            List<string> slots2 = SlotsToList(basePythonType.GetBoundMember(DefaultContext.Default, null, Symbols.Slots));
                            if (curBasePythonType.UnderlyingSystemType.BaseType == basePythonType.UnderlyingSystemType.BaseType &&
                                slots1.Count == 1 && slots2.Count == 1 &&
                                ((slots1[0] == "__dict__" && slots2[0] == "__weakref__") ||
                                (slots2[0] == "__dict__" && slots1[0] == "__weakref__"))) {
                                isOkConflit = true;
                                curTypeToExtend = curBasePythonType.UnderlyingSystemType.BaseType;
                                if (slots != null) {
                                    if (slots.Contains("__weakref__"))
                                        throw PythonOps.TypeError("__weakref__ disallowed, base class already defines this");

                                    slots.Add("__weakref__");
                                    if (!slots.Contains("__dict__"))
                                        slots.Add("__dict__");
                                }
                            }
                        }
                        if (!isOkConflit) throw PythonOps.TypeError(typeName + ": can only extend one CLI or builtin type, not both {0} (for {1}) and {2} (for {3})",
                                             baseCLIType.FullName, basePythonType, curTypeToExtend.FullName, curBasePythonType);
                    }

                    // we have a new base type
                    baseCLIType = curTypeToExtend;
                    basePythonType = curBasePythonType;
                }

            }

            return new NewTypeInfo(baseCLIType, interfaceTypes, slots);
        }

        /// <summary>
        /// Filters out old-classes and throws if any non-types are included, returning a
        /// yielding the remaining PythonType objects.
        /// </summary>
        private static IEnumerable<PythonType> GetPythonTypes(string typeName, ICollection<object> bases) {
            foreach (object curBaseType in bases) {
                PythonType curBasePythonType = curBaseType as PythonType;
                if (curBasePythonType == null) {
                    if (curBaseType is OldClass)
                        continue;
                    throw PythonOps.TypeError(typeName + ": unsupported base type for new-style class " + curBaseType);
                }

                yield return curBasePythonType;
            }
        }
        private static Type GetBaseTypeFromUserType(PythonType curBasePythonType, IList<Type> baseInterfaces, Type curTypeToExtend) {
            Queue<PythonType> processing = new Queue<PythonType>();
            processing.Enqueue(curBasePythonType);

            do {
                PythonType walking = processing.Dequeue();
                foreach (PythonType dt in walking.BaseTypes) {
                    if (dt.ExtensionType == curTypeToExtend || curTypeToExtend.IsSubclassOf(dt.ExtensionType)) continue;

                    if (dt.ExtensionType.IsInterface) {
                        baseInterfaces.Add(dt.ExtensionType);
                    } else if (IsInstanceType(dt.ExtensionType)) {
                        processing.Enqueue(dt);
                    } else if (!dt.IsOldClass) {
                        curTypeToExtend = null;
                        break;
                    }
                }
            } while (processing.Count > 0);
            return curTypeToExtend;
        }

        internal NewTypeMaker(PythonTuple baseClasses, NewTypeInfo typeInfo) {
            _baseType = typeInfo.BaseType;
            _baseClasses = baseClasses;
            _interfaceTypes = typeInfo.InterfaceTypes;
            _slots = typeInfo.Slots;
        }

        private static IEnumerable<string> GetBaseName(MethodInfo mi, Dictionary<string, List<string>> specialNames) {
            string newName;
            if (mi.Name.StartsWith(BaseMethodPrefix)) {
                newName = mi.Name.Substring(BaseMethodPrefix.Length);
            } else if (mi.Name.StartsWith(FieldGetterPrefix)) {
                newName = mi.Name.Substring(FieldGetterPrefix.Length);
            } else if (mi.Name.StartsWith(FieldSetterPrefix)) {
                newName = mi.Name.Substring(FieldSetterPrefix.Length);
            } else {
                throw new InvalidOperationException();
            }

            Debug.Assert(specialNames.ContainsKey(newName));

            return specialNames[newName];
        }

        // Build a name which is unique to this TypeInfo.
        protected virtual string GetName() {
            StringBuilder name = new StringBuilder(_baseType.Namespace);
            name.Append('.');
            name.Append(_baseType.Name);
            foreach (Type interfaceType in _interfaceTypes) {
                name.Append("#");
                name.Append(interfaceType.Name);
            }

            name.Append("_");
            name.Append(System.Threading.Interlocked.Increment(ref _typeCount));
            return name.ToString();

        }

        protected virtual void ImplementInterfaces() {
            foreach (Type interfaceType in _interfaceTypes) {
                ImplementInterface(interfaceType);
            }
        }

        protected void ImplementInterface(Type interfaceType) {
            _tg.AddInterfaceImplementation(interfaceType);
        }

        private Type CreateNewType() {
            string name = GetName();
            _tg = Snippets.Shared.DefinePublicType(TypePrefix + name, _baseType);

            ImplementInterfaces();

            GetOrDefineClass();

            GetOrDefineDict();

            ImplementSlots();

            ImplementPythonObject();

            ImplementConstructors();

            Dictionary<string, List<string>> specialNames = new Dictionary<string, List<string>>();

            OverrideMethods(_baseType, specialNames);

            ImplementProtectedFieldAccessors(specialNames);

            Dictionary<Type, bool> doneTypes = new Dictionary<Type, bool>();
            foreach (Type interfaceType in _interfaceTypes) {
                DoInterfaceType(interfaceType, doneTypes, specialNames);
            }

            // Hashtable slots = collectSlots(dict, tg);
            // if (slots != null) tg.createAttrMethods(slots);

            Type ret = FinishType();

            AddBaseMethods(ret, specialNames);

            return ret;
        }

        protected virtual void ImplementPythonObject() {
            ImplementIPythonObject();

            ImplementDynamicObject();

#if !SILVERLIGHT // ICustomTypeDescriptor
            ImplementCustomTypeDescriptor();
#endif
            ImplementPythonEquals();

            ImplementWeakReference();
        }

        private void GetOrDefineDict() {
            if (NeedsDictionary) {
                _dictField = _tg.DefineField("__dict__", typeof(IAttributesCollection), FieldAttributes.Private);
            }
        }

        private void GetOrDefineClass() {
            if (!typeof(IPythonObject).IsAssignableFrom(_baseType)) {
                _typeField = _tg.DefineField("__class__", typeof(PythonType), FieldAttributes.Private);
            }
        }

        protected void EmitGetDict(ILGen gen) {
            if (_dictField != null) {
                gen.EmitFieldGet(_dictField);
            } else {
                gen.EmitPropertyGet(typeof(IPythonObject).GetProperty("Dict"));
            }
        }

        protected void EmitSetDict(ILGen gen) {
            if (_dictField != null) {
                gen.EmitFieldSet(_dictField);
            } else {
                gen.EmitCall(typeof(IPythonObject).GetMethod("ReplaceDict"));
                gen.Emit(OpCodes.Pop); // pop bool result
            }
        }

        protected virtual ParameterInfo[] GetOverrideCtorSignature(ParameterInfo[] original) {
            if (typeof(IPythonObject).IsAssignableFrom(_baseType)) {
                return original;
            }

            ParameterInfo[] argTypes = new ParameterInfo[original.Length + 1];
            if (original.Length == 0 || original[0].ParameterType != typeof(CodeContext)) {
                argTypes[0] = new ParameterInfoWrapper(typeof(PythonType), "cls");
                Array.Copy(original, 0, argTypes, 1, argTypes.Length - 1);
            } else {
                argTypes[0] = original[0];
                argTypes[1] = new ParameterInfoWrapper(typeof(PythonType), "cls");
                Array.Copy(original, 1, argTypes, 2, argTypes.Length - 2);
            }

            return argTypes;
        }

        private void ImplementConstructors() {
            ConstructorInfo[] constructors;
            constructors = _baseType.GetConstructors(BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Instance
                                                    );

            foreach (ConstructorInfo ci in constructors) {
                if (ci.IsPublic || ci.IsFamily || ci.IsFamilyOrAssembly) {
                    OverrideConstructor(ci);
                }
            }
        }

        protected virtual bool ShouldOverrideVirtual(MethodInfo mi) {
            return true;
        }

        private static bool CanOverrideMethod(MethodInfo mi) {
#if !SILVERLIGHT
            return true;
#else
            // can only override the method if it is not SecurityCritical
            return mi.GetCustomAttributes(typeof(System.Security.SecurityCriticalAttribute), false).Length == 0;
#endif
        }


        private void AddBaseMethods(Type finishedType, Dictionary<string, List<string>> specialNames) {
            // "Adds" base methods to super type - this makes super(...).xyz to work - otherwise 
            // we'd return a function that did a virtual call resulting in a stack overflow.
            // The addition is to a seperate cache that NewTypeMaker maintains.  TypeInfo consults this
            // cache when doing member lookup and includes these members in the returned members.
            foreach (MethodInfo mi in finishedType.GetMethods()) {
                if (!ShouldOverrideVirtual(mi)) continue;

                string methodName = mi.Name;
                if (methodName.StartsWith(BaseMethodPrefix) || methodName.StartsWith(FieldGetterPrefix) || methodName.StartsWith(FieldSetterPrefix)) {
                    foreach (string newName in GetBaseName(mi, specialNames)) {
                        if (mi.IsSpecialName && (newName.StartsWith("get_") || newName.StartsWith("set_"))) {
                            // if it's a property we want to override it
                            string propName = newName.Substring(4);

                            MemberInfo[] defaults = _baseType.GetDefaultMembers();
                            if (defaults.Length > 0) {
                                // if it's an indexer then we want to override get_Item/set_Item methods
                                // which map to __getitem__ and __setitem__ as normal Python methods.
                                foreach (MemberInfo method in defaults) {
                                    if (method.Name == propName) {
                                        StoreOverriddenMethod(mi, newName);
                                        break;
                                    }
                                }
                            }

                            StoreOverriddenProperty(mi, newName);
                        } else if (mi.IsSpecialName && (newName.StartsWith(FieldGetterPrefix) || newName.StartsWith(FieldSetterPrefix))) {
                            StoreOverriddenField(mi, newName);
                        } else {
                            // not a property, just store the overridden method.
                            StoreOverriddenMethod(mi, newName);
                        }
                    }
                }
            }
        }

        private void StoreOverriddenProperty(MethodInfo mi, string newName) {
            string propName = newName.Substring(4); // get_ or set_
            ExtensionPropertyTracker newProp = null;
            foreach (PropertyInfo pi in _baseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
                if (pi.Name == propName) {
                    if (newName.StartsWith("get_")) {
                        newProp = AddPropertyInfo(propName, mi, null);
                    } else if (newName.StartsWith("set_")) {
                        newProp = AddPropertyInfo(propName, null, mi);
                    }
                }
            }

            if (newProp != null) {
                // back-patch any existing functions so that cached rules will work
                // when called again...
                foreach (ReflectedGetterSetter rg in PythonTypeOps._propertyCache.Values) {
                    if (rg.DeclaringType != _baseType ||
                        rg.__name__ != newProp.Name) {
                        continue;
                    }

                    if (newProp.GetGetMethod(true) != null) {
                        rg.AddGetter(newProp.GetGetMethod(true));
                    }

                    if (newProp.GetSetMethod(true) != null) {
                        rg.AddGetter(newProp.GetSetMethod(true));
                    }
                }
            }
        }

        private void StoreOverriddenField(MethodInfo mi, string newName) {
            string fieldName = newName.Substring(FieldGetterPrefix.Length); // get_ or set_
            foreach (FieldInfo pi in _baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
                if (pi.Name == fieldName) {
                    if (newName.StartsWith(FieldGetterPrefix)) {
                        AddPropertyInfo(fieldName, mi, null);
                    } else if (newName.StartsWith(FieldSetterPrefix)) {
                        AddPropertyInfo(fieldName, null, mi);
                    }
                }
            }
        }

        private ExtensionPropertyTracker AddPropertyInfo(string/*!*/ propName, MethodInfo get, MethodInfo set) {
            MethodInfo mi = get ?? set;

            Dictionary<string, List<ExtensionPropertyTracker>> propInfoList;
            
            if (!_overriddenProperties.TryGetValue(_baseType, out propInfoList)) {
                _overriddenProperties[_baseType] = propInfoList = new Dictionary<string, List<ExtensionPropertyTracker>>();
            }
            
            List<ExtensionPropertyTracker> trackers;

            if (!propInfoList.TryGetValue(propName, out trackers)) {
                propInfoList[propName] = trackers = new List<ExtensionPropertyTracker>();
            }

            ExtensionPropertyTracker res;
            for (int i = 0; i < trackers.Count; i++) {
                if (trackers[i].DeclaringType == mi.DeclaringType) {
                    trackers[i] = res = new ExtensionPropertyTracker(
                        propName,
                        get ?? trackers[i].GetGetMethod(),
                        set ?? trackers[i].GetSetMethod(),
                        null,
                        mi.DeclaringType
                    );
                    return res;
                }
            }

            trackers.Add(
                res = new ExtensionPropertyTracker(
                    propName,
                    mi,
                    null,
                    null,
                    mi.DeclaringType
                )
            );

            return res;
        }

        private void StoreOverriddenMethod(MethodInfo mi, string newName) {
            MemberInfo[] members = _baseType.GetMember(newName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            Debug.Assert(members.Length > 0, String.Format("{0} from {1}", newName, _baseType.Name));
            Type declType = members[0].DeclaringType;

            string pythonName = newName;
            switch (newName) {
                case "get_Item": pythonName = "__getitem__"; break;
                case "set_Item": pythonName = "__setitem__"; break;
            }

            // back-patch any existing functions so that cached rules will work
            // when called again...
            lock (PythonTypeOps._functions) {
                foreach (BuiltinFunction bf in PythonTypeOps._functions.Values) {
                    if (bf.Name == pythonName && bf.DeclaringType == declType) {
                        bf.AddMethod(mi);
                        break;
                    }
                }
            }

            lock (_overriddenMethods) {
                Dictionary<string, List<MethodInfo>> overrideInfo;
                if (!_overriddenMethods.TryGetValue(declType, out overrideInfo)) {
                    _overriddenMethods[declType] = overrideInfo = new Dictionary<string, List<MethodInfo>>();
                }

                List<MethodInfo> methods;
                if (!overrideInfo.TryGetValue(newName, out methods)) {
                    overrideInfo[newName] = methods = new List<MethodInfo>();
                }

                methods.Add(mi);
            }
        }

        internal static IList<MethodInfo> GetOverriddenMethods(Type type, string name) {
            lock (_overriddenMethods) {
                Dictionary<string, List<MethodInfo>> methods;
                List<MethodInfo> res = null;
                Type curType = type;
                while (curType != null) {
                    if (_overriddenMethods.TryGetValue(curType, out methods)) {
                        List<MethodInfo> methodList;
                        if (methods.TryGetValue(name, out methodList)) {
                            if (res == null) {
                                res = methodList;
                            } else {
                                res = new List<MethodInfo>(res);
                                res.AddRange(methodList);
                            }
                        }
                    }
                    curType = curType.BaseType;
                }
                if (res != null) {
                    return res;
                }
            }
            return new MethodInfo[0];
        }
        
        internal static IList<ExtensionPropertyTracker> GetOverriddenProperties(Type type, string name) {
            lock (_overriddenProperties) {
                Dictionary<string, List<ExtensionPropertyTracker>> props;
                if (_overriddenProperties.TryGetValue(type, out props)) {
                    List<ExtensionPropertyTracker> propList;
                    if (props.TryGetValue(name, out propList)) {
                        return propList;
                    }
                }
            }

            return new ExtensionPropertyTracker[0];
        }

        private void DoInterfaceType(Type interfaceType, Dictionary<Type, bool> doneTypes, Dictionary<string, List<string>> specialNames) {
            if (interfaceType == typeof(IDynamicMetaObjectProvider)) {
                // very tricky, we'll handle it when we're creating
                // our own IDynamicMetaObjectProvider interface
                return;
            }

            if (doneTypes.ContainsKey(interfaceType)) return;
            doneTypes.Add(interfaceType, true);
            OverrideMethods(interfaceType, specialNames);

            foreach (Type t in interfaceType.GetInterfaces()) {
                DoInterfaceType(t, doneTypes, specialNames);
            }
        }

        private void OverrideConstructor(ConstructorInfo parentConstructor) {
            ParameterInfo[] pis = parentConstructor.GetParameters();
            if (pis.Length == 0 && typeof(IPythonObject).IsAssignableFrom(_baseType)) {
                // default ctor on a base type, don't override this one, it assumes
                // the PythonType is some default value and we'll always be unique.
                return;
            }

            ParameterInfo[] overrideParams = GetOverrideCtorSignature(pis);

            Type[] argTypes = new Type[overrideParams.Length];
            string[] paramNames = new string[overrideParams.Length];
            for (int i = 0; i < overrideParams.Length; i++) {
                argTypes[i] = overrideParams[i].ParameterType;
                paramNames[i] = overrideParams[i].Name;
            }

            ConstructorBuilder cb = _tg.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, argTypes);

            for (int i = 0; i < overrideParams.Length; i++) {
                ParameterBuilder pb = cb.DefineParameter(i + 1,
                    overrideParams[i].Attributes,
                    overrideParams[i].Name);

                int origIndex = GetOriginalIndex(pis, overrideParams, i);
                if (origIndex >= 0) {
                    if (pis[origIndex].IsDefined(typeof(ParamArrayAttribute), false)) {
                        pb.SetCustomAttribute(new CustomAttributeBuilder(
                            typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), ArrayUtils.EmptyObjects));
                    } else if (pis[origIndex].IsDefined(typeof(ParamDictionaryAttribute), false)) {
                        pb.SetCustomAttribute(new CustomAttributeBuilder(
                            typeof(ParamDictionaryAttribute).GetConstructor(Type.EmptyTypes), ArrayUtils.EmptyObjects));
                    }

                    if ((pis[origIndex].Attributes & ParameterAttributes.HasDefault) != 0) {
                        pb.SetConstant(pis[origIndex].DefaultValue);
                    }
                }
            }

            ILGen il = CreateILGen(cb.GetILGenerator());

            int typeArg;
            if (pis.Length == 0 || pis[0].ParameterType != typeof(CodeContext)) {
                typeArg = 1;
            } else {
                typeArg = 2;
            }

            // this.__class__ = <arg?>
            //  can occur 2 ways:
            //      1. If we have our own _typeField then we set it
            //      2. If we're a subclass of IPythonObject (e.g. one of our exception classes) then we'll flow it to the
            //             base type constructor which will set it.
            if (!typeof(IPythonObject).IsAssignableFrom(_baseType)) {
                il.EmitLoadArg(0);
                // base class could have CodeContext parameter in which case our type is the 2nd parameter.
                il.EmitLoadArg(typeArg);
                il.EmitFieldSet(_typeField);
            }

            if (_explicitMO != null) {
                il.Emit(OpCodes.Ldarg_0);
                il.EmitNew(_explicitMO.FieldType.GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stfld, _explicitMO);
            }

            // initialize all slots to Uninitialized.instance
            if (_slots != null) {
                MethodInfo init = typeof(PythonOps).GetMethod("InitializeUserTypeSlots");

                il.EmitLoadArg(0);
                
                il.EmitLoadArg(typeArg);
                il.EmitCall(init);
                
                il.EmitFieldSet(_slotsField);
            }

            CallBaseConstructor(parentConstructor, pis, overrideParams, il);
        }

        /// <summary>
        /// Gets the position for the parameter which we are overriding.
        /// </summary>
        /// <param name="pis"></param>
        /// <param name="overrideParams"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int GetOriginalIndex(ParameterInfo[] pis, ParameterInfo[] overrideParams, int i) {
            if (pis.Length == 0 || pis[0].ParameterType != typeof(CodeContext)) {
                return i - (overrideParams.Length - pis.Length);
            }

            // context & cls are swapped, context comes first.
            if (i == 1) return -1;
            if (i == 0) return 0;

            return i - (overrideParams.Length - pis.Length);
        }

        private static void CallBaseConstructor(ConstructorInfo parentConstructor, ParameterInfo[] pis, ParameterInfo[] overrideParams, ILGen il) {
            il.EmitLoadArg(0);
#if DEBUG
            int lastIndex = -1;
#endif
            for (int i = 0; i < overrideParams.Length; i++) {
                int index = GetOriginalIndex(pis, overrideParams, i);

#if DEBUG
                // we insert a new parameter (the class) but the parametrers should
                // still remain in the same order after the extra parameter is removed.
                if (index >= 0) {
                    Debug.Assert(index > lastIndex);
                    lastIndex = index;
                }
#endif
                if (index >= 0) {
                    il.EmitLoadArg(i + 1);
                }
            }
            il.Emit(OpCodes.Call, parentConstructor);
            il.Emit(OpCodes.Ret);
        }

        private ILGen _cctor;
        private LocalBuilder _cctorSymbolIdTemp;

        ILGen GetCCtor() {
            if (_cctor == null) {
                ConstructorBuilder cctor = _tg.DefineTypeInitializer();
                _cctor = CreateILGen(cctor.GetILGenerator());
            }
            return _cctor;
        }

        LocalBuilder GetCCtorSymbolIdTemp() {
            ILGen cctor = GetCCtor();
            if (_cctorSymbolIdTemp == null) {
                _cctorSymbolIdTemp = cctor.DeclareLocal(typeof(SymbolId));
            }
            return _cctorSymbolIdTemp;
        }


#if !SILVERLIGHT // ICustomTypeDescriptor
        private void ImplementCustomTypeDescriptor() {
            ImplementInterface(typeof(ICustomTypeDescriptor));

            foreach (MethodInfo m in typeof(ICustomTypeDescriptor).GetMethods()) {
                ImplementCTDOverride(m);
            }
        }

        private void ImplementCTDOverride(MethodInfo m) {
            MethodBuilder builder;
            ILGen il = DefineExplicitInterfaceImplementation(m, out builder);
            il.EmitLoadArg(0);

            ParameterInfo[] pis = m.GetParameters();
            Type[] paramTypes = new Type[pis.Length + 1];
            paramTypes[0] = typeof(object);
            for (int i = 0; i < pis.Length; i++) {
                il.EmitLoadArg(i + 1);
                paramTypes[i + 1] = pis[i].ParameterType;
            }

            il.EmitCall(typeof(CustomTypeDescHelpers), m.Name, paramTypes);
            il.EmitBoxing(m.ReturnType);
            il.Emit(OpCodes.Ret);
            _tg.DefineMethodOverride(builder, m);
        }
#endif

        protected bool NeedsDictionary {
            get {
                if (_slots == null) return true;
                if (_slots.Contains("__dict__")) return true;

                foreach (PythonType pt in _baseClasses) {
                    if (IsInstanceType(pt.UnderlyingSystemType)) return true;
                }

                return false;
            }
        }

        protected bool NeedsPythonObject {
            get {
                Type curType = _baseType;
                while (curType != null) {
                    if (_baseType.IsDefined(typeof(DynamicBaseTypeAttribute), true)) {
                        return false;
                    }
                    curType = curType.BaseType;
                }
                return true;
            }
        }

        private void ImplementDynamicObject() {
            ImplementInterface(typeof(IDynamicMetaObjectProvider));

            MethodInfo decl;
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(MethodAttributes.Private, typeof(IDynamicMetaObjectProvider), "GetMetaObject", out decl, out impl);
            MethodInfo mi = typeof(UserTypeOps).GetMethod("GetMetaObjectHelper");

            bool explicitDynamicObject = false;
            foreach (Type t in _interfaceTypes) {
                if (t == typeof(IDynamicMetaObjectProvider)) {
                    explicitDynamicObject = true;
                    break;
                }
            }

            LocalBuilder retVal = il.DeclareLocal(typeof(DynamicMetaObject));
            Label retLabel = il.DefineLabel();
            if (explicitDynamicObject) {
                _explicitMO = _tg.DefineField("__gettingMO", typeof(ThreadLocal<bool>), FieldAttributes.InitOnly | FieldAttributes.Private);

                Label ipyImpl = il.DefineLabel();
                Label noOverride = il.DefineLabel();
                Label retNull = il.DefineLabel();

                // check if the we're recursing (this enables the user to refer to self
                // during GetMetaObject calls)
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, _explicitMO);
                il.EmitPropertyGet(typeof(ThreadLocal<bool>), "Value");
                il.Emit(OpCodes.Brtrue, ipyImpl);

                // we're not recursing, set the flag...
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, _explicitMO);
                il.Emit(OpCodes.Ldc_I4_1);
                il.EmitPropertySet(typeof(ThreadLocal<bool>), "Value");

                il.BeginExceptionBlock();

                LocalBuilder callTarget = EmitNonInheritedMethodLookup("GetMetaObject", il);

                il.Emit(OpCodes.Brfalse, noOverride);

                // call the user GetMetaObject function
                EmitClrCallStub(il, typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject"), callTarget);

                // check for null return
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Beq, retNull);
                
                // store the local value
                il.Emit(OpCodes.Stloc_S, retVal.LocalIndex);

                // returned a value, that's our result
                il.Emit(OpCodes.Leave, retLabel);

                // user returned null, fallback to base impl
                il.MarkLabel(retNull);
                il.Emit(OpCodes.Pop);
                
                // no override exists
                il.MarkLabel(noOverride);

                // will emit leave to end of exception block
                il.BeginFinallyBlock();

                // restore the flag now that we're done
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, _explicitMO);
                il.Emit(OpCodes.Ldc_I4_0);
                il.EmitPropertySet(typeof(ThreadLocal<bool>), "Value");

                il.EndExceptionBlock();

                // no user defined function or no result
                il.MarkLabel(ipyImpl);
            }

            il.EmitLoadArg(0);  // this
            il.EmitLoadArg(1);  // parameter
                
            // baseMetaObject
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(_baseType)) {
                InterfaceMapping imap = _baseType.GetInterfaceMap(typeof(IDynamicMetaObjectProvider));

                il.EmitLoadArg(0);  // this
                il.EmitLoadArg(1);  // parameter
                il.EmitCall(imap.TargetMethods[0]);
            } else {
                il.EmitNull();
            }

            il.EmitCall(mi);
            il.Emit(OpCodes.Stloc, retVal.LocalIndex);

            il.MarkLabel(retLabel);

            il.Emit(OpCodes.Ldloc, retVal.LocalIndex);
            il.Emit(OpCodes.Ret);

            _tg.DefineMethodOverride(impl, decl);
        }

        private void ImplementIPythonObject() {
            if (NeedsPythonObject) {
                ILGen il;
                MethodInfo decl;
                MethodBuilder impl;

                ImplementInterface(typeof(IPythonObject));

                MethodAttributes attrs = MethodAttributes.Private;
                if (_slots != null) attrs = MethodAttributes.Virtual;

                il = DefineMethodOverride(attrs, typeof(IPythonObject), "get_Dict", out decl, out impl);
                if (NeedsDictionary) {
                    il.EmitLoadArg(0);
                    EmitGetDict(il);
                } else {
                    il.EmitNull();
                }
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(attrs, typeof(IPythonObject), "ReplaceDict", out decl, out impl);
                if (NeedsDictionary) {
                    il.EmitLoadArg(0);
                    il.EmitLoadArg(1);
                    EmitSetDict(il);
                    il.EmitBoolean(true);
                } else {
                    il.EmitBoolean(false);
                }
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(attrs, typeof(IPythonObject), "get_HasDictionary", out decl, out impl);
                il.EmitBoolean(NeedsDictionary);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(attrs, typeof(IPythonObject), "SetDict", out decl, out impl);
                if (NeedsDictionary) {
                    il.EmitLoadArg(0);
                    il.EmitFieldAddress(_dictField);
                    il.EmitLoadArg(1);
                    il.EmitCall(typeof(UserTypeOps), "SetDictHelper");
                } else {
                    il.EmitNull();
                }
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(attrs, typeof(IPythonObject), "get_PythonType", out decl, out impl);
                il.EmitLoadArg(0);
                il.EmitFieldGet(_typeField);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);

                il = DefineMethodOverride(attrs, typeof(IPythonObject), "SetPythonType", out decl, out impl);
                il.EmitLoadArg(0);
                il.EmitLoadArg(1);
                il.EmitFieldSet(_typeField);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);
            }
        }

        /// <summary>
        /// Defines an interface on the type that forwards all calls
        /// to a helper method in UserType.  The method names all will
        /// have Helper appended to them to get the name for UserType.  The 
        /// UserType version should take 1 extra parameter (self).
        /// </summary>
        /// <param name="intf"></param>
        /// <param name="fExplicit"></param>
        private void DefineHelperInterface(Type intf) {
            ImplementInterface(intf);
            MethodInfo[] mis = intf.GetMethods();

            foreach (MethodInfo mi in mis) {
                MethodBuilder impl;
                ILGen il = DefineExplicitInterfaceImplementation(mi, out impl);
                ParameterInfo[] pis = mi.GetParameters();

                MethodInfo helperMethod = typeof(UserTypeOps).GetMethod(mi.Name + "Helper");
                int offset = 0;
                if (pis.Length > 0 && pis[0].ParameterType == typeof(CodeContext)) {
                    // if the interface takes CodeContext then the helper method better take
                    // it as well.
                    Debug.Assert(helperMethod.GetParameters()[0].ParameterType == typeof(CodeContext));
                    offset = 1;
                    il.EmitLoadArg(1);
                }

                il.EmitLoadArg(0);
                for (int i = offset; i < pis.Length; i++) {
                    il.EmitLoadArg(i + 1);
                }

                il.EmitCall(helperMethod);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, mi);
            }
        }

        private void ImplementPythonEquals() {
            if (this._baseType.GetInterface("IValueEquality", false) == null) {
                DefineHelperInterface(typeof(IValueEquality));
            }
        }

        private void CreateWeakRefField() {
            if (_weakrefField != null) {
                return;
            }

            _weakrefField = _baseType.GetField("__weakref__");
            if (_weakrefField == null) {
                _weakrefField = _tg.DefineField("__weakref__", typeof(WeakRefTracker), FieldAttributes.Private);
            }
        }

        internal bool BaseHasWeakRef(PythonType curType) {
            PythonType dt = curType;
            PythonTypeSlot dts;
            if (dt != null &&
                dt.TryLookupSlot(DefaultContext.Default, Symbols.Slots, out dts) &&
                dt.TryLookupSlot(DefaultContext.Default, Symbols.WeakRef, out dts)) {
                return true;
            }

            foreach (PythonType baseType in curType.BaseTypes) {
                if (BaseHasWeakRef(baseType)) return true;
            }
            return false;
        }

        protected virtual void ImplementWeakReference() {
            CreateWeakRefField();

            bool isWeakRefAble = true;
            if (_slots != null && !_slots.Contains("__weakref__")) {
                // always define the field, only implement the interface
                // if we are slotless or the user defined __weakref__ in slots
                bool baseHasWeakRef = false;
                foreach (object pt in _baseClasses) {
                    PythonType dt = pt as PythonType;
                    if (dt != null && BaseHasWeakRef(dt)) {
                        baseHasWeakRef = true;
                        break;
                    }
                }
                if (baseHasWeakRef) return;

                isWeakRefAble = false;
            }

            ImplementInterface(typeof(IWeakReferenceable));
            MethodInfo decl;
            MethodBuilder impl;
            ILGen il;

            il = DefineMethodOverride(MethodAttributes.Private, typeof(IWeakReferenceable), "SetWeakRef", out decl, out impl);
            if (!isWeakRefAble) {
                il.EmitBoolean(false);
            } else {
                il.EmitLoadArg(0);
                il.EmitLoadArg(1);
                il.EmitFieldSet(_weakrefField);
                il.EmitBoolean(true);
            }
            il.Emit(OpCodes.Ret);
            _tg.DefineMethodOverride(impl, decl);

            il = DefineMethodOverride(MethodAttributes.Private, typeof(IWeakReferenceable), "SetFinalizer", out decl, out impl);
            il.EmitLoadArg(0);
            il.EmitFieldAddress(_weakrefField);
            il.EmitLoadArg(1);
            il.EmitCall(typeof(UserTypeOps).GetMethod("SetFinalizerWorker"));
            il.Emit(OpCodes.Ret);
            _tg.DefineMethodOverride(impl, decl);

            il = DefineMethodOverride(MethodAttributes.Private, typeof(IWeakReferenceable), "GetWeakRef", out decl, out impl);
            il.EmitLoadArg(0);
            il.EmitFieldGet(_weakrefField);
            il.Emit(OpCodes.Ret);
            _tg.DefineMethodOverride(impl, decl);
        }

        private void ImplementSlots() {
            if (_slots != null) {
                _slotsField = _tg.DefineField(".SlotValues", typeof(object[]), FieldAttributes.Private);
                _tg.AddInterfaceImplementation(typeof(IObjectWithSlots));

                MethodInfo decl;
                MethodBuilder impl;
                ILGen il = DefineMethodOverride(MethodAttributes.Private, typeof(IObjectWithSlots), "GetSlots", out decl, out impl);
                il.EmitLoadArg(0);
                il.EmitFieldGet(_slotsField);
                il.Emit(OpCodes.Ret);
                _tg.DefineMethodOverride(impl, decl);
            }
        }

        private void ImplementProtectedFieldAccessors(Dictionary<string, List<string>> specialNames) {
            // For protected fields to be accessible from the derived type in Silverlight,
            // we need to create public helper methods that expose them. These methods are
            // used by the IDynamicMetaObjectProvider implementation (in MetaUserObject)

            FieldInfo[] fields = _baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo fi in fields) {
                if (!fi.IsFamily && !fi.IsFamilyOrAssembly) {
                    continue;
                }

                List<string> fieldAccessorNames = new List<string>();

                PropertyBuilder pb = _tg.DefineProperty(fi.Name, PropertyAttributes.None, fi.FieldType, Type.EmptyTypes);
                MethodAttributes methodAttrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
                if (fi.IsStatic) {
                    methodAttrs |= MethodAttributes.Static;
                }

                MethodBuilder method;
                method = _tg.DefineMethod(FieldGetterPrefix + fi.Name, methodAttrs,
                                          fi.FieldType, Type.EmptyTypes);
                ILGen il = CreateILGen(method.GetILGenerator());
                if (!fi.IsStatic) {
                    il.EmitLoadArg(0);
                }

                if (fi.IsLiteral) {
                    // literal fields need to be inlined directly in here... We use GetRawConstant
                    // which will work even in partial trust if the constant is protected.
                    object value = fi.GetRawConstantValue();
                    switch (Type.GetTypeCode(fi.FieldType)) {
                        case TypeCode.Boolean:
                            if ((bool)value) {
                                il.Emit(OpCodes.Ldc_I4_1);
                            } else {
                                il.Emit(OpCodes.Ldc_I4_0);
                            }
                            break;
                        case TypeCode.Byte: il.Emit(OpCodes.Ldc_I4, (byte)value); break;
                        case TypeCode.Char: il.Emit(OpCodes.Ldc_I4, (char)value); break;
                        case TypeCode.Double: il.Emit(OpCodes.Ldc_R8, (double)value); break;
                        case TypeCode.Int16: il.Emit(OpCodes.Ldc_I4, (short)value); break;
                        case TypeCode.Int32: il.Emit(OpCodes.Ldc_I4, (int)value); break;
                        case TypeCode.Int64: il.Emit(OpCodes.Ldc_I8, (long)value); break;
                        case TypeCode.SByte: il.Emit(OpCodes.Ldc_I4, (sbyte)value); break;
                        case TypeCode.Single: il.Emit(OpCodes.Ldc_R4, (float)value); break;
                        case TypeCode.String: il.Emit(OpCodes.Ldstr, (string)value); break;
                        case TypeCode.UInt16: il.Emit(OpCodes.Ldc_I4, (ushort)value); break;
                        case TypeCode.UInt32: il.Emit(OpCodes.Ldc_I4, (uint)value); break;
                        case TypeCode.UInt64: il.Emit(OpCodes.Ldc_I8, (ulong)value); break;
                    }
                } else {
                    il.EmitFieldGet(fi);
                }
                il.Emit(OpCodes.Ret);

                pb.SetGetMethod(method);
                fieldAccessorNames.Add(method.Name);

                if (!fi.IsLiteral && !fi.IsInitOnly) {
                    method = _tg.DefineMethod(FieldSetterPrefix + fi.Name, methodAttrs,
                                              null, new Type[] { fi.FieldType });
                    method.DefineParameter(1, ParameterAttributes.None, "value");
                    il = CreateILGen(method.GetILGenerator());
                    il.EmitLoadArg(0);
                    if (!fi.IsStatic) {
                        il.EmitLoadArg(1);
                    }
                    il.EmitFieldSet(fi);
                    il.Emit(OpCodes.Ret);
                    pb.SetSetMethod(method);

                    fieldAccessorNames.Add(method.Name);
                }

                specialNames[fi.Name] = fieldAccessorNames;                
            }
        }

        /// <summary>
        /// Overrides methods - this includes all accessible virtual methods as well as protected non-virtual members
        /// including statics and non-statics.
        /// </summary>
        private void OverrideMethods(Type type, Dictionary<string, List<string>> specialNames) {
            // if we have conflicting virtual's do to new slots only override the methods on the
            // most derived class.
            Dictionary<KeyValuePair<string, MethodSignatureInfo>, MethodInfo> added = new Dictionary<KeyValuePair<string, MethodSignatureInfo>, MethodInfo>();

            MethodInfo overridden;
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (MethodInfo mi in methods) {
                KeyValuePair<string, MethodSignatureInfo> key = new KeyValuePair<string, MethodSignatureInfo>(mi.Name, new MethodSignatureInfo(mi.IsStatic, mi.GetParameters()));

                if (!added.TryGetValue(key, out overridden)) {
                    added[key] = mi;
                    continue;
                }

                if (overridden.DeclaringType.IsAssignableFrom(mi.DeclaringType)) {
                    added[key] = mi;
                }
            }

            Dictionary<PropertyInfo, PropertyBuilder> overriddenProperties = new Dictionary<PropertyInfo, PropertyBuilder>();
            foreach (MethodInfo mi in added.Values) {
                if (!ShouldOverrideVirtual(mi) || !CanOverrideMethod(mi)) continue;

                if (mi.IsPublic || mi.IsFamily || mi.IsFamilyOrAssembly) {
                    if (mi.IsSpecialName) {
                        OverrideSpecialName(mi, specialNames, overriddenProperties);
                    } else {
                        OverrideBaseMethod(mi, specialNames);
                    }
                }
            }
        }

        private void OverrideSpecialName(MethodInfo mi, Dictionary<string, List<string>> specialNames, Dictionary<PropertyInfo, PropertyBuilder> overridden) {
            if (!mi.IsVirtual || mi.IsFinal) {
                if ((mi.IsFamily || mi.IsSpecialName) && (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))) {
                    // need to be able to call into protected getter/setter methods from derived types,
                    // even if these methods aren't virtual and we are in partial trust.
                    List<string> methodNames = new List<string>();
                    methodNames.Add(mi.Name);
                    specialNames[mi.Name] = methodNames;
                    MethodBuilder mb = CreateSuperCallHelper(mi);

                    foreach (PropertyInfo pi in mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                        if (pi.GetGetMethod(true) == mi || pi.GetSetMethod(true) == mi) {
                            AddPublicProperty(mi, overridden, mb, pi);
                            break;
                        }
                    }
                }
            } else if (!TryOverrideProperty(mi, specialNames, overridden)) {
                string name;
                EventInfo[] eis = mi.DeclaringType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (EventInfo ei in eis) {
                    if (ei.GetAddMethod() == mi) {
                        if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), ei, mi, out name) == NameType.None) return;
                        CreateVTableEventOverride(mi, mi.Name);
                        return;
                    } else if (ei.GetRemoveMethod() == mi) {
                        if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), ei, mi, out name) == NameType.None) return;
                        CreateVTableEventOverride(mi, mi.Name);
                        return;
                    }
                }

                OverrideBaseMethod(mi, specialNames);
            }
        }

        private bool TryOverrideProperty(MethodInfo mi, Dictionary<string, List<string>> specialNames, Dictionary<PropertyInfo, PropertyBuilder> overridden) {
            string name;
            PropertyInfo[] pis = mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            List<string> names = new List<string>();
            names.Add(mi.Name);
            specialNames[mi.Name] = names;
            MethodBuilder mb = null;
            PropertyInfo foundProperty = null;
            foreach (PropertyInfo pi in pis) {
                if (pi.GetIndexParameters().Length > 0) {
                    if (mi == pi.GetGetMethod(true)) {
                        mb = CreateVTableMethodOverride(mi, "__getitem__");
                        if (!mi.IsAbstract) {
                            CreateSuperCallHelper(mi);
                        }
                        foundProperty = pi;
                        break;
                    } else if (mi == pi.GetSetMethod(true)) {
                        mb = CreateVTableMethodOverride(mi, "__setitem__");
                        if (!mi.IsAbstract) {
                            CreateSuperCallHelper(mi);
                        }
                        foundProperty = pi;
                        break;
                    }
                } else if (mi == pi.GetGetMethod(true)) {
                    if (mi.Name != "get_PythonType") {
                        if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) {
                            return true;
                        }
                        mb = CreateVTableGetterOverride(mi, name);
                        if (!mi.IsAbstract) {
                            CreateSuperCallHelper(mi);
                        }
                    }
                    foundProperty = pi;
                    break;
                } else if (mi == pi.GetSetMethod(true)) {
                    if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), pi, mi, out name) == NameType.None) {
                        return true;
                    }
                    mb = CreateVTableSetterOverride(mi, name);
                    if (!mi.IsAbstract) {
                        CreateSuperCallHelper(mi);
                    }
                    foundProperty = pi;
                    break;
                }
            }

            if (foundProperty != null) {
                AddPublicProperty(mi, overridden, mb, foundProperty);
                return true;
            }
            return false;
        }

        private void AddPublicProperty(MethodInfo mi, Dictionary<PropertyInfo, PropertyBuilder> overridden, MethodBuilder mb, PropertyInfo foundProperty) {
            MethodInfo getter = foundProperty.GetGetMethod(true);
            MethodInfo setter = foundProperty.GetSetMethod(true);
            if (IsProtected(getter) || IsProtected(setter)) {
                PropertyBuilder builder;
                if (!overridden.TryGetValue(foundProperty, out builder)) {
                    ParameterInfo[] indexArgs = foundProperty.GetIndexParameters();
                    Type[] paramTypes = new Type[indexArgs.Length];
                    for (int i = 0; i < paramTypes.Length; i++) {
                        paramTypes[i] = indexArgs[i].ParameterType;
                    }

                    overridden[foundProperty] = builder = _tg.DefineProperty(foundProperty.Name, foundProperty.Attributes, foundProperty.PropertyType, paramTypes);
                }

                if (foundProperty.GetGetMethod(true) == mi) {
                    builder.SetGetMethod(mb);
                } else if (foundProperty.GetSetMethod(true) == mi) {
                    builder.SetSetMethod(mb);
                }
            }
        }

        private static bool IsProtected(MethodInfo mi) {
            if (mi != null) {
                return mi.IsFamilyOrAssembly || mi.IsFamily;
            }
            return false;
        }

        /// <summary>
        /// Loads all the incoming arguments and forwards them to mi which
        /// has the same signature and then returns the result
        /// </summary>
        private static void EmitBaseMethodDispatch(MethodInfo mi, ILGen il) {
            if (!mi.IsAbstract) {
                int offset = 0;
                if (!mi.IsStatic) {
                    il.EmitLoadArg(0);
                    offset = 1;
                }
                ParameterInfo[] parameters = mi.GetParameters();
                for (int i = 0; i < parameters.Length; i++) {
                    il.EmitLoadArg(i + offset);
                }
                il.EmitCall(OpCodes.Call, mi, null); // base call must be non-virtual
                il.Emit(OpCodes.Ret);
            } else {
                il.EmitLoadArg(0);
                il.EmitString(mi.Name);
                il.EmitCall(typeof(PythonOps), "MissingInvokeMethodException");
                il.Emit(OpCodes.Throw);
            }
        }

        private void OverrideBaseMethod(MethodInfo mi, Dictionary<string, List<string>> specialNames) {
            if ((!mi.IsVirtual || mi.IsFinal) && !mi.IsFamily) {
                return;
            }

            PythonType basePythonType;
            if (_baseType == mi.DeclaringType || _baseType.IsSubclassOf(mi.DeclaringType)) {
                basePythonType = DynamicHelpers.GetPythonTypeFromType(_baseType);
            } else {
                // We must be inherting from an interface
                Debug.Assert(mi.DeclaringType.IsInterface);
                basePythonType = DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType);
            }

            string name = null;
            if (NameConverter.TryGetName(basePythonType, mi, out name) == NameType.None)
                return;

            if (mi.DeclaringType == typeof(object) && mi.Name == "Finalize") return;

            List<string> names = new List<string>();
            names.Add(mi.Name);
            specialNames[mi.Name] = names;

            if (!mi.IsStatic) {
                CreateVTableMethodOverride(mi, name);
            }
            if (!mi.IsAbstract) {
                CreateSuperCallHelper(mi);
            }
        }

        /// <summary>
        /// Emits code to check if the class has overridden this specific
        /// function.  For example:
        /// 
        /// MyDerivedType.SomeVirtualFunction = ...
        ///     or
        /// 
        /// class MyDerivedType(MyBaseType):
        ///     def SomeVirtualFunction(self, ...):
        /// 
        /// </summary>
        internal LocalBuilder EmitBaseClassCallCheckForProperties(ILGen il, MethodInfo baseMethod, string name) {
            Label instanceCall = il.DefineLabel();
            LocalBuilder callTarget = il.DeclareLocal(typeof(object));

            il.EmitLoadArg(0);
            il.EmitFieldGet(_typeField);
            il.EmitLoadArg(0);
            EmitSymbolId(il, name);
            il.Emit(OpCodes.Ldloca, callTarget);
            il.EmitCall(typeof(UserTypeOps), "TryGetNonInheritedValueHelper");

            il.Emit(OpCodes.Brtrue, instanceCall);

            EmitBaseMethodDispatch(baseMethod, il);

            il.MarkLabel(instanceCall);

            return callTarget;
        }

        private MethodBuilder CreateVTableGetterOverride(MethodInfo mi, string name) {
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
            LocalBuilder callTarget = EmitBaseClassCallCheckForProperties(il, mi, name);

            il.Emit(OpCodes.Ldloc, callTarget);
            il.EmitLoadArg(0);
            EmitSymbolId(il, name);
            il.EmitCall(typeof(UserTypeOps), "GetPropertyHelper");

            if (!il.TryEmitImplicitCast(typeof(object), mi.ReturnType)) {
                EmitConvertFromObject(il, mi.ReturnType);
            }
            il.Emit(OpCodes.Ret);
            _tg.DefineMethodOverride(impl, mi);
            return impl;
        }

        /// <summary>
        /// Emit code to convert object to a given type. This code is semantically equivalent
        /// to PythonBinder.EmitConvertFromObject, except this version accepts ILGen whereas
        /// PythonBinder accepts Compiler. The Binder will chagne soon and the two will merge.
        /// </summary>
        public static void EmitConvertFromObject(ILGen il, Type toType) {
            if (toType == typeof(object)) return;
            if (toType.IsGenericParameter) {
                il.EmitCall(typeof(PythonOps).GetMethod("ConvertFromObject").MakeGenericMethod(toType));
                return;
            }

            MethodInfo fastConvertMethod = PythonBinder.GetFastConvertMethod(toType);
            if (fastConvertMethod != null) {
                il.EmitCall(fastConvertMethod);
            } else if (toType == typeof(void)) {
                il.Emit(OpCodes.Pop);
            } else if (typeof(Delegate).IsAssignableFrom(toType)) {
                il.EmitType(toType);
                il.EmitCall(typeof(Converter), "ConvertToDelegate");
                il.Emit(OpCodes.Castclass, toType);
            } else {
                Label end = il.DefineLabel();
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Isinst, toType);

                il.Emit(OpCodes.Brtrue_S, end);
                il.Emit(OpCodes.Ldtoken, toType);
                il.EmitCall(PythonBinder.GetGenericConvertMethod(toType));
                il.MarkLabel(end);

                il.Emit(OpCodes.Unbox_Any, toType); //??? this check may be redundant
            }
        }

        private MethodBuilder CreateVTableSetterOverride(MethodInfo mi, string name) {
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
            LocalBuilder callTarget = EmitBaseClassCallCheckForProperties(il, mi, name);

            il.Emit(OpCodes.Ldloc, callTarget);     // property
            il.EmitLoadArg(0);                      // instance
            il.EmitLoadArg(1);
            il.EmitBoxing(mi.GetParameters()[0].ParameterType);    // newValue
            EmitSymbolId(il, name);    // name
            il.EmitCall(typeof(UserTypeOps), "SetPropertyHelper");
            il.Emit(OpCodes.Ret);
            _tg.DefineMethodOverride(impl, mi);
            return impl;
        }

        private void CreateVTableEventOverride(MethodInfo mi, string name) {
            // override the add/remove method  
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(mi, out impl);

            LocalBuilder callTarget = EmitBaseClassCallCheckForProperties(il, mi, name);

            il.Emit(OpCodes.Ldloc, callTarget);
            il.EmitLoadArg(0);
            il.EmitLoadArg(0);
            il.EmitFieldGet(_typeField);
            il.EmitLoadArg(1);
            il.EmitBoxing(mi.GetParameters()[0].ParameterType);
            EmitSymbolId(il, name);
            il.EmitCall(typeof(UserTypeOps), "AddRemoveEventHelper");
            il.Emit(OpCodes.Ret);
            _tg.DefineMethodOverride(impl, mi);
        }

        private MethodBuilder CreateVTableMethodOverride(MethodInfo mi, string name) {
            ParameterInfo[] parameters = mi.GetParameters();
            MethodBuilder impl;
            ILGen il;
            if (mi.IsVirtual && !mi.IsFinal) {
                il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
            } else {
                impl = _tg.DefineMethod(
                    mi.Name,
                    mi.IsVirtual ?
                        (mi.Attributes | MethodAttributes.NewSlot) :
                        ((mi.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public),
                    mi.ReturnType,
                    ReflectionUtils.GetParameterTypes(parameters));
                il = CreateILGen(impl.GetILGenerator());
            }
            //CompilerHelpers.GetArgumentNames(parameters));  TODO: Set names

            LocalBuilder callTarget = EmitNonInheritedMethodLookup(name, il);            
            Label instanceCall = il.DefineLabel();
            il.Emit(OpCodes.Brtrue, instanceCall);

            // lookup failed, call the base class method (this returns or throws)
            EmitBaseMethodDispatch(mi, il);

            // lookup succeeded, call the user defined method & return
            il.MarkLabel(instanceCall);
            EmitClrCallStub(il, mi, callTarget);
            il.Emit(OpCodes.Ret);

            if (mi.IsVirtual && !mi.IsFinal) {
                _tg.DefineMethodOverride(impl, mi);
            }
            return impl;
        }

        /// <summary>
        /// Emits the call to lookup a member defined in the user's type.  Returns
        /// the local which stores the resulting value and leaves a value on the
        /// stack indicating the success of the lookup.
        /// </summary>
        private LocalBuilder EmitNonInheritedMethodLookup(string name, ILGen il) {
            LocalBuilder callTarget = il.DeclareLocal(typeof(object));

            // emit call to helper to do lookup
            il.EmitLoadArg(0);

            if (typeof(IPythonObject).IsAssignableFrom(_baseType)) {
                Debug.Assert(_typeField == null);
                il.EmitPropertyGet(TypeInfo._IPythonObject.PythonType);
            } else {
                il.EmitFieldGet(_typeField);
            }

            il.EmitLoadArg(0);
            EmitSymbolId(il, name);
            il.Emit(OpCodes.Ldloca, callTarget);
            il.EmitCall(typeof(UserTypeOps), "TryGetNonInheritedMethodHelper");
            return callTarget;
        }

        /// <summary>
        /// Creates a method for doing a base method dispatch.  This is used to support
        /// super(type, obj) calls.
        /// </summary>
        public MethodBuilder CreateSuperCallHelper(MethodInfo mi) {
            ParameterInfo[] parms = mi.GetParameters();
            Type[] types = ReflectionUtils.GetParameterTypes(parms);
            Type miType = mi.DeclaringType;
            for (int i = 0; i < types.Length; i++) {
                if (types[i] == miType) {
                    types[i] = _tg;
                }
            }

            MethodAttributes attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            if (mi.IsStatic) {
                attrs |= MethodAttributes.Static;
            }

            MethodBuilder method = _tg.DefineMethod(
                BaseMethodPrefix + mi.Name,
                attrs,
                mi.ReturnType, types
            );

            for (int i = 0; i < types.Length; i++) {
                method.DefineParameter(i + 1, ParameterAttributes.None, parms[i].Name);
            }

            EmitBaseMethodDispatch(mi, CreateILGen(method.GetILGenerator()));
            return method;
        }

        private Dictionary<SymbolId, FieldBuilder> _symbolFields = new Dictionary<SymbolId, FieldBuilder>();
        private void EmitSymbolId(ILGen il, string name) {
            Debug.Assert(name != null);
            SymbolId id = SymbolTable.StringToId(name);

            FieldBuilder fb;
            if (!_symbolFields.TryGetValue(id, out fb)) {
                fb = _tg.DefineField("symbol_" + name, typeof(int), FieldAttributes.Private | FieldAttributes.Static);
                ILGen cctor = GetCCtor();
                LocalBuilder localTmp = GetCCtorSymbolIdTemp();
                cctor.EmitString(name);
                cctor.EmitCall(typeof(SymbolTable), "StringToId");
                cctor.Emit(OpCodes.Stloc, localTmp);
                cctor.Emit(OpCodes.Ldloca, localTmp);
                cctor.EmitPropertyGet(typeof(SymbolId), "Id");
                cctor.EmitFieldSet(fb);

                _symbolFields[id] = fb;
            }

            il.EmitFieldGet(fb);
            // TODO: Cache the signature type!!!
            il.EmitNew(typeof(SymbolId), new Type[] { typeof(int) });
        }


        private Type FinishType() {
            if (_cctor != null) {
                _cctor.Emit(OpCodes.Ret);
            }

            return _tg.CreateType();
        }

        internal protected ILGen CreateILGen(ILGenerator il) {
            // TODO: Debugging support
            return new ILGen(il);
        }

        private ILGen DefineExplicitInterfaceImplementation(MethodInfo baseMethod, out MethodBuilder builder) {
            MethodAttributes attrs = baseMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.Public);
            attrs |= MethodAttributes.NewSlot | MethodAttributes.Final;

            Type[] baseSignature = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
            builder = _tg.DefineMethod(
                baseMethod.DeclaringType.Name + "." + baseMethod.Name,
                attrs,
                baseMethod.ReturnType,
                baseSignature
            );
            return CreateILGen(builder.GetILGenerator());
        }

        protected const MethodAttributes MethodAttributesToEraseInOveride = MethodAttributes.Abstract | MethodAttributes.ReservedMask;

        protected ILGen DefineMethodOverride(Type type, string name, out MethodInfo decl, out MethodBuilder impl) {
            return DefineMethodOverride(MethodAttributes.PrivateScope, type, name, out decl, out impl);
        }

        protected ILGen DefineMethodOverride(MethodAttributes extra, Type type, string name, out MethodInfo decl, out MethodBuilder impl) {
            decl = type.GetMethod(name);
            return DefineMethodOverride(extra, decl, out impl);
        }

        protected ILGen DefineMethodOverride(MethodInfo decl, out MethodBuilder impl) {
            return DefineMethodOverride(MethodAttributes.PrivateScope, decl, out impl);
        }

        protected ILGen DefineMethodOverride(MethodAttributes extra, MethodInfo decl, out MethodBuilder impl) {
            MethodAttributes finalAttrs = (decl.Attributes & ~(MethodAttributesToEraseInOveride)) | extra;
            if (!decl.DeclaringType.IsInterface) {
                finalAttrs &= ~MethodAttributes.NewSlot;
            }

            if ((extra & MethodAttributes.MemberAccessMask) != 0) {
                // remove existing member access, add new member access
                finalAttrs &= ~MethodAttributes.MemberAccessMask;
                finalAttrs |= extra;
            }
            Type[] signature = ReflectionUtils.GetParameterTypes(decl.GetParameters());
            impl = _tg.DefineMethod(decl.Name, finalAttrs, decl.ReturnType, signature);
            if (decl.IsGenericMethodDefinition) {
                Type[] args = decl.GetGenericArguments();
                string[] names = new string[args.Length];
                for (int i = 0; i < args.Length; i++) {
                    names[i] = args[i].Name;
                }
                var builders = impl.DefineGenericParameters(names);
                for (int i = 0; i < args.Length; i++) {
                    // Copy template parameter attributes
                    builders[i].SetGenericParameterAttributes(args[i].GenericParameterAttributes);

                    // Copy template parameter constraints
                    Type[] constraints = args[i].GetGenericParameterConstraints();
                    List<Type> interfaces = new List<Type>(constraints.Length);
                    foreach (Type constraint in constraints) {
                        if (constraint.IsInterface) {
                            interfaces.Add(constraint);
                        } else {
                            builders[i].SetBaseTypeConstraint(constraint);
                        }
                    }
                    if (interfaces.Count > 0) {
                        builders[i].SetInterfaceConstraints(interfaces.ToArray());
                    }
                }
            }
            return CreateILGen(impl.GetILGenerator());
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// This code is same as StubGenerator.cs in the Microsoft.Scripting, except it
        /// accepts ILGen instead of Compiler.
        /// </summary>
        private void EmitClrCallStub(ILGen il, MethodInfo mi, LocalBuilder callTarget) {
            int firstArg = 0;
            bool list = false;              // The list calling convention
            bool context = false;           // Context is an argument

            ParameterInfo[] pis = mi.GetParameters();
            if (pis.Length > 0) {
                if (pis[0].ParameterType == typeof(CodeContext)) {
                    firstArg = 1;
                    context = true;
                }
                if (pis[pis.Length - 1].IsDefined(typeof(ParamArrayAttribute), false)) {
                    list = true;
                }
            }
            ParameterInfo[] args = pis;
            int nargs = args.Length - firstArg;
            Type[] genericArgs = mi.GetGenericArguments();

            // Create the action
            ILGen cctor = GetCCtor();
            if (list || genericArgs.Length > 0) {
                // Use a complex call signature that includes param array and keywords
                cctor.EmitInt(nargs);
                cctor.EmitBoolean(list);

                // Emit an array of SymbolIds for the types
                cctor.EmitInt(genericArgs.Length);
                cctor.Emit(OpCodes.Newarr, typeof(SymbolId));
                for (int i = 0; i < genericArgs.Length; i++) {
                    cctor.Emit(OpCodes.Dup);
                    cctor.EmitInt(i);
                    cctor.Emit(OpCodes.Ldelema, typeof(SymbolId));
                    EmitSymbolId(cctor, genericArgs[i].Name);
                    cctor.Emit(OpCodes.Stobj, typeof(SymbolId));
                }
                cctor.EmitCall(typeof(PythonOps).GetMethod("MakeComplexCallAction"));
            } else {
                cctor.EmitInt(nargs);
                cctor.EmitCall(typeof(PythonOps).GetMethod("MakeSimpleCallAction"));
            }

            // Create the dynamic site
            Type siteType = CompilerHelpers.MakeCallSiteType(MakeSiteSignature(nargs + genericArgs.Length));
            FieldBuilder site = _tg.DefineField("site$" + _site++, siteType, FieldAttributes.Private | FieldAttributes.Static);
            cctor.EmitCall(siteType.GetMethod("Create"));
            cctor.EmitFieldSet(site);

            List<ReturnFixer> fixers = new List<ReturnFixer>(0);

            //
            // Emit the site invoke
            //
            il.EmitFieldGet(site);
            FieldInfo target = siteType.GetField("Target");
            il.EmitFieldGet(target);
            il.EmitFieldGet(site);

            // Emit the code context
            EmitCodeContext(il, context);

            il.Emit(OpCodes.Ldloc, callTarget);

            for (int i = firstArg; i < args.Length; i++) {
                ReturnFixer rf = ReturnFixer.EmitArgument(il, args[i], i + 1);
                if (rf != null) {
                    fixers.Add(rf);
                }
            }

            for (int i = 0; i < genericArgs.Length; i++) {
                il.EmitType(genericArgs[i]);
                il.EmitCall(typeof(DynamicHelpers).GetMethod("GetPythonTypeFromType"));
            }

            il.EmitCall(target.FieldType, "Invoke");

            foreach (ReturnFixer rf in fixers) {
                rf.FixReturn(il);
            }

            EmitConvertFromObject(il, mi.ReturnType);
        }

        private static void EmitCodeContext(ILGen il, bool context) {
            if (context) {
                il.EmitLoadArg(1);
            } else {
                il.EmitPropertyGet(typeof(DefaultContext).GetProperty("Default"));
            }
        }

        private static Type[] MakeSiteSignature(int nargs) {
            Type[] sig = new Type[nargs + 4];
            sig[0] = typeof(CallSite);
            sig[1] = typeof(CodeContext);
            for (int i = 2; i < sig.Length; i++) {
                sig[i] = typeof(object);
            }
            return sig;
        }
    }

    /// <summary>
    /// Same as the DLR ReturnFixer, but accepts lower level constructs,
    /// such as LocalBuilder, ParameterInfos and ILGen.
    /// </summary>
    sealed class ReturnFixer {
        private readonly ParameterInfo _parameter;
        private readonly LocalBuilder _reference;
        private readonly int _index;

        private ReturnFixer(LocalBuilder reference, ParameterInfo parameter, int index) {
            Debug.Assert(reference.LocalType.IsGenericType && reference.LocalType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            Debug.Assert(parameter.ParameterType.IsByRef);

            _parameter = parameter;
            _reference = reference;
            _index = index;
        }

        public void FixReturn(ILGen il) {
            il.EmitLoadArg(_index);
            il.Emit(OpCodes.Ldloc, _reference);
            il.EmitFieldGet(_reference.LocalType.GetField("Value"));
            il.EmitStoreValueIndirect(_parameter.ParameterType.GetElementType());
        }

        public static ReturnFixer EmitArgument(ILGen il, ParameterInfo parameter, int index) {
            il.EmitLoadArg(index);
            if (parameter.ParameterType.IsByRef) {
                Type elementType = parameter.ParameterType.GetElementType();
                Type concreteType = typeof(StrongBox<>).MakeGenericType(elementType);
                LocalBuilder refSlot = il.DeclareLocal(concreteType);
                il.EmitLoadValueIndirect(elementType);
                ConstructorInfo ci = concreteType.GetConstructor(new Type[] { elementType });
                il.Emit(OpCodes.Newobj, ci);
                il.Emit(OpCodes.Stloc, refSlot);
                il.Emit(OpCodes.Ldloc, refSlot);
                return new ReturnFixer(refSlot, parameter, index);
            } else {
                il.EmitBoxing(parameter.ParameterType);
                return null;
            }
        }
    }
}
