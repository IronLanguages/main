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

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Builtins;
using System.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Scripting;

#if SILVERLIGHT
[assembly: DynamicLanguageProvider(typeof(RubyContext), RubyContext.IronRubyDisplayName, RubyContext.IronRubyNames, RubyContext.IronRubyFileExtensions)]
#endif

namespace IronRuby {
    
    public static class Ruby {
        /// <summary>
        /// Creates a new script runtime configured using .NET configuration files.
        /// If the .NET configuration doesn't include IronRuby a default IronRuby configuration is added.
        /// </summary>
        public static ScriptRuntime/*!*/ CreateRuntime() {
            var setup = ScriptRuntimeSetup.ReadConfiguration();
            setup.AddRubySetup();
            return new ScriptRuntime(setup);
        }

        public static ScriptRuntime/*!*/ CreateRuntime(ScriptRuntimeSetup/*!*/ setup) {
            return new ScriptRuntime(setup);
        }

        /// <summary>
        /// Creates a new script runtime and returns its IronRuby engine.
        /// </summary>
        public static ScriptEngine/*!*/ CreateEngine() {
            return GetEngine(CreateRuntime());
        }

        /// <summary>
        /// Creates a new script runtime and returns its IronRuby engine configured using <paramref name="rubySetup"/>.
        /// </summary>
        /// <remarks>
        /// If the configuration loaded from .config files already contains 
        /// </remarks>
        public static ScriptEngine/*!*/ CreateEngine(Action<LanguageSetup/*!*/>/*!*/ setupInitializer) {
            ContractUtils.RequiresNotNull(setupInitializer, "setupInitializer");

            var runtimeSetup = ScriptRuntimeSetup.ReadConfiguration();
            int index = IndexOfRubySetup(runtimeSetup);
            if (index != -1) {
                setupInitializer(runtimeSetup.LanguageSetups[index]);
            } else {
                runtimeSetup.LanguageSetups.Add(CreateRubySetup(setupInitializer));
            }

            return GetEngine(CreateRuntime(runtimeSetup));
        }

        /// <summary>
        /// Creates IronRuby setup with default language names and file extensions.
        /// </summary>
        public static LanguageSetup/*!*/ CreateRubySetup() {
            return new LanguageSetup(
                typeof(RubyContext).AssemblyQualifiedName,
                RubyContext.IronRubyDisplayName,
                RubyContext.IronRubyNames.Split(';'),
                RubyContext.IronRubyFileExtensions.Split(';')
            );
        }

        public static LanguageSetup/*!*/ CreateRubySetup(Action<LanguageSetup/*!*/>/*!*/ initializer) {
            ContractUtils.RequiresNotNull(initializer, "initializer");

            var setup = CreateRubySetup();
            initializer(setup);
            return setup;
        }

        public static ScriptEngine/*!*/ GetEngine(ScriptRuntime/*!*/ runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            return runtime.GetEngineByTypeName(typeof(RubyContext).AssemblyQualifiedName);
        }

        public static bool RequireFile(ScriptEngine/*!*/ engine, string/*!*/ path) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(path, "path");

            return HostingHelpers.CallEngine<KeyValuePair<string, Scope>, bool>(engine, RequireFile, 
                new KeyValuePair<string, Scope>(path, null));
        }

        public static bool RequireFile(ScriptEngine/*!*/ engine, string/*!*/ path, ScriptScope/*!*/ scope) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(scope, "scope");

            return HostingHelpers.CallEngine<KeyValuePair<string, Scope>, bool>(engine, RequireFile, 
                new KeyValuePair<string, Scope>(path, HostingHelpers.GetScope(scope)));
        }

        internal static bool RequireFile(LanguageContext/*!*/ context, KeyValuePair<string, Scope> pathAndScope) {
            var rc = (RubyContext)context;
            return rc.Loader.LoadFile(pathAndScope.Value, null, MutableString.Create(pathAndScope.Key),
                LoadFlags.LoadOnce | LoadFlags.AppendExtensions);
        }

        // TODO:
        public static RubyContext/*!*/ GetExecutionContext(ScriptEngine/*!*/ engine) {
            ContractUtils.RequiresNotNull(engine, "engine");
            return (RubyContext)HostingHelpers.GetLanguageContext(engine);
        }

        // TODO:
        public static RubyContext/*!*/ GetExecutionContext(ScriptRuntime/*!*/ runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            return GetExecutionContext(GetEngine(runtime));
        }

        internal static int IndexOfRubySetup(ScriptRuntimeSetup/*!*/ runtimeSetup) {
            for (int i = 0; i < runtimeSetup.LanguageSetups.Count; i++) {
                var langSetup = runtimeSetup.LanguageSetups[i];
                if (langSetup.TypeName == typeof(RubyContext).AssemblyQualifiedName) {
                    return i;
                }
            }
            return -1;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RubyHostingExtensions {
        public static bool RequireRubyFile(this ScriptEngine/*!*/ engine, string/*!*/ path) {
            return Ruby.RequireFile(engine, path);
        }

        public static ScriptEngine/*!*/ GetRubyEngine(this ScriptRuntime/*!*/ runtime) {
            return Ruby.GetEngine(runtime);
        }

        public static LanguageSetup/*!*/ AddRubySetup(this ScriptRuntimeSetup/*!*/ runtimeSetup) {
            return AddRubySetup(runtimeSetup, null);
        }

        /// <summary>
        /// Adds a new Ruby setup into the given runtime setup unless Ruby setup is already there.
        /// Returns the newly created setup or the existing one.
        /// If non-null <paramref name="newSetupInitializer"/> is given and a new setup is created runs the initializer on the new setup instance.
        /// </summary>
        public static LanguageSetup/*!*/ AddRubySetup(this ScriptRuntimeSetup/*!*/ runtimeSetup, Action<LanguageSetup/*!*/> newSetupInitializer) {
            ContractUtils.RequiresNotNull(runtimeSetup, "runtimeSetup");
            
            LanguageSetup langSetup;
            int index = Ruby.IndexOfRubySetup(runtimeSetup);
            if (index == -1) {
                langSetup = Ruby.CreateRubySetup();
                if (newSetupInitializer != null) {
                    newSetupInitializer(langSetup);
                }
                runtimeSetup.LanguageSetups.Add(langSetup);
            } else {
                langSetup = runtimeSetup.LanguageSetups[index];
            }
            return langSetup;
        }
    }
}
