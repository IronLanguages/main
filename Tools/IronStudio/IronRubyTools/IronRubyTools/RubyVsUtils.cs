/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.IronRubyTools.Intellisense;
using Microsoft.IronRubyTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32.SafeHandles;
using Microsoft.IronStudio;

namespace Microsoft.IronRubyTools {
    internal static class RubyVsUtils {
        private sealed class GenericWaitHandle : WaitHandle {
            public GenericWaitHandle(IntPtr handle) {
                SafeWaitHandle = new SafeWaitHandle(handle, false);
            }
        }

        internal static WaitHandle/*!*/  GetWaitHandle(this Process/*!*/ process) {
            return new GenericWaitHandle(process.Handle);
        }

        internal static RubyProjectNode GetRubyProject(this EnvDTE.Project project) {
            return project.GetCommonProject() as RubyProjectNode;
        }

        internal static IVsOutputWindowPane GetOrCreateOutputPane(string/*!*/ name, Guid guid, bool clearWithSolution = false) {
            IVsOutputWindowPane outputPane = null;
            IVsOutputWindow output = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));
            
            if (ErrorHandler.Failed(output.GetPane(ref guid, out outputPane))) {
                if (!ErrorHandler.Failed(output.CreatePane(ref guid, name, Convert.ToInt32(true), Convert.ToInt32(clearWithSolution)))) {
                    output.GetPane(ref guid, out outputPane);
                }
            }

            Debug.Assert(outputPane != null);
            return outputPane;
        }

        internal static void ShowWindow(object/*!*/ windowGuid) {
            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));
            var outputWindow = dte.Windows.Item(windowGuid);
            Debug.Assert(outputWindow != null, "Unknown window: " + windowGuid);
            outputWindow.Visible = true;
        }

        internal static void GotoSource(this LocationInfo location) {
            IronRubyToolsPackage.NavigateTo(
                location.FilePath,
                Guid.Empty,
                location.Line - 1,
                location.Column - 1);
        }

#if FEATURE_INTELLISENSE
        internal static StandardGlyphGroup ToGlyphGroup(this ObjectType objectType) {
            StandardGlyphGroup group;
            switch (objectType) {
                case ObjectType.Class: group = StandardGlyphGroup.GlyphGroupClass; break;
                case ObjectType.Delegate: group = StandardGlyphGroup.GlyphGroupDelegate; break;
                case ObjectType.Enum: group = StandardGlyphGroup.GlyphGroupEnum; break;
                case ObjectType.Namespace: group = StandardGlyphGroup.GlyphGroupNamespace; break;
                case ObjectType.Multiple: group = StandardGlyphGroup.GlyphGroupOverload; break;
                case ObjectType.Field: group = StandardGlyphGroup.GlyphGroupField; break;
                case ObjectType.Module: group = StandardGlyphGroup.GlyphGroupModule; break;
                case ObjectType.Property: group = StandardGlyphGroup.GlyphGroupProperty; break;
                case ObjectType.Instance: group = StandardGlyphGroup.GlyphGroupVariable; break;
                case ObjectType.Constant: group = StandardGlyphGroup.GlyphGroupConstant; break;
                case ObjectType.EnumMember: group = StandardGlyphGroup.GlyphGroupEnumMember; break;
                case ObjectType.Event: group = StandardGlyphGroup.GlyphGroupEvent; break;
                case ObjectType.Function:
                case ObjectType.Method:
                default:
                    group = StandardGlyphGroup.GlyphGroupMethod;
                    break;
            }
            return group;
        }
#endif
    }
}
