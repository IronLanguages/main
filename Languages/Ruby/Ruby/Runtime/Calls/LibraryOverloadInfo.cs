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
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace IronRuby.Runtime.Calls {
    internal sealed class LibraryOverloadInfo : LibraryOverload {
        internal LibraryOverloadInfo(Delegate/*!*/ overloadDelegate, short defaultProtocolAttrs, short notNullAttrs)
            : base(overloadDelegate, defaultProtocolAttrs, notNullAttrs) {
        }

        public override bool IsVariadic {
            get { return false; }
        }
    }

    internal sealed class LibraryVariadicOverloadInfo : LibraryOverload {
        internal LibraryVariadicOverloadInfo(Delegate/*!*/ overloadDelegate, short defaultProtocolAttrs, short notNullAttrs)
            : base(overloadDelegate, defaultProtocolAttrs, notNullAttrs) {
        }

        public override bool IsVariadic {
            get { return true; }
        }
    }

    public abstract class LibraryOverload : OverloadInfo {
        private readonly Delegate/*!*/ _delegate;
        private ReadOnlyCollection<ParameterInfo> _parameters;

        private short _defaultProtocolAttrs;
        private short _notNullAttrs;

        protected LibraryOverload(Delegate/*!*/ overloadDelegate, short defaultProtocolAttrs, short notNullAttrs) {
            Assert.NotNull(overloadDelegate);
            _delegate = overloadDelegate;
            _defaultProtocolAttrs = defaultProtocolAttrs;
            _notNullAttrs = notNullAttrs;
        }

        // [1:is-variadic][1:unused][15:default-protocol][15:not-null]
        internal static LibraryOverload/*!*/ Create(Delegate/*!*/ overloadDelegate, uint customAttributes) {
            return Create(
                overloadDelegate,
                (customAttributes & 0x80000000) != 0,
                (short)((customAttributes >> 15) & 0x7fff),
                (short)(customAttributes & 0x7fff)
            );
        }

        internal static uint EncodeCustomAttributes(MethodInfo/*!*/ method) {
            var ps = method.GetParameters();
            if (ps.Length > 15) {
                throw new NotSupportedException();
            }
            uint defaultProtocol = 0;
            uint notNull = 0;
            
            for (int i = 0; i < ps.Length; i++) {
                defaultProtocol |= ps[i].IsDefined(typeof(DefaultProtocolAttribute), false) ? (1U << i) : 0U;
                notNull |= ps[i].IsDefined(typeof(NotNullAttribute), false) ? (1U << i) : 0U;
            }

            int last = ps.Length - 1;
            bool isVariadic = last >= 0 && ps[last].IsDefined(typeof(ParamArrayAttribute), false);
            if (isVariadic) {
                notNull |= ps[last].IsDefined(typeof(NotNullItemsAttribute), false) ? (1U << last) : 0U;
            }

            return (isVariadic ? 0x80000000 : 0) | (defaultProtocol << 15) | notNull;
        }

        public static LibraryOverload/*!*/ Create(Delegate/*!*/ overloadDelegate, bool isVariadic, short defaultProtocolAttrs, short notNullAttrs) {
            if (isVariadic) {
                return new LibraryVariadicOverloadInfo(overloadDelegate, defaultProtocolAttrs, notNullAttrs);
            } else {
                return new LibraryOverloadInfo(overloadDelegate, defaultProtocolAttrs, notNullAttrs);
            }
        }

        internal static LibraryOverload/*!*/ Reflect(Delegate/*!*/ overloadDelegate) {
            return Create(overloadDelegate, EncodeCustomAttributes(overloadDelegate.Method));
        }

        public override MethodBase ReflectionInfo {
            get { return _delegate.Method; }
        }

        public override Type ReturnType {
            get { return _delegate.Method.ReturnType; }
        }

        public override ParameterInfo/*!*/ ReturnParameter {
            get { return _delegate.Method.ReturnParameter; }
        }

        public override Type/*!*/ DeclaringType {
            get { return _delegate.Method.DeclaringType; }
        }

        // name is irrelevant for library methods
        public override string/*!*/ Name {
            get { return String.Empty; }
        }

        public override IList<ParameterInfo>/*!*/ Parameters {
            get { return _parameters ?? (_parameters = new ReadOnlyCollection<ParameterInfo>(_delegate.Method.GetParameters())); }
        }

        public override bool IsParamArray(int parameterIndex) {
            return IsVariadic && parameterIndex == Parameters.Count - 1;
        }

        public override bool IsParamDictionary(int parameterIndex) {
            return false;
        }

        public override bool ProhibitsNull(int parameterIndex) {
            return (_notNullAttrs & (1 << parameterIndex)) != 0;
        }

        public override bool ProhibitsNullItems(int parameterIndex) {
            return IsParamArray(parameterIndex) && ProhibitsNull(parameterIndex);
        }

        public bool HasDefaultProtocol(int parameterIndex) {
            return (_defaultProtocolAttrs & (1 << parameterIndex)) != 0;
        }

        public override MethodAttributes Attributes {
            get { return MethodAttributes.Public | MethodAttributes.Static; }
        }

        public override bool IsConstructor {
            get { return false; }
        }

        public override bool IsExtension {
            get { return false; }
        }

        public override bool IsGenericMethodDefinition {
            get { return false; }
        }

        public override bool IsGenericMethod {
            get { return false; }
        }

        public override bool ContainsGenericParameters {
            get { return false; }
        }

        public override IList<Type>/*!*/ GenericArguments {
            get { return Type.EmptyTypes; }
        }

        public override OverloadInfo/*!*/ MakeGenericMethod(Type[] genericArguments) {
            throw new InvalidOperationException();
        }
    }
}
