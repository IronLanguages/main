/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

namespace Microsoft.Scripting {

    /// <summary>
    /// Class that represents compiler options.
    /// Note that this class is likely to change when hosting API becomes part of .Net
    /// </summary>
    [Serializable]
    public class CompilerOptions
#if !SILVERLIGHT
 : ICloneable
#endif
 {
        public CompilerOptions() {
        }

        public virtual object Clone() {
            return base.MemberwiseClone();
        }
    }
}
