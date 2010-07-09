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
using System.Text;

namespace Microsoft.Scripting.Actions.Calls {
    public enum Candidate {
        Equivalent = 0,
        One = +1,
        Two = -1,
        Ambiguous = 2
    }

    internal static class CandidateExtension {
        public static bool Chosen(this Candidate candidate) {
            return candidate == Candidate.One || candidate == Candidate.Two;
        }

        public static Candidate TheOther(this Candidate candidate) {
            if (candidate == Candidate.One) {
                return Candidate.Two;
            }
            if (candidate == Candidate.Two) {
                return Candidate.One;
            }
            return candidate;
        }
    }
}
