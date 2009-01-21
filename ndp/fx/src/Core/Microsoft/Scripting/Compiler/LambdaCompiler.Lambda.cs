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

using System.Diagnostics;
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
            if (_method is DynamicMethod) {
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
        private void EmitDelegateConstruction(LambdaCompiler inner) {
            Type delegateType = inner._lambda.Type;
            DynamicMethod dynamicMethod = inner._method as DynamicMethod;
            if (dynamicMethod != null) {
                // dynamicMethod.CreateDelegate(delegateType, closure)
                _boundConstants.EmitConstant(this, dynamicMethod, typeof(DynamicMethod));
                _ilg.EmitType(delegateType);
                EmitClosureCreation(inner);
                _ilg.Emit(OpCodes.Callvirt, typeof(DynamicMethod).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object) }));
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
        /// 
        private void EmitDelegateConstruction(LambdaExpression lambda) {
            // 1. Create the new compiler
            LambdaCompiler impl;
            if (_method is DynamicMethod) {
                impl = new LambdaCompiler(_tree, lambda);
            } else {
                //The lambda must be a nested one, we generate a private method for it.
                MethodBuilder mb = _typeBuilder.DefineMethod(GetGeneratedName(lambda.Name), MethodAttributes.Private | MethodAttributes.Static);
                impl = new LambdaCompiler(_tree, lambda, mb, _emitDebugSymbols);
            }

            // 3. emit the lambda
            impl.EmitLambdaBody(_scope);

            // 4. emit the delegate creation in the outer lambda
            EmitDelegateConstruction(impl);
        }

        private static Type[] GetParameterTypes(LambdaExpression lambda) {
            return lambda.Parameters.Map(p => p.IsByRef ? p.Type.MakeByRefType() : p.Type);
        }

        private static string GetGeneratedName(string prefix) {
            return (prefix ?? "") + "$" + Interlocked.Increment(ref _Counter);
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
        }
    }
}
