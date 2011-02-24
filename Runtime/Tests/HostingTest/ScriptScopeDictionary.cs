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
using System;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Runtime.Serialization;

namespace HostingTest {

    /// <summary>
    /// Example containner to be used for misc ScriptScope testing.
    /// </summary>
    [Serializable()]
    public class ScriptScopeDictionary : CustomStringDictionary, ISerializable  {

        public ScriptScopeDictionary(SerializationInfo info, StreamingContext context):base() {
            var e = info.GetEnumerator();
            while(e.MoveNext()){
                base[e.Name as object] = e.Value;
            }
        }

        public ScriptScopeDictionary()
            : base() {
        }

        public override string[] GetExtraKeys() {
            return new string[] { };
        }


        protected override bool TrySetExtraValue(string key, object value) {
            return false;
        }

        protected override bool TryGetExtraValue(string key, out object value) {
            value = null;
            return false;
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            foreach (var v in base.Keys) {
                info.AddValue(v as string, base[v]);
            }
        }

        #endregion
    }

}
