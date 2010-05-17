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

using System;
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

internal sealed class RubyConsoleHost : ConsoleHost {

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

    private static void OnCancelKey(object sender, ConsoleCancelEventArgs ev, RubyContext context, Thread mainThread) {
        if (ev.SpecialKey == ConsoleSpecialKey.ControlC) {
            ev.Cancel = true;
            Action handler = context.InterruptSignalHandler;

            if (handler != null) {
                try {
                    handler();
                } catch (Exception e) {
                    RubyUtils.RaiseAsyncException(mainThread, e);
                }
            }
        }
    }

    protected override IConsole CreateConsole(ScriptEngine engine, CommandLine commandLine, ConsoleOptions options) {
        IConsole console = base.CreateConsole(engine, commandLine, options);

        Thread mainThread = Thread.CurrentThread;
        RubyContext context = (RubyContext)HostingHelpers.GetLanguageContext(engine);
        context.InterruptSignalHandler = delegate() { RubyUtils.RaiseAsyncException(mainThread, new Interrupt()); };
        ((BasicConsole)console).ConsoleCancelEventHandler = delegate(object sender, ConsoleCancelEventArgs e) {
            OnCancelKey(sender, e, context, mainThread); 
        };

        return console;
    }

    private static void SetHome() {
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
    }

    [STAThread]
    [RubyStackTraceHidden]
    static int Main(string[] args) {
        SetHome();
        return new RubyConsoleHost().Run(args);
    }
}
