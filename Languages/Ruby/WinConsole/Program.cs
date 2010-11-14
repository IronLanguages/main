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
using IronRuby.Hosting;
using IronRuby.Runtime;
using System.Windows.Forms;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Hosting;

internal sealed class Host : RubyConsoleHost {
    protected override ConsoleOptions ParseOptions(string/*!*/[]/*!*/ args, ScriptRuntimeSetup/*!*/ runtimeSetup, LanguageSetup/*!*/ languageSetup) {
        var rubyOptions = (RubyConsoleOptions)base.ParseOptions(args, runtimeSetup, languageSetup);
        if (rubyOptions == null) {
            return null;
        }

        if (rubyOptions.ChangeDirectory != null) {
            Environment.CurrentDirectory = rubyOptions.ChangeDirectory;
        }

        if (rubyOptions.Introspection || rubyOptions.Command == null && rubyOptions.FileName == null) {
            PrintHelp();
            return null;
        }

        return rubyOptions;
    }

    protected override void ReportInvalidOption(InvalidOptionException e) {
        MessageBox.Show(e.Message);
    }

    [STAThread]
    [RubyStackTraceHidden]
    static int Main(string[] args) {
        return new Host().Run(args);
    }

    protected override void PrintHelp() {
        MessageBox.Show(GetHelp(), "IronRuby Window Console Help");
    }
}