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

using System;
using System.IO;
using System.Threading;
using IronRuby;
using IronRuby.Builtins;
using IronRuby.Hosting;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Utils;

namespace IronRuby.Hosting {
    public abstract class RubyConsoleHost : ConsoleHost {
        protected RubyConsoleHost() {
            SetHomeEnvironmentVariable();
        }

        protected override Type Provider {
            get { return typeof(RubyContext); }
        }

        protected override CommandLine/*!*/ CreateCommandLine() {
            return new RubyCommandLine();
        }

        protected override OptionsParser/*!*/ CreateOptionsParser() {
            return new RubyOptionsParser();
        }

        protected override LanguageSetup CreateLanguageSetup() {
            return Ruby.CreateRubySetup();
        }

        protected override ConsoleOptions ParseOptions(string[] args, ScriptRuntimeSetup runtimeSetup, LanguageSetup languageSetup) {
#if !SILVERLIGHT
            languageSetup.Options["ApplicationBase"] = AppDomain.CurrentDomain.BaseDirectory;
#endif
            return base.ParseOptions(args, runtimeSetup, languageSetup);
        }

        private static void SetHomeEnvironmentVariable() {
#if !SILVERLIGHT
            try {
                PlatformAdaptationLayer platform = PlatformAdaptationLayer.Default;
                string homeDir = RubyUtils.GetHomeDirectory(platform);
                platform.SetEnvironmentVariable("HOME", homeDir);
            } catch (System.Security.SecurityException e) {
                // Ignore EnvironmentPermission exception
                if (e.PermissionType != typeof(System.Security.Permissions.EnvironmentPermission)) {
                    throw;
                }
            }
#endif
        }
    }
}
