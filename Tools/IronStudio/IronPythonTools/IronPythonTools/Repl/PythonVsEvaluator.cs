/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Microsoft.IronPythonTools.Library.Repl;
using Microsoft.IronPythonTools.Options;
using Microsoft.Scripting.Hosting;
using Microsoft.IronStudio.Core.Repl;

namespace Microsoft.IronPythonTools.Repl {
    internal class PythonVsEvaluator : PythonEvaluator {
        // Constructed via reflection when deserialized from the registry.
        public PythonVsEvaluator() {
        }

        public override void PublishScopeVariables(ScriptScope scope) {
            scope.SetVariable("DTE", IronPythonToolsPackage.Instance.DTE);
        }

        protected override bool ShouldEvaluateForCompletion(string source) {
            switch (IronPythonToolsPackage.Instance.OptionsPage.ReplIntellisenseMode) {
                case ReplIntellisenseMode.AlwaysEvaluate: return true;
                case ReplIntellisenseMode.DontEvaluateCalls: return base.ShouldEvaluateForCompletion(source);
                case ReplIntellisenseMode.NeverEvaluate: return false;
                default: throw new InvalidOperationException();
            }
        }
    }
}
