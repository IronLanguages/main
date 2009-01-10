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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Dynamic.Utils;

namespace System.Linq.Expressions.Compiler {
    partial class LambdaCompiler {
        private enum ExpressionStart {
            None = 0,
            LabelBlock = 1
        }

        /// <summary>
        /// Generates code for this expression in a value position.
        /// This method will leave the value of the expression
        /// on the top of the stack typed as Type.
        /// </summary>
        internal void EmitExpression(Expression node) {
            EmitExpression(node, true);
        }

        /// <summary>
        /// Emits an expression and discards the result.  For some nodes this emits
        /// more optimial code then EmitExpression/Pop
        /// </summary>
        private void EmitExpressionAsVoid(Expression node) {
            Debug.Assert(node != null);

            ExpressionStart startEmitted = EmitExpressionStart(node);

            switch (node.NodeType) {
                case ExpressionType.Assign:
                    EmitAssign((BinaryExpression)node, EmitAs.Void);
                    break;
                case ExpressionType.Block:
                    Emit((BlockExpression)node, EmitAs.Void);
                    break;
                case ExpressionType.Throw:
                    EmitThrow((UnaryExpression)node, EmitAs.Void);
                    break;
                case ExpressionType.Constant:
                case ExpressionType.Default:
                case ExpressionType.Parameter:
                    // no-op
                    break;
                default:
                    EmitExpression(node, false);
                    if (node.Type != typeof(void)) {
                        _ilg.Emit(OpCodes.Pop);
                    }
                    break;
            }
            EmitExpressionEnd(startEmitted);
        }

        #region DebugMarkers

        private ExpressionStart EmitExpressionStart(Expression node) {
            if (TryPushLabelBlock(node)) {
                return ExpressionStart.LabelBlock;
            }
            return ExpressionStart.None;
        }

        private void EmitExpressionEnd(ExpressionStart emitted) {
            if (emitted == ExpressionStart.LabelBlock) {
                PopLabelBlock(_labelBlock.Kind);
            }
        }

        #endregion

        #region InvocationExpression

        //CONFORMING
        private void EmitInvocationExpression(Expression expr) {
            InvocationExpression node = (InvocationExpression)expr;

            // Note: If node.Expression is a lambda, ExpressionCompiler inlines
            // the lambda here as an optimization. We don't, for various
            // reasons:
            //
            // * It's not necessarily optimal for large statement trees (JIT
            //   does better with small methods)
            // * We support returning from anywhere,
            // * The frame wouldn't show up in the stack trace,
            // * Possibly other subtle semantic differences
            //
            expr = node.Expression;
            if (typeof(LambdaExpression).IsAssignableFrom(expr.Type)) {
                // if the invoke target is a lambda expression tree, first compile it into a delegate
                expr = Expression.Call(expr, expr.Type.GetMethod("Compile", new Type[0]));
            }
            expr = Expression.Call(expr, expr.Type.GetMethod("Invoke"), node.Arguments);

            EmitExpression(expr);
        }

        #endregion

        #region IndexExpression

        private void EmitIndexExpression(Expression expr) {
            var node = (IndexExpression)expr;

            // Emit instance, if calling an instance method
            Type objectType = null;
            if (node.Object != null) {
                EmitInstance(node.Object, objectType = node.Object.Type);
            }

            // Emit indexes. We don't allow byref args, so no need to worry
            // about writebacks or EmitAddress
            foreach (var arg in node.Arguments) {
                EmitExpression(arg);
            }

            EmitGetIndexCall(node, objectType);
        }

        private void EmitIndexAssignment(BinaryExpression node, EmitAs emitAs) {
            var index = (IndexExpression)node.Left;

            // Emit instance, if calling an instance method
            Type objectType = null;
            if (index.Object != null) {
                EmitInstance(index.Object, objectType = index.Object.Type);
            }

            // Emit indexes. We don't allow byref args, so no need to worry
            // about writebacks or EmitAddress
            foreach (var arg in index.Arguments) {
                EmitExpression(arg);
            }

            // Emit value
            EmitExpression(node.Right);

            // Save the expression value, if needed
            LocalBuilder temp = null;
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, temp = GetLocal(node.Type));
            }

            EmitSetIndexCall(index, objectType);

            // Restore the value
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Ldloc, temp);
                FreeLocal(temp);
            }
        }

        private void EmitArrayIndexAssignment(BinaryExpression node, EmitAs emitAs) {
            Debug.Assert(node.Left.NodeType == ExpressionType.ArrayIndex);
            var arrayIndex = (BinaryExpression)node.Left;

            // Emit array object
            EmitInstance(arrayIndex.Left, arrayIndex.Left.Type);

            // Emit index
            EmitExpression(arrayIndex.Right);

            // Emit value
            EmitExpression(node.Right);

            // Save the expression value, if needed
            LocalBuilder temp = null;
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, temp = GetLocal(node.Type));
            }

            _ilg.EmitStoreElement(arrayIndex.Type);

            // Restore the value
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Ldloc, temp);
                FreeLocal(temp);
            }
        }

        private void EmitGetIndexCall(IndexExpression node, Type objectType) {
            if (node.Indexer != null) {
                // For indexed properties, just call the getter
                var method = node.Indexer.GetGetMethod(true);
                EmitCall(objectType, method);
            } else if (node.Arguments.Count != 1) {
                // Multidimensional arrays, call get
                _ilg.Emit(OpCodes.Call, node.Object.Type.GetMethod("Get", BindingFlags.Public | BindingFlags.Instance));
            } else {
                // For one dimensional arrays, emit load
                _ilg.EmitLoadElement(node.Type);
            }
        }

        private void EmitSetIndexCall(IndexExpression node, Type objectType) {
            if (node.Indexer != null) {
                // For indexed properties, just call the setter
                var method = node.Indexer.GetSetMethod(true);
                EmitCall(objectType, method);
            } else if (node.Arguments.Count != 1) {
                // Multidimensional arrays, call set
                _ilg.Emit(OpCodes.Call, node.Object.Type.GetMethod("Set", BindingFlags.Public | BindingFlags.Instance));
            } else {
                // For one dimensional arrays, emit store
                _ilg.EmitStoreElement(node.Type);
            }
        }

        #endregion

        #region MethodCallExpression

        //CONFORMING
        private void EmitMethodCallExpression(Expression expr) {
            MethodCallExpression node = (MethodCallExpression)expr;

            EmitMethodCall(node.Object, node.Method, node);
        }

        //CONFORMING
        private void EmitMethodCall(Expression obj, MethodInfo method, IArgumentProvider methodCallExpr) {
            // Emit instance, if calling an instance method
            Type objectType = null;
            if (!method.IsStatic) {
                EmitInstance(obj, objectType = obj.Type);
            }

            EmitMethodCall(method, methodCallExpr, objectType);
        }

        //CONFORMING
        // assumes 'object' of non-static call is already on stack
        private void EmitMethodCall(MethodInfo mi, IArgumentProvider args, Type objectType) {

            // Emit arguments
            List<WriteBack> wb = EmitArguments(mi, args);

            // Emit the actual call
            OpCode callOp = UseVirtual(mi) ? OpCodes.Callvirt : OpCodes.Call;
            if (callOp == OpCodes.Callvirt && objectType.IsValueType) {
                // This automatically boxes value types if necessary.
                _ilg.Emit(OpCodes.Constrained, objectType);
            }
            if (mi.CallingConvention == CallingConventions.VarArgs) {
                _ilg.EmitCall(callOp, mi, args.Map(a => a.Type));
            } else {
                _ilg.Emit(callOp, mi);
            }

            // Emit writebacks for properties passed as "ref" arguments
            EmitWriteBack(wb);
        }

        private void EmitCall(Type objectType, MethodInfo method) {
            if (method.CallingConvention == CallingConventions.VarArgs) {
                throw Error.UnexpectedVarArgsCall(method.FormatSignature());
            }

            OpCode callOp = UseVirtual(method) ? OpCodes.Callvirt : OpCodes.Call;
            if (callOp == OpCodes.Callvirt && objectType.IsValueType) {
                _ilg.Emit(OpCodes.Constrained, objectType);
            }
            _ilg.Emit(callOp, method);
        }

        //CONFORMING
        private static bool UseVirtual(MethodInfo mi) {
            // There are two factors: is the method static, virtual or non-virtual instance?
            // And is the object ref or value?
            // The cases are:
            //
            // static, ref:     call
            // static, value:   call
            // virtual, ref:    callvirt
            // virtual, value:  call -- eg, double.ToString must be a non-virtual call to be verifiable.
            // instance, ref:   callvirt -- this looks wrong, but is verifiable and gives us a free null check.
            // instance, value: call
            //
            // We never need to generate a nonvirtual call to a virtual method on a reference type because
            // expression trees do not support "base.Foo()" style calling.
            // 
            // We could do an optimization here for the case where we know that the object is a non-null
            // reference type and the method is a non-virtual instance method.  For example, if we had
            // (new Foo()).Bar() for instance method Bar we don't need the null check so we could do a
            // call rather than a callvirt.  However that seems like it would not be a very big win for
            // most dynamically generated code scenarios, so let's not do that for now.

            if (mi.IsStatic) {
                return false;
            }
            if (mi.DeclaringType.IsValueType) {
                return false;
            }
            return true;
        }


        //CONFORMING
        private List<WriteBack> EmitArguments(MethodBase method, IArgumentProvider args) {
            ParameterInfo[] pis = method.GetParametersCached();
            Debug.Assert(args.ArgumentCount == pis.Length);

            var writeBacks = new List<WriteBack>();
            for (int i = 0, n = pis.Length; i < n; i++) {
                ParameterInfo parameter = pis[i];
                Expression argument = args.GetArgument(i);
                Type type = parameter.ParameterType;

                if (type.IsByRef) {
                    type = type.GetElementType();

                    WriteBack wb = EmitAddressWriteBack(argument, type);
                    if (wb != null) {
                        writeBacks.Add(wb);
                    }
                } else {
                    EmitExpression(argument);
                }
            }
            return writeBacks;
        }

        //CONFORMING
        private static void EmitWriteBack(IList<WriteBack> writeBacks) {
            foreach (WriteBack wb in writeBacks) {
                wb();
            }
        }

        #endregion

        //CONFORMING
        private void EmitConstantExpression(Expression expr) {
            ConstantExpression node = (ConstantExpression)expr;

            EmitConstant(node.Value, node.Type);
        }

        //CONFORMING
        private void EmitConstant(object value, Type type) {
            // Try to emit the constant directly into IL
            if (ILGen.CanEmitConstant(value, type)) {
                _ilg.EmitConstant(value, type);
                return;
            }

            Debug.Assert(_dynamicMethod); // constructor enforces this

            _boundConstants.EmitConstant(this, value, type);
        }

        private void EmitDynamicExpression(Expression expr) {
            var node = (DynamicExpression)expr;
            
            var site = CallSite.Create(node.DelegateType, node.Binder);
            Type siteType = site.GetType();

            var invoke = node.DelegateType.GetMethod("Invoke");

            var siteVar = Expression.Variable(siteType, null);
            _scope.AddLocal(this, siteVar);

            // site.Target.Invoke(site, args)
            EmitConstant(site, siteType);
            _ilg.Emit(OpCodes.Dup);
            _scope.EmitSet(siteVar);
            _ilg.Emit(OpCodes.Ldfld, siteType.GetField("Target"));

            List<WriteBack> wb = EmitArguments(invoke, new ArgumentPrepender(siteVar, node));
            _ilg.Emit(OpCodes.Callvirt, invoke);
            EmitWriteBack(wb);
        }

        //CONFORMING
        private void EmitNewExpression(Expression expr) {
            NewExpression node = (NewExpression)expr;

            if (node.Constructor != null) {
                List<WriteBack> wb = EmitArguments(node.Constructor, node);
                _ilg.Emit(OpCodes.Newobj, node.Constructor);
                EmitWriteBack(wb);
            } else {
                Debug.Assert(node.Arguments.Count == 0, "Node with arguments must have a constructor.");
                Debug.Assert(node.Type.IsValueType, "Only value type may have constructor not set.");
                LocalBuilder temp = GetLocal(node.Type);
                _ilg.Emit(OpCodes.Ldloca, temp);
                _ilg.Emit(OpCodes.Initobj, node.Type);
                _ilg.Emit(OpCodes.Ldloc, temp);
                FreeLocal(temp);
            }
        }

        //CONFORMING
        private void EmitTypeBinaryExpression(Expression expr) {
            TypeBinaryExpression node = (TypeBinaryExpression)expr;

            if (node.NodeType == ExpressionType.TypeEqual) {
                EmitExpression(node.ReduceTypeEqual());
                return;
            }

            Type type = node.Expression.Type;

            // Try to determine the result statically
            AnalyzeTypeIsResult result = ConstantCheck.AnalyzeTypeIs(node);

            if (result == AnalyzeTypeIsResult.KnownTrue ||
                result == AnalyzeTypeIsResult.KnownFalse) {
                // Result is known statically, so just emit the expression for
                // its side effects and return the result
                EmitExpressionAsVoid(node.Expression);
                _ilg.EmitBoolean(result == AnalyzeTypeIsResult.KnownTrue);
                return;
            }

            if (result == AnalyzeTypeIsResult.KnownAssignable) {
                // We know the type can be assigned, but still need to check
                // for null at runtime
                if (type.IsNullableType()) {
                    EmitAddress(node.Expression, type);
                    _ilg.EmitHasValue(type);
                    return;
                }

                Debug.Assert(!type.IsValueType);
                EmitExpression(node.Expression);
                _ilg.Emit(OpCodes.Ldnull);
                _ilg.Emit(OpCodes.Ceq);
                _ilg.Emit(OpCodes.Ldc_I4_0);
                _ilg.Emit(OpCodes.Ceq);
                return;
            }

            Debug.Assert(result == AnalyzeTypeIsResult.Unknown);

            // Emit a full runtime "isinst" check
            EmitExpression(node.Expression);
            if (type.IsValueType) {
                _ilg.Emit(OpCodes.Box, type);
            }
            _ilg.Emit(OpCodes.Isinst, node.TypeOperand);
            _ilg.Emit(OpCodes.Ldnull);
            _ilg.Emit(OpCodes.Cgt_Un);
        }

        private void EmitVariableAssignment(BinaryExpression node, EmitAs emitAs) {
            var variable = (ParameterExpression)node.Left;

            EmitExpression(node.Right);
            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Dup);
            }

            if (variable.IsByRef) {
                // Note: the stloc/ldloc pattern is a bit suboptimal, but it
                // saves us from having to spill stack when assigning to a
                // byref parameter. We already make this same tradeoff for
                // hoisted variables, see ElementStorage.EmitStore

                LocalBuilder value = GetLocal(variable.Type);
                _ilg.Emit(OpCodes.Stloc, value);
                _scope.EmitGet(variable);
                _ilg.Emit(OpCodes.Ldloc, value);
                FreeLocal(value);
                _ilg.EmitStoreValueIndirect(variable.Type);
            } else {
                _scope.EmitSet(variable);
            }
        }

        private void EmitAssignBinaryExpression(Expression expr) {
            EmitAssign((BinaryExpression)expr, EmitAs.Default);
        }

        private void EmitAssign(BinaryExpression node, EmitAs emitAs) {
            switch (node.Left.NodeType) {
                case ExpressionType.Index:
                    EmitIndexAssignment(node, emitAs);
                    return;
                case ExpressionType.MemberAccess:
                    EmitMemberAssignment(node, emitAs);
                    return;
                case ExpressionType.Parameter:
                    EmitVariableAssignment(node, emitAs);
                    return;
                case ExpressionType.ArrayIndex:
                    EmitArrayIndexAssignment(node, emitAs);
                    return;
                default:
                    throw Error.InvalidLvalue(node.Left.NodeType);
            }
        }

        private void EmitParameterExpression(Expression expr) {
            ParameterExpression node = (ParameterExpression)expr;
            _scope.EmitGet(node);
            if (node.IsByRef) {
                _ilg.EmitLoadValueIndirect(node.Type);
            }
        }

        private void EmitLambdaExpression(Expression expr) {
            LambdaExpression node = (LambdaExpression)expr;
            EmitDelegateConstruction(node, node.Type);
        }

        private void EmitRuntimeVariablesExpression(Expression expr) {
            RuntimeVariablesExpression node = (RuntimeVariablesExpression)expr;
            _scope.EmitVariableAccess(this, node.Variables);
        }

        private void EmitMemberAssignment(BinaryExpression node, EmitAs emitAs) {
            MemberExpression lvalue = (MemberExpression)node.Left;
            MemberInfo member = lvalue.Member;

            // emit "this", if any
            Type objectType = null;
            if (lvalue.Expression != null) {
                EmitInstance(lvalue.Expression, objectType = lvalue.Expression.Type);
            }

            // emit value
            EmitExpression(node.Right);

            LocalBuilder temp = null;
            if (emitAs != EmitAs.Void) {
                // save the value so we can return it
                _ilg.Emit(OpCodes.Dup);
                _ilg.Emit(OpCodes.Stloc, temp = GetLocal(node.Type));
            }

            switch (member.MemberType) {
                case MemberTypes.Field:
                    _ilg.EmitFieldSet((FieldInfo)member);
                    break;
                case MemberTypes.Property:
                    EmitCall(objectType, ((PropertyInfo)member).GetSetMethod(true));
                    break;
                default:
                    throw Error.InvalidMemberType(member.MemberType);
            }

            if (emitAs != EmitAs.Void) {
                _ilg.Emit(OpCodes.Ldloc, temp);
                FreeLocal(temp);
            }
        }

        //CONFORMING
        private void EmitMemberExpression(Expression expr) {
            MemberExpression node = (MemberExpression)expr;

            // emit "this", if any
            Type instanceType = null;
            if (node.Expression != null) {
                EmitInstance(node.Expression, instanceType = node.Expression.Type);
            }

            EmitMemberGet(node.Member, instanceType);
        }

        // assumes instance is already on the stack
        private void EmitMemberGet(MemberInfo member, Type objectType) {
            switch (member.MemberType) {
                case MemberTypes.Field:
                    FieldInfo fi = (FieldInfo)member;
                    if (fi.IsLiteral) {
                        EmitConstant(fi.GetRawConstantValue(), fi.FieldType);
                    } else {
                        _ilg.EmitFieldGet(fi);
                    }
                    break;
                case MemberTypes.Property:
                    EmitCall(objectType, ((PropertyInfo)member).GetGetMethod(true));
                    break;
                default:
                    throw ContractUtils.Unreachable;
            }
        }

        //CONFORMING
        private void EmitInstance(Expression instance, Type type) {
            if (instance != null) {
                if (type.IsValueType) {
                    EmitAddress(instance, type);
                } else {
                    EmitExpression(instance);
                }
            }
        }

        //CONFORMING
        private void EmitNewArrayExpression(Expression expr) {
            NewArrayExpression node = (NewArrayExpression)expr;

            if (node.NodeType == ExpressionType.NewArrayInit) {
                _ilg.EmitArray(
                    node.Type.GetElementType(),
                    node.Expressions.Count,
                    delegate(int index) {
                        EmitExpression(node.Expressions[index]);
                    }
                );
            } else {
                ReadOnlyCollection<Expression> bounds = node.Expressions;
                for (int i = 0; i < bounds.Count; i++) {
                    Expression x = bounds[i];
                    EmitExpression(x);
                    _ilg.EmitConvertToType(x.Type, typeof(int), true);
                }
                _ilg.EmitArray(node.Type);
            }
        }

        private void EmitDebugInfoExpression(Expression expr) {
            var node = (DebugInfoExpression)expr;

            if (!_emitDebugSymbols) {
                // just emit the body
                EmitExpression(node.Expression);
                return;
            }

            var symbolWriter = GetSymbolWriter(node.Document);
            _ilg.MarkSequencePoint(symbolWriter, node.StartLine, node.StartColumn, node.EndLine, node.EndColumn);
            _ilg.Emit(OpCodes.Nop);

            EmitExpression(node.Expression);

            // Clear the sequence point
            _ilg.MarkSequencePoint(symbolWriter, 0xfeefee, 0, 0xfeefee, 0);
            _ilg.Emit(OpCodes.Nop);
        }

        private ISymbolDocumentWriter GetSymbolWriter(SymbolDocumentInfo document) {
            Debug.Assert(_emitDebugSymbols);

            ISymbolDocumentWriter result;
            if (!_tree.SymbolWriters.TryGetValue(document, out result)) {
                var module = (ModuleBuilder)_typeBuilder.Module;
                result = module.DefineDocument(document.FileName, document.Language, document.LanguageVendor, SymbolGuids.DocumentType_Text);
                _tree.SymbolWriters.Add(document, result);
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expr")]
        private static void EmitExtensionExpression(Expression expr) {
            throw Error.ExtensionNotReduced();
        }

        #region ListInit, MemberInit

        private void EmitListInitExpression(Expression expr) {
            EmitListInit((ListInitExpression)expr);
        }

        private void EmitMemberInitExpression(Expression expr) {
            EmitMemberInit((MemberInitExpression)expr);
        }

        private void EmitBinding(MemberBinding binding, Type objectType) {
            switch (binding.BindingType) {
                case MemberBindingType.Assignment:
                    EmitMemberAssignment((MemberAssignment)binding, objectType);
                    break;
                case MemberBindingType.ListBinding:
                    EmitMemberListBinding((MemberListBinding)binding);
                    break;
                case MemberBindingType.MemberBinding:
                    EmitMemberMemberBinding((MemberMemberBinding)binding);
                    break;
                default:
                    throw Error.UnknownBindingType();
            }
        }

        private void EmitMemberAssignment(MemberAssignment binding, Type objectType) {
            EmitExpression(binding.Expression);
            FieldInfo fi = binding.Member as FieldInfo;
            if (fi != null) {
                _ilg.Emit(OpCodes.Stfld, fi);
            } else {
                PropertyInfo pi = binding.Member as PropertyInfo;
                if (pi != null) {
                    EmitCall(objectType, pi.GetSetMethod(true));
                } else {
                    throw Error.UnhandledBinding();
                }
            }
        }

        private void EmitMemberMemberBinding(MemberMemberBinding binding) {
            Type type = GetMemberType(binding.Member);
            if (binding.Member is PropertyInfo && type.IsValueType) {
                throw Error.CannotAutoInitializeValueTypeMemberThroughProperty(binding.Member);
            }
            if (type.IsValueType) {
                EmitMemberAddress(binding.Member, binding.Member.DeclaringType);
            } else {
                EmitMemberGet(binding.Member, binding.Member.DeclaringType);
            }
            if (binding.Bindings.Count == 0) {
                _ilg.Emit(OpCodes.Pop);
            } else {
                EmitMemberInit(binding.Bindings, false, type);
            }
        }

        private void EmitMemberListBinding(MemberListBinding binding) {
            Type type = GetMemberType(binding.Member);
            if (binding.Member is PropertyInfo && type.IsValueType) {
                throw Error.CannotAutoInitializeValueTypeElementThroughProperty(binding.Member);
            }
            if (type.IsValueType) {
                EmitMemberAddress(binding.Member, binding.Member.DeclaringType);
            } else {
                EmitMemberGet(binding.Member, binding.Member.DeclaringType);
            }
            EmitListInit(binding.Initializers, false, type);
        }

        private void EmitMemberInit(MemberInitExpression init) {
            EmitExpression(init.NewExpression);
            LocalBuilder loc = null;
            if (init.NewExpression.Type.IsValueType && init.Bindings.Count > 0) {
                loc = _ilg.DeclareLocal(init.NewExpression.Type);
                _ilg.Emit(OpCodes.Stloc, loc);
                _ilg.Emit(OpCodes.Ldloca, loc);
            }
            EmitMemberInit(init.Bindings, loc == null, init.NewExpression.Type);
            if (loc != null) {
                _ilg.Emit(OpCodes.Ldloc, loc);
            }
        }

        private void EmitMemberInit(ReadOnlyCollection<MemberBinding> bindings, bool keepOnStack, Type objectType) {
            for (int i = 0, n = bindings.Count; i < n; i++) {
                if (keepOnStack || i < n - 1) {
                    _ilg.Emit(OpCodes.Dup);
                }
                EmitBinding(bindings[i], objectType);
            }
        }

        private void EmitListInit(ListInitExpression init) {
            EmitExpression(init.NewExpression);
            LocalBuilder loc = null;
            if (init.NewExpression.Type.IsValueType) {
                loc = _ilg.DeclareLocal(init.NewExpression.Type);
                _ilg.Emit(OpCodes.Stloc, loc);
                _ilg.Emit(OpCodes.Ldloca, loc);
            }
            EmitListInit(init.Initializers, loc == null, init.NewExpression.Type);
            if (loc != null) {
                _ilg.Emit(OpCodes.Ldloc, loc);
            }
        }

        private void EmitListInit(ReadOnlyCollection<ElementInit> initializers, bool keepOnStack, Type objectType) {
            for (int i = 0, n = initializers.Count; i < n; i++) {
                if (keepOnStack || i < n - 1) {
                    _ilg.Emit(OpCodes.Dup);
                }
                EmitMethodCall(initializers[i].AddMethod, initializers[i], objectType);

                // Aome add methods, ArrayList.Add for example, return non-void
                if (initializers[i].AddMethod.ReturnType != typeof(void)) {
                    _ilg.Emit(OpCodes.Pop);
                }
            }
        }

        private static Type GetMemberType(MemberInfo member) {
            FieldInfo fi = member as FieldInfo;
            if (fi != null) return fi.FieldType;
            PropertyInfo pi = member as PropertyInfo;
            if (pi != null) return pi.PropertyType;
            throw Error.MemberNotFieldOrProperty(member);
        }

        #endregion

        #region Expression helpers

        //CONFORMING
        internal static void ValidateLift(IList<ParameterExpression> variables, IList<Expression> arguments) {
            System.Diagnostics.Debug.Assert(variables != null);
            System.Diagnostics.Debug.Assert(arguments != null);

            if (variables.Count != arguments.Count) {
                throw Error.IncorrectNumberOfIndexes();
            }
            for (int i = 0, n = variables.Count; i < n; i++) {
                if (!TypeUtils.AreReferenceAssignable(variables[i].Type, TypeUtils.GetNonNullableType(arguments[i].Type))) {
                    throw Error.ArgumentTypesMustMatch();
                }
            }
        }

        //CONFORMING
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void EmitLift(ExpressionType nodeType, Type resultType, MethodCallExpression mc, IList<ParameterExpression> parameters, IList<Expression> arguments) {
            Debug.Assert(TypeUtils.GetNonNullableType(resultType) == TypeUtils.GetNonNullableType(mc.Type));
            ReadOnlyCollection<ParameterExpression> paramList = new ReadOnlyCollection<ParameterExpression>(parameters);
            ReadOnlyCollection<Expression> argList = new ReadOnlyCollection<Expression>(arguments);

            switch (nodeType) {
                default:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual: {
                        Label exit = _ilg.DefineLabel();
                        Label exitNull = _ilg.DefineLabel();
                        LocalBuilder anyNull = _ilg.DeclareLocal(typeof(bool));
                        for (int i = 0, n = paramList.Count; i < n; i++) {
                            ParameterExpression v = paramList[i];
                            Expression arg = argList[i];
                            if (TypeUtils.IsNullableType(arg.Type)) {
                                _scope.AddLocal(this, v);
                                EmitAddress(arg, arg.Type);
                                _ilg.Emit(OpCodes.Dup);
                                _ilg.EmitHasValue(arg.Type);
                                _ilg.Emit(OpCodes.Ldc_I4_0);
                                _ilg.Emit(OpCodes.Ceq);
                                _ilg.Emit(OpCodes.Stloc, anyNull);
                                _ilg.EmitGetValueOrDefault(arg.Type);
                                _scope.EmitSet(v);
                            } else {
                                _scope.AddLocal(this, v);
                                EmitExpression(arg);
                                if (!arg.Type.IsValueType) {
                                    _ilg.Emit(OpCodes.Dup);
                                    _ilg.Emit(OpCodes.Ldnull);
                                    _ilg.Emit(OpCodes.Ceq);
                                    _ilg.Emit(OpCodes.Stloc, anyNull);
                                }
                                _scope.EmitSet(v);
                            }
                            _ilg.Emit(OpCodes.Ldloc, anyNull);
                            _ilg.Emit(OpCodes.Brtrue, exitNull);
                        }
                        EmitMethodCallExpression(mc);
                        if (TypeUtils.IsNullableType(resultType) && resultType != mc.Type) {
                            ConstructorInfo ci = resultType.GetConstructor(new Type[] { mc.Type });
                            _ilg.Emit(OpCodes.Newobj, ci);
                        }
                        _ilg.Emit(OpCodes.Br_S, exit);
                        _ilg.MarkLabel(exitNull);
                        if (resultType == TypeUtils.GetNullableType(mc.Type)) {
                            if (resultType.IsValueType) {
                                LocalBuilder result = GetLocal(resultType);
                                _ilg.Emit(OpCodes.Ldloca, result);
                                _ilg.Emit(OpCodes.Initobj, resultType);
                                _ilg.Emit(OpCodes.Ldloc, result);
                                FreeLocal(result);
                            } else {
                                _ilg.Emit(OpCodes.Ldnull);
                            }
                        } else {
                            switch (nodeType) {
                                case ExpressionType.LessThan:
                                case ExpressionType.LessThanOrEqual:
                                case ExpressionType.GreaterThan:
                                case ExpressionType.GreaterThanOrEqual:
                                    _ilg.Emit(OpCodes.Ldc_I4_0);
                                    break;
                                default:
                                    throw Error.UnknownLiftType(nodeType);
                            }
                        }
                        _ilg.MarkLabel(exit);
                        return;
                    }
                case ExpressionType.Equal:
                case ExpressionType.NotEqual: {
                        if (resultType == TypeUtils.GetNullableType(mc.Type)) {
                            goto default;
                        }
                        Label exit = _ilg.DefineLabel();
                        Label exitAllNull = _ilg.DefineLabel();
                        Label exitAnyNull = _ilg.DefineLabel();

                        LocalBuilder anyNull = _ilg.DeclareLocal(typeof(bool));
                        LocalBuilder allNull = _ilg.DeclareLocal(typeof(bool));
                        _ilg.Emit(OpCodes.Ldc_I4_0);
                        _ilg.Emit(OpCodes.Stloc, anyNull);
                        _ilg.Emit(OpCodes.Ldc_I4_1);
                        _ilg.Emit(OpCodes.Stloc, allNull);

                        for (int i = 0, n = paramList.Count; i < n; i++) {
                            ParameterExpression v = paramList[i];
                            Expression arg = argList[i];
                            _scope.AddLocal(this, v);
                            if (TypeUtils.IsNullableType(arg.Type)) {
                                EmitAddress(arg, arg.Type);
                                _ilg.Emit(OpCodes.Dup);
                                _ilg.EmitHasValue(arg.Type);
                                _ilg.Emit(OpCodes.Ldc_I4_0);
                                _ilg.Emit(OpCodes.Ceq);
                                _ilg.Emit(OpCodes.Dup);
                                _ilg.Emit(OpCodes.Ldloc, anyNull);
                                _ilg.Emit(OpCodes.Or);
                                _ilg.Emit(OpCodes.Stloc, anyNull);
                                _ilg.Emit(OpCodes.Ldloc, allNull);
                                _ilg.Emit(OpCodes.And);
                                _ilg.Emit(OpCodes.Stloc, allNull);
                                _ilg.EmitGetValueOrDefault(arg.Type);
                            } else {
                                EmitExpression(arg);
                                if (!arg.Type.IsValueType) {
                                    _ilg.Emit(OpCodes.Dup);
                                    _ilg.Emit(OpCodes.Ldnull);
                                    _ilg.Emit(OpCodes.Ceq);
                                    _ilg.Emit(OpCodes.Dup);
                                    _ilg.Emit(OpCodes.Ldloc, anyNull);
                                    _ilg.Emit(OpCodes.Or);
                                    _ilg.Emit(OpCodes.Stloc, anyNull);
                                    _ilg.Emit(OpCodes.Ldloc, allNull);
                                    _ilg.Emit(OpCodes.And);
                                    _ilg.Emit(OpCodes.Stloc, allNull);
                                } else {
                                    _ilg.Emit(OpCodes.Ldc_I4_0);
                                    _ilg.Emit(OpCodes.Stloc, allNull);
                                }
                            }
                            _scope.EmitSet(v);
                        }
                        _ilg.Emit(OpCodes.Ldloc, allNull);
                        _ilg.Emit(OpCodes.Brtrue, exitAllNull);
                        _ilg.Emit(OpCodes.Ldloc, anyNull);
                        _ilg.Emit(OpCodes.Brtrue, exitAnyNull);

                        EmitMethodCallExpression(mc);
                        if (TypeUtils.IsNullableType(resultType) && resultType != mc.Type) {
                            ConstructorInfo ci = resultType.GetConstructor(new Type[] { mc.Type });
                            _ilg.Emit(OpCodes.Newobj, ci);
                        }
                        _ilg.Emit(OpCodes.Br_S, exit);

                        _ilg.MarkLabel(exitAllNull);
                        _ilg.EmitBoolean(nodeType == ExpressionType.Equal);
                        _ilg.Emit(OpCodes.Br_S, exit);

                        _ilg.MarkLabel(exitAnyNull);
                        _ilg.EmitBoolean(nodeType == ExpressionType.NotEqual);

                        _ilg.MarkLabel(exit);
                        return;
                    }
            }
        }

        #endregion

        enum EmitAs {
            Default,
            Void
        }
    }
}
