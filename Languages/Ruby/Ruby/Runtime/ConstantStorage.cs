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
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;

namespace IronRuby.Runtime {
    internal struct ConstantStorage {
        internal static ConstantStorage Removed = new ConstantStorage(false);

        public readonly object Value;
        public readonly WeakReference WeakValue;

        internal bool IsRemoved {
            get { return Value == Removed.Value; }
        }

        internal ConstantStorage(object value) {
            Value = value;

            if (value == null) {
                // We use null as a missing constant sentinel and the real null is wrapped in a singleton WeakRef:
                WeakValue = ConstantSiteCache.WeakNull;
            } else {
                switch (Type.GetTypeCode(value.GetType())) {
                    case TypeCode.Object:
                        RubyModule module = value as RubyModule;
                        if (module != null) {
                            WeakValue = module.WeakSelf;
                        } else {
                            WeakValue = new WeakReference(value);
                        }
                        break;

                    case TypeCode.String:
                        WeakValue = new WeakReference(value);
                        break;

                    default:
                        WeakValue = null;
                        break;
                }
            }
        }

        internal ConstantStorage(object value, WeakReference weakValue) {
            Assert.NotNull(value, weakValue);
            Value = value;
            WeakValue = weakValue;
        }

        private ConstantStorage(bool dummy) {
            Value = new object();
            WeakValue = null;
        }
    }
}
