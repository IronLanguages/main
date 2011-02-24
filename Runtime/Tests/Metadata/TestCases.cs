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
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

namespace Metadata.Tests {
    public class Attr : Attribute {
    }

    public interface Interface1<out A, in B, out C> {
        bool Property1 { get; set; }
        int Method2();
    }

    [Attr]
    public interface Interface2 {
    }

    public class Class1<A, B, C, [Attr]D, E> : Interface1<A, B, C>
        where A : struct
        where B : class, IEnumerable
        where C : IEnumerable<int>
        where D : Class1<A, string, C, D, E>
        where E : Interface2  {

        [Attr]
        const int Const1 = 1234;

        [Attr]
        int Field1 = 1;

        [Attr]
        public int Method1<[Attr]X, Y, [Attr]Z>(X x, [Optional]Y y, params Z[] z) where X : IEnumerable<Y> {
            return Field1;
        }

        int Interface1<A, B, C>.Method2() {
            return 1;
        }

        [Attr]
        public bool Property1 {
            [Attr]
            get { return true; }
            set { }
        }

        [Attr]
        public event Func<int> Evnt {
            add { }
            remove { }
        }
    }
}
