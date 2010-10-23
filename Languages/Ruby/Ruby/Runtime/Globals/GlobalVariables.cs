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

using IronRuby.Compiler;

namespace IronRuby.Runtime {
    internal enum GlobalVariableId {
        MatchData,
        EntireMatch,
        MatchLastGroup,
        PreMatch,
        PostMatch,

        CurrentException,
        CurrentExceptionBacktrace,
        ItemSeparator,
        StringSeparator,
        InputSeparator,
        OutputSeparator,
        LastInputLine,
        InputFileName,
        CommandLineProgramPath,
        CommandLineArguments,
        LoadPath,
        LoadedFiles,
        InputContent,
        OutputStream,
        LastInputLineNumber,

        InputStream,
        ErrorOutputStream,
        SafeLevel,
        Verbose,
        KCode,
        ChildProcessExitStatus
    }

    public static class GlobalVariables {
        // regex:
        public static readonly GlobalVariable MatchData = new SpecialGlobalVariableInfo(GlobalVariableId.MatchData);
        public static readonly GlobalVariable EntireMatch = new SpecialGlobalVariableInfo(GlobalVariableId.EntireMatch);
        public static readonly GlobalVariable MatchLastGroup = new SpecialGlobalVariableInfo(GlobalVariableId.MatchLastGroup);
        public static readonly GlobalVariable PreMatch = new SpecialGlobalVariableInfo(GlobalVariableId.PreMatch);
        public static readonly GlobalVariable PostMatch = new SpecialGlobalVariableInfo(GlobalVariableId.PostMatch);

        public static readonly GlobalVariable CurrentException = new SpecialGlobalVariableInfo(GlobalVariableId.CurrentException);
        public static readonly GlobalVariable CurrentExceptionBacktrace = new SpecialGlobalVariableInfo(GlobalVariableId.CurrentExceptionBacktrace);

        public static readonly GlobalVariable CommandLineArguments = new SpecialGlobalVariableInfo(GlobalVariableId.CommandLineArguments);
        public static readonly GlobalVariable LastInputLine = new SpecialGlobalVariableInfo(GlobalVariableId.LastInputLine);
        public static readonly GlobalVariable InputFileName = new SpecialGlobalVariableInfo(GlobalVariableId.InputFileName);
        public static readonly GlobalVariable CommandLineProgramPath = new SpecialGlobalVariableInfo(GlobalVariableId.CommandLineProgramPath);
        public static readonly GlobalVariable InputContent = new SpecialGlobalVariableInfo(GlobalVariableId.InputContent);
        public static readonly GlobalVariable LastInputLineNumber = new SpecialGlobalVariableInfo(GlobalVariableId.LastInputLineNumber);

        public static readonly GlobalVariable InputSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.InputSeparator);
        public static readonly GlobalVariable ItemSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.ItemSeparator);
        public static readonly GlobalVariable StringSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.StringSeparator);
        public static readonly GlobalVariable OutputSeparator = new SpecialGlobalVariableInfo(GlobalVariableId.OutputSeparator);

        public static readonly GlobalVariable LoadPath = new SpecialGlobalVariableInfo(GlobalVariableId.LoadPath);
        public static readonly GlobalVariable LoadedFiles = new SpecialGlobalVariableInfo(GlobalVariableId.LoadedFiles);

        public static readonly GlobalVariable OutputStream = new SpecialGlobalVariableInfo(GlobalVariableId.OutputStream);
        public static readonly GlobalVariable InputStream = new SpecialGlobalVariableInfo(GlobalVariableId.InputStream);
        public static readonly GlobalVariable ErrorOutputStream = new SpecialGlobalVariableInfo(GlobalVariableId.ErrorOutputStream);

        public static readonly GlobalVariable SafeLevel = new SpecialGlobalVariableInfo(GlobalVariableId.SafeLevel);
        public static readonly GlobalVariable Verbose = new SpecialGlobalVariableInfo(GlobalVariableId.Verbose);
        public static readonly GlobalVariable KCode = new SpecialGlobalVariableInfo(GlobalVariableId.KCode);
        public static readonly GlobalVariable ChildProcessExitStatus = new SpecialGlobalVariableInfo(GlobalVariableId.ChildProcessExitStatus);

        //
        // Defines variables backed by a field on RubyContext or Scope and variables that derived from them.
        // Also variables that need type check on assignment are defined here.
        //
        // Other variables are simply looked up in the dictionary on RubyContext. All uses of such variables in libraries 
        // go thru alias table so they don't need a direct reference from RubyContext.
        //
        internal static void DefineVariablesNoLock(RubyContext/*!*/ context) {
            // scope based variables (regex and input line):
            context.DefineGlobalVariableNoLock(Symbols.MatchData, MatchData);
            context.DefineGlobalVariableNoLock(Symbols.EntireMatch, EntireMatch);
            context.DefineGlobalVariableNoLock(Symbols.MatchLastGroup, MatchLastGroup);
            context.DefineGlobalVariableNoLock(Symbols.PreMatch, PreMatch);
            context.DefineGlobalVariableNoLock(Symbols.PostMatch, PostMatch);
            context.DefineGlobalVariableNoLock(Symbols.LastInputLine, LastInputLine);

            // directly accessed variables provided by execution context:
            context.DefineGlobalVariableNoLock(Symbols.CommandLineProgramPath, CommandLineProgramPath);
            context.DefineGlobalVariableNoLock(Symbols.CurrentException, CurrentException);
            context.DefineGlobalVariableNoLock(Symbols.CurrentExceptionBacktrace, CurrentExceptionBacktrace);
            context.DefineGlobalVariableNoLock(Symbols.CommandLineArguments, CommandLineArguments);
            context.DefineGlobalVariableNoLock(Symbols.InputSeparator, InputSeparator);
            context.DefineGlobalVariableNoLock(Symbols.ItemSeparator, ItemSeparator);
            context.DefineGlobalVariableNoLock(Symbols.StringSeparator, StringSeparator);
            context.DefineGlobalVariableNoLock(Symbols.OutputSeparator, OutputSeparator);
            context.DefineGlobalVariableNoLock(Symbols.InputContent, InputContent);
            context.DefineGlobalVariableNoLock(Symbols.OutputStream, OutputStream);
            context.DefineGlobalVariableNoLock(Symbols.LoadedFiles, LoadedFiles);
            context.DefineGlobalVariableNoLock(Symbols.LoadPath, LoadPath);
            context.DefineGlobalVariableNoLock(Symbols.LastInputLineNumber, LastInputLineNumber);
            context.DefineGlobalVariableNoLock(Symbols.ChildProcessExitStatus, ChildProcessExitStatus);
        }    
    }       
}
