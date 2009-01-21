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

using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Interpreter {
    public class LightScriptCode : ScriptCode {
        private LightLambda _code, _optimizedCode;
        private Scope _optimizedScope;

        public LightScriptCode(LambdaExpression lambda, SourceUnit sourceUnit, bool optimized)
            : base(lambda, sourceUnit)
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            EnsureCompiled(optimized);
            //sw.Stop();
            //System.Console.WriteLine("{0} light compile", sw.Elapsed.TotalSeconds);
        }

        private void EnsureCompiled(bool optimized) {
            //TODO too much duplicated code between these two blocks
            if (optimized) {
                if (_optimizedCode != null) return;
                var rewriter = new LightGlobalRewriter();
                var newLambda = rewriter.RewriteLambda(Code, Code.Name, LanguageContext, optimized);
                _optimizedScope = rewriter.Scope;
                var compiler = new LightCompiler();
                var interpreter = compiler.CompileTop(newLambda);
                _optimizedCode = new LightLambda(interpreter, null);
            } else {
                if (_code != null) return;
                var rewriter = new LightGlobalRewriter();
                var newLambda = rewriter.RewriteLambda(Code, Code.Name, LanguageContext, optimized);
                var compiler = new LightCompiler();
                var interpreter = compiler.CompileTop(newLambda);
                _code = new LightLambda(interpreter, null);
            }
        }

        public override Scope CreateScope() {
            return _optimizedScope;
        }

        public override void EnsureCompiled() {
            // nop
        }

        protected override object InvokeTarget(LambdaExpression code, Scope scope) {
            //TODO code is used bizarrly here in this API
            if (_optimizedScope == null || scope != _optimizedScope) {
                EnsureCompiled(false);
                return _code.Run(scope, LanguageContext);
            } else {
                return _optimizedCode.Run();
            }
        }
    }
}
