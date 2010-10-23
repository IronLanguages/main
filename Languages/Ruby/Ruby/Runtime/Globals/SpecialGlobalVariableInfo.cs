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
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using System.Text;
using System.Diagnostics;

namespace IronRuby.Runtime {
    internal sealed class SpecialGlobalVariableInfo : GlobalVariable {
        private readonly GlobalVariableId _id;

        internal SpecialGlobalVariableInfo(GlobalVariableId id) {
            _id = id;
        }

        public override object GetValue(RubyContext/*!*/ context, RubyScope scope) {
            switch (_id) {
                
                // regular expressions:
                case GlobalVariableId.MatchData:
                    return (scope != null) ? scope.GetInnerMostClosureScope().CurrentMatch : null;

                case GlobalVariableId.MatchLastGroup:
                    return (scope != null) ? scope.GetInnerMostClosureScope().GetCurrentMatchLastGroup() : null;

                case GlobalVariableId.PreMatch:
                    return (scope != null) ? scope.GetInnerMostClosureScope().GetCurrentPreMatch() : null;

                case GlobalVariableId.PostMatch:
                    return (scope != null) ? scope.GetInnerMostClosureScope().GetCurrentPostMatch() : null;

                case GlobalVariableId.EntireMatch:
                    return (scope != null) ? scope.GetInnerMostClosureScope().GetCurrentMatchGroup(0) : null;


                // exceptions:
                case GlobalVariableId.CurrentException:
                    return context.CurrentException;

                case GlobalVariableId.CurrentExceptionBacktrace:
                    return context.GetCurrentExceptionBacktrace();


                // input:
                case GlobalVariableId.InputContent:
                    return context.InputProvider.Singleton;

                case GlobalVariableId.InputFileName:
                    return context.InputProvider.CurrentFileName;

                case GlobalVariableId.LastInputLine:
                    return (scope != null) ? scope.GetInnerMostClosureScope().LastInputLine : null;

                case GlobalVariableId.LastInputLineNumber:
                    return context.InputProvider.LastInputLineNumber;

                case GlobalVariableId.CommandLineArguments:
                    return context.InputProvider.CommandLineArguments;


                // output:
                case GlobalVariableId.OutputStream:
                    return context.StandardOutput;

                case GlobalVariableId.ErrorOutputStream:
                    return context.StandardErrorOutput;

                case GlobalVariableId.InputStream:
                    return context.StandardInput;


                // separators:
                case GlobalVariableId.InputSeparator:
                    return context.InputSeparator;

                case GlobalVariableId.OutputSeparator:
                    return context.OutputSeparator;

                case GlobalVariableId.StringSeparator:
                    return context.StringSeparator;

                case GlobalVariableId.ItemSeparator:
                    return context.ItemSeparator;

                
                // loader:
                case GlobalVariableId.LoadPath:
                    return context.Loader.LoadPaths;

                case GlobalVariableId.LoadedFiles:
                    return context.Loader.LoadedFiles;


                // misc:
                case GlobalVariableId.SafeLevel:
                    return context.CurrentSafeLevel;

                case GlobalVariableId.Verbose:
                    return context.Verbose;

                case GlobalVariableId.KCode:
                    context.ReportWarning("variable $KCODE is no longer effective");
                    return null;

                case GlobalVariableId.ChildProcessExitStatus:
                    return context.ChildProcessExitStatus;

                case GlobalVariableId.CommandLineProgramPath:
                    return context.CommandLineProgramPath;

                default:
                    throw Assert.Unreachable;
            }
        }

        public override void SetValue(RubyContext/*!*/ context, RubyScope scope, string/*!*/ name, object value) {
            switch (_id) {
                // regex:
                case GlobalVariableId.MatchData:
                    if (scope == null) {
                        throw ReadOnlyError(name);
                    }

                    scope.GetInnerMostClosureScope().CurrentMatch = (value != null) ? RequireType<MatchData>(value, name, "MatchData") : null;
                    return;

                case GlobalVariableId.MatchLastGroup:
                case GlobalVariableId.PreMatch:
                case GlobalVariableId.PostMatch:
                case GlobalVariableId.EntireMatch:
                    throw ReadOnlyError(name);
                
                
                // exceptions:
                case GlobalVariableId.CurrentException:
                    context.SetCurrentException(value);
                    return;

                case GlobalVariableId.CurrentExceptionBacktrace:
                    context.SetCurrentExceptionBacktrace(value);
                    return;


                // input:
                case GlobalVariableId.LastInputLine:
                    if (scope == null) {
                        throw ReadOnlyError(name);
                    }
                    scope.GetInnerMostClosureScope().LastInputLine = value;
                    return;

                case GlobalVariableId.LastInputLineNumber:
                    context.InputProvider.LastInputLineNumber = RequireType<int>(value, name, "Fixnum");
                    return;

                case GlobalVariableId.CommandLineArguments:
                case GlobalVariableId.InputFileName:
                    throw ReadOnlyError(name);

                // output:
                case GlobalVariableId.OutputStream:
                    context.StandardOutput = RequireWriteProtocol(context, value, name);
                    return;

                case GlobalVariableId.ErrorOutputStream:
                    context.StandardErrorOutput = RequireWriteProtocol(context, value, name);
                    break;

                case GlobalVariableId.InputStream:
                    context.StandardInput = value;
                    return;

                // separators:
                case GlobalVariableId.InputContent:
                    throw ReadOnlyError(name);

                case GlobalVariableId.InputSeparator:
                    context.InputSeparator = (value != null) ? RequireType<MutableString>(value, name, "String") : null;
                    return;

                case GlobalVariableId.OutputSeparator:
                    context.OutputSeparator = (value != null) ? RequireType<MutableString>(value, name, "String") : null;
                    return;

                case GlobalVariableId.StringSeparator:
                    // type not enforced:
                    context.StringSeparator = value;
                    return;

                case GlobalVariableId.ItemSeparator:
                    context.ItemSeparator = (value != null) ? RequireType<MutableString>(value, name, "String") : null;
                    return;


                // loader:
                case GlobalVariableId.LoadedFiles:
                case GlobalVariableId.LoadPath:
                    throw ReadOnlyError(name);


                // misc:
                case GlobalVariableId.SafeLevel:
                    context.SetSafeLevel(RequireType<int>(value, name, "Fixnum"));
                    return;

                case GlobalVariableId.Verbose:
                    context.Verbose = value;
                    return;

                case GlobalVariableId.CommandLineProgramPath:
                    context.CommandLineProgramPath = (value != null) ? RequireType<MutableString>(value, name, "String") : null;
                    return;
                
                case GlobalVariableId.KCode:
                    context.ReportWarning("variable $KCODE is no longer effective");
                    return;

                case GlobalVariableId.ChildProcessExitStatus:
                    throw ReadOnlyError(name);
                    
                default:
                    throw Assert.Unreachable;
            }
        }
    
        private object RequireWriteProtocol(RubyContext/*!*/ context, object value, string/*!*/ variableName) {
            if (!context.RespondTo(value, "write")) {
                throw RubyExceptions.CreateTypeError(String.Format("${0} must have write method, {1} given", variableName, context.GetClassDisplayName(value)));
            }

            return value;
        }
    }
}
