/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using Microsoft.Scripting.Hosting;


// Simple wrapper for a DLR object.  Currently, it's only used for making calls, but could be extended
// to other usage (e.g. getting atttributes)
namespace Microsoft.Scripting.AspNet.Util {
    public class DynamicFunction {
        private object _object;

        public DynamicFunction(object o) {
            _object = o;
        }

        public object Invoke(ScriptEngine engine, params object[] args) {
            return engine.Operations.Invoke(_object, args);
        }
    }
}
