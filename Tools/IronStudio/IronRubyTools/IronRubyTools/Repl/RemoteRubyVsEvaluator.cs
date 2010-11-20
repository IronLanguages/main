/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Microsoft.IronRubyTools.Library.Repl;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.Scripting.Hosting;
using System.IO;
using IronRuby;
using Microsoft.Scripting;
using IronRuby.Runtime;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Scripting.Utils;

namespace Microsoft.IronRubyTools.Repl {
    internal sealed class RemoteRubyVsEvaluator : RemoteRubyEvaluator {
        // Constructed via reflection when deserialized from the registry.
        public RemoteRubyVsEvaluator() {
        }

        public override void Reset() {
            base.Reset();
            Initialize();
        }

        public void Initialize() {
            string filename, dir;
            if (CommonPackage.TryGetStartupFileAndDirectory(out filename, out dir)) {
                RemoteScriptFactory.SetCurrentDirectory(dir);
            }
        }

        protected override bool ShouldEvaluateForCompletion(string source) {
            switch (IronRubyToolsPackage.Instance.OptionsPage.ReplIntellisenseMode) {
                case ReplIntellisenseMode.AlwaysEvaluate: return true;
                case ReplIntellisenseMode.DontEvaluateCalls: return base.ShouldEvaluateForCompletion(source);
                case ReplIntellisenseMode.NeverEvaluate: return false;
                default: throw new InvalidOperationException();
            }
        }

        private static LanguageSetup GetSetupByName(ScriptRuntimeSetup/*!*/ setup, string/*!*/ name) {
            foreach (var languageSetup in setup.LanguageSetups) {
                if (languageSetup.Names.IndexOf(name) >= 0) {
                    return languageSetup;
                }
            }
            return null;
        }

        private static IList<string>/*!*/ NormalizePaths(string/*!*/ dir, IList<string> paths) {
            if (paths == null) {
                return ArrayUtils.EmptyStrings;
            }

            var result = new List<string>(paths.Count);
            foreach (string path in paths) {
                try {
                    result.Add(Path.GetFullPath(Path.Combine(dir, path)));
                } catch {
                    // nop
                }
            }
            return result;
        }

        protected override ScriptRuntime/*!*/ CreateRuntime() {
            string root = IronRubyToolsPackage.Instance.IronRubyBinPath;
            string configPath = IronRubyToolsPackage.Instance.IronRubyExecutable + ".config";
            LanguageSetup existingRubySetup = null;
            LanguageSetup rubySetup = Ruby.CreateRubySetup();

            if (File.Exists(configPath)) {
                try {
                    existingRubySetup = GetSetupByName(ScriptRuntimeSetup.ReadConfiguration(configPath), "IronRuby");
                } catch {
                    // TODO: report the error
                }
            }

            if (existingRubySetup != null) {
                var options = new RubyOptions(existingRubySetup.Options);
                rubySetup.Options["StandardLibraryPath"] = NormalizePaths(root, new[] { options.StandardLibraryPath })[0];
                rubySetup.Options["RequiredPaths"] = NormalizePaths(root, options.RequirePaths);
                rubySetup.Options["SearchPaths"] = NormalizePaths(root, options.SearchPaths);
            }

            var runtimeSetup = new ScriptRuntimeSetup();
            runtimeSetup.LanguageSetups.Add(rubySetup);
            return RemoteScriptFactory.CreateRuntime(runtimeSetup);
        }
    }
}
