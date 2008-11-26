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

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Dynamic.Utils;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Linq.Expressions.Compiler {

    /// <summary>
    /// Dynamic Language Runtime Compiler.
    /// This part compiles lambdas.
    /// </summary>
    partial class LambdaCompiler {
        private static int _Counter;

        internal void EmitConstantArray<T>(T[] array) {
            // Emit as runtime constant if possible
            // if not, emit into IL
            if (_dynamicMethod) {
                EmitConstant(array, typeof(T[]));
            } else if(_typeBuilder != null) {
                // store into field in our type builder, we will initialize
                // the value only once.
                FieldBuilder fb = _typeBuilder.DefineField("constantArray" + typeof(T).Name.Replace('.', '_').Replace('+', '_') + Interlocked.Increment(ref _Counter), typeof(T[]), FieldAttributes.Static | FieldAttributes.Private);
                Label l = _ilg.DefineLabel();
                _ilg.Emit(OpCodes.Ldsfld, fb);
                _ilg.Emit(OpCodes.Ldnull);
                _ilg.Emit(OpCodes.Bne_Un, l);
                _ilg.EmitArray(array);
                _ilg.Emit(OpCodes.Stsfld, fb);
                _ilg.MarkLabel(l);
                _ilg.Emit(OpCodes.Ldsfld, fb);
            } else { 
                _ilg.EmitArray(array);
            }
        }

        private void EmitClosureCreation(LambdaCompiler inner) {
            bool closure = inner._scope.NeedsClosure;
            bool boundConstants = inner._boundConstants.Count > 0;

            if (!closure && !boundConstants) {
                _ilg.EmitNull();
                return;
            }

            // new Closure(constantPool, currentHoistedLocals)
            if (boundConstants) {
                _boundConstants.EmitConstant(this, inner._boundConstants.ToArray(), typeof(object[]));
            } else {
                _ilg.EmitNull();
            }
            if (closure) {
                _scope.EmitGet(_scope.NearestHoistedLocals.SelfVariable);
            } else {
                _ilg.EmitNull();
            }
            _ilg.EmitNew(typeof(Closure).GetConstructor(new Type[] { typeof(object[]), typeof(object[]) }));
        }

        /// <summary>
        /// Emits code which creates new instance of the delegateType delegate.
        /// 
        /// Since the delegate is getting closed over the "Closure" argument, this
        /// cannot be used with virtual/instance methods (inner must be static method)
        /// </summary>
        private void EmitDelegateConstruction(LambdaCompiler inner, Type delegateType) {
            DynamicMethod dynamicMethod = inner._method as DynamicMethod;
            if (dynamicMethod != null) {
                // dynamicMethod.CreateDelegate(delegateType, closure)
                _boundConstants.EmitConstant(this, dynamicMethod, typeof(DynamicMethod));
                _ilg.EmitType(delegateType);
                EmitClosureCreation(inner);
                _ilg.EmitCall(typeof(DynamicMethod).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object) }));
                _ilg.Emit(OpCodes.Castclass, delegateType);
            } else {
                // new DelegateType(closure)
                EmitClosureCreation(inner);
                _ilg.Emit(OpCodes.Ldftn, (MethodInfo)inner._method);
                _ilg.Emit(OpCodes.Newobj, (ConstructorInfo)(delegateType.GetMember(".ctor")[0]));
            }
        }

        /// <summary>
        /// Emits a delegate to the method generated for the LambdaExpression.
        /// May end up creating a wrapper to match the requested delegate type.
        /// </summary>
        /// <param name="lambda">Lambda for which to generate a delegate</param>
        /// <param name="delegateType">Type of the delegate.</param>
        private void EmitDelegateConstruction(LambdaExpression lambda, Type delegateType) {
            // 1. create the signature
            List<Type> paramTypes;
            List<string> paramNames;
            string implName;
            Type returnType;
            ComputeSignature(lambda, out paramTypes, out paramNames, out implName, out returnType);

            // 2. create the new compiler
            LambdaCompiler impl;
            if (_dynamicMethod) {
                impl = CreateDynamicCompiler(_tree, lambda, implName, returnType, paramTypes, paramNames, _emitDebugSymbols, _method is DynamicMethod);
            } else {
                impl = CreateStaticCompiler(_tree, lambda, _typeBuilder, implName, TypeUtils.PublicStatic, returnType, paramTypes, paramNames, _dynamicMethod, _emitDebugSymbols);
            }

            // 3. emit the lambda
            impl.EmitLambdaBody(_scope);

            // 4. emit the delegate creation in the outer lambda
            EmitDelegateConstruction(impl, delegateType);
        }

        /// <summary>
        /// Creates the signature for the lambda as list of types and list of names separately
        /// </summary>
        private static void ComputeSignature(
            LambdaExpression lambda,
            out List<Type> paramTypes,
            out List<string> paramNames,
            out string implName,
            out Type returnType) {

            paramTypes = new List<Type>();
            paramNames = new List<string>();

            foreach (ParameterExpression p in lambda.Parameters) {
                paramTypes.Add(p.IsByRef ? p.Type.MakeByRefType() : p.Type);
                paramNames.Add(p.Name);
            }

            implName = GetGeneratedName(lambda.Name);
            returnType = lambda.ReturnType;
        }

        private static string GetGeneratedName(string prefix) {
            return prefix + "$" + Interlocked.Increment(ref _Counter);
        }

        private void EmitLambdaBody(CompilerScope parent) {
            _scope.Enter(this, parent);

            Type returnType = _method.GetReturnType();
            if (returnType == typeof(void)) {
                EmitExpressionAsVoid(_lambda.Body);
            } else {
                Debug.Assert(_lambda.Body.Type != typeof(void));
                EmitExpression(_lambda.Body);
            }
            //must be the last instruction in the body
            _ilg.Emit(OpCodes.Ret);
            _scope.Exit();

            // Validate labels
            Debug.Assert(_labelBlock.Parent == null && _labelBlock.Kind == LabelBlockKind.Block);
            foreach (LabelInfo label in _labelInfo.Values) {
                label.ValidateFinish();
            }

            if (_dynamicMethod) {
                CreateDelegateMethodInfo();
            }
        }
    }
}
