/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using dynamic = System.Object;
#endif

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Builtins;
using IronRuby.Hosting;
using System.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Scripting;
using System.Runtime.Remoting;

#if SILVERLIGHT
[assembly: DynamicLanguageProvider(typeof(RubyContext), RubyContext.IronRubyDisplayName, RubyContext.IronRubyNames, RubyContext.IronRubyFileExtensions)]
#endif

namespace IronRuby {
    /// <summary>
    /// Implements IronRuby hosting APIs that enable embedding IronRuby in .NET application.
    /// </summary>
    /// <remarks>
    /// This class provides convenience APIs for applications that host Ruby language specifically.
    /// Use Dynamic Language Runtime (DLR) Hosting API from <see cref="Microsoft.Scripting.Hosting"/> namespace to implement a language agnostic host, i.e.
    /// a host that doesn't depend on a particular language and can host any DLR based language.
    /// </remarks>
    public static class Ruby {
        /// <summary>
        /// Creates a new script runtime configured using .NET configuration files.
        /// A default IronRuby configuration is added if not found anywhere in the .NET configuration files.
        /// </summary>
        /// <returns>A runtime that is capable of running IronRuby scripts and scripts of other languages specified in the .NET configuration files.</returns>
        public static ScriptRuntime/*!*/ CreateRuntime() {
            var setup = ScriptRuntimeSetup.ReadConfiguration();
            setup.AddRubySetup();
            return new ScriptRuntime(setup);
        }

        /// <summary>
        /// Creates a runtime given a runtime setup.
        /// </summary>
        /// <param name="setup">The setup to use to configure the runtime. It provides all information that's needed for the runtime initialization.</param>
        /// <returns>A runtime that is capable of running scripts of languages listed in <paramref name="setup"/>.</returns>
        public static ScriptRuntime/*!*/ CreateRuntime(ScriptRuntimeSetup/*!*/ setup) {
            return new ScriptRuntime(setup);
        }

        /// <summary>
        /// Creates a new script runtime and returns its IronRuby engine. The configuration of the runtime is loaded from .NET configuration files if available.
        /// </summary>
        /// <returns>A new IronRuby engine.</returns>
        /// <remarks>Creates a runtime using <see cref="CreateRuntime()"/> method.</remarks>
        public static ScriptEngine/*!*/ CreateEngine() {
            return GetEngine(CreateRuntime());
        }

        /// <summary>
        /// Creates a new script runtime and returns its IronRuby engine. The configuration of the runtime is loaded from .NET configuration files if available.
        /// </summary>
        /// <param name="setupInitializer">
        /// Delegate that receives an instance of IronRuby's <see cref="LanguageSetup"/> loaded from the .NET configuration
        /// and can set its properties before it is used for runtime configuration. 
        /// If no IronRuby setup information is found in the .NET configuration a default one is created and passed to the initializer.
        /// </param>
        /// <returns>A new IronRuby engine.</returns>
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
        /// <returns>The IronRuby setup object.</returns>
        public static LanguageSetup/*!*/ CreateRubySetup() {
            return new LanguageSetup(
                typeof(RubyContext).AssemblyQualifiedName,
                RubyContext.IronRubyDisplayName,
                RubyContext.IronRubyNames.Split(';'),
                RubyContext.IronRubyFileExtensions.Split(';')
            );
        }

        /// <summary>
        /// Creates a default IronRuby setup and passes it to the given delegate for initialization.
        /// </summary>
        /// <param name="initializer">
        /// A delegate that receives a fresh instance of IronRuby's <see cref="LanguageSetup"/> 
        /// and can set its properties before it is used for runtime configuration. 
        /// </param>
        /// <returns>The IronRuby setup object initialized by <paramref name="initializer"/>.</returns>
        public static LanguageSetup/*!*/ CreateRubySetup(Action<LanguageSetup/*!*/>/*!*/ initializer) {
            ContractUtils.RequiresNotNull(initializer, "initializer");

            var setup = CreateRubySetup();
            initializer(setup);
            return setup;
        }

        /// <summary>
        /// Retrieves an IronRuby engine from a given script runtime.
        /// </summary>
        /// <param name="runtime">The runtime to get the engine from.</param>
        /// <returns>An IronRuby engine from the specified runtime.</returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="runtime"/> is not set up to run IronRuby. 
        /// The <see cref="ScriptRuntimeSetup"/> doesn't contain IronRuby's <see cref="ScriptLanguageSetup"/>.
        /// </exception>
        public static ScriptEngine/*!*/ GetEngine(ScriptRuntime/*!*/ runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            return runtime.GetEngineByTypeName(typeof(RubyContext).AssemblyQualifiedName);
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
        /// <summary>
        /// Loads a script file using the semantics of Kernel#require method.
        /// </summary>
        /// <param name="engine">The Ruby engine.</param>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>Whether the file has already been required.</returns>
        /// <remarks>
        /// If a relative path is given the current search paths are used to locate the script file.
        /// The current search paths could be read and modified via <see cref="ScriptEngine.GetSearchPaths"/> and <see cref="ScriptEngine.SetSearchPaths"/>,
        /// respectively.
        /// </remarks>
        public static bool RequireFile(this ScriptEngine/*!*/ engine, string/*!*/ path) {
            return engine.GetService<RubyService>(engine).RequireFile(path);
        }

        /// <summary>
        /// Loads a script file using the semantics of Kernel#require method.
        /// The script is executed within the context of a given <see cref="ScriptScope"/>.
        /// </summary>
        /// <param name="engine">The Ruby engine.</param>
        /// <param name="path">The path to the file to load.</param>
        /// <param name="scope">The scope to use for the script execution.</param>
        /// <returns>Whether the file has already been required.</returns>
        /// <remarks>
        /// If a relative path is given the current search paths are used to locate the script file.
        /// The current search paths could be read and modified via <see cref="ScriptEngine.GetSearchPaths"/> and <see cref="ScriptEngine.SetSearchPaths"/>,
        /// respectively.
        /// </remarks>
        public static bool RequireFile(this ScriptEngine/*!*/ engine, string/*!*/ path, ScriptScope/*!*/ scope) {
            return engine.GetService<RubyService>(engine).RequireFile(path, scope);
        }

        /// <summary>
        /// Retrieves an IronRuby engine from this script runtime.
        /// </summary>
        /// <param name="runtime">The runtime to get the engine from.</param>
        /// <returns>An existing IronRuby engine.</returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="runtime"/> is not set up to run IronRuby. 
        /// The <see cref="ScriptRuntimeSetup"/> doesn't contain IronRuby's <see cref="ScriptLanguageSetup"/>.
        /// </exception>
        public static ScriptEngine/*!*/ GetRubyEngine(this ScriptRuntime/*!*/ runtime) {
            return Ruby.GetEngine(runtime);
        }

        /// <summary>
        /// Retrieves IronRuby setup from the script runtime setup.
        /// </summary>
        /// <param name="runtimeSetup">Runtime setup.</param>
        /// <returns>IronRuby setup or <c>null</c> if the runtime setup doesn't contain one.</returns>
        public static LanguageSetup GetRubySetup(this ScriptRuntimeSetup/*!*/ runtimeSetup) {
            int index = Ruby.IndexOfRubySetup(runtimeSetup);
            return (index != -1) ? runtimeSetup.LanguageSetups[index] : null;
        }

        /// <summary>
        /// Adds a new IronRuby setup into the runtime setup unless already included.
        /// </summary>
        /// <param name="runtimeSetup">The <see cref="ScriptRuntimeSetup"/> to update.</param>
        /// <returns>The new setup or the existing setup object.</returns>
        public static LanguageSetup/*!*/ AddRubySetup(this ScriptRuntimeSetup/*!*/ runtimeSetup) {
            return AddRubySetup(runtimeSetup, null);
        }

        /// <summary>
        /// Adds a new IronRuby setup into the given runtime setup unless already included.
        /// </summary>
        /// <param name="runtimeSetup">The <see cref="ScriptRuntimeSetup"/> to update.</param>
        /// <param name="newSetupInitializer">
        /// If not <c>null</c>, <paramref name="newSetupInitializer"/> is used to initialize the new IronRuby <see cref="LanguageSetup"/>.
        /// </param>
        /// <returns>The new setup initialized by <paramref name="newSetupInitializer"/> or the existing setup object.</returns>
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
