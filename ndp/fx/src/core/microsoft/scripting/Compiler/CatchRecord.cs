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

using System.Reflection.Emit;

namespace System.Linq.Expressions.Compiler {
    internal struct CatchRecord {
        private LocalBuilder _local;
        private CatchBlock _block;

        internal CatchRecord(LocalBuilder local, CatchBlock block) {
            _local = local;
            _block = block;
        }

        internal LocalBuilder Local {
            get { return _local; }
        }

        internal CatchBlock Block {
            get { return _block; }
        }
    }
}
