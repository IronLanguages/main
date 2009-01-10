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
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using IronRuby;
using IronRuby.Hosting;
using IronRuby.Runtime;
using System.Dynamic.Utils;

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

    [STAThread]
    [RubyStackTraceHidden]
    static int Main(string[] args) {
        return new RubyConsoleHost().Run(args);
    }
}