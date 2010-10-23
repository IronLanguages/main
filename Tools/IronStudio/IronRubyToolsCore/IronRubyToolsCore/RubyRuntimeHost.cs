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
using IronRuby;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core;
using Microsoft.Scripting.Hosting;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronRubyTools {
    [Export(typeof(IRubyRuntimeHost))]
    internal sealed class RubyRuntimeHost : IRubyRuntimeHost {
        private readonly IContentType/*!*/ _contentType;
        private readonly ScriptEngine/*!*/ _engine;
        private bool _enterOutliningOnOpen;

        [ImportingConstructor]
        internal RubyRuntimeHost(IContentTypeRegistryService/*!*/ contentTypeRegistryService, IFileExtensionRegistryService/*!*/ fileExtensionRegistryService) {
            _engine = Ruby.CreateEngine((setup) => { setup.Options["NoAssemblyResolveHook"] = true; });
            _contentType = contentTypeRegistryService.GetContentType(RubyCoreConstants.ContentType);
            CoreUtils.RegisterExtensions(contentTypeRegistryService, fileExtensionRegistryService, _contentType, _engine.Setup.FileExtensions);   
        }

        public ScriptEngine RubyScriptEngine {
            get {
                return _engine;
            }
        }

        public IContentType ContentType {
            get { 
                return _contentType; 
            }
        }

        public bool EnterOutliningModeOnOpen {
            get {
                return _enterOutliningOnOpen;
            }
            set {
                _enterOutliningOnOpen = value;
            }
        }
    }
}
