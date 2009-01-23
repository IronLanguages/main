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
using System.Dynamic.Utils;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

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

        private readonly ILGenerator _ilg;

        // The TypeBuilder backing this method, if any
        private readonly TypeBuilder _typeBuilder;

        private readonly MethodInfo _method;

        // Currently active LabelTargets and their mapping to IL labels
        private LabelScopeInfo _labelBlock = new LabelScopeInfo(null, LabelScopeKind.Lambda);
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

        // Free list of locals, so we reuse them rather than creating new ones
        private readonly KeyedQueue<Type, LocalBuilder> _freeLocals = new KeyedQueue<Type, LocalBuilder>();

        /// <summary>
        /// Creates a lambda compiler that will compile to a dynamic method
        /// </summary>
        private LambdaCompiler(AnalyzedTree tree, LambdaExpression lambda) {
            Type[] parameterTypes = GetParameterTypes(lambda).AddFirst(typeof(Closure));

#if SILVERLIGHT
            var method = new DynamicMethod(GetGeneratedName(lambda.Name), lambda.ReturnType, parameterTypes);
#else
            var method = new DynamicMethod(GetGeneratedName(lambda.Name), lambda.ReturnType, parameterTypes, true);
#endif

            _tree = tree;
            _lambda = lambda;
            _method = method;
            _ilg = method.GetILGenerator();
            _paramTypes = new ReadOnlyCollection<Type>(parameterTypes);

            // These are populated by AnalyzeTree/VariableBinder
            _scope = tree.Scopes[lambda];
            _boundConstants = tree.Constants[lambda];

            Initialize();
        }

        /// <summary>
        /// Creates a lambda compiler that will compile into the provided Methodbuilder
        /// </summary>
        private LambdaCompiler(AnalyzedTree tree, LambdaExpression lambda, MethodBuilder method, bool emitDebugSymbols) {
            bool closure = tree.Scopes[lambda].NeedsClosure;
            Type[] paramTypes = GetParameterTypes(lambda);
            if (closure) {
                paramTypes = paramTypes.AddFirst(typeof(Closure));
            }

            method.SetReturnType(lambda.ReturnType);
            method.SetParameters(paramTypes);
            var paramNames = lambda.Parameters.Map(p => p.Name);
            // parameters are index from 1, with closure argument we need to skip the first arg
            int startIndex = closure ? 2 : 1;
            for (int i = 0; i < paramNames.Length; i++) {
                method.DefineParameter(i + startIndex, ParameterAttributes.None, paramNames[i]);
            }

            _tree = tree;
            _lambda = lambda;
            _typeBuilder = (TypeBuilder)method.DeclaringType;
            _method = method;
            _ilg = method.GetILGenerator();
            _paramTypes = new ReadOnlyCollection<Type>(paramTypes);
            _emitDebugSymbols = emitDebugSymbols;

            // These are populated by AnalyzeTree/VariableBinder
            _scope = tree.Scopes[lambda];
            _boundConstants = tree.Constants[lambda];

            Initialize();
        }

        private void Initialize() {
            // See if we can find a return label, so we can emit better IL
            AddReturnLabel(_lambda.Body);
            _boundConstants.EmitCacheConstants(this);
        }

        public override string ToString() {
            return _method.ToString();
        }

        internal ILGenerator IL {
            get { return _ilg; }
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
        /// <returns>The compiled delegate.</returns>
        internal static Delegate Compile(LambdaExpression lambda) {
            // 1. Bind lambda
            AnalyzedTree tree = AnalyzeLambda(ref lambda);

            // 2. Create lambda compiler
            LambdaCompiler c = new LambdaCompiler(tree, lambda);

            // 3. Emit
            c.EmitLambdaBody(null);

            // 4. Return the delegate.
            return c.CreateDelegate();
        }

        /// <summary>
        /// Mutates the MethodBuilder parameter, filling in IL, parameters,
        /// and return type.
        /// 
        /// (probably shouldn't be modifying parameters/return type...)
        /// </summary>
        internal static void Compile(LambdaExpression lambda, MethodBuilder method, bool emitDebugSymbols) {
            // 1. Bind lambda
            AnalyzedTree tree = AnalyzeLambda(ref lambda);

            // 2. Create lambda compiler
            LambdaCompiler c = new LambdaCompiler(tree, lambda, method, emitDebugSymbols);

            // 3. Emit
            c.EmitLambdaBody(null);
        }

        #endregion

        private static AnalyzedTree AnalyzeLambda(ref LambdaExpression lambda) {
            // Spill the stack for any exception handling blocks or other
            // constructs which require entering with an empty stack
            lambda = StackSpiller.AnalyzeLambda(lambda);

            // Bind any variable references in this lambda
            return VariableBinder.Bind(lambda);
        }

        internal LocalBuilder GetLocal(Type type) {
            Debug.Assert(type != null);

            LocalBuilder local;
            if (_freeLocals.TryDequeue(type, out local)) {
                Debug.Assert(type == local.LocalType);
                return local;
            }

            return _ilg.DeclareLocal(type);
        }

        internal void FreeLocal(LocalBuilder local) {
            if (local != null) {
                _freeLocals.Enqueue(local.LocalType, local);
            }
        }

        internal LocalBuilder GetNamedLocal(Type type, string name) {
            Debug.Assert(type != null);

            LocalBuilder lb = _ilg.DeclareLocal(type);
            if (_emitDebugSymbols && name != null) {
                lb.SetLocalSymInfo(name);
            }
            return lb;
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
            Debug.Assert(_method.IsStatic, "must be a static method");
            _ilg.EmitLoadArg(0);
        }

        private Delegate CreateDelegate() {
            Debug.Assert(_method is DynamicMethod);

            return _method.CreateDelegate(_lambda.Type, new Closure(_boundConstants.ToArray(), null));
        }

        private FieldBuilder CreateStaticField(string name, Type type) {
            // We are emitting into someone else's type. We don't want name
            // conflicts, so choose a long name that is unlikely to confict.
            // Naming scheme chosen here is similar to what the C# compiler
            // uses.
            return _typeBuilder.DefineField("<ExpressionCompilerImplementationDetails>{" + Interlocked.Increment(ref _Counter) + "}" + name, type, FieldAttributes.Static | FieldAttributes.Private);
        }

        /// <summary>
        /// Creates an unitialized field suitible for private implementation details
        /// Works with DynamicMethods or TypeBuilders.
        /// </summary>
        private MemberExpression CreateLazyInitializedField<T>(string name) {
            if (_method is DynamicMethod) {
                return Expression.Field(Expression.Constant(new StrongBox<T>()), "Value");
            } else {
                return Expression.Field(null, CreateStaticField(name, typeof(T)));
            }
        }
    }
}
