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
using System.Collections.Generic;
using Microsoft.IronPythonTools.Intellisense;
using Microsoft.IronPythonTools.Language;
using Microsoft.IronPythonTools.Library.Repl;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core.Repl;
using Microsoft.IronStudio.Repl;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronPythonTools.Repl {
    public sealed class RemotePythonVsEvaluator : RemotePythonEvaluator {
        // Constructed via reflection when deserialized from the registry.
        public RemotePythonVsEvaluator() {
        }

        public override void TextViewCreated(IReplWindow window, VisualStudio.Text.Editor.ITextView view) {
            var adapterFactory = IronPythonToolsPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
            new EditFilter(IronPythonToolsPackage.ComponentModel.GetService<IPythonAnalyzer>(), (IWpfTextView)view, adapterFactory.GetViewAdapter(view));
            window.UseSmartUpDown = IronPythonToolsPackage.Instance.OptionsPage.ReplSmartHistory;
            base.TextViewCreated(window, view);
        }

        public override void Reset() {
            base.Reset(); 
            Initialize();
        }

        public override Dictionary<string, object> GetOptions() {
            Dictionary<string, object> res = new Dictionary<string, object>();
            foreach (string option in IronPythonToolsPackage.Instance.OptionsPage.InteractiveOptions.Split(';')) {
                string[] nameValue = option.Split(new[] { '=' }, 2);
                if (nameValue.Length == 2) {
                    res[nameValue[0]] = nameValue[1];
                }
            }
            return res;
        }

        public void Initialize() {
            string filename, dir;
            if (CommonPackage.TryGetStartupFileAndDirectory(out filename, out dir)) {
                RemoteScriptFactory.SetCurrentDirectory(dir);
            }
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
