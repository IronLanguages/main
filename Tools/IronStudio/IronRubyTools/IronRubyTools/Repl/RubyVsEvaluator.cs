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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IronRubyTools.Library.Repl;
using Microsoft.Scripting.Hosting;
using Microsoft.IronStudio.Core.Repl;

namespace Microsoft.IronRubyTools.Repl {
    internal sealed class RubyVsEvaluator : RubyEvaluator {
        public RubyVsEvaluator() {
        }

        public override void PublishScopeVariables(ScriptScope scope) {
            scope.SetVariable("DTE", IronRubyToolsPackage.Instance.DTE);
        }

        protected override bool ShouldEvaluateForCompletion(string source) {
            switch (IronRubyToolsPackage.Instance.OptionsPage.ReplIntellisenseMode) {
                case ReplIntellisenseMode.AlwaysEvaluate: return true;
                case ReplIntellisenseMode.DontEvaluateCalls: return base.ShouldEvaluateForCompletion(source);
                case ReplIntellisenseMode.NeverEvaluate: return false;
                default: throw new InvalidOperationException();
            }
        }
    }
}
