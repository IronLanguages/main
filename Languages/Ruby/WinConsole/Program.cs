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

internal sealed class Host : RubyConsoleHost {
    protected override int OptionsParsed(OptionsParser parser) {
        var rubyOptions = ((RubyOptionsParser)parser).ConsoleOptions;
        if (rubyOptions.ChangeDirectory != null) {
            Environment.CurrentDirectory = rubyOptions.ChangeDirectory;
        }

        if (rubyOptions.Introspection || rubyOptions.Command == null && rubyOptions.FileName == null) {
            PrintHelp();
            return 1;
        }

        return 0;
    }

    protected override int InvalidOption(InvalidOptionException e) {
        MessageBox.Show(e.Message);
        return 1;
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