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
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Text;

namespace IronRuby.Hosting {
   
    /// <summary>
    /// A simple Ruby command-line should mimic the standard irb.exe
    /// </summary>
    public class RubyCommandLine : CommandLine {
        public RubyCommandLine() {
        }

        internal new RubyConsoleOptions Options {
            get { return (RubyConsoleOptions)base.Options; }
        }

        protected override string Logo {
            get { return GetLogo(); }
        }

        public static string GetLogo() {
            return String.Format(CultureInfo.InvariantCulture,
                "IronRuby {1} on {2}{0}Copyright (c) Microsoft Corporation. All rights reserved.{0}{0}",
                Environment.NewLine, RubyContext.IronRubyVersion, RubyContext.MakeRuntimeDesriptionString());
        }

        protected override int? TryInteractiveAction() {
            try {
                return base.TryInteractiveAction();
            } catch (ThreadAbortException e) {
                Exception visibleException = RubyUtils.GetVisibleException(e);
                if (visibleException == e || visibleException == null) {
                    throw;
                } else {
                    throw visibleException;
                }
            } catch (SystemExit e) {
                return e.Status;
            }
        }

        // overridden to set the default encoding to -KX
        protected override int RunFile(string fileName) {
            return RunFile(Engine.CreateScriptSourceFromFile(RubyUtils.CanonicalizePath(fileName), GetSourceCodeEncoding()));
        }

        protected override ScriptCodeParseResult GetCommandProperties(string code) {
            return CreateCommandSource(code, SourceCodeKind.InteractiveCode, "(ir)").GetCodeProperties(Engine.GetCompilerOptions(ScriptScope));
        }

        protected override void ExecuteCommand(string/*!*/ command) {
            ExecuteCommand(CreateCommandSource(command, SourceCodeKind.InteractiveCode, "(ir)"));
        }

        protected override int RunCommand(string/*!*/ command) {
            return RunFile(CreateCommandSource(command, SourceCodeKind.Statements, "-e"));
        }

        private ScriptSource/*!*/ CreateCommandSource(string/*!*/ command, SourceCodeKind kind, string/*!*/ sourceUnitId) {
#if SILVERLIGHT
            return Engine.CreateScriptSourceFromString(command, kind);
#else
            var encoding = GetSourceCodeEncoding();
            return Engine.CreateScriptSource(new BinaryContentProvider(encoding.GetBytes(command)), sourceUnitId, encoding, kind);
#endif
        }

        private Encoding/*!*/ GetSourceCodeEncoding() {
            return (((RubyContext)Language).RubyOptions.DefaultEncoding ?? RubyEncoding.Ascii).Encoding;
        }
        
        protected override Scope/*!*/ CreateScope() {
            Scope scope = base.CreateScope();
            RubyOps.ScopeSetMember(scope, "iron_ruby", Engine);
            return scope;
        }

        protected override void UnhandledException(Exception e) {
            // Kernel#at_exit can access $!. So we need to publish the uncaught exception
            ((RubyContext)Language).CurrentException = e;

            base.UnhandledException(e);
        }

        protected override void Shutdown() {
            try {
                Engine.Runtime.Shutdown();
            } catch (SystemExit e) {
                // Kernel#at_exit runs during shutdown, and it can set the exitcode by calling exit
                ExitCode = e.Status;
            }
        }
    }
}
