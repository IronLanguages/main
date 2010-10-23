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

using System.ComponentModel.Composition;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core;
using Microsoft.IronStudio.Library;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronRubyTools {
    /// <summary>
    /// Ruby classifier provider - we just subclass the DLR classifier provider and
    /// give it our engine and content type.
    /// </summary>
    [Export(typeof(IClassifierProvider)), ContentType(RubyCoreConstants.ContentType)]
    public class RubyClassifierProvider : DlrClassifierProvider {
        private readonly IContentType _type;
        private readonly ScriptEngine _engine;
        public static RubyClassifierProvider Instance;

        [ImportingConstructor]
        public RubyClassifierProvider(IRubyRuntimeHost host)
            : this(host.ContentType, host.RubyScriptEngine) {
            Instance = this;
        }

        public RubyClassifierProvider(IContentType contentType, ScriptEngine scriptEngine) {
            _type = contentType;
            _engine = scriptEngine;
        }

        #region IDlrClassifierProvider

        public override IContentType ContentType {
            get {
                return _type;
            }
        }

        public override ScriptEngine Engine {
            get {
                return _engine;
            }
        }

        #endregion
    }
}
