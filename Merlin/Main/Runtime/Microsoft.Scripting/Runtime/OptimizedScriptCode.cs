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
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    [Obsolete]
    public class OptimizedScriptCode : ScriptCode {
        private Scope _optimizedScope;
        private readonly LambdaExpression _code;
        private DlrMainCallTarget _optimizedTarget, _unoptimizedTarget;

        public OptimizedScriptCode(LambdaExpression code, SourceUnit sourceUnit)
            : base(sourceUnit) {
            Debug.Assert(code.Parameters.Count == 0, "GlobalRewritter shouldn't have been applied yet");
            _code = code;
        }

        public OptimizedScriptCode(Scope optimizedScope, DlrMainCallTarget optimizedTarget, SourceUnit sourceUnit)
            : base(sourceUnit) {
            ContractUtils.RequiresNotNull(optimizedScope, "optimizedScope");

            _optimizedScope = optimizedScope;
            _optimizedTarget = optimizedTarget;
        }

        public override Scope CreateScope() {
            return MakeOptimizedScope();
        }

        private Scope MakeOptimizedScope() {
            Debug.Assert((_optimizedTarget == null) == (_optimizedScope == null));

            if (_optimizedScope != null) {
                return _optimizedScope;
            }

            return CompileOptimizedScope();
        }

        public override object Run() {
            return InvokeTarget(_code, CreateScope());
        }

        public override object Run(Scope scope) {
            return InvokeTarget(_code, scope);
        }

        protected object InvokeTarget(LambdaExpression code, Scope scope) {
            if (scope == _optimizedScope) {
                return _optimizedTarget(scope, LanguageContext);
            } 

            // new scope, compile unoptimized code and use that.
            if (_unoptimizedTarget == null) {
                // TODO: fix generated DLR ASTs - languages should remove their usage
                // of GlobalVariables and then this can go away.
                Expression<DlrMainCallTarget> lambda = new GlobalLookupRewriter().RewriteLambda(code);

                _unoptimizedTarget = lambda.Compile(SourceUnit.EmitDebugSymbols);
            }


            return _unoptimizedTarget(scope, LanguageContext);            
        }

        /// <summary>
        /// Creates the methods and optimized Scope's which get associated with each ScriptCode.
        /// </summary>
        private Scope CompileOptimizedScope() {
            DlrMainCallTarget target;
            IAttributesCollection globals;
            CompileWithStaticGlobals(out target, out globals);

            // Force creation of names used in other script codes into all optimized dictionaries
            Scope scope = new Scope(globals);
            ((IModuleDictionaryInitialization)globals).InitializeModuleDictionary(new CodeContext(scope, LanguageContext));

            // everything succeeded, commit the results
            _optimizedTarget = target;
            _optimizedScope = scope;

            return scope;
        }

        public LambdaExpression Code {
            get { return _code; }
        }

        private void CompileWithStaticGlobals(out DlrMainCallTarget target, out IAttributesCollection globals) {
            // Create typegen
            TypeGen typeGen = Snippets.Shared.DefineType(MakeDebugName(), typeof(CustomSymbolDictionary), false, SourceUnit.EmitDebugSymbols);
            typeGen.TypeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            // Create rewriter
            GlobalStaticFieldRewriter rewriter = new GlobalStaticFieldRewriter(typeGen);

            // Compile lambda
            LambdaExpression lambda = rewriter.RewriteLambda(Code, "Initialize");
            MethodBuilder mb = typeGen.TypeBuilder.DefineMethod(lambda.Name, CompilerHelpers.PublicStatic);
            lambda.CompileToMethod(mb, SourceUnit.EmitDebugSymbols);

            // Create globals dictionary, finish type
            rewriter.EmitDictionary();
            Type type = typeGen.FinishType();
            globals = (IAttributesCollection)Activator.CreateInstance(type);

            // Create target
            target = (DlrMainCallTarget)Delegate.CreateDelegate(typeof(DlrMainCallTarget), type.GetMethod("Initialize"));

            // TODO: clean this up after clarifying dynamic site initialization logic
            InitializeFields(type);
        }        

        //
        // Initialization of dynamic sites stored in static fields 
        //

        public static void InitializeFields(Type type) {
            InitializeFields(type, false);
        }

        public static void InitializeFields(Type type, bool reusable) {
            if (type == null) return;

            const string slotStorageName = "#Constant";
            foreach (FieldInfo fi in type.GetFields()) {
                if (fi.Name.StartsWith(slotStorageName)) {
                    object value;
                    if (reusable) {
                        value = GlobalStaticFieldRewriter.GetConstantDataReusable(Int32.Parse(fi.Name.Substring(slotStorageName.Length)));
                    } else {
                        value = GlobalStaticFieldRewriter.GetConstantData(Int32.Parse(fi.Name.Substring(slotStorageName.Length)));
                    }
                    Debug.Assert(value != null);
                    fi.SetValue(null, value);
                }
            }
        }

        protected override KeyValuePair<MethodBuilder, Type> CompileForSave(TypeGen typeGen, Dictionary<SymbolId, FieldBuilder> symbolDict) {
            // first, serialize constants and dynamic sites:
            ToDiskRewriter diskRewriter = new ToDiskRewriter(typeGen);
            LambdaExpression lambda = diskRewriter.RewriteLambda(Code);
            
            // rewrite global variables:
            var globalRewriter = new GlobalArrayRewriter(symbolDict, typeGen);
            lambda = globalRewriter.RewriteLambda(lambda);
            
            MethodBuilder builder = typeGen.TypeBuilder.DefineMethod(lambda.Name ?? "lambda_method", CompilerHelpers.PublicStatic | MethodAttributes.SpecialName);
            lambda.CompileToMethod(builder, false);

            builder.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(CachedOptimizedCodeAttribute).GetConstructor(new Type[] { typeof(string[]) }),
                new object[] { ArrayUtils.ToArray(globalRewriter.Names) }
            ));

            return new KeyValuePair<MethodBuilder, Type>(builder, typeof(DlrMainCallTarget));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private string MakeDebugName() {
#if DEBUG
            if (SourceUnit != null && SourceUnit.HasPath) {
                return "OptScope_" + ReflectionUtils.ToValidTypeName(Path.GetFileNameWithoutExtension(IOUtils.ToValidPath(SourceUnit.Path)));
            }
#endif
            return "S";
        }

    }
}
