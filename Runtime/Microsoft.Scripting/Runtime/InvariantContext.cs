/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using System.Linq.Expressions;
#else
using dynamic = System.Object;
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Dynamic;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Singleton LanguageContext which represents a language-neutral LanguageContext
    /// </summary>
    internal sealed class InvariantContext : LanguageContext {
        // friend: ScriptDomainManager
        internal InvariantContext(ScriptDomainManager manager)
            : base(manager) {
        }

        public override bool CanCreateSourceCode {
            get { return false; }
        }

        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink) {
            // invariant language doesn't have a grammar:
            throw new NotSupportedException();
        }

        public override T ScopeGetVariable<T>(Scope scope, string name) {
            var storage = scope.Storage as ScopeStorage;
            object res;
            if (storage != null && storage.TryGetValue(name, false, out res)) {
                return Operations.ConvertTo<T>(res);
            }

            StringDictionaryExpando dictStorage = scope.Storage as StringDictionaryExpando;
            if (dictStorage != null && dictStorage.Dictionary.TryGetValue(name, out res)) {
                return Operations.ConvertTo<T>(res);                
            }

            return base.ScopeGetVariable<T>(scope, name);
        }

        public override dynamic ScopeGetVariable(Scope scope, string name) {
            var storage = scope.Storage as ScopeStorage;
            object res;
            if (storage != null && storage.TryGetValue(name, false, out res)) {
                return res;
            }

            StringDictionaryExpando dictStorage = scope.Storage as StringDictionaryExpando;
            if (dictStorage != null && dictStorage.Dictionary.TryGetValue(name, out res)) {
                return res;
            }

            return base.ScopeGetVariable(scope, name);
        }

        public override void ScopeSetVariable(Scope scope, string name, object value) {
            var storage = scope.Storage as ScopeStorage;
            if (storage != null) {
                storage.SetValue(name, false, value);
                return;
            }

            StringDictionaryExpando dictStorage = scope.Storage as StringDictionaryExpando;
            if (dictStorage != null) {
                dictStorage.Dictionary[name] = value;
                return;
            }

            base.ScopeSetVariable(scope, name, value);
        }

        public override bool ScopeTryGetVariable(Scope scope, string name, out dynamic value) {
            var storage = scope.Storage as ScopeStorage;
            if (storage != null && storage.TryGetValue(name, false, out value)) {
                return true;
            }

            StringDictionaryExpando dictStorage = scope.Storage as StringDictionaryExpando;
            if (dictStorage != null && dictStorage.Dictionary.TryGetValue(name, out value)) {
                return true;
            }

            return base.ScopeTryGetVariable(scope, name, out value);
        }
    }
}
