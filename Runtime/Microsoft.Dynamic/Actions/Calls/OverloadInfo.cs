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

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using System.Diagnostics;

namespace Microsoft.Scripting.Actions.Calls {
    /// <summary>
    /// Defines a method overload abstraction for the purpose of overload resolution. 
    /// It provides the overload resolver the metadata it needs to perform the resolution.
    /// </summary>
    /// <remarks>
    /// WARNING: This is a temporary API that will undergo breaking changes in future versions.
    /// </remarks>
    [DebuggerDisplay("{(object)ReflectionInfo ?? Name}")]
    public abstract class OverloadInfo {
        public abstract string Name { get; }
        public abstract IList<ParameterInfo> Parameters { get; }

        public virtual int ParameterCount {
            get { return Parameters.Count; }
        }

        /// <summary>
        /// Null for constructors.
        /// </summary>
        public abstract ParameterInfo ReturnParameter { get; }

        public virtual bool ProhibitsNull(int parameterIndex) {
            return Parameters[parameterIndex].ProhibitsNull();
        }

        public virtual bool ProhibitsNullItems(int parameterIndex) {
            return Parameters[parameterIndex].ProhibitsNullItems();
        }

        public virtual bool IsParamArray(int parameterIndex) {
            return Parameters[parameterIndex].IsParamArray();
        }

        public virtual bool IsParamDictionary(int parameterIndex) {
            return Parameters[parameterIndex].IsParamDictionary();
        }

        public abstract Type DeclaringType { get; }
        public abstract Type ReturnType { get; }
        public abstract MethodAttributes Attributes { get; }
        public abstract bool IsConstructor { get; }
        public abstract bool IsExtension { get; }

        /// <summary>
        /// The method arity can vary, i.e. the method has params array or params dict parameters.
        /// </summary>
        public abstract bool IsVariadic { get; }
        
        public abstract bool IsGenericMethodDefinition { get; }
        public abstract bool IsGenericMethod { get; }
        public abstract bool ContainsGenericParameters { get; }
        public abstract IList<Type> GenericArguments { get; }
        public abstract OverloadInfo MakeGenericMethod(Type[] genericArguments);

        public virtual CallingConventions CallingConvention {
            get { return CallingConventions.Standard; }
        }

        public virtual MethodBase ReflectionInfo {
            get { return null; }
        }

        // TODO: remove
        public virtual bool IsInstanceFactory { 
            get { return IsConstructor; } 
        }

        public bool IsPrivate {
            get { return ((Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private); }
        }

        public bool IsPublic {
            get { return ((Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public); }
        }

        public bool IsAssembly {
            get { return ((Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly); }
        }

        public bool IsFamily {
            get { return ((Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family); }
        }

        public bool IsFamilyOrAssembly {
            get { return ((Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem); }
        }

        public bool IsFamilyAndAssembly {
            get { return ((Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem); }
        }

        public bool IsProtected {
            get { return IsFamily || IsFamilyOrAssembly; }
        }

        public bool IsStatic {
            get { return IsConstructor || (Attributes & MethodAttributes.Static) != 0; }
        }

        public bool IsVirtual {
            get { return (Attributes & MethodAttributes.Virtual) != 0; }
        }

        public bool IsSpecialName {
            get { return (Attributes & MethodAttributes.SpecialName) != 0; }
        }

        public bool IsFinal {
            get { return (Attributes & MethodAttributes.Final) != 0; }
        }
    }

    /// <summary>
    /// Represents a method overload that is bound to a <see cref="T:System.Reflection.MethodBase"/>.
    /// </summary>
    /// <remarks>
    /// Not thread safe.
    /// WARNING: This is a temporary API that will undergo breaking changes in future versions. 
    /// </remarks>
    public class ReflectionOverloadInfo : OverloadInfo {
        [Flags]
        private enum _Flags {
            None = 0,
            IsVariadic = 1,
            KnownVariadic = 2,
            ContainsGenericParameters = 4,
            KnownContainsGenericParameters = 8,
            IsExtension = 16,
            KnownExtension = 32,
        }

        private readonly MethodBase _method;
        private ReadOnlyCollection<ParameterInfo> _parameters; // lazy
        private ReadOnlyCollection<Type> _genericArguments; // lazy
        private _Flags _flags; // lazy

        public ReflectionOverloadInfo(MethodBase method) {
            _method = method;
        }

        public override MethodBase ReflectionInfo {
            get { return _method; }
        }

        public override string Name {
            get { return _method.Name; }
        }

        public override IList<ParameterInfo> Parameters {
            get { return _parameters ?? (_parameters = new ReadOnlyCollection<ParameterInfo>(_method.GetParameters())); }
        }

        public override ParameterInfo ReturnParameter {
            get {
                MethodInfo method = _method as MethodInfo;
                return method != null ? method.ReturnParameter : null;
            }
        }
        
        public override IList<Type> GenericArguments {
            get { return _genericArguments ?? (_genericArguments = new ReadOnlyCollection<Type>(_method.GetGenericArguments())); }
        }

        public override Type DeclaringType {
            get { return _method.DeclaringType; }
        }

        public override Type ReturnType {
            get { return _method.GetReturnType(); }
        }

        public override CallingConventions CallingConvention {
            get { return _method.CallingConvention; }
        }

        public override MethodAttributes Attributes {
            get { return _method.Attributes; }
        }

        public override bool IsInstanceFactory {
            get { return CompilerHelpers.IsConstructor(_method); }
        }

        public override bool IsConstructor {
            get { return _method.IsConstructor; }
        }

        public override bool IsExtension {
            get {
                if ((_flags & _Flags.KnownExtension) == 0) {
                    _flags |= _Flags.KnownExtension | (_method.IsExtension() ? _Flags.IsExtension : 0);
                }
                return (_flags & _Flags.IsExtension) != 0;
            }
        }

        public override bool IsVariadic {
            get { 
                if ((_flags & _Flags.KnownVariadic) == 0) {
                    _flags |= _Flags.KnownVariadic | (IsVariadicInternal() ? _Flags.IsVariadic : 0);
                }
                return (_flags & _Flags.IsVariadic) != 0;
            }
        }

        private bool IsVariadicInternal() {
            var ps = Parameters;
            for (int i = ps.Count - 1; i >= 0; i--) {
                if (ps[i].IsParamArray() || ps[i].IsParamDictionary()) {
                    return true;
                }
            }
            return false;
        }

        public override bool IsGenericMethod {
            get { return _method.IsGenericMethod; }
        }

        public override bool IsGenericMethodDefinition {
            get { return _method.IsGenericMethodDefinition; }
        }

        public override bool ContainsGenericParameters {
            get { 
                if ((_flags & _Flags.KnownContainsGenericParameters) == 0) {
                    _flags |= _Flags.KnownContainsGenericParameters | (_method.ContainsGenericParameters ? _Flags.ContainsGenericParameters : 0);
                }
                return (_flags & _Flags.ContainsGenericParameters) != 0;
            }
        }

        public override OverloadInfo MakeGenericMethod(Type[] genericArguments) {
            return new ReflectionOverloadInfo(((MethodInfo)_method).MakeGenericMethod(genericArguments));
        }

        public static OverloadInfo[] CreateArray(MemberInfo[] methods) {
            return ArrayUtils.ConvertAll(methods, (m) => new ReflectionOverloadInfo((MethodBase)m));
        }
    }
}
