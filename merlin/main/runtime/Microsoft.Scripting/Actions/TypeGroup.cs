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
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using System.Text;
using Microsoft.Contracts;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// A TypeCollision is used when we have a collsion between
    /// two types with the same name.  Currently this is only possible w/ generic
    /// methods that should logically have arity as a portion of their name. For eg:
    ///      System.EventHandler and System.EventHandler[T]
    ///      System.Nullable and System.Nullable[T]
    ///      System.IComparable and System.IComparable[T]
    /// 
    /// The TypeCollision provides an indexer but also is a real type.  When used
    /// as a real type it is the non-generic form of the type.
    /// 
    /// The indexer allows the user to disambiguate between the generic and
    /// non-generic versions.  Therefore users must always provide additional
    /// information to get the generic version.
    /// </summary>
    public sealed class TypeGroup : TypeTracker {
        private Dictionary<int, Type> _typesByArity;

        private TypeGroup(Type t1, Type t2) {
            Debug.Assert(ReflectionUtils.GetNormalizedTypeName(t1) == ReflectionUtils.GetNormalizedTypeName(t2));
            _typesByArity = new Dictionary<int, Type>();

            Debug.Assert(GetGenericArity(t1) != GetGenericArity(t2));
            _typesByArity[GetGenericArity(t1)] = t1;
            _typesByArity[GetGenericArity(t2)] = t2;
        }

        private TypeGroup(Type t1, Dictionary<int, Type> existingTypes) {
            _typesByArity = existingTypes;
            _typesByArity[GetGenericArity(t1)] = t1;
        }

        [Confined]
        public override string ToString() {
            StringBuilder repr = new StringBuilder(base.ToString());
            repr.Append(":" + NormalizedName + "(");

            bool pastFirstType = false;
            foreach (Type type in Types) {
                if (pastFirstType) {
                    repr.Append(", ");
                }
                repr.Append(type.Name);
                pastFirstType = true;
            }
            repr.Append(")");

            return repr.ToString();
        }

        public TypeTracker GetTypeForArity(int arity) {
            Type typeWithMatchingArity;
            if (!_typesByArity.TryGetValue(arity, out typeWithMatchingArity)) {
                return null;
            }
            return ReflectionCache.GetTypeTracker(typeWithMatchingArity);
        }

        /// <param name="existingTypeEntity">The merged list so far. Could be null</param>
        /// <param name="newType">The new type(s) to add to the merged list</param>
        /// <returns>The merged list.  Could be a TypeTracker or TypeGroup</returns>
        public static TypeTracker UpdateTypeEntity(
            TypeTracker existingTypeEntity,
            TypeTracker newType) {

            Debug.Assert(newType != null);
            Debug.Assert(existingTypeEntity == null || (existingTypeEntity is NestedTypeTracker) || (existingTypeEntity is TypeGroup));

            if (existingTypeEntity == null) {
                return newType;
            }

            NestedTypeTracker existingType = existingTypeEntity as NestedTypeTracker;
            TypeGroup existingTypeCollision = existingTypeEntity as TypeGroup;
#if DEBUG
            string existingEntityNormalizedName = (existingType != null) ? ReflectionUtils.GetNormalizedTypeName(existingType.Type)
                                                                         : existingTypeCollision.NormalizedName;
            string newEntityNormalizedName = ReflectionUtils.GetNormalizedTypeName(newType.Type);
            Debug.Assert(existingEntityNormalizedName == newEntityNormalizedName);
#endif

            if (existingType != null) {
                if (GetGenericArity(existingType.Type) == GetGenericArity(newType.Type)) {
                    return newType;
                }

                return new TypeGroup(existingType.Type, newType.Type);
            }

            // copy the dictionary and return a new collision
            Dictionary<int, Type> copy = new Dictionary<int, Type>(existingTypeCollision._typesByArity);
            return new TypeGroup(newType.Type, copy);
        }

        /// <summary> Gets the arity of generic parameters</summary>
        private static int GetGenericArity(Type type) {
            if (!type.IsGenericType) {
                return 0;
            }

            Debug.Assert(type.IsGenericTypeDefinition);
            return type.GetGenericArguments().Length;
        }

        /// <summary>
        /// This will throw an exception if all the colliding types are generic
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")] // TODO: fix
        public Type NonGenericType {
            get {
                Type nonGenericType;
                if (TryGetNonGenericType(out nonGenericType)) {
                    return nonGenericType;
                }

                throw Error.NonGenericWithGenericGroup(NormalizedName);
            }
        }

        public bool TryGetNonGenericType(out Type nonGenericType) {
            return _typesByArity.TryGetValue(0, out nonGenericType);
        }

        private Type SampleType {
            get {
                IEnumerator<Type> e = Types.GetEnumerator();
                e.MoveNext();
                return e.Current;
            }
        }

        public IEnumerable<Type> Types {
            get {
                return _typesByArity.Values;
            }
        }

        public string NormalizedName {
            get {
                return ReflectionUtils.GetNormalizedTypeName(SampleType);
            }
        }


        #region MemberTracker overrides

        public override TrackerTypes MemberType {
            get {
                return TrackerTypes.TypeGroup;
            }
        }

        /// <summary>
        /// This returns the DeclaringType of all the types in the TypeGroup
        /// </summary>
        public override Type DeclaringType {
            get {
                return AnyType().DeclaringType;
            }
        }

        /// <summary>
        /// This returns the base name of the TypeGroup (the name shared by all types minus arity)
        /// </summary>
        public override string Name {
            get {
                string name = AnyType().Name;

                if (name.IndexOf(ReflectionUtils.GenericArityDelimiter) != -1) {
                    return name.Substring(0, name.LastIndexOf(ReflectionUtils.GenericArityDelimiter));
                }
                return name;
            }
        }

        /// <summary>
        /// This will return the result only for the non-generic type if one exists, and will throw 
        /// an exception if all types in the TypeGroup are generic
        /// </summary>
        public override Type Type {
            get { return NonGenericType; }
        }

        public override bool IsGenericType {
            get { return _typesByArity.Count > 0; }
        }

        /// <summary>
        /// This will return the result only for the non-generic type if one exists, and will throw 
        /// an exception if all types in the TypeGroup are generic
        /// </summary>
        public override bool IsPublic {
            get { return NonGenericType.IsPublic; }
        }

        #endregion

        private Type AnyType() {
            foreach (KeyValuePair<int, Type> kvp in _typesByArity) {
                return kvp.Value;
            }
            throw new InvalidOperationException();
        }
    }
}
