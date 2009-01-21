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
using System.Reflection.Emit;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Compiler;

using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Ast = System.Linq.Expressions.Expression;

namespace IronRuby.Runtime.Calls {
    
    /// <summary>
    /// Performs method binding for calling CLR methods.
    /// Currently this is used for all builtin libary methods and interop calls to CLR methods
    /// </summary>
    public class RubyMethodGroupInfo : RubyMethodGroupBase {
        private MethodBase/*!*/[] _staticDispatchMethods;
        private bool? _hasVirtuals;

        // remove call site type object (CLR static methods don't accept self type):
        private readonly bool _isStatic;

        internal override SelfCallConvention CallConvention {
            get { return _isStatic ? SelfCallConvention.NoSelf : SelfCallConvention.SelfIsInstance; }
        }

        internal override bool HidesInheritedOverloads {
            get { return false; }
        }

        /// <summary>
        /// Creates a CLR method group.
        /// </summary>
        internal RubyMethodGroupInfo(MethodBase/*!*/[]/*!*/ methods, RubyModule/*!*/ declaringModule, bool isStatic)
            : base(methods, RubyMemberFlags.Public, declaringModule) {
            _isStatic = isStatic;
        }

        // copy ctor
        private RubyMethodGroupInfo(RubyMethodGroupInfo/*!*/ info, RubyMemberFlags flags, RubyModule/*!*/ module)
            : base(info.MethodBases, flags, module) {
            _isStatic = info._isStatic;
            _hasVirtuals = info._hasVirtuals;
            _staticDispatchMethods = info._staticDispatchMethods;
        }

        // copy ctor
        private RubyMethodGroupInfo(RubyMethodGroupInfo/*!*/ info, MethodBase/*!*/[] methods)
            : base(methods, info.Flags, info.DeclaringModule) {
            _isStatic = info._isStatic;
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyMethodGroupInfo(this, flags, module);
        }

        protected override RubyMemberInfo/*!*/ Copy(MethodBase/*!*/[]/*!*/ methods) {
            return new RubyMethodGroupInfo(this, methods);
        }
        
        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return ArrayUtils.MakeArray(MethodBases);
        }
        
        #region Static dispatch to virtual methods

        private bool HasVirtuals {
            get {
                if (!_hasVirtuals.HasValue) {
                    if (_isStatic) {
                        _hasVirtuals = false;
                    } else {
                        bool hasVirtuals = false;
                        foreach (MethodBase method in MethodBases) {
                            if (method.IsVirtual) {
                                hasVirtuals = true;
                                break;
                            }
                        }
                        _hasVirtuals = hasVirtuals;
                    }
                }
                return _hasVirtuals.Value;
            }
        }

        protected override MethodBase/*!*/[]/*!*/ GetStaticDispatchMethods(Type/*!*/ baseType, string/*!*/ name) {
            if (!HasVirtuals) {
                return MethodBases;
            }
            if (_staticDispatchMethods == null) {
                _staticDispatchMethods = new MethodBase[MethodBases.Length];
                for (int i = 0; i < MethodBases.Length; i++) {
                    MethodBase method = MethodBases[i];
                    _staticDispatchMethods[i] = method;

                    MethodInfo methodInfo = method as MethodInfo;
                    if (methodInfo != null && methodInfo.IsVirtual) {
                        _staticDispatchMethods[i] = WrapMethod(methodInfo, baseType);
                    }
                }
            }
            return _staticDispatchMethods;
        }

        public static DynamicMethod/*!*/ WrapMethod(MethodInfo/*!*/ info, Type/*!*/ associatedType) {
            var originalParams = info.GetParameters();
            var newParams = new Type[originalParams.Length + 1];
            string name = "";
            newParams[0] = info.DeclaringType;
            for (int i = 0; i < originalParams.Length; i++) {
                newParams[i + 1] = originalParams[i].ParameterType;
            }
            DynamicMethod result = new DynamicMethod(name, info.ReturnType, newParams, associatedType);
            ILGenerator ilg = result.GetILGenerator();
            for (int i = 0; i < newParams.Length; i++) {
                ilg.Emit(OpCodes.Ldarg, i);
            }
            ilg.EmitCall(OpCodes.Call, info, null);
            ilg.Emit(OpCodes.Ret);
            return result;
        }

        #endregion
    }
}

