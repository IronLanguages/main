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
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Threading;
using Microsoft.Contracts;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {

    /// <summary>
    /// NamespaceTracker represent a CLS namespace.
    /// </summary>
    public class NamespaceTracker : MemberTracker, IAttributesCollection, IMembersList {
        // _dict contains all the currently loaded entries. However, there may be pending types that have
        // not yet been loaded in _typeNames
        internal Dictionary<string, MemberTracker> _dict = new Dictionary<string, MemberTracker>();

        internal readonly List<Assembly> _packageAssemblies = new List<Assembly>();
        internal readonly Dictionary<Assembly, TypeNames> _typeNames = new Dictionary<Assembly, TypeNames>();

        private readonly string _fullName; // null for the TopReflectedPackage
        private TopNamespaceTracker _topPackage;
        private int _id;

        private static int _masterId;

        #region Protected API Surface

        protected NamespaceTracker(string name) {
            UpdateId();
            _fullName = name;
        }

        [Confined]
        public override string ToString() {
            return base.ToString() + ":" + _fullName;
        }

        #endregion

        #region Internal API Surface

        internal NamespaceTracker GetOrMakeChildPackage(string childName, Assembly assem) {
            // lock is held when this is called
            Assert.NotNull(childName, assem);
            Debug.Assert(childName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
            Debug.Assert(_packageAssemblies.Contains(assem)); // Parent namespace must contain all the assemblies of the child

            MemberTracker ret;
            if (_dict.TryGetValue(childName, out ret)) {
                // If we have a module, then we add the assembly to the InnerModule
                // If it's not a module, we'll wipe it out below, eg "def System(): pass" then 
                // "import System" will result in the namespace being visible.
                NamespaceTracker package = ret as NamespaceTracker;
                if (package != null) {
                    if (!package._packageAssemblies.Contains(assem)) {
                        package._packageAssemblies.Add(assem);
                        package.UpdateSubtreeIds();
                    }
                    return package;
                }
            }

            return MakeChildPackage(childName, assem);
        }

        private NamespaceTracker MakeChildPackage(string childName, Assembly assem) {
            // lock is held when this is called
            Assert.NotNull(childName, assem);
            NamespaceTracker rp = new NamespaceTracker(GetFullChildName(childName));
            rp.SetTopPackage(_topPackage);
            rp._packageAssemblies.Add(assem);

            _dict[childName] = rp;
            return rp;
        }

        private string GetFullChildName(string childName) {
            Assert.NotNull(childName);
            Debug.Assert(childName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
            if (_fullName == null) {
                return childName;
            }

            return _fullName + Type.Delimiter + childName;
        }

        private static Type LoadType(Assembly assem, string fullTypeName) {
            Assert.NotNull(assem, fullTypeName);
            Type type = assem.GetType(fullTypeName);
            // We should ignore nested types. They will be loaded when the containing type is loaded
            Debug.Assert(type == null || !type.IsNested());
            return type;
        }

        internal void AddTypeName(string typeName, Assembly assem) {
            // lock is held when this is called
            Assert.NotNull(typeName, assem);
            Debug.Assert(typeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name

            if (!_typeNames.ContainsKey(assem)) {
                _typeNames[assem] = new TypeNames(assem, _fullName);
            }
            _typeNames[assem].AddTypeName(typeName);

            string normalizedTypeName = ReflectionUtils.GetNormalizedTypeName(typeName);
            if (_dict.ContainsKey(normalizedTypeName)) {
                // A similarly named type, namespace, or module already exists.
                Type newType = LoadType(assem, GetFullChildName(typeName));

                if (newType != null) {
                    object existingValue = _dict[normalizedTypeName];
                    TypeTracker existingTypeEntity = existingValue as TypeTracker;
                    if (existingTypeEntity == null) {
                        // Replace the existing namespace or module with the new type
                        Debug.Assert(existingValue is NamespaceTracker);
                        _dict[normalizedTypeName] = MemberTracker.FromMemberInfo(newType);
                    } else {
                        // Unify the new type with the existing type
                        _dict[normalizedTypeName] = TypeGroup.UpdateTypeEntity(existingTypeEntity, ReflectionCache.GetTypeTracker(newType));
                    }
                }
            }
        }

        /// <summary>
        /// Loads all the types from all assemblies that contribute to the current namespace (but not child namespaces)
        /// </summary>
        private void LoadAllTypes() {
            foreach (TypeNames typeNameList in _typeNames.Values) {
                foreach (string typeName in typeNameList.GetNormalizedTypeNames()) {
                    object value;
                    if (!TryGetValue(SymbolTable.StringToId(typeName), out value)) {
                        Debug.Assert(false, "We should never get here as TryGetMember should raise a TypeLoadException");
                        throw new TypeLoadException(typeName);
                    }
                }
            }
        }

        #endregion

        public override string Name {
            get {
                return _fullName;
            }
        }

        protected void DiscoverAllTypes(Assembly assem) {
            // lock is held when this is called
            Assert.NotNull(assem);

            NamespaceTracker previousPackage = null;
            string previousFullNamespace = String.Empty; // Note that String.Empty is not a valid namespace

            foreach (TypeName typeName in AssemblyTypeNames.GetTypeNames(assem, _topPackage.DomainManager.Configuration.PrivateBinding)) {
                NamespaceTracker package;
                Debug.Assert(typeName.Namespace != String.Empty);
                if (typeName.Namespace == previousFullNamespace) {
                    // We have a cache hit. We dont need to call GetOrMakePackageHierarchy (which generates
                    // a fair amount of temporary substrings)
                    package = previousPackage;
                } else {
                    package = GetOrMakePackageHierarchy(assem, typeName.Namespace);
                    previousFullNamespace = typeName.Namespace;
                    previousPackage = package;
                }

                package.AddTypeName(typeName.Name, assem);
            }
        }

        /// <summary>
        /// Populates the tree with nodes for each part of the namespace
        /// </summary>
        /// <param name="assem"></param>
        /// <param name="fullNamespace">Full namespace name. It can be null (for top-level types)</param>
        /// <returns></returns>
        private NamespaceTracker GetOrMakePackageHierarchy(Assembly assem, string fullNamespace) {
            // lock is held when this is called
            Assert.NotNull(assem);

            if (fullNamespace == null) {
                // null is the top-level namespace
                return this;
            }

            NamespaceTracker ret = this;
            string[] pieces = fullNamespace.Split(Type.Delimiter);
            for (int i = 0; i < pieces.Length; i++) {
                ret = ret.GetOrMakeChildPackage(pieces[i], assem);
            }

            return ret;
        }
        /// <summary>
        /// As a fallback, so if the type does exist in any assembly. This would happen if a new type was added
        /// that was not in the hardcoded list of types. 
        /// This code is not accurate because:
        /// 1. We dont deal with generic types (TypeCollision). 
        /// 2. Previous calls to GetCustomMemberNames (eg. "from foo import *" in Python) would not have included this type.
        /// 3. This does not deal with new namespaces added to the assembly
        /// </summary>
        private MemberTracker CheckForUnlistedType(string nameString) {
            Assert.NotNull(nameString);

            string fullTypeName = GetFullChildName(nameString);
            foreach (Assembly assem in _packageAssemblies) {
                Type type = assem.GetType(fullTypeName, false);
                if (type == null || type.IsNested()) {
                    continue;
                }

                bool publishType = type.IsPublic || _topPackage.DomainManager.Configuration.PrivateBinding;
                if (!publishType) {
                    continue;
                }

                // We dont use TypeCollision.UpdateTypeEntity here because we do not handle generic type names                    
                return ReflectionCache.GetTypeTracker(type);
            }

            return null;
        }

        #region IAttributesCollection Members

        public void Add(SymbolId name, object value) {
            throw new InvalidOperationException();
        }

        public bool TryGetValue(SymbolId name, out object value) {
            MemberTracker tmp;
            bool res = TryGetValue(name, out tmp);
            value = tmp;
            return res;
        }

        public bool TryGetValue(SymbolId name, out MemberTracker value) {
            lock (_topPackage.HierarchyLock) {
                LoadNamespaces();

                if (_dict.TryGetValue(SymbolTable.IdToString(name), out value)) {
                    return true;
                }

                MemberTracker existingTypeEntity = null;
                string nameString = SymbolTable.IdToString(name);

                if (nameString.IndexOf(Type.Delimiter) != -1) {
                    value = null;
                    return false;
                }

                // Look up the type names and load the type if its name exists

                foreach (KeyValuePair<Assembly, TypeNames> kvp in _typeNames) {
                    if (!kvp.Value.Contains(nameString)) {
                        continue;
                    }

                    existingTypeEntity = kvp.Value.UpdateTypeEntity((TypeTracker)existingTypeEntity, nameString);
                }

                if (existingTypeEntity == null) {
                    existingTypeEntity = CheckForUnlistedType(nameString);
                }

                if (existingTypeEntity != null) {
                    _dict[SymbolTable.IdToString(name)] = existingTypeEntity;
                    value = existingTypeEntity;
                    return true;
                }

                return false;
            }
        }

        public bool Remove(SymbolId name) {
            throw new InvalidOperationException();
        }

        public bool ContainsKey(SymbolId name) {
            object dummy;
            return TryGetValue(name, out dummy);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public object this[SymbolId name] {
            get {
                object res;
                if (TryGetValue(name, out res)) {
                    return res;
                }
                throw new KeyNotFoundException();
            }
            set {
                throw new InvalidOperationException();
            }
        }

        public IDictionary<SymbolId, object> SymbolAttributes {
            get {
                LoadNamespaces();

                Dictionary<SymbolId, object> res = new Dictionary<SymbolId, object>();
                foreach (KeyValuePair<object, object> kvp in this) {
                    string strkey = kvp.Key as string;
                    if (strkey != null) {
                        res[SymbolTable.StringToId(strkey)] = kvp.Value;
                    }
                }

                return res;
            }
        }

        public void AddObjectKey(object name, object value) {
            throw new InvalidOperationException();
        }

        public bool TryGetObjectValue(object name, out object value) {
            string str = name as string;
            if (str != null) {
                return TryGetValue(SymbolTable.StringToId(str), out value);
            }

            value = null;
            return false;
        }

        public bool RemoveObjectKey(object name) {
            throw new InvalidOperationException();
        }

        public bool ContainsObjectKey(object name) {
            object dummy;
            return TryGetObjectValue(name, out dummy);
        }

        public IDictionary<object, object> AsObjectKeyedDictionary() {
            LoadNamespaces();

            lock (_topPackage.HierarchyLock) {
                Dictionary<object, object> res = new Dictionary<object, object>();
                foreach (KeyValuePair<string, MemberTracker> kvp in _dict) {
                    res[kvp.Key] = kvp.Value;
                }
                return res;
            }
        }

        public int Count {
            get { return _dict.Count; }
        }

        public ICollection<object> Keys {
            get {
                LoadNamespaces();

                lock (_topPackage.HierarchyLock) {
                    List<object> res = new List<object>();
                    return (ICollection<object>)AddKeys(res);
                }
            }
        }

        private IList AddKeys(IList res) {
            foreach (string s in _dict.Keys) {
                res.Add(s);
            }

            foreach (KeyValuePair<Assembly, TypeNames> kvp in _typeNames) {
                foreach (string typeName in kvp.Value.GetNormalizedTypeNames()) {
                    if (!res.Contains(typeName)) {
                        res.Add(typeName);
                    }
                }
            }
            
            return res;
        }

        #endregion

        #region IEnumerable<KeyValuePair<object,object>> Members

        [Pure]
        public IEnumerator<KeyValuePair<object, object>> GetEnumerator() {
            foreach (object key in Keys) {
                yield return new KeyValuePair<object, object>(key, this[SymbolTable.StringToId((string)key)]);
            }
        }

        #endregion

        #region IEnumerable Members

        [Pure]
        IEnumerator IEnumerable.GetEnumerator() {
            foreach (object key in Keys) {
                yield return new KeyValuePair<object, object>(key, this[SymbolTable.StringToId((string)key)]);
            }
        }

        #endregion

        public IList<Assembly> PackageAssemblies {
            get {
                LoadNamespaces();

                return _packageAssemblies;
            }
        }

        protected virtual void LoadNamespaces() {
            if (_topPackage != null) {
                _topPackage.LoadNamespaces();
            }
        }

        protected void SetTopPackage(TopNamespaceTracker pkg) {
            Assert.NotNull(pkg);
            _topPackage = pkg;
        }

        /// <summary>
        /// This stores all the public non-nested type names in a single namespace and from a single assembly.
        /// This allows inspection of the namespace without eagerly loading all the types. Eagerly loading
        /// types slows down startup, increases working set, and is semantically incorrect as it can trigger
        /// TypeLoadExceptions sooner than required.
        /// </summary>
        internal class TypeNames {
            List<string> _simpleTypeNames = new List<string>();
            Dictionary<string, List<string>> _genericTypeNames = new Dictionary<string, List<string>>();

            Assembly _assembly;
            string _fullNamespace;

            internal TypeNames(Assembly assembly, string fullNamespace) {
                _assembly = assembly;
                _fullNamespace = fullNamespace;
            }

            internal bool Contains(string normalizedTypeName) {
                Debug.Assert(normalizedTypeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
                Debug.Assert(ReflectionUtils.GetNormalizedTypeName(normalizedTypeName) == normalizedTypeName);

                return _simpleTypeNames.Contains(normalizedTypeName) || _genericTypeNames.ContainsKey(normalizedTypeName);
            }

            internal MemberTracker UpdateTypeEntity(TypeTracker existingTypeEntity, string normalizedTypeName) {
                Debug.Assert(normalizedTypeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
                Debug.Assert(ReflectionUtils.GetNormalizedTypeName(normalizedTypeName) == normalizedTypeName);

                // Look for a non-generic type
                if (_simpleTypeNames.Contains(normalizedTypeName)) {
                    Type newType = LoadType(_assembly, GetFullChildName(normalizedTypeName));
                    if (newType != null) {
                        existingTypeEntity = TypeGroup.UpdateTypeEntity(existingTypeEntity, ReflectionCache.GetTypeTracker(newType));
                    }
                }

                // Look for generic types
                if (_genericTypeNames.ContainsKey(normalizedTypeName)) {
                    List<string> actualNames = _genericTypeNames[normalizedTypeName];
                    foreach (string actualName in actualNames) {
                        Type newType = LoadType(_assembly, GetFullChildName(actualName));
                        if (newType != null) {
                            existingTypeEntity = TypeGroup.UpdateTypeEntity(existingTypeEntity, ReflectionCache.GetTypeTracker(newType));
                        }
                    }
                }

                return existingTypeEntity;
            }

            internal void AddTypeName(string typeName) {
                Debug.Assert(typeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name

                string normalizedName = ReflectionUtils.GetNormalizedTypeName(typeName);
                if (normalizedName == typeName) {
                    _simpleTypeNames.Add(typeName);
                } else {
                    List<string> actualNames;
                    if (_genericTypeNames.ContainsKey(normalizedName)) {
                        actualNames = _genericTypeNames[normalizedName];
                    } else {
                        actualNames = new List<string>();
                        _genericTypeNames[normalizedName] = actualNames;
                    }
                    actualNames.Add(typeName);
                }
            }

            string GetFullChildName(string childName) {
                Debug.Assert(childName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
                if (_fullNamespace == null) {
                    return childName;
                }

                return _fullNamespace + Type.Delimiter + childName;
            }

            internal ICollection<string> GetNormalizedTypeNames() {
                List<string> normalizedTypeNames = new List<string>();

                normalizedTypeNames.AddRange(_simpleTypeNames);
                normalizedTypeNames.AddRange(_genericTypeNames.Keys);

                return normalizedTypeNames;
            }
        }

        public int Id {
            get {
                return _id;
            }
        }

        #region IMembersList Members

        public IList<string> GetMemberNames() {
            LoadNamespaces();

            lock (_topPackage.HierarchyLock) {

                List<string> res = new List<string>();
                AddKeys(res);
                res.Sort();
               return res;
            }
        }

        #endregion

        public override TrackerTypes MemberType {
            get { return TrackerTypes.Namespace; }
        }

        public override Type DeclaringType {
            get { return null; }
        }

        private void UpdateId() {
            _id = Interlocked.Increment(ref _masterId);
        }

        protected void UpdateSubtreeIds() {
            // lock is held when this is called
            UpdateId();

            foreach (KeyValuePair<string, MemberTracker> kvp in _dict) {
                NamespaceTracker ns = kvp.Value as NamespaceTracker;
                if (ns != null) {
                    ns.UpdateSubtreeIds();
                }
            }
        }
    }
}
