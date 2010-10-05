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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;

namespace IronRuby.Runtime {
    internal sealed class RubyStackTraceBuilder {
        internal const string TopLevelMethodName = "#";

        private readonly RubyArray/*!*/ _trace;
        private readonly bool _hasFileAccessPermission;
        private readonly bool _exceptionDetail;
        private readonly RubyEncoding/*!*/ _encoding;
        private IList<InterpretedFrameInfo> _interpretedFrames;
        private int _interpretedFrameIndex;
        private string _nextFrameMethodName;

        private RubyStackTraceBuilder(RubyContext/*!*/ context) {
            _hasFileAccessPermission = DetectFileAccessPermissions();
            _exceptionDetail = context.Options.ExceptionDetail;
            _encoding = context.GetPathEncoding();
            _trace = new RubyArray();
        }
        
        internal RubyStackTraceBuilder(RubyContext/*!*/ context, Exception/*!*/ exception, StackTrace catchSiteTrace, bool isCatchSiteInterpreted) 
            : this(context) {
            // Compiled trace: contains frames starting with the throw site up to the first filter/catch that the exception was caught by:
            StackTrace throwSiteTrace = GetClrStackTrace(exception);
            _interpretedFrames = InterpretedFrame.GetExceptionStackTrace(exception);

            AddBacktrace(throwSiteTrace.GetFrames(), 0, false);

            // Compiled trace: contains frames above and including the first Ruby filter/catch site that the exception was caught by:
            if (catchSiteTrace != null) {
                // Interpreted: skip one interpreter Run method frame - is was already matched with the last processed interpreted frame:
                // Compiled: skip one frame - the catch-site frame is already included
                AddBacktrace(catchSiteTrace.GetFrames(), isCatchSiteInterpreted ? 0 : 1, isCatchSiteInterpreted);
            }
        }

        internal RubyStackTraceBuilder(RubyContext/*!*/ context, int skipFrames)
            : this(context) {
            var trace = GetClrStackTrace(null);

            _interpretedFrames = InterpretedFrame.CurrentFrame.Value != null ?
                new List<InterpretedFrameInfo>(InterpretedFrame.CurrentFrame.Value.GetStackTraceDebugInfo()) :
                null;

            AddBacktrace(trace.GetFrames(), skipFrames, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // CF
        internal static StackTrace GetClrStackTrace(Exception exception) {
#if SILVERLIGHT
            return exception != null ? new StackTrace(exception) : new StackTrace();
#else
            return exception != null ? new StackTrace(exception, true) : new StackTrace(true);
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // CF
        private static string GetFileName(StackFrame/*!*/ frame) {
            return frame.GetFileName();
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // CF
        private static string/*!*/ GetAssemblyName(Assembly/*!*/ assembly) {
            return assembly.GetName().Name;
        }

        private void InitializeInterpretedFrames() {
        }

        public RubyArray/*!*/ RubyTrace {
            get { return _trace; }
        }

        private const int NextFrameLine = Int32.MinValue;
        internal const string InterpretedCallSiteName = "CallSite.Target";

        private void AddBacktrace(IEnumerable<StackFrame> stackTrace, int skipFrames, bool skipInterpreterRunMethod) {
            if (stackTrace != null) {
                foreach (var frame in InterpretedFrame.GroupStackFrames(stackTrace)) {
                    string methodName, file;
                    int line;

                    if (_interpretedFrames != null && _interpretedFrameIndex < _interpretedFrames.Count
                        && InterpretedFrame.IsInterpretedFrame(frame.GetMethod())) {

                        if (skipInterpreterRunMethod) {
                            skipInterpreterRunMethod = false;
                            continue;
                        }

                        InterpretedFrameInfo info = _interpretedFrames[_interpretedFrameIndex++];

                        if (info.DebugInfo != null) {
                            file = info.DebugInfo.FileName;
                            line = info.DebugInfo.StartLine;
                        } else {
                            file = null;
                            line = 0;
                        }
                        methodName = info.MethodName;

                        // TODO: We need some more general way to recognize and parse non-Ruby interpreted frames
                        TryParseRubyMethodName(ref methodName, ref file, ref line);

                        if (methodName == InterpretedCallSiteName) {
                            // ignore ruby interpreted call sites
                            continue;
                        }
                    } else if (TryGetStackFrameInfo(frame, out methodName, out file, out line)) {
                        // special case: the frame will be added with the next frame's source info:
                        if (line == NextFrameLine) {
                            _nextFrameMethodName = methodName;
                            continue;
                        }
                    } else {
                        continue;
                    }

                    if (_nextFrameMethodName != null) {
                        if (skipFrames == 0) {
                            _trace.Add(MutableString.Create(FormatFrame(file, line, _nextFrameMethodName), _encoding));
                        } else {
                            skipFrames--;
                        }
                        _nextFrameMethodName = null;
                    }

                    if (skipFrames == 0) {
                        _trace.Add(MutableString.Create(FormatFrame(file, line, methodName), _encoding));
                    } else {
                        skipFrames--;
                    }
                }
            }
        }

        private static string/*!*/ FormatFrame(string file, int line, string methodName) {
            if (String.IsNullOrEmpty(methodName)) {
                return String.Format("{0}:{1}", file, line);
            } else {
                return String.Format("{0}:{1}:in `{2}'", file, line, methodName);
            }
        }

        private bool TryGetStackFrameInfo(StackFrame/*!*/ frame, out string/*!*/ methodName, out string/*!*/ fileName, out int line) {
            MethodBase method = frame.GetMethod();
            methodName = method.Name;

            fileName = (_hasFileAccessPermission) ? GetFileName(frame) : null;
            var sourceLine = line = PlatformAdaptationLayer.IsCompactFramework ? 0 : frame.GetFileLineNumber();

            if (TryParseRubyMethodName(ref methodName, ref fileName, ref line)) {
                if (sourceLine == 0) {
                    RubyMethodDebugInfo debugInfo;
                    if (RubyMethodDebugInfo.TryGet(method, out debugInfo)) {
                        var ilOffset = frame.GetILOffset();
                        if (ilOffset >= 0) {
                            var mappedLine = debugInfo.Map(ilOffset);
                            if (mappedLine != 0) {
                                line = mappedLine;
                            }
                        }
                    }
                }

                return true;
            } else if (method.IsDefined(typeof(RubyStackTraceHiddenAttribute), false)) {
                return false;
            } else {
                object[] attrs = method.GetCustomAttributes(typeof(RubyMethodAttribute), false);
                if (attrs.Length > 0) {
                    // Ruby library method:
                    // TODO: aliases
                    methodName = ((RubyMethodAttribute)attrs[0]).Name;

                    if (!_exceptionDetail) {
                        fileName = null;
                        line = NextFrameLine;
                    }

                    return true;
                } else if (_exceptionDetail || IsVisibleClrFrame(method)) {
                    // Visible CLR method:
                    if (String.IsNullOrEmpty(fileName)) {
                        if (method.DeclaringType != null) {
                            fileName = (_hasFileAccessPermission) ? GetAssemblyName(method.DeclaringType.Assembly) : null;
                            line = 0;
                        }
                    }
                    return true;
                } else {
                    // Invisible CLR method:
                    return false;
                }
            }
        }

        private static bool IsVisibleClrFrame(MethodBase/*!*/ method) {
            if (Microsoft.Scripting.Actions.DynamicSiteHelpers.IsInvisibleDlrStackFrame(method)) {
                return false;
            }

            Type type = method.DeclaringType;
            if (type != null) {
                if (type.Assembly == typeof(RubyOps).Assembly || type.Namespace != null && 
                    (type.Namespace.StartsWith("IronRuby.StandardLibrary", StringComparison.Ordinal) ||
                    type.Namespace.StartsWith("IronRuby.Builtins", StringComparison.Ordinal))) {
                    return false;
                }                
            }

            // TODO: check loaded assemblies?
            return true;
        }

        private const char NamePartsSeparator = ':';
        private const string RubyMethodPrefix = "\u2111\u211c:";
        private static int _Id = 0;
        internal const int MaxDebugModePathSize = 256; // PDB limit

        internal static string/*!*/ EncodeMethodName(string/*!*/ methodName, string sourcePath, SourceSpan location, bool debugMode) {
            return new StringBuilder().
                Append(RubyMethodPrefix).
                Append(methodName).
                Append(NamePartsSeparator).
                Append(location.IsValid ? location.Start.Line : 0).
                Append(NamePartsSeparator).
                Append(Interlocked.Increment(ref _Id)).
                Append(NamePartsSeparator).
                Append(debugMode && sourcePath != null && sourcePath.Length > MaxDebugModePathSize ? sourcePath.Substring(0, MaxDebugModePathSize) : sourcePath).
                ToString();
        }

        // \u2111\u211c:{method-name}:{line-number}:{unique-id}:{file-name}
        internal static bool TryParseRubyMethodName(ref string methodName, ref string fileName, ref int line) {
            if (methodName != null && methodName.StartsWith(RubyMethodPrefix, StringComparison.Ordinal)) {
                string encoded = methodName;

                // method name:
                int s = RubyMethodPrefix.Length;
                int e = encoded.IndexOf(NamePartsSeparator, s);
                if (e < 0) {
                    return false;
                }
                methodName = encoded.Substring(s, e - s);
                if (methodName == TopLevelMethodName) {
                    methodName = null;
                }

                // line number:
                s = e + 1;
                e = encoded.IndexOf(NamePartsSeparator, s);
                if (e < 0) {
                    return false;
                }
                if (line == 0 && !Int32.TryParse(encoded.Substring(s, e - s), out line)) {
                    return false;
                }

                // file name:
                s = e + 1;
                e = encoded.IndexOf(NamePartsSeparator, s);
                if (e < 0) {
                    return false;
                }
                
                if (fileName == null) {
                    fileName = encoded.Substring(e + 1);
                }
                return true;
            }
            return false;
        }

        private static string ParseRubyMethodName(string/*!*/ lambdaName) {
            if (!lambdaName.StartsWith(RubyMethodPrefix, StringComparison.Ordinal)) {
                return lambdaName;
            }

            int nameEnd = lambdaName.IndexOf(NamePartsSeparator, RubyMethodPrefix.Length);
            string name = lambdaName.Substring(RubyMethodPrefix.Length, nameEnd - RubyMethodPrefix.Length);
            return (name != TopLevelMethodName) ? name : null;
        }

        // TODO: partial trust
        private static bool DetectFileAccessPermissions() {
#if SILVERLIGHT
                return false;
#else
            try {
                new FileIOPermission(PermissionState.Unrestricted).Demand();
                return true;
            } catch (SecurityException) {
                return false;
            }
#endif
        }
    }
}
