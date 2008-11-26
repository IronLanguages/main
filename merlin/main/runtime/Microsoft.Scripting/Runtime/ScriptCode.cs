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
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.Contracts;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;

namespace Microsoft.Scripting {
    /// <summary>
    /// ScriptCode is an instance of compiled code that is bound to a specific LanguageContext
    /// but not a specific ScriptScope. The code can be re-executed multiple times in different
    /// scopes. Hosting API counterpart for this class is <c>CompiledCode</c>.
    /// </summary>
    public class ScriptCode {
        // TODO: should probably store this as Expression<DlrMainCallTarget>
        private readonly LambdaExpression _code;
        private readonly SourceUnit _sourceUnit;
        private DlrMainCallTarget _target;

        public ScriptCode(LambdaExpression code, SourceUnit sourceUnit)
            : this(code, null, sourceUnit) {
        }

        public ScriptCode(LambdaExpression code, DlrMainCallTarget target, SourceUnit sourceUnit) {
            if (code == null && target == null) {
                throw Error.MustHaveCodeOrTarget();
            }

            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");

            _code = code;
            _sourceUnit = sourceUnit;
            _target = target;
        }

        public LanguageContext LanguageContext {
            get { return _sourceUnit.LanguageContext; }
        }

        public DlrMainCallTarget Target {
            get { return _target; }
        }

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public LambdaExpression Code {
            get { return _code; }
        }

        public virtual Scope CreateScope() {
            return new Scope();
        }

        public virtual void EnsureCompiled() {
            EnsureTarget(_code);
        }

        public object Run(Scope scope) {
            return InvokeTarget(_code, scope);
        }

        public object Run() {
            return Run(CreateScope());
        }

        protected virtual object InvokeTarget(LambdaExpression code, Scope scope) {
            return EnsureTarget(code)(scope, LanguageContext);
        }

        private DlrMainCallTarget EnsureTarget(LambdaExpression code) {
            if (_target == null) {
                var lambda = code as Expression<DlrMainCallTarget>;
                if (lambda == null) {
                    // If language APIs produced the wrong delegate type,
                    // rewrite the lambda with the correct type
                    lambda = Expression.Lambda<DlrMainCallTarget>(code.Body, code.Name, code.Parameters);
                }
                Interlocked.CompareExchange(ref _target, lambda.Compile(SourceUnit.EmitDebugSymbols), null);
            }
            return _target;
        }

        internal MethodBuilder CompileToDisk(TypeGen typeGen, Dictionary<SymbolId, FieldBuilder> symbolDict) {
            if (_code == null) {
                throw Error.NoCodeToCompile();
            }

            MethodBuilder mb = CompileForSave(typeGen, symbolDict);
            return mb;
        }

        public static ScriptCode Load(DlrMainCallTarget method, LanguageContext language, string path) {
            SourceUnit su = new SourceUnit(language, NullTextContentProvider.Null, path, SourceCodeKind.File);
            return new ScriptCode(null, method, su);
        }

        protected virtual MethodBuilder CompileForSave(TypeGen typeGen, Dictionary<SymbolId, FieldBuilder> symbolDict) {
            var diskRewriter = new ToDiskRewriter(typeGen);
            var lambda = diskRewriter.RewriteLambda(_code);
            
            return lambda.CompileToMethod(typeGen.TypeBuilder, CompilerHelpers.PublicStatic | MethodAttributes.SpecialName, false);
        }

        /// <summary>
        /// This takes an assembly name including extension and saves the provided ScriptCode objects into the assembly.  
        /// 
        /// The provided script codes can constitute code from multiple languages.  The assemblyName can be either a fully qualified 
        /// or a relative path.  The DLR will simply save the assembly to the desired location.  The assembly is created by the DLR and 
        /// if a file already exists than an exception is raised.  
        /// 
        /// The DLR determines the internal format of the ScriptCode and the DLR can feel free to rev this as appropriate.  
        /// </summary>
        public static void SaveToAssembly(string assemblyName, params ScriptCode[] codes) {
            ContractUtils.RequiresNotNull(assemblyName, "assemblyName");
            ContractUtils.RequiresNotNullItems(codes, "codes");

            // break the assemblyName into it's dir/name/extension
            string dir = Path.GetDirectoryName(assemblyName);
            if (String.IsNullOrEmpty(dir)) {
                dir = Environment.CurrentDirectory;
            }

            string name = Path.GetFileNameWithoutExtension(assemblyName);
            string ext = Path.GetExtension(assemblyName);

            // build the assembly & type gen that all the script codes will live in...
            AssemblyGen ag = new AssemblyGen(new AssemblyName(name), dir, ext, /*emitSymbols*/false);
            TypeBuilder tb = ag.DefinePublicType("DLRCachedCode", typeof(object), true);
            TypeGen tg = new TypeGen(ag, tb);
            var symbolDict = new Dictionary<SymbolId, FieldBuilder>();
            // then compile all of the code

            Dictionary<Type, List<KeyValuePair<MethodBuilder, ScriptCode>>> langCtxBuilders = new Dictionary<Type, List<KeyValuePair<MethodBuilder, ScriptCode>>>();
            foreach (ScriptCode sc in codes) {
                List<KeyValuePair<MethodBuilder, ScriptCode>> builders;
                if (!langCtxBuilders.TryGetValue(sc.LanguageContext.GetType(), out builders)) {
                    langCtxBuilders[sc.LanguageContext.GetType()] = builders = new List<KeyValuePair<MethodBuilder, ScriptCode>>();
                }

                builders.Add(
                    new KeyValuePair<MethodBuilder, ScriptCode>(
                        sc.CompileToDisk(tg, symbolDict),
                        sc
                    )
                );
            }

            MethodBuilder mb = tb.DefineMethod(
                "GetScriptCodeInfo",
                MethodAttributes.SpecialName | MethodAttributes.Public | MethodAttributes.Static,
                typeof(Tuple<Type[], DlrMainCallTarget[][], string[][], object>),
                Type.EmptyTypes);

            ILGen ilgen = new ILGen(mb.GetILGenerator());

            var langsWithBuilders = langCtxBuilders.ToArray();

            // lang ctx array
            ilgen.EmitArray(typeof(Type), langsWithBuilders.Length, (index) => {
                ilgen.Emit(OpCodes.Ldtoken, langsWithBuilders[index].Key);
                ilgen.EmitCall(typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }));
            });

            // builders array of array
            ilgen.EmitArray(typeof(DlrMainCallTarget[]), langsWithBuilders.Length, (index) => {
                List<KeyValuePair<MethodBuilder, ScriptCode>> builders = langsWithBuilders[index].Value;

                ilgen.EmitArray(typeof(DlrMainCallTarget), builders.Count, (innerIndex) => {
                    ilgen.EmitNull();
                    ilgen.Emit(OpCodes.Ldftn, builders[innerIndex].Key);
                    ilgen.EmitNew(
                        typeof(DlrMainCallTarget),
                        new[] { typeof(object), typeof(IntPtr) }
                    );
                });
            });

            // paths array of array
            ilgen.EmitArray(typeof(string[]), langsWithBuilders.Length, (index) => {
                List<KeyValuePair<MethodBuilder, ScriptCode>> builders = langsWithBuilders[index].Value;

                ilgen.EmitArray(typeof(string), builders.Count, (innerIndex) => {
                    ilgen.EmitString(builders[innerIndex].Value._sourceUnit.Path);
                });
            });

            // 4th element in tuple - always null...
            ilgen.EmitNull();

            ilgen.EmitNew(
                typeof(Tuple<Type[], DlrMainCallTarget[][], string[][], object>), 
                new[] { typeof(Type[]), typeof(DlrMainCallTarget[][]), typeof(string[][]), typeof(object) }
            );
            ilgen.Emit(OpCodes.Ret);

            mb.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(DlrCachedCodeAttribute).GetConstructor(Type.EmptyTypes),
                ArrayUtils.EmptyObjects
            ));

            tg.FinishType();
            ag.SaveAssembly();
        }

        /// <summary>
        /// This will take an assembly object which the user has loaded and return a new set of ScriptCode’s which have 
        /// been loaded into the provided ScriptDomainManager.  
        /// 
        /// If the language associated with the ScriptCode’s has not already been loaded the DLR will load the 
        /// LanguageContext into the ScriptDomainManager based upon the saved LanguageContext type.  
        /// 
        /// If the LanguageContext or the version of the DLR the language was compiled against is unavailable a 
        /// TypeLoadException will be raised unless policy has been applied by the administrator to redirect bindings.
        /// </summary>
        public static ScriptCode[] LoadFromAssembly(ScriptDomainManager runtime, Assembly assembly) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            ContractUtils.RequiresNotNull(assembly, "assembly");

            // get the type which has our cached code...
            Type t = assembly.GetType("DLRCachedCode");
            if (t == null) {
                return new ScriptCode[0];
            }

            List<ScriptCode> codes = new List<ScriptCode>();

            MethodInfo mi = t.GetMethod("GetScriptCodeInfo");
            if (mi.IsSpecialName && mi.IsDefined(typeof(DlrCachedCodeAttribute), false)) {
                var infos = (Tuple<Type[], DlrMainCallTarget[][], string[][], object>)mi.Invoke(null, ArrayUtils.EmptyObjects);

                for (int i = 0; i < infos.Item000.Length; i++) {
                    Type curType = infos.Item000[i];
                    LanguageContext lc = runtime.GetLanguage(curType);

                    Debug.Assert(infos.Item001[i].Length == infos.Item002[i].Length);

                    DlrMainCallTarget[] methods = infos.Item001[i];
                    string[] names = infos.Item002[i];

                    for (int j = 0; j < methods.Length; j++) {                        
                        codes.Add(lc.LoadCompiledCode(methods[j], names[j]));                        
                    }
                }
            }

            return codes.ToArray();
        }

        [Confined]
        public override string ToString() {
            return String.Format("ScriptCode '{0}' from {1}", SourceUnit.Path, LanguageContext.GetType().Name);
        }
    }
}
