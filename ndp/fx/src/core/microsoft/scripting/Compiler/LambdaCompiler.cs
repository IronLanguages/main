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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Dynamic.Utils;
using System.Text;

namespace System.Linq.Expressions.Compiler {

    /// <summary>
    /// LambdaCompiler is responsible for compiling individual lambda (LambdaExpression). The complete tree may
    /// contain multiple lambdas, the Compiler class is reponsible for compiling the whole tree, individual
    /// lambdas are then compiled by the LambdaCompiler.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal sealed partial class LambdaCompiler {

        private delegate void WriteBack();

        // Information on the entire lambda tree currently being compiled
        private readonly AnalyzedTree _tree;

        // Indicates that the method should logically be treated as a
        // DynamicMethod. We need this because in debuggable code we have to
        // emit into a MethodBuilder, but we still want to pretend it's a
        // DynamicMethod
        private readonly bool _dynamicMethod;

        private readonly ILGen _ilg;

        // The TypeBuilder backing this method, if any
        private readonly TypeBuilder _typeBuilder;

        private readonly MethodInfo _method;

        // Currently active LabelTargets and their mapping to IL labels
        private LabelBlockInfo _labelBlock = new LabelBlockInfo(null, LabelBlockKind.Block);
        // Mapping of labels used for "long" jumps (jumping out and into blocks)
        private readonly Dictionary<LabelTarget, LabelInfo> _labelInfo = new Dictionary<LabelTarget, LabelInfo>();

        // The currently active variable scope
        private CompilerScope _scope;

        // The lambda we are compiling
        private readonly LambdaExpression _lambda;

        /// <summary>
        /// Argument types
        /// 
        /// This list contains _all_ arguments on the underlying method builder (except for the
        /// "this"). There are two views on the list. First provides the raw view (shows all
        /// arguments), the second view provides view of the arguments which are in the original
        /// lambda (so first argument, which may be closure argument, is skipped in that case)
        /// </summary>
        private readonly ReadOnlyCollection<Type> _paramTypes;

        // True if we want to emitting debug symbols
        private readonly bool _emitDebugSymbols;

        // Runtime constants bound to the delegate
        private readonly BoundConstants _boundConstants;

        private LambdaCompiler(
            AnalyzedTree tree,
            LambdaExpression lambda,
            TypeBuilder typeBuilder,
            MethodInfo method,
            ILGenerator ilg,
            IList<Type> paramTypes,
            bool dynamicMethod,
            bool emitDebugSymbols) {

            ContractUtils.Requires(dynamicMethod || method.IsStatic, "dynamicMethod");
            _tree = tree;
            _lambda = lambda;
            _typeBuilder = typeBuilder;
            _method = method;
            _paramTypes = new ReadOnlyCollection<Type>(paramTypes);
            _dynamicMethod = dynamicMethod;

            // These are populated by AnalyzeTree/VariableBinder
            _scope = tree.Scopes[lambda];
            _boundConstants = tree.Constants[lambda];

            if (!dynamicMethod && _boundConstants.Count > 0) {
                throw Error.RtConstRequiresBundDelegate();
            }

            _ilg = new ILGen(ilg);

            Debug.Assert(!emitDebugSymbols || _typeBuilder != null, "emitting debug symbols requires a TypeBuilder");
            _emitDebugSymbols = emitDebugSymbols;

            // See if we can find a return label, so we can emit better IL
            AddReturnLabel(_lambda.Body);

            _boundConstants.EmitCacheConstants(this);
        }

        public override string ToString() {
            return _method.ToString();
        }

        internal ILGen IL {
            get { return _ilg; }
        }

        private bool IsDynamicMethod {
            get {
                return _method is DynamicMethod;
            }
        }

        internal ReadOnlyCollection<ParameterExpression> Parameters {
            get { return _lambda.Parameters; }
        }

        private bool HasClosure {
            get { return _paramTypes[0] == typeof(Closure); }
        }

        #region Compiler entry points

        /// <summary>
        /// Compiler entry point
        /// </summary>
        /// <param name="lambda">LambdaExpression to compile.</param>
        /// <param name="method">Product of compilation</param>
        /// <param name="delegateType">Type of the delegate to create</param>
        /// <param name="emitDebugSymbols">True to emit debug symbols, false otherwise.</param>
        /// <param name="forceDynamic">Force dynamic method regardless of save assemblies.</param>
        /// <returns>The compiled delegate.</returns>
        internal static Delegate CompileLambda(LambdaExpression lambda, Type delegateType, bool emitDebugSymbols, bool forceDynamic, out MethodInfo method) {
            // 1. Create signature
            List<Type> types;
            List<string> names;
            string name;
            Type returnType;
            ComputeSignature(lambda, out types, out names, out name, out returnType);

            // 2. Bind lambda
            AnalyzedTree tree = AnalyzeLambda(ref lambda);

            // 3. Create lambda compiler
            LambdaCompiler c = CreateDynamicCompiler(tree, lambda, name, returnType, types, null, emitDebugSymbols, forceDynamic);

            // 4. Emit
            c.EmitLambdaBody(null);

            // 5. Return the delegate.
            return c.CreateDelegate(delegateType, out method);
        }

        internal static T CompileLambda<T>(LambdaExpression lambda, bool forceDynamic, out MethodInfo method) {
            return (T)(object)CompileLambda(lambda, typeof(T), false, forceDynamic, out method);
        }

        internal static T CompileLambda<T>(LambdaExpression lambda, bool emitDebugSymbols) {
            MethodInfo method;
            return (T)(object)CompileLambda(lambda, typeof(T), emitDebugSymbols, false, out method);
        }

        internal static Delegate CompileLambda(LambdaExpression lambda, bool emitDebugSymbols) {
            MethodInfo method;
            return CompileLambda(lambda, lambda.Type, emitDebugSymbols, false, out method);
        }

        /// <summary>
        /// Creates and returns a MethodBuilder
        /// </summary>
        internal static MethodBuilder CompileLambda(LambdaExpression lambda, TypeBuilder type, MethodAttributes attributes, bool emitDebugSymbols) {
            // 1. Create signature
            List<Type> types;
            List<string> names;
            string lambdaName;
            Type returnType;
            ComputeSignature(lambda, out types, out names, out lambdaName, out returnType);

            // 2. Bind lambda
            AnalyzedTree tree = AnalyzeLambda(ref lambda);

            // 3. Create lambda compiler
            LambdaCompiler c = CreateStaticCompiler(
                tree,
                lambda,
                type,
                lambda.Name ?? "lambda_method", // don't use generated name
                attributes,
                returnType,
                types,
                names,
                false, // dynamicMethod
                emitDebugSymbols
            );

            // 4. Emit
            c.EmitLambdaBody(null);

            return (MethodBuilder)c._method;
        }

        #endregion

        private static AnalyzedTree AnalyzeLambda(ref LambdaExpression lambda) {
            // Spill the stack for any exception handling blocks or other
            // constructs which require entering with an empty stack
            lambda = StackSpiller.AnalyzeLambda(lambda);

            // Bind any variable references in this lambda
            return VariableBinder.Bind(lambda);
        }

        internal LocalBuilder GetNamedLocal(Type type, string name) {
            Assert.NotNull(type);

            if (_emitDebugSymbols && name != null) {
                LocalBuilder lb = _ilg.DeclareLocal(type);
                // TODO: we need to set the lexical scope properly, so it can
                // be freed and reused!
                lb.SetLocalSymInfo(name);
                return lb;
            }
            return _ilg.GetLocal(type);
        }

        internal void FreeNamedLocal(LocalBuilder local, string name) {
            if (_emitDebugSymbols && name != null) {
                // local has a name, we can't free it
                return;
            }
            _ilg.FreeLocal(local);
        }

        /// <summary>
        /// Gets the argument slot corresponding to the parameter at the given
        /// index. Assumes that the method takes a certain number of prefix
        /// arguments, followed by the real parameters stored in Parameters
        /// </summary>
        internal int GetLambdaArgument(int index) {
            return index + (HasClosure ? 1 : 0) + (_method.IsStatic ? 0 : 1);
        }

        internal Type GetLambdaArgumentType(int index) {
            return _paramTypes[index + (HasClosure ? 1 : 0)];
        }

        /// <summary>
        /// Returns the index-th argument. This method provides access to the actual arguments
        /// defined on the lambda itself, and excludes the possible 0-th closure argument.
        /// </summary>
        internal void EmitLambdaArgument(int index) {
            _ilg.EmitLoadArg(GetLambdaArgument(index));
        }

        internal void EmitClosureArgument() {
            Debug.Assert(HasClosure, "must have a Closure argument");

            // TODO: this is for emitting into instance methods, but it's not
            // used anymore (we always emit into static MethodBuilders or
            // DynamicMethods, which are both static)
            _ilg.EmitLoadArg(_method.IsStatic ? 0 : 1);
        }

        private MethodInfo CreateDelegateMethodInfo() {
            if (_method is DynamicMethod) {
                return (MethodInfo)_method;
            } else {
                var mb = (MethodBuilder)_method;
                Type methodType = _typeBuilder.CreateType();
                return methodType.GetMethod(mb.Name);
            }
        }

        private Delegate CreateDelegate(Type delegateType, out MethodInfo method) {
            method = CreateDelegateMethodInfo();

            if (_dynamicMethod) {
                return method.CreateDelegate(delegateType, new Closure(_boundConstants.ToArray(), null));
            } else {
                return method.CreateDelegate(delegateType);
            }
        }

        #region Factory methods

        /// <summary>
        /// Creates a compiler backed by dynamic method. Sometimes (when debugging is required) the dynamic
        /// method is actually a 'fake' dynamic method and is backed by static type created specifically for
        /// the one method
        /// </summary>
        private static LambdaCompiler CreateDynamicCompiler(
            AnalyzedTree tree,
            LambdaExpression lambda,
            string methodName,
            Type returnType,
            IList<Type> paramTypes,
            IList<string> paramNames,
            bool emitDebugSymbols,
            bool forceDynamic) {

            Assert.NotEmpty(methodName);
            Assert.NotNull(returnType);
            Assert.NotNullItems(paramTypes);

            LambdaCompiler lc;

            //
            // Generate a static method if either
            // 1) we want to dump all geneated IL to an assembly on disk (SaveSnippets on)
            // 2) the method is debuggable, i.e. DebugMode is on and a source unit is associated with the method
            //
            if ((Snippets.Shared.SaveSnippets || emitDebugSymbols) && !forceDynamic) {
                var typeBuilder = Snippets.Shared.DefineType(methodName, typeof(object), false, false, emitDebugSymbols);
                lc = CreateStaticCompiler(
                    tree,
                    lambda,
                    typeBuilder,
                    methodName,
                    TypeUtils.PublicStatic,
                    returnType,
                    paramTypes,
                    paramNames,
                    true, // dynamicMethod
                    emitDebugSymbols
                );
            } else {
                Type[] parameterTypes = paramTypes.AddFirst(typeof(Closure));
                DynamicMethod target = Snippets.Shared.CreateDynamicMethod(methodName, returnType, parameterTypes);
                lc = new LambdaCompiler(
                    tree,
                    lambda,
                    null, // typeGen
                    target,
                    target.GetILGenerator(),
                    parameterTypes,
                    true, // dynamicMethod
                    false // emitDebugSymbols
                );
            }

            return lc;
        }

        /// <summary>
        /// Creates a LambdaCompiler backed by a method on a static type
        /// </summary>
        private static LambdaCompiler CreateStaticCompiler(
            AnalyzedTree tree,
            LambdaExpression lambda,
            TypeBuilder typeBuilder,
            string name,
            MethodAttributes attributes,
            Type retType,
            IList<Type> paramTypes,
            IList<string> paramNames,
            bool dynamicMethod,
            bool emitDebugSymbols) {

            Assert.NotNull(name, retType);

            bool closure = tree.Scopes[lambda].NeedsClosure;
            Type[] parameterTypes;
            if (dynamicMethod || closure) {
                parameterTypes = paramTypes.AddFirst(typeof(Closure));
            } else {
                parameterTypes = paramTypes.ToArray();
            }

            MethodBuilder mb = typeBuilder.DefineMethod(name, attributes, retType, parameterTypes);
            LambdaCompiler lc = new LambdaCompiler(
                tree,
                lambda,
                typeBuilder,
                mb,
                mb.GetILGenerator(),
                parameterTypes,
                dynamicMethod,
                emitDebugSymbols
            );

            if (paramNames != null) {
                // parameters are index from 1, with closure argument we need to skip the first arg
                int startIndex = (dynamicMethod || closure) ? 2 : 1;
                for (int i = 0; i < paramNames.Count; i++) {
                    mb.DefineParameter(i + startIndex, ParameterAttributes.None, paramNames[i]);
                }
            }
            return lc;
        }

        #endregion
    }
}
