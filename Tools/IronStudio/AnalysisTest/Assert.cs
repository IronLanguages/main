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

namespace AnalysisTest {
    class Assert {
        internal static void IsTrue(bool p) {
            if (!p) {
                Fail("not true");
            }
        }

        internal static void AreEqual(object p, object p_2) {
            if (p == null & p_2 == null) {
                return;
            }
            if (p == null ||
                !p.Equals(p_2)) {
                Fail(String.Format("{0} != {1}", p, p_2));
            }
        }

        internal static void Fail(string p) {
            throw new Exception(p);
        }
    }

}
