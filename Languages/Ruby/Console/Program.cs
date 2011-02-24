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
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Hosting;
using IronRuby.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Hosting.Shell;
using System.IO;

internal sealed class Host : RubyConsoleHost {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private static void OnCancelKey(ConsoleCancelEventArgs ev, RubyContext context, Thread mainThread) {
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
            OnCancelKey(e, context, mainThread); 
        };

        return console;
    }

    protected override ConsoleOptions ParseOptions(string/*!*/[]/*!*/ args, ScriptRuntimeSetup/*!*/ runtimeSetup, LanguageSetup/*!*/ languageSetup) {
        var rubyOptions = (RubyConsoleOptions)base.ParseOptions(args, runtimeSetup, languageSetup);
        if (rubyOptions == null) {
            return null;
        }

        if (rubyOptions.ChangeDirectory != null) {
            Environment.CurrentDirectory = rubyOptions.ChangeDirectory;
        }

        if (rubyOptions.DisplayVersion && (rubyOptions.Command != null || rubyOptions.FileName != null)) {
            Console.WriteLine(RubyContext.MakeDescriptionString(), Style.Out);
        }

        return rubyOptions;
    }

    protected override void ReportInvalidOption(InvalidOptionException e) {
        Console.Error.WriteLine(e.Message);
    }

    [STAThread]
    [RubyStackTraceHidden]
    static int Main(string[] args) {
        return new Host().Run(args);
    }
}
