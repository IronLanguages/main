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
using System.Linq;
using System.Text;

namespace Microsoft.PyAnalysis {
    /// <summary>
    /// Simple structure used to track a position in code w/ line and column info.
    /// </summary>
    struct SimpleSrcLocation : IEquatable<SimpleSrcLocation> {
        public readonly int Line, Column, Length;

        public SimpleSrcLocation(int line, int column, int length) {
            Line = line;
            Column = column;
            Length = length;
        }

        public SimpleSrcLocation(Scripting.SourceSpan sourceSpan) {
            Line = sourceSpan.Start.Line;
            Column = sourceSpan.Start.Column;
            Length = sourceSpan.Length;
        }

        public override int GetHashCode() {
            return Line ^ Column ^ Length;
        }

        public override bool Equals(object obj) {
            if (obj is SimpleSrcLocation) {
                return Equals((SimpleSrcLocation)obj);
            }
            return false;
        }

        #region IEquatable<SimpleSrcLocation> Members

        public bool Equals(SimpleSrcLocation other) {
            return Line == other.Line && Column == other.Column && Length == other.Length;
        }

        #endregion
    }
}
