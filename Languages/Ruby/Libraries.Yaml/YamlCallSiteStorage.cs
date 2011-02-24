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
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Collections;
using System.Collections.Generic;

namespace IronRuby.StandardLibrary.Yaml {
    public sealed class YamlCallSiteStorage : RubyCallSiteStorage {
        // TODO: remove
        private readonly RubyMemberInfo _objectToYamlMethod;

        private CallSite<Func<CallSite, object, object>> _TagUri;
        private CallSite<Func<CallSite, object, object>> _ToYamlStyle;
        private CallSite<Func<CallSite, object, object>> _ToYamlProperties;
        private CallSite<Func<CallSite, object, object, object>> _ToYamlNode;
        private CallSite<Func<CallSite, object, object, object>> _ToYaml;

        public YamlCallSiteStorage(RubyContext/*!*/ context)
            : base(context) {
            _objectToYamlMethod = context.ObjectClass.ResolveMethod("to_yaml", VisibilityContext.AllVisible).Info;
        }

        internal RubyMemberInfo ObjectToYamlMethod {
            get { return _objectToYamlMethod; }
        }

        public CallSite<Func<CallSite, object, object>>/*!*/ TagUri {
            get { return RubyUtils.GetCallSite(ref _TagUri, Context, "taguri", RubyCallSignature.WithImplicitSelf(0)); }
        }

        public CallSite<Func<CallSite, object, object>>/*!*/ ToYamlStyle {
            get { return RubyUtils.GetCallSite(ref _ToYamlStyle, Context, "to_yaml_style", RubyCallSignature.WithImplicitSelf(0)); }
        }

        public CallSite<Func<CallSite, object, object>>/*!*/ ToYamlProperties {
            get { return RubyUtils.GetCallSite(ref _ToYamlProperties, Context, "to_yaml_properties", RubyCallSignature.WithImplicitSelf(0)); }
        }

        public CallSite<Func<CallSite, object, object, object>>/*!*/ ToYamlNode {
            get { return RubyUtils.GetCallSite(ref _ToYamlNode, Context, "to_yaml_node", RubyCallSignature.WithImplicitSelf(1)); }
        }

        public CallSite<Func<CallSite, object, object, object>>/*!*/ ToYaml {
            get { return RubyUtils.GetCallSite(ref _ToYaml, Context, "to_yaml", RubyCallSignature.WithImplicitSelf(1)); }
        }
    }
}
